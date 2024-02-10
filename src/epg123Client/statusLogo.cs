using GaRyan2.Utilities;
using Microsoft.Win32;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Xml;
using System.Xml.Linq;

namespace epg123Client
{
    public static class statusLogo
    {
        public enum EPG123STATUS
        {
            SUCCESS = 0,
            UPDATEAVAIL = 1,
            WARNING = 0xBAD1,
            ERROR = 0xDEAD,
            OTHERMXF = 0xFFFF
        }

        private static string _mxfFile { get; set; }
        public static bool UpdateAvailable { get; set; }

        private static EPG123STATUS MxfFileStatus
        {
            get
            {
                if (_mxfFile == null) return EPG123STATUS.SUCCESS;

                // if mxf file doesn't exist, automatically an error
                if (_mxfFile.StartsWith("http"))
                {
                    if (_mxfFile.Contains("epg123.mxf")) _mxfFile = Helper.Epg123MxfPath;
                    else if (_mxfFile.Contains("hdhr2mxf.mxf")) _mxfFile = Helper.Hdhr2MxfMxfPath;
                }
                if (!File.Exists(_mxfFile))
                {
                    return EPG123STATUS.ERROR;
                }

                // look at the status of the mxf file generated for warnings
                XDocument providers = null;
                using (var reader = XmlReader.Create(_mxfFile))
                {
                    reader.MoveToContent();
                    while (reader.Read())
                    {
                        if (reader.Name != "Providers") continue;
                        providers = XDocument.Load(reader.ReadSubtree());
                    }
                }

                var provider = providers?.Descendants()
                    .Where(arg => arg.Name.LocalName == "Provider")
                    .Where(arg => arg.Attribute("name") != null)
                    .SingleOrDefault(arg => arg.Attribute("name")?.Value == "EPG123" || arg.Attribute("name")?.Value == "HDHR2MXF");
                if (provider == null) return EPG123STATUS.OTHERMXF;

                // determine the update available flag
                if (provider.Attribute("displayName") != null)
                {
                    UpdateAvailable = provider.Attribute("displayName").Value.Contains("Available");
                }

                // read the epg123 status
                if (provider.Attribute("status") == null) return EPG123STATUS.SUCCESS;
                var ret = (EPG123STATUS)(int.Parse(provider.Attribute("status")?.Value));
                return ret;
            }
        }

