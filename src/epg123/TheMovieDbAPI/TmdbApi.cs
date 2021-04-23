using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using Newtonsoft.Json;

namespace epg123.TheMovieDbAPI
{
    public static class tmdbApi
    {
        public static bool IsAlive;
        public const string TmdbBaseUrl = @"http://api.themoviedb.org/3/";

        public static TmdbConfiguration Config;
        public static TmdbMovieListResponse SearchResults;
        private static bool incAdult;

        public static void Initialize(bool includeAdult)
        {
            IsAlive = (Config = GetTmdbConfiguration()) != null;
            incAdult = includeAdult;
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
                    Logger.WriteInformation($"TMDb API WebException thrown. Message: {wex.Message} , Status: {wex.Status}");
                    IsAlive = false;
                    break;
                }
                catch (Exception ex)
                {
                    Logger.WriteInformation($"TMDb API Unknown exception thrown. Message: {ex.Message}");
                    IsAlive = false;
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
                    SearchResults = JsonConvert.DeserializeObject<TmdbMovieListResponse>(sr.ReadToEnd());
                    var count = SearchResults?.Results.Count ?? 0;
                    if (count > 0) Logger.WriteVerbose($"TMDb catalog search for \"{title}\" from {year} found {count} results.");
                    return count;
                }
            }
            catch (Exception ex)
            {
                SearchResults = new TmdbMovieListResponse();
                Logger.WriteError(ex.Message);
            }
            return -1;
        }
    }
}