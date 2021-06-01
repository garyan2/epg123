using System;
using Newtonsoft.Json;

namespace epg123.SchedulesDirect
{
    public static partial class SdApi
    {
        public static bool AddLineup(string lineup)
        {
            var sr = GetRequestResponse(methods.PUT, $"lineups/{lineup}");
            if (sr == null)
            {
                Logger.WriteError($"Failed to get a response from Schedules Direct when trying to add lineup {lineup}.");
                return false;
            }

            try
            {
                var resp = JsonConvert.DeserializeObject<AddRemoveLineupResponse>(sr, jSettings);
                switch (resp.Code)
                {
                    case 0:
                        Logger.WriteVerbose($"Successfully added lineup {lineup} to account. serverID: {resp.ServerId} , message: {resp.Message} , changesRemaining: {resp.ChangesRemaining}");
                        return true;
                    default:
                        Logger.WriteError($"Failed to add lineup {lineup} to account. serverID: {resp.ServerId} , code: {resp.Code} , message: {resp.Message} , changesRemaining: {resp.ChangesRemaining}");
                        break;
                }
            }
            catch (Exception ex)
            {
                Logger.WriteError($"AddLineup() Unknown exception thrown. Message: {ex.Message}");
            }
            return false;
        }

        public static bool RemoveLineup(string lineup)
        {
            var sr = GetRequestResponse(methods.DELETE, $"lineups/{lineup}");
            if (sr == null)
            {
                Logger.WriteError($"Failed to get a response from Schedules Direct when trying to remove lineup {lineup}.");
                return false;
            }

            try
            {
                var resp = JsonConvert.DeserializeObject<AddRemoveLineupResponse>(sr);
                switch (resp.Code)
                {
                    case 0:
                        Logger.WriteVerbose($"Successfully removed lineup {lineup} from account. serverID: {resp.ServerId} , message: {resp.Message} , changesRemaining: {resp.ChangesRemaining}");
                        return true;
                    default:
                        Logger.WriteError($"Failed to remove lineup {lineup} from account. serverID: {resp.ServerId} , code: {resp.Code} , message: {resp.Message} , changesRemaining: {resp.ChangesRemaining}");
                        break;
                }
            }
            catch (Exception ex)
            {
                Logger.WriteError($"RemoveLineup() Unknown exception thrown. Message: {ex.Message}");
            }
            return false;
        }
    }

    public class AddRemoveLineupResponse : BaseResponse
    {
        [JsonProperty("changesRemaining")]
        public int ChangesRemaining { get; set; }
    }
}
