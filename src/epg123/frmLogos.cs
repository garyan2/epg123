using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;
using epg123.SchedulesDirectAPI;

namespace epg123
{
    public partial class frmLogos : Form
    {
        private string Callsign = string.Empty;
        private PictureBox selectedBox = null;
        private SdLineupStation Station;

        public frmLogos(SdLineupStation station)
        {
            InitializeComponent();

            Callsign = station.Callsign;
            Station = station;

            label7.Text = $"{station.Callsign}\n{station.Name}\n{station.Affiliate}";
            openFileDialog1.InitialDirectory = $"{Helper.Epg123LogosFolder}";
        }

        private void frmLogos_Load(object sender, EventArgs e)
        {
            pbDarkLocal.AllowDrop = true;
            pbWhiteLocal.AllowDrop = true;
            pbLightLocal.AllowDrop = true;
            pbGrayLocal.AllowDrop = true;
            pbDefaultLocal.AllowDrop = true;
            pbCustomLocal.AllowDrop = true;

            Location = Owner.Location;
            Left += (Owner.ClientSize.Width - Width) / 2;
            Top += (Owner.ClientSize.Height - Height) / 4;
        }

        private void frmLogos_Shown(object sender, EventArgs e)
        {
            Refresh();
            LoadLocalImages();
            LoadRemoteImages(Station);
        }

        private void LoadLocalImages()
        {
            if (File.Exists($"{Helper.Epg123LogosFolder}\\{Callsign}_c.png") && pbCustomLocal.Image == null)
            {
                pbCustomLocal.BackColor = Color.FromArgb(255, 6, 15, 30);
                pbCustomLocal.Image = Image.FromFile($"{Helper.Epg123LogosFolder}\\{Callsign}_c.png");
            }

            if (File.Exists($"{Helper.Epg123LogosFolder}\\{Callsign}_d.png") && pbDarkLocal.Image == null)
            {
                pbDarkLocal.BackColor = Color.FromArgb(255, 6, 15, 30); ;
                pbDarkLocal.Image = Image.FromFile($"{Helper.Epg123LogosFolder}\\{Callsign}_d.png");
            }

            if (File.Exists($"{Helper.Epg123LogosFolder}\\{Callsign}_w.png") && pbWhiteLocal.Image == null)
            {
                pbWhiteLocal.BackColor = Color.FromArgb(255, 6, 15, 30); ;
                pbWhiteLocal.Image = Image.FromFile($"{Helper.Epg123LogosFolder}\\{Callsign}_w.png");
            }

            if (File.Exists($"{Helper.Epg123LogosFolder}\\{Callsign}_l.png") && pbLightLocal.Image == null)
            {
                pbLightLocal.BackColor = Color.White;
                pbLightLocal.Image = Image.FromFile($"{Helper.Epg123LogosFolder}\\{Callsign}_l.png");
            }

            if (File.Exists($"{Helper.Epg123LogosFolder}\\{Callsign}_g.png") && pbGrayLocal.Image == null)
            {
                pbGrayLocal.BackColor = Color.White;
                pbGrayLocal.Image = Image.FromFile($"{Helper.Epg123LogosFolder}\\{Callsign}_g.png");
            }

            if (File.Exists($"{Helper.Epg123LogosFolder}\\{Callsign}.png") && pbDefaultLocal.Image == null)
            {
                pbDefaultLocal.BackColor = Color.FromArgb(255, 6, 15, 30);
                pbDefaultLocal.Image = Image.FromFile($"{Helper.Epg123LogosFolder}\\{Callsign}.png");
            }
        }

        private void LoadRemoteImages(SdLineupStation station)
        {
            if (station.Logo != null)
            {
                pbDefaultRemote.BackColor = Color.FromArgb(255, 6, 15, 30); ;
                pbDefaultRemote.Load(station.Logo.Url);
            }

            if (station.StationLogos == null) return;
            foreach (var image in station.StationLogos)
            {
                switch (image.Category)
                {
                    case "dark":
                        pbDarkRemote.BackColor = Color.FromArgb(255, 6, 15, 30); ;
                        pbDarkRemote.Load(image.Url);
                        break;
                    case "white":
                        pbWhiteRemote.BackColor = Color.FromArgb(255, 6, 15, 30); ;
                        pbWhiteRemote.Load(image.Url);
                        break;
                    case "light":
                        pbLightRemote.BackColor = Color.White;
                        pbLightRemote.Load(image.Url);
                        break;
                    case "gray":
                        pbGrayRemote.BackColor = Color.White;
                        pbGrayRemote.Load(image.Url);
                        break;
                }
            }
        }

