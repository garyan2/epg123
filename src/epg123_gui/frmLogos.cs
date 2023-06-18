using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows.Forms;
using GaRyan2.SchedulesDirectAPI;
using Microsoft.VisualBasic.FileIO;
using GaRyan2.Utilities;

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
            LoadRemoteImages();
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
            if (File.Exists($"{Helper.Epg123LogosFolder}{_callsign}_c.png") && pbCustomLocal.Image == null)
            {
                pbCustomLocal.BackColor = Color.FromArgb(255, 6, 15, 30);
                pbCustomLocal.Image = Image.FromFile($"{Helper.Epg123LogosFolder}{_callsign}_c.png");
                pbCustomLocal.Tag = $"{Helper.Epg123LogosFolder}{_callsign}_c.png";
                pbCustomLocal.Refresh();
            }

            var categories = new string[] { "dark", "white", "light", "gray" };
            var pictureBoxes = new PictureBox[] { pbDarkLocal, pbWhiteLocal, pbLightLocal, pbGrayLocal };
            var backColors = new Color[] { Color.FromArgb(255, 6, 15, 30), Color.FromArgb(255, 6, 15, 30), Color.White, Color.White };
            for (int i = 0; i < categories.Length; ++i)
            {
                var stationLogo = _station.StationLogos?.FirstOrDefault(arg => arg.Category != null && arg.Category.Equals(categories[i], StringComparison.OrdinalIgnoreCase));
                if (stationLogo == null) continue;
                var logoFilepath = $"{Helper.Epg123LogosFolder}{stationLogo.Md5}.png";
                if (File.Exists(logoFilepath) && pictureBoxes[i].Image == null)
                {
                    pictureBoxes[i].BackColor = backColors[i];
                    pictureBoxes[i].Image = Image.FromFile(logoFilepath);
                    pictureBoxes[i].Tag = logoFilepath;
                    pictureBoxes[i].Refresh();
                }
            }

            if (_station.Logo?.Url != null)
            {
                var logoFilepath = $"{Helper.Epg123LogosFolder}{_station.Logo.Md5}.png";
                if (File.Exists(logoFilepath) && pbDefaultLocal.Image == null)
                {
                    pbDefaultLocal.BackColor = Color.FromArgb(255, 6, 15, 30);
                    pbDefaultLocal.Image = Image.FromFile(logoFilepath);
                    pbDefaultLocal.Tag = logoFilepath;
                    pbDefaultLocal.Refresh();
                }
            }
        }

        private void LoadRemoteImages()
        {
            if (_station.Logo != null)
            {
                pbDefaultRemote.BackColor = Color.FromArgb(255, 6, 15, 30);
                pbDefaultRemote.Load(_station.Logo.Url);
            }

            if (_station.StationLogos == null) return;
            foreach (var image in _station.StationLogos)
            {
                switch (image.Category)
                {
                    case "dark":
                        if (pbDarkRemote.Image != null) break;
                        pbDarkRemote.BackColor = Color.FromArgb(255, 6, 15, 30); ;
                        pbDarkRemote.Load(image.Url);
                        pbDarkRemote.Refresh();
                        break;
                    case "white":
                        if (pbWhiteRemote.Image != null) break;
                        pbWhiteRemote.BackColor = Color.FromArgb(255, 6, 15, 30); ;
                        pbWhiteRemote.Load(image.Url);
                        pbWhiteRemote.Refresh();
                        break;
                    case "light":
                        if (pbLightRemote.Image != null) break;
                        pbLightRemote.BackColor = Color.White;
                        pbLightRemote.Load(image.Url);
                        pbLightRemote.Refresh();
                        break;
                    case "gray":
                        if (pbGrayRemote.Image != null) break;
                        pbGrayRemote.BackColor = Color.White;
                        pbGrayRemote.Load(image.Url);
                        pbGrayRemote.Refresh();
                        break;
                }
            }
        }

        private void menuDeleteLocal_Click(object sender, EventArgs e)
        {
            var path = (string)_selectedBox.Tag;
            List<PictureBox> pictureBoxes = new List<PictureBox> { pbCustomLocal, pbDarkLocal, pbDefaultLocal, pbGrayLocal, pbLightLocal, pbWhiteLocal };
            for (int i = 0; i < pictureBoxes.Count; ++i)
            {
                if (pictureBoxes[i].Tag == null || (string)pictureBoxes[i].Tag != path) continue;
                pictureBoxes[i].Image.Dispose();
                pictureBoxes[i].Image = null;
                pictureBoxes[i].Update();
                pictureBoxes[i].BackColor = SystemColors.Control;
                pictureBoxes[i] = null;
            }

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

            var path = $"{Helper.Epg123LogosFolder}{_callsign}";
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