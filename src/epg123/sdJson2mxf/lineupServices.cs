using GaRyan2.MxfXml;
using GaRyan2.SchedulesDirectAPI;
using GaRyan2.Utilities;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using api = GaRyan2.SchedulesDirect;

namespace epg123.sdJson2mxf
{
    internal static partial class sdJson2Mxf
    {
        private static readonly HashSet<string> IncludedStations = new HashSet<string>();
        private static readonly HashSet<string> ExcludedStations = new HashSet<string>();

        public static int AddedStations;
        public static int MissingStations;

        public static System.ComponentModel.BackgroundWorker BackgroundDownloader;
        public static List<KeyValuePair<MxfService, string[]>> StationLogosToDownload = new List<KeyValuePair<MxfService, string[]>>();
        public static volatile bool StationLogosDownloadComplete = true;

        private static bool BuildLineupServices()
        {
            PopulateIncludedExcludedStations(config.StationId);

            // query what lineups client is subscribed to
            var clientLineups = api.GetSubscribedLineups();
            if (clientLineups?.Lineups == null) return false;

            // reset counters
            processedObjects = 0; totalObjects = clientLineups.Lineups.Count;
            ReportProgress();

            // process lineups
            Logger.WriteMessage($"Entering BuildLineupServices() for {clientLineups.Lineups.Count} lineups.");
            foreach (var clientLineup in clientLineups.Lineups)
            {
                ++processedObjects; ReportProgress();

                // don't download station map if lineup not included
                if (!config.IncludedLineup.Contains(clientLineup.Lineup))
                {
                    Logger.WriteVerbose($"Subscribed lineup {clientLineup.Lineup} has been EXCLUDED by user from download and processing.");
                    continue;
                }

                // give warning that headend has been deleted
                if (clientLineup.IsDeleted)
                {
                    Logger.WriteError($"Subscribed lineup {clientLineup.Lineup} has been DELETED at the headend.");
                    Logger.WriteError("ACTION: The lineup could have been replaced with a new lineup. Use the configuration GUI to see if there is a new lineup to replace this lineup with.");
                    Logger.WriteError("ACTION: If there is no replacement lineup available, submit a ticket with Schedules Direct at https://schedulesdirect.org");
                    return false;
                }

                // request the lineup's station maps
                var lineupMap = api.GetStationChannelMap(clientLineup.Lineup);
                if ((lineupMap?.Stations?.Count ?? 0) == 0)
                {
                    Logger.WriteError($"Subscribed lineup {clientLineup.Lineup} does not contain any stations.");
                    Logger.WriteError("ACTION: The lineup could have been replaced with a new lineup. Use the configuration GUI to see if there is a new lineup to replace this lineup with.");
                    Logger.WriteError("ACTION: If there is no replacement lineup available, submit a ticket with Schedules Direct at https://schedulesdirect.org");
                    return false;
                }

                // log if channels numbers are discarded
                if (config.DiscardChanNumbers.Contains(clientLineup.Lineup))
                {
                    Logger.WriteVerbose($"Subscribed lineup {clientLineup.Lineup} will ignore all channel numbers.");
                }

                // get/create lineup
                var mxfLineup = mxf.FindOrCreateLineup(clientLineup.Lineup, $"EPG123 {clientLineup.Name} ({clientLineup.Location})");

                // use hashset to make sure we don't duplicate channel entries for this station
                var channelNumbers = new HashSet<string>();

                // build the services and lineup
                foreach (var station in lineupMap.Stations)
                {
                    // check if station should be downloaded and processed
                    if (station == null || ExcludedStations.Contains(station.StationId)) continue;
                    if (!IncludedStations.Contains(station.StationId) && !config.AutoAddNew)
                    {
                        Logger.WriteWarning($"**** Lineup {clientLineup.Name} ({clientLineup.Location}) has added station {station.StationId} ({station.Callsign}). ****");
                        Logger.WriteWarning("ACTION: If the option 'Automaticaly download new stations in lineups' is not enabled, this warning will appear every time your lineup adds a new station.");
                        Logger.WriteWarning("ACTION: Open the configuration GUI and either add the station to be downloaded for future updates and click [Save], or just click [Save] to not download it.");
                        continue;
                    }

                    // build the service if necessary
                    var mxfService = mxf.FindOrCreateService(station.StationId);
                    if (string.IsNullOrEmpty(mxfService.CallSign))
                    {
                        // instantiate stationLogo and override uid
                        StationImage stationLogo = null;
                        mxfService.UidOverride = $"!Service!EPG123_{station.StationId}";

                        // add callsign and station name
                        mxfService.CallSign = CheckCustomCallsign(station.StationId) ?? station.Callsign;
                        if (string.IsNullOrEmpty(mxfService.Name = CheckCustomServicename(station.StationId)))
                        {
                            var names = Regex.Matches(station.Name.Replace("-", ""), station.Callsign);
                            if (names.Count > 0)
                            {
                                mxfService.Name = (!string.IsNullOrEmpty(station.Affiliate) ? $"{station.Name} ({station.Affiliate})" : station.Name);
                            }
                            else mxfService.Name = station.Name;
                        }

                        // add affiliate if available
                        if (!string.IsNullOrEmpty(station.Affiliate))
                        {
                            mxfService.mxfAffiliate = mxf.FindOrCreateAffiliate(station.Affiliate);
                        }

                        // add station logo if available
                        stationLogo = station.StationLogos?.FirstOrDefault(arg => arg.Category != null && arg.Category.Equals(config.PreferredLogoStyle, StringComparison.OrdinalIgnoreCase)) ??
                                      station.StationLogos?.FirstOrDefault(arg => arg.Category != null && arg.Category.Equals(config.AlternateLogoStyle, StringComparison.OrdinalIgnoreCase)) ??
                                      (!config.PreferredLogoStyle.Equals("none", StringComparison.OrdinalIgnoreCase) ? station.Logo : null);

                        // initialize as custom logo
                        var logoPath = string.Empty;
                        var urlLogoPath = string.Empty;
                        string encodedLogo = null;

                        var logoFilename = $"{station.Callsign}_c.png";
                        if (config.IncludeSdLogos && File.Exists($"{Helper.Epg123LogosFolder}{logoFilename}"))
                        {
                            logoPath = $"{Helper.Epg123LogosFolder}{logoFilename}";
                            urlLogoPath = $"http://{HostAddress}:{Helper.TcpUdpPort}/logos/{logoFilename}";
                            encodedLogo = GetStringEncodedImage(logoPath);
                        }
                        else if (stationLogo != null)
                        {
                            logoFilename = $"{stationLogo.Md5}.png";
                            logoPath = $"{Helper.Epg123LogosFolder}{logoFilename}";
                            urlLogoPath = $"http://{HostAddress}:{Helper.TcpUdpPort}/logos/{logoFilename}";

                            // include refresh of logos in case logo changes but md5 is not changed
                            var daysInMonth = DateTime.DaysInMonth(DateTime.UtcNow.Year, DateTime.UtcNow.Month);
                            var bingo = int.Parse(stationLogo.Md5.Substring(stationLogo.Md5.Length - 2), NumberStyles.HexNumber) & 0x1F;
                            bingo = Math.Max(Math.Min(daysInMonth, bingo), 1);
                            TimeSpan lastRefresh = DateTime.UtcNow - (new FileInfo(logoPath)?.LastWriteTimeUtc ?? DateTime.MinValue);
                            if ((bingo == DateTime.UtcNow.Day && lastRefresh.Days > 0) || lastRefresh.Days >= 31) Helper.DeleteFile(logoPath);

                            if (config.IncludeSdLogos && !File.Exists(logoPath))
                            {
                                StationLogosToDownload.Add(new KeyValuePair<MxfService, string[]>(mxfService, new[] { logoPath, stationLogo.Url }));
                            }
                            else if (config.IncludeSdLogos && Helper.Standalone) encodedLogo = GetStringEncodedImage(logoPath);
                        }

                        // add to mxf guide images if file exists already
                        if (config.IncludeSdLogos && !string.IsNullOrEmpty(logoPath) && File.Exists(logoPath))
                        {
                            mxfService.mxfGuideImage = mxf.FindOrCreateGuideImage(Helper.Standalone ? $"file://{logoPath}" : urlLogoPath, encodedLogo);
                        }

                        // handle xmltv logos
                        if (config.XmltvIncludeChannelLogos.Equals("url") && stationLogo != null)
                        {
                            mxfService.extras.Add("logo", stationLogo);
                        }
                        else if (config.IncludeSdLogos)
                        {
                            if (!string.IsNullOrEmpty(logoPath) && File.Exists(logoPath))
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

                    // match station with mapping for lineup number and subnumbers
                    foreach (var map in lineupMap.Map)
                    {
                        if (!map.StationId.Equals(station.StationId)) continue;
                        var number = map.myChannelNumber;
                        var subnumber = map.myChannelSubnumber;

                        string matchName = map.ProviderCallsign;
                        switch (clientLineup.Transport)
                        {
                            case "Satellite":
                            case "DVB-S":
                                var m = Regex.Match(lineupMap.Metadata.Lineup, @"\d+\.\d+");
                                if (m.Success && map.FrequencyHz > 0 && map.NetworkId > 0 && map.TransportId > 0 && map.ServiceId > 0)
                                {
                                    while (map.FrequencyHz > 13000)
                                    {
                                        map.FrequencyHz /= 1000;
                                    }
                                    matchName = $"DVBS:{m.Value.Replace(".", "")}:{map.FrequencyHz}:{map.NetworkId}:{map.TransportId}:{map.ServiceId}";
                                    number = -1;
                                    subnumber = 0;
                                }
                                break;
                            case "Antenna":
                            case "DVB-T":
                                if (map.NetworkId > 0 && map.TransportId > 0 && map.ServiceId > 0)
                                {
                                    matchName = $"DVBT:{map.NetworkId}:{map.TransportId}:{map.ServiceId}";
                                    break;
                                }
                                if (map.AtscMajor > 0 && map.AtscMinor > 0)
                                {
                                    matchName = $"OC:{map.AtscMajor}:{map.AtscMinor}";
                                }
                                break;
                        }

                        if (config.DiscardChanNumbers.Contains(clientLineup.Lineup))
                        {
                            number = -1; subnumber = 0;
                        }

                        var channelNumber = $"{number}{(subnumber > 0 ? $".{subnumber}" : "")}";
                        if (channelNumbers.Add($"{channelNumber}:{station.StationId}"))
                        {
                            mxfLineup.channels.Add(new MxfChannel(mxfLineup, mxfService, number, subnumber)
                            {
                                MatchName = matchName
                            });
                        }
                    }
                }
            }

            if (StationLogosToDownload.Count > 0)
            {
                Directory.CreateDirectory(Helper.Epg123LogosFolder);
                StationLogosDownloadComplete = false;
                Logger.WriteInformation($"Kicking off background worker to download and process {StationLogosToDownload.Count} station logos.");
                BackgroundDownloader = new System.ComponentModel.BackgroundWorker();
                BackgroundDownloader.DoWork += BackgroundDownloader_DoWork;
                BackgroundDownloader.RunWorkerCompleted += BackgroundDownloader_RunWorkerCompleted;
                BackgroundDownloader.WorkerSupportsCancellation = true;
                BackgroundDownloader.RunWorkerAsync();
            }

            if (mxf.With.Services.Count > 0)
            {
                // report specific stations that are no longer available
                var missing = (from station in IncludedStations where mxf.With.Services.FirstOrDefault(arg => arg.StationId.Equals(station)) == null select config.StationId.Single(arg => arg.StationId.Equals(station)).CallSign).ToList();
                if (missing.Count > 0)
                {
                    MissingStations = missing.Count;
                    Logger.WriteInformation($"Stations no longer available since last configuration save are: {string.Join(", ", missing)}");
                }
                var extras = mxf.With.Services.Where(arg => !IncludedStations.Contains(arg.StationId)).ToList();
                if (extras.Count > 0)
                {
                    AddedStations = extras.Count;
                    Logger.WriteInformation($"Stations added for download since last configuration save are: {string.Join(", ", extras.Select(e => e.CallSign))}");
                }

                Logger.WriteMessage("Exiting BuildLineupServices(). SUCCESS.");
                return true;
            }

            Logger.WriteError($"There are 0 stations queued for download from {clientLineups.Lineups.Count} subscribed lineups. Exiting.");
            Logger.WriteError("ACTION: Check that lineups are 'INCLUDED' and stations are selected in the EPG123 Configuration GUI.");
            return false;
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
                if ((File.Exists(logoPath) || DownloadSdLogo(serviceLogo.Value[1], logoPath)) && string.IsNullOrEmpty(serviceLogo.Key.LogoImage))
                {
                    serviceLogo.Key.mxfGuideImage = mxf.FindOrCreateGuideImage(Helper.Standalone ? $"file://{logoPath}" : $"http://{HostAddress}:{Helper.TcpUdpPort}/logos/{Path.GetFileName(logoPath)}",
                        Helper.Standalone ? GetStringEncodedImage(logoPath) : null);

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
                Logger.WriteVerbose($"An exception occurred during DownloadSDLogo(). Message:{Helper.ReportExceptionMessages(ex)}");
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