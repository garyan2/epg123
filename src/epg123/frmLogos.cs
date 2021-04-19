using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Windows.Forms;
using epg123.SchedulesDirect;
using Microsoft.VisualBasic.FileIO;

namespace epg123
{
    public partial class frmLogos : Form
    {
        private readonly string _callsign = string.Empty;
        private readonly LineupStation _station;
        private PictureBox _selectedBox = null;

        public frmLogos(LineupStation station)
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
            try
            {
                FileSystem.DeleteFile(file, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
            }
            catch
            {
                // do nothing
            }
        }

        private void LoadLocalImages()
        {
            if (File.Exists($"{Helper.Epg123LogosFolder}\\{_callsign}_c.png") && pbCustomLocal.Image == null)
            {
                pbCustomLocal.BackColor = Color.FromArgb(255, 6, 15, 30);
                pbCustomLocal.Image = Image.FromFile($"{Helper.Epg123LogosFolder}\\{_callsign}_c.png");
                pbCustomLocal.Refresh();
            }

            if (File.Exists($"{Helper.Epg123LogosFolder}\\{_callsign}_d.png") && pbDarkLocal.Image == null)
            {
                pbDarkLocal.BackColor = Color.FromArgb(255, 6, 15, 30); ;
                pbDarkLocal.Image = Image.FromFile($"{Helper.Epg123LogosFolder}\\{_callsign}_d.png");
                pbDarkLocal.Refresh();
            }

            if (File.Exists($"{Helper.Epg123LogosFolder}\\{_callsign}_w.png") && pbWhiteLocal.Image == null)
            {
                pbWhiteLocal.BackColor = Color.FromArgb(255, 6, 15, 30); ;
                pbWhiteLocal.Image = Image.FromFile($"{Helper.Epg123LogosFolder}\\{_callsign}_w.png");
                pbWhiteLocal.Refresh();
            }

            if (File.Exists($"{Helper.Epg123LogosFolder}\\{_callsign}_l.png") && pbLightLocal.Image == null)
            {
                pbLightLocal.BackColor = Color.White;
                pbLightLocal.Image = Image.FromFile($"{Helper.Epg123LogosFolder}\\{_callsign}_l.png");
                pbLightLocal.Refresh();
            }

            if (File.Exists($"{Helper.Epg123LogosFolder}\\{_callsign}_g.png") && pbGrayLocal.Image == null)
            {
                pbGrayLocal.BackColor = Color.White;
                pbGrayLocal.Image = Image.FromFile($"{Helper.Epg123LogosFolder}\\{_callsign}_g.png");
                pbGrayLocal.Refresh();
            }

            if (File.Exists($"{Helper.Epg123LogosFolder}\\{_callsign}.png") && pbDefaultLocal.Image == null)
            {
                pbDefaultLocal.BackColor = Color.FromArgb(255, 6, 15, 30);
                pbDefaultLocal.Image = Image.FromFile($"{Helper.Epg123LogosFolder}\\{_callsign}.png");
                pbDefaultLocal.Refresh();
            }
        }

        private void LoadRemoteImages(LineupStation station)
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
                        pbDarkRemote.Refresh();
                        break;
                    case "white":
                        pbWhiteRemote.BackColor = Color.FromArgb(255, 6, 15, 30); ;
                        pbWhiteRemote.Load(image.Url);
                        pbWhiteRemote.Refresh();
                        break;
                    case "light":
                        pbLightRemote.BackColor = Color.White;
                        pbLightRemote.Load(image.Url);
                        pbLightRemote.Refresh();
                        break;
                    case "gray":
                        pbGrayRemote.BackColor = Color.White;
                        pbGrayRemote.Load(image.Url);
                        pbGrayRemote.Refresh();
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

        private void picDragSource_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;
            var img = ((PictureBox)sender).Image?.Clone();
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
                imgBitmap = Image.FromFile(((string[]) e.Data.GetData(DataFormats.FileDrop))[0]).Clone() as Bitmap;
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

            var image = Helper.CropAndResizeImage(imgBitmap);
            image.Save(path, ImageFormat.Png);
            imgBitmap.Dispose();
            image.Dispose();

            GC.Collect();
            LoadLocalImages();
        }

        private void contextMenuStrip2_Opening(object sender, CancelEventArgs e)
        {
            var owner = (ContextMenuStrip) sender;
            _selectedBox = (PictureBox) owner.SourceControl;
            if (_selectedBox?.Image == null) e.Cancel = true;
        }

        private void frmLogos_FormClosing(object sender, FormClosingEventArgs e)
        {
            pbCustomLocal.Image?.Dispose();
            pbDarkLocal.Image?.Dispose();
            pbWhiteLocal.Image?.Dispose();
            pbLightLocal.Image?.Dispose();
            pbGrayLocal.Image?.Dispose();
        }
    }
}