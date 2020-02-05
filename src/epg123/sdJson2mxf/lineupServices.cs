using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using epg123.MxfXml;

namespace epg123
{
    public static partial class sdJson2mxf
    {
        private static HashSet<string> includedStations = new HashSet<string>();
        private static HashSet<string> excludedStations = new HashSet<string>();
        private static SdStationMapResponse customMap;

        private static CustomLineup customLineup;
        private static HashSet<string> customStations = new HashSet<string>();
        private static Dictionary<string, SdLineupStation> allStations = new Dictionary<string, SdLineupStation>();

        public static System.ComponentModel.BackgroundWorker backgroundDownloader;
        public static List<KeyValuePair<MxfService, string>> stationLogosToDownload = new List<KeyValuePair<MxfService, string>>();
        public static volatile bool stationLogosDownloadComplete = true;

        private static bool buildLineupServices()
        {
            // query what lineups client is subscribed to
            SdLineupResponse clientLineups = sdAPI.sdGetLineups();
            if (clientLineups == null) return false;

            // determine if there are custom lineups to consider
            if (File.Exists(Helper.Epg123CustomLineupsXmlPath))
            {
                CustomLineups customLineups = new CustomLineups();
                using (StreamReader stream = new StreamReader(Helper.Epg123CustomLineupsXmlPath, Encoding.Default))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(CustomLineups));
                    TextReader reader = new StringReader(stream.ReadToEnd());
                    customLineups = (CustomLineups)serializer.Deserialize(reader);
                    reader.Close();
                }

                foreach (CustomLineup lineup in customLineups.CustomLineup)
                {
                    if (!config.IncludedLineup.Contains(lineup.Lineup)) continue;
                    customLineup = lineup;

                    clientLineups.Lineups.Add(new SdLineup()
                    {
                        Lineup = lineup.Lineup,
                        Name = lineup.Name,
                        Transport = string.Empty,
                        Location = lineup.Location,
                        Uri = "CUSTOM",
                        IsDeleted = false
                    });

                    customMap = new SdStationMapResponse();
                    customMap.Map = new List<SdLineupMap>();
                    customMap.Stations = new List<SdLineupStation>();
                    customMap.Metadata = new sdMetadata()
                    {
                        Lineup = lineup.Lineup
                    };
                }
            }

            // reset counters
            processedObjects = 0; totalObjects = clientLineups.Lineups.Count;
            reportProgress();

