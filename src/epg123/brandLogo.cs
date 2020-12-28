using System;
using System.Drawing;

namespace epg123
{
    public static class BrandLogo
    {
        public enum EPG123STATUS
        {
            SUCCESS = 0,
            UPDATEAVAIL = 1,
            WARNING = 0xBAD1,
            ERROR = 0xDEAD
        }

        public static bool UpdateAvailable { get; set; }
        public static Image StatusImage(string accent)
        {
            // determine overall status
            var status = (EPG123STATUS)Logger.EventId;

            // select base image based on status code
            if (string.IsNullOrEmpty(accent)) accent = "none";

            // set up the base image; default for brandlogo is light
            Bitmap baseImage;
            switch (accent.ToLower())
            {
                case "light":
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
                case "dark":
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
                    return new Bitmap(64, 40);
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

            // this is scaled to be aspect ratio 64x40 and cutting off the antenna (19 px)
            var height = 75 - 19;
            var image = new Bitmap(height * 64 / 40, height);

            // create new image with base image and date text
            image.SetResolution(baseImage.HorizontalResolution, baseImage.VerticalResolution);
            using (var g = Graphics.FromImage(image))
            {
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

                var updateImageWidth = (updateImage.Width == 1) ? 0 : updateImage.Width + 2;

                g.DrawImage(updateImage, image.Width - updateImageWidth + 2, 0);
                g.DrawImage(baseImage, image.Width - updateImageWidth - baseImage.Width, image.Height - 75);

                using (Brush textbrush = new SolidBrush(textColor))
                {
                    g.DrawString(text, font, textbrush, image.Width - updateImageWidth - textSize.Width + 3, image.Height - textSize.Height + 2);
                }
            }
            return image;
        }
    }
}