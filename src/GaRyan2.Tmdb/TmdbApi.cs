using GaRyan2.Utilities;
using System;
using System.Linq;

namespace GaRyan2.TmdbApi
{
    internal class API : BaseAPI
    {
        public string ApiKey;
        private bool IsAlive = true;

        public TmdbConfiguration Config;
        private readonly bool IncludeAdultTitles;

        private TmdbConfiguration GetTmdbConfigurations()
        {
            var ret = GetApiResponse<TmdbConfiguration>(Method.GET, $"configuration?api_key={ApiKey}");
            if (ret == null)
            {
                Logger.WriteInformation("Failed to get TMDb configurations.");
                IsAlive = false;
            }
            return ret;
        }

        public MovieResults SearchMovieCatalog(string title, int year, string lang)
        {
            // verify tmdb is up and configurations have been downloaded
            if (!IsAlive || string.IsNullOrEmpty(title)) return null;
            if (Config == null)
            {
                Config = GetTmdbConfigurations();
                if (Config == null) return null;
            }

            // search the catalog
            var movies = GetApiResponse<MovieListResponse>(Method.GET, $"search/movie?api_key={ApiKey}&language={lang}&query={Uri.EscapeDataString(title)}&include_adult={IncludeAdultTitles.ToString().ToLower()}{(year == 0 ? "" : $"&primary_release_year={year}")}");
            if (movies == null || movies.TotalResults == 0) return null;
            Logger.WriteVerbose($"TMDb catalog search for \"{title}\" from {year} found {movies.TotalResults} results.");

            // find exact title match and production year
            var movie = movies.Results.FirstOrDefault(arg => arg.Title.Equals(title, StringComparison.OrdinalIgnoreCase) && (year <= 0 || arg.ReleaseDate.Year == year));
            return movie;
        }
    }
}