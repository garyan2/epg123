using System;
using System.Collections.Generic;
using System.Drawing;
using System.Net;
using System.Windows.Forms;
using epg123Transfer.MxfXml;

namespace epg123Transfer
{
    public partial class frmManualMatch : Form
    {
        public string idWas;
        public string idIs;

        MxfRequest request;
        public frmManualMatch(MxfRequest request)
        {
            this.request = request;
            InitializeComponent();
        }

        private void frmManualMatch_Shown(object sender, EventArgs e)
        {
            if ((request.SeriesElement != null) && !string.IsNullOrEmpty(request.SeriesElement.Title)) txtRoviTitle.Text = request.SeriesElement.Title;
            else txtRoviTitle.Text = request.PrototypicalTitle ?? request.Title;

            if (request.SeriesElement != null)
            {
                tbRoviDescription.Text = request.SeriesElement.DescriptionElement ?? request.SeriesElement.DescriptionAttribute;
            }

            Cursor.Current = Cursors.WaitCursor;
            if (string.IsNullOrEmpty(idIs))
            {
                idWas = request.SeriesAttribute ?? request.SeriesElement.Uid;
                IList<tvdbSeriesSearchData> search = tvdbApi.tvdbSearchSeriesTitle(txtRoviTitle.Text);
                if (search == null) return;

                foreach (tvdbSeriesSearchData data in search)
                {
                    cmbTvdbTitles.Items.Add(tvdbApi.tvdbGetSeriesData(data.Id));
                }
                cmbTvdbTitles.SelectedIndex = 0;
            }
            else
            {
                grpTvdb.Enabled = btnApply.Visible = false;
                btnCancel.Text = "Exit";
                loadGracenotePanel(idIs.Replace("!Series!", ""));
            }
            Cursor.Current = Cursors.Default;
        }

        private void cmbTvdbTitles_SelectedIndexChanged(object sender, EventArgs e)
        {
            tvdbSeries series = (tvdbSeries)cmbTvdbTitles.SelectedItem;
            tbTvdbDescription.Text = series.Overview;

            if ((picTvdb.Image = series.SeriesImage) == null)
            {
                Cursor.Current = Cursors.WaitCursor;
                string link = tvdbApi.tvdbGetSeriesImageUrl(((tvdbSeries)cmbTvdbTitles.SelectedItem).Id);
                if (!string.IsNullOrEmpty(link))
                {
                    try
                    {
                        WebRequest req = HttpWebRequest.Create(link);
                        picTvdb.Image = Image.FromStream(req.GetResponse().GetResponseStream());
                        series.SeriesImage = picTvdb.Image;
                    }
                    catch { }
                }
                Cursor.Current = Cursors.Default;
            }

            loadGracenotePanel(string.IsNullOrEmpty(series.Zap2itId) ? null : series.Zap2itId.Substring(2));
        }

        private void loadGracenotePanel(string seriesId)
        {
            if (string.IsNullOrEmpty(seriesId))
            {
                txtGracenoteTitle.Text = string.Empty;
                tbGracenoteDescription.Text = string.Empty;
                picGracenote.Image = null;
            }
            else
            {
                Cursor.Current = Cursors.WaitCursor;
                try
                {
                    sdProgram program = sdAPI.sdGetPrograms(new string[] { "SH" + seriesId + "0000" })[0];
                    if ((program.Titles == null) || (program.Descriptions == null))
                    {
                        txtGracenoteTitle.Text = string.Empty;
                        tbGracenoteDescription.Text = string.Empty;
                        picGracenote.Image = null;
                    }
                    else
                    {
                        txtGracenoteTitle.Text = program.Titles[0].Title120;
                        tbGracenoteDescription.Text = program.Descriptions.Description1000[0].Description ?? program.Descriptions.Description100[0].Description;

                        string url = string.Empty;
                        if (!string.IsNullOrEmpty(url = sdAPI.sdGetSeriesImageUrl(seriesId)))
                        {
                            try
                            {
                                WebRequest req = HttpWebRequest.Create(url);
                                picGracenote.Image = Image.FromStream(req.GetResponse().GetResponseStream());
                            }
                            catch { }
                        }
                    }
                }
                catch { }
                Cursor.Current = Cursors.Default;
            }
            btnApply.Enabled = !string.IsNullOrEmpty(txtGracenoteTitle.Text);
        }

        private void btnApply_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(idIs))
            {
                idIs = "!Series!" + ((tvdbSeries)cmbTvdbTitles.SelectedItem).Zap2itId.Substring(2);
            }
            this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
