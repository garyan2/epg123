using GaRyan2.Utilities;
using Microsoft.MediaCenter.Guide;
using System;
using System.Linq;

namespace GaRyan2.WmcUtilities
{
    public static partial class WmcStore
    {
        /// <summary>
        /// Clears all service logos
        /// </summary>
        public static void ClearLineupChannelLogos()
        {
            foreach (Service service in new Services(WmcObjectStore).Cast<Service>())
            {
                if (service.LogoImage == null) continue;
                service.LogoImage = null;
                service.Update();
            }
            Logger.WriteInformation("Completed clearing all station logos.");
        }

        /// <summary>
        /// Clears all schedule entries in a service
        /// </summary>
        /// <param name="mergedChannelId"></param>
        public static void ClearServiceScheduleEntries(long mergedChannelId)
        {
            try
            {
                if (!(WmcObjectStore.Fetch(mergedChannelId) is MergedChannel channel)) return;
                foreach (ScheduleEntry scheduleEntry in channel.Service.ScheduleEntries.Cast<ScheduleEntry>())
                {
                    scheduleEntry.Service = null;
                    scheduleEntry.Program = null;
                    scheduleEntry.Unlock();
                    scheduleEntry.Update();
                }
                channel.Service.ScheduleEndTime = DateTime.MinValue;
                channel.Service.Update();

                // notify channel it was updated
                channel.Update();
            }
            catch (Exception ex)
            {
                Logger.WriteError($"Exception thrown during ClearServiceScheduleEntries(). Message: {ex.Message}");
            }
        }
    }
}