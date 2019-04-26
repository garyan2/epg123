using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.IO;
using System.Net;
using System.Text;
using Newtonsoft.Json;

namespace epg123Transfer
{
    public static class sdAPI
    {
        enum METHODS
        {
            GET,
            GETVERBOSEMAP,
            POST,
            PUT,
            DELETE
        };

        public static string jsonBaseUrl = @"https://json.schedulesdirect.org";
        public static string jsonApi = @"/20141201/";
        private static string token_;
        private static string token
        {
            get
            {
                if (string.IsNullOrEmpty(token_))
                {
                    frmLogin form = new frmLogin();
                    form.ShowDialog();

                    StreamReader sr = sdGetRequestResponse(METHODS.POST, "token", new SdTokenRequest() { Username = form.username, Password_hash = form.passwordHash }, false);
                    if (sr == null)
                    {
                        MessageBox.Show("Failed to login to Schedules Direct. Please close this window and try again.", "Login Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return null;
                    }

                    SdTokenResponse ret = JsonConvert.DeserializeObject<SdTokenResponse>(sr.ReadToEnd());
                    token_ = ret.Token;
                }
                return token_;
            }
        }

        private static StreamReader sdGetRequestResponse(METHODS method, string uri, object jsonRequest = null, bool tkRequired = true)
        {
            // build url
            string url = string.Format("{0}{1}{2}", jsonBaseUrl, jsonApi, uri);

            // send request and get response
            try
            {
                // create the request with defaults
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
                req.UserAgent = "EPG123";
                req.AutomaticDecompression = DecompressionMethods.Deflate;
                req.Timeout = 60000;

                // add token if it is required
                if (tkRequired && !string.IsNullOrEmpty(token))
                {
                    req.Headers.Add("token", token);
                }

                // setup request
                switch (method)
                {
                    case METHODS.GET:
                        req.Method = "GET";
                        break;
                    case METHODS.GETVERBOSEMAP:
                        req.Method = "GET";
                        req.Headers["verboseMap"] = "true";
                        break;
                    case METHODS.PUT:
                        req.Method = "PUT";
                        break;
                    case METHODS.DELETE:
                        req.Method = "DELETE";
                        break;
                    case METHODS.POST:
                        string send = JsonConvert.SerializeObject(jsonRequest);
                        byte[] body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(jsonRequest));
                        req.Method = "POST";
                        req.ContentType = "application/json";
                        req.Accept = "application/json";
                        req.ContentLength = body.Length;

                        Stream reqStream = req.GetRequestStream();
                        reqStream.Write(body, 0, body.Length);
                        reqStream.Close();
                        break;
                    default:
                        break;
                }

                WebResponse resp = req.GetResponse();
                return new StreamReader(resp.GetResponseStream(), Encoding.UTF8);
            }
            catch { }
            return null;
        }

        public static IList<sdProgram> sdGetPrograms(string[] request)
        {
            StreamReader sr = sdGetRequestResponse(METHODS.POST, "programs", request);
            if (sr != null)
            {
                return JsonConvert.DeserializeObject<IList<sdProgram>>(sr.ReadToEnd());
            }
            return null;
        }

        public static string sdGetSeriesImageUrl(string id)
        {
            string url = string.Empty;

            // get available images
            StreamReader sr = sdGetRequestResponse(METHODS.GET, "metadata/programs/SH" + id, null, false);
            if (sr == null) return url;

            return determineSeriesImage(JsonConvert.DeserializeObject<IList<sdImage>>(sr.ReadToEnd()));
        }

        private static string determineSeriesImage(IList<sdImage> images)
        {
            if (images == null) return null;

            string[] links = new string[9];
            foreach (sdImage image in images)
            {
                if ((image.Category == null) || (image.Aspect == null) || (image.Size == null) || (image.Uri == null)) continue;

                // only want 4x3, medium sized images for series and sports events
                if ((image.Aspect.ToLower() == "2x3") && (image.Size.ToLower() == "sm") &&
                    ((image.Tier == null) || (image.Tier.ToLower() == "series") || (image.Tier.ToLower() == "sport") || (image.Tier.ToLower() == "sport event")))
                {
                    string url = image.Uri.ToLower();
                    if (!url.StartsWith("http"))
                    {
                        url = string.Format("{0}{1}image/{2}", sdAPI.jsonBaseUrl, sdAPI.jsonApi, url);
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
                        default:
                            break;
                    }
                }
            }

            // return the most preferred image
            foreach (string link in links)
            {
                if (!string.IsNullOrEmpty(link)) return link;
            }
            return null;
        }
    }
}