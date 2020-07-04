using System;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml.Linq;
using System.Windows.Forms;
using Microsoft.MediaCenter.Store;
using Microsoft.Win32;

namespace epg123
{
    public partial class frmWmcTweak : Form
    {
        #region ========== Native Externals =========
        [DllImport("Kernel32.dll", BestFitMapping = false, CharSet = CharSet.Unicode, SetLastError = true, ThrowOnUnmappableChar = true)]
        public static extern IntPtr BeginUpdateResource(string pFileName, [MarshalAs(UnmanagedType.Bool)] bool bDeleteExistingResources);

        [DllImport("Kernel32.dll", BestFitMapping = false, CharSet = CharSet.Unicode, SetLastError = true, ThrowOnUnmappableChar = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool UpdateResource(IntPtr updateHandle, IntPtr type, IntPtr name, ushort lang, IntPtr data, int length);

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
        private enum SHELLRESOURCE
        {
            EPG_MCML,
            EPGCELLS_MCML,
            EPGCOMMON_MCML,
            MAX
        }

        // enumeration of resources in ehres.dll
        private enum RESRESOURCE
        {
            GUIDEDETAILSBASE_XML,
            //DEFAULTGEOSETTINGS_XML,
            MAX
        }
        XDocument[] shellDllResources = new XDocument[(int)SHELLRESOURCE.MAX];
        XDocument[] resDllResources = new XDocument[(int)RESRESOURCE.MAX];

        // filepaths
        string shellEhomePath = Environment.GetEnvironmentVariable("WINDIR") + @"\ehome\Microsoft.MediaCenter.Shell.dll";
        string shellTempPath = Environment.GetEnvironmentVariable("TEMP") + @"\Microsoft.MediaCenter.Shell.dll";
        string resEhomePath = Environment.GetEnvironmentVariable("WINDIR") + @"\ehome\ehres.dll";
        string resTempPath = Environment.GetEnvironmentVariable("TEMP") + @"\ehres.dll";

        // calculation constants
        private const double pixelsPerPoint = 1.33;
        private const int minMainTableTop = 50;
        private const int minMiniTableTop = 300;
        private const int hiddenDetailsOffset = 1000;

        const int TUNERLIMIT = 32;

        public frmWmcTweak()
        {
            InitializeComponent();

            setNamePatternExampleText();

            using (RegistryKey key = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Media Center\\Start Menu", false))
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
                catch { }
            }
        }
        private void frmWmcTweak_Load(object sender, EventArgs e)
        {
            // import all the needed resource files
            importResources();

            // update controls
            formInitialized = getGuideConfigurations();

            // setup widgets
            recalculateAll();

            // populate current values
            readRegistries();
        }
        private void frmWmcTweak_Shown(object sender, EventArgs e)
        {
            this.Refresh();

            // determine if tuner limit tweak is in place
            lblTunerLimit.Enabled = btnTunerLimit.Enabled = !isTunerCountTweaked();
        }

        private void setNamePatternExampleText()
        {
            textBox2.Text = "HomeTown\r\n" +
                            "s03e07 Home is Where the Art Is\r\n" +
                            string.Format("{0:G}\r\n", new DateTime(2019, 10, 06, 11, 0, 0)) +
                            string.Format("{0:G}\r\n", new DateTime(2019, 2, 25, 0, 0 , 0)) +
                            "37\r\n" +
                            "HGTVP\r\n" +
                            "Home & Garden Television (Pacific)";
        }

        #region ========== Trackbar Calculations =========
        // setable attributes
        private double rowHeightMultiplier { get; set; }
        private bool formInitialized { get; set; }
        private bool showMainDetails { get; set; }
        private bool showMiniDetails { get; set; }
        private int cellFontPointSize { get; set; }
        private int detailFontPointSize { get; set; }
        private int mainGuideRows { get; set; }
        private int miniGuideRows { get; set; }
        private int columnMinutes { get; set; }

        // calculated parameters
        private int cellFontPixelHeight { get { return (int)(cellFontPointSize * pixelsPerPoint); } }
        private int rowHeightPixel { get { return (int)(cellFontPixelHeight * rowHeightMultiplier + 0.5); } }
        private int maxMainTableRows { get { return (int)((maxTableBottom - minMainTableTop + ((showMainDetails) ? 0 : detailsVerticalSize)) / (double)rowHeightPixel); } }
        private int mainTableTop { get { return ((minMainTableTop + maxTableBottom + ((showMainDetails) ? 0 : detailsVerticalSize) - (mainGuideRows * rowHeightPixel)) / 2); } }
        private int mainTableBottom { get { return (mainTableTop + rowHeightPixel * mainGuideRows); } }
        private int detailsVerticalSize { get { return (int)((4.2 * detailFontPointSize) * pixelsPerPoint); } }
        private int maxTableBottom { get { return (768 - minMainTableTop - detailsVerticalSize); } }
        private int miniTableBottom { get { return (maxTableBottom + ((showMiniDetails) ? 0 : detailsVerticalSize)); } }
        private int miniTableTop { get { return (miniTableBottom - rowHeightPixel * miniGuideRows); } }
        private int maxMiniTableRows { get { return (int)((miniTableBottom - minMiniTableTop) / (double)rowHeightPixel); } }
        private int smallLogoHeight { get { return cellFontPixelHeight; } }
        private int largeLogoHeight { get { return Math.Min(rowHeightPixel, 75); } }
        private int mediumLogoHeight { get { return (int)((smallLogoHeight + largeLogoHeight) / 2.0); } }

        // widget reactions
        private void recalculateAll()
        {
            if (!formInitialized) return;

            // cell font size
            cellFontPointSize = trackCellFontSize.Value;
            lblCellFontSize.Text = cellFontPointSize.ToString() + " point";

            // row height
            rowHeightMultiplier = 1.0 + trackRowHeight.Value / 100.0;
            lblRowHeight.Text = string.Format("{0:N2}X Font Height", rowHeightMultiplier);

            // logo size
            switch (trackLogoSize.Value)
            {
                case 0: // small
                    lblLogoSize.Text = string.Format("Small ({0}x{1})", smallLogoHeight * 3, smallLogoHeight);
                    break;
                case 1: // medium
                    lblLogoSize.Text = string.Format("Medium ({0}x{1})", mediumLogoHeight * 3, mediumLogoHeight);
                    break;
                case 2: // large
                    lblLogoSize.Text = string.Format("Large ({0}x{1})", largeLogoHeight * 3, largeLogoHeight);
                    break;
                default:
                    break;
            }

            // detail views
            showMainDetails = cbMainShowDetails.Checked;
            showMiniDetails = cbMiniShowDetails.Checked;

            // main guide rows
            mainGuideRows = Math.Min(trackMainRows.Value, maxMainTableRows);
            trackMainRows.Maximum = maxMainTableRows;
            lblMainRows.Text = mainGuideRows.ToString() + " rows";

            // mini guide rows
            miniGuideRows = Math.Min(trackMiniRows.Value, maxMiniTableRows);
            trackMiniRows.Maximum = maxMiniTableRows;
            lblMiniRows.Text = miniGuideRows.ToString() + " rows";

            // column time
            columnMinutes = trackMinutes.Value;
            lblMinutes.Text = columnMinutes.ToString() + " minutes";

            // channel cell width
            if (cbAutoAdjustColumnWidth.Checked)
            {
                trackColumnWidth.Value = Math.Max(Math.Min(calculateColumnWidth(), trackColumnWidth.Maximum), trackColumnWidth.Minimum);
            }
            lblColumnWidth.Text = ((trackColumnWidth.Value == 0) ? "Default" : trackColumnWidth.Value.ToString()) + " pixels";
        }
        private void trackBar_ValueChanged(object sender, EventArgs e)
        {
            TrackBar[] bars = { trackCellFontSize, trackMainRows, trackMiniRows, trackMinutes, trackColumnWidth, trackRowHeight };
            Label[] labels = { lblCellFontSize, lblMainRows, lblMiniRows, lblMinutes, lblColumnWidth, lblRowHeight };
            string[] units = { " point", " rows", " rows", " minutes", " pixels", "X Font Height" };

            int bar, step, value;
            for (bar = 0; bar < bars.Length; ++bar)
            {
                if (sender.Equals(bars[bar]))
                {
                    break;
                }
            }

            if (bar < bars.Length)
            {
                step = bars[bar].SmallChange;
                value = bars[bar].Value;

                if (value % step != 0)
                {
                    value = (int)((double)value / step + 0.5) * step;
                }
                bars[bar].Value = value;
            }

            recalculateAll();
        }
        private void cbMainShowDetails_CheckStateChanged(object sender, EventArgs e)
        {
            recalculateAll();
        }
        private void cbAutoAdjustColumnWidth_CheckStateChanged(object sender, EventArgs e)
        {
            if (cbAutoAdjustColumnWidth.Checked) trackColumnWidth.Value = calculateColumnWidth();
        }
        private int calculateColumnWidth()
        {
            int logoHeight = (trackLogoSize.Value == 0) ? smallLogoHeight : (trackLogoSize.Value == 1) ? mediumLogoHeight : largeLogoHeight;
            int cellwidth = (3 * logoHeight + 10) + ((cbHideNumber.Checked) ? 0 : (int)(cellFontPixelHeight * 2.67) + 10);
            return cellwidth;
        }
        #endregion

        #region ========== Resource Read/Writes ==========
        private bool getGuideConfigurations()
        {
            // get cell font size
            var font1 = shellDllResources[(int)SHELLRESOURCE.EPGCELLS_MCML].Descendants()
                .Where(arg => arg.Name.LocalName == "Font")
                .Where(arg => arg.Attribute("Name") != null)
                .Where(arg => arg.Attribute("Name").Value == "TitleDefaultFont")
                .Single();
            if (font1 != null)
            {
                trackCellFontSize.Value = cellFontPointSize = safeTrackBarValue(int.Parse(font1.Attribute("FontSize").Value), trackCellFontSize);
            }

            // get detail font size
            var font2 = resDllResources[(int)RESRESOURCE.GUIDEDETAILSBASE_XML].Descendants()
                .Where(arg => arg.Name.LocalName == "Font")
                .Where(arg => arg.Attribute("Name") != null)
                .Where(arg => arg.Attribute("Name").Value == "TitleFont")
                .Single();
            if (font2 != null)
            {
                detailFontPointSize = int.Parse(font2.Attribute("FontSize").Value);
            }

            // get main guide rows and mini guide rows
            var actions = shellDllResources[(int)SHELLRESOURCE.EPG_MCML].Descendants()
                .Where(arg => arg.Name.LocalName == "Set")
                .Where(arg => arg.Attribute("Target") != null)
                .Where(arg => arg.Attribute("Target").Value == "[Table.VisibleRowCapacity]")
                .Select(arg => arg.Parent);
            foreach (XElement action in actions)
            {
                var mode = action.Parent.Descendants()
                    .Where(arg => arg.Name.LocalName == "Equality")
                    .Where(arg => arg.Attribute("Source") != null)
                    .Where(arg => arg.Attribute("Source").Value == "[MiniMode.Value]")
                    .Single();
                if (mode != null)
                {
                    if (mode.Attribute("Value").Value == "false") // main guide
                    {
                        int top = 0; int bottom = 0;
                        foreach (XElement target in action.Descendants())
                        {
                            if (target.Attribute("Target") == null) continue;
                            switch (target.Attribute("Target").Value)
                            {
                                case "[Table.VisibleRowCapacity]":
                                    mainGuideRows = int.Parse(target.Attribute("Value").Value);
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
                                default:
                                    break;
                            }
                        }
                        rowHeightMultiplier = ((bottom - top) / (double)mainGuideRows) / cellFontPixelHeight + 0.005;
                        trackMainRows.Maximum = maxMainTableRows;
                        trackMainRows.Value = safeTrackBarValue(mainGuideRows, trackMainRows);
                        trackRowHeight.Value = safeTrackBarValue((int)(100 * rowHeightMultiplier - 100), trackRowHeight);
                    }
                    else // mini guide
                    {
                        foreach (XElement target in action.Descendants())
                        {
                            if (target.Attribute("Target") == null) continue;
                            switch (target.Attribute("Target").Value)
                            {
                                case "[Table.VisibleRowCapacity]":
                                    miniGuideRows = int.Parse(target.Attribute("Value").Value);
                                    trackMiniRows.Value = safeTrackBarValue(miniGuideRows, trackMiniRows);
                                    break;
                                case "[DetailsLayout.Top.Offset]":
                                    cbMiniShowDetails.Checked = (int.Parse(target.Attribute("Value").Value) < 768);
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                }
            }

            // get guide visible columns
            var columns = shellDllResources[(int)SHELLRESOURCE.EPG_MCML].Descendants()
                .Where(arg => arg.Name.LocalName == "Condition")
                .Where(arg => arg.Attribute("Target") != null)
                .Where(arg => arg.Attribute("Target").Value == "[Table.VisibleColumnCapacity]")
                .Single();
            if (columns != null)
            {
                trackMinutes.Value = columnMinutes = safeTrackBarValue(int.Parse(columns.Attribute("Value").Value), trackMinutes);
            }

            // determine channel logo size
            var channelLogo = shellDllResources[(int)SHELLRESOURCE.EPGCOMMON_MCML].Descendants()
                .Where(arg => arg.Name.LocalName == "UI")
                .Where(arg => arg.Attribute("Name") != null)
                .Where(arg => arg.Attribute("Name").Value == "ChannelLogo")
                .Single();
            if (channelLogo != null)
            {
                var size = channelLogo.Descendants()
                    .Where(arg => arg.Name.LocalName == "Size")
                    .Where(arg => arg.Attribute("Name") != null)
                    .Where(arg => arg.Attribute("Name").Value == "MaximumSize")
                    .Single();
                if (size != null)
                {
                    string[] value = size.Attribute("Size").Value.Split(',');
                    if (Math.Abs(largeLogoHeight - int.Parse(value[1])) <= 1) trackLogoSize.Value = 2;
                    else if (Math.Abs(mediumLogoHeight - int.Parse(value[1])) <= 1) trackLogoSize.Value = 1;
                    else trackLogoSize.Value = 0;
                }
            }

            // determine if channel number is hidden and if animations are disabled
            var epgChannelCell = shellDllResources[(int)SHELLRESOURCE.EPGCELLS_MCML].Descendants()
                .Where(arg => arg.Name.LocalName == "UI")
                .Where(arg => arg.Attribute("Name") != null)
                .Where(arg => arg.Attribute("Name").Value == "EpgChannelCell")
                .Single();
            if (epgChannelCell != null)
            {
                var channelNumber = epgChannelCell.Descendants()
                    .Where(arg => arg.Name.LocalName == "Text")
                    .Where(arg => arg.Attribute("Name") != null)
                    .Where(arg => arg.Attribute("Name").Value == "Number")
                    .Single();
                if (channelNumber != null)
                {
                    cbHideNumber.Checked = (channelNumber.Attribute("MaximumSize").Value == "1,0");
                }

                string expandedFont = "22";
                string defaultFont = "18";
                var fonts = epgChannelCell.Descendants()
                    .Where(arg => arg.Name.LocalName == "Font")
                    .Where(arg => arg.Attribute("Name") != null);
                foreach (XElement font in fonts)
                {
                    switch (font.Attribute("Name").Value)
                    {
                        case "NumberExpandedFont":
                            expandedFont = font.Attribute("FontSize").Value;
                            break;
                        case "NumberDefaultFont":
                            defaultFont = font.Attribute("FontSize").Value;
                            break;
                        default:
                            break;
                    }
                }
                cbRemoveAnimations.Checked = (expandedFont == defaultFont);

                // determine if logos are centered
                var panels = epgChannelCell.Descendants()
                    .Where(arg => arg.Name.LocalName == "Panel")
                    .Where(arg => arg.Attribute("Name") != null)
                    .Where(arg => arg.Attribute("Name").Value == "SmallLogoPanel")
                    .Single();
                if (panels != null)
                {
                    XElement e = panels.Descendants().Where(arg => arg.Name.LocalName == "FormLayoutInput").Single();
                    cbCenterLogo.Checked = (e.Attribute("Horizontal") != null);
                }
            }

            // determine if callsign is overridden with channel name
            var callsigns = shellDllResources[(int)SHELLRESOURCE.EPGCELLS_MCML].Descendants()
                .Where(arg => arg.Name.LocalName == "Default")
                .Where(arg => arg.Attribute("Target") != null)
                .Where(arg => arg.Attribute("Target").Value == "[Callsign.Content]");
            foreach (XElement callsign in callsigns)
            {
                cbChannelName.Checked = (callsign.Attribute("Value").Value == "[Cell.Name]");
                break;
            }

            // get channel column width
            using (RegistryKey key = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Media Center\\Settings\\ProgramGuide", false))
            {
                if (key != null)
                {
                    int width = 0;
                    if (key.GetValue("ChannelCellWidth") != null) width = (int)key.GetValue("ChannelCellWidth");
                    trackColumnWidth.Value = safeTrackBarValue(width == 0 ? 240 : width, trackColumnWidth);

                    cbAutoAdjustColumnWidth.Checked = (width == calculateColumnWidth());
                }
            }

            return true;
        }
        private void setFontSizes()
        {
            var test = shellDllResources[(int)SHELLRESOURCE.EPG_MCML].Descendants()
                .Where(arg => arg.Name.LocalName == "Size")
                .Where(arg => arg.Attribute("Name") != null)
                .Where(arg => arg.Attribute("Name").Value == "GuideShowCardImageSize")
                .Single();
            if (test != null)
            {
                test.SetAttributeValue("Size", "210,150");
            }

            // set program/movie details font sizes
            var fonts1 = resDllResources[(int)RESRESOURCE.GUIDEDETAILSBASE_XML].Descendants()
                .Where(arg => arg.Name.LocalName == "Font")
                .Where(arg => arg.Parent.Name.LocalName == "Properties")
                .Where(arg => arg.Parent.Parent.Name.LocalName == "UI")
                .Where(arg => arg.Parent.Parent.Attribute("Name").Value == "GuideDetailsBase");
            foreach (XElement font in fonts1)
            {
                if (font.Attribute("Name") == null) continue;

                int value;
                switch (font.Attribute("Name").Value)
                {
                    case "TitleFont":
                        font.SetAttributeValue("FontSize", detailFontPointSize.ToString()); // default 22
                        break;
                    case "OtherFont":
                    case "ClockFont":
                    case "LabelFont":
                    case "AlertFont":
                        value = (int)(detailFontPointSize * 0.82 + 0.5);
                        font.SetAttributeValue("FontSize", value.ToString()); // default is 18
                        break;
                    default:
                        break;
                }
            }

            // set channel details font sizes
            var fonts2 = shellDllResources[(int)SHELLRESOURCE.EPG_MCML].Descendants()
                .Where(arg => arg.Name.LocalName == "Font")
                .Where(arg => arg.Parent.Name.LocalName == "Properties")
                .Where(arg => arg.Parent.Parent.Name.LocalName == "UI")
                .Where(arg => arg.Parent.Parent.Attribute("Name").Value == "ChannelDetailsView");
            foreach (XElement font in fonts2)
            {
                if (font.Attribute("Name") == null) continue;

                int value;
                switch (font.Attribute("Name").Value)
                {
                    case "NameFont":
                        font.SetAttributeValue("FontSize", detailFontPointSize.ToString()); // default is 22
                        break;
                    case "OtherFont":
                        value = (int)(detailFontPointSize * 0.82 + 0.5);
                        font.SetAttributeValue("FontSize", value.ToString()); // default is 18
                        break;
                    default:
                        break;
                }
            }

            // set on demand details font sizes
            var fonts3 = shellDllResources[(int)SHELLRESOURCE.EPG_MCML].Descendants()
                .Where(arg => arg.Name.LocalName == "Font")
                .Where(arg => arg.Parent.Name.LocalName == "Properties")
                .Where(arg => arg.Parent.Parent.Name.LocalName == "UI")
                .Where(arg => arg.Parent.Parent.Attribute("Name").Value == "OnDemandOfferView");
            foreach (XElement font in fonts3)
            {
                if (font.Attribute("Name") == null) continue;

                int value;
                switch (font.Attribute("Name").Value)
                {
                    case "TitleFont":
                        font.SetAttributeValue("FontSize", detailFontPointSize.ToString()); // default is 22
                        break;
                    case "OtherFont":
                        value = (int)(detailFontPointSize * 0.82 + 0.5);
                        font.SetAttributeValue("FontSize", value.ToString()); // default is 18
                        break;
                    default:
                        break;
                }
            }

            // replace callsign with channel name
            if (cbChannelName.Checked)
            {
                var callsigns = shellDllResources[(int)SHELLRESOURCE.EPGCELLS_MCML].Descendants()
                    .Where(arg => arg.Name.LocalName == "Default")
                    .Where(arg => arg.Attribute("Target") != null)
                    .Where(arg => arg.Attribute("Target").Value == "[Callsign.Content]");
                foreach (XElement callsign in callsigns)
                {
                    callsign.SetAttributeValue("Value", "[Cell.Name]"); // default is [Cell.Callsign]
                }
            }

            // set table column header font sizes
            int columnHeaderFontSize = 16 - ((columnMinutes - 120) / 30);
            var fonts5 = shellDllResources[(int)SHELLRESOURCE.EPGCELLS_MCML].Descendants()
                .Where(arg => arg.Name.LocalName == "UI")
                .Where(arg => arg.Attribute("Name") != null)
                .Where(arg => arg.Attribute("Name").Value == "EpgTimeCell")
                .Single();
            var fonts6 = shellDllResources[(int)SHELLRESOURCE.EPGCOMMON_MCML].Descendants()
                .Where(arg => arg.Name.LocalName == "UI")
                .Where(arg => arg.Attribute("Name") != null)
                .Where(arg => arg.Attribute("Name").Value == "EpgDateCell")
                .Single();
            if (fonts5 != null)
            {
                var timeFont = fonts5.Descendants()
                    .Where(arg => arg.Name.LocalName == "Font")
                    .Where(arg => arg.Attribute("Name") != null)
                    .Where(arg => arg.Attribute("Name").Value == "TimeFont")
                    .Single();
                if (timeFont != null)
                {
                    timeFont.SetAttributeValue("FontSize", columnHeaderFontSize.ToString()); // default is 16
                }

                var dateFont = fonts6.Descendants()
                    .Where(arg => arg.Name.LocalName == "Font")
                    .Where(arg => arg.Attribute("Name") != null)
                    .Where(arg => arg.Attribute("Name").Value == "DateFont")
                    .Single();
                if (dateFont != null)
                {
                    dateFont.SetAttributeValue("FontSize", columnHeaderFontSize.ToString()); // default is 16
                }
            }

            // set page title font size "guide"
            var fonts4 = shellDllResources[(int)SHELLRESOURCE.EPG_MCML].Descendants()
                .Where(arg => arg.Name.LocalName == "Font")
                .Where(arg => arg.Parent.Name.LocalName == "Font")
                .Where(arg => arg.Parent.Parent.Name.LocalName == "StaticText")
                .Where(arg => arg.Parent.Parent.Attribute("Name").Value == "Title")
                .Single();
            if (fonts4 != null)
            {
                int maxFont = (int)((0.7 * mainTableTop - 18 + (16 - columnHeaderFontSize) * 1.33) / 1.33);
                int value = (maxFont >= 22) ? Math.Min(maxFont, 48) : 0;
                fonts4.SetAttributeValue("FontSize", value.ToString()); // default is 48
            }

            // set channel cell column font sizes and insets (first column)
            var fonts7 = shellDllResources[(int)SHELLRESOURCE.EPGCELLS_MCML].Descendants()
                .Where(arg => arg.Name.LocalName == "UI")
                .Where(arg => arg.Attribute("Name") != null)
                .Where(arg => arg.Attribute("Name").Value == "EpgChannelCell")
                .Single();
            if (fonts7 != null)
            {
                var fonts = fonts7.Descendants()
                    .Where(arg => arg.Name.LocalName == "Font")
                    .Where(arg => arg.Attribute("Name") != null);
                foreach (XElement font in fonts)
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
                            value = (int)(trackCellFontSize.Value * 0.82 + 0.5);
                            font.SetAttributeValue("FontSize", value.ToString()); // default is 18
                            break;
                        default:
                            break;
                    }
                }

                var insets = fonts7.Descendants()
                    .Where(arg => arg.Name.LocalName == "Inset")
                    .Where(arg => arg.Attribute("Name") != null);
                int logoHeight = (trackLogoSize.Value == 0) ? smallLogoHeight : (trackLogoSize.Value == 1) ? mediumLogoHeight : largeLogoHeight;
                foreach (XElement inset in insets)
                {
                    string value;
                    switch (inset.Attribute("Name").Value)
                    {
                        case "BackgroundDefaultPadding": // channel logo
                            value = string.Format("0,{0},{1},{0}", (cbRemoveAnimations.Checked) ? 0 : (int)((rowHeightPixel - 0.82 * logoHeight) / 2.0),
                                                                   (cbHideNumber.Checked) ? 0 : 5);
                            inset.SetAttributeValue("Inset", value); // default is "10,12,10,8"
                            break;
                        case "BackgroundExpandedPadding": // channel logo
                            value = (cbHideNumber.Checked) ? "0,0,0,0" : "0,0,5,0";
                            inset.SetAttributeValue("Inset", value); // default is "10,6,10,6"
                            break;
                        case "DefaultNumberMargins":
                            value = (cbHideNumber.Checked) ? "-1,0,0,0" : string.Format("5,{0},0,{0}", (cbRemoveAnimations.Checked) ? 0 : -(int)((rowHeightPixel - 0.82 * logoHeight) / 2.0));
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
                        default:
                            break;
                    }
                }

                var panels = fonts7.Descendants()
                    .Where(arg => arg.Name.LocalName == "Panel")
                    .Where(arg => arg.Attribute("Name") != null);
                foreach (XElement panel in panels)
                {
                    XElement e;
                    string value;
                    switch (panel.Attribute("Name").Value)
                    {
                        case "SmallLogoPanel":
                            e = panel.Descendants().Where(arg => arg.Name.LocalName == "FormLayoutInput").Single();
                            e.SetAttributeValue("Bottom", "Parent,1"); // default is "Number,1"
                            e.SetAttributeValue("Vertical", "Center"); // default is null
                            e.SetAttributeValue("Horizontal", (cbCenterLogo.Checked || cbHideNumber.Checked) ? "Center" : null); // default is null
                            e.SetAttributeValue("Left", (cbCenterLogo.Checked && !cbHideNumber.Checked) ? "Parent,1," + (3 * cellFontPixelHeight - trackColumnWidth.Value + 5).ToString() : null);

                            e = panel.Descendants().Where(arg => arg.Name.LocalName == "ChannelLogo").Single();
                            value = string.Format("{0},{1}", 3 * logoHeight, logoHeight);
                            e.SetAttributeValue("MaximumSize", value); // default is "70,40"
                            break;
                        case "CallsignPanel":
                            e = panel.Descendants().Where(arg => arg.Name.LocalName == "FormLayoutInput").Single();
                            e.SetAttributeValue("Bottom", "Parent,1"); // default is "Number,1"
                            e.SetAttributeValue("Vertical", "Center"); // default is null
                            break;
                        default:
                            break;
                    }
                }

                var ui = shellDllResources[(int)SHELLRESOURCE.EPGCOMMON_MCML].Descendants()
                    .Where(arg => arg.Name.LocalName == "UI")
                    .Where(arg => arg.Attribute("Name") != null);
                foreach (XElement e in ui)
                {
                    string value;
                    switch (e.Attribute("Name").Value)
                    {
                        case "ChannelLogo":
                            value = string.Format("{0},{1}", 3 * logoHeight, logoHeight);
                            e.Descendants().Where(arg => arg.Name.LocalName == "Size").Single()
                                .SetAttributeValue("Size", value); // default is "75,35"
                            break;
                        default:
                            break;
                    }
                }

                var text = fonts7.Descendants()
                    .Where(arg => arg.Name.LocalName == "Text")
                    .Where(arg => arg.Attribute("Name") != null)
                    .Where(arg => arg.Attribute("Name").Value == "Number")
                    .Single();
                if (text != null)
                {
                    string value = (cbHideNumber.Checked) ? "1,0" : string.Format("{0},0", cellFontPixelHeight * 3);
                    text.SetAttributeValue("MaximumSize", value); // default is "75,0"

                    var layout = text.Descendants().Where(arg => arg.Name.LocalName == "FormLayoutInput").Single();
                    if (layout != null)
                    {
                        layout.SetAttributeValue("Vertical", "Center"); // default is null
                    }
                }
            }

            // set title line text font size and insets
            var fonts8 = shellDllResources[(int)SHELLRESOURCE.EPGCELLS_MCML].Descendants()
                .Where(arg => arg.Name.LocalName == "UI")
                .Where(arg => arg.Attribute("Name") != null)
                .Where(arg => arg.Attribute("Name").Value == "EpgShowCell")
                .Single();
            if (fonts8 != null)
            {
                var fonts = fonts8.Descendants()
                    .Where(arg => arg.Name.LocalName == "Font")
                    .Where(arg => arg.Attribute("Name") != null)
                    .Where(arg => arg.Parent.Name.LocalName == "Properties");
                foreach (XElement font in fonts)
                {
                    switch (font.Attribute("Name").Value)
                    {
                        case "TitleDefaultFont":
                        case "TitleFocusFont":
                            font.SetAttributeValue("FontSize", trackCellFontSize.Value.ToString()); // default is 22
                            break;
                        default:
                            break;
                    }
                }

                double scaleFactor = cellFontPointSize / 22.0;
                var panel = fonts8.Descendants()
                    .Where(arg => arg.Name.LocalName == "Panel")
                    .Where(arg => arg.Attribute("Name") != null)
                    .Where(arg => arg.Attribute("Name").Value == "TitleLine")
                    .Single();
                if (panel != null)
                {
                    string value = string.Format("{0},0,{0},0", (int)(10 * scaleFactor + 0.5));
                    panel.SetAttributeValue("Padding", value); // default is "10,8,10,0"
                }

                var graphics = fonts8.Descendants()
                    .Where(arg => arg.Name.LocalName == "Graphic")
                    .Where(arg => arg.Attribute("Name") != null);
                foreach (XElement graphic in graphics)
                {
                    string value;
                    switch (graphic.Attribute("Name").Value)
                    {
                        case "HDImage":
                            value = string.Format("{0},0,0,0", (int)(27 * scaleFactor - 26.5));
                            graphic.SetAttributeValue("Margins", value); // default is "0,6,0,8"
                            value = string.Format("{0},{1}", (int)(27 * Math.Min(scaleFactor, 1.0) + 0.5), (int)(17 * Math.Min(scaleFactor, 1.0) + 0.5));
                            graphic.SetAttributeValue("MinimumSize", value); // default is "27,17"

                            graphic.SetAttributeValue("Scale", string.Format("{0},{0},{0}", scaleFactor)); // default is null
                            graphic.SetAttributeValue("CenterPointPercent", "1.0,0.5,0.5"); // default is null
                            break;
                        case "ContinuingPrevious":
                            value = string.Format("0,0,{0},0", (int)(9 * scaleFactor + 14 * scaleFactor - 13.5));
                            graphic.SetAttributeValue("Margins", value); // default is "0,1,9,8"
                            value = string.Format("{0},{1}", (int)(14 * Math.Min(scaleFactor, 1.0) + 0.5), (int)(19 * Math.Min(scaleFactor, 1.0) + 0.5));
                            graphic.SetAttributeValue("MinimumSize", value); // default is "14,19"

                            graphic.SetAttributeValue("Scale", string.Format("{0},{0},{0}", scaleFactor)); // default is null
                            graphic.SetAttributeValue("CenterPointPercent", "0.0,0.5,0.5"); // default is null
                            break;
                        case "ContinuingNext":
                            value = string.Format("{0},0,0,0", (int)(9 * scaleFactor + 14 * scaleFactor - 13.5));
                            graphic.SetAttributeValue("Margins", value); // default is "9,1,0,8"
                            value = string.Format("{0},{1}", (int)(14 * Math.Min(scaleFactor, 1.0) + 0.5), (int)(19 * Math.Min(scaleFactor, 1.0) + 0.5));
                            graphic.SetAttributeValue("MinimumSize", value); // default is "14,19"

                            graphic.SetAttributeValue("Scale", string.Format("{0},{0},{0}", scaleFactor)); // default is null
                            graphic.SetAttributeValue("CenterPointPercent", "1.0,0.5,0.5"); // default is null
                            break;
                        case "Record":
                            graphic.SetAttributeValue("Margins", "0,0,0,0"); // default is "0,6,0,8"

                            graphic.SetAttributeValue("Scale", string.Format("{0},{0},{0}", scaleFactor)); // default is null
                            graphic.SetAttributeValue("CenterPointPercent", "1.0,0.5,0.5"); // default is null
                            break;
                        default:
                            break;
                    }
                }

                var title = fonts8.Descendants()
                    .Where(arg => arg.Name.LocalName == "Text")
                    .Where(arg => arg.Attribute("Name") != null)
                    .Where(arg => arg.Attribute("Name").Value == "Title")
                    .Single();
                if (title != null)
                {
                    int margin = (int)((((mainTableBottom - mainTableTop) / mainGuideRows) - (cellFontPointSize * pixelsPerPoint) - 6) / 2.0) - (int)(0.08 * cellFontPointSize * pixelsPerPoint + 0.5);
                    string value = string.Format("0,{0},0,0", margin);
                    title.SetAttributeValue("Margins", value); // default is null
                }
            }
        }
        private void setTableGeometries()
        {
            // get main guide rows and mini guide rows
            var actions = shellDllResources[(int)SHELLRESOURCE.EPG_MCML].Descendants()
                .Where(arg => arg.Name.LocalName == "Set")
                .Where(arg => arg.Attribute("Target") != null)
                .Where(arg => arg.Attribute("Target").Value == "[Table.VisibleRowCapacity]")
                .Select(arg => arg.Parent);
            foreach (XElement action in actions)
            {
                var mode = action.Parent.Descendants()
                    .Where(arg => arg.Name.LocalName == "Equality")
                    .Where(arg => arg.Attribute("Source") != null)
                    .Where(arg => arg.Attribute("Source").Value == "[MiniMode.Value]")
                    .Single();
                if (mode != null)
                {
                    if (mode.Attribute("Value").Value == "false") // main guide
                    {
                        foreach (XElement target in action.Descendants())
                        {
                            if (target.Attribute("Target") == null) continue;
                            int value;
                            switch (target.Attribute("Target").Value)
                            {
                                case "[Table.VisibleRowCapacity]":
                                    target.SetAttributeValue("Value", mainGuideRows.ToString()); // default is 7
                                    break;
                                case "[FilterButtonLayout.Top.Offset]":
                                    target.SetAttributeValue("Value", mainTableTop.ToString()); // default is 118
                                    break;
                                case "[FilterButtonLayout.Left.Offset]":
                                    target.SetAttributeValue("Value", "0"); // default is 55
                                    break;
                                case "[FilterButtonLayout.Right.Offset]":
                                    target.SetAttributeValue("Value", "55"); // default is 106
                                    break;
                                case "[FilterButtonLayout.Bottom.Offset]":
                                    target.SetAttributeValue("Value", mainTableBottom.ToString()); // default is 493
                                    break;
                                case "[DetailsLayout.Top.Offset]":
                                    value = (cbMainShowDetails.Checked) ? 6 : 1000;
                                    target.SetAttributeValue("Value", value.ToString()); // defualt is 27
                                    break;
                                case "[ContentImageLayout.Top.Offset]":
                                    value = (cbMainShowDetails.Checked) ? 12 : 1000;
                                    target.SetAttributeValue("Value", value.ToString()); // default is 34
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                    else // mini guide
                    {
                        foreach (XElement target in action.Descendants())
                        {
                            if (target.Attribute("Target") == null) continue;
                            int value;
                            switch (target.Attribute("Target").Value)
                            {
                                case "[Table.VisibleRowCapacity]":
                                    target.SetAttributeValue("Value", miniGuideRows.ToString()); // default is 2
                                    break;
                                case "[FilterButtonLayout.Top.Offset]":
                                    target.SetAttributeValue("Value", miniTableTop.ToString()); // default is 476
                                    break;
                                case "[FilterButtonLayout.Left.Offset]":
                                    target.SetAttributeValue("Value", "0"); // default is 54
                                    break;
                                case "[FilterButtonLayout.Right.Offset]":
                                    target.SetAttributeValue("Value", "55"); // default is 55
                                    break;
                                case "[FilterButtonLayout.Bottom.Offset]":
                                    target.SetAttributeValue("Value", miniTableBottom.ToString()); // default is 585
                                    break;
                                case "[DetailsLayout.Top.Offset]":
                                    value = (cbMiniShowDetails.Checked) ? 6 : 1000;
                                    target.SetAttributeValue("Value", value.ToString()); // default is 25
                                    break;
                                case "[ContentImageLayout.Top.Offset]":
                                    value = (cbMiniShowDetails.Checked) ? 12 : 1000;
                                    target.SetAttributeValue("Value", value.ToString()); // default is 31
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                }
            }

            // set guide visible columns
            var columns = shellDllResources[(int)SHELLRESOURCE.EPG_MCML].Descendants()
                .Where(arg => arg.Name.LocalName == "Condition") // IsWidescreen
                .Where(arg => arg.Attribute("Target") != null)
                .Where(arg => arg.Attribute("Target").Value == "[Table.VisibleColumnCapacity]")
                .Single();
            if (columns != null)
            {
                columns.SetAttributeValue("Value", trackMinutes.Value.ToString()); // default is 120
            }
            columns = shellDllResources[(int)SHELLRESOURCE.EPG_MCML].Descendants()
                .Where(arg => arg.Name.LocalName == "Default")
                .Where(arg => arg.Attribute("Target") != null)
                .Where(arg => arg.Attribute("Target").Value == "[Table.VisibleColumnCapacity]")
                .Single();
            if (columns != null)
            {
                columns.SetAttributeValue("Value", trackMinutes.Value.ToString()); // default is 90
            }

            // set program / movie details placement
            var inputs = shellDllResources[(int)SHELLRESOURCE.EPG_MCML].Descendants()
                .Where(arg => arg.Name.LocalName == "FormLayoutInput")
                .Where(arg => arg.Attribute("Name") != null);
            if (inputs.Count() > 0)
            {
                foreach (XElement input in inputs)
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
                        default:
                            break;
                    }
                }
            }

            // set channel column width
            using (RegistryKey key = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Media Center\\Settings\\ProgramGuide", true))
            {
                if (key != null)
                {
                    key.SetValue("ChannelCellWidth", trackColumnWidth.Value, RegistryValueKind.DWord); // default is 0
                }
            }
        }
        private int safeTrackBarValue(int value, TrackBar trackbar)
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
                File.Copy(shellEhomePath, shellTempPath, true);

                // update resources of the shell dll in the temp folder
                bool updateSuccess = true;
                for (int i = 0; i < (int)SHELLRESOURCE.MAX; ++i)
                {
                    updateSuccess &= ReplaceFileResource(shellTempPath, ((SHELLRESOURCE)i).ToString().Replace("_", "."), shellDllResources[i]);
                }

                if (updateSuccess)
                {
                    // kill any processes running for WMC shell
                    foreach (Process process in Process.GetProcessesByName("ehshell"))
                    {
                        process.Kill();
                        process.WaitForExit(10000);
                    }
                    foreach (Process process in Process.GetProcessesByName("ehexthost"))
                    {
                        process.Kill();
                        process.WaitForExit(10000);
                    }

                    // move the shell dll from the temp folder to the ehome folder
                    try
                    {
                        File.Copy(shellTempPath, shellEhomePath, true);
                    }
                    catch (UnauthorizedAccessException)
                    {
                        // take ownership and try again
                        if (TakeOwnership(shellEhomePath))
                        {
                            try
                            {
                                File.Copy(shellTempPath, shellEhomePath, true);
                            }
                            catch { }
                        }
                    }
                    catch (Exception ex)
                    {
                        string msg = ex.Message + "\n";
                        msg += "\nThe following processes are preventing the shell file from being updated:";
                        foreach (Process process in FileUtil.WhoIsLocking(shellEhomePath))
                        {
                            if (process.MainModule != null)
                            {
                                msg += "\n" + process.MainModule.FileVersionInfo.FileDescription ?? string.Empty;
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
                File.Delete(shellTempPath);
            }
            catch (Exception ex)
            {
                Logger.WriteError(ex.Message);
                Logger.WriteError(ex.InnerException.Message);
                Logger.WriteError(ex.StackTrace);
            }
        }
        private void importResources()
        {
            for (int i = 0; i < shellDllResources.Length; ++i)
            {
                shellDllResources[i] = GetFileResource(shellEhomePath, ((SHELLRESOURCE)i).ToString().Replace("_", "."));
            }

            for (int i = 0; i < resDllResources.Length; ++i)
            {
                resDllResources[i] = GetFileResource(resEhomePath, ((RESRESOURCE)i).ToString().Replace("_", "."));
            }
        }
        private XDocument GetFileResource(string resourceName)
        {
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    return XDocument.Parse(reader.ReadToEnd());
                }
            }
        }
        private XDocument GetFileResource(string filePath, string resourceName, uint resType = 23)
        {
            int resourceSize = 0;
            IntPtr hModule, hResource, resourceData, memoryPointer;

            if (((hModule = LoadLibrary(filePath)) != IntPtr.Zero) &&
                ((hResource = FindResource(hModule, resourceName, resType)) != IntPtr.Zero) &&
                ((resourceData = LoadResource(hModule, hResource)) != IntPtr.Zero) &&
                ((resourceSize = SizeOfResource(hModule, hResource)) > 0) &&
                ((memoryPointer = LockResource(resourceData)) != IntPtr.Zero))
            {
                byte[] bytes = new byte[resourceSize];
                Marshal.Copy(memoryPointer, bytes, 0, bytes.Length);                
                while (FreeLibrary(hModule));
                
                // cleanup needed for MCE Reset Toolbox
                string xml = Encoding.ASCII.GetString(bytes);
                return XDocument.Parse(xml.Substring(xml.IndexOf('<')));
            }
            else
            {
                MessageBox.Show("Failed to get file resource. Error Code: " + GetLastError().ToString());
            }
            return null;
        }
        private bool ReplaceFileResource(string filePath, string resourceName, XDocument resource, int resType = 23, short resLang = 1033)
        {
            IntPtr hUpdate;
            StringBuilder lpName = new StringBuilder(resourceName);
            byte[] bytes = Encoding.UTF8.GetBytes(resource.ToString(SaveOptions.DisableFormatting).Replace(" />", "/>"));

            if (!((hUpdate = BeginUpdateResource(filePath, false)) != IntPtr.Zero) ||
                !(UpdateResource(hUpdate, resType, lpName, resLang, bytes, bytes.Length) == 1) ||
                !EndUpdateResource(hUpdate, false))
            {
                MessageBox.Show("Failed to update resource. Error Code: " + GetLastError().ToString());
                return false;
            }
            return true;
        }
        private bool TakeOwnership(string filePath)
        {
            Process procTakeown, procIcacls;
            string error;

            try
            {
                // user will be prompted to allow takeown to execute
                // will throw if user denies
                procTakeown = Process.Start(new ProcessStartInfo
                {
                    FileName = "takeown.exe",
                    Arguments = "/f \"" + filePath + "\"",
                    UseShellExecute = true,
                    CreateNoWindow = true,
                    Verb = "runas"
                });
                procTakeown.WaitForExit();
            }
            catch
            {
                return false;
            }

            // give users modify rights to file
            procIcacls = Process.Start(new ProcessStartInfo
            {
                FileName = "icacls.exe",
                Arguments = "\"" + filePath + "\" /grant *S-1-5-32-545:(M)",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardError = true,
                Verb = "runas"
            });
            error = procIcacls.StandardError.ReadToEnd();
            procIcacls.WaitForExit();

            if ((procIcacls.ExitCode == 0) || (error == null) || (error.Trim() == string.Empty))
            {
                return true;
            }
            return false;
        }
        #endregion

        #region ========== Buttons ==========
        private void btnUpdateGuideConfigurations_Click(object sender, EventArgs e)
        {
            // wait cursor
            this.Cursor = Cursors.WaitCursor;
            formInitialized = false;

            // populate xdocuments with default files
            for (int i = 0; i < (int)SHELLRESOURCE.MAX; ++i)
            {
                shellDllResources[i] = GetFileResource("epg123Client." + ((SHELLRESOURCE)i).ToString().Replace("_", "."));
            }

            // update xdocuments
            if (!sender.Equals(btnResetToDefault))
            {
                setFontSizes();
                setTableGeometries();

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
            setColumnWidthRegistry();

            // download the current resource files
            formInitialized = getGuideConfigurations();

            // make sure all updates are reflected
            recalculateAll();

            // restore arrow cursor
            this.Cursor = Cursors.Arrow;
        }
        private void btnRemoveLogos_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;

            ProcessStartInfo startInfo = new ProcessStartInfo()
            {
                FileName = "epg123Client.exe",
                Arguments = "-nologo"
            };
            Process proc = Process.Start(startInfo);
            proc.WaitForExit();

            this.Cursor = Cursors.Arrow;
        }
        private void btnUpdateFilePattern(object sender, EventArgs e)
        {
            using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Media Center\Service\Recording", true))
            {
                if (key != null)
                {
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
                        readRegistries();
                    }
                    catch { }
                }
            }
        }

        string[] countries = { /*"default", */"au", "be", "br", "ca", "ch", "cn", "cz", "de", "dk", "es", "fi", "fr", "gb", "hk", "hu", "ie", "in",/* "it",*/ "jp", "kr", "mx", "nl", "no", "nz", "pl",/* "pt",*/ "ru", "se", "sg", "sk",/* "tr", "tw",*/ "us", "za" };
        private void btnTunerLimit_Click(object sender, EventArgs e)
        {
            // activate the wait cursor for the tweak form
            this.UseWaitCursor = true;

            // create the MXF file to import
            string xml = "<?xml version=\"1.0\" standalone=\"yes\"?>\r\n" +
                         "<MXF version=\"1.0\" xmlns=\"\">\r\n" +
                         "  <Assembly name=\"mcstore\">\r\n" +
                         "    <NameSpace name=\"Microsoft.MediaCenter.Store\">\r\n" +
                         "      <Type name=\"StoredType\" />\r\n" +
                         "    </NameSpace>\r\n" +
                         "  </Assembly>\r\n" +
                         "  <Assembly name=\"ehshell\">\r\n" +
                         "    <NameSpace name=\"ServiceBus.UIFramework\">\r\n" +
                         "      <Type name=\"TvSignalSetupParams\" />\r\n" +
                         "    </NameSpace>\r\n" +
                         "  </Assembly>\r\n";
            xml += string.Format("  <With maxRecordersForHomePremium=\"{0}\" maxRecordersForUltimate=\"{0}\" maxRecordersForRacing=\"{0}\" maxRecordersForBusiness=\"{0}\" maxRecordersForEnterprise=\"{0}\" maxRecordersForOthers=\"{0}\">\r\n", TUNERLIMIT);

            foreach (string country in countries)
            {
                if (country.Equals("ca"))
                {
                    // sneak this one in for our Canadian friends just north of the (contiguous) border to be able to tune ATSC stations from the USA
                    xml += string.Format("    <TvSignalSetupParams uid=\"tvss-{0}\" atscSupported=\"true\" autoSetupLikelyAtscChannels=\"34, 35, 36, 43, 31, 39, 38, 32, 41, 27, 19, 51, 44, 42, 30, 28\" tvRatingSystem=\"US\" />\r\n", country);
                }
                else
                {
                    xml += string.Format("    <TvSignalSetupParams uid=\"tvss-{0}\" />\r\n", country);
                }
            }

            xml += "  </With>\r\n";
            xml += "</MXF>";

            // create temporary file
            string mxfFilepath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.mxf");
            using (StreamWriter writer = new StreamWriter(mxfFilepath, false))
            {
                writer.Write(xml);
            }

            // import tweak using loadmxf.exe because for some reason the MxfImporter doesn't work for this
            ProcessStartInfo startInfo = new ProcessStartInfo()
            {
                FileName = Environment.ExpandEnvironmentVariables(@"%SystemRoot%\ehome\loadmxf.exe"),
                Arguments = string.Format("-i \"{0}\"", mxfFilepath),
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };
            using (Process process = Process.Start(startInfo))
            {
                process.StandardOutput.ReadToEnd();
                process.WaitForExit(30000);
                if (process.ExitCode == 0)
                {
                    MessageBox.Show("The tuner limit increase has been successfully applied.", "Tuner Limit Tweak", MessageBoxButtons.OK);
                }
                else
                {
                    MessageBox.Show("The tuner limit increase tweak has failed.", "Tuner Limit Tweak", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            // delete temporary file
            File.Delete(mxfFilepath);

            // restore cursor for tweak form
            this.UseWaitCursor = false;

            //// check tweak status
            //if (lblTunerLimit.Enabled = btnTunerLimit.Enabled = !isTunerCountTweaked())
            //{
            //    MessageBox.Show("The tuner limit increase tweak failed to get applied.", "Unknown Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            //}
        }

        private bool isTunerCountTweaked()
        {
            // return false always to enable the button
            // it appears that if some UIDs are viewed, epg123 decides
            // to grab Microsoft.MediaCenter.Shell.dll and lock itself
            // out of applying any updates
            return false;

            int success = 0;
            using (UIds uids = new UIds(Store.objectStore))
            {
                foreach (UId uid in uids.Where(arg => arg.IdValue.StartsWith("vss-")))
                {
                    foreach (string country in countries)
                    {
                        if (uid.IdValue.Equals("vss-" + country))
                        {
                            Type t = uid.Target.GetType();
                            PropertyInfo[] properties = t.GetProperties();
                            foreach (var property in properties.Where(arg => arg.Name.StartsWith("MaxRecordersFor")))
                            {
                                var q = property.GetValue(uid.Target, null);
                                if ((int)q != TUNERLIMIT) return false;
                            }
                            ++success;
                            continue;
                        }
                    }
                    if (success == countries.Length) return true;
                }
            }
            return false;
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
        private void readRegistries()
        {
            using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Media Center\Service\Recording", false))
            {
                if (key != null)
                {
                    try
                    {
                        txtNamePattern.Text = key.GetValue("filenaming", "%T_%Cs_%Dt").ToString();
                    }
                    catch { }
                }
            }

            using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Media Center\Service\Video\Tuners\DVR", false))
            {
                if (key != null)
                {
                    try
                    {
                        int files = (int)key.GetValue("BackingStoreMaxNumBackingFiles", 8);
                        int seconds = (int)key.GetValue("BackingStoreEachFileDurationSeconds", 300);
                        numBuffer.Value = files * seconds / 60;
                    }
                    catch { }
                }
            }

            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Media Center\Settings\VideoSettings", false))
            {
                if (key != null)
                {
                    try
                    {
                        numSkipAhead.Value = (int)key.GetValue("SkipAheadInterval", 29000) / 1000;
                        numInstantReplay.Value = (int)key.GetValue("InstantReplayInterval", 7000) / 1000;
                    }
                    catch { }
                }
            }

            using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Media Center\Settings\MCE.GlobalSettings", false))
            {
                if (key != null)
                {
                    try
                    {
                        lblMovieGuide.Enabled = btnMovieGuide.Enabled = ((string)key.GetValue("SystemGeoISO2") != "US") &&
                                                                        ((string)key.GetValue("SystemGeoISO2") != "CA") &&
                                                                        ((string)key.GetValue("SystemGeoISO2") != "GB");
                    }
                    catch { }
                }
            }
        }
        private void updateRegistryValues(object sender, EventArgs e)
        {
            if (sender.Equals(numBuffer))
            {
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Media Center\Service\Video\Tuners\DVR", true))
                {
                    if (key != null)
                    {
                        try
                        {
                            int files = (int)key.GetValue("BackingStoreMaxNumBackingFiles", 8);
                            key.SetValue("BackingStoreEachFileDurationSeconds", (int)(((double)numBuffer.Value * 60.0) / (double)files + 0.5), RegistryValueKind.DWord);
                        }
                        catch { }
                    }
                }
            }
            else if (sender.Equals(numSkipAhead))
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Media Center\Settings\VideoSettings", true))
                {
                    if (key != null)
                    {
                        try
                        {
                            key.SetValue("SkipAheadInterval", numSkipAhead.Value * 1000, RegistryValueKind.DWord);
                        }
                        catch { }
                    }
                }
            }
            else if (sender.Equals(numInstantReplay))
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Media Center\Settings\VideoSettings", true))
                {
                    if (key != null)
                    {
                        try
                        {
                            key.SetValue("InstantReplayInterval", numInstantReplay.Value * 1000, RegistryValueKind.DWord);
                        }
                        catch { }
                    }
                }
            }
            else if (sender.Equals(btnMovieGuide))
            {
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Media Center\Settings\MCE.GlobalSettings", true))
                {
                    if (key != null)
                    {
                        try
                        {
                            key.SetValue("SystemGeoISO2", "US", RegistryValueKind.String);
                        }
                        catch { }
                    }
                }
            }
            readRegistries();
        }
        private void setColumnWidthRegistry()
        {
            using (RegistryKey key = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Media Center\\Settings\\ProgramGuide", true))
            {
                if (key != null)
                {
                    key.SetValue("ChannelCellWidth", trackColumnWidth.Value, RegistryValueKind.DWord);
                }
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

        private void rdoCheckedChanged(object sender, EventArgs e)
        {
            RadioButton btn = sender as RadioButton;
            if (!btn.Checked) return;

            using (RegistryKey key = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Media Center\\Start Menu", true))
            {
                try
                {
                    string imagePath = "file://" + Helper.Epg123StatusLogoPath;
                    if ((btn.Tag as string).Equals("None"))
                    {
                        imagePath = string.Empty;
                    }
                    key.SetValue("OEMLogoAccent", $"{btn.Tag as string}" + (cbNoSuccess.Checked ? "_ns" : string.Empty), RegistryValueKind.String);
                    key.SetValue("OEMLogoUri", imagePath);
                }
                catch { }
            }

            setStatusLogoImage();
        }

        private void cbNoSuccess_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox checkBox = (CheckBox)sender;
            using (RegistryKey key = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Media Center\\Start Menu", true))
            {
                try
                {
                    string val = string.Empty;
                    if (rdoLight.Checked)
                    {
                        val = "Light";
                    }
                    else if (rdoDark.Checked)
                    {
                        val = "Dark";
                    }

                    if (!string.IsNullOrEmpty(val))
                    {
                        key.SetValue("OEMLogoAccent", val + (checkBox.Checked ? "_ns" : string.Empty), RegistryValueKind.String);
                        if (!checkBox.Checked)
                        {
                            key.SetValue("OEMLogoUri", "file://" + Helper.Epg123StatusLogoPath);
                        }
                    }
                }
                catch { }
            }
        }

        private void trkOpacityChanged(object sender, EventArgs e)
        {
            using (RegistryKey key = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Media Center\\Start Menu", true))
            {
                try
                {
                    key.SetValue("OEMLogoOpacity", trackBar1.Value, RegistryValueKind.DWord);
                    lblStatusLogoOpaque.Text = string.Format("{0}% Opaque", trackBar1.Value);
                }
                catch { }
            }

            if (pbStatusLogo.Image != null)
            {
                setStatusLogoImage();
            }
        }

        private void setStatusLogoImage()
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
            statusLogo.adjustImageOpacity(bmp, (double)(trackBar1.Value / 100.0));
            pbStatusLogo.Image = bmp;
        }
    }
}