using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using epg123.SchedulesDirectAPI;
using Newtonsoft.Json;

namespace epg123.TheMovieDbAPI
{
    public static class tmdbApi
    {
        public static bool IsAlive;
        private const string TmdbBaseUrl = @"http://api.themoviedb.org/3/";

        private static TmdbConfiguration config;
        private static TmdbMovieListResponse searchResults;
        private static bool incAdult;
        private static int posterSizeIdx;

        #region Public Attributes
        public static IList<sdImage> SdImages
        {
            get
            {
                var ret = new List<sdImage>();
                if (string.IsNullOrEmpty(PosterImageUrl)) return ret;
                var width = int.Parse(config.Images.PosterSizes[posterSizeIdx].Substring(1));
                var height = (int)(width * 1.5);
                ret.Add(new sdImage()
                {
                    Aspect = "2x3",
                    Category = "Poster Art",
                    Height = height,
                    Size = "Md",
                    Uri = PosterImageUrl,
                    Width = width
                });
                return ret;
            }
        }

        public static string PosterImageUrl => searchResults.Results[0]?.PosterPath != null
            ? $"{config.Images.BaseUrl}{config.Images.PosterSizes[posterSizeIdx]}{searchResults.Results[0].PosterPath}"
            : null;

        #endregion

        public static void Initialize(bool includeAdult)
        {
            IsAlive = ((config = GetTmdbConfiguration()) != null);
            incAdult = includeAdult;

            if (!IsAlive || config == null) return;
            for (var i = 0; i < config.Images.PosterSizes.Count; ++i)
            {
                if (int.Parse(config.Images.PosterSizes[i].Substring(1)) < 300) continue;
                posterSizeIdx = i;
                break;
            }
        }

        private static StreamReader TmdbGetRequestResponse(string uri)
        {
            // build url
            var url = $"{TmdbBaseUrl}{uri}";

            while (true)
            {
                try
                {
                    // setup web request method
                    var req = (HttpWebRequest)WebRequest.Create(url);
                    req.Method = "GET";

                    // perform request and get response
                    var resp = req.GetResponse();
                    return new StreamReader(resp.GetResponseStream(), Encoding.UTF8);
                }
                catch (WebException wex)
                {
                    var response = (HttpWebResponse)wex.Response;
                    if ((int)response.StatusCode == 429 && IsAlive)
                    {
                        var delay = int.Parse(response.Headers.GetValues("Retry-After")?[0]) + 1;
                        Thread.Sleep(delay * 1000);
                        continue;
                    }
                    Logger.WriteError($"TMDb API WebException thrown. Message: {wex.Message} , Status: {wex.Status}");
                    break;
                }
                catch (Exception ex)
                {
                    Logger.WriteError($"TMDb API Unknown exception thrown. Message: {ex.Message}");
                    break;
                }
            }
            return null;
        }

        private static TmdbConfiguration GetTmdbConfiguration()
        {
            var uri = $"configuration?api_key={Properties.Resources.tmdbAPIKey}";
            try
            {
                var sr = TmdbGetRequestResponse(uri);
                if (sr != null)
                {
                    Logger.WriteVerbose("Successfully retrieved TMDb configurations.");
                    return JsonConvert.DeserializeObject<TmdbConfiguration>(sr.ReadToEnd());
                }
            }
            catch (Exception ex)
            {
                Logger.WriteInformation(ex.Message);
            }
            Logger.WriteInformation("Failed to retrieve TMDb configurations.");
            return null;
        }

        public static int SearchCatalog(string title, int year, string lang)
        {
            var uri = $"search/movie?api_key={Properties.Resources.tmdbAPIKey}&language={lang}&query={Uri.EscapeDataString(title)}&include_adult={incAdult.ToString().ToLower()}{((year == 0) ? string.Empty : $"&primary_release_year={year}")}";
            try
            {
                var sr = TmdbGetRequestResponse(uri);
                if (sr != null)
                {
                    searchResults = JsonConvert.DeserializeObject<TmdbMovieListResponse>(sr.ReadToEnd());
                    var count = searchResults?.Results.Count ?? 0;
                    if (count > 0) Logger.WriteVerbose($"TMDb catalog search for \"{title}\" from {year} found {count} results.");
                    return count;
                }
            }
            catch (Exception ex)
            {
                searchResults = new TmdbMovieListResponse();
                Logger.WriteError(ex.Message);
            }
            return -1;
        }
    }
}