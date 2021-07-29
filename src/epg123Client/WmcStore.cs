using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;
using Microsoft.MediaCenter.Guide;
using Microsoft.MediaCenter.Pvr;
using Microsoft.MediaCenter.Store;
using Microsoft.MediaCenter.TV.Tuning;
using epg123;

namespace epg123Client
{
    internal static class WmcStore
    {
        private static ObjectStore objectStore;
        private static MergedLineup wmcMergedLineup;
        public static bool StoreExpired;

        public static ObjectStore WmcObjectStore
        {
            get
            {
                if (objectStore != null && !StoreExpired) return objectStore;
                var sha256Man = new SHA256Managed();
                ObjectStore.FriendlyName = @"Anonymous!User";
                ObjectStore.DisplayName = Convert.ToBase64String(sha256Man.ComputeHash(Encoding.Unicode.GetBytes(ObjectStore.GetClientId(true))));
                objectStore = ObjectStore.AddObjectStoreReference();
                StoreExpired = false;

                objectStore.StoreExpired += WmcObjectStore_StoreExpired;
                return objectStore;
            }
        }

        private static void WmcObjectStore_StoreExpired(object sender, StoredObjectEventArgs e)
        {
            Logger.WriteError("A database recovery has been detected. Attempting to open new database.");
            Close();
            StoreExpired = true;
            objectStore.StoreExpired -= WmcObjectStore_StoreExpired;
            if (WmcObjectStore != null)
            {
                Logger.WriteInformation("Successfully opened new store.");
            }
        }

        public static MergedLineup WmcMergedLineup
        {
            get
            {
                if (wmcMergedLineup != null) return wmcMergedLineup;
                using (var mergedLineups = new MergedLineups(WmcObjectStore))
                {
                    foreach (MergedLineup lineup in mergedLineups)
                    {
                        if (!lineup.UncachedChannels.Any()) continue;
                        wmcMergedLineup = lineup;
                        break;
                    }
                }
                return wmcMergedLineup;
            }
        }

        /// <summary>
        /// Closes the WMC ObjectStore with the option to dispose the ObjectStore
        /// </summary>
        /// <param name="dispose">true = dispose ObjectStore</param>
        public static void Close(bool dispose = false)
        {
            if (objectStore != null)
            {
                objectStore.StoreExpired -= WmcObjectStore_StoreExpired;
                ObjectStore.ReleaseObjectStoreReference();
            }
            wmcMergedLineup = null;

            try
            {
                if (dispose)
                {
                    objectStore?.Dispose();
                }
            }
            catch (Exception ex)
            {
                Logger.WriteInformation($"Exception thrown while trying to dispose of ObjectStore. {ex.Message}\n{ex.StackTrace}");
            }
            objectStore = null;
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }

        /// <summary>
        /// Ensure lineups will show in the "About Guide" option in WMC as well as a language value
        /// </summary>
        /// <returns>true if at least 1 EPG123 or HRHR2MXF lineup was found</returns>
        public static bool ActivateEpg123LineupsInStore()
        {
            var lineups = 0;
            try
            {
                foreach (Lineup lineup in new Lineups(WmcObjectStore))
                {
                    // only want to do this with EPG123 lineups
                    if (!lineup.Provider.Name.Equals("EPG123") && !lineup.Provider.Name.Equals("HDHR2MXF")) continue;

                    // make sure the lineup type and language are set
                    if (string.IsNullOrEmpty(lineup.LineupTypes) || string.IsNullOrEmpty(lineup.Language))
                    {
                        lineup.LineupTypes = "WMIS";
                        lineup.Language = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
                        lineup.Update();
                    }
                    ++lineups;
                }
            }
            catch (Exception ex)
            {
                Logger.WriteInformation($"Exception thrown during ActivateEpg123LineupsInStore(). {ex.Message}\n{ex.StackTrace}");
            }
            return (lineups > 0);
        }

        /// <summary>
        /// Set a custom callsign for a merged channel
        /// </summary>
        /// <param name="id">database object key</param>
        /// <param name="callsign">desired custom callsign, a null or empty string will assign primary channel's callsign</param>
        /// <returns>resulting callsign for the merged channel</returns>
        public static void SetChannelCustomCallsign(long id, string callsign)
        {
            try
            {
                if (!(WmcObjectStore.Fetch(id) is MergedChannel channel)) return;
                channel.CallSign = callsign;
                channel.Update();
            }
            catch (Exception ex)
            {
                Logger.WriteError($"Exception thrown during SetChannelCustomCallsign(). {ex.Message}\n{ex.StackTrace}");
            }
        }

