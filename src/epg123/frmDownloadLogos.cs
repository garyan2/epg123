using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace epg123
{
    public partial class frmDownloadLogos : Form
    {
        private bool _cancelDownload;

        public frmDownloadLogos(Dictionary<string, string> sdLogos)
        {
            Application.EnableVisualStyles();
            InitializeComponent();

            backgroundWorker1.RunWorkerAsync(sdLogos);
        }

        private void backgroundWorker1_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            var sdlogos = (Dictionary<string, string>)e.Argument;
            if (!Directory.Exists(Helper.Epg123SdLogosFolder))
            {
                Directory.CreateDirectory(Helper.Epg123SdLogosFolder);
            }

            var processedLogo = 0;
            var totalLogos = sdlogos.Count;
            foreach (var station in sdlogos)
            {
                if (_cancelDownload) break;

                var logo = station.Key.Split('-')[0];
                backgroundWorker1.ReportProgress(++processedLogo * 100 / totalLogos, $"Downloading logos for station {logo} ({processedLogo}/{totalLogos})");
                var file = $"{Helper.Epg123SdLogosFolder}\\{station.Key}.png";
                try
                {
                    var wc = new System.Net.WebClient();
                    using (var stream = new MemoryStream(wc.DownloadData(station.Value)))
                    {
                        // crop image
                        Bitmap cropImg;
                        using (var origImg = Image.FromStream(stream) as Bitmap)
                        {
                            // Find the min/max non-transparent pixels
                            var min = new Point(int.MaxValue, int.MaxValue);
                            var max = new Point(int.MinValue, int.MinValue);

                            for (var x = 0; x < origImg.Width; ++x)
                            {
                                for (var y = 0; y < origImg.Height; ++y)
                                {
                                    var pixelColor = origImg.GetPixel(x, y);
                                    if (pixelColor.A > 0)
                                    {
                                        if (x < min.X) min.X = x;
                                        if (y < min.Y) min.Y = y;

                                        if (x > max.X) max.X = x;
                                        if (y > max.Y) max.Y = y;
                                    }
                                }
                            }

                            // Create a new bitmap from the crop rectangle
                            var cropRectangle = new Rectangle(min.X, min.Y, max.X - min.X + 1, max.Y - min.Y + 1);
                            cropImg = new Bitmap(cropRectangle.Width, cropRectangle.Height);
                            cropImg.SetResolution(origImg.HorizontalResolution, origImg.VerticalResolution);
                            using (var g = Graphics.FromImage(cropImg))
                            {
                                g.DrawImage(origImg, 0, 0, cropRectangle, GraphicsUnit.Pixel);
                            }
                        }
                        cropImg.Save(file, System.Drawing.Imaging.ImageFormat.Png);
                    }
                }
                catch (Exception ex)
                {
                    Logger.WriteVerbose(ex.Message);
                }
            }
        }

        private void backgroundWorker1_ProgressChanged(object sender, System.ComponentModel.ProgressChangedEventArgs e)
        {
            label2.Text = (string)e.UserState;
            if (progressBarTask.Value == e.ProgressPercentage) return;
            progressBarTask.Value = e.ProgressPercentage;
            lblTaskProgress.Text = $"{e.ProgressPercentage}%";
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            Close();
        }

        private void frmDownloadLogos_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!backgroundWorker1.IsBusy) return;
            backgroundWorker1.CancelAsync();
            _cancelDownload = true;
            e.Cancel = true;
        }
    }
}
