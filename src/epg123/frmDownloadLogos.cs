using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace epg123
{
    public partial class frmDownloadLogos : Form
    {
        private bool cancelDownload;

        public frmDownloadLogos(Dictionary<string, string> sdLogos)
        {
            Application.EnableVisualStyles();
            InitializeComponent();

            backgroundWorker1.RunWorkerAsync(sdLogos);
        }

        private void backgroundWorker1_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            Dictionary<string, string> sdlogos = (Dictionary<string, string>)e.Argument;
            if (!Directory.Exists(Helper.Epg123SdLogosFolder))
            {
                Directory.CreateDirectory(Helper.Epg123SdLogosFolder);
            }

            int processedLogo = 0;
            int totalLogos = sdlogos.Count;
            foreach (KeyValuePair<string, string> station in sdlogos)
            {
                if (cancelDownload) break;

                string logo = station.Key.Split('-')[0];
                backgroundWorker1.ReportProgress(++processedLogo * 100 / totalLogos, string.Format("Downloading logos for station {0} ({1}/{2})", logo, processedLogo, totalLogos));
                string file = string.Format("{0}\\{1}.png", Helper.Epg123SdLogosFolder, station.Key);
                try
                {
                    System.Net.WebClient wc = new System.Net.WebClient();
                    using (MemoryStream stream = new MemoryStream(wc.DownloadData(station.Value)))
                    {
                        // crop image
                        Bitmap cropImg;
                        using (Bitmap origImg = Bitmap.FromStream(stream) as Bitmap)
                        {
                            // Find the min/max non-transparent pixels
                            Point min = new Point(int.MaxValue, int.MaxValue);
                            Point max = new Point(int.MinValue, int.MinValue);

                            for (int x = 0; x < origImg.Width; ++x)
                            {
                                for (int y = 0; y < origImg.Height; ++y)
                                {
                                    Color pixelColor = origImg.GetPixel(x, y);
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
                            Rectangle cropRectangle = new Rectangle(min.X, min.Y, max.X - min.X + 1, max.Y - min.Y + 1);
                            cropImg = new Bitmap(cropRectangle.Width, cropRectangle.Height);
                            cropImg.SetResolution(origImg.HorizontalResolution, origImg.VerticalResolution);
                            using (Graphics g = Graphics.FromImage(cropImg))
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
            if (progressBarTask.Value != e.ProgressPercentage)
            {
                progressBarTask.Value = e.ProgressPercentage;
                lblTaskProgress.Text = string.Format("{0}%", e.ProgressPercentage);
            }
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            this.Close();
        }

        private void frmDownloadLogos_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (backgroundWorker1.IsBusy)
            {
                backgroundWorker1.CancelAsync();
                cancelDownload = true;
                e.Cancel = true;
                return;
            }
        }
    }
}
