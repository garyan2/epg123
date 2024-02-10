using GaRyan2.Utilities;
using Microsoft.MediaCenter.Guide;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace GaRyan2.WmcUtilities
{
    public static partial class WmcStore
    {
        private static MergedLineup _mergedLineup;

        public static MergedLineup WmcMergedLineup
        {
            get
            {
                if (_mergedLineup != null) return _mergedLineup;
                using (var mergedLineups = new MergedLineups(WmcObjectStore))
                {
                    foreach (MergedLineup lineup in mergedLineups.Cast<MergedLineup>())
                    {
                        if (!lineup.UncachedChannels.Any()) continue;
                        _mergedLineup = lineup;
                        break;
                    }
                }
                return _mergedLineup;
            }
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
                foreach (Lineup lineup in new Lineups(WmcObjectStore).Cast<Lineup>())
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
                Logger.WriteInformation($"Exception thrown during ActivateEpg123LineupsInStore(). Message:{Helper.ReportExceptionMessages(ex)}");
            }
            return (lineups > 0);
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
                Logger.WriteError($"Exception thrown during UnsubscribeChannelsInLineup(). Message:{Helper.ReportExceptionMessages(ex)}");
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
                Logger.WriteError($"Exception thrown during DeleteLineup(). Message:{Helper.ReportExceptionMessages(ex)}");
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
                Logger.WriteInformation($"Exception thrown during GetDeviceLineupsAndIds(). Message:{Helper.ReportExceptionMessages(ex)}");
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
                foreach (Lineup lineup in new Lineups(WmcObjectStore).Cast<Lineup>())
                {
                    if (!lineup.LineupTypes.Equals("BB") &&
                        !string.IsNullOrEmpty(lineup.Name) &&
                        !lineup.Name.StartsWith("Broadband") &&
                        !lineup.Name.StartsWith("FINAL") &&
                        !lineup.Name.StartsWith("Scanned") &&
                        !lineup.Name.StartsWith("DefaultLineup") &&
                        !lineup.Name.StartsWith("Deleted") &&
                        !lineup.UncachedChannels.Empty &&
                        !lineup.UIds.Empty)
                    {
                        ret.Add(new myLineup()
                        {
                            Name = $"{lineup.Name} [{lineup.GetUIdValue().Substring("!MCLineup!".Length)}]",
                            LineupId = lineup.Id,
                            ChannelCount = lineup.UncachedChannels.Count()
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.WriteInformation($"Exception thrown during GetWmisLineups(). Message:{Helper.ReportExceptionMessages(ex)}");
            }
            return ret;
        }
    }

    public class myLineup
    {
        public override string ToString()
        {
            return Name;
        }

        public long LineupId { get; set; }

        public string Name { get; set; }

        public int ChannelCount { get; set; }
    }
}