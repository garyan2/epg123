using GaRyan2.Converters;
using GaRyan2.MxfXml;
using GaRyan2.SiliconDustApi;
using GaRyan2.Utilities;
using GaRyan2.WmcUtilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace hdhr2mxf
{
    class Program
    {
        private static readonly API api = new API()
        {
            BaseAddress = "https://api.hdhomerun.com/",
            UserAgent = $"HDHR2MXF/{Helper.Epg123Version}"
        };
        private static bool _noLogos;
        private static List<string> _ipAddresses;

        private static int Main(string[] args)
        {
            var error = false;
            var showHelp = false;
            var import = false;
            if (args != null)
            {
                for (var i = 0; i < args.Length; ++i)
                {
                    switch (args[i].ToLower())
                    {
                        case "-nologos":
                            _noLogos = true;
                            break;
                        case "-ip":
                            if (i + 1 < args.Length && !args[i + 1].StartsWith("-"))
                            {
                                _ipAddresses = args[++i].Split(',').ToList();
                                foreach (var addr in _ipAddresses)
                                {
                                    if (!Regex.Match(addr, @"\b\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}(:[0-9]+)?\b").Success)
                                    {
                                        Console.WriteLine($"IP Address \"{addr}\" is not a valid IPv4 address.");
                                        error = true;
                                    }
                                }
                            }
                            else
                            {
                                Console.WriteLine("Invalid argument usage. -IP requires additional information.");
                                error = true;
                            }
                            break;
                        case "-import":
                            import = true;
                            break;
                        case "-o": // deprecated
                            ++i;
                            break;
                        case "-update": // deprecated
                            break;
                        case "-h":
                        case "/h":
                        case "/?":
                            showHelp = true;
                            break;
                        default:
                            Console.WriteLine($"Invalid switch - \"{args[i]}\"\n");
                            error = true;
                            break;
                    }
                }
            }
            if (error || showHelp)
            {
                var help = "HDHR2MXF [-IP addresses] [-NOLOGOS] [-IMPORT]\n\n" +
                           "-IP         Discovers HDHomeRun devices at specific addresses.\n" +
                           "addresses   Comma delimited list of IPv4 addresses to devices.\n" +
                           "            Include port numbers as needed (RECORD).\n" +
                           "            Example: 192.168.1.54,192.168.1.74:50000\n\n" +
                           "-NOLOGOS    Resulting files do not include links to station logos.\n\n" +
                           "-IMPORT     Automatically imports MXF file into Windows Media Center.";
                Console.WriteLine(help);
                return error ? -1 : 0;
            }

            Logger.Initialize(Helper.Epg123TraceLogPath, "Beginning HDHR2MXF update execution", false);
            if (import) Logger.LogWmcDescription();
            api.Initialize();

            var success = Build();
            if (!success || !import) return success ? 0 : 0xDEAD;
            success = WmcStore.ImportMxfFile(Helper.Hdhr2MxfMxfPath) && WmcStore.ReindexDatabase();
            return success ? 0 : 0xDEAD;
        }

        public static bool Build()
        {
            var startTime = DateTime.UtcNow;

            // verify hdhomerun devices on network
            List<HdhrDiscover> devices;
            if (_ipAddresses != null && _ipAddresses.Count > 0)
            {
                devices = new List<HdhrDiscover>();
                foreach (var addr in _ipAddresses)
                {
                    devices.Add(new HdhrDiscover { DiscoverUrl = $"http://{addr}/discover.json" });
                }
            }
            else
            {
                devices = api.DiscoverDevices();
            }
            if (devices == null) return false;

            // verify dvr service is active
            var authString = string.Empty;
            var dvrActive = false;
            foreach (var device in devices)
            {
                var detail = api.GetDeviceDetails(device.DiscoverUrl);
                if (detail == null) continue;
                if (device.DeviceId == null)
                {
                    device.DeviceId = detail.DeviceId;
                    device.Legacy = detail.Legacy;
                    device.LineupUrl = detail.LineupUrl;
                }
                Logger.WriteVerbose($"Discovered {detail.FriendlyName}{(string.IsNullOrEmpty(detail.Version) ? "" : $" version {detail.Version}")}{(string.IsNullOrEmpty(detail.ModelNumber) ? "" : $" {detail.ModelNumber}")}{(string.IsNullOrEmpty(detail.DeviceId) ? "" : $" ({detail.DeviceId})")}{(string.IsNullOrEmpty(detail.FirmwareName) ? "" : $" with firmware {detail.FirmwareVersion}")}.{(detail.TotalSpace > 0 ? $" ({(double)(detail.TotalSpace - detail.FreeSpace) / detail.TotalSpace * 100.0:N1}% of {Helper.BytesToString(detail.TotalSpace)} used)" : "")}");
                if (!string.IsNullOrEmpty(detail.DeviceAuth))
                {
                    authString += detail.DeviceAuth;
                    dvrActive |= (api.GetDeviceAccount(detail.DeviceAuth)?.DvrActive ?? 0) == 1;
                }
            }
            Logger.WriteInformation($"HDHomeRun DVR Service is {(dvrActive ? "" : "not ")}active.");
            if (!dvrActive) return false;

            // download xmltv file
            Helper.SendPipeMessage("Downloading|Requesting XMLTV from SiliconDust...");
            Logger.WriteInformation("Downloading available XMLTV file from SiliconDust.");
            var xml = api.DownloadXmltvFile(authString) ?? throw new Exception("Failed to download xmltv file from SiliconDust.");
            if (xml != null)
            {
                var fi = new FileInfo(Helper.Hdhr2mxfXmltvPath);
                Logger.WriteInformation($"Completed save of the XMLTV file to \"{Helper.Hdhr2mxfXmltvPath}\". ({Helper.BytesToString(fi.Length)})");
                Logger.WriteVerbose($"Generated XMLTV file contains {xml.Channels.Count} channels and {xml.Programs.Count} programs.");
            }

            // remove logos if requested probably be removed later
            if (_noLogos)
            {
                foreach (var channel in xml.Channels)
                {
                    channel.Icons?.Clear();
                }
            }

            // initialize mxf class
            Helper.SendPipeMessage("Downloading|Building and saving MXF file...");
            var mxf = new MXF("HDHR2MXF", "EPG123 SiliconDust HDHR to MXF", "GaRyan2", "SiliconDust");

            // start m3u file for each device
            var m3uWrite = new StreamWriter(Helper.Hdhr2MxfM3uPath);
            m3uWrite.WriteLine("#EXTM3U");

            // populate lineups, channels, and services
            var channelsDone = new HashSet<string>();
            foreach (var device in devices.Where(arg => arg.DeviceId != null))
            {
                // get device information
                var detail = api.GetDeviceDetails(device.DiscoverUrl);
                if (detail == null) continue;

                // get or create lineup entry in mxf file
                var lineup = mxf.FindOrCreateLineup(detail.MxfLineupID, detail.MxfLineupName);

                // get device channel tuning info and extra info
                var channels = api.GetDeviceChannels(device.LineupUrl, device.Legacy);
                var extras = api.GetDeviceChannelDetails(detail.DeviceAuth);

                foreach (var channel in channels ?? new List<HdhrChannel>())
                {
                    // find matching channel in xmltv file
                    var xmlChannel = xml.Channels.FirstOrDefault(arg => arg.Lcn.Where(text => text.Text.Equals(channel.GuideNumber)).Any());
                    if (xmlChannel == null) continue;

                    // find matching channel in extras
                    var extra = extras?.FirstOrDefault(arg => xmlChannel.Lcn.Where(text => text.Text.Equals(arg.GuideNumber)).Any());

                    // add channel info in m3u file
                    if (extra != null && device.Legacy == 0)
                    {
                        m3uWrite.WriteLine($"#EXTINF:-1 channel-id=\"{channel.GuideNumber}\" channel-number=\"{channel.GuideNumber}\" tvg-id=\"{xmlChannel.Id}\" tvg-chno=\"{channel.GuideNumber}\" tvg-name=\"{channel.GuideName}\"{(_noLogos ? " " : $" tvg-logo=\"{extra.ImageUrl}\" ")}group-title=\"{detail.ModelNumber}-{detail.DeviceId}\",{channel.GuideNumber} {channel.GuideName}");
                        m3uWrite.WriteLine($"{channel.Url}");
                    }

                    // ensure channel has not already been added to lineup
                    var deviceType = detail.ModelNumber.Substring(detail.ModelNumber.Length - 2).Replace("4K", "US");
                    if (!channelsDone.Add($"{deviceType} {channel.GuideNumber} {channel.GuideName}")) continue;

                    // determine match name for terrestrial tuners
                    var matchName = channel.GuideName;
                    switch (deviceType)
                    {
                        case "US":
                            matchName = $"OC:{channel.Number}:{channel.Subnumber}";
                            break;
                        case "DT":
                            matchName = $"DVBT:{channel.OriginalNetworkId}:{channel.TransportStreamId}:{channel.ProgramNumber}";
                            break;
                    }

                    // create service in mxf
                    var service = mxf.FindOrCreateService(xmlChannel.Id);
                    if (string.IsNullOrEmpty(service.CallSign))
                    {
                        service.CallSign = extra?.GuideName ?? channel.GuideName;

                        if (!_noLogos && extra?.ImageUrl != null) service.mxfGuideImage = mxf.FindOrCreateGuideImage(extra.ImageUrl);
                        else if (xmlChannel.Urls != null && xmlChannel.Urls.Count > 0) service.mxfGuideImage = mxf.FindOrCreateGuideImage(xmlChannel.Urls[0]);

                        if (extra?.Affiliate != null)
                        {
                            service.mxfAffiliate = mxf.FindOrCreateAffiliate(extra.Affiliate);
                            service.Name = $"{service.CallSign} ({extra.Affiliate})";
                        }
                        else service.Name = service.CallSign;
                    }

                    // add channel to lineup in mxf
                    lineup.channels.Add(new MxfChannel(lineup, service, channel.Number, channel.Subnumber)
                    {
                        MatchName = matchName,
                    });
                }
            }
            m3uWrite.Flush();
            m3uWrite.Close();

            var converter = new Xmltv2Mxf();
            mxf = converter.ConvertToMxf(xml, mxf);

            if (File.Exists(Helper.Hdhr2MxfM3uPath))
            {
                var fi = new FileInfo(Helper.Hdhr2MxfM3uPath);
                Logger.WriteInformation($"Completed save of the M3U file to \"{Helper.Hdhr2MxfM3uPath}\". ({Helper.BytesToString(fi.Length)})");
            }

            Helper.SendPipeMessage("Downloading|Saving MXF file...");
            if (mxf != null)
            {
                if (Helper.WriteXmlFile(mxf, Helper.Hdhr2MxfMxfPath, true))
                {
                    var fi = new FileInfo(Helper.Hdhr2MxfMxfPath);
                    Logger.WriteInformation($"Completed save of the MXF file to \"{Helper.Hdhr2MxfMxfPath}\". ({Helper.BytesToString(fi.Length)})");
                    Logger.WriteVerbose($"Generated MXF file contains {mxf.With.Services.Count} services, {mxf.With.SeriesInfos.Count} series, {mxf.With.Seasons.Count} seasons, {mxf.With.Programs.Count} programs, {mxf.With.ScheduleEntries.Sum(x => x.ScheduleEntry.Count)} schedule entries, and {mxf.With.People.Count} people with {mxf.With.GuideImages.Count} image links.");
                }
            }

            Helper.SendPipeMessage("Download Complete");
            Logger.WriteVerbose($"HDHR2MXF update execution time was {DateTime.UtcNow - startTime}.");
            return true;
        }
    }
}