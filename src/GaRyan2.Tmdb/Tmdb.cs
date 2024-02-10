using GaRyan2.TmdbApi;

namespace GaRyan2
{
    public static class Tmdb
    {
        private static readonly API api = new API()
        {
            BaseAddress = "https://api.themoviedb.org/3/",
            ApiKey = "05c57e15f625338ad1c5aa8a2899e589"
        };

        public static int PosterWidth { get; private set; }
        private static int _minWidth;

        public static void Initialize(string userAgent, string size)
        {
            switch (size)
            {
                case "Sm": _minWidth = 120; break;
                case "Lg": _minWidth = 480; break;
                default: _minWidth = 240; break;
            }
            api.UserAgent = userAgent;
            api.Initialize();
        }

        public static string FindPosterArtwork(string title, int year, string lang = "en")
        {
            var movie = api.SearchMovieCatalog(title, year, lang);
            if (movie == null || api.Config == null) return null;

            if (PosterWidth == 0)
            {
                var width = 0;
                for (int index = 0; index < api.Config.Images.PosterSizes.Count; ++index)
                {
                    if ((width = int.Parse(api.Config.Images.PosterSizes[index].Substring(1))) < _minWidth) continue;
                    break;
                }
                if (width > 0) PosterWidth = width;
                else return null;
            }

            return $"{api.Config.Images.BaseUrl}w{PosterWidth}{movie.PosterPath}";
        }
    }
}