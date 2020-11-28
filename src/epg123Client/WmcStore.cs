using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;
using Microsoft.MediaCenter.Guide;
using Microsoft.MediaCenter.Store;
using Microsoft.MediaCenter.TV.Tuning;
using epg123;

namespace epg123Client
{
    static class WmcStore
    {
        private static ObjectStore WmcObjectStore_;
        private static MergedLineup WmcMergedLineup_;
        public static bool StoreExpired;

        public static ObjectStore WmcObjectStore
        {
            get
            {
                if (WmcObjectStore_ == null || StoreExpired)
                {
                    SHA256Managed sha256Man = new SHA256Managed();
                    ObjectStore.FriendlyName = @"Anonymous!User";
                    ObjectStore.DisplayName = Convert.ToBase64String(sha256Man.ComputeHash(Encoding.Unicode.GetBytes(ObjectStore.GetClientId(true))));
                    WmcObjectStore_ = ObjectStore.AddObjectStoreReference();
                    StoreExpired = false;

                    WmcObjectStore_.StoreExpired += WmcObjectStore_StoreExpired;
                }
                return WmcObjectStore_;
            }
        }

        private static void WmcObjectStore_StoreExpired(object sender, StoredObjectEventArgs e)
        {
            Logger.WriteError("A database recovery has been detected. Attempting to open new database.");
            Close();
            StoreExpired = true;
            WmcObjectStore_.StoreExpired -= WmcObjectStore_StoreExpired;
            if (WmcObjectStore != null)
            {
                Logger.WriteInformation("Successfully opened new store.");
            }
        }

        public static MergedLineup WmcMergedLineup
        {
            get
            {
                if (WmcMergedLineup_ == null)
                {
                    using (MergedLineups mergedLineups = new MergedLineups(WmcObjectStore))
                    {
                        foreach (MergedLineup lineup in mergedLineups)
                        {
                            if (lineup.UncachedChannels.Count() > 0)
                            {
                                WmcMergedLineup_ = lineup;
                                break;
                            }
                        }
                    }
                }
                return WmcMergedLineup_;
            }
        }

