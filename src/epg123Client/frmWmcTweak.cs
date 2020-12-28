using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml.Linq;
using System.Windows.Forms;
using Microsoft.Win32;
using epg123Client;

namespace epg123
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
            MAX
        }

        // enumeration of resources in ehres.dll
        private enum resresource
        {
            GUIDEDETAILSBASE_XML,
            //DEFAULTGEOSETTINGS_XML,
            MAX
        }

        private readonly XDocument[] _shellDllResources = new XDocument[(int) shellresource.MAX];
        private readonly XDocument[] _resDllResources = new XDocument[(int) resresource.MAX];

        // filepaths
        private readonly string _shellEhomePath = Environment.GetEnvironmentVariable("WINDIR") + @"\ehome\Microsoft.MediaCenter.Shell.dll";

        private readonly string _shellTempPath = Environment.GetEnvironmentVariable("TEMP") + @"\Microsoft.MediaCenter.Shell.dll";

        private readonly string _resEhomePath = Environment.GetEnvironmentVariable("WINDIR") + @"\ehome\ehres.dll";

        // calculation constants
        private const double PixelsPerPoint = 1.33;
        private const int MinMainTableTop = 50;
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
        private int DetailFontPointSize { get; set; }
        private int MainGuideRows { get; set; }
        private int MiniGuideRows { get; set; }
        private int ColumnMinutes { get; set; }

        // calculated parameters
        private int CellFontPixelHeight => (int)(CellFontPointSize * PixelsPerPoint);
        private int RowHeightPixel => (int)(CellFontPixelHeight * RowHeightMultiplier + 0.5);
        private int MaxMainTableRows => (int)((MaxTableBottom - MinMainTableTop + ((ShowMainDetails) ? 0 : DetailsVerticalSize)) / (double)RowHeightPixel);
        private int MainTableTop => ((MinMainTableTop + MaxTableBottom + ((ShowMainDetails) ? 0 : DetailsVerticalSize) - (MainGuideRows * RowHeightPixel)) / 2);
        private int MainTableBottom => (MainTableTop + RowHeightPixel * MainGuideRows);
        private int DetailsVerticalSize => (int) ((4.2 * DetailFontPointSize) * PixelsPerPoint);
        private int MaxTableBottom => (768 - MinMainTableTop - DetailsVerticalSize);
        private int MiniTableBottom => (MaxTableBottom + (ShowMiniDetails ? 0 : DetailsVerticalSize));
        private int MiniTableTop => (MiniTableBottom - RowHeightPixel * MiniGuideRows);
        private int MaxMiniTableRows => (int) ((MiniTableBottom - MinMiniTableTop) / (double) RowHeightPixel);
        private int SmallLogoHeight => CellFontPixelHeight;
        private int LargeLogoHeight => Math.Min(RowHeightPixel, 75);
        private int MediumLogoHeight => (int) ((SmallLogoHeight + LargeLogoHeight) / 2.0);

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
            TrackBar[] bars = {trackCellFontSize, trackMainRows, trackMiniRows, trackMinutes, trackColumnWidth, trackRowHeight};

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
                    value = (int)((double) value / step + 0.5) * step;
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
            return 3 * logoHeight + 10 + (cbHideNumber.Checked ? 0 : (int)(CellFontPixelHeight * 2.67) + 10);
        }
        #endregion

        #region ========== Resource Read/Writes ==========
        private bool GetGuideConfigurations()
        {
            // get cell font size
            var font1 = _shellDllResources[(int) shellresource.EPGCELLS_MCML]
                .Descendants()
                .Where(arg => arg.Name.LocalName == "Font")
                .Where(arg => arg.Attribute("Name") != null)
                .Single(arg => arg.Attribute("Name").Value == "TitleDefaultFont");
            if (font1 != null)
            {
                trackCellFontSize.Value = CellFontPointSize = SafeTrackBarValue(int.Parse(font1.Attribute("FontSize").Value), trackCellFontSize);
            }

            // get detail font size
            var font2 = _resDllResources[(int) resresource.GUIDEDETAILSBASE_XML]
                .Descendants()
                .Where(arg => arg.Name.LocalName == "Font")
                .Where(arg => arg.Attribute("Name") != null)
                .Single(arg => arg.Attribute("Name").Value == "TitleFont");
            if (font2 != null)
            {
                DetailFontPointSize = int.Parse(font2.Attribute("FontSize").Value);
            }

            // get main guide rows and mini guide rows
            var actions = _shellDllResources[(int) shellresource.EPG_MCML].Descendants()
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
            var columns = _shellDllResources[(int) shellresource.EPG_MCML]
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

        private void SetFontSizes()
        {
            var test = _shellDllResources[(int) shellresource.EPG_MCML]
                .Descendants()
                .Where(arg => arg.Name.LocalName == "Size")
                .Where(arg => arg.Attribute("Name") != null)
                .Single(arg => arg.Attribute("Name").Value == "GuideShowCardImageSize");
            test?.SetAttributeValue("Size", "210,150");

            // set program/movie details font sizes
            var fonts1 = _resDllResources[(int) resresource.GUIDEDETAILSBASE_XML].Descendants()
                .Where(arg => arg.Name.LocalName == "Font")
                .Where(arg => arg.Parent.Name.LocalName == "Properties")
                .Where(arg => arg.Parent.Parent.Name.LocalName == "UI")
                .Where(arg => arg.Parent.Parent.Attribute("Name").Value == "GuideDetailsBase");
            foreach (var font in fonts1)
            {
                if (font.Attribute("Name") == null) continue;

                switch (font.Attribute("Name").Value)
                {
                    case "TitleFont":
                        font.SetAttributeValue("FontSize", DetailFontPointSize.ToString()); // default 22
                        break;
                    case "OtherFont":
                    case "ClockFont":
                    case "LabelFont":
                    case "AlertFont":
                        var value = (int)(DetailFontPointSize * 0.82 + 0.5);
                        font.SetAttributeValue("FontSize", value.ToString()); // default is 18
                        break;
                }
            }

            // set channel details font sizes
            var fonts2 = _shellDllResources[(int)shellresource.EPG_MCML].Descendants()
                .Where(arg => arg.Name.LocalName == "Font")
                .Where(arg => arg.Parent.Name.LocalName == "Properties")
                .Where(arg => arg.Parent.Parent.Name.LocalName == "UI")
                .Where(arg => arg.Parent.Parent.Attribute("Name").Value == "ChannelDetailsView");
            foreach (var font in fonts2)
            {
                if (font.Attribute("Name") == null) continue;

                switch (font.Attribute("Name").Value)
                {
                    case "NameFont":
                        font.SetAttributeValue("FontSize", DetailFontPointSize.ToString()); // default is 22
                        break;
                    case "OtherFont":
                        var value = (int)(DetailFontPointSize * 0.82 + 0.5);
                        font.SetAttributeValue("FontSize", value.ToString()); // default is 18
                        break;
                }
            }

            // set on demand details font sizes
            var fonts3 = _shellDllResources[(int)shellresource.EPG_MCML].Descendants()
                .Where(arg => arg.Name.LocalName == "Font")
                .Where(arg => arg.Parent.Name.LocalName == "Properties")
                .Where(arg => arg.Parent.Parent.Name.LocalName == "UI")
                .Where(arg => arg.Parent.Parent.Attribute("Name").Value == "OnDemandOfferView");
            foreach (var font in fonts3)
            {
                if (font.Attribute("Name") == null) continue;

                switch (font.Attribute("Name").Value)
                {
                    case "TitleFont":
                        font.SetAttributeValue("FontSize", DetailFontPointSize.ToString()); // default is 22
                        break;
                    case "OtherFont":
                        var value = (int)(DetailFontPointSize * 0.82 + 0.5);
                        font.SetAttributeValue("FontSize", value.ToString()); // default is 18
                        break;
                }
            }

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

            // set table column header font sizes
            var columnHeaderFontSize = 16 - (ColumnMinutes - 120) / 30;
            var fonts5 = _shellDllResources[(int)shellresource.EPGCELLS_MCML]
                .Descendants()
                .Where(arg => arg.Name.LocalName == "UI")
                .Where(arg => arg.Attribute("Name") != null)
                .Single(arg => arg.Attribute("Name").Value == "EpgTimeCell");
            var fonts6 = _shellDllResources[(int)shellresource.EPGCOMMON_MCML]
                .Descendants()
                .Where(arg => arg.Name.LocalName == "UI")
                .Where(arg => arg.Attribute("Name") != null)
                .Single(arg => arg.Attribute("Name").Value == "EpgDateCell");
            if (fonts5 != null)
            {
                var timeFont = fonts5
                    .Descendants()
                    .Where(arg => arg.Name.LocalName == "Font")
                    .Where(arg => arg.Attribute("Name") != null)
                    .Single(arg => arg.Attribute("Name").Value == "TimeFont");
                timeFont?.SetAttributeValue("FontSize", columnHeaderFontSize.ToString()); // default is 16

                var dateFont = fonts6
                    .Descendants()
                    .Where(arg => arg.Name.LocalName == "Font")
                    .Where(arg => arg.Attribute("Name") != null)
                    .Single(arg => arg.Attribute("Name").Value == "DateFont");
                dateFont?.SetAttributeValue("FontSize", columnHeaderFontSize.ToString()); // default is 16
            }

            // set page title font size "guide"
            var fonts4 = _shellDllResources[(int)shellresource.EPG_MCML]
                .Descendants()
                .Where(arg => arg.Name.LocalName == "Font")
                .Where(arg => arg.Parent.Name.LocalName == "Font")
                .Where(arg => arg.Parent.Parent.Name.LocalName == "StaticText")
                .Single(arg => arg.Parent.Parent.Attribute("Name").Value == "Title");
            if (fonts4 != null)
            {
                var maxFont = (int)((0.7 * MainTableTop - 18 + (16 - columnHeaderFontSize) * 1.33) / 1.33);
                var value = maxFont >= 22 ? Math.Min(maxFont, 48) : 0;
                fonts4.SetAttributeValue("FontSize", value.ToString()); // default is 48
            }

            // set channel cell column font sizes and insets (first column)
            var fonts7 = _shellDllResources[(int)shellresource.EPGCELLS_MCML]
                .Descendants()
                .Where(arg => arg.Name.LocalName == "UI")
                .Where(arg => arg.Attribute("Name") != null)
                .Single(arg => arg.Attribute("Name").Value == "EpgChannelCell");
            if (fonts7 != null)
            {
                var fonts = fonts7.Descendants()
                    .Where(arg => arg.Name.LocalName == "Font")
                    .Where(arg => arg.Attribute("Name") != null);
                foreach (var font in fonts)
                {
                    int value;
                    switch (font.Attribute("Name").Value)
                    {
                        case "NumberExpandedFont":
                            font.SetAttributeValue("FontSize", trackCellFontSize.Value.ToString()); // default is 22
                            break;
                        case "NumberDefaultFont":
                            value = (cbRemoveAnimations.Checked) ? trackCellFontSize.Value : (int)(trackCellFontSize.Value * 0.82 + 0.5);
                            font.SetAttributeValue("FontSize", value.ToString()); // default is 18
                            break;
                        case "CallsignDefaultFont":
                        case "CallsignExpandedFont":
                            value = (int)(trackCellFontSize.Value * 0.82 - 1);
                            font.SetAttributeValue("FontSize", value.ToString()); // default is 18
                            break;
                    }
                }

                var insets = fonts7.Descendants()
                    .Where(arg => arg.Name.LocalName == "Inset")
                    .Where(arg => arg.Attribute("Name") != null);
                var logoHeight = (trackLogoSize.Value == 0) ? SmallLogoHeight : trackLogoSize.Value == 1 ? MediumLogoHeight : LargeLogoHeight;
                foreach (var inset in insets)
                {
                    string value;
                    switch (inset.Attribute("Name").Value)
                    {
                        case "BackgroundDefaultPadding": // channel logo
                            value = string.Format("0,{0},{1},{0}", cbRemoveAnimations.Checked ? 0 : (int)((RowHeightPixel - 0.82 * logoHeight) / 2.0 - 0.5), cbHideNumber.Checked ? 0 : 5);
                            inset.SetAttributeValue("Inset", value); // default is "10,12,10,8"
                            break;
                        case "BackgroundExpandedPadding": // channel logo
                            value = (cbHideNumber.Checked) ? "0,0,0,0" : "0,0,5,0";
                            inset.SetAttributeValue("Inset", value); // default is "10,6,10,6"
                            break;
                        case "DefaultNumberMargins":
                            value = cbHideNumber.Checked ? "-1,0,0,0" : string.Format("5,{0},0,{0}", cbRemoveAnimations.Checked ? 0 : -(int)((RowHeightPixel - 0.82 * logoHeight) / 2.0));
                            inset.SetAttributeValue("Inset", value); // default is "0,0,0,0"
                            break;
                        case "FocusNumberMargins":
                            value = (cbHideNumber.Checked) ? "-1,0,0,0" : "5,0,0,0";
                            inset.SetAttributeValue("Inset", value); // default is "-3,2,0,0"
                            break;
                        case "DefaultCallsignMargins":
                            inset.SetAttributeValue("Inset", "5,0,0,0"); // default is "0,0,0,0"
                            break;
                        case "FocusCallsignMargins":
                            inset.SetAttributeValue("Inset", "5,0,0,0");
                            break;
                    }
                }

                var panels = fonts7.Descendants()
                    .Where(arg => arg.Name.LocalName == "Panel")
                    .Where(arg => arg.Attribute("Name") != null);
                foreach (var panel in panels)
                {
                    XElement e;
                    switch (panel.Attribute("Name").Value)
                    {
                        case "SmallLogoPanel":
                            e = panel.Descendants().Single(arg => arg.Name.LocalName == "FormLayoutInput");
                            e.SetAttributeValue("Bottom", "Parent,1"); // default is "Number,1"
                            e.SetAttributeValue("Vertical", "Center"); // default is null
                            e.SetAttributeValue("Horizontal", cbCenterLogo.Checked || cbHideNumber.Checked ? "Center" : null); // default is null
                            e.SetAttributeValue("Left", cbCenterLogo.Checked && !cbHideNumber.Checked ? "Parent,1," + (3 * CellFontPixelHeight - trackColumnWidth.Value + 5).ToString() : null);

                            e = panel.Descendants().Single(arg => arg.Name.LocalName == "ChannelLogo");
                            var value = $"{3 * logoHeight},{logoHeight}";
                            e.SetAttributeValue("MaximumSize", value); // default is "70,40"
                            break;
                        case "CallsignPanel":
                            e = panel.Descendants().Single(arg => arg.Name.LocalName == "FormLayoutInput");
                            e.SetAttributeValue("Bottom", "Parent,1"); // default is "Number,1"
                            e.SetAttributeValue("Vertical", "Center"); // default is null
                            break;
                    }
                }

                var ui = _shellDllResources[(int)shellresource.EPGCOMMON_MCML].Descendants()
                    .Where(arg => arg.Name.LocalName == "UI")
                    .Where(arg => arg.Attribute("Name") != null);
                foreach (var e in ui)
                {
                    switch (e.Attribute("Name").Value)
                    {
                        case "ChannelLogo":
                            var value = $"{3 * logoHeight},{logoHeight}";
                            e.Descendants().Single(arg => arg.Name.LocalName == "Size")
                                .SetAttributeValue("Size", value); // default is "75,35"
                            break;
                    }
                }

                var text = fonts7
                    .Descendants()
                    .Where(arg => arg.Name.LocalName == "Text")
                    .Where(arg => arg.Attribute("Name") != null)
                    .Single(arg => arg.Attribute("Name").Value == "Number");
                if (text != null)
                {
                    var value = cbHideNumber.Checked ? "1,0" : $"{CellFontPixelHeight * 3},0";
                    text.SetAttributeValue("MaximumSize", value); // default is "75,0"

                    var layout = text.Descendants().Single(arg => arg.Name.LocalName == "FormLayoutInput");
                    layout?.SetAttributeValue("Vertical", "Center"); // default is null
                }
            }

            // set title line text font size and insets
            var fonts8 = _shellDllResources[(int) shellresource.EPGCELLS_MCML]
                .Descendants()
                .Where(arg => arg.Name.LocalName == "UI")
                .Where(arg => arg.Attribute("Name") != null)
                .Single(arg => arg.Attribute("Name").Value == "EpgShowCell");
            if (fonts8 != null)
            {
                var fonts = fonts8.Descendants()
                    .Where(arg => arg.Name.LocalName == "Font")
                    .Where(arg => arg.Attribute("Name") != null)
                    .Where(arg => arg.Parent.Name.LocalName == "Properties");
                foreach (var font in fonts)
                {
                    switch (font.Attribute("Name").Value)
                    {
                        case "TitleDefaultFont":
                        case "TitleFocusFont":
                            font.SetAttributeValue("FontSize", trackCellFontSize.Value.ToString()); // default is 22
                            break;
                    }
                }

                var scaleFactor = CellFontPointSize / 22.0;
                var panel = fonts8
                    .Descendants()
                    .Where(arg => arg.Name.LocalName == "Panel")
                    .Where(arg => arg.Attribute("Name") != null)
                    .Single(arg => arg.Attribute("Name").Value == "TitleLine");
                if (panel != null)
                {
                    var value = string.Format("{0},0,{0},0", (int)(10 * scaleFactor + 0.5));
                    panel.SetAttributeValue("Padding", value); // default is "10,8,10,0"
                }

                var graphics = fonts8.Descendants()
                    .Where(arg => arg.Name.LocalName == "Graphic")
                    .Where(arg => arg.Attribute("Name") != null);
                foreach (var graphic in graphics)
                {
                    string value;
                    switch (graphic.Attribute("Name").Value)
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

                var title = fonts8
                    .Descendants()
                    .Where(arg => arg.Name.LocalName == "Text")
                    .Where(arg => arg.Attribute("Name") != null)
                    .Single(arg => arg.Attribute("Name").Value == "Title");
                if (title != null)
                {
                    var margin = (int)(((MainTableBottom - MainTableTop) / MainGuideRows - CellFontPointSize * PixelsPerPoint - 6) / 2.0) - (int)(0.08 * CellFontPointSize * PixelsPerPoint + 0.5);
                    var value = $"0,{margin},0,0";
                    title.SetAttributeValue("Margins", value); // default is null
                }
            }
        }

        private void SetTableGeometries()
        {
            // get main guide rows and mini guide rows
            var actions = _shellDllResources[(int) shellresource.EPG_MCML].Descendants()
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
                if (mode != null)
                {
                    if (mode.Attribute("Value").Value == "false") // main guide
                    {
                        foreach (var target in action.Descendants())
                        {
                            if (target.Attribute("Target") == null) continue;
                            int value;
                            switch (target.Attribute("Target").Value)
                            {
                                case "[Table.VisibleRowCapacity]":
                                    target.SetAttributeValue("Value", MainGuideRows.ToString()); // default is 7
                                    break;
                                case "[FilterButtonLayout.Top.Offset]":
                                    target.SetAttributeValue("Value", MainTableTop.ToString()); // default is 118
                                    break;
                                case "[FilterButtonLayout.Left.Offset]":
                                    target.SetAttributeValue("Value", "0"); // default is 55
                                    break;
                                case "[FilterButtonLayout.Right.Offset]":
                                    target.SetAttributeValue("Value", "55"); // default is 106
                                    break;
                                case "[FilterButtonLayout.Bottom.Offset]":
                                    target.SetAttributeValue("Value", MainTableBottom.ToString()); // default is 493
                                    break;
                                case "[DetailsLayout.Top.Offset]":
                                    value = cbMainShowDetails.Checked ? 6 : 1000;
                                    target.SetAttributeValue("Value", value.ToString()); // defualt is 27
                                    break;
                                case "[ContentImageLayout.Top.Offset]":
                                    value = cbMainShowDetails.Checked ? 12 : 1000;
                                    target.SetAttributeValue("Value", value.ToString()); // default is 34
                                    break;
                            }
                        }
                    }
                    else // mini guide
                    {
                        foreach (var target in action.Descendants())
                        {
                            if (target.Attribute("Target") == null) continue;
                            int value;
                            switch (target.Attribute("Target").Value)
                            {
                                case "[Table.VisibleRowCapacity]":
                                    target.SetAttributeValue("Value", MiniGuideRows.ToString()); // default is 2
                                    break;
                                case "[FilterButtonLayout.Top.Offset]":
                                    target.SetAttributeValue("Value", MiniTableTop.ToString()); // default is 476
                                    break;
                                case "[FilterButtonLayout.Left.Offset]":
                                    target.SetAttributeValue("Value", "0"); // default is 54
                                    break;
                                case "[FilterButtonLayout.Right.Offset]":
                                    target.SetAttributeValue("Value", "55"); // default is 55
                                    break;
                                case "[FilterButtonLayout.Bottom.Offset]":
                                    target.SetAttributeValue("Value", MiniTableBottom.ToString()); // default is 585
                                    break;
                                case "[DetailsLayout.Top.Offset]":
                                    value = (cbMiniShowDetails.Checked) ? 6 : 1000;
                                    target.SetAttributeValue("Value", value.ToString()); // default is 25
                                    break;
                                case "[ContentImageLayout.Top.Offset]":
                                    value = (cbMiniShowDetails.Checked) ? 12 : 1000;
                                    target.SetAttributeValue("Value", value.ToString()); // default is 31
                                    break;
                            }
                        }
                    }
                }
            }

            // set guide visible columns
            var columns = _shellDllResources[(int) shellresource.EPG_MCML]
                .Descendants()
                .Where(arg => arg.Name.LocalName == "Condition") // IsWidescreen
                .Where(arg => arg.Attribute("Target") != null)
                .Single(arg => arg.Attribute("Target").Value == "[Table.VisibleColumnCapacity]");
            columns?.SetAttributeValue("Value", trackMinutes.Value.ToString()); // default is 120
            columns = _shellDllResources[(int) shellresource.EPG_MCML]
                .Descendants()
                .Where(arg => arg.Name.LocalName == "Default")
                .Where(arg => arg.Attribute("Target") != null)
                .Single(arg => arg.Attribute("Target").Value == "[Table.VisibleColumnCapacity]");
            columns?.SetAttributeValue("Value", trackMinutes.Value.ToString()); // default is 90

            // set program / movie details placement
            var inputs = _shellDllResources[(int) shellresource.EPG_MCML].Descendants()
                .Where(arg => arg.Name.LocalName == "FormLayoutInput")
                .Where(arg => arg.Attribute("Name") != null);
            if (inputs.Any())
            {
                foreach (var input in inputs)
                {
                    switch (input.Attribute("Name").Value)
                    {
                        case "DetailsLayout":
                            input.SetAttributeValue("Left", "FilterButton,0,225"); // default is "FilterButton,1,260"
                            input.SetAttributeValue("Right", "Table,1,-74"); // default is "Table,1,-155"
                            break;
                        case "ContentImageLayout":
                            input.SetAttributeValue("Top", "Parent,0"); // default is "Parent,0"
                            input.SetAttributeValue("Right", "Parent,0,215"); // default is "Parent,1"
                            break;
                    }
                }
            }

            // set channel column width
            using (var key = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Media Center\\Settings\\ProgramGuide", true))
            {
                key?.SetValue("ChannelCellWidth", trackColumnWidth.Value, RegistryValueKind.DWord); // default is 0
            }
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
                for (var i = 0; i < (int) shellresource.MAX; ++i)
                {
                    updateSuccess &= ReplaceFileResource(_shellTempPath, ((shellresource) i).ToString().Replace("_", "."), _shellDllResources[i]);
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
                        var msg = ex.Message + "\n";
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
                Logger.WriteError(ex.Message);
                if (ex.InnerException != null) Logger.WriteError(ex.InnerException.Message);
                Logger.WriteError(ex.StackTrace);
            }
        }

        private void ImportResources()
        {
            for (var i = 0; i < _shellDllResources.Length; ++i)
            {
                _shellDllResources[i] = GetFileResource(_shellEhomePath, ((shellresource) i).ToString().Replace("_", "."));
            }

            for (var i = 0; i < _resDllResources.Length; ++i)
            {
                _resDllResources[i] = GetFileResource(_resEhomePath, ((resresource) i).ToString().Replace("_", "."));
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
            var resourceSize = 0;
            IntPtr hModule, hResource, resourceData, memoryPointer;

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
            for (var i = 0; i < (int) shellresource.MAX; ++i)
            {
                _shellDllResources[i] = GetFileResource("epg123Client." + ((shellresource) i).ToString().Replace("_", "."));
            }

            // update xdocuments
            if (!sender.Equals(btnResetToDefault))
            {
                SetFontSizes();
                SetTableGeometries();

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

            var startInfo = new ProcessStartInfo()
            {
                FileName = "epg123Client.exe",
                Arguments = "-nologo"
            };
            var proc = Process.Start(startInfo);
            proc?.WaitForExit();

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

            if (WmcUtilities.SetWmcTunerLimits(TunerLimit))
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
            char[] invalidChars = {'"', '<', '>', '|', ':', '*', '?', '\\', '/'};
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
                        (string) key.GetValue("SystemGeoISO2") != "US" &&
                        (string) key.GetValue("SystemGeoISO2") != "CA" &&
                        (string) key.GetValue("SystemGeoISO2") != "GB";
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
                    if (((string) btn.Tag).Equals("None"))
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
            var checkBox = (CheckBox) sender;
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