using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows.Forms;
using System.Xml.Serialization;
using Newtonsoft.Json;

namespace epg123Transfer.SchedulesDirectAPI
{
    public static class sdApi
    {
        private enum methods
        {
            GET,
            POST
        };

        private const string JsonBaseUrl = @"https://json.schedulesdirect.org";
        private const string JsonApi = @"/20141201/";
        private static string sdToken;
        private static string Token
        {
            get
            {
                if (!string.IsNullOrEmpty(sdToken)) return sdToken;
                StreamReader sr = null;
                if (File.Exists(epg123.Helper.Epg123CfgPath))
                {
                    epg123.epgConfig config;
                    using (var stream = new StreamReader(epg123.Helper.Epg123CfgPath, Encoding.Default))
                    {
                        var serializer = new XmlSerializer(typeof(epg123.epgConfig));
                        TextReader reader = new StringReader(stream.ReadToEnd());
                        config = (epg123.epgConfig)serializer.Deserialize(reader);
                        reader.Close();
                    }

                    sr = SdGetRequestResponse(methods.POST, "token", new SdTokenRequest() { Username = config.UserAccount.LoginName, PasswordHash = config.UserAccount.PasswordHash }, false);
                }

                if (sr == null)
                {
                    var form = new frmLogin();
                    form.ShowDialog();

                    sr = SdGetRequestResponse(methods.POST, "token", new SdTokenRequest() { Username = form.Username, PasswordHash = form.PasswordHash }, false);
                    if (sr == null)
                    {
                        MessageBox.Show("Failed to login to Schedules Direct. Please close this window and try again.", "Login Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return null;
                    }
                }

                var ret = JsonConvert.DeserializeObject<SdTokenResponse>(sr.ReadToEnd());
                sdToken = ret.Token;
                return sdToken;
            }
        }

        private static StreamReader SdGetRequestResponse(methods method, string uri, object jsonRequest = null, bool tkRequired = true)
        {
            // build url
            var url = $"{JsonBaseUrl}{JsonApi}{uri}";

            // send request and get response
            try
            {
                // create the request with defaults
                var req = (HttpWebRequest)WebRequest.Create(url);
                req.UserAgent = "EPG123";
                req.AutomaticDecompression = DecompressionMethods.Deflate;
                req.Timeout = 3000;

                // add token if it is required
                if (tkRequired && !string.IsNullOrEmpty(Token))
                {
                    req.Headers.Add("token", Token);
                }

                // setup request
                switch (method)
                {
                    case methods.GET:
                        req.Method = "GET";
                        break;
                    case methods.POST:
                        var body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(jsonRequest));
                        req.Method = "POST";
                        req.ContentType = "application/json";
                        req.Accept = "application/json";
                        req.ContentLength = body.Length;

                        var reqStream = req.GetRequestStream();
                        reqStream.Write(body, 0, body.Length);
                        reqStream.Close();
                        break;
                }

                var resp = req.GetResponse();
                return new StreamReader(resp.GetResponseStream(), Encoding.UTF8);
            }
            catch
            {
                // ignored
            }

            return null;
        }

        public static IList<sdProgram> SdGetPrograms(string[] request)
        {
            var sr = SdGetRequestResponse(methods.POST, "programs", request);
            return sr != null ? JsonConvert.DeserializeObject<IList<sdProgram>>(sr.ReadToEnd()) : null;
        }

        public static string SdGetSeriesImageUrl(string id)
        {
            var url = string.Empty;

            // get available images
            var sr = SdGetRequestResponse(methods.GET, "metadata/programs/SH" + id, null, false);
            return sr == null ? url : DetermineSeriesImage(JsonConvert.DeserializeObject<IList<sdImage>>(sr.ReadToEnd()));
        }

        private static string DetermineSeriesImage(IList<sdImage> images)
        {
            if (images == null) return null;

            var links = new string[9];
            foreach (var image in images)
            {
                if ((image.Category == null) || (image.Aspect == null) || (image.Size == null) || (image.Uri == null)) continue;

                // only want 4x3, medium sized images for series and sports events
                if ((image.Aspect.ToLower() == "2x3") && (image.Size.ToLower() == "sm") &&
                    ((image.Tier == null) || (image.Tier.ToLower() == "series") || (image.Tier.ToLower() == "sport") || (image.Tier.ToLower() == "sport event")))
                {
                    var url = image.Uri.ToLower();
                    if (!url.StartsWith("http"))
                    {
                        url = $"{JsonBaseUrl}{JsonApi}image/{url}";
                    }

                    switch (image.Category.ToLower())
                    {
                        case "banner":      // source-provided image, usually shows cast ensemble with source-provided text
                            if (string.IsNullOrEmpty(links[0])) links[0] = url;
                            break;
                        case "banner-l1":   // same as Banner
                            if (string.IsNullOrEmpty(links[1])) links[1] = url;
                            break;
                        case "banner-l1t":  // undocumented
                            if (string.IsNullOrEmpty(links[2])) links[2] = url;
                            break;
                        case "banner-l2":   // source-provided image with plain text
                            if (string.IsNullOrEmpty(links[3])) links[3] = url;
                            break;
                        case "banner-lo":   // banner with Logo Only
                            if (string.IsNullOrEmpty(links[4])) links[4] = url;
                            break;
                        case "logo":        // official logo for program, sports organization, sports conference, or TV station
                            if (string.IsNullOrEmpty(links[5])) links[5] = url;
                            break;
                        case "banner-l3":   // stock photo image with plain text
                            if (string.IsNullOrEmpty(links[6])) links[6] = url;
                            break;
                        case "iconic":      // representative series/season/episode image, no text
                            if (string.IsNullOrEmpty(links[7])) links[7] = url;
                            break;
                        case "staple":      // the staple image is intended to cover programs which do not have a unique banner image
                            if (string.IsNullOrEmpty(links[8])) links[8] = url;
                            break;
                    }
                }
            }

            // return the most preferred image
            return links.FirstOrDefault(link => !string.IsNullOrEmpty(link));
        }
    }
}