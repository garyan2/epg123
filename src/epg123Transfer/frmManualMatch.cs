using System;
using System.Drawing;
using System.Net;
using System.Windows.Forms;
using epg123Transfer.SchedulesDirectAPI;

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
            }
            else
            {
                btnCancel.Text = "Exit";
                LoadGracenotePanel(IdIs.Replace("!Series!", ""));
            }
            Cursor.Current = Cursors.Default;
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
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}