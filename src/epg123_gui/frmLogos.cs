using epg123_gui.Properties;
using GaRyan2;
using GaRyan2.SchedulesDirectAPI;
using GaRyan2.Utilities;
using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows.Forms;

namespace epg123_gui
{
    public partial class frmLogos : Form
    {
        private readonly LineupStation _station;
        private PictureBox _selectedBox = null;
        private readonly string _customLogo;
        private readonly bool isRemote = Settings.Default.CfgLocation.StartsWith("http");
        private readonly string baseLogoPath = Settings.Default.CfgLocation.Replace("\\epg123.cfg", "\\logos\\").Replace("/epg123/epg123.cfg", "/logos/");
        private readonly List<string> availableLogos;
        public bool LogoChanged;

        public frmLogos(LineupStation station)
        {
            InitializeComponent();

            _station = station;
            _customLogo = $"{station.Callsign}_c.png";
            if (isRemote) availableLogos = SchedulesDirect.GetCustomLogosFromServer($"{baseLogoPath}custom");
            else availableLogos = Directory.GetFiles(Helper.Epg123LogosFolder, "*.png").ToList().Select(logo => logo.Substring(Helper.Epg123LogosFolder.Length)).ToList();

            label7.Text = $"{station.Callsign}\n{station.Name}\n{station.Affiliate}";
        }

        private void frmLogos_Load(object sender, EventArgs e)
        {
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
            // load custom logo
            if (availableLogos.Contains(_customLogo))
            {
                var logoFilePath = $"{baseLogoPath}{_customLogo}";

                pbCustomLocal.BackColor = Color.FromArgb(255, 6, 15, 30);
                if (isRemote) pbCustomLocal.Load(logoFilePath);
                else pbCustomLocal.Image = Image.FromFile(logoFilePath);
                pbCustomLocal.Tag = logoFilePath;
                pbCustomLocal.Refresh();
            }

            // load targeted background logos
            var categories = new string[] { "dark", "white", "light", "gray" };
            var pictureBoxes = new PictureBox[] { pbDarkLocal, pbWhiteLocal, pbLightLocal, pbGrayLocal };
            var backColors = new Color[] { Color.FromArgb(255, 6, 15, 30), Color.FromArgb(255, 6, 15, 30), Color.White, Color.White };
            for (int i = 0; i < categories.Length; ++i)
            {
                var stationLogo = _station.StationLogos?.FirstOrDefault(arg => arg.Category != null && arg.Category.Equals(categories[i], StringComparison.OrdinalIgnoreCase));
                if (stationLogo == null) continue;

                if (availableLogos.Contains($"{stationLogo.Md5}.png"))
                {
                    var logoFilepath = $"{baseLogoPath}{stationLogo.Md5}.png";

                    pictureBoxes[i].BackColor = backColors[i];
                    if (isRemote) pictureBoxes[i].Load(logoFilepath);
                    else pictureBoxes[i].Image = Image.FromFile(logoFilepath);
                    pictureBoxes[i].Tag = logoFilepath;
                    pictureBoxes[i].Refresh();
                }
            }

            // load default logo
            if (_station.Logo?.Url != null)
            {
                if (availableLogos.Contains($"{_station.Logo.Md5}.png"))
                {
                    var logoFilepath = $"{baseLogoPath}{_station.Logo.Md5}.png";

                    pbDefaultLocal.BackColor = Color.FromArgb(255, 6, 15, 30);
                    if (isRemote) pbDefaultLocal.Load(logoFilepath);
                    else pbDefaultLocal.Image = Image.FromFile(logoFilepath);
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
                        pbDarkRemote.BackColor = Color.FromArgb(255, 6, 15, 30);
                        pbDarkRemote.Load(image.Url);
                        pbDarkRemote.Refresh();
                        break;
                    case "white":
                        if (pbWhiteRemote.Image != null) break;
                        pbWhiteRemote.BackColor = Color.FromArgb(255, 6, 15, 30);
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
                if (isRemote && SchedulesDirect.DeleteLogo(path.Replace("/logos/", "/logos/custom/")))
                {
                    availableLogos.Remove($"{_customLogo}");
                    LogoChanged = true;
                }
                else if (File.Exists(path))
                {
                    availableLogos.Remove($"{_customLogo}");
                    DeleteToRecycle(path);
                }
                //return;
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
            _selectedBox = (PictureBox)sender;
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
                imgBitmap = Image.FromFile(((string[])e.Data.GetData(DataFormats.FileDrop))[0]).Clone() as Bitmap;
            }
            else if (e.Data.GetDataPresent(DataFormats.StringFormat))
            {
                var link = (string)e.Data.GetData(DataFormats.StringFormat);
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

            var path = $"{baseLogoPath}{_customLogo}";
            if (target.Image != null)
            {
                target.Image.Dispose();
                target.Image = null;
                target.Refresh();
            }

            var image = Helper.CropAndResizeImage(imgBitmap);
            if (isRemote)
            {
                using (var ms = new MemoryStream())
                {
                    image.Save(ms, ImageFormat.Png);
                    if (SchedulesDirect.UploadLogo(path.Replace("/logos/", "/logos/custom/"), ms.ToArray()) && !availableLogos.Contains($"{_customLogo}"))
                    {
                        availableLogos.Add($"{_customLogo}");
                        LogoChanged = true;
                    }
                }
            }
            else
            {
                if (File.Exists(path)) DeleteToRecycle(path);
                image.Save(path, ImageFormat.Png);
                if (!availableLogos.Contains($"{_customLogo}")) availableLogos.Add($"{_customLogo}");
            }
            imgBitmap.Dispose();
            image.Dispose();

            LoadLocalImages();
        }

        private void contextMenuStrip2_Opening(object sender, CancelEventArgs e)
        {
            var owner = (ContextMenuStrip)sender;
            _selectedBox = (PictureBox)owner.SourceControl;
            if (_selectedBox?.Image == null) e.Cancel = true;
        }

        private void frmLogos_FormClosing(object sender, FormClosingEventArgs e)
        {
            pbCustomLocal.Image?.Dispose();
            pbDarkLocal.Image?.Dispose();
            pbWhiteLocal.Image?.Dispose();
            pbLightLocal.Image?.Dispose();
            pbGrayLocal.Image?.Dispose();
            pbDefaultLocal.Image?.Dispose();
        }
    }
}