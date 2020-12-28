using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Web;
using Newtonsoft.Json;

namespace epg123Transfer.tvdbAPI
{
    public static class tvdbApi
    {
        private const string JsonBaseUrl = @"https://api.thetvdb.com/";
        private const string ApiKey = "125AF7126EDFF4D9";

        private enum methods
        {
            GET,
            POST
        };

        private static string token;
        private static string Token
        {
            get
            {
                if (!string.IsNullOrEmpty(token)) return token;
                var sr = GetRequestResponse(methods.POST, "login", new tvdbTokenRequest() { ApiKey = ApiKey }, false);
                if (sr == null) return token;
                var response = JsonConvert.DeserializeObject<tvdbTokenResponse>(sr.ReadToEnd());
                token = "Bearer " + response.Token;
                return token;
            }
        }

        private static StreamReader GetRequestResponse(methods method, string uri, object jsonRequest = null, bool tkRequired = true)
        {
            // build url
            var url = $"{JsonBaseUrl}{uri}";

            // send request and get response
            const int timeout = 60000;
            try
            {
                // create the request with defaults
                var req = (HttpWebRequest)WebRequest.Create(url);
                req.AutomaticDecompression = DecompressionMethods.Deflate;
                req.Timeout = timeout;

                // add token if it is required
                if (tkRequired && !string.IsNullOrEmpty(Token))
                {
                    req.Headers.Add("Authorization", Token);
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

        public static IList<tvdbSeriesSearchData> TvdbSearchSeriesTitle(string title)
        {
            var sr = GetRequestResponse(methods.GET, "search/series?name=" + HttpUtility.UrlEncode(title));
            return sr != null ? JsonConvert.DeserializeObject<tvdbSearchSeriesResponse>(sr.ReadToEnd()).Data : null;
        }

        public static tvdbSeries TvdbGetSeriesData(int id)
        {
            var sr = GetRequestResponse(methods.GET, "series/" + id);
            return sr != null ? JsonConvert.DeserializeObject<tvdbSeriesDataResponse>(sr.ReadToEnd()).Data : null;
        }

        public static string TvdbGetSeriesImageUrl(int id)
        {
            var url = string.Empty;

            // determine available images
            var sr1 = GetRequestResponse(methods.GET, "series/" + id + "/images");
            if (sr1 == null) return url;

            // pick which image type to pull
            string type;
            var imagesCount = JsonConvert.DeserializeObject<tvdbSeriesImagesCounts>(sr1.ReadToEnd()).Data;
            if (imagesCount.Poster > 0) type = "poster";
            else if (imagesCount.Season > 0) type = "season";
            else if (imagesCount.Series > 0) type = "series";
            else if (imagesCount.Seasonwide > 0) type = "seasonwide";
            else if (imagesCount.Fanart > 0) type = "fanart";
            else return url;

            // get all series images of selected type
            var sr2 = GetRequestResponse(methods.GET, "series/" + id + "/images/query?keyType=" + type);
            if (sr2 == null) return url;

            // pick the highest rated image
            var maxRating = -0.1M;
            var images = JsonConvert.DeserializeObject<tvdbSeriesImageQueryResults>(sr2.ReadToEnd()).Data;
            foreach (var image in images)
            {
                if ((image.RatingsInfo.Average * image.RatingsInfo.Count) <= maxRating) continue;
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

            return url;
        }
    }
}