        private void menuDeleteLocal_Click(object sender, EventArgs e)
        {
            var path = $"{Helper.Epg123LogosFolder}\\{Callsign}";
            if (selectedBox == pbCustomLocal) path += "_c.png";
            if (selectedBox == pbDarkLocal) path += "_d.png";
            if (selectedBox == pbWhiteLocal) path += "_w.png";
            if (selectedBox == pbLightLocal) path += "_l.png";
            if (selectedBox == pbGrayLocal) path += "_g.png";
            if (selectedBox == pbDefaultLocal) path += ".png";

            selectedBox.Image?.Dispose();
            selectedBox.Image = null;
            selectedBox.Update();
            selectedBox.BackColor = SystemColors.Control;

            try
            {
                File.Delete(path);
                return;
            }
            catch
            {
                // do nothing
            }

            LoadLocalImages();
        }

        private static Bitmap CropAndResizeImage(Bitmap origImg)
        {
            if (origImg == null) return null;

            // set target image size
            const int tgtWidth = 360;
            const int tgtHeight = 270;

            // set target aspect/image size
            const double tgtAspect = 3.0;

            // Find the min/max non-transparent pixels
            var min = new Point(int.MaxValue, int.MaxValue);
            var max = new Point(int.MinValue, int.MinValue);

            for (var x = 0; x < origImg.Width; ++x)
            {
                for (var y = 0; y < origImg.Height; ++y)
                {
                    var pixelColor = origImg.GetPixel(x, y);
                    if (pixelColor.A <= 0) continue;
                    if (x < min.X) min.X = x;
                    if (y < min.Y) min.Y = y;

                    if (x > max.X) max.X = x;
                    if (y > max.Y) max.Y = y;
                }
            }

            // Create a new bitmap from the crop rectangle and increase canvas size if necessary
            var offsetY = 0;
            var cropRectangle = new Rectangle(min.X, min.Y, max.X - min.X + 1, max.Y - min.Y + 1);
            if ((max.X - min.X + 1) / tgtAspect > (max.Y - min.Y + 1))
            {
                offsetY = (int)((max.X - min.X + 1) / tgtAspect - (max.Y - min.Y + 1) + 0.5) / 2;
            }

            var cropImg = new Bitmap(cropRectangle.Width, cropRectangle.Height + offsetY * 2);
            cropImg.SetResolution(origImg.HorizontalResolution, origImg.VerticalResolution);
            using (var g = Graphics.FromImage(cropImg))
            {
                g.DrawImage(origImg, 0, offsetY, cropRectangle, GraphicsUnit.Pixel);
            }

            // resize image if needed
            if (tgtHeight >= cropImg.Height && tgtWidth >= cropImg.Width) return cropImg;
            
            var scale = Math.Min((double) tgtWidth / cropImg.Width, (double) tgtHeight / cropImg.Height);
            var destWidth = (int) (cropImg.Width * scale);
            var destHeight = (int) (cropImg.Height * scale);
            return new Bitmap(cropImg, new Size(destWidth, destHeight));
        }

        private void contextMenuStrip2_Opening(object sender, CancelEventArgs e)
        {
            selectedBox = pbCustomLocal;
        }

        private void picDragSource_MouseDown(object sender, MouseEventArgs e)
        {
            // Start the drag if it's the right mouse button.
            if (e.Button != MouseButtons.Left) return;
            var img = ((PictureBox)sender).Image;
            if (img == null) return;
            DoDragDrop(img, DragDropEffects.Copy);
        }

        // Allow a copy of an image.
        private void pictureBox_DragEnter(object sender, DragEventArgs e)
        {
            // See if this is a copy and the data includes an image.
            if (e.Data.GetDataPresent(DataFormats.Bitmap) &&
                (e.AllowedEffect & DragDropEffects.Copy) != 0)
            {
                // Allow this.
                e.Effect = DragDropEffects.Copy;
            }
            else if (e.Data.GetDataPresent(DataFormats.FileDrop) &&
                     (e.AllowedEffect & DragDropEffects.Copy) != 0)
            {
                e.Effect = DragDropEffects.Copy;
            }
            else
            {
                // Don't allow any other drop.
                e.Effect = DragDropEffects.None;
            }
        }

        // Accept the drop.
        private void pictureBox_DragDrop(object sender, DragEventArgs e)
        {
            Bitmap imgBitmap = null;
            if (e.Data.GetDataPresent(DataFormats.Bitmap))
            {
                imgBitmap = (Bitmap)e.Data.GetData(DataFormats.Bitmap, true);
            }
            else if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                imgBitmap = Image.FromFile(((string[]) e.Data.GetData(DataFormats.FileDrop))[0]) as Bitmap;
            }
            if (imgBitmap == null) return;

            var target = (PictureBox) sender;
            var path = $"{Helper.Epg123LogosFolder}\\{Callsign}";
            if (target == pbCustomLocal) path += "_c.png";
            if (target == pbDarkLocal) path += "_d.png";
            if (target == pbWhiteLocal) path += "_w.png";
            if (target == pbLightLocal) path += "_l.png";
            if (target == pbGrayLocal) path += "_g.png";
            if (target == pbDefaultLocal) path += ".png";

            if (target.Image != null)
            {
                target.Image.Dispose();
                target.Image = null;
                target.Refresh();
            }

            var image = CropAndResizeImage(imgBitmap);
            image.Save(path, ImageFormat.Png);

            LoadLocalImages();
        }
    }
}