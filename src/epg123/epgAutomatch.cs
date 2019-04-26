using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using Microsoft.MediaCenter.Guide;
using Microsoft.MediaCenter.Store;

namespace epg123
{
    public class epgAutomatch
    {
        epg123Tracing trace = new epg123Tracing();
        private static ObjectStore objectStore_;
        public static ObjectStore object_store
        {
            get
            {
                // Totally ripped this off from glugglug's Lineup Selector code
                // just remember to add <configuration>< startup useLegacyV2RuntimeActivationPolicy = "true" > to the App.config file
                if (objectStore_ == null)
                {
                    // Crazy hack to get administrative ObjectStore connection from this thread:
                    // https://social.msdn.microsoft.com/Forums/en-US/ea979075-f602-475d-b485-3a4f787dcb70/new-media-center-addin-x64-microsoftmediacenterguidesubscribed?forum=netfx64bit
                    byte[] bytes = Convert.FromBase64String("FAAODBUITwADRicSARc=");
                    byte[] buffer2 = Encoding.ASCII.GetBytes("Unable upgrade recording state.");
                    for (int i = 0; i != bytes.Length; i++)
                    {
                        bytes[i] = (byte)(bytes[i] ^ buffer2[i]);
                    }
                    string FriendlyName = Encoding.ASCII.GetString(bytes);
                    string clientId = Microsoft.MediaCenter.Store.ObjectStore.GetClientId(true);
                    byte[] buffer = Encoding.Unicode.GetBytes(clientId);
                    string DisplayName = Convert.ToBase64String(new SHA256Managed().ComputeHash(buffer));
                    objectStore_ = Microsoft.MediaCenter.Store.ObjectStore.Open("", FriendlyName, DisplayName, true);
                }
                return objectStore_;
            }
        }

        private static string epg123Lineup_;
        public epgAutomatch(string lineup)
        {
            epg123Lineup_ = lineup;
            makeLineupViewable();
        }

        public class ToStringComparer<T> : IComparer<T>
        {
            public int Compare(T x, T y)
            {
                return x.ToString().CompareTo(y.ToString());
            }
        }

        private static Lineup[] lineups_;
        private static int epgLineup = -1;

        public static Lineup[] GetLineups()
        {
            if (null == lineups_)
            {
                lineups_ = new Lineups(object_store).ToArray();

                for (int i = 0; i < lineups_.Length; ++i)
                {
                    if (lineups_[i].Name == epg123Lineup_)
                    {
                        epgLineup = i;
                        break;
                    }
                }
            }
            return lineups_;
        }

        public static List<Lineup> GetScannedLineups()
        {
            List<Lineup> scanned_lineups = new List<Lineup>();
            foreach (Lineup lineup in GetLineups())
                if (lineup.ScanDevices.Count() > 0) scanned_lineups.Add(lineup);
            return scanned_lineups;
        }

        public static List<Service> GetServices()
        {
            List<Service> service_list = new Services(object_store).ToList();
            service_list.Sort(new ToStringComparer<Service>());
            return service_list;
        }

        public static List<Channel> GetUserAddedChannelsForLineup(Lineup lineup)
        {
            List<Channel> user_channels = new List<Channel>();
            foreach (Channel ch in lineup.GetChannels())
            {
                if (ChannelType.UserAdded == ch.ChannelType)
                {
                    user_channels.Add(ch);
                }
            }
            return user_channels;
        }

        public bool makeLineupViewable()
        {
            Lineup[] lineups = GetLineups();
            if (epgLineup == -1) return false;

            if (string.IsNullOrEmpty(lineups[epgLineup].LineupTypes))
            {
                lineups[epgLineup].LineupTypes = "WMIS";
                //lineups[epgLineup].AlternateLineupTypes = "ATSC;QAM;CAB;CABd;SAT;SATd;DVBt;DVBs;DVBc;ISDBs;ISDBc";
                lineups[epgLineup].Update();
            }
            return true;
        }

        public void matchLineups()
        {
            Channel[] epgValidChannels = lineups_[epgLineup].GetChannels();
            using (MergedLineups merged_lineups = new MergedLineups(object_store))
            {
                foreach (MergedLineup merged_lineup in merged_lineups)
                {
                    foreach (Channel ch in merged_lineup.GetChannels())
                    {
                        Channel epgChannel = lineups_[epgLineup].GetChannelFromNumber(ch.OriginalNumber, ch.OriginalSubNumber);
                        if ((epgChannel != null) && channelsContain(ref epgValidChannels, ref epgChannel))
                        {
                            if (!ch.Service.IsSameAs(epgChannel.Service))
                            {
                                trace.WriteTraceLog(string.Format("[ INFO] Matching {0} to channel {1}", epgChannel.CallSign, ch.ChannelNumber));
                                ch.UserBlockedState = UserBlockedState.Enabled;
                                ch.Service = epgChannel.Service;
                                ch.Update();
                            }
                        }
                        else if (epgChannel != null) 
                        {
                            trace.WriteTraceLog(string.Format("[ INFO] Disabling {0} on channel {1}", epgChannel.CallSign, ch.ChannelNumber));
                            ch.UserBlockedState = UserBlockedState.Blocked;
                            ch.Update();
                        }
                        else if ((int)ch.UserBlockedState < (int)UserBlockedState.Blocked)
                        {
                            ch.UserBlockedState = UserBlockedState.Blocked;
                            ch.Update();
                        }
                    }
                    merged_lineup.Update();
                }
            }
        }

        private bool channelsContain(ref Channel[] arr, ref Channel ch)
        {
            for (int i = 0; i < arr.Length; ++i)
            {
                if (arr[i].IsSameAs(ch))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
