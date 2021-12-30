using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using epg123.MxfXml;
using epg123.SchedulesDirect;

namespace epg123.sdJson2mxf
{
    internal static partial class sdJson2Mxf
    {
        private static readonly HashSet<string> IncludedStations = new HashSet<string>();
        private static readonly HashSet<string> ExcludedStations = new HashSet<string>();
        public static int MissingStations;
        private static StationChannelMap customMap;

        private static CustomLineup customLineup;
        private static readonly HashSet<string> CustomStations = new HashSet<string>();
        private static readonly Dictionary<string, LineupStation> AllStations = new Dictionary<string, LineupStation>();

        public static System.ComponentModel.BackgroundWorker BackgroundDownloader;
        public static List<KeyValuePair<MxfService, string[]>> StationLogosToDownload = new List<KeyValuePair<MxfService, string[]>>();
        public static volatile bool StationLogosDownloadComplete = true;

        private static bool BuildLineupServices()
        {
            // query what lineups client is subscribed to
            var clientLineups = SdApi.GetSubscribedLineups();
            if (clientLineups == null) return false;

            // determine if there are custom lineups to consider
            if (File.Exists(Helper.Epg123CustomLineupsXmlPath))
            {
                CustomLineups customLineups;
                using (var stream = new StreamReader(Helper.Epg123CustomLineupsXmlPath, Encoding.Default))
                {
                    var serializer = new XmlSerializer(typeof(CustomLineups));
                    TextReader reader = new StringReader(stream.ReadToEnd());
                    customLineups = (CustomLineups)serializer.Deserialize(reader);
                    reader.Close();
                }

                foreach (var lineup in customLineups.CustomLineup.Where(lineup => config.IncludedLineup.Contains(lineup.Lineup)))
                {
                    customLineup = lineup;

                    clientLineups.Lineups.Add(new SubscribedLineup()
                    {
                        Lineup = lineup.Lineup,
                        Name = lineup.Name,
                        Transport = string.Empty,
                        Location = lineup.Location,
                        Uri = "CUSTOM",
                        IsDeleted = false
                    });

                    customMap = new StationChannelMap
                    {
                        Map = new List<LineupChannel>(),
                        Stations = new List<LineupStation>(),
                        Metadata = new LineupMetadata() {Lineup = lineup.Lineup}
                    };
                }
            }

            // reset counters
            processedObjects = 0; totalObjects = clientLineups.Lineups.Count;
            ReportProgress();

            // process lineups
            Logger.WriteMessage($"Entering BuildLineupServices() for {clientLineups.Lineups.Count} lineups.");
            foreach (var clientLineup in clientLineups.Lineups)
            {
                var flagCustom = (!string.IsNullOrEmpty(clientLineup.Uri) && clientLineup.Uri.Equals("CUSTOM"));
                ++processedObjects; ReportProgress();

                // request the lineup's station maps
                StationChannelMap lineupMap = null;
                if (!flagCustom)
                {
                    lineupMap = SdApi.GetStationChannelMap(clientLineup.Lineup);
                    if (lineupMap == null) continue;

                    foreach (var station in lineupMap.Stations.Where(station => !AllStations.ContainsKey(station.StationId)))
                    {
                        AllStations.Add(station.StationId, station);
                    }
                }

                if (!config.IncludedLineup.Contains(clientLineup.Lineup))
                {
                    Logger.WriteVerbose($"Subscribed lineup {clientLineup.Lineup} has been EXCLUDED from download and processing.");
                    continue;
                }
                if (clientLineup.IsDeleted)
                {
                    Logger.WriteWarning($"Subscribed lineup {clientLineup.Lineup} has been DELETED at the headend.");
                    continue;
                }
                if (flagCustom)
                {
                    foreach (var station in customLineup.Station)
                    {
                        if (AllStations.TryGetValue(station.StationId, out var lineupStation))
                        {
                            customMap.Map.Add(new LineupChannel()
                            {
                                StationId = station.StationId,
                                AtscMajor = station.Number,
                                AtscMinor = station.Subnumber
                            });
                            CustomStations.Add(station.StationId);
                            customMap.Stations.Add(lineupStation);
                        }
                        else if (!string.IsNullOrEmpty(station.Alternate) && AllStations.TryGetValue(station.Alternate, out lineupStation))
                        {
                            customMap.Map.Add(new LineupChannel()
                            {
                                StationId = station.Alternate,
                                AtscMajor = station.Number,
                                AtscMinor = station.Subnumber
                            });
                            CustomStations.Add(station.Alternate);
                            customMap.Stations.Add(lineupStation);
                        }
                    }
                    lineupMap = customMap;
                    Logger.WriteVerbose($"Successfully retrieved the station mapping for lineup {clientLineup.Lineup}.");
                }
                if (lineupMap == null) return false;

                var lineupIndex = SdMxf.With.Lineups.Count;
                SdMxf.With.Lineups.Add(new MxfLineup
                {
                    Index = lineupIndex + 1,
                    LineupId = clientLineup.Lineup,
                    Name = $"EPG123 {clientLineup.Name} ({clientLineup.Location})"
                });

                // build the services and lineup
                foreach (var station in lineupMap.Stations)
                {
                    // check if station should be downloaded and processed
                    if (!flagCustom)
                    {
                        if ((station == null) || (ExcludedStations.Contains(station.StationId) && !CustomStations.Contains(station.StationId))) continue;
                        if (!IncludedStations.Contains(station.StationId) && !config.AutoAddNew)
                        {
                            Logger.WriteWarning($"**** Lineup {clientLineup.Name} ({clientLineup.Location}) has added station {station.StationId} ({station.Callsign}). ****");
                            continue;
                        }
                    }

                    // build the service if necessary
                    var mxfService = SdMxf.GetService(station.StationId);
                    if (string.IsNullOrEmpty(mxfService.CallSign))
                    {
                        // instantiate stationLogo and override uid
                        StationImage stationLogo = null;
                        mxfService.UidOverride = $"EPG123_{station.StationId}";

                        // determine station name for ATSC stations
                        var atsc = false;
                        var names = station.Name.Replace("-", "").Split(' ');
                        if (!string.IsNullOrEmpty(station.Affiliate) && names.Length == 2 && names[0] == station.Callsign && $"({names[0]})" == $"{names[1]}")
                        {
                            atsc = true;
                        }

                        // add callsign and station name
                        mxfService.CallSign = CheckCustomCallsign(station.StationId) ?? station.Callsign;
                        mxfService.Name = CheckCustomServicename(station.StationId) ?? (atsc ? $"{station.Callsign} ({station.Affiliate})" : null) ?? station.Name;

                        // add affiliate if available
                        if (!string.IsNullOrEmpty(station.Affiliate))
                        {
                            mxfService.mxfAffiliate = SdMxf.GetAffiliate(station.Affiliate);
                        }

                        // add station logo if available and allowed
                        var logoPath = $"{Helper.Epg123LogosFolder}\\{station.Callsign}.png";
                        var urlLogoPath = logoPath.Replace($"{Helper.Epg123LogosFolder}\\", $"http://{Environment.MachineName}:{Helper.TcpPort}/logos/");
                        var customPath = $"{Helper.Epg123LogosFolder}\\{station.Callsign}_c.png";
                        if (config.IncludeSdLogos)
                        {
                            // make sure logos directory exists
                            if (!Directory.Exists(Helper.Epg123LogosFolder))
                            {
                                Directory.CreateDirectory(Helper.Epg123LogosFolder);
                            }

                            if (station.StationLogos != null)
                            {
                                stationLogo = station.StationLogos.FirstOrDefault(arg => arg.Category != null && arg.Category.Equals(config.PreferredLogoStyle, StringComparison.OrdinalIgnoreCase)) ??
                                              station.StationLogos.FirstOrDefault(arg => arg.Category != null && arg.Category.Equals(config.AlternateLogoStyle, StringComparison.OrdinalIgnoreCase));

                                if (stationLogo != null)
                                {
                                    switch (stationLogo.Category)
                                    {
                                        case "dark":
                                            logoPath = logoPath.Replace(".png", "_d.png");
                                            break;
                                        case "gray":
                                            logoPath = logoPath.Replace(".png", "_g.png");
                                            break;
                                        case "light":
                                            logoPath = logoPath.Replace(".png", "_l.png");
                                            break;
                                        case "white":
                                            logoPath = logoPath.Replace(".png", "_w.png");
                                            break;
                                    }
                                }
                            }
                            if (stationLogo == null && !config.PreferredLogoStyle.Equals("none", StringComparison.OrdinalIgnoreCase) && !config.AlternateLogoStyle.Equals("none", StringComparison.OrdinalIgnoreCase))
                            {
                                stationLogo = station.Logo;
                            }

                            // download the logo from SD if not present in the .\logos folder
                            if (stationLogo != null && !File.Exists(logoPath))
                            {
                                var url = stationLogo.Url;

                                // download, crop & resize logo image, save and add
                                if (!string.IsNullOrEmpty(url))
                                {
                                    StationLogosToDownload.Add(new KeyValuePair<MxfService, string[]>(mxfService, new[] { logoPath, url }));
                                }
                            }

                            // add the existing logo; custom logo overrides downloaded logos
                            if (File.Exists(customPath))
                            {
                                logoPath = customPath;
                            }
                            urlLogoPath = logoPath.Replace($"{Helper.Epg123LogosFolder}\\", $"http://{Environment.MachineName}:{Helper.TcpPort}/logos/");

                            if (File.Exists(logoPath))
                            {
                                mxfService.mxfGuideImage = SdMxf.GetGuideImage(Helper.Standalone ? $"file://{logoPath}" : urlLogoPath, GetStringEncodedImage(logoPath));
                            }
                        }

                        // handle xmltv logos
                        if (config.XmltvIncludeChannelLogos.Equals("url"))
                        {
                            if (stationLogo != null) mxfService.extras.Add("logo", stationLogo);
                            else if (station.Logo?.Url != null) mxfService.extras.Add("logo", station.Logo);
                        }
                        else if (config.XmltvIncludeChannelLogos.Equals("local") && config.IncludeSdLogos)
                        {
                            if (File.Exists(logoPath))
                            {
                                var image = Image.FromFile(logoPath);
                                mxfService.extras.Add("logo", new StationImage
                                {
                                    Url = Helper.Standalone ? logoPath : urlLogoPath,
                                    Height = image.Height,
                                    Width = image.Width
                                });
                            }
                            else if (stationLogo != null)
                            {
                                mxfService.extras.Add("logo", new StationImage
                                {
                                    Url = Helper.Standalone ? logoPath : urlLogoPath
                                });
                            }
                        }
                    }

                    // use hashset to make sure we don't duplicate channel entries for this station
                    var channelNumbers = new HashSet<string>();

                    // match station with mapping for lineup number and subnumbers
                    foreach (var map in lineupMap.Map)
                    {
                        var number = -1;
                        var subnumber = 0;
                        if (!map.StationId.Equals(station.StationId)) continue;

                        // QAM
                        if (map.ChannelMajor > 0)
                        {
                            number = map.ChannelMajor;
                            subnumber = map.ChannelMinor;
                        }

                        // ATSC or NTSC
                        else if (map.AtscMajor > 0)
                        {
                            number = map.AtscMajor;
                            subnumber = map.AtscMinor;
                        }
                        else if (map.UhfVhf > 0)
                        {
                            number = map.UhfVhf;
                        }

                        // Cable or Satellite
                        else if (!string.IsNullOrEmpty(map.Channel))
                        {
                            subnumber = 0;
                            if (Regex.Match(map.Channel, @"[A-Za-z]{1}[\d]{4}").Length > 0)
                            {
                                // 4dtv has channels starting with 2 character satellite identifier
                                number = int.Parse(map.Channel.Substring(2));
                            }
                            else if (!int.TryParse(Regex.Replace(map.Channel, "[^0-9.]", ""), out number))
                            {
                                // if channel number is not a whole number, must be a decimal number
                                var numbers = Regex.Replace(map.Channel, "[^0-9.]", "").Replace('_', '.').Replace('-', '.').Split('.');
                                if (numbers.Length == 2)
                                {
                                    number = int.Parse(numbers[0]);
                                    subnumber = int.Parse(numbers[1]);
                                }
                            }
                        }

                        string matchName = null;
                        switch (clientLineup.Transport)
                        {
                            case "DVB-S":
                                var m = Regex.Match(lineupMap.Metadata.Lineup, @"\d+\.\d+");
                                if (m.Success && map.FrequencyHz > 0 && map.NetworkId > 0 && map.TransportId > 0 && map.ServiceId > 0)
                                {
                                    while (map.FrequencyHz > 13000)
                                    {
                                        map.FrequencyHz /= 1000;
                                    }
                                    matchName = $"DVBS:{m.Value.Replace(".", "")}:{map.FrequencyHz}:{map.NetworkId}:{map.TransportId}:{map.ServiceId}";
                                }
                                number = -1;
                                subnumber = 0;
                                break;
                            case "DVB-T":
                                if (map.NetworkId > 0 && map.TransportId > 0 && map.ServiceId > 0)
                                {
                                    matchName = $"DVBT:{map.NetworkId}:{map.TransportId}:{map.ServiceId}";
                                }
                                break;
                            case "Antenna":
                                if (map.AtscMajor > 0 && map.AtscMinor > 0)
                                {
                                    matchName = $"OC:{map.AtscMajor}:{map.AtscMinor}";
                                }
                                break;
                        }

                        var channelNumber = number + ((subnumber > 0) ? "." + subnumber : null);
                        if (channelNumbers.Add(channelNumber + ":" + station.StationId))
                        {
                            SdMxf.With.Lineups[lineupIndex].channels.Add(new MxfChannel
                            {
                                mxfLineup = SdMxf.With.Lineups[lineupIndex],
                                mxfService = mxfService,
                                Number = number,
                                SubNumber = subnumber,
                                MatchName = matchName
                            });
                        }
                    }
                }
            }

            if (StationLogosToDownload.Count > 0)
            {
                StationLogosDownloadComplete = false;
                Logger.WriteInformation($"Kicking off background worker to download and process {StationLogosToDownload.Count} station logos.");
                BackgroundDownloader = new System.ComponentModel.BackgroundWorker();
                BackgroundDownloader.DoWork += BackgroundDownloader_DoWork;
                BackgroundDownloader.RunWorkerCompleted += BackgroundDownloader_RunWorkerCompleted;
                BackgroundDownloader.WorkerSupportsCancellation = true;
                BackgroundDownloader.RunWorkerAsync();
            }

            if (SdMxf.With.Services.Count > 0)
            {
                // report specific stations that are no longer available
                var missing = (from station in IncludedStations where SdMxf.With.Services.FirstOrDefault(arg => arg.StationId.Equals(station)) == null select config.StationId.Single(arg => arg.StationId.Equals(station)).CallSign).ToList();
                if (missing.Count > 0)
                {
                    MissingStations = missing.Count;
                    Logger.WriteInformation($"Stations no longer available since last configuration save are: {string.Join(", ", missing)}");
                }

                Logger.WriteMessage("Exiting BuildLineupServices(). SUCCESS.");
                return true;
            }
            else
            {
                Logger.WriteError($"There are 0 stations queued for download from {clientLineups.Lineups.Count} subscribed lineups. Exiting.");
                Logger.WriteError("Check that lineups are 'INCLUDED' and stations are selected in the EPG123 GUI.");
                return false;
            }
        }

