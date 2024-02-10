using GaRyan2.Utilities;
using GaRyan2.WmcUtilities;
using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using System.Xml.Linq;

namespace epg123Client
{
    public partial class frmWmcTweak : Form
    {
        #region ========== Native Externals =========

        [DllImport("Kernel32.dll", BestFitMapping = false, CharSet = CharSet.Unicode, SetLastError = true, ThrowOnUnmappableChar = true)]
        public static extern IntPtr BeginUpdateResource(string pFileName, [MarshalAs(UnmanagedType.Bool)] bool bDeleteExistingResources);

        [DllImport("Kernel32.dll", BestFitMapping = false, CharSet = CharSet.Unicode, SetLastError = true, ThrowOnUnmappableChar = true)]
        public static extern int UpdateResource(IntPtr hUpdate, int lpType, StringBuilder lpName, short wLanguage, byte[] lpData, int cbData);

        [DllImport("Kernel32.dll", BestFitMapping = false, CharSet = CharSet.Unicode, SetLastError = true, ThrowOnUnmappableChar = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool EndUpdateResource(IntPtr hUpdate, [MarshalAs(UnmanagedType.Bool)] bool bDiscard);

        [DllImport("Kernel32.dll", BestFitMapping = false, CharSet = CharSet.Unicode, SetLastError = true, ThrowOnUnmappableChar = true)]
        public static extern IntPtr LoadLibrary(string fileName);

        [DllImport("Kernel32.dll", BestFitMapping = false, CharSet = CharSet.Unicode, SetLastError = true, ThrowOnUnmappableChar = true)]
        public static extern IntPtr LoadResource(IntPtr hModule, IntPtr hResInfo);

        [DllImport("Kernel32.dll", BestFitMapping = false, CharSet = CharSet.Unicode, SetLastError = true, ThrowOnUnmappableChar = true)]
        public static extern IntPtr LockResource(IntPtr hGlobal);

        [DllImport("Kernel32.dll", BestFitMapping = false, CharSet = CharSet.Unicode, SetLastError = true, ThrowOnUnmappableChar = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool FreeLibrary(IntPtr hModule);

        [DllImport("Kernel32.dll", BestFitMapping = false, CharSet = CharSet.Unicode, EntryPoint = "SizeofResource", SetLastError = true, ThrowOnUnmappableChar = true)]
        public static extern int SizeOfResource(IntPtr hModule, IntPtr hResInfo);

        [DllImport("kernel32.dll")]
        private static extern IntPtr FindResource(IntPtr hModule, string lpName, uint lpType);

        [DllImport("kernel32.dll")]
        private static extern uint GetLastError();
        #endregion

        // enumeration of resources in Microsoft.MediaCenter.Shell.dll
        private enum shellresource
        {
            EPG_MCML,
            EPGCELLS_MCML,
            EPGCOMMON_MCML,
            FILTERBUTTON_MCML,
            FILTERLISTBOX_MCML,
            CLOCK_MCML,
            TABLE_MCML,
            MAX
        }

        private readonly XDocument[] _shellDllResources = new XDocument[(int)shellresource.MAX];

        // filepaths
        private readonly string _shellEhomePath = Environment.GetEnvironmentVariable("WINDIR") + @"\ehome\Microsoft.MediaCenter.Shell.dll";
        private readonly string _shellTempPath = Environment.GetEnvironmentVariable("TEMP") + @"\Microsoft.MediaCenter.Shell.dll";

        // calculation constants
        private const double PixelsPerPoint = 1.33;
        private const int MinMainTableTop = 65;
        private const int MinMiniTableTop = 300;

        private const int TunerLimit = 32;

        public frmWmcTweak()
        {
            InitializeComponent();

            SetNamePatternExampleText();

            using (var key = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Media Center\\Start Menu", false))
            {
                try
                {
                    trackBar1.Value = (int)key.GetValue("OEMLogoOpacity", 100);
                    switch ((string)key.GetValue("OEMLogoAccent", "Light"))
                    {
                        case "Light":
                            rdoLight.Checked = true;
                            break;
                        case "Light_ns":
                            rdoLight.Checked = true;
                            cbNoSuccess.Checked = true;
                            break;
                        case "Dark":
                            rdoDark.Checked = true;
                            break;
                        case "Dark_ns":
                            rdoDark.Checked = true;
                            cbNoSuccess.Checked = true;
                            break;
                        default:
                            rdoNone.Checked = true;
                            break;
                    }
                }
                catch
                {
                    // ignored
                }
            }
        }

        private void frmWmcTweak_Load(object sender, EventArgs e)
        {
            // import all the needed resource files
            ImportResources();

            // update controls
            FormInitialized = GetGuideConfigurations();

            // setup widgets
            RecalculateAll();

            // populate current values
            ReadRegistries();
        }

        private void frmWmcTweak_Shown(object sender, EventArgs e)
        {
            Refresh();
        }

        private void SetNamePatternExampleText()
        {
            textBox2.Text = "HomeTown\r\n" +
                            "s03e07 Home is Where the Art Is\r\n" +
                            $"{new DateTime(2019, 10, 06, 11, 0, 0):G}\r\n" +
                            $"{new DateTime(2019, 2, 25, 0, 0, 0):G}\r\n" +
                            "37\r\n" +
                            "HGTVP\r\n" +
                            "Home & Garden Television (Pacific)";
        }

        #region ========== Trackbar Calculations =========

        // setable attributes
        private double RowHeightMultiplier { get; set; }
        private bool FormInitialized { get; set; }
        private bool ShowMainDetails { get; set; }
        private bool ShowMiniDetails { get; set; }
        private int CellFontPointSize { get; set; }
        private int MainGuideRows { get; set; }
        private int MiniGuideRows { get; set; }
        private int ColumnMinutes { get; set; }

        // calculated parameters
        private int CellFontPixelHeight => (int)(CellFontPointSize * PixelsPerPoint);
        private int RowHeightPixel => (int)(CellFontPixelHeight * RowHeightMultiplier + 0.5);
        private int MaxMainTableRows => (int)((MaxTableBottom - MinMainTableTop) / (double)RowHeightPixel);
        private int MainTableTop => (MinMainTableTop + MaxTableBottom - MainGuideRows * RowHeightPixel) / 2;
        private int MainTableBottom => MainTableTop + RowHeightPixel * MainGuideRows;
        private int MaxTableBottom => 768 - (ShowMainDetails ? 25 + 150 : (int)(HalfScaleNumber(16) * PixelsPerPoint) + 7) - 50;
        private int MiniTableBottom => 768 - (ShowMiniDetails ? 25 + 118 : (int)(HalfScaleNumber(16) * PixelsPerPoint + 7)) - 50;
        private int MiniTableTop => MiniTableBottom - (RowHeightPixel + 3) * MiniGuideRows;
        private int MaxMiniTableRows => (int)((MiniTableBottom - MinMiniTableTop) / (double)RowHeightPixel);
        private int SmallLogoHeight => CellFontPixelHeight;
        private int LargeLogoHeight => Math.Min(RowHeightPixel - ScaleNumber(6), 75);
        private int MediumLogoHeight => (int)((SmallLogoHeight + LargeLogoHeight) / 2.0);
        private int ScaleNumber(double fontSize) => (int)(fontSize * Math.Min(CellFontPointSize, 36) / 22 + 0.5);
        private int HalfScaleNumber(double fontSize) => (int)((1 + (Math.Min(CellFontPointSize, 36) - 22) / 44.0) * fontSize);
        private int FilterButtonWidth => (int)(ScaleNumber(16) * PixelsPerPoint * RowHeightMultiplier + 0.5);

        // widget reactions
        private void RecalculateAll()
        {
            if (!FormInitialized) return;

            // cell font size
            CellFontPointSize = trackCellFontSize.Value;
            lblCellFontSize.Text = $"{CellFontPointSize} point";

            // row height
            RowHeightMultiplier = 1.0 + trackRowHeight.Value / 100.0;
            lblRowHeight.Text = $"{RowHeightMultiplier:N2}X Font Height";

            // logo size
            switch (trackLogoSize.Value)
            {
                case 0: // small
                    lblLogoSize.Text = $"Small ({SmallLogoHeight * 3}x{SmallLogoHeight})";
                    break;
                case 1: // medium
                    lblLogoSize.Text = $"Medium ({MediumLogoHeight * 3}x{MediumLogoHeight})";
                    break;
                case 2: // large
                    lblLogoSize.Text = $"Large ({LargeLogoHeight * 3}x{LargeLogoHeight})";
                    break;
            }

            // detail views
            ShowMainDetails = cbMainShowDetails.Checked;
            ShowMiniDetails = cbMiniShowDetails.Checked;

            // main guide rows
            MainGuideRows = Math.Min(trackMainRows.Value, MaxMainTableRows);
            trackMainRows.Maximum = MaxMainTableRows;
            lblMainRows.Text = $"{MainGuideRows} rows";

            // mini guide rows
            MiniGuideRows = Math.Min(trackMiniRows.Value, MaxMiniTableRows);
            trackMiniRows.Maximum = MaxMiniTableRows;
            lblMiniRows.Text = $"{MiniGuideRows} rows";

            // column time
            ColumnMinutes = trackMinutes.Value;
            lblMinutes.Text = $"{ColumnMinutes} minutes";

            // channel cell width
            if (cbAutoAdjustColumnWidth.Checked)
            {
                trackColumnWidth.Value = Math.Max(Math.Min(CalculateColumnWidth(), trackColumnWidth.Maximum), trackColumnWidth.Minimum);
            }
            lblColumnWidth.Text = trackColumnWidth.Value == 0 ? "Default" : $"{trackColumnWidth.Value} pixels";
        }

        private void trackBar_ValueChanged(object sender, EventArgs e)
        {
            TrackBar[] bars = { trackCellFontSize, trackMainRows, trackMiniRows, trackMinutes, trackColumnWidth, trackRowHeight };

            int bar;
            for (bar = 0; bar < bars.Length; ++bar)
            {
                if (sender.Equals(bars[bar]))
                {
                    break;
                }
            }

            if (bar < bars.Length)
            {
                var step = bars[bar].SmallChange;
                var value = bars[bar].Value;

                if (value % step != 0)
                {
                    value = (int)((double)value / step + 0.5) * step;
                }

                bars[bar].Value = value;
            }

            RecalculateAll();
        }

        private void cbMainShowDetails_CheckStateChanged(object sender, EventArgs e)
        {
            RecalculateAll();
        }

        private void cbAutoAdjustColumnWidth_CheckStateChanged(object sender, EventArgs e)
        {
            if (cbAutoAdjustColumnWidth.Checked) trackColumnWidth.Value = CalculateColumnWidth();
        }

        private int CalculateColumnWidth()
        {
            var logoHeight = trackLogoSize.Value == 0 ? SmallLogoHeight : trackLogoSize.Value == 1 ? MediumLogoHeight : LargeLogoHeight;
            return 3 * logoHeight + ScaleNumber(5) + (cbHideNumber.Checked ? 0 : (int)(CellFontPixelHeight * 2.67) + ScaleNumber(5));
        }
        #endregion

        #region ========== Resource Read/Writes ==========
        private bool GetGuideConfigurations()
        {
            // get cell font size
            var font1 = _shellDllResources[(int)shellresource.EPGCELLS_MCML]
                .Descendants()
                .Where(arg => arg.Name.LocalName == "Font")
                .Where(arg => arg.Attribute("Name") != null)
                .Single(arg => arg.Attribute("Name").Value == "TitleDefaultFont");
            if (font1 != null)
            {
                trackCellFontSize.Value = CellFontPointSize = SafeTrackBarValue(int.Parse(font1.Attribute("FontSize").Value), trackCellFontSize);
            }

            // get main guide rows and mini guide rows
            var actions = _shellDllResources[(int)shellresource.EPG_MCML].Descendants()
                .Where(arg => arg.Name.LocalName == "Set")
                .Where(arg => arg.Attribute("Target") != null)
                .Where(arg => arg.Attribute("Target").Value == "[Table.VisibleRowCapacity]")
                .Select(arg => arg.Parent);
            foreach (var action in actions)
            {
                var mode = action.Parent
                    .Descendants()
                    .Where(arg => arg.Name.LocalName == "Equality")
                    .Where(arg => arg.Attribute("Source") != null)
                    .Single(arg => arg.Attribute("Source").Value == "[MiniMode.Value]");
                if (mode == null) continue;
                if (mode.Attribute("Value").Value == "false") // main guide
                {
                    var top = 0;
                    var bottom = 0;
                    foreach (var target in action.Descendants())
                    {
                        if (target.Attribute("Target") == null) continue;
                        switch (target.Attribute("Target").Value)
                        {
                            case "[Table.VisibleRowCapacity]":
                                MainGuideRows = int.Parse(target.Attribute("Value").Value);
                                break;
                            case "[DetailsLayout.Top.Offset]":
                                cbMainShowDetails.Checked = (int.Parse(target.Attribute("Value").Value) < 768);
                                break;
                            case "[FilterButtonLayout.Top.Offset]":
                                top = int.Parse(target.Attribute("Value").Value);
                                break;
                            case "[FilterButtonLayout.Bottom.Offset]":
                                bottom = int.Parse(target.Attribute("Value").Value);
                                break;
                        }
                    }

                    RowHeightMultiplier = (bottom - top) / (double)MainGuideRows / CellFontPixelHeight + 0.005;
                    trackMainRows.Maximum = MaxMainTableRows;
                    trackMainRows.Value = SafeTrackBarValue(MainGuideRows, trackMainRows);
                    trackRowHeight.Value = SafeTrackBarValue((int)(100 * RowHeightMultiplier - 100), trackRowHeight);
                }
                else // mini guide
                {
                    foreach (var target in action.Descendants())
                    {
                        if (target.Attribute("Target") == null) continue;
                        switch (target.Attribute("Target").Value)
                        {
                            case "[Table.VisibleRowCapacity]":
                                MiniGuideRows = int.Parse(target.Attribute("Value").Value);
                                trackMiniRows.Value = SafeTrackBarValue(MiniGuideRows, trackMiniRows);
                                break;
                            case "[DetailsLayout.Top.Offset]":
                                cbMiniShowDetails.Checked = (int.Parse(target.Attribute("Value").Value) < 768);
                                break;
                        }
                    }
                }
            }

            // get guide visible columns
            var columns = _shellDllResources[(int)shellresource.EPG_MCML]
                .Descendants()
                .Where(arg => arg.Name.LocalName == "Condition")
                .Where(arg => arg.Attribute("Target") != null)
                .Single(arg => arg.Attribute("Target").Value == "[Table.VisibleColumnCapacity]");
            if (columns != null)
            {
                trackMinutes.Value = ColumnMinutes = SafeTrackBarValue(int.Parse(columns.Attribute("Value").Value), trackMinutes);
            }

            // determine channel logo size
            var channelLogo = _shellDllResources[(int)shellresource.EPGCOMMON_MCML]
                .Descendants()
                .Where(arg => arg.Name.LocalName == "UI")
                .Where(arg => arg.Attribute("Name") != null)
                .Single(arg => arg.Attribute("Name").Value == "ChannelLogo");

            var size = (channelLogo?.Descendants()
                    .Where(arg => arg.Name.LocalName == "Size")
                    .Where(arg => arg.Attribute("Name") != null))
                    .Single(arg => arg.Attribute("Name").Value == "MaximumSize");
            if (size != null)
            {
                var value = size.Attribute("Size").Value.Split(',');
                if (Math.Abs(LargeLogoHeight - int.Parse(value[1])) <= 1) trackLogoSize.Value = 2;
                else if (Math.Abs(MediumLogoHeight - int.Parse(value[1])) <= 1) trackLogoSize.Value = 1;
                else trackLogoSize.Value = 0;
            }

            // determine if channel number is hidden and if animations are disabled
            var epgChannelCell = _shellDllResources[(int)shellresource.EPGCELLS_MCML]
                .Descendants()
                .Where(arg => arg.Name.LocalName == "UI")
                .Where(arg => arg.Attribute("Name") != null)
                .Single(arg => arg.Attribute("Name").Value == "EpgChannelCell");
            if (epgChannelCell != null)
            {
                var channelNumber = epgChannelCell
                    .Descendants()
                    .Where(arg => arg.Name.LocalName == "Text")
                    .Where(arg => arg.Attribute("Name") != null)
                    .Single(arg => arg.Attribute("Name").Value == "Number");
                if (channelNumber != null)
                {
                    cbHideNumber.Checked = (channelNumber.Attribute("MaximumSize").Value == "1,0");
                }

                var expandedFont = "22";
                var defaultFont = "18";
                var fonts = epgChannelCell.Descendants()
                    .Where(arg => arg.Name.LocalName == "Font")
                    .Where(arg => arg.Attribute("Name") != null);
                foreach (var font in fonts)
                {
                    switch (font.Attribute("Name").Value)
                    {
                        case "NumberExpandedFont":
                            expandedFont = font.Attribute("FontSize").Value;
                            break;
                        case "NumberDefaultFont":
                            defaultFont = font.Attribute("FontSize").Value;
                            break;
                    }
                }
                cbRemoveAnimations.Checked = (expandedFont == defaultFont);

                // determine if logos are centered
                var panels = epgChannelCell
                    .Descendants()
                    .Where(arg => arg.Name.LocalName == "Panel")
                    .Where(arg => arg.Attribute("Name") != null)
                    .Single(arg => arg.Attribute("Name").Value == "SmallLogoPanel");
                if (panels != null)
                {
                    var e = panels.Descendants().Single(arg => arg.Name.LocalName == "FormLayoutInput");
                    cbCenterLogo.Checked = (e.Attribute("Horizontal") != null);
                }
            }

            // determine if callsign is overridden with channel name
            var callsigns = _shellDllResources[(int)shellresource.EPGCELLS_MCML].Descendants()
                .Where(arg => arg.Name.LocalName == "Default")
                .Where(arg => arg.Attribute("Target") != null)
                .Where(arg => arg.Attribute("Target").Value == "[Callsign.Content]");
            foreach (var callsign in callsigns)
            {
                cbChannelName.Checked = (callsign.Attribute("Value").Value == "[Cell.Name]");
                break;
            }

            // determine if expanded epg is active
            var rule1 = _shellDllResources[(int)shellresource.EPGCELLS_MCML]
                .Descendants()
                .Where(arg => arg.Name.LocalName == "Equality")
                .Where(arg => arg.Attribute("Source").Value == "[Cell.Episode]")
                .Where(arg => arg.Attribute("Value") != null)
                .SingleOrDefault(arg => arg.Attribute("Value").Value == "");
            cbExpandedEpg.Checked = rule1 != null;

            // determine if expanded movie is active
            var rule2 = _shellDllResources[(int)shellresource.EPGCELLS_MCML]
                .Descendants()
                .Where(arg => arg.Name.LocalName == "Equality")
                .Where(arg => arg.Attribute("Source").Value == "[Cell.MovieYear]")
                .Where(arg => arg.Attribute("Value") != null)
                .SingleOrDefault(arg => arg.Attribute("Value").Value == "");
            cbExpandedMovie.Checked = rule2 != null;

            // determine if expanded clock is active
            var clock = _shellDllResources[(int)shellresource.CLOCK_MCML].Descendants()
                .Single(arg => arg.Name.LocalName == "DateTimeFormats");
            cbClock.Checked = clock.Attribute("DateTimeFormats").Value.Contains("AbbreviatedLongDate");

            // get channel column width
            using (var key = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Media Center\\Settings\\ProgramGuide", false))
            {
                if (key == null) return true;
                var width = 0;
                if (key.GetValue("ChannelCellWidth") != null) width = (int)key.GetValue("ChannelCellWidth");
                trackColumnWidth.Value = SafeTrackBarValue(width == 0 ? 240 : width, trackColumnWidth);

                cbAutoAdjustColumnWidth.Checked = (width == CalculateColumnWidth());
            }
            return true;
        }

        private void SetEpgChannelCellProperties()
        {
            var logoHeight = trackLogoSize.Value == 0 ? SmallLogoHeight : trackLogoSize.Value == 1 ? MediumLogoHeight : LargeLogoHeight;
            var uiElements = _shellDllResources[(int)shellresource.EPGCELLS_MCML].Descendants()
                .Where(arg => arg.Name.LocalName == "UI")
                .Single(arg => arg.Attribute("Name") != null && arg.Attribute("Name").Value == "EpgChannelCell");

            // edit properties
            var properties = uiElements.Descendants()
                .Where(arg => arg.Name.LocalName == "Properties").Elements();
            foreach (var e in properties)
            {
                if (e.Attribute("Name") == null) continue;

                int iValue;
                switch (e.Attribute("Name")?.Value)
                {
                    case "NumberDefaultFont":
                        iValue = cbRemoveAnimations.Checked ? CellFontPointSize : ScaleNumber(18);
                        e.SetAttributeValue("FontSize", $"{iValue}"); // default is 18
                        break;
                    case "NumberExpandedFont":
                        e.SetAttributeValue("FontSize", $"{CellFontPointSize}"); // default is 22
                        break;
                    case "CallsignDefaultFont":
                        e.SetAttributeValue("FontSize", $"{ScaleNumber(18)}"); // default is 18
                        break;
                    case "CallsignExpandedFont":
                        e.SetAttributeValue("FontSize", $"{ScaleNumber(18)}"); // default is 18
                        break;
                    case "BackgroundDefaultPadding":
                        e.SetAttributeValue("Inset", $"{ScaleNumber(5)},0,{ScaleNumber(5)},0"); // default is "10,12,10,8"
                        break;
                    case "BackgroundExpandedPadding":
                        e.SetAttributeValue("Inset", $"{ScaleNumber(5)},0,{ScaleNumber(5)},0"); // default is "10,6,10,6"
                        break;
                    case "DefaultNumberMargins":
                        e.SetAttributeValue("Inset", "0,0,0,0"); // default is "0,0,0,0"
                        break;
                    case "FocusNumberMargins":
                        e.SetAttributeValue("Inset", "0,0,0,0"); // default is "-3,2,0,0"
                        break;
                    case "DefaultCallsignMargins":
                        e.SetAttributeValue("Inset", $"{ScaleNumber(5)},0,0,0"); // default is "0,0,0,0"
                        break;
                    case "FocusCallsignMargins":
                        e.SetAttributeValue("Inset", $"{ScaleNumber(5)},0,0,0"); // default is "0,6,0,0"
                        break;
                }
            }

            // set channel number size/location attributes
            var text = uiElements.Descendants()
                .Where(arg => arg.Name.LocalName == "Text")
                .Where(arg => arg.Attribute("Name") != null)
                .Single(arg => arg.Attribute("Name")?.Value == "Number");
            if (text != null)
            {
                var value = cbHideNumber.Checked ? "1,0" : $"{CellFontPixelHeight * 3},0";
                text.SetAttributeValue("MaximumSize", value); // default is "75,0"

                var layout = text.Descendants().Single(arg => arg.Name.LocalName == "FormLayoutInput");
                layout?.SetAttributeValue("Vertical", "Center"); // default is null
            }

            // edit channel logo and number panels
            var panels = uiElements.Descendants()
                .Where(arg => arg.Name.LocalName == "Panel" && arg.Attribute("Name") != null);
            foreach (var panel in panels)
            {
                XElement e;
                switch (panel.Attribute("Name")?.Value)
                {
                    case "SmallLogoPanel":
                        e = panel.Descendants().Single(arg => arg.Name.LocalName == "FormLayoutInput");
                        e.SetAttributeValue("Bottom", "Parent,1"); // default is "Number,1"
                        e.SetAttributeValue("Vertical", "Center"); // default is null
                        e.SetAttributeValue("Horizontal", cbCenterLogo.Checked ? "Center" : null); // default is null
                        e.SetAttributeValue("Left", cbCenterLogo.Checked && !cbHideNumber.Checked ? "Parent,1," + (3 * CellFontPixelHeight - trackColumnWidth.Value + 10) : null);

                        var height = cbRemoveAnimations.Checked ? logoHeight : (int)(logoHeight * 0.82);
                        e = panel.Descendants().Single(arg => arg.Name.LocalName == "ChannelLogo");
                        e.SetAttributeValue("MaximumSize", $"{3 * height},{height}"); // default is "70,40"
                        break;
                    case "CallsignPanel":
                        e = panel.Descendants().Single(arg => arg.Name.LocalName == "FormLayoutInput");
                        e.SetAttributeValue("Bottom", "Parent,1"); // default is "Number,1"
                        e.SetAttributeValue("Vertical", "Center"); // default is null
                        break;
                }
            }

            // set logo maximum size
            var logoElement = _shellDllResources[(int)shellresource.EPGCOMMON_MCML].Descendants()
                .Where(arg => arg.Name.LocalName == "UI" && arg.Attribute("Name").Value == "ChannelLogo");
            logoElement.Descendants().Single(arg => arg.Name.LocalName == "Size")
                .SetAttributeValue("Size", $"{3 * logoHeight},{logoHeight}"); // default is "75,35"

            // replace callsign with channel name
            if (cbChannelName.Checked)
            {
                var callsigns = _shellDllResources[(int)shellresource.EPGCELLS_MCML].Descendants()
                    .Where(arg => arg.Name.LocalName == "Default")
                    .Where(arg => arg.Attribute("Target") != null)
                    .Where(arg => arg.Attribute("Target").Value == "[Callsign.Content]");
                foreach (var callsign in callsigns)
                {
                    callsign.SetAttributeValue("Value", "[Cell.Name]"); // default is [Cell.Callsign]
                }
            }

            // set channel column width
            using (var key = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Media Center\\Settings\\ProgramGuide", true))
            {
                key?.SetValue("ChannelCellWidth", trackColumnWidth.Value, RegistryValueKind.DWord); // default is 0
            }
        }

        private void SetGuidePageDateTimeFilterFonts()
        {
            // set date font above channel cells
            var date = _shellDllResources[(int)shellresource.EPGCOMMON_MCML]
                .Descendants()
                .Where(arg => arg.Name.LocalName == "Font")
                .Single(arg => arg.Attribute("Name") != null && arg.Attribute("Name")?.Value == "DateFont");
            date.SetAttributeValue("FontSize", $"{ScaleNumber(16)}"); // default is 16

            var date2 = _shellDllResources[(int)shellresource.EPGCOMMON_MCML].Descendants()
                .Where(arg => arg.Name.LocalName == "Text")
                .Single(arg => arg.Attribute("Name") != null && arg.Attribute("Name")?.Value == "Date");
            date2.SetAttributeValue("Margins", $"{ScaleNumber(14)},0,{ScaleNumber(5)},0"); // default is "14,0,5,0"

            // set time font above guide cells
            var time = _shellDllResources[(int)shellresource.EPGCELLS_MCML].Descendants()
                .Where(arg => arg.Name.LocalName == "Font")
                .Single(arg => arg.Attribute("Name") != null && arg.Attribute("Name")?.Value == "TimeFont");
            time.SetAttributeValue("FontSize", $"{ScaleNumber(16)}"); // default is 16

            var time2 = _shellDllResources[(int)shellresource.EPGCELLS_MCML].Descendants()
                .Where(arg => arg.Name.LocalName == "UI")
                .Where(arg => arg.Attribute("Name") != null && arg.Attribute("Name")?.Value == "EpgTimeCell")
                .Descendants()
                .Single(arg => arg.Attribute("Name") != null && arg.Attribute("Name")?.Value == "Label");
            time2.SetAttributeValue("Margins", $"{ScaleNumber(14)},0,{ScaleNumber(5)},0"); // default is "14,0,5,0"

            // set title font for guide page
            var maxFont = Math.Min((int)((0.7 * MainTableTop - 18 + (16 - ScaleNumber(16)) * 1.33) / 1.33), 48);
            var title = _shellDllResources[(int)shellresource.EPG_MCML].Descendants()
                .Where(arg => arg.Name.LocalName == "Font")
                .Where(arg => arg.Parent != null && arg.Parent.Name.LocalName == "Font")
                .Where(arg => arg.Parent.Parent != null && arg.Parent.Parent.Name.LocalName == "StaticText")
                .Single(arg => arg.Parent.Parent.Attribute("Name")?.Value == "Title");
            title.SetAttributeValue("FontSize", $"{maxFont}"); // default is 48

            // set filter button text font and margins
            var button = _shellDllResources[(int)shellresource.FILTERBUTTON_MCML].Descendants()
                .Where(arg => arg.Name.LocalName == "Font")
                .Single(arg => arg.Attribute("Name") != null && arg.Attribute("Name")?.Value == "Font");
            button.SetAttributeValue("FontSize", $"{ScaleNumber(16)}"); // default is 16

            var buttonText = _shellDllResources[(int)shellresource.FILTERBUTTON_MCML].Descendants()
                .Where(arg => arg.Name.LocalName == "Panel")
                .Single(arg => arg.Attribute("Name") != null && arg.Attribute("Name")?.Value == "LabelPanel");
            buttonText.SetAttributeValue("Margins", "0,0,0,0"); // default is "0,8,0,0"

            // set chevron margins
            var chevron = _shellDllResources[(int)shellresource.FILTERBUTTON_MCML].Descendants()
                .Where(arg => arg.Name.LocalName == "Graphic")
                .Single(arg => arg.Attribute("Name") != null && arg.Attribute("Name")?.Value == "Chevron");
            chevron.SetAttributeValue("Margins", $"14,0,{ScaleNumber(36)},0"); // default is "14,0,36,0"
            chevron.SetAttributeValue("MaintainAspectRatio", "true"); // default is false (not present)

            // set filter list text font
            var list = _shellDllResources[(int)shellresource.FILTERLISTBOX_MCML].Descendants()
                .Where(arg => arg.Name.LocalName == "UI")
                .Where(arg => arg.Attribute("Name") != null && arg.Attribute("Name")?.Value == "FilteredGuide").Descendants()
                .Where(arg => arg.Name.LocalName == "Properties").Elements();
            foreach (var e in list)
            {
                switch (e.Attribute("Name")?.Value)
                {
                    case "FocusLabelMargins":
                        e.SetAttributeValue("Inset", $"{ScaleNumber(12)},0,{ScaleNumber(8)},0"); // default is "12,6,8,0"
                        break;
                    case "TileSize":
                        e.SetAttributeValue("Size", $"{ScaleNumber(270)},{RowHeightPixel - 3}"); // default is "270,51"
                        break;
                    case "LabelMargins":
                        e.SetAttributeValue("Inset", $"{ScaleNumber(36)},0,{ScaleNumber(8)},0"); // default is "36,6,8,0"
                        break;
                    case "Font":
                        e.SetAttributeValue("FontSize", $"{ScaleNumber(22)}"); // default is 22
                        break;
                    case "FocusFont":
                        e.SetAttributeValue("FontSize", $"{ScaleNumber(22)}"); // default is 22
                        break;
                    //case "FocusImage":
                    //    e.SetAttributeValue("BaseImage", "null");
                    //    break;
                    case "IconMinSize":
                        e.SetAttributeValue("Size", $"{ScaleNumber(39)},{ScaleNumber(32)}");
                        break;
                    case "IconPadding":
                        e.SetAttributeValue("Inset", $"0,0,{ScaleNumber(20)},0");
                        break;
                }
            }

            // set filter menu width
            var menu = _shellDllResources[(int)shellresource.EPG_MCML].Descendants()
                .Where(arg => arg.Name.LocalName == "GuideFilterListBox").Descendants()
                .Single(arg => arg.Name.LocalName == "FormLayoutInput");
            menu.SetAttributeValue("Right", $"FilterButton,1,{ScaleNumber(274)}"); // default is "FilterButton,1,274"

            var panel = _shellDllResources[(int)shellresource.EPG_MCML].Descendants()
                .Where(arg => arg.Name.LocalName == "Panel")
                .Where(arg => arg.Attribute("Name") != null && arg.Attribute("Name").Value == "ContentPanel").Descendants()
                .First(arg => arg.Name.LocalName == "FormLayoutInput");
            panel.SetAttributeValue("Right", $"FilterButton,1,{ScaleNumber(238)}");

            // set date/time format
            if (cbClock.Checked)
            {
                var clock = _shellDllResources[(int)shellresource.CLOCK_MCML].Descendants()
                    .Single(arg => arg.Name.LocalName == "DateTimeFormats");
                clock.SetAttributeValue("DateTimeFormats", "AbbreviatedLongDate,ShortTime"); // default is "ShortTime"
            }
        }

        private void SetTableAndDetailsViewProperties()
        {
            var epgPage = _shellDllResources[(int)shellresource.EPG_MCML].Descendants()
                .Where(arg => arg.Name.LocalName == "UI")
                .Single(arg => arg.Attribute("Name") != null && arg.Attribute("Name")?.Value == "EpgPage");

            // set main detail image size
            var image1 = epgPage.Descendants()
                .Where(arg => arg.Name.LocalName == "Size")
                .Single(arg => arg.Attribute("Name") != null && arg.Attribute("Name")?.Value == "GuideShowCardImageSize");
            image1?.SetAttributeValue("Size", "267,150"); // default is 210,116

            // set detail vertical offsets, visible row counts, filter offsets, and image offsets
            var actions = epgPage.Descendants()
                .Where(arg => arg.Name.LocalName == "Set")
                .Where(arg => arg.Attribute("Target") != null && arg.Attribute("Target")?.Value == "[Table.VisibleRowCapacity]")
                .Select(arg => arg.Parent);
            foreach (var action in actions)
            {
                var mode = action?.Parent?.Descendants()
                    .Where(arg => arg.Name.LocalName == "Equality")
                    .Single(arg => arg.Attribute("Source") != null && arg.Attribute("Source")?.Value == "[MiniMode.Value]");
                if (mode.Attribute("Value")?.Value == "false") // main guide
                {
                    foreach (var target in action.Descendants())
                    {
                        int iValue;
                        switch (target.Attribute("Target")?.Value)
                        {
                            case "[Table.VisibleRowCapacity]":
                                target.SetAttributeValue("Value", $"{MainGuideRows}"); // default is 7
                                break;
                            case "[FilterButtonLayout.Top.Offset]":
                                target.SetAttributeValue("Value", $"{MainTableTop}"); // default is 118
                                break;
                            case "[FilterButtonLayout.Left.Offset]":
                                target.SetAttributeValue("Value", "0"); // default is 55
                                break;
                            case "[FilterButtonLayout.Right.Offset]":
                                target.SetAttributeValue("Value", $"{FilterButtonWidth}"); // default is 106
                                break;
                            case "[FilterButtonLayout.Bottom.Offset]":
                                target.SetAttributeValue("Value", $"{MainTableBottom}"); // default is 493
                                break;
                            case "[DetailsLayout.Top.Offset]":
                                iValue = cbMainShowDetails.Checked ? 27 : 1000;
                                target.SetAttributeValue("Value", $"{iValue}"); // default is 27
                                break;
                            case "[DetailsLayout.Bottom.Offset]":
                                break;
                            case "[ContentImageLayout.Top.Offset]":
                                iValue = cbMainShowDetails.Checked ? 34 : 1000;
                                target.SetAttributeValue("Value", $"{iValue}"); // default is 34
                                break;

                        }
                    }
                }
                else // mini-guide
                {
                    foreach (var target in action.Descendants())
                    {
                        int iValue;
                        switch (target.Attribute("Target")?.Value)
                        {
                            case "[Table.VisibleRowCapacity]":
                                target.SetAttributeValue("Value", $"{MiniGuideRows}"); // default is 2
                                break;
                            case "[FilterButtonLayout.Top.Offset]":
                                target.SetAttributeValue("Value", $"{MiniTableTop}"); // default is 476
                                break;
                            case "[FilterButtonLayout.Left.Offset]":
                                target.SetAttributeValue("Value", "0"); // default is 54
                                break;
                            case "[FilterButtonLayout.Right.Offset]":
                                target.SetAttributeValue("Value", $"{FilterButtonWidth}"); // default is 55
                                break;
                            case "[FilterButtonLayout.Bottom.Offset]":
                                target.SetAttributeValue("Value", $"{MiniTableBottom}"); // default is 585
                                break;
                            case "[DetailsLayout.Top.Offset]":
                                iValue = (cbMiniShowDetails.Checked) ? 25 : 1000;
                                target.SetAttributeValue("Value", $"{iValue}"); // default is 25
                                break;
                            case "[DetailsLayout.Bottom.Offset]":
                                break;
                            case "[ContentImageLayout.Top.Offset]":
                                iValue = (cbMiniShowDetails.Checked) ? 31 : 1000;
                                target.SetAttributeValue("Value", $"{iValue}"); // default is 31
                                break;
                        }
                    }

                    // set mini-guide background vertical size
                    var graphic = epgPage.Descendants()
                        .Where(arg => arg.Name.LocalName == "Graphic")
                        .Where(arg => arg.Attribute("Name")?.Value == "MiniGuideBackground").Descendants()
                        .Single(arg => arg.Name.LocalName == "AnchorLayoutInput");
                    graphic.SetAttributeValue("Bottom", "Parent,1"); // default is "Parent,1,10"
                    graphic.SetAttributeValue("Top", $"Parent,0,{(int)((MiniTableTop - Math.Max(ScaleNumber(16) * PixelsPerPoint + 3, 47)) * 972 / 768.0) - 203}"); // default is "Parent,0,323"
                }
            }

            // set the scroll hotspot size and locations
            var scroll = _shellDllResources[(int)shellresource.TABLE_MCML].Descendants()
                .Where(arg => arg.Name.LocalName == "TableAxisAutoScrollRegion")
                .Where(arg => arg.Attribute("Name") != null);
            foreach (var e in scroll)
            {
                var layout = e.Descendants().Single(arg => arg.Name.LocalName == "FormLayoutInput");
                switch (e.Attribute("Name").Value)
                {
                    // up/down images are 44,38
                    // left/right images are 38,44
                    case "Up":
                        e.SetAttributeValue("NavigationHintMargins", "0,0,0,0"); // default is "10,3,10,18"
                        layout.SetAttributeValue("Left", "CornerPanel,1,28"); // default is 20
                        layout.SetAttributeValue("Top", "CornerPanel,1,-44"); // default is -49
                        layout.SetAttributeValue("Bottom", "CornerPanel,1,0"); // default is 10
                        layout.SetAttributeValue("Right", "Parent,1,-28"); // default is -33
                        break;
                    case "Down":
                        e.SetAttributeValue("NavigationHintMargins", "0,0,0,0"); // default is "10,14,10,7"
                        layout.SetAttributeValue("Left", "CornerPanel,1,28"); // default is 20
                        layout.SetAttributeValue("Top", "Parent,1,0"); // default is -10
                        layout.SetAttributeValue("Bottom", "Parent,1,44"); // default is 49
                        layout.SetAttributeValue("Right", "Parent,1,-28"); // default is -33
                        break;
                    case "Left":
                        e.SetAttributeValue("NavigationHintMargins", $"-10,0,0,0"); // default is "5,10,5,10"
                        layout.SetAttributeValue("Left", $"CornerPanel,1,0"); // default is -28
                        layout.SetAttributeValue("Top", $"CornerPanel,1,{ScaleNumber(10)}"); // default is 10
                        layout.SetAttributeValue("Bottom", $"Parent,1,{-ScaleNumber(10)}"); // default is -10
                        layout.SetAttributeValue("Right", $"CornerPanel,1,28"); // default is 20
                        break;
                    case "Right":
                        e.SetAttributeValue("NavigationHintMargins", $"-10,0,{FilterButtonWidth},0"); // default is "5,10,47,10"
                        layout.SetAttributeValue("Left", $"Parent,1,-28"); // default is -33
                        layout.SetAttributeValue("Top", $"CornerPanel,1,{ScaleNumber(10)}"); // default is 10
                        layout.SetAttributeValue("Bottom", $"Parent,1,{-ScaleNumber(10)}"); // default is -10
                        layout.SetAttributeValue("Right", $"Parent,1,{FilterButtonWidth + 10}"); // default is 56
                        break;
                }
            }
        }

        private void SetEpgCellProperties()
        {
            var epgShow = _shellDllResources[(int)shellresource.EPGCELLS_MCML].Descendants()
                .Where(arg => arg.Name.LocalName == "UI")
                .Single(arg => arg.Attribute("Name") != null && arg.Attribute("Name")?.Value == "EpgShowCell");

            // set title line text font size and insets
            var fonts = epgShow.Descendants()
                .Where(arg => arg.Name.LocalName == "Font");
            foreach (var font in fonts)
            {
                switch (font.Attribute("Name")?.Value)
                {
                    case "TitleDefaultFont":
                    case "TitleFocusFont":
                        font.SetAttributeValue("FontSize", trackCellFontSize.Value.ToString()); // default is 22
                        break;
                }
            }

            // set titleline padding
            var panel = epgShow.Descendants()
                .Where(arg => arg.Name.LocalName == "Panel")
                .Single(arg => arg.Attribute("Name") != null && arg.Attribute("Name")?.Value == "TitleLine");
            panel.SetAttributeValue("Padding", $"{ScaleNumber(10)},0,{ScaleNumber(10)},0"); // default is "10,8,10,0"

            // set graphics sizes and margins
            var scaleFactor = CellFontPointSize / 22.0;
            var graphics = epgShow.Descendants()
                .Where(arg => arg.Name.LocalName == "Graphic" && arg.Attribute("Name") != null);
            foreach (var graphic in graphics)
            {
                string value;
                switch (graphic.Attribute("Name")?.Value)
                {
                    case "HDImage":
                        value = $"{(int)(27 * scaleFactor - 26.5)},0,0,0";
                        graphic.SetAttributeValue("Margins", value); // default is "0,6,0,8"
                        value = $"{(int)(27 * Math.Min(scaleFactor, 1.0) + 0.5)},{(int)(17 * Math.Min(scaleFactor, 1.0) + 0.5)}";
                        graphic.SetAttributeValue("MinimumSize", value); // default is "27,17"

                        graphic.SetAttributeValue("Scale", string.Format("{0},{0},{0}", scaleFactor)); // default is null
                        graphic.SetAttributeValue("CenterPointPercent", "1.0,0.5,0.5"); // default is null
                        break;
                    case "ContinuingPrevious":
                        value = $"0,0,{(int)(23 * scaleFactor - 13.5)},0";
                        graphic.SetAttributeValue("Margins", value); // default is "0,1,9,8"
                        value = $"{(int)(14 * Math.Min(scaleFactor, 1.0) + 0.5)},{(int)(19 * Math.Min(scaleFactor, 1.0) + 0.5)}";
                        graphic.SetAttributeValue("MinimumSize", value); // default is "14,19"

                        graphic.SetAttributeValue("Scale", string.Format("{0},{0},{0}", scaleFactor)); // default is null
                        graphic.SetAttributeValue("CenterPointPercent", "0.0,0.5,0.5"); // default is null
                        break;
                    case "ContinuingNext":
                        value = $"{(int)(23 * scaleFactor - 13.5)},0,0,0";
                        graphic.SetAttributeValue("Margins", value); // default is "9,1,0,8"
                        value = $"{(int)(14 * Math.Min(scaleFactor, 1.0) + 0.5)},{(int)(19 * Math.Min(scaleFactor, 1.0) + 0.5)}";
                        graphic.SetAttributeValue("MinimumSize", value); // default is "14,19"

                        graphic.SetAttributeValue("Scale", string.Format("{0},{0},{0}", scaleFactor)); // default is null
                        graphic.SetAttributeValue("CenterPointPercent", "1.0,0.5,0.5"); // default is null
                        break;
                    case "Record":
                        graphic.SetAttributeValue("Margins", "0,0,0,0"); // default is "0,6,0,8"

                        graphic.SetAttributeValue("Scale", string.Format("{0},{0},{0}", scaleFactor)); // default is null
                        graphic.SetAttributeValue("CenterPointPercent", "1.0,0.5,0.5"); // default is null
                        break;
                }
            }

            // set program title margins
            var title = epgShow.Descendants()
                .Where(arg => arg.Name.LocalName == "Text")
                .Single(arg => arg.Attribute("Name") != null && arg.Attribute("Name")?.Value == "Title");
            if (title != null)
            {
                var margin = (int)(((MainTableBottom - MainTableTop) / (float)MainGuideRows - CellFontPointSize * PixelsPerPoint - 6) / 2.0) - (int)(0.08 * CellFontPointSize * PixelsPerPoint + 0.5);
                var value = $"0,{margin},0,0";
                title.SetAttributeValue("Margins", value); // default is null
            }

            // set expanded epg
            if (cbExpandedEpg.Checked)
            {
                var rule = _shellDllResources[(int)shellresource.EPGCELLS_MCML]
                    .Descendants()
                    .Where(arg => arg.Name.LocalName == "Equality")
                    .Single(arg => arg.Attribute("Value")?.Value == "[Cell.Episode]");
                rule.SetAttributeValue("Value", string.Empty);
            }

            // set expanded movie
            if (cbExpandedMovie.Checked)
            {
                var rule = _shellDllResources[(int)shellresource.EPGCELLS_MCML]
                    .Descendants()
                    .Where(arg => arg.Name.LocalName == "Equality")
                    .Single(arg => arg.Attribute("Value")?.Value == "[Cell.MovieYear]");
                rule.SetAttributeValue("Value", string.Empty);
            }
        }

        private void SetTableAndDetailGeometries()
        {
            // set guide visible columns
            var columns = _shellDllResources[(int)shellresource.EPG_MCML]
                .Descendants()
                .Where(arg => arg.Name.LocalName == "Condition") // IsWidescreen
                .Where(arg => arg.Attribute("Target") != null)
                .Single(arg => arg.Attribute("Target")?.Value == "[Table.VisibleColumnCapacity]");
            columns?.SetAttributeValue("Value", trackMinutes.Value.ToString()); // default is 120
            columns = _shellDllResources[(int)shellresource.EPG_MCML]
                .Descendants()
                .Where(arg => arg.Name.LocalName == "Default")
                .Where(arg => arg.Attribute("Target") != null)
                .Single(arg => arg.Attribute("Target")?.Value == "[Table.VisibleColumnCapacity]");
            columns?.SetAttributeValue("Value", trackMinutes.Value.ToString()); // default is 90

            // determine if in widescreen
            var isNormalScreen = false;
            var isWideScreen = false;
            foreach (var screen in Screen.AllScreens)
            {
                var w = screen.Bounds.Width;
                var h = screen.Bounds.Height;
                if ((float)w / (float)h > 1.5f) isWideScreen = true;
                else isNormalScreen = true;
            }

            // if multiple formats, allow user to override the 4x3 offsets
            if (isNormalScreen && isWideScreen)
            {
                if (DialogResult.No ==
                    MessageBox.Show(
                        "You have multiple format displays available. Do you wish to apply settings for a 4x3 display?",
                        "Muliple Display Formats", MessageBoxButtons.YesNo, MessageBoxIcon.Question))
                    isNormalScreen = false;
            }

            // set program / movie details placement
            var inputs = _shellDllResources[(int)shellresource.EPG_MCML].Descendants()
                .Where(arg => arg.Name.LocalName == "FormLayoutInput" || arg.Name.LocalName == "Font")
                .Where(arg => arg.Attribute("Name") != null);
            if (inputs.Any())
            {
                foreach (var input in inputs)
                {
                    switch (input.Attribute("Name")?.Value)
                    {
                        case "InitialGridPosition":
                            input.SetAttributeValue("Right", $"Parent,1,-{FilterButtonWidth + 3}"); // default is "Parent,1,-53"
                            break;
                        case "FinalGridPosition":
                            input.SetAttributeValue("Right", $"Parent,1,{ScaleNumber(230)}"); // default is "Parent,1,298"
                            break;
                        case "DetailsLayout":
                            input.SetAttributeValue("Left", $"FilterButton,0,{(isNormalScreen ? 225 : 315)}"); // default is "FilterButton,1,260"
                            input.SetAttributeValue("Right", $"Table,1,{(isNormalScreen ? -50 : -120)}"); // default is "Table,1,-155"
                            break;
                        case "ContentImageLayout":
                            input.SetAttributeValue("Top", "Parent,0"); // default is "Parent,0"
                            input.SetAttributeValue("Right", $"Parent,0,{(isNormalScreen ? 215 : 300)}"); // default is "Parent,1"
                            break;
                        case "MyTitleFont":
                            input.SetAttributeValue("FontSize", $"{HalfScaleNumber(22)}");
                            break;
                        case "MyOtherFont":
                            input.SetAttributeValue("FontSize", $"{HalfScaleNumber(18)}");
                            break;
                        case "MyClockFont":
                            input.SetAttributeValue("FontSize", $"{HalfScaleNumber(18)}");
                            break;
                        case "MyLabelFont":
                            input.SetAttributeValue("FontSize", $"{HalfScaleNumber(32)}");
                            break;
                        case "MyAlertFont":
                            input.SetAttributeValue("FontSize", $"{HalfScaleNumber(16)}");
                            break;
                    }
                }
            }

            // add channel details bottom line
            var entry = _shellDllResources[(int)shellresource.EPG_MCML].Descendants()
                .Single(arg => arg.Name.LocalName == "ScheduleEntryDetailsView");
            if (cbHideNumber.Checked)
            {
                entry.SetAttributeValue("ShowChannelInfoOnBottomLine", "true");
            }

            if (CellFontPixelHeight > 22)
            {
                entry.SetAttributeValue("TightenTitleLineSpacing", "true");
            }

            // set channel details font size
            var fonts = _shellDllResources[(int)shellresource.EPG_MCML].Descendants()
                .Where(arg => arg.Name.LocalName == "Font")
                .Where(arg => arg.Attribute("Name") != null);
            foreach (var font in fonts)
            {
                switch (font.Attribute("Name").Value)
                {
                    case "NameFont":
                        font.SetAttributeValue("FontSize", $"{HalfScaleNumber(22)}");
                        break;
                    case "OtherFont":
                        font.SetAttributeValue("FontSize", $"{HalfScaleNumber(18)}");
                        break;
                }
            }

            // set channel details time/title offset positions
            var anchors = _shellDllResources[(int)shellresource.EPG_MCML].Descendants()
                .Where(arg =>
                    arg.Name.LocalName == "UI" && arg.Attribute("Name") != null &&
                    arg.Attribute("Name").Value == "ChannelDetailsView").Descendants()
                .Where(arg => arg.Name.LocalName == "AnchorLayoutInput");
            foreach (var anchor in anchors)
            {
                if (!anchor.Parent.Parent.Attribute("Name").Value.StartsWith("Program")) continue;
                anchor.SetAttributeValue("Left", $"Parent,0,{HalfScaleNumber(125)}");
            }

            // set clock position
            var clock = _shellDllResources[(int)shellresource.EPG_MCML].Descendants()
                .Where(arg => arg.Name.LocalName == "Clock").Descendants()
                .Single(arg => arg.Name.LocalName == "FormLayoutInput");
            clock.SetAttributeValue("Right", $"Parent,1,-{FilterButtonWidth + 3}");

            // set brandlogo position
            var brand = _shellDllResources[(int)shellresource.EPG_MCML].Descendants()
                .Where(arg => arg.Name.LocalName == "BrandLogo").Descendants()
                .Single(arg => arg.Name.LocalName == "FormLayoutInput");
            brand.SetAttributeValue("Right", $"Parent,1,-{FilterButtonWidth + 3}");
        }

        private static int SafeTrackBarValue(int value, TrackBar trackbar)
        {
            return Math.Min(Math.Max(value, trackbar.Minimum), trackbar.Maximum);
        }
        #endregion

        #region ========== Resource Files ==========
        private void UpdateShellDll()
        {
            try
            {
                // copy current shell dll file to temp folder
                File.Copy(_shellEhomePath, _shellTempPath, true);

                // update resources of the shell dll in the temp folder
                var updateSuccess = true;
                for (var i = 0; i < (int)shellresource.MAX; ++i)
                {
                    updateSuccess &= ReplaceFileResource(_shellTempPath, ((shellresource)i).ToString().Replace("_", "."), _shellDllResources[i]);
                }

                if (updateSuccess)
                {
                    // kill any processes running for WMC shell
                    foreach (var process in Process.GetProcessesByName("ehshell"))
                    {
                        process.Kill();
                        process.WaitForExit(10000);
                    }

                    foreach (var process in Process.GetProcessesByName("ehexthost"))
                    {
                        process.Kill();
                        process.WaitForExit(10000);
                    }

                    // move the shell dll from the temp folder to the ehome folder
                    try
                    {
                        File.Copy(_shellTempPath, _shellEhomePath, true);
                    }
                    catch (UnauthorizedAccessException)
                    {
                        // take ownership and try again
                        if (TakeOwnership(_shellEhomePath))
                        {
                            try
                            {
                                File.Copy(_shellTempPath, _shellEhomePath, true);
                            }
                            catch
                            {
                                // ignored
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        var msg = $"{Helper.ReportExceptionMessages(ex)}\n";
                        msg += "\nThe following processes are preventing the shell file from being updated:";
                        foreach (var process in FileUtil.WhoIsLocking(_shellEhomePath))
                        {
                            if (process.MainModule != null)
                            {
                                msg += "\n" + process.MainModule.FileVersionInfo.FileDescription;
                                msg += " (" + process.MainModule.FileName + ")";
                            }
                            else
                            {
                                msg += "\n" + process.ProcessName;
                            }
                        }
                        MessageBox.Show(msg, "Shell Update Failed");
                    }
                }

                // cleanup and exit
                File.Delete(_shellTempPath);
            }
            catch (Exception ex)
            {
                Logger.WriteError($"{Helper.ReportExceptionMessages(ex)}");
            }
        }

        private void ImportResources()
        {
            for (var i = 0; i < _shellDllResources.Length; ++i)
            {
                _shellDllResources[i] = GetFileResource(_shellEhomePath, ((shellresource)i).ToString().Replace("_", "."));
            }
        }

        private static XDocument GetFileResource(string resourceName)
        {
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
            {
                using (var reader = new StreamReader(stream))
                {
                    return XDocument.Parse(reader.ReadToEnd());
                }
            }
        }

        private static XDocument GetFileResource(string filePath, string resourceName, uint resType = 23)
        {
            IntPtr hModule, hResource, resourceData, memoryPointer;

            int resourceSize;
            if (((hModule = LoadLibrary(filePath)) != IntPtr.Zero) &&
                ((hResource = FindResource(hModule, resourceName, resType)) != IntPtr.Zero) &&
                ((resourceData = LoadResource(hModule, hResource)) != IntPtr.Zero) &&
                ((resourceSize = SizeOfResource(hModule, hResource)) > 0) &&
                ((memoryPointer = LockResource(resourceData)) != IntPtr.Zero))
            {
                var bytes = new byte[resourceSize];
                Marshal.Copy(memoryPointer, bytes, 0, bytes.Length);
                while (FreeLibrary(hModule)) ;

                // cleanup needed for MCE Reset Toolbox
                var xml = Encoding.ASCII.GetString(bytes);
                return XDocument.Parse(xml.Substring(xml.IndexOf('<')));
            }
            else
            {
                MessageBox.Show($"Failed to get file resource. Error Code: {GetLastError()}");
            }
            return null;
        }

        private static bool ReplaceFileResource(string filePath, string resourceName, XDocument resource,
            int resType = 23, short resLang = 1033)
        {
            IntPtr hUpdate;
            var lpName = new StringBuilder(resourceName);
            var bytes = Encoding.UTF8.GetBytes(resource.ToString(SaveOptions.DisableFormatting).Replace(" />", "/>"));

            if ((hUpdate = BeginUpdateResource(filePath, false)) != IntPtr.Zero &&
                UpdateResource(hUpdate, resType, lpName, resLang, bytes, bytes.Length) == 1 &&
                EndUpdateResource(hUpdate, false)) return true;
            MessageBox.Show($"Failed to update resource. Error Code: {GetLastError()}");
            return false;
        }

        private static bool TakeOwnership(string filePath)
        {
            try
            {
                // user will be prompted to allow takeown to execute
                // will throw if user denies
                var procTakeown = Process.Start(new ProcessStartInfo
                {
                    FileName = "takeown.exe",
                    Arguments = "/f \"" + filePath + "\"",
                    UseShellExecute = true,
                    CreateNoWindow = true,
                    Verb = "runas"
                });
                procTakeown?.WaitForExit();
            }
            catch
            {
                return false;
            }

            // give users modify rights to file
            var procIcacls = Process.Start(new ProcessStartInfo
            {
                FileName = "icacls.exe",
                Arguments = "\"" + filePath + "\" /grant *S-1-5-32-545:(M)",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardError = true,
                Verb = "runas"
            });
            var error = procIcacls?.StandardError.ReadToEnd();
            procIcacls?.WaitForExit();

            return procIcacls?.ExitCode == 0 || error?.Trim() == string.Empty;
        }
        #endregion

        #region ========== Buttons ==========
        private void btnUpdateGuideConfigurations_Click(object sender, EventArgs e)
        {
            // wait cursor
            Cursor = Cursors.WaitCursor;
            FormInitialized = false;

            // populate xdocuments with default files
            for (var i = 0; i < (int)shellresource.MAX; ++i)
            {
                _shellDllResources[i] = GetFileResource("epg123Client.WmcTweak." + ((shellresource)i).ToString().Replace("_", "."));
            }

            // update xdocuments
            if (!sender.Equals(btnResetToDefault))
            {
                SetGuidePageDateTimeFilterFonts();
                SetEpgChannelCellProperties();
                SetTableAndDetailsViewProperties();
                SetEpgCellProperties();
                SetTableAndDetailGeometries();

                // update the shell dll
                UpdateShellDll();
            }
            else
            {
                cbAutoAdjustColumnWidth.Checked = false;
                trackColumnWidth.Value = 0;

                // update the shell dll
                UpdateShellDll();

                MessageBox.Show("All settings have been restored to WMC defaults. Do not click the [Update] button or the defaults will be overwritten.", "Successful Operation", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            // update column width registry
            SetColumnWidthRegistry();

            // download the current resource files
            FormInitialized = GetGuideConfigurations();

            // make sure all updates are reflected
            RecalculateAll();

            // restore arrow cursor
            Cursor = Cursors.Arrow;
        }

        private void btnRemoveLogos_Click(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;
            WmcStore.ClearLineupChannelLogos();
            Cursor = Cursors.Arrow;
        }

        private void BtnUpdateFilePattern(object sender, EventArgs e)
        {
            using (var key = Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Media Center\Service\Recording", true))
            {
                if (key == null) return;
                try
                {
                    if (sender.Equals(btnSetPattern))
                    {
                        txtNamePattern.Text = txtNamePattern.Text.TrimStart(' ').TrimEnd(' ');
                        key.SetValue("filenaming", txtNamePattern.Text, RegistryValueKind.String);
                    }
                    else
                    {
                        key.DeleteValue("filenaming");
                    }
                    ReadRegistries();
                }
                catch
                {
                    // ignored
                }
            }
        }

        private void btnTunerLimit_Click(object sender, EventArgs e)
        {
            // activate the wait cursor for the tweak form
            UseWaitCursor = true;

            if (WmcStore.SetWmcTunerLimits(TunerLimit))
            {
                MessageBox.Show("The tuner limit increase has been successfully applied.", "Tuner Limit Tweak", MessageBoxButtons.OK);
            }
            else
            {
                MessageBox.Show("The tuner limit increase tweak has failed.", "Tuner Limit Tweak", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            // restore cursor for tweak form
            UseWaitCursor = false;
        }

        private void txtNamePattern_KeyPress(object sender, KeyPressEventArgs e)
        {
            char[] invalidChars = { '"', '<', '>', '|', ':', '*', '?', '\\', '/' };
            if (invalidChars.Contains(e.KeyChar))
            {
                e.Handled = true;
            }
        }
        #endregion

        #region ========== Registry =========
        private void ReadRegistries()
        {
            using (var key = Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Media Center\Service\Recording", false))
            {
                if (key != null)
                {
                    try
                    {
                        txtNamePattern.Text = key.GetValue("filenaming", "%T_%Cs_%Dt").ToString();
                    }
                    catch
                    {
                        // ignored
                    }
                }
            }

            using (var key = Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Media Center\Service\Video\Tuners\DVR", false))
            {
                if (key != null)
                {
                    try
                    {
                        var files = (int)key.GetValue("BackingStoreMaxNumBackingFiles", 8);
                        var seconds = (int)key.GetValue("BackingStoreEachFileDurationSeconds", 300);
                        numBuffer.Value = files * seconds / 60;
                    }
                    catch
                    {
                        // ignored
                    }
                }
            }

            using (var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Media Center\Settings\VideoSettings", false))
            {
                if (key != null)
                {
                    try
                    {
                        numSkipAhead.Value = (int)key.GetValue("SkipAheadInterval", 29000) / 1000;
                        numInstantReplay.Value = (int)key.GetValue("InstantReplayInterval", 7000) / 1000;
                    }
                    catch
                    {
                        // ignored
                    }
                }
            }

            using (var key = Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Media Center\Settings\MCE.GlobalSettings", false))
            {
                if (key == null) return;
                try
                {
                    lblMovieGuide.Enabled = btnMovieGuide.Enabled =
                        (string)key.GetValue("SystemGeoISO2") != "US" &&
                        (string)key.GetValue("SystemGeoISO2") != "CA" &&
                        (string)key.GetValue("SystemGeoISO2") != "GB";
                }
                catch
                {
                    // ignored
                }
            }
        }

        private void UpdateRegistryValues(object sender, EventArgs e)
        {
            if (sender.Equals(numBuffer))
            {
                using (var key = Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Media Center\Service\Video\Tuners\DVR", true))
                {
                    if (key != null)
                    {
                        try
                        {
                            var files = (int)key.GetValue("BackingStoreMaxNumBackingFiles", 8);
                            key.SetValue("BackingStoreEachFileDurationSeconds", (int)(((double)numBuffer.Value * 60.0) / files + 0.5), RegistryValueKind.DWord);
                        }
                        catch
                        {
                            // ignored
                        }
                    }
                }
            }
            else if (sender.Equals(numSkipAhead))
            {
                using (var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Media Center\Settings\VideoSettings", true))
                {
                    if (key != null)
                    {
                        try
                        {
                            key.SetValue("SkipAheadInterval", numSkipAhead.Value * 1000, RegistryValueKind.DWord);
                        }
                        catch
                        {
                            // ignored
                        }
                    }
                }
            }
            else if (sender.Equals(numInstantReplay))
            {
                using (var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Media Center\Settings\VideoSettings", true))
                {
                    if (key != null)
                    {
                        try
                        {
                            key.SetValue("InstantReplayInterval", numInstantReplay.Value * 1000, RegistryValueKind.DWord);
                        }
                        catch
                        {
                            // ignored
                        }
                    }
                }
            }
            else if (sender.Equals(btnMovieGuide))
            {
                using (var key = Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Media Center\Settings\MCE.GlobalSettings", true))
                {
                    if (key != null)
                    {
                        try
                        {
                            key.SetValue("SystemGeoISO2", "US", RegistryValueKind.String);
                        }
                        catch
                        {
                            // ignored
                        }
                    }
                }
            }
            ReadRegistries();
        }

        private void SetColumnWidthRegistry()
        {
            using (var key = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Media Center\\Settings\\ProgramGuide", true))
            {
                key?.SetValue("ChannelCellWidth", trackColumnWidth.Value, RegistryValueKind.DWord);
            }
        }

        private void txtNamePattern_TextChanged(object sender, EventArgs e)
        {
            lblPatternExample.Text = txtNamePattern.Text.Replace("%T", "Home Town")
                .Replace("%Et", "s03e07 Home is Where the Art Is")
                .Replace("%Dt", "2019_10_06_11_00_00")
                .Replace("%Ch", "37")
                .Replace("%Cn", "Home && Garden Television (Pacific)")
                .Replace("%Cs", "HGTVP")
                .Replace("%Do", "2019_02_25_00_00_00") + ".wtv";
        }
        #endregion

        private void RdoCheckedChanged(object sender, EventArgs e)
        {
            var btn = sender as RadioButton;
            if (!btn.Checked) return;

            using (var key = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Media Center\\Start Menu", true))
            {
                try
                {
                    var imagePath = "file://" + Helper.Epg123StatusLogoPath;
                    if (((string)btn.Tag).Equals("None"))
                    {
                        imagePath = string.Empty;
                    }

                    key?.SetValue("OEMLogoAccent", $"{(string)btn.Tag}" + (cbNoSuccess.Checked ? "_ns" : string.Empty), RegistryValueKind.String);
                    key?.SetValue("OEMLogoUri", imagePath);
                }
                catch
                {
                    // ignored
                }
            }

            SetStatusLogoImage();
        }

        private void cbNoSuccess_CheckedChanged(object sender, EventArgs e)
        {
            var checkBox = (CheckBox)sender;
            using (var key =
                Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Media Center\\Start Menu", true))
            {
                if (key == null) return;
                try
                {
                    var val = string.Empty;
                    if (rdoLight.Checked)
                    {
                        val = "Light";
                    }
                    else if (rdoDark.Checked)
                    {
                        val = "Dark";
                    }

                    if (string.IsNullOrEmpty(val)) return;
                    key?.SetValue("OEMLogoAccent", val + (checkBox.Checked ? "_ns" : string.Empty), RegistryValueKind.String);
                    if (!checkBox.Checked)
                    {
                        key?.SetValue("OEMLogoUri", "file://" + Helper.Epg123StatusLogoPath);
                    }
                }
                catch
                {
                    // ignored
                }
            }
        }

        private void TrkOpacityChanged(object sender, EventArgs e)
        {
            using (var key = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Media Center\\Start Menu", true))
            {
                try
                {
                    key?.SetValue("OEMLogoOpacity", trackBar1.Value, RegistryValueKind.DWord);
                    lblStatusLogoOpaque.Text = $"{trackBar1.Value}% Opaque";
                }
                catch
                {
                    // ignored
                }
            }

            if (pbStatusLogo.Image != null)
            {
                SetStatusLogoImage();
            }
        }

        private void SetStatusLogoImage()
        {
            Bitmap bmp = null;
            if (rdoLight.Checked)
            {
                pbStatusLogo.BackColor = Color.Black;
                bmp = new Bitmap(resImages.statusLogoLight);
            }
            else if (rdoDark.Checked)
            {
                pbStatusLogo.BackColor = Color.White;
                bmp = new Bitmap(resImages.statusLogoDark);
            }
            else
            {
                pbStatusLogo.BackColor = SystemColors.Control;
            }

            statusLogo.AdjustImageOpacity(bmp, trackBar1.Value / 100.0);
            pbStatusLogo.Image = bmp;
        }
    }
}