        public static void StatusImage(string mxfFile)
        {
            _mxfFile = mxfFile;
            // don't update the status logo if the imported mxf was not from epg123
            var filestatus = (int)MxfFileStatus;
            if ((EPG123STATUS)filestatus == EPG123STATUS.OTHERMXF) return;

            // determine overall status
            var status = (EPG123STATUS)(Math.Max(Logger.Status, filestatus));

            // enter datetime and status of this update run
            try
            {
                using (var key = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Media Center\\Service\\Epg", true))
                {
                    key?.SetValue("epg123LastUpdateTime", DateTime.Now.ToString("s"), RegistryValueKind.String);
                    key?.SetValue("epg123LastUpdateStatus", (int)status, RegistryValueKind.DWord);
                }
            }
            catch
            {
                Logger.WriteInformation("Failed to set registry entries for time and status of update.");
            }

            // select base image based on status code
            var opacity = 1.0;
            var accent = "Light";
            try
            {
                using (var key = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Media Center\\Start Menu", false))
                {
                    if (key != null)
                    {
                        accent = (string)key.GetValue("OEMLogoAccent", "Light");
                        opacity = (int)key.GetValue("OEMLogoOpacity", 100) / 100.0;
                    }
                    else
                    {
                        Logger.WriteInformation("Could not read registry settings for OEMLogo. Using default");
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.WriteInformation($"Could not read registry settings for OEMLogo. Exception:{Helper.ReportExceptionMessages(ex)}");
            }

            // set up the base image; default for brandlogo is light
            Bitmap baseImage;
            switch (accent)
            {
                case "Light":
                case "Light_ns":
                    switch (status)
                    {
                        case EPG123STATUS.SUCCESS:
                        case EPG123STATUS.UPDATEAVAIL:
                            baseImage = resImages.EPG123OKLight;
                            break;
                        case EPG123STATUS.WARNING:
                            baseImage = resImages.EPG123WarningLight;
                            break;
                        default:
                            baseImage = resImages.EPG123ErrorLight;
                            break;
                    }
                    break;
                case "Dark":
                case "Dark_ns":
                    switch (status)
                    {
                        case EPG123STATUS.SUCCESS:
                        case EPG123STATUS.UPDATEAVAIL:
                            baseImage = resImages.EPG123OKDark;
                            break;
                        case EPG123STATUS.WARNING:
                            baseImage = resImages.EPG123WarningDark;
                            break;
                        default:
                            baseImage = resImages.EPG123ErrorDark;
                            break;
                    }
                    break;
                default:
                    return;
            }

            // make text color match logo color
            var textColor = baseImage.GetPixel(45, 55);

            // prep for update symbol
            var updateImage = new Bitmap(1, 1);
            if (UpdateAvailable)
            {
                updateImage = resImages.updateAvailable;
            }

            // determine width of date text to add to bottom of image
            SizeF textSize;
            var text = $"{DateTime.Now:d}";
            var font = new Font(FontFamily.GenericSansSerif, 10, FontStyle.Bold, GraphicsUnit.Point);
            using (Image img = new Bitmap(1, 1))
            {
                using (var g = Graphics.FromImage(img))
                {
                    textSize = g.MeasureString(text, font);

                    // adjust for screen dpi
                    var scaleFactor = (g.DpiX / 96f);
                    if (Math.Abs(scaleFactor - 1.0) > 0.01)
                    {
                        textSize.Width /= scaleFactor;
                        textSize.Height /= scaleFactor;
                    }
                }
            }

            // create new image with base image and date text
            var image = new Bitmap(200, 75);
            image.SetResolution(baseImage.HorizontalResolution, baseImage.VerticalResolution);
            using (var g = Graphics.FromImage(image))
            {
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

                var updateImageWidth = (updateImage.Width == 1) ? 0 : updateImage.Width;
                var centerPoint = image.Width - updateImageWidth - Math.Max(baseImage.Width, textSize.Width) / 2 + 1;

                g.DrawImage(updateImage, image.Width - updateImageWidth, 0);
                g.DrawImage(baseImage, centerPoint - baseImage.Width / 2, 0);

                using (Brush textbrush = new SolidBrush(textColor))
                {
                    g.DrawString(text, font, textbrush, centerPoint - textSize.Width / 2, baseImage.Height);
                }
            }

            // adjust alpha channel as needed
            if (opacity < 1.0)
            {
                AdjustImageOpacity(image, opacity);
            }

            // save status image
            image.Save(Helper.Epg123StatusLogoPath);

            // ensure OEMLogoUri is pointed to the file
            try
            {
                using (var key = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Media Center\\Start Menu", true))
                {
                    // if selected, do not display SUCCESS logo
                    if (key == null) return;
                    if (accent.Contains("_ns") && status == EPG123STATUS.SUCCESS)
                    {
                        key.SetValue("OEMLogoUri", string.Empty);
                    }
                    else
                    {
                        key.SetValue("OEMLogoUri", "file://" + Helper.Epg123StatusLogoPath);
                    }
                }
            }
            catch
            {
                Logger.WriteInformation("Could not set OEMLogoUri in registry.");
            }
        }

        public static void AdjustImageOpacity(Bitmap image, double opacity)
        {
            if (image == null) return;

            try
            {
                // lock bitmap and return bitmap data with 32-bits per pixel color (ARGB)
                var bitmapData = image.LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);

                // create byte array to copy pixel values
                const int step = 32 / 8;
                var pixels = new byte[image.Width * image.Height * step];
                var ptr = bitmapData.Scan0;

                // copy data from pointer to array
                Marshal.Copy(ptr, pixels, 0, pixels.Length);

                // adjust alpha channel
                for (var counter = step - 1; counter < pixels.Length; counter += step)
                {
                    pixels[counter] = (byte)(pixels[counter] * opacity);
                }

                // copy array back into bitmap
                Marshal.Copy(pixels, 0, ptr, pixels.Length);

                // unlock bitmap
                image.UnlockBits(bitmapData);
            }
            catch
            {
                // ignored
            }
        }
    }
}