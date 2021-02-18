using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Windows.Forms;
using epg123.SchedulesDirectAPI;
using Microsoft.VisualBasic.FileIO;

namespace epg123
{
    public partial class frmLogos : Form
    {
        private readonly string _callsign = string.Empty;
        private readonly SdLineupStation _station;
        private PictureBox _selectedBox = null;

        public frmLogos(SdLineupStation station)
        {
            InitializeComponent();

            _callsign = station.Callsign;
            _station = station;

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
            LoadRemoteImages(_station);
        }

        private void DeleteToRecycle(string file)
        {
            FileSystem.DeleteFile(file, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
        }

        private void LoadLocalImages()
        {
            if (File.Exists($"{Helper.Epg123LogosFolder}\\{_callsign}_c.png") && pbCustomLocal.Image == null)
            {
                pbCustomLocal.BackColor = Color.FromArgb(255, 6, 15, 30);
                pbCustomLocal.Image = Image.FromFile($"{Helper.Epg123LogosFolder}\\{_callsign}_c.png");
            }

            if (File.Exists($"{Helper.Epg123LogosFolder}\\{_callsign}_d.png") && pbDarkLocal.Image == null)
            {
                pbDarkLocal.BackColor = Color.FromArgb(255, 6, 15, 30); ;
                pbDarkLocal.Image = Image.FromFile($"{Helper.Epg123LogosFolder}\\{_callsign}_d.png");
            }

            if (File.Exists($"{Helper.Epg123LogosFolder}\\{_callsign}_w.png") && pbWhiteLocal.Image == null)
            {
                pbWhiteLocal.BackColor = Color.FromArgb(255, 6, 15, 30); ;
                pbWhiteLocal.Image = Image.FromFile($"{Helper.Epg123LogosFolder}\\{_callsign}_w.png");
            }

            if (File.Exists($"{Helper.Epg123LogosFolder}\\{_callsign}_l.png") && pbLightLocal.Image == null)
            {
                pbLightLocal.BackColor = Color.White;
                pbLightLocal.Image = Image.FromFile($"{Helper.Epg123LogosFolder}\\{_callsign}_l.png");
            }

            if (File.Exists($"{Helper.Epg123LogosFolder}\\{_callsign}_g.png") && pbGrayLocal.Image == null)
            {
                pbGrayLocal.BackColor = Color.White;
                pbGrayLocal.Image = Image.FromFile($"{Helper.Epg123LogosFolder}\\{_callsign}_g.png");
            }

            if (File.Exists($"{Helper.Epg123LogosFolder}\\{_callsign}.png") && pbDefaultLocal.Image == null)
            {
                pbDefaultLocal.BackColor = Color.FromArgb(255, 6, 15, 30);
                pbDefaultLocal.Image = Image.FromFile($"{Helper.Epg123LogosFolder}\\{_callsign}.png");
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
            var path = $"{Helper.Epg123LogosFolder}\\{_callsign}";
            if (_selectedBox == pbCustomLocal) path += "_c.png";
            if (_selectedBox == pbDarkLocal) path += "_d.png";
            if (_selectedBox == pbWhiteLocal) path += "_w.png";
            if (_selectedBox == pbLightLocal) path += "_l.png";
            if (_selectedBox == pbGrayLocal) path += "_g.png";
            if (_selectedBox == pbDefaultLocal) path += ".png";

            _selectedBox.Image.Dispose();
            _selectedBox.Image = null;
            _selectedBox.Update();
            _selectedBox.BackColor = SystemColors.Control;

            try
            {
                if (File.Exists(path)) DeleteToRecycle(path);
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

            if (tgtHeight >= cropImg.Height && tgtWidth >= cropImg.Width) return cropImg;

            // resize image if needed
            var scale = Math.Min((double) tgtWidth / cropImg.Width, (double) tgtHeight / cropImg.Height);
            var destWidth = (int) (cropImg.Width * scale);
            var destHeight = (int) (cropImg.Height * scale);
            return new Bitmap(cropImg, new Size(destWidth, destHeight));
        }

        private void picDragSource_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;
            var img = ((PictureBox)sender).Image;
            if (img == null) return;
            _selectedBox = (PictureBox) sender;
            DoDragDrop(img, DragDropEffects.Copy);
        }

        private void pictureBox_DragEnter(object sender, DragEventArgs e)
        {
            if ((e.Data.GetDataPresent(DataFormats.Bitmap) ||
                e.Data.GetDataPresent(DataFormats.FileDrop) ||
                e.Data.GetDataPresent(DataFormats.StringFormat)) &&
                (e.AllowedEffect & DragDropEffects.Copy) != 0)
            {
                e.Effect = DragDropEffects.Copy;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

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
            else if (e.Data.GetDataPresent(DataFormats.StringFormat))
            {
                var link = (string) e.Data.GetData(DataFormats.StringFormat);
                if (!link.StartsWith("http", StringComparison.OrdinalIgnoreCase)) return;

                try
                {
                    ServicePointManager.Expect100Continue = true;
                    ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;
                    using (var stream = new MemoryStream(new WebClient().DownloadData(link)))
                    {
                        imgBitmap = Image.FromStream(stream) as Bitmap;
                    }
                }
                catch
                {
                    // do nothing, will return anyway since imgBitmap will be null
                }
            }
            var target = (PictureBox)sender;
            if (imgBitmap == null || _selectedBox == target) return;

            var path = $"{Helper.Epg123LogosFolder}\\{_callsign}";
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

            if (File.Exists(path)) DeleteToRecycle(path);

            var image = CropAndResizeImage(imgBitmap);
            image.Save(path, ImageFormat.Png);

            LoadLocalImages();
        }

        private void contextMenuStrip2_Opening(object sender, CancelEventArgs e)
        {
            var owner = (ContextMenuStrip) sender;
            _selectedBox = (PictureBox) owner.SourceControl;
            if (_selectedBox?.Image == null) e.Cancel = true;
        }
    }
}