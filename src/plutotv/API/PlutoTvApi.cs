using GaRyan2.Utilities;
using System.Collections.Generic;
using System;

namespace GaRyan2.PlutoTvAPI
{
    internal class API : BaseAPI
    {
        public List<PlutoChannel> GetPlutoChannels()
        {
            var now = DateTime.UtcNow;
            var ret = GetApiResponse<List<PlutoChannel>>(Method.GET, $"channels.json?start={now:yyyy-MM-ddTHH:00:00.000Z}&stop={now + TimeSpan.FromHours(24.0):yyyy-MM-ddTHH:00:00.000Z}");
            if (ret == null) Logger.WriteError("Failed to download channels from PlutoTV.");
            else Logger.WriteVerbose($"Downloaded {ret.Count} channels from PlutoTV.");
            return ret;
        }
    }
}