        /// <summary>
        /// Set a custom channel number for a merged channel
        /// </summary>
        /// <param name="id">database object key</param>
        /// <param name="number">desired custom number, a null or empty string will revert to original channel numbers</param>
        /// <returns>resulting channel number for the merged channel</returns>
        public static void SetChannelCustomNumber(long id, string number)
        {
            try
            {
                if (!(WmcObjectStore.Fetch(id) is MergedChannel channel)) return;
                if (!string.IsNullOrEmpty(number))
                {
                    var digits = number.Split('.');
                    if (digits.Length > 0) channel.Number = int.Parse(digits[0]);
                    channel.SubNumber = digits.Length > 1 ? int.Parse(digits[1]) : 0;
                }
                else
                {
                    channel.Number = channel.OriginalNumber;
                    channel.SubNumber = channel.OriginalSubNumber;
                }
                channel.Update();
            }
            catch (Exception ex)
            {
                Logger.WriteError($"Exception thrown during SetChannelCustomNumber(). {ex.Message}\n{ex.StackTrace}");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="scannedLineup"></param>
        /// <param name="service"></param>
        /// <param name="channel"></param>
        /// <param name="tuningInfos"></param>
        public static void AddUserChannel(Lineup scannedLineup, Service service, Channel channel, List<TuningInfo> tuningInfos)
        {
            try
            {
                WmcObjectStore.Add(service);
                scannedLineup.AddChannel(channel);

                foreach (var tuningInfo in tuningInfos)
                {
                    WmcObjectStore.Add(tuningInfo);
                    channel.TuningInfos.Add(tuningInfo);
                }

                scannedLineup.NotifyChannelAdded(channel);
            }
            catch (Exception ex)
            {
                Logger.WriteError($"Exception thrown during AddUserChannel(). {ex.Message}\n{ex.StackTrace}");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="channelId"></param>
        public static void DeleteChannel(long channelId)
        {
            try
            {
                if (!(WmcObjectStore.Fetch(channelId) is Channel channel)) return;
                if (channel.ChannelType == ChannelType.UserAdded) WmcMergedLineup.DeleteUserAddedChannel(channel);
                else WmcMergedLineup.RemoveChannel(channel);
                WmcMergedLineup.Update();
                channel.Uncache();
            }
            catch (Exception ex)
            {
                Logger.WriteError($"Exception thrown during DeleteChannel(). {ex.Message}\n{ex.StackTrace}");
            }
        }

        /// <summary>
        /// Clears all service logos
        /// </summary>
        public static void ClearLineupChannelLogos()
        {
            foreach (Service service in new Services(WmcObjectStore))
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
                foreach (ScheduleEntry scheduleEntry in channel.Service.ScheduleEntries)
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
                Logger.WriteError($"Exception thrown during ClearServiceScheduleEntries(). {ex.Message}\n{ex.StackTrace}");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lineupId"></param>
        public static void UnsubscribeChannelsInLineup(long lineupId)
        {
            try
            {
                // unsubscribe all channels with this lineup as a primary
                if (!(WmcObjectStore.Fetch(lineupId) is Lineup lineup)) return;
                foreach (var channel in lineup.GetChannels())
                {
                    foreach (MergedChannel mergedChannel in channel.ReferencingPrimaryChannels)
                    {
                        SubscribeLineupChannel(0, mergedChannel.Id);
                    }

                    foreach (MergedChannel mergedChannel in channel.ReferencingSecondaryChannels)
                    {
                        mergedChannel.SecondaryChannels.RemoveAllMatching(channel);
                        mergedChannel.Update();
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.WriteError($"Exception thrown during UnsubscribeChannelsInLineup(). {ex.Message}\n{ex.StackTrace}");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lineupId"></param>
        public static void DeleteLineup(long lineupId)
        {
            try
            {
                // remove all channels from lineup
                if (!(WmcObjectStore.Fetch(lineupId) is Lineup lineup)) return;
                foreach (var channel in lineup.GetChannels())
                {
                    lineup.RemoveChannel(channel);
                }

                // remove lineup from device(s)
                foreach (Device device in new Devices(WmcObjectStore))
                {
                    if (!device.WmisLineups.Contains(lineup)) continue;
                    device.WmisLineups.RemoveAllMatching(lineup);
                    device.Update();
                }

                // null the lineup name (necessary?)
                lineup.Name = null;
                lineup.Update();
            }
            catch (Exception ex)
            {
                Logger.WriteError($"Exception thrown during DeleteLineup(). {ex.Message}\n{ex.StackTrace}");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mergedChannel"></param>
        /// <returns></returns>
        public static HashSet<long> GetAllScannedSourcesForChannel(MergedChannel mergedChannel)
        {
            var ret = new HashSet<long>();
            try
            {
                if (mergedChannel.PrimaryChannel.Lineup.Name.StartsWith("Scanned"))
                {
                    ret.Add(mergedChannel.PrimaryChannel.Lineup.Id);
                }
                foreach (Channel channel in mergedChannel.SecondaryChannels)
                {
                    if (!(channel.Lineup?.Name ?? "").StartsWith("Scanned")) continue;
                    if (channel.Lineup != null) ret.Add(channel.Lineup.Id);
                }
            }
            catch (Exception ex)
            {
                Logger.WriteError($"Exception thrown during GetAllScannedSourcesForChannel() for merged channel {mergedChannel}. {ex.Message}\n{ex.StackTrace}");
            }
            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mergedChannel"></param>
        /// <returns></returns>
        public static string GetAllTuningInfos(MergedChannel mergedChannel)
        {
            // attempt to repair tuning infos
            if (mergedChannel.TuningInfos == null || mergedChannel.TuningInfos.Empty)
            {
                try
                {
                    mergedChannel.AddChannelListings(null);
                }
                catch
                {
                    Logger.WriteInformation($"Attempted to repair merged channel \"{mergedChannel}\" with no tuning infos.");
                }
            }

            return GetAllTuningInfos((Channel) mergedChannel);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mergedChannel"></param>
        /// <returns></returns>
        public static string GetAllTuningInfos(Channel mergedChannel)
        {
            string ret = null;
            try
            {
                // build tuning info
                var tuningInfos = new HashSet<string>();
                foreach (TuningInfo tuningInfo in mergedChannel.TuningInfos)
                {
                    // handle any overrides to tuninginfo
                    // assumes that tuninginfo is valid unless an override explicitly says otherwise
                    var shown = true;
                    if (!tuningInfo.Overrides.Empty)
                    {
                        foreach (TuningInfoOverride tuningInfoOverride in tuningInfo.Overrides)
                        {
                            if ((tuningInfoOverride.Channel?.Id ?? 0) != mergedChannel.Id || !tuningInfoOverride.IsLatestVersion) continue;
                            if (tuningInfoOverride.Channel.ChannelType != ChannelType.UserHidden &&
                                (!tuningInfoOverride.IsUserOverride || tuningInfoOverride.UserBlockedState != UserBlockedState.Disabled)) continue;
                            shown = false;
                            break;
                        }
                    }

                    if (!shown) continue;

                    // unfortunately the lock emoji didn't come into play until Unicode 6.0.0
                    // Windows 7 uses Unicode 5.x, it will just show an open block
                    const string lockSymbol = "\uD83D\uDD12";
                    switch (tuningInfo)
                    {
                        case DvbTuningInfo dvbTuningInfo:
                        {
                            var ti = dvbTuningInfo;
                            switch (dvbTuningInfo.TuningSpace)
                            {
                                case "DVB-T":
                                    // formula to convert channel (n) to frequency (fc) is fc = 8n + 306 (in MHz) for UHF
                                    // formula to convert channel (n) to frequency (fc) is fc = 7n + 142.5 (in MHz) for VHF
                                    int channel;
                                    var band = "UHF";
                                    if (ti.Frequency < 230000)
                                    {
                                        channel = (ti.Frequency - 142300) / 7000;
                                        band = "VHF";
                                    }
                                    else
                                    {
                                        channel = (ti.Frequency - 305800) / 8000;
                                    }
                                    tuningInfos.Add($"{(ti.IsEncrypted ? lockSymbol : string.Empty)}{band} {channel}");
                                    break;
                                case "DVB-S":
                                    var locator = ti.TuneRequest.Locator as DVBSLocator;
                                    var polarization = string.Empty;
                                    switch (locator.SignalPolarisation)
                                    {
                                        case Polarisation.BDA_POLARISATION_LINEAR_H:
                                            polarization = " H";
                                            break;
                                        case Polarisation.BDA_POLARISATION_LINEAR_V:
                                            polarization = " V";
                                            break;
                                        case Polarisation.BDA_POLARISATION_CIRCULAR_L:
                                            polarization = " LHC";
                                            break;
                                        case Polarisation.BDA_POLARISATION_CIRCULAR_R:
                                            polarization = " RHC";
                                            break;
                                    }

                                    tuningInfos.Add($"{(ti.IsEncrypted ? lockSymbol : string.Empty)}{ti.Frequency / 1000.0:F0}{polarization} ({ti.Sid})");
                                    break;
                                case "DVB-C":
                                case "ISDB-T":
                                case "ISDB-S":
                                case "ISDB-C":
                                    tuningInfos.Add($"{dvbTuningInfo.TuningSpace} not implemented yet. Contact me!");
                                    break;
                            }
                            break;
                        }
                        case ChannelTuningInfo channelTuningInfo:
                        {
                            var ti = channelTuningInfo;
                            switch (channelTuningInfo.TuningSpace)
                            {
                                case "ATSC":
                                    tuningInfos.Add($"{(ti.PhysicalNumber < 14 ? "VHF" : "UHF")} {ti.PhysicalNumber}");
                                    break;
                                case "Cable":
                                case "ClearQAM":
                                case "Digital Cable":
                                    tuningInfos.Add($"{(ti.IsEncrypted ? lockSymbol : string.Empty)}C{ti.PhysicalNumber}");
                                    break;
                                case "{adb10da8-5286-4318-9ccb-cbedc854f0dc}":
                                    tuningInfos.Add($"IR {ti.PhysicalNumber}{((ti.SubNumber > 0) ? "." + ti.SubNumber : null)}");
                                    break;
                                case "AuxIn1":
                                case "Antenna":
                                case "ATSCCable":
                                    tuningInfos.Add($"{channelTuningInfo.TuningSpace} not implemented yet. Contact me!");
                                    break;
                            }
                            break;
                        }
                        case StringTuningInfo stringTuningInfo:
                        {
                            var ti = stringTuningInfo;
                            switch (stringTuningInfo.TuningSpace)
                            {
                                case "dc65aa02-5cb0-4d6d-a020-68702a5b34b8":
                                    foreach (Channel channel in ti.Channels)
                                    {
                                        tuningInfos.Add("C" + channel.OriginalNumber);
                                    }
                                    break;
                                default:
                                    tuningInfos.Add($"{stringTuningInfo.TuningSpace} not implemented yet. Contact me!");
                                    break;
                            }
                            break;
                        }
                    }
                }

                if (tuningInfos.Count == 0)
                {
                    Logger.WriteInformation($"There are no tuners associated with \"{mergedChannel}\".");
                }

                // sort the hashset into a new array
                var sortedTuningInfos = tuningInfos.ToArray();
                Array.Sort(sortedTuningInfos);

                foreach (var info in sortedTuningInfos)
                {
                    if (!string.IsNullOrEmpty(ret)) ret += " + ";
                    ret += info;
                }
            }
            catch (Exception ex)
            {
                Logger.WriteError($"Exception thrown during GetAllTuningInfos() for merged channel {mergedChannel}. {ex.Message}\n{ex.StackTrace}");
            }
            return ret;
        }

        public static void CleanUpMergedChannelTuningInfos()
        {
            foreach (MergedChannel mergedChannel in WmcMergedLineup.UncachedChannels)
            {
                if (!mergedChannel.TuningInfos.Empty || mergedChannel.PrimaryChannel == null) continue;
                try
                {
                    mergedChannel.AddChannelListings(null);
                }
                catch
                {
                    Logger.WriteInformation($"Attempted to repair merged channel \"{mergedChannel}\" with no tuning infos.");
                }
            }
        }

        public static void AutoMapChannels()
        {
            // get all active channels in lineup(s) from EPG123
            var epg123Channels = GetEpg123LineupChannels();
            if (epg123Channels.Count == 0)
            {
                Logger.WriteError("There are no EPG123 listings in the database to perform any mappings.");
                return;
            }

            // get all merged channels
            var mergedChannels = new List<MergedChannel>();
            foreach (MergedChannel mergedChannel in WmcMergedLineup.UncachedChannels)
            {
                mergedChannels.Add(mergedChannel);
            }
            if (mergedChannels.Count == 0)
            {
                Logger.WriteError("There are no merged channels in the database to perform any mappings.");
                return;
            }

            // map stations to channels as needed
            foreach (var mergedChannel in mergedChannels)
            {
                var epg123Channel = epg123Channels
                    .Where(arg => arg.ChannelNumber.Number == mergedChannel.OriginalNumber)
                    .FirstOrDefault(arg => arg.ChannelNumber.SubNumber == mergedChannel.OriginalSubNumber);
                if (epg123Channel != null)
                {
                    if (mergedChannel.PrimaryChannel.Id == epg123Channel.Id) continue;
                    if (mergedChannel.PrimaryChannel.Lineup.Name.StartsWith("Scanned") || mergedChannel.PrimaryChannel.Lineup.Name.StartsWith("ZZZ123"))
                    {
                        Logger.WriteVerbose($"Matching {epg123Channel.CallSign} to channel {mergedChannel.ChannelNumber}");
                        SubscribeLineupChannel(epg123Channel.Id, mergedChannel.Id);
                    }
                    else
                    {
                        Logger.WriteVerbose($"Skipped matching {epg123Channel.CallSign} to channel {mergedChannel.ChannelNumber} due to channel already having an assigned listing.");
                    }
                }
                else if (mergedChannel.PrimaryChannel.Lineup.Name.StartsWith("EPG123") && !epg123Channels.Contains(mergedChannel.PrimaryChannel))
                {
                    Logger.WriteVerbose($"Removing {mergedChannel.PrimaryChannel.CallSign} from channel {mergedChannel.ChannelNumber}.");
                    SubscribeLineupChannel(0, mergedChannel.Id);
                }
                else if (mergedChannel.PrimaryChannel.Lineup.Name.StartsWith("Scanned") && mergedChannel.UserBlockedState < UserBlockedState.Blocked)
                {
                    SetChannelEnableState(mergedChannel.Id, false);
                }
            }

            // finish it
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
                            Logger.WriteVerbose($"Failed to associate lineup {listings.Lineup} with device {device.Name ?? "<<NULL>>"} ({(device.ScannedLineup == null ? "<<NULL>>" : device.ScannedLineup.Name)}). {ex.Message}");
                        }
                    }
                }

                if (lineupChannelId == 0 && (mergedChannel.PrimaryChannel?.Lineup?.Name?.StartsWith("Scanned") ?? false))
                {
                    // do nothing
                }
                else
                {
                    foreach (Channel secondary in mergedChannel.SecondaryChannels)
                    {
                        if (secondary.Lineup == null)
                        {
                            mergedChannel.SecondaryChannels.RemoveAllMatching(secondary);
                        }
                    }
                    mergedChannel.AddChannelListings(listings);
                }
                mergedChannel.UserBlockedState = listings == null ? UserBlockedState.Blocked : UserBlockedState.Enabled;
                mergedChannel.Update();
            }
            catch (Exception ex)
            {
                Logger.WriteInformation($"Exception thrown during SubscribeLineupChannel(). {ex.Message}\n{ex.StackTrace}");
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
                mergedChannel.UserBlockedState = enable ? UserBlockedState.Enabled : UserBlockedState.Blocked;
                mergedChannel.Update();
            }
            catch (Exception ex)
            {
                Logger.WriteInformation($"Exception thrown during SetChannelEnableState(). {ex.Message}\n{ex.StackTrace}");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static List<myLineup> GetDeviceLineupsAndIds()
        {
            var ret = new List<myLineup>();
            try
            {
                var scannedLineups = new HashSet<long>();
                foreach (Device device in new Devices(WmcObjectStore))
                {
                    if (device.ScannedLineup == null) continue;
                    if (scannedLineups.Add(device.ScannedLineup.Id))
                    {
                        ret.Add(new myLineup()
                        {
                            LineupId = device.ScannedLineup.Id,
                            Name = device.ScannedLineup.Name
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.WriteInformation($"Exception thrown during GetDeviceLineupsAndIds(). {ex.Message}\n{ex.StackTrace}");
            }
            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static List<myLineup> GetWmisLineups()
        {
            var ret = new List<myLineup>();
            try
            {
                const string legacyName = "EPG123 Lineups with Schedules Direct";
                foreach (Lineup lineup in new Lineups(WmcObjectStore))
                {
                    if (lineup.Name.Equals(legacyName))
                    {
                        foreach (Device device in new Devices(WmcObjectStore))
                        {
                            if (!device.WmisLineups.Contains(lineup)) continue;
                            device.WmisLineups.RemoveAllMatching(lineup);
                            device.Update();
                        }
                    }
                    else if (!lineup.LineupTypes.Equals("BB") &&
                             !string.IsNullOrEmpty(lineup.Name) &&
                             !lineup.Name.StartsWith("Broadband") &&
                             !lineup.Name.StartsWith("FINAL") &&
                             !lineup.Name.StartsWith("Scanned") &&
                             !lineup.Name.StartsWith("DefaultLineup") &&
                             !lineup.Name.StartsWith("Deleted") &&
                             !lineup.Name.Equals(legacyName) &&
                             !lineup.UncachedChannels.Empty &&
                             !lineup.UIds.Empty)
                    {
                        ret.Add(new myLineup()
                        {
                            Name = lineup.Name,
                            LineupId = lineup.Id,
                            ChannelCount = lineup.UncachedChannels.Count()
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.WriteInformation($"Exception thrown during GetWmisLineups(). {ex.Message}\n{ex.StackTrace}");
            }
            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lineupId"></param>
        /// <returns></returns>
        public static List<myLineupLvi> GetLineupChannels(long lineupId)
        {
            var ret = new List<myLineupLvi>();
            try
            {
                if (!(WmcObjectStore.Fetch(lineupId) is Lineup lineup)) return ret;
                foreach (Channel channel in lineup.UncachedChannels)
                {
                    ret.Add(new myLineupLvi(channel));
                }
            }
            catch (Exception ex)
            {
                Logger.WriteInformation($"Exception thrown during GetLineupChannelTags(). {ex.Message}\n{ex.StackTrace}");
            }
            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static List<Channel> GetEpg123LineupChannels()
        {
            var ret = new List<Channel>();
            try
            {
                foreach (var lineup in from myLineup in GetWmisLineups()
                    where myLineup.Name.StartsWith("EPG123") || myLineup.Name.StartsWith("HDHR2MXF")
                    select WmcObjectStore.Fetch(myLineup.LineupId) as Lineup)
                {
                    ret.AddRange(lineup.UncachedChannels.ToList());
                }
            }
            catch (Exception ex)
            {
                Logger.WriteInformation($"Exception thrown during GetEpg123LineupChannels(). {ex.Message}\n{ex.StackTrace}");
            }
            return ret;
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
                Logger.WriteInformation($"Exception thrown during DetermineRecordingsInProgress(). {ex.Message}\n{ex.StackTrace}");
            }
            return false;
        }
    }

    public class myLineup
    {
        public long LineupId { get; set; }

        public int ChannelCount { get; set; }

        public string Name { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }

    public class myLineupLvi : ListViewItem
    {
        public long ChannelId { get; private set; }

        public string Callsign { get; private set; }

        public string Number { get; private set; }

        public myLineupLvi(Channel channel) : base(new string[3])
        {
            ChannelId = channel.Id;
            SubItems[0].Text = Callsign = channel.CallSign;
            SubItems[1].Text = Number = channel.ChannelNumber.ToString();
            SubItems[2].Text = channel.Service.Name;
        }
    }

    public class myChannelLvi : ListViewItem
    {
        public long ChannelId { get; private set; }

        public HashSet<long> ScannedLineupIds { get; private set; }

        public bool Enabled { get; private set; }

        private bool Custom { get; set; } = true;

        public string Callsign { get; private set; }

        public string CustomCallsign { get; private set; }

        public string Number { get; private set; }

        public string CustomNumber { get; private set; }

        private MergedChannel MergedChannel { get; set; }

        public myChannelLvi(MergedChannel channel) : base(new string[7])
        {
            MergedChannel = channel;
            MergedChannel.Updated += Channel_Updated;

            ChannelId = MergedChannel.Id;
            UseItemStyleForSubItems = false;
            PopulateMergedChannelItems();
        }

        public void RemoveDelegate()
        {
            MergedChannel.Refresh();
            MergedChannel.Updated -= Channel_Updated;
            MergedChannel = null;
        }

        private void Channel_Updated(object sender, StoredObjectEventArgs e)
        {
            if (ListView != null && ListView.InvokeRequired)
            {
                try
                {
                    ListView?.Invoke(new Action(delegate
                    {
                        ((myChannelLvi)ListView?.Items[Index]).PopulateMergedChannelItems();
                        ((myChannelLvi)ListView?.Items[Index]).ShowCustomLabels(Custom);
                        ListView?.Invalidate(Bounds);
                    }));
                }
                catch
                {
                    // do nothing
                }
            }
            else
            {
                PopulateMergedChannelItems();
                ShowCustomLabels(Custom);
            }
        }

        public void ShowCustomLabels(bool set)
        {
            if (Custom == set) return;
            Custom = set;
            SubItems[0].Text = Custom ? CustomCallsign ?? Callsign : Callsign;
            SubItems[1].Text = Custom ? CustomNumber ?? Number : Number;
        }

        public void PopulateMergedChannelItems()
        {
            MergedChannel.Refresh();
            var scanned = MergedChannel.PrimaryChannel.Lineup?.Name?.StartsWith("Scanned") ?? false;

            // set callsign and backcolor
            Callsign = MergedChannel.PrimaryChannel.CallSign;
            CustomCallsign = MergedChannel.HasUserSpecifiedCallSign ? MergedChannel.CallSign : null;
            SubItems[0].Text = Custom ? CustomCallsign ?? Callsign : Callsign;
            SubItems[0].BackColor = MergedChannel.HasUserSpecifiedCallSign ? Color.Pink : SystemColors.Window;

            // set number and backcolor
            Number = $"{MergedChannel.OriginalNumber}{(MergedChannel.OriginalSubNumber > 0 ? $".{MergedChannel.OriginalSubNumber}" : "")}";
            CustomNumber = MergedChannel.HasUserSpecifiedNumber || MergedChannel.HasUserSpecifiedSubNumber ? $"{MergedChannel.Number}{(MergedChannel.SubNumber > 0 ? $".{MergedChannel.SubNumber}" : "")}" : null;
            SubItems[1].Text = Custom ? CustomNumber ?? Number : Number;
            SubItems[1].BackColor = MergedChannel.HasUserSpecifiedNumber || MergedChannel.HasUserSpecifiedSubNumber ? Color.Pink : SystemColors.Window;

            // set service name, lineup name, and guide end time
            SubItems[2].Text = !scanned ? MergedChannel.Service?.Name : "";
            SubItems[3].Text = !scanned ? MergedChannel.PrimaryChannel.Lineup?.Name : "";
            SubItems[6].Text = !scanned ? MergedChannel.Service?.ScheduleEndTime.ToLocalTime().ToString() : "";

            // set scanned sources and tuning info
            ScannedLineupIds = WmcStore.GetAllScannedSourcesForChannel(MergedChannel);
            if (ScannedLineupIds.Count > 0)
            {
                var names = new HashSet<string>();
                foreach (var name in ScannedLineupIds.Select(id =>
                    ((Lineup) WmcStore.WmcObjectStore.Fetch(id)).Name.Remove(0, 9)))
                {
                    names.Add(name.Remove(name.Length - 1));
                }

                var text = string.Empty;
                foreach (var name in names)
                {
                    if (!string.IsNullOrEmpty(text)) text += " + ";
                    text += name;
                }
                SubItems[4].Text = text;
            }
            SubItems[5].Text = WmcStore.GetAllTuningInfos(MergedChannel);

            // set checkbox
            Checked = Enabled = MergedChannel.UserBlockedState <= UserBlockedState.Enabled;
        }
    }
}