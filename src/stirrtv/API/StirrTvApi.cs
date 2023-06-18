using GaRyan2.Utilities;

namespace GaRyan2.StirrTvApi
{
    internal class API : BaseAPI
    {
        public StirrLineup GetStirrLineup()
        {
            var auto = GetApiResponse<StirrAutoSelect>(Method.GET, @"https://ott-stationselection.sinclairstoryline.com/stationAutoSelection");
            var lineup = auto?.Page[0].Button.MediaContent.Config.Stations[0] ?? "national";

            var ret = GetApiResponse<StirrLineup>(Method.GET, $"channels/stirr?station={lineup}");
            if (ret == null) Logger.WriteError("Failed to download lineup channels from StirrTV.");
            else Logger.WriteVerbose($"Downloaded {lineup.ToUpperInvariant()} lineup from StirrTV.");
            return ret;
        }

        public StirrChannelStatus GetChannelDetail(string stationId)
        {
            var ret = GetApiResponse<StirrChannelStatus>(Method.GET, $"status/{stationId}");
            if (ret == null) Logger.WriteError($"Failed to download channel status from StirrTV for station ID \"{stationId}\".");
            return ret;
        }

        public StirrChannelGuide GetChannelGuide(string stationId)
        {
            var ret = GetApiResponse<StirrChannelGuide>(Method.GET, $"program/stirr/ott/{stationId}");
            if (ret == null) Logger.WriteError($"Failed to download channel guide listings from StirrTV for station ID \"{stationId}\".");
            return ret;
        }
    }
}