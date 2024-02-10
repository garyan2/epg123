using GaRyan2.Utilities;
using Microsoft.MediaCenter.Guide;
using Microsoft.MediaCenter.Pvr;
using Microsoft.Win32;
using System;
using System.IO;
using System.Linq;

namespace GaRyan2.WmcUtilities
{
    public static partial class WmcStore
    {
        public static void AutoMapChannels(bool mapChannels = false)
        {
            if (!mapChannels) return;

            // get all active channels in lineup(s) from EPG123/HDHR2MXF
            var epg123Channels = GetEpg123LineupChannels();
            if (epg123Channels.Count == 0)
            {
                Logger.WriteError("There are no EPG123/HDHR2MXF listings in the database to perform any mappings.");
                return;
            }

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
            WmcMergedLineup.FullMerge(false);
            WmcMergedLineup.Update();
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
                            Logger.WriteVerbose($"Failed to associate lineup {listings.Lineup} with device {device.Name ?? "<<NULL>>"} ({(device.ScannedLineup == null ? "<<NULL>>" : device.ScannedLineup.Name)}). Exception:{Helper.ReportExceptionMessages(ex)}");
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
                Logger.WriteInformation($"Exception thrown during SubscribeLineupChannel(). Message:{Helper.ReportExceptionMessages(ex)}");
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
                Logger.WriteInformation($"Exception thrown during SetChannelEnableState(). Message:{Helper.ReportExceptionMessages(ex)}");
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
                Logger.WriteInformation($"Exception thrown during DetermineRecordingsInProgress(). Message:{Helper.ReportExceptionMessages(ex)}");
            }
            return false;
        }

        public static void DetermineStorageStatus()
        {
            EpgNotifier notifier = Helper.ReadJsonFile(Helper.EmailNotifier, typeof(EpgNotifier)) ?? new EpgNotifier();
            try
            {
                var daysToWarning = 3;
                var daysToError = 1;
                using (var recKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Media Center\Service\Recording", false))
                {
                    if (recKey == null) return;

                    var recPath = (string)recKey.GetValue("RecordPath");
                    var dirInfo = new DirectoryInfo(recPath);

                    var quota = (long)recKey.GetValue("Quota", long.MaxValue);
                    long quotaUsed = dirInfo.EnumerateFiles("*.*", SearchOption.AllDirectories).Sum(fi => fi.Length);
                    long quotaRemaining = quota - quotaUsed;

                    var drive = new DriveInfo(recPath.Substring(0, 1));
                    long driveRemaining = drive.AvailableFreeSpace;

                    long available = Math.Min(quotaRemaining, driveRemaining);
                    var pctUsed = 100 * quotaUsed / (double)(quotaUsed + available);

                    var msg = $"Recorder storage drive {drive.Name} has {Helper.BytesToString(available)} available. ({Helper.BytesToString(quotaUsed)} of {Helper.BytesToString(quotaUsed + available)} used)";
                    if (notifier.StorageErrorGB > 0 && (long)notifier.StorageErrorGB * 1024 * 1024 * 1024 > available) Logger.WriteError(msg);
                    else if (notifier.StorageWarningGB > 0 && (long)notifier.StorageWarningGB * 1024 * 1024 * 1024 > available) Logger.WriteWarning(msg);
                    else Logger.WriteInformation(msg);
                }

                var recordings = new Recordings(WmcObjectStore).Where(arg => !arg.Abandoned && arg.State != RecordingState.Deleted && arg.ReasonDeleted == DeleteReason.BumpedToRecord)
                                                               .OrderBy(arg => arg.ExpectedDeleteDate).ToList();
                if (recordings.Count > 0)
                {
                    var firstDelete = recordings.First();
                    var deleteTime = DateTime.SpecifyKind(firstDelete.ExpectedDeleteDate, DateTimeKind.Utc);
                    var msg = $"WMC predicts limited storage starting at {deleteTime.ToLocalTime()} and may need to delete existing recordings to record new programs.";
                    if (notifier.StorageErrorGB == 0 && deleteTime < DateTime.UtcNow + TimeSpan.FromDays(daysToError)) Logger.WriteError(msg);
                    else if (notifier.StorageWarningGB == 0 && deleteTime < DateTime.UtcNow + TimeSpan.FromDays(daysToWarning)) Logger.WriteWarning(msg);
                    else Logger.WriteInformation(msg);
                }

                recordings = new Recordings(WmcObjectStore).Where(arg => !arg.Abandoned && arg.State != RecordingState.Deleted && arg.ReasonDeleted == DeleteReason.DiskFull)
                                                           .OrderBy(arg => arg.StartTime).ToList();
                if (recordings.Count > 0)
                {
                    var firstDelete = recordings.First();
                    var startTime = DateTime.SpecifyKind(firstDelete.StartTime, DateTimeKind.Utc);
                    var msg = $"WMC predicts running out of storage starting at {startTime.ToLocalTime()} and may need to cancel scheduled recordings.";
                    if (notifier.StorageErrorGB == 0 && startTime < DateTime.UtcNow + TimeSpan.FromDays(daysToError)) Logger.WriteError(msg);
                    else if (notifier.StorageWarningGB == 0 && startTime < DateTime.UtcNow + TimeSpan.FromDays(daysToWarning)) Logger.WriteWarning(msg);
                    else Logger.WriteInformation(msg);
                }

                var requests = new RequestedPrograms(WmcObjectStore).Where(arg => arg.IsActive && arg.IsConflicted && !arg.WillRecord)
                                                                    .OrderBy(arg => arg.PossibleAssignments?.First().ScheduleEntry.StartTime).ToList();
                var conflicts = requests.Count;
                if (conflicts > 0)
                {
                    foreach (var request in requests)
                    {
                        var entry = request.PossibleAssignments.First().ScheduleEntry;
                        var startTime = DateTime.SpecifyKind(entry.StartTime, DateTimeKind.Utc);
                        var msg = $"{entry.Program.Title} - {entry.Program.EpisodeTitle} at {startTime.ToLocalTime()} will not be recorded due to a tuner conflict.";
                        if (startTime > DateTime.UtcNow && startTime < DateTime.UtcNow + TimeSpan.FromDays(notifier.ConflictErrorDays)) Logger.WriteError(msg);
                        else if (startTime > DateTime.UtcNow && startTime < DateTime.UtcNow + TimeSpan.FromDays(notifier.ConflictWarningDays)) Logger.WriteWarning(msg);
                        else --conflicts;
                    }
                }
                if (conflicts == 0) Logger.WriteInformation($"No tuner conflicts detected within next {Math.Max(notifier.ConflictErrorDays, notifier.ConflictWarningDays)} days.");
            }
            catch { }
        }
    }
}