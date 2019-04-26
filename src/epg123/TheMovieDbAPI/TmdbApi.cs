using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using System.Threading;

namespace epg123
{
    public static class tmdbAPI
    {
        public static bool isAlive;
        private const string tmdbBaseUrl = @"http://api.themoviedb.org/3/";

        private static TmdbConfiguration config;
        private static TmdbMovieListResponse search_results;
        private static int search_index;
        private static bool incAdult;
        private static int posterSizeIdx;
        private static int backdropSizeIdx;

        #region Public Attributes
        public static IList<sdImage> sdImages
        {
            get
            {
                List<sdImage> ret = new List<sdImage>();
                if (!string.IsNullOrEmpty(posterImageUrl))
                {
                    int width = int.Parse(config.Images.PosterSizes[posterSizeIdx].Substring(1));
                    int height = (int)(width * 1.5);
                    ret.Add(new sdImage()
                    {
                        Aspect = "2x3",
                        Category = "Poster Art",
                        Height = height.ToString(),
                        Size = "Md",
                        Uri = posterImageUrl,
                        Width = width.ToString()
                    });
                }
                //if (!string.IsNullOrEmpty(backdropImageUrl))
                //{
                //    int width = int.Parse(config.Images.BackdropSizes[backdropSizeIdx].Substring(1));
                //    int height = (int)(width * 9.0 / 16.0);
                //    ret.Add(new sdImage()
                //    {
                //        Aspect = "16x9",
                //        Category = "Poster Art",
                //        Height = height.ToString(),
                //        Size = "Md",
                //        Uri = backdropImageUrl,
                //        Width = width.ToString()
                //    });
                //}
                return ret;
            }
        }
        public static string posterImageUrl
        {
            get
            {
                string ret = null;
                if (search_results.Results[search_index].PosterPath != null)
                {
                    ret = String.Format("{0}{1}{2}", config.Images.BaseUrl, config.Images.PosterSizes[posterSizeIdx], search_results.Results[search_index].PosterPath);
                }
                return ret;
            }
        }
        public static string backdropImageUrl
        {
            get
            {
                string ret = null;
                if (search_results.Results[search_index].BackdropPath != null)
                {
                    ret = String.Format("{0}{1}{2}", config.Images.BaseUrl, config.Images.BackdropSizes[posterSizeIdx], search_results.Results[search_index].BackdropPath);
                }
                return ret;
            }
        }
        public static int SearchIndex
        {
            set
            {
                search_index = value;
            }
        }
        public static string[] SearchResults
        {
            get
            {
                if ((search_results.Results == null) || (search_results.Results.Count == 0)) return null;

                List<string> ret = new List<string>();
                foreach (TmdbMovieResults movie in search_results.Results)
                {
                    string title = movie.Title;
                    if (!string.IsNullOrEmpty(movie.ReleaseDate))
                    {
                        DateTime dt;
                        if (DateTime.TryParse(movie.ReleaseDate, out dt))
                        {
                            title += string.Format(" ({0})", dt.Year.ToString());
                        }
                    }
                    ret.Add(title);
                }
                return ret.ToArray();
            }
        }
        public static string Title
        {
            get
            {
                return search_results.Results[search_index].Title;
            }
        }
        public static string Year
        {
            get
            {
                if (search_results.Results[search_index].ReleaseDate == null) return null;

                string ret = String.Empty;
                DateTime dt;
                if (DateTime.TryParse(search_results.Results[search_index].ReleaseDate, out dt))
                {
                    ret = dt.Year.ToString();
                }
                return ret;
            }
        }
        public static int ID
        {
            get
            {
                return search_results.Results[search_index].Id;
            }
        }
        #endregion

        public static void Initialize(bool includeAdult)
        {
            isAlive = ((config = getTmdbConfiguration()) != null);
            incAdult = includeAdult;

            if (isAlive)
            {
                for (int i = 0; i < config.Images.PosterSizes.Count; ++i)
                {
                    if (int.Parse(config.Images.PosterSizes[i].Substring(1)) >= 300)
                    {
                        posterSizeIdx = i;
                        break;
                    }
                }
                for (int i = 0; i < config.Images.BackdropSizes.Count; ++i)
                {
                    if (int.Parse(config.Images.BackdropSizes[i].Substring(1)) >= 500)
                    {
                        backdropSizeIdx = i;
                        break;
                    }
                }
            }
        }

        private static StreamReader tmdbGetRequestResponse(string uri)
        {
            // build url
            string url = string.Format("{0}{1}", tmdbBaseUrl, uri);

            while (true)
            {
                try
                {
                    // setup web request method
                    HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
                    req.Method = "GET";

                    // perform request and get response
                    WebResponse resp = req.GetResponse();
                    return new StreamReader(resp.GetResponseStream(), Encoding.UTF8);
                }
                catch (WebException wex)
                {
                    var response = (HttpWebResponse)wex.Response;
                    if (((int)response.StatusCode == 429) && isAlive)
                    {
                        int delay = int.Parse(response.Headers.GetValues("Retry-After")[0]) + 1;
                        Logger.WriteVerbose(string.Format("TMDb API server requested a delay of {0} seconds before next request.", delay));
                        Thread.Sleep(delay * 1000);
                        continue;
                    }
                    Logger.WriteError(string.Format("TMDb API WebException thrown. Message: {0} , Status: {1}", wex.Message, wex.Status));
                    break;
                }
                catch (Exception ex)
                {
                    Logger.WriteError(string.Format("TMDb API Unknown exception thrown. Message: {0}", ex.Message));
                    break;
                }
            }
            return null;
        }

        private static TmdbConfiguration getTmdbConfiguration()
        {
            string uri = string.Format("configuration?api_key={0}", Properties.Resources.tmdbAPIKey);
            try
            {
                StreamReader sr = tmdbGetRequestResponse(uri);
                if (sr != null)
                {
                    Logger.WriteVerbose("Successfully retrieved TMDb configurations.");
                    return JsonConvert.DeserializeObject<TmdbConfiguration>(sr.ReadToEnd());
                }
                Logger.WriteError("Failed to retrieve TMDb configurations.");
            }
            catch (Exception ex)
            {
                Logger.WriteError(ex.Message);
            }
            return null;
        }

        public static int SearchCatalog(string title, int year, string lang)
        {
            string uri = String.Format("search/movie?api_key={0}&language={1}&query={2}&include_adult={3}{4}",
                                       Properties.Resources.tmdbAPIKey, lang, Escape(title), incAdult.ToString().ToLower(),
                                       (year == 0) ? string.Empty : string.Format("&primary_release_year={0}", year));
            try
            {
                StreamReader sr = tmdbGetRequestResponse(uri);
                if (sr != null)
                {
                    search_results = JsonConvert.DeserializeObject<TmdbMovieListResponse>(sr.ReadToEnd());
                    int count = (search_results == null) ? 0 : search_results.Results.Count;
                    Logger.WriteVerbose(string.Format("TMDb catalog search for \"{0}\" from {1} found {2} results.", title, year, count));
                    return count;
                }
            }
            catch (Exception ex)
            {
                search_results = new TmdbMovieListResponse();
                Logger.WriteError(ex.Message);
            }
            return -1;
        }

        private static string Escape(string s)
        {
            s = s.Replace("&", "and");
            return Regex.Replace(s, "[" + Regex.Escape(new String(Path.GetInvalidFileNameChars())) + "]", "-");
        }
    }
}