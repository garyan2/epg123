using GaRyan2.Utilities;
using Microsoft.MediaCenter.Guide;
using Microsoft.MediaCenter.TV.Tuning;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GaRyan2.WmcUtilities
{
    public static partial class WmcStore
    {
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
                Logger.WriteError($"Exception thrown during SetChannelCustomCallsign(). {ex}");
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
                Logger.WriteError($"Exception thrown during SetChannelCustomNumber(). {ex}");
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
                Logger.WriteError($"Exception thrown during AddUserChannel(). {ex}");
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
                Logger.WriteError($"Exception thrown during DeleteChannel(). {ex}");
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
                Logger.WriteError($"Exception thrown during GetAllScannedSourcesForChannel() for merged channel {mergedChannel}. {ex}");
            }
            return ret;
        }

        public static string GetAllTuningInfos(Channel mergedChannel)
        {
            string ret = null;
            try
            {
                // build tuning info
                var tuningInfos = new HashSet<string>();
                foreach (TuningInfo tuningInfo in mergedChannel.TuningInfos.Cast<TuningInfo>())
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

                    switch (tuningInfo)
                    {
                        case DvbTuningInfo dvbTuningInfo:
                            {
                                switch (dvbTuningInfo.TuningSpace)
                                {
                                    case "DVB-T":
                                        if (tuningInfo.IsSuggestedBlocked) continue;
                                        tuningInfos.Add($"{dvbTuningInfo.Frequency / 1000.0:F3} ({dvbTuningInfo.Sid})");
                                        break;
                                    case "DVB-S":
                                        var locator = dvbTuningInfo.TuneRequest.Locator as DVBSLocator;
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
                                        tuningInfos.Add($"{locator.OrbitalPosition}:{dvbTuningInfo.Frequency / 1000.0:F0}{polarization} ({dvbTuningInfo.Sid})");
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
                                switch (channelTuningInfo.TuningSpace)
                                {
                                    case "ATSC":
                                        tuningInfos.Add($"{(channelTuningInfo.PhysicalNumber < 14 ? "VHF" : "UHF")} {channelTuningInfo.PhysicalNumber}");
                                        break;
                                    case "Cable":
                                    case "ClearQAM":
                                    case "Digital Cable":
                                        tuningInfos.Add($"C{channelTuningInfo.PhysicalNumber}");
                                        break;
                                    case "{adb10da8-5286-4318-9ccb-cbedc854f0dc}":
                                        tuningInfos.Add($"IR {channelTuningInfo.PhysicalNumber}{(channelTuningInfo.SubNumber > 0 ? $".{channelTuningInfo.SubNumber}" : null)}");
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
                                switch (stringTuningInfo.TuningSpace)
                                {
                                    case "dc65aa02-5cb0-4d6d-a020-68702a5b34b8": // Hauppauge HD PVR
                                        foreach (Channel channel in stringTuningInfo.Channels)
                                        {
                                            tuningInfos.Add("C" + channel.OriginalNumber);
                                        }
                                        break;
                                    case "DVB-T": // DVBLink
                                    case "DVB-S":
                                    case "DVB-C":
                                        foreach (Channel channel in stringTuningInfo.Channels)
                                        {
                                            tuningInfos.Add($"C{channel.OriginalNumber}{(channel.SubNumber > 0 ? $".{channel.SubNumber}" : string.Empty)}");
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
                Logger.WriteError($"Exception thrown during GetAllTuningInfos() for merged channel {mergedChannel}. {ex}");
            }
            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lineupId"></param>
        /// <returns></returns>
        public static List<Channel> GetLineupChannels(long lineupId)
        {
            var ret = new List<Channel>();
            try
            {
                if (!(WmcObjectStore.Fetch(lineupId) is Lineup lineup)) return ret;
                ret = lineup.UncachedChannels.ToList();
            }
            catch (Exception ex)
            {
                Logger.WriteInformation($"Exception thrown during GetLineupChannels(). {ex}");
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
                    ret.AddRange(lineup.UncachedChannels);
                }
            }
            catch (Exception ex)
            {
                Logger.WriteInformation($"Exception thrown during GetEpg123LineupChannels(). {ex}");
            }
            return ret;
        }
    }
}