using System;
using System.Drawing;
using System.Net;
using System.Windows.Forms;
using epg123Transfer.SchedulesDirectAPI;
using epg123Transfer.tvdbAPI;

namespace epg123Transfer
{
    public partial class frmManualMatch : Form
    {
        public string IdWas;
        public string IdIs;

        readonly MxfRequest _request;
        public frmManualMatch(MxfRequest request)
        {
            _request = request;
            InitializeComponent();
        }

        private void frmManualMatch_Shown(object sender, EventArgs e)
        {
            if ((_request.SeriesElement != null) && !string.IsNullOrEmpty(_request.SeriesElement.Title)) txtRoviTitle.Text = _request.SeriesElement.Title;
            else txtRoviTitle.Text = _request.PrototypicalTitle ?? _request.Title;

            if (_request.SeriesElement != null)
            {
                tbRoviDescription.Text = _request.SeriesElement.DescriptionElement ?? _request.SeriesElement.DescriptionAttribute;
            }

            Cursor.Current = Cursors.WaitCursor;
            if (string.IsNullOrEmpty(IdIs))
            {
                IdWas = _request.SeriesAttribute ?? _request.SeriesElement?.Uid;
                var search = tvdbApi.TvdbSearchSeriesTitle(txtRoviTitle.Text);
                if (search == null) return;

                foreach (var data in search)
                {
                    try
                    {
                        cmbTvdbTitles.Items.Add(tvdbApi.TvdbGetSeriesData(data.Id));
                        if (cmbTvdbTitles.Items.Count == 7) break;
                    }
                    catch
                    {
                        // ignored
                    }
                }
                cmbTvdbTitles.SelectedIndex = 0;
            }
            else
            {
                grpTvdb.Enabled = btnApply.Visible = false;
                btnCancel.Text = "Exit";
                LoadGracenotePanel(IdIs.Replace("!Series!", ""));
            }
            Cursor.Current = Cursors.Default;
        }

        private void cmbTvdbTitles_SelectedIndexChanged(object sender, EventArgs e)
        {
            var series = (tvdbSeries)cmbTvdbTitles.SelectedItem;
            tbTvdbDescription.Text = series.Overview;

            if ((picTvdb.Image = series.SeriesImage) == null)
            {
                Cursor.Current = Cursors.WaitCursor;
                var link = tvdbApi.TvdbGetSeriesImageUrl(((tvdbSeries)cmbTvdbTitles.SelectedItem).Id);
                if (!string.IsNullOrEmpty(link))
                {
                    try
                    {
                        var req = WebRequest.Create(link);
                        picTvdb.Image = Image.FromStream(req.GetResponse().GetResponseStream());
                        series.SeriesImage = picTvdb.Image;
                    }
                    catch
                    {
                        // ignored
                    }
                }
                Cursor.Current = Cursors.Default;
            }

            LoadGracenotePanel(string.IsNullOrEmpty(series.Zap2ItId) ? null : series.Zap2ItId.Substring(2));
        }

        private void LoadGracenotePanel(string seriesId)
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
                    var program = sdApi.SdGetPrograms(new[] { "SH" + seriesId + "0000" })[0];
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

                        string url;
                        if (!string.IsNullOrEmpty(url = sdApi.SdGetSeriesImageUrl(seriesId)))
                        {
                            try
                            {
                                var req = WebRequest.Create(url);
                                picGracenote.Image = Image.FromStream(req.GetResponse().GetResponseStream());
                            }
                            catch
                            {
                                // ignored
                            }
                        }
                    }
                }
                catch
                {
                    // ignored
                }

                Cursor.Current = Cursors.Default;
            }
            btnApply.Enabled = !string.IsNullOrEmpty(txtGracenoteTitle.Text);
        }

        private void btnApply_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(IdIs))
            {
                IdIs = "!Series!" + ((tvdbSeries)cmbTvdbTitles.SelectedItem).Zap2ItId.Substring(2);
            }
            Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}