        private static void BackgroundDownloader_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                Logger.WriteInformation("The background worker to download station logos was cancelled.");
            }
            else if (e.Error != null)
            {
                Logger.WriteInformation($"The background worker to download station logos threw an exception. Message: {e.Error.Message}");
            }
            StationLogosDownloadComplete = true;
        }

        private static void BackgroundDownloader_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            foreach (var serviceLogo in StationLogosToDownload)
            {
                var logoPath = serviceLogo.Value[0];
                if (DownloadSdLogo(serviceLogo.Value[1], logoPath) && string.IsNullOrEmpty(serviceLogo.Key.LogoImage))
                {
                    serviceLogo.Key.mxfGuideImage = SdMxf.GetGuideImage("file://" + logoPath, GetStringEncodedImage(logoPath));

                    if (File.Exists(logoPath))
                    {
                        // update dimensions
                        var image = Image.FromFile(logoPath);
                        serviceLogo.Key.extras["logo"].Height = image.Height;
                        serviceLogo.Key.extras["logo"].Width = image.Width;
                    }
                }

                if (!BackgroundDownloader.CancellationPending) continue;
                e.Cancel = true;
                break;
            }
        }

        private static bool DownloadSdLogo(string uri, string filepath)
        {
            try
            {
                var wc = new System.Net.WebClient();
                using (var stream = new MemoryStream(wc.DownloadData(uri)))
                {
                    // crop and save image
                    var cropImg = Helper.CropAndResizeImage(Image.FromStream(stream) as Bitmap);
                    cropImg.Save(filepath, System.Drawing.Imaging.ImageFormat.Png);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Logger.WriteVerbose($"An exception occurred during downloadSDLogo(). {ex.Message}");
            }
            return false;
        }

        private static string GetStringEncodedImage(string path)
        {
            if (string.IsNullOrEmpty(path) || !File.Exists(path)) return null;
            try
            {
                using (var ms = new MemoryStream())
                {
                    Image.FromFile(path).Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                    return Convert.ToBase64String(ms.ToArray());
                }
            }
            catch
            {
                // ignored
            }

            return null;
        }

        private static void PopulateIncludedExcludedStations(List<SdChannelDownload> list)
        {
            if (list == null) return;
            foreach (var station in list)
            {
                if (station.StationId.StartsWith("-"))
                {
                    ExcludedStations.Add(station.StationId.Replace("-", ""));
                }
                else
                {
                    IncludedStations.Add(station.StationId);
                }
            }
        }

        private static string CheckCustomCallsign(string stationId)
        {
            var cus = config.StationId.SingleOrDefault(arg => arg.StationId == stationId);
            return string.IsNullOrEmpty(cus?.CustomCallSign) ? null : cus.CustomCallSign;
        }
        private static string CheckCustomServicename(string stationId)
        {
            var cus = config.StationId.SingleOrDefault(arg => arg.StationId == stationId);
            return string.IsNullOrEmpty(cus?.CustomServiceName) ? null : cus.CustomServiceName;
        }
    }
}