        /// <summary>
        /// Closes the WMC ObjectStore with the option to dispose the ObjectStore
        /// </summary>
        /// <param name="dispose">true = dispose ObjectStore</param>
        public static void Close(bool dispose = false)
        {
            if (WmcObjectStore_ != null)
            {
                WmcObjectStore_.StoreExpired -= WmcObjectStore_StoreExpired;
                ObjectStore.ReleaseObjectStoreReference();
            }
            WmcMergedLineup_ = null;

            try
            {
                if (dispose)
                {
                    if (WmcObjectStore_ != null) WmcObjectStore_.Dispose();
                }
            }
            catch (Exception ex)
            {
                Logger.WriteInformation($"Exception thrown while trying to dispose of ObjectStore. {ex.Message}\n{ex.StackTrace}");
            }
            WmcObjectStore_ = null;
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
            int lineups = 0;
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
                MergedChannel channel = WmcObjectStore.Fetch(id) as MergedChannel;
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
                MergedChannel channel = WmcObjectStore.Fetch(id) as MergedChannel;
                if (!string.IsNullOrEmpty(number))
                {
                    string[] digits = number.Split('.');
                    if (digits.Length > 0) channel.Number = int.Parse(digits[0]);
                    if (digits.Length > 1) channel.SubNumber = int.Parse(digits[1]);
                    else channel.SubNumber = 0;
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
        /// <param name="channelId"></param>
        public static void DeleteChannel(long channelId)
        {
            try
            {
                Channel channel = WmcObjectStore.Fetch(channelId) as Channel;
                WmcMergedLineup.RemoveChannel(channel);
                WmcMergedLineup.Update();
                channel.Uncache();
            }
            catch (Exception ex)
            {
                Logger.WriteError($"Exception thrown during DeleteChannel(). {ex.Message}\n{ex.StackTrace}");
            }
        }

        /// <summary>
        /// Clears all schedule entries in a service
        /// </summary>
        /// <param name="mergedChannelId"></param>
        public static void ClearServiceScheduleEntries(long mergedChannelId)
        {
            try
            {
                MergedChannel channel = WmcObjectStore.Fetch(mergedChannelId) as MergedChannel;
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
                foreach (MergedChannel mergedChannel in WmcMergedLineup.UncachedChannels)
                {
                    if ((mergedChannel.PrimaryChannel?.Lineup?.Id ?? 0).Equals(lineupId))
                    {
                        SubscribeLineupChannel(0, mergedChannel.Id);
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
                Lineup lineup = WmcObjectStore.Fetch(lineupId) as Lineup;
                foreach (Channel channel in lineup.GetChannels())
                {
                    lineup.RemoveChannel(channel);
                }

                // remove linup from device(s)
                foreach (Device device in new Devices(WmcObjectStore))
                {
                    if (device.WmisLineups.Contains(lineup))
                    {
                        device.WmisLineups.RemoveAllMatching(lineup);
                        device.Update();
                    }
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
            HashSet<long> ret = new HashSet<long>();
            try
            {
                if (mergedChannel.PrimaryChannel.Lineup.Name.StartsWith("Scanned"))
                {
                    ret.Add(mergedChannel.PrimaryChannel.Lineup.Id);
                }
                foreach (Channel channel in mergedChannel.SecondaryChannels)
                {
                    if ((channel.Lineup?.Name ?? "").StartsWith("Scanned"))
                    {
                        ret.Add(channel.Lineup.Id);
                    }
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
            return GetAllTuningInfos((Channel)mergedChannel);
        }
        public static string GetAllTuningInfos(Channel mergedChannel)
        {
            string ret = null;
            try
            {
                // build tuning info
                HashSet<string> tuningInfos = new HashSet<string>();
                foreach (TuningInfo tuningInfo in mergedChannel.TuningInfos)
                {
                    // handle any overrides to tuninginfo
                    // assumes that tuninginfo is valid unless an override explicitly says otherwise
                    bool shown = true;
                    if (!tuningInfo.Overrides.Empty)
                    {
                        foreach (TuningInfoOverride tuningInfoOverride in tuningInfo.Overrides)
                        {
                            if ((tuningInfoOverride.Channel?.Id ?? 0) != mergedChannel.Id) continue;
                            else if (!tuningInfoOverride.IsLatestVersion) continue;
                            else if ((tuningInfoOverride.Channel.ChannelType == ChannelType.UserHidden) ||
                                     (tuningInfoOverride.IsUserOverride && tuningInfoOverride.UserBlockedState == UserBlockedState.Disabled))
                            {
                                shown = false;
                                break;
                            }
                        }
                    }
                    if (!shown) continue;

                    // unfortunately the lock emoji didn't come into play until Unicode 6.0.0
                    // Windows 7 uses Unicode 5.x, it will just show an open block
                    string lockSymbol = "\uD83D\uDD12";
                    if (tuningInfo is DvbTuningInfo)
                    {
                        DvbTuningInfo ti = tuningInfo as DvbTuningInfo;
                        switch (tuningInfo.TuningSpace)
                        {
                            case "DVB-T":
                                // formula to convert channel (n) to frequency (fc) is fc = 8n + 306 (in MHz)
                                // offset is -167KHz, 0Hz, +167KHz => int offset = ti.Frequency - (channel * 8000) - 306000;
                                int channel = (ti.Frequency - 305833) / 8000;
                                tuningInfos.Add(string.Format("{0}UHF C{1}", ti.IsEncrypted ? lockSymbol : string.Empty, channel));
                                break;
                            case "DVB-S":
                                DVBSLocator locator = ti.TuneRequest.Locator as DVBSLocator;
                                string polarization = string.Empty;
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
                                    default:
                                        break;
                                }
                                tuningInfos.Add(string.Format("{0}{1:F0}{2} ({3})", ti.IsEncrypted ? lockSymbol : string.Empty, ti.Frequency / 1000.0, polarization, ti.Sid));
                                break;
                            case "DVB-C":
                            case "ISDB-T":
                            case "ISDB-S":
                            case "ISDB-C":
                                tuningInfos.Add($"{tuningInfo.TuningSpace} not implemented yet. Contact me!");
                                break;
                            default:
                                break;
                        }
                    }
                    else if (tuningInfo is ChannelTuningInfo)
                    {
                        ChannelTuningInfo ti = tuningInfo as ChannelTuningInfo;
                        switch (tuningInfo.TuningSpace)
                        {
                            case "ATSC":
                                tuningInfos.Add(string.Format("{0} {1}{2}",
                                    (ti.PhysicalNumber < 14) ? "VHF" : "UHF", ti.PhysicalNumber, (ti.SubNumber > 0) ? "." + ti.SubNumber.ToString() : null));
                                break;
                            case "Cable":
                            case "ClearQAM":
                            case "Digital Cable":
                                tuningInfos.Add(string.Format("{0}C{1}{2}", ti.IsEncrypted ? lockSymbol : string.Empty,
                                    ti.PhysicalNumber, (ti.SubNumber > 0) ? "." + ti.SubNumber.ToString() : null));
                                break;
                            case "{adb10da8-5286-4318-9ccb-cbedc854f0dc}":
                                tuningInfos.Add(string.Format("IR {0}{1}",
                                    ti.PhysicalNumber, (ti.SubNumber > 0) ? "." + ti.SubNumber.ToString() : null));
                                break;
                            case "AuxIn1":
                            case "Antenna":
                            case "ATSCCable":
                                tuningInfos.Add($"{tuningInfo.TuningSpace} not implemented yet. Contact me!");
                                break;
                            default:
                                break;
                        }
                    }
                    else if (tuningInfo is StringTuningInfo)
                    {
                        StringTuningInfo ti = tuningInfo as StringTuningInfo;
                        switch (tuningInfo.TuningSpace)
                        {
                            case "dc65aa02-5cb0-4d6d-a020-68702a5b34b8":
                                foreach (Channel channel in ti.Channels)
                                {
                                    tuningInfos.Add("C" + channel.OriginalNumber.ToString());
                                }
                                break;
                            default:
                                tuningInfos.Add($"{tuningInfo.TuningSpace} not implemented yet. Contact me!");
                                break;
                        }
                    }
                }

                if (tuningInfos.Count == 0)
                {
                    Logger.WriteInformation($"There are no tuners associated with \"{mergedChannel}\".");
                }

                // sort the hashset into a new array
                string[] sortedTuningInfos = tuningInfos.ToArray();
                Array.Sort(sortedTuningInfos);

                foreach (string info in sortedTuningInfos)
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

        public static void AutoMapChannels()
        {
            // get all active channels in lineup(s) from EPG123
            List<Channel> epg123Channels = GetEpg123LineupChannels();
            if (epg123Channels.Count == 0)
            {
                Logger.WriteError("There are no EPG123 listings in the database to perform any mappings.");
                return;
            }

            // get all merged channels
            List<MergedChannel> mergedChannels = new List<MergedChannel>();
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
            foreach (MergedChannel mergedChannel in mergedChannels)
            {
                Channel epg123Channel = epg123Channels.Where(arg => arg.ChannelNumber.Number == mergedChannel.OriginalNumber)
                                                      .Where(arg => arg.ChannelNumber.SubNumber == mergedChannel.OriginalSubNumber)
                                                      .FirstOrDefault();
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
                MergedChannel mergedChannel = WmcObjectStore.Fetch(mergedChannelId) as MergedChannel;
                if (lineupChannelId > 0)
                {
                    // grab the listings
                    listings = WmcObjectStore.Fetch(lineupChannelId) as Channel;

                    // add this channel lineup to the device group if necessary
                    foreach (Device device in mergedChannel.Lineup.DeviceGroup.Devices)
                    {
                        try
                        {
                            if (!device.Name.ToLower().Contains("delete") &&
                                (device.ScannedLineup != null) && device.ScannedLineup.IsSameAs(mergedChannel.PrimaryChannel.Lineup) &&
                                ((device.WmisLineups == null) || !device.WmisLineups.Contains(listings.Lineup)))
                            {
                                device.SubscribeToWmisLineup(listings.Lineup);
                                device.Update();
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.WriteVerbose(string.Format("Failed to associate lineup {0} with device {1} ({2}). {3}", listings.Lineup,
                                                                device.Name ?? "NULL", (device.ScannedLineup == null) ? "NULL" : device.ScannedLineup.Name, ex.Message));
                        }
                    }
                }

                if (lineupChannelId == 0 && mergedChannel.PrimaryChannel.Lineup.Name.StartsWith("Scanned"))
                {
                    // do nothing
                }
                else
                {
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
                MergedChannel mergedChannel = WmcObjectStore.Fetch(mergedChannelId) as MergedChannel;
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
            List<myLineup> ret = new List<myLineup>();
            try
            {
                HashSet<long> scannedLineups = new HashSet<long>();
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
            List<myLineup> ret = new List<myLineup>();
            try
            {
                string legacyName = "EPG123 Lineups with Schedules Direct";
                foreach (Lineup lineup in new Lineups(WmcObjectStore))
                {
                    if (lineup.Name.Equals(legacyName))
                    {
                        foreach (Device device in new Devices(WmcObjectStore))
                        {
                            if (device.WmisLineups.Contains(lineup))
                            {
                                device.WmisLineups.RemoveAllMatching(lineup);
                                device.Update();
                            }
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
            List<myLineupLvi> ret = new List<myLineupLvi>();
            try
            {
                Lineup lineup = WmcObjectStore.Fetch(lineupId) as Lineup;
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
            List<Channel> ret = new List<Channel>();
            try
            {
                foreach (myLineup myLineup in GetWmisLineups())
                {
                    if (!myLineup.Name.StartsWith("EPG123") && !myLineup.Name.StartsWith("HDHR2MXF")) continue;
                    Lineup lineup = WmcObjectStore.Fetch(myLineup.LineupId) as Lineup;
                    ret.AddRange(lineup.UncachedChannels.ToList());
                }
            }
            catch (Exception ex)
            {
                Logger.WriteInformation($"Exception thrown during GetEpg123LineupChannels(). {ex.Message}\n{ex.StackTrace}");
            }
            return ret;
        }
    }

    public class myLineup
    {
        public long LineupId { get; set; }

        public int ChannelCount { get; set; }

        public string Name { get; set; }

        public myLineup() { }

        public override string ToString()
        {
            return Name;
        }
    }

    public class myLineupLvi : ListViewItem
    {
        public long ChannelId { get; private set; }

        public string callsign { get; private set; }

        public string number { get; private set; }

        public myLineupLvi(Channel channel) : base(new string[3])
        {
            this.ChannelId = channel.Id;
            base.SubItems[0].Text = this.callsign = channel.CallSign;
            base.SubItems[1].Text = this.number = channel.ChannelNumber.ToString();
            base.SubItems[2].Text = channel.Service.Name;
        }
    }

    public class myChannelLvi : ListViewItem
    {
        public long ChannelId { get; private set; }

        public HashSet<long> ScannedLineupIds { get; private set; }

        public bool Enabled { get; private set; }

        private bool custom { get; set; } = true;

        public string callsign { get; private set; }

        public string customCallsign { get; private set; }

        public string number { get; private set; }

        public string customNumber { get; private set; }

        private MergedChannel MergedChannel { get; set; }

        public myChannelLvi(MergedChannel channel) : base(new string[7])
        {
            MergedChannel = channel;
            MergedChannel.Updated += Channel_Updated;

            this.ChannelId = MergedChannel.Id;
            base.UseItemStyleForSubItems = false;
            PopulateMergedChannelItems();
        }

        public void RemoveDelegate()
        {
            this.MergedChannel.Refresh();
            this.MergedChannel.Updated -= Channel_Updated;
            this.MergedChannel = null;
        }

        public myChannelLvi(Channel channel) : base(new string[3])
        {
            this.ChannelId = channel.Id;
            base.SubItems[0].Text = this.callsign = channel.CallSign;
            base.SubItems[1].Text = this.number = channel.ChannelNumber.ToString();
            base.SubItems[2].Text = channel.Service.Name;
        }

        private void Channel_Updated(object sender, StoredObjectEventArgs e)
        {
            if (this.ListView != null && this.ListView.InvokeRequired)
            {
                ListView.Invoke(new Action(delegate ()
                {
                    ((myChannelLvi)this.ListView.Items[this.Index]).PopulateMergedChannelItems();
                    ((myChannelLvi)this.ListView.Items[this.Index]).ShowCustomLabels(custom);
                    this.ListView.Invalidate(this.Bounds);
                }));
            }
            else
            {
                PopulateMergedChannelItems();
                ShowCustomLabels(custom);
            }
        }

        public void ShowCustomLabels(bool set)
        {
            if (custom != set)
            {
                custom = set;
                base.SubItems[0].Text = custom ? (customCallsign ?? callsign) : callsign;
                base.SubItems[1].Text = custom ? (customNumber ?? number) : number;
            }
        }

        public void PopulateMergedChannelItems()
        {
            MergedChannel.Refresh();
            bool scanned = MergedChannel.PrimaryChannel.Lineup.Name.StartsWith("Scanned");

            // set callsign and backcolor
            this.callsign = MergedChannel.PrimaryChannel.CallSign;
            this.customCallsign = MergedChannel.HasUserSpecifiedCallSign ? MergedChannel.CallSign : null;
            base.SubItems[0].Text = this.custom ? this.customCallsign ?? this.callsign : this.callsign;
            base.SubItems[0].BackColor = MergedChannel.HasUserSpecifiedCallSign ? Color.Pink : SystemColors.Window;

            // set number and backcolor
            this.number = string.Format("{0}{1}", MergedChannel.OriginalNumber, MergedChannel.OriginalSubNumber > 0 ? $".{MergedChannel.OriginalSubNumber}" : "");
            this.customNumber = (MergedChannel.HasUserSpecifiedNumber || MergedChannel.HasUserSpecifiedSubNumber) ? string.Format("{0}{1}", MergedChannel.Number, MergedChannel.SubNumber > 0 ? $".{MergedChannel.SubNumber}" : "") : null;
            base.SubItems[1].Text = this.custom ? this.customNumber ?? this.number : this.number;
            base.SubItems[1].BackColor = (MergedChannel.HasUserSpecifiedNumber || MergedChannel.HasUserSpecifiedSubNumber) ? Color.Pink : SystemColors.Window;

            // set service name, lineup name, and guide end time
            base.SubItems[2].Text = !scanned ? MergedChannel.Service?.Name : "";
            base.SubItems[3].Text = !scanned ? MergedChannel.PrimaryChannel.Lineup?.Name : "";
            base.SubItems[6].Text = !scanned ? MergedChannel.Service?.ScheduleEndTime.ToLocalTime().ToString() : "";

            // set scanned sources and tuning info
            this.ScannedLineupIds = WmcStore.GetAllScannedSourcesForChannel(MergedChannel);
            if (this.ScannedLineupIds.Count > 0)
            {
                HashSet<string> names = new HashSet<string>();
                foreach (long id in this.ScannedLineupIds)
                {
                    string name = ((Lineup)WmcStore.WmcObjectStore.Fetch(id)).Name.Remove(0, 9);
                    names.Add(name.Remove(name.Length - 1));
                }

                string text = string.Empty;
                foreach (string name in names)
                {
                    if (!string.IsNullOrEmpty(text)) text += " + ";
                    text += name;
                }
                base.SubItems[4].Text = text;
            }
            base.SubItems[5].Text = WmcStore.GetAllTuningInfos(MergedChannel);

            // set checkbox
            base.Checked = this.Enabled = MergedChannel.UserBlockedState <= UserBlockedState.Enabled;
        }
    }
}