            // process lineups
            Logger.WriteMessage(string.Format("Entering buildLineupServices() for {0} lineups.", clientLineups.Lineups.Count));
            foreach (SdLineup clientLineup in clientLineups.Lineups)
            {
                bool flagCustom = (!string.IsNullOrEmpty(clientLineup.Uri) && clientLineup.Uri.Equals("CUSTOM"));
                ++processedObjects; reportProgress();

                // request the lineup's station maps
                SdStationMapResponse lineupMap = null;
                if (!flagCustom)
                {
                    lineupMap = sdAPI.sdGetStationMaps(clientLineup.Lineup);
                    if (lineupMap == null) continue;

                    foreach (SdLineupStation station in lineupMap.Stations)
                    {
                        if (!allStations.ContainsKey(station.StationID))
                        {
                            allStations.Add(station.StationID, station);
                        }
                    }
                }

                if (!config.IncludedLineup.Contains(clientLineup.Lineup))
                {
                    Logger.WriteVerbose(string.Format("Subscribed lineup {0} has been EXCLUDED from download and processing.", clientLineup.Lineup));
                    continue;
                }
                else if (clientLineup.IsDeleted)
                {
                    Logger.WriteWarning(string.Format("Subscribed lineup {0} has been DELETED at the headend.", clientLineup.Lineup));
                    continue;
                }
                else if (flagCustom)
                {
                    foreach (CustomStation station in customLineup.Station)
                    {
                        SdLineupStation lineupStation;
                        if (allStations.TryGetValue(station.StationId, out lineupStation))
                        {
                            customMap.Map.Add(new SdLineupMap()
                            {
                                StationID = station.StationId,
                                AtscMajor = int.Parse(station.Number),
                                AtscMinor = int.Parse(station.Subnumber)
                            });
                            customStations.Add(station.StationId);
                            customMap.Stations.Add(lineupStation);
                        }
                        else if (!string.IsNullOrEmpty(station.Alternate) && allStations.TryGetValue(station.Alternate, out lineupStation))
                        {
                            customMap.Map.Add(new SdLineupMap()
                            {
                                StationID = station.Alternate,
                                AtscMajor = int.Parse(station.Number),
                                AtscMinor = int.Parse(station.Subnumber)
                            });
                            customStations.Add(station.Alternate);
                            customMap.Stations.Add(lineupStation);
                        }
                    }
                    lineupMap = customMap;
                    Logger.WriteVerbose(string.Format("Successfully retrieved the station mapping for lineup {0}.", clientLineup.Lineup));
                }
                if (lineupMap == null) return false;

                int lineupIndex = sdMxf.With[0].Lineups.Count;
                sdMxf.With[0].Lineups.Add(new MxfLineup()
                {
                    index = lineupIndex + 1,
                    Uid = clientLineup.Lineup,
                    Name = "EPG123 " + clientLineup.Name + " (" + clientLineup.Location + ")",
                    channels = new List<MxfChannel>()
                });

                // build the services and lineup
                foreach (SdLineupStation station in lineupMap.Stations)
                {
                    // check if station should be downloaded and processed
                    if (!flagCustom)
                    {
                        if ((station == null) || (excludedStations.Contains(station.StationID) && !customStations.Contains(station.StationID))) continue;
                        if (!includedStations.Contains(station.StationID) && !config.AutoAddNew)
                        {
                            Logger.WriteWarning(string.Format("**** Lineup {0} ({1}) has added station {2} ({3}). ****", clientLineup.Name, clientLineup.Location, station.StationID, station.Callsign));
                            continue;
                        }
                    }

                    // build the service if necessary
                    MxfService mxfService = sdMxf.With[0].getService(station.StationID);
                    if (string.IsNullOrEmpty(mxfService.CallSign))
                    {
                        // add callsign and station name
                        mxfService.CallSign = station.Callsign;
                        mxfService.Name = station.Name;

                        // add affiliate if available
                        if (!string.IsNullOrEmpty(station.Affiliate))
                        {
                            mxfService.Affiliate = sdMxf.With[0].getAffiliateId(station.Affiliate);
                        }

                        // set the ScheduleEntries service id
                        mxfService.mxfScheduleEntries.Service = mxfService.Id;

                        // add station logo if available and allowed
                        string logoPath = string.Format("{0}\\{1}.png", Helper.Epg123LogosFolder, station.Callsign);
                        if (config.IncludeSDLogos)
                        {
                            // make sure logos directory exists
                            if (!Directory.Exists(Helper.Epg123LogosFolder))
                            {
                                Directory.CreateDirectory(Helper.Epg123LogosFolder);
                            }

                            // add the existing logo or download the new logo if available
                            if (File.Exists(logoPath))
                            {
                                mxfService.LogoImage = sdMxf.With[0].getGuideImage("file://" + logoPath, getStringEncodedImage(logoPath)).Id;
                            }
                            else
                            {
                                string url = string.Empty;
                                if ((station.StationLogos != null) && (station.StationLogos.Count > 0))
                                {
                                    // the second station logo is typically the best contrast
                                    url = station.StationLogos[Math.Min(station.StationLogos.Count - 1, 1)].URL;
                                }
                                else if (station.Logo != null)
                                {
                                    url = station.Logo.URL;
                                }

                                // download, crop & resize logo image, save and add
                                if (!string.IsNullOrEmpty(url))
                                {
                                    stationLogosToDownload.Add(new KeyValuePair<MxfService, string>(mxfService, url));
                                }
                            }
                        }

                        // handle xmltv logos
                        SdStationImage logoImage = (station.StationLogos != null) ? station.StationLogos[station.StationLogos.Count - 1] : station.Logo;
                        if (config.XmltvIncludeChannelLogos.ToLower().Equals("url") && (logoImage != null))
                        {
                            mxfService.logoImage = logoImage;
                        }
                        else if (config.XmltvIncludeChannelLogos.ToLower().Equals("local") && File.Exists(logoPath))
                        {
                            Image image = Image.FromFile(logoPath);
                            mxfService.logoImage = new SdStationImage()
                            {
                                URL = logoPath,
                                Height = image.Height,
                                Width = image.Width
                            };
                        }
                        else if (config.XmltvIncludeChannelLogos.ToLower().Equals("substitute") && File.Exists(logoPath))
                        {
                            Image image = Image.FromFile(logoPath);
                            mxfService.logoImage = new SdStationImage()
                            {
                                URL = string.Format("{0}\\{1}.png", config.XmltvLogoSubstitutePath.TrimEnd('\\'), station.Callsign),
                                Height = image.Height,
                                Width = image.Width
                            };
                        }
                    }

                    // use hashset to make sure we don't duplicate channel entries for this station
                    HashSet<string> channelNumbers = new HashSet<string>();

                    // match station with mapping for lineup number and subnumbers
                    foreach (SdLineupMap map in lineupMap.Map)
                    {
                        int number = -1;
                        int subnumber = 0;
                        if (map.StationID.Equals(station.StationID))
                        {
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
                                    string[] numbers = Regex.Replace(map.Channel, "[^0-9.]", "").Replace('_', '.').Replace('-', '.').Split('.');
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
                                    Match m = Regex.Match(lineupMap.Metadata.Lineup, @"\d+\.\d+");
                                    if (m.Success && map.FrequencyHz > 0 && map.NetworkID > 0 && map.TransportID > 0 && map.ServiceID > 0)
                                    {
                                        while (map.FrequencyHz > 13000)
                                        {
                                            map.FrequencyHz /= 1000;
                                        }
                                        matchName = string.Format("DVBS:{0}:{1}:{2}:{3}:{4}", m.Value.Replace(".", ""),
                                                                                              map.FrequencyHz,
                                                                                              map.NetworkID,
                                                                                              map.TransportID,
                                                                                              map.ServiceID);
                                    }
                                    number = -1;
                                    subnumber = 0;
                                    break;
                                case "DVB-T":
                                    if (map.NetworkID > 0 && map.TransportID > 0 && map.ServiceID > 0)
                                    {
                                        matchName = string.Format("DVBT:{0}:{1}:{2}", map.NetworkID, map.TransportID, map.ServiceID);
                                    }
                                    break;
                                case "Antenna":
                                    if (map.AtscMajor > 0 && map.AtscMinor > 0)
                                    {
                                        matchName = string.Format("OC:{0}:{1}", map.AtscMajor, map.AtscMinor);
                                    }
                                    break;
                                default:
                                    break;
                            }

                            string channelNumber = number.ToString() + ((subnumber > 0) ? "." + subnumber.ToString() : null);
                            if (channelNumbers.Add(channelNumber + ":" + station.StationID))
                            {
                                sdMxf.With[0].Lineups[lineupIndex].channels.Add(new MxfChannel()
                                {
                                    Lineup = sdMxf.With[0].Lineups[lineupIndex].Id,
                                    lineupUid = lineupMap.Metadata.Lineup,
                                    stationId = mxfService.StationID,
                                    Service = mxfService.Id,
                                    Number = number,
                                    SubNumber = subnumber,
                                    MatchName = matchName
                                });
                            }
                        }
                    }
                }
            }

            if (stationLogosToDownload.Count > 0)
            {
                stationLogosDownloadComplete = false;
                Logger.WriteInformation(string.Format("Kicking off background worker to download and process {0} station logos.", stationLogosToDownload.Count));
                backgroundDownloader = new System.ComponentModel.BackgroundWorker();
                backgroundDownloader.DoWork += BackgroundDownloader_DoWork;
                backgroundDownloader.RunWorkerCompleted += BackgroundDownloader_RunWorkerCompleted;
                backgroundDownloader.WorkerSupportsCancellation = true;
                backgroundDownloader.RunWorkerAsync();
            }

            if (sdMxf.With[0].Services.Count > 0)
            {
                Logger.WriteMessage("Exiting buildLineupServices(). SUCCESS.");
                return true;
            }
            else
            {
                Logger.WriteError(string.Format("There are 0 stations queued for download from {0} subscribed lineups. Exiting.", clientLineups.Lineups.Count));
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
                Logger.WriteError(string.Format("The background worker to download station logos threw an exception. Message: {0}", e.Error.Message));
            }
            stationLogosDownloadComplete = true;
        }

        private static void BackgroundDownloader_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            foreach (KeyValuePair<MxfService, string> keyValuePair in stationLogosToDownload)
            {
                string logoPath = string.Format("{0}\\{1}.png", Helper.Epg123LogosFolder, keyValuePair.Key.CallSign);
                if (downloadSDLogo(keyValuePair.Value, logoPath))
                {
                    keyValuePair.Key.LogoImage = sdMxf.With[0].getGuideImage("file://" + logoPath, getStringEncodedImage(logoPath)).Id;
                }

                if (backgroundDownloader.CancellationPending)
                {
                    e.Cancel = true;
                    break;
                }
            }
        }

        private static bool downloadSDLogo(string uri, string filepath)
        {
            try
            {
                // set target aspect/image size
                double tgtAspect = 3.0;

                System.Net.WebClient wc = new System.Net.WebClient();
                using (MemoryStream stream = new MemoryStream(wc.DownloadData(uri)))
                {
                    // crop image
                    Bitmap cropImg;
                    using (Bitmap origImg = Image.FromStream(stream) as Bitmap)
                    {
                        // Find the min/max non-transparent pixels
                        Point min = new Point(int.MaxValue, int.MaxValue);
                        Point max = new Point(int.MinValue, int.MinValue);

                        for (int x = 0; x < origImg.Width; ++x)
                        {
                            for (int y = 0; y < origImg.Height; ++y)
                            {
                                Color pixelColor = origImg.GetPixel(x, y);
                                if (pixelColor.A == 255)
                                {
                                    if (x < min.X) min.X = x;
                                    if (y < min.Y) min.Y = y;

                                    if (x > max.X) max.X = x;
                                    if (y > max.Y) max.Y = y;
                                }
                            }
                        }

                        // Create a new bitmap from the crop rectangle and increase canvas size if necessary
                        int offsetY = 0;
                        Rectangle cropRectangle = new Rectangle(min.X, min.Y, max.X - min.X + 1, max.Y - min.Y + 1);
                        if (((double)(max.X - min.X + 1) / tgtAspect) > (max.Y - min.Y + 1))
                        {
                            offsetY = (int)((max.X - min.X + 1) / tgtAspect - (max.Y - min.Y + 1) + 0.5) / 2;
                        }

                        cropImg = new Bitmap(cropRectangle.Width, cropRectangle.Height + offsetY * 2);
                        cropImg.SetResolution(origImg.HorizontalResolution, origImg.VerticalResolution);
                        using (Graphics g = Graphics.FromImage(cropImg))
                        {
                            g.DrawImage(origImg, 0, offsetY, cropRectangle, GraphicsUnit.Pixel);
                        }

                        // save image
                        cropImg.Save(filepath, System.Drawing.Imaging.ImageFormat.Png);
                    }
                    return true;
                }
            }
            catch (Exception ex)
            {
                Logger.WriteVerbose(ex.Message);
            }
            return false;
        }

        private static string getStringEncodedImage(string path)
        {
            if (!string.IsNullOrEmpty(path) && File.Exists(path))
            {
                try
                {
                    using (var ms = new MemoryStream())
                    {
                        Image.FromFile(path).Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                        return Convert.ToBase64String(ms.ToArray());
                    }
                }
                catch { }
            }
            return null;
        }

        private static void populateIncludedExcludedStations(List<SdChannelDownload> list)
        {
            if (list != null)
            {
                foreach (SdChannelDownload station in list)
                {
                    if (station.StationID.StartsWith("-"))
                    {
                        excludedStations.Add(station.StationID.Replace("-", ""));
                    }
                    else
                    {
                        includedStations.Add(station.StationID);
                    }
                }
            }
        }
    }
}