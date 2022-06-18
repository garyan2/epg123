using Newtonsoft.Json;

namespace epg123.SchedulesDirect
{
    public static partial class SdApi
    {
        public static bool AddLineup(string lineup)
        {
            var ret = GetSdApiResponse<AddRemoveLineupResponse>("PUT", $"lineups/{lineup}");
            if (ret != null) Logger.WriteVerbose($"Successfully added lineup {lineup} to account. serverID: {ret.ServerId} , message: {ret.Message} , changesRemaining: {ret.ChangesRemaining}");
            else Logger.WriteError($"Failed to get a response from Schedules Direct when trying to add lineup {lineup}.");
            return ret != null;
        }

        public static bool RemoveLineup(string lineup)
        {
            var ret = GetSdApiResponse<AddRemoveLineupResponse>("DELETE", $"lineups/{lineup}");
            if (ret != null) Logger.WriteVerbose($"Successfully removed lineup {lineup} from account. serverID: {ret.ServerId} , message: {ret.Message} , changesRemaining: {ret.ChangesRemaining}");
            else Logger.WriteError($"Failed to get a response from Schedules Direct when trying to remove lineup {lineup}.");
            return ret != null;
        }
    }

    public class AddRemoveLineupResponse : BaseResponse
    {
        [JsonProperty("changesRemaining")]
        public int ChangesRemaining { get; set; }
    }
}
