using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Windows.Forms;
using System.Web;
using Newtonsoft.Json;

namespace epg123Transfer
{
    public static class tvdbApi
    {
        public static bool isAlive = true;
        private const string jsonBaseUrl = @"https://api.thetvdb.com/";
        private static string ApiKey = "125AF7126EDFF4D9";

        private enum METHODS
        {
            GET,
            POST
        };

        private static string token_;
        private static string token
        {
            get
            {
                if (string.IsNullOrEmpty(token_))
                {
                    StreamReader sr = GetRequestResponse(METHODS.POST, "login", new tvdbTokenRequest() { ApiKey = ApiKey }, false);
                    if (sr != null)
                    {
                        tvdbTokenResponse response = JsonConvert.DeserializeObject<tvdbTokenResponse>(sr.ReadToEnd());
                        token_ = "Bearer " + response.Token;
                    }
                }
                return token_;
            }
        }

        private static StreamReader GetRequestResponse(METHODS method, string uri, object jsonRequest = null, bool tkRequired = true)
        {
            // build url
            string url = string.Format("{0}{1}", jsonBaseUrl, uri);

            // send request and get response
            int timeout = 60000;
            try
            {
                // create the request with defaults
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
                req.AutomaticDecompression = DecompressionMethods.Deflate;
                req.Timeout = timeout;

                // add token if it is required
                if (tkRequired && !string.IsNullOrEmpty(token))
                {
                    req.Headers.Add("Authorization", token);
                }

                // setup request
                switch (method)
                {
                    case METHODS.GET:
                        req.Method = "GET";
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

        public static IList<tvdbSeriesSearchData> tvdbSearchSeriesTitle(string title)
        {
            StreamReader sr = GetRequestResponse(METHODS.GET, "search/series?name=" + HttpUtility.UrlEncode(title));
            if (sr != null)
            {
                return JsonConvert.DeserializeObject<tvdbSearchSeriesResponse>(sr.ReadToEnd()).Data;
            }
            return null;
        }

        public static tvdbSeries tvdbGetSeriesData(int id)
        {
            StreamReader sr = GetRequestResponse(METHODS.GET, "series/" + id.ToString());
            if (sr != null)
            {
                return JsonConvert.DeserializeObject<tvdbSeriesDataResponse>(sr.ReadToEnd()).Data;
            }
            return null;
        }

        public static string tvdbGetSeriesImageUrl(int id)
        {
            string url = string.Empty;

            // determine available images
            StreamReader sr1 = GetRequestResponse(METHODS.GET, "series/" + id.ToString() + "/images");
            if (sr1 == null) return url;

            // pick which image type to pull
            string type = string.Empty;
            tvdbSeriesImagesCount imagesCount = JsonConvert.DeserializeObject<tvdbSeriesImagesCounts>(sr1.ReadToEnd()).Data;
            if (imagesCount.Poster > 0) type = "poster";
            else if (imagesCount.Season > 0) type = "season";
            else if (imagesCount.Series > 0) type = "series";
            else if (imagesCount.Seasonwide > 0) type = "seasonwide";
            else if (imagesCount.Fanart > 0) type = "fanart";
            else return url;

            // get all series images of selected type
            StreamReader sr2 = GetRequestResponse(METHODS.GET, "series/" + id.ToString() + "/images/query?keyType=" + type);
            if (sr2 == null) return url;

            // pick the highest rated image
            decimal maxRating = -0.1M;
            IList<tvdbSeriesImageQueryResult> images = JsonConvert.DeserializeObject<tvdbSeriesImageQueryResults>(sr2.ReadToEnd()).Data;
            foreach (tvdbSeriesImageQueryResult image in images)
            {
                if ((image.RatingsInfo.Average * image.RatingsInfo.Count) > maxRating)
                {
                    maxRating = image.RatingsInfo.Average * image.RatingsInfo.Count;
                    if (!string.IsNullOrEmpty(image.Thumbnail))
                    {
                        url = "http://thetvdb.com/banners/" + image.Thumbnail;
                    }
                    else
                    {
                        url = "http://thetvdb.com/banners/_cache/" + image.FileName;
                    }
                }
            }

            return url;
        }
    }
}
