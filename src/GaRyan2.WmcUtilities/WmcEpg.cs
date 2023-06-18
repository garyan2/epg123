using GaRyan2.Utilities;
using Microsoft.MediaCenter.Guide;
using Microsoft.MediaCenter.Pvr;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GaRyan2.WmcUtilities
{
    public static partial class WmcStore
    {
        public static void AutoMapChannels(bool mapChannels = false)
        {
            // get all active channels in lineup(s) from EPG123/HDHR2MXF
            var epg123Channels = GetEpg123LineupChannels();
            if (epg123Channels.Count == 0)
            {
                Logger.WriteError("There are no EPG123/HDHR2MXF listings in the database to perform any mappings.");
                return;
            }

            // get all stations that have been removed from EPG123/HDHR2MXF lineups
            var removedStations = new Channels(WmcObjectStore).Where(arg => (arg.Lineup?.Name.StartsWith("EPG123") ?? false) ||
                                                                            (arg.Lineup?.Name.StartsWith("HDHR2MXF") ?? false))
                                                              .Where(arg => ChannelIsOrphaned(epg123Channels, arg))
                                                              .OrderBy(arg => arg.Number).ThenBy(arg => arg.SubNumber).ToList();

            // remove orphaned stations from epg123/hdhr2mxf lineup(s)
            foreach (var station in removedStations)
            {
                var lineup = station.Lineup;
                Logger.WriteVerbose($"Channel '{station}' was removed from lineup '{lineup}'");
                lineup.RemoveChannel(station);
                lineup.Update();
            }
            Logger.WriteVerbose($"Completed channel cleanup as needed after MXF file import.");

            // stop here if mapping channels is false
            if (!mapChannels) goto Finish;

            // get all merged channels
            var mergedChannels = (from MergedChannel mergedChannel in WmcMergedLineup.UncachedChannels
                                  where !(mergedChannel.TuningInfos?.Empty ?? true) &&
                                         mergedChannel.ChannelType != ChannelType.UserHidden &&
                                         mergedChannel.ChannelType != ChannelType.WmisBroadband &&
                                         mergedChannel.PrimaryChannel?.Lineup != null
                                  select mergedChannel)
                                  .OrderBy(arg => arg.Number).ThenBy(arg => arg.SubNumber).ToList();
            if (mergedChannels.Count == 0)
            {
                Logger.WriteError("There are no merged channels in the database to perform any mappings.");
                return;
            }

            // map stations to channels as needed
            foreach (var mergedChannel in mergedChannels)
            {
                var originalChannelNumber = $"{mergedChannel.OriginalNumber}{(mergedChannel.OriginalSubNumber > 0 ? $".{mergedChannel.OriginalSubNumber}" : "")}";
                var epg123Channel = epg123Channels.Where(arg => arg.ChannelNumber.Number == mergedChannel.OriginalNumber)
                                                  .FirstOrDefault(arg => arg.ChannelNumber.SubNumber == mergedChannel.OriginalSubNumber);
                if (epg123Channel != null && !mergedChannel.PrimaryChannel.IsSameAs(epg123Channel))
                {
                    if (mergedChannel.PrimaryChannel.Lineup.Name.StartsWith("Scanned") || mergedChannel.PrimaryChannel.Lineup.Name.StartsWith("ZZZ123"))
                    {
                        Logger.WriteVerbose($"Mapping '{epg123Channel.CallSign} - {epg123Channel.Lineup.Name}' to merged channel '{mergedChannel}'");
                        SubscribeLineupChannel(epg123Channel.Id, mergedChannel.Id);
                    }
                    else if (!epg123Channel.Service.IsSameAs(mergedChannel.Service))
                    {
                        Logger.WriteVerbose($"Skipped matching '{epg123Channel.CallSign} - {epg123Channel.Lineup.Name}' to merged channel '{mergedChannel}' due to having channel '{mergedChannel.PrimaryChannel.CallSign} - {mergedChannel.PrimaryChannel.Lineup?.Name ?? "<<NULL>>"}' already assigned.");
                    }
                }
                else if (mergedChannel.PrimaryChannel.Lineup.Name.StartsWith("Scanned") && mergedChannel.UserBlockedState < UserBlockedState.Blocked)
                {
                    SetChannelEnableState(mergedChannel.Id, false);
                }
            }
            Logger.WriteInformation("Completed the automatic mapping of lineup stations to tuner channels.");

        Finish:
            // finish it
            WmcMergedLineup.FullMerge(false);
            WmcMergedLineup.Update();
        }

        private static bool ChannelIsOrphaned(List<Channel> channels, Channel channel)
        {
            foreach (Channel c in channels)
            {
                if (c.IsSameAs(channel)) return false;
            }
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lineupChannelId"></param>
        /// <param name="mergedChannelId"></param>
        public static void SubscribeLineupChannel(long lineupChannelId, long mergedChannelId)
        {
            try
            {
                Channel listings = null;
                if (!(WmcObjectStore.Fetch(mergedChannelId) is MergedChannel mergedChannel)) return;
                if (lineupChannelId > 0)
                {
                    // grab the listings
                    listings = WmcObjectStore.Fetch(lineupChannelId) as Channel;
                    if (listings == null) return;

                    // add this channel lineup to the device group if necessary
                    foreach (Device device in mergedChannel.Lineup.DeviceGroup.Devices)
                    {
                        try
                        {
                            if (device.Name.ToLower().Contains("delete") || device.ScannedLineup == null ||
                                !device.ScannedLineup.IsSameAs(mergedChannel.PrimaryChannel.Lineup) ||
                                device.WmisLineups != null && device.WmisLineups.Contains(listings.Lineup))
                                continue;
                            device.SubscribeToWmisLineup(listings.Lineup);
                            device.Update();
                        }
                        catch (Exception ex)
                        {
                            Logger.WriteVerbose($"Failed to associate lineup {listings.Lineup} with device {device.Name ?? "<<NULL>>"} ({(device.ScannedLineup == null ? "<<NULL>>" : device.ScannedLineup.Name)}). {ex}");
                        }
                    }

                    // map listings to primary channel and touch up secondary channels
                    if ((mergedChannel.PrimaryChannel?.Lineup?.Name?.StartsWith("Scanned") ?? false) && !mergedChannel.SecondaryChannels.Contains(mergedChannel.PrimaryChannel))
                    {
                        mergedChannel.SecondaryChannels.Add(mergedChannel.PrimaryChannel);
                    }
                    if (mergedChannel.SecondaryChannels.Contains(listings))
                    {
                        mergedChannel.SecondaryChannels.RemoveAllMatching(listings);
                    }
                    mergedChannel.PrimaryChannel = listings;
                    mergedChannel.Service = listings.Service;
                }
                else if (!mergedChannel.PrimaryChannel?.Lineup?.Name?.StartsWith("Scanned") ?? true)
                {
                    var scanned = mergedChannel.SecondaryChannels.FirstOrDefault(arg => arg.Lineup?.Name?.StartsWith("Scanned") ?? false);
                    if (scanned != null)
                    {
                        mergedChannel.PrimaryChannel = scanned;
                        mergedChannel.Service = scanned.Service;
                        mergedChannel.SecondaryChannels.RemoveAllMatching(scanned);
                    }
                }
                else return;

                mergedChannel.Update();
                SetChannelEnableState(mergedChannelId, listings != null);
            }
            catch (Exception ex)
            {
                Logger.WriteInformation($"Exception thrown during SubscribeLineupChannel(). {ex}");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mergedChannelId"></param>
        /// <param name="enable"></param>
        public static void SetChannelEnableState(long mergedChannelId, bool enable)
        {
            try
            {
                if (!(WmcObjectStore.Fetch(mergedChannelId) is MergedChannel mergedChannel)) return;
                foreach (TuningInfo tuningInfo in mergedChannel.TuningInfos)
                {
                    var infoOverride = tuningInfo.GetOverride(mergedChannel);
                    if (!infoOverride?.IsUserOverride ?? true) continue;
                    infoOverride.UserBlockedState = tuningInfo.IsSuggestedBlocked ? UserBlockedState.Blocked : UserBlockedState.Enabled;
                    infoOverride.Update();
                    tuningInfo.Update();
                }
                mergedChannel.UserBlockedState = enable ? UserBlockedState.Enabled : UserBlockedState.Blocked;
                mergedChannel.Update();
                mergedChannel.Lineup.NotifyChannelUpdated(mergedChannel);
            }
            catch (Exception ex)
            {
                Logger.WriteInformation($"Exception thrown during SetChannelEnableState(). {ex}");
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static bool DetermineRecordingsInProgress()
        {
            try
            {
                foreach (Recording recording in new Recordings(WmcObjectStore))
                {
                    if (recording.State == RecordingState.Initializing ||
                        recording.State == RecordingState.Recording) return true;
                }
            }
            catch (Exception ex)
            {
                Logger.WriteInformation($"Exception thrown during DetermineRecordingsInProgress(). {ex}");
            }
            return false;
        }
    }
}