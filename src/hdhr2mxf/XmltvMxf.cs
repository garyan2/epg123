using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using hdhr2mxf.HDHR;
using hdhr2mxf.MXF;
using hdhr2mxf.XMLTV;

namespace hdhr2mxf
{
    public class XmltvMxf
    {
        private static xmltv xmltv;

        public static bool BuildMxfFromXmltvGuide(List<hdhrDiscover> homeruns)
        {
            var concatenatedDeviceAuth = string.Empty;
            var channelsDone = new HashSet<string>();
            foreach (var device in homeruns.Select(homerun => Common.Api.ConnectDevice(homerun.DiscoverUrl)))
            {
                if (device == null || string.IsNullOrEmpty(device.LineupUrl))
                {
                    if (!string.IsNullOrEmpty(device?.DeviceAuth))
                    {
                        concatenatedDeviceAuth += device.DeviceAuth;
                    }
                    continue;
                }

                // determine lineup
                var model = device.ModelNumber.Split('-')[1];
                var deviceModel = model.Substring(model.Length - 2);
                var mxfLineup = Common.Mxf.With[0].GetLineup(deviceModel);

                // get channels
                var channels = Common.Api.GetDeviceChannels(device.LineupUrl);
                if (channels == null) continue;
                foreach (var channel in channels)
                {
                    // skip if channel has already been added
                    if (!channelsDone.Add($"{deviceModel} {channel.GuideNumber} {channel.GuideName}")) continue;

                    // determine number and subnumber
                    var digits = channel.GuideNumber.Split('.');
                    var number = int.Parse(digits[0]);
                    var subnumber = 0;
                    if (digits.Length > 1)
                    {
                        subnumber = int.Parse(digits[1]);
                    }

                    // determine final matchname
                    string matchName = null;
                    switch (deviceModel)
                    {
                        case "US":
                            matchName = $"OC:{number}:{subnumber}";
                            break;
                        case "DT":
                            matchName = $"DVBT:{channel.OriginalNetworkId.Replace(":", "")}:{channel.TransportStreamId.Replace(":", "")}:{channel.ProgramNumber.Replace(":", "")}";
                            break;
                        //case "CC":
                        //case "DC":
                        //case "IS":
                        default:
                            matchName = channel.GuideName;
                            break;
                    }

                    // add channel to the lineup
                    mxfLineup.channels.Add(new MxfChannel()
                    {
                        Lineup = mxfLineup.Id,
                        LineupUid = mxfLineup.UniqueId,
                        MatchName = channel.GuideName.ToUpper(),
                        Number = number,
                        SubNumber = subnumber,
                        Match = matchName,
                        IsHd = channel.Hd
                    });
                }

                concatenatedDeviceAuth += device.DeviceAuth;
            }

            if ((xmltv = Common.Api.GetHdhrXmltvGuide(concatenatedDeviceAuth)) == null)
            {
                return false;
            }

            BuildLineupServices();
            BuildScheduleEntries();
            return true;
        }

        private static void BuildLineupServices()
        {
            // build a comparison dictionary to determine service id for each channel from XMLTV
            var displayNameChannelIDs = new Dictionary<string, string>();
            foreach (var channel in xmltv.Channels)
            {
                foreach (var displayName in channel.DisplayNames)
                {
                    try
                    {
                        displayNameChannelIDs.Add(displayName.Text.ToUpper(), channel.Id);
                    }
                    catch
                    {
                        // ignored
                    }
                }
            }

            // find the stationid for each tuner channel for the MXF
            foreach (var lineup in Common.Mxf.With[0].Lineups)
            {
                foreach (var channel in lineup.channels)
                {
                    var match = $"{channel.Number}{(channel.SubNumber > 0 ? "." + channel.SubNumber : string.Empty)} {channel.MatchName.ToUpper()}";
                    if (!displayNameChannelIDs.TryGetValue(match, out var serviceid) &&
                        !displayNameChannelIDs.TryGetValue(match.Split(' ')[0], out serviceid)) continue;
                    channel.StationId = GetStationIdFromChannelId(serviceid);
                    var service = Common.Mxf.With[0].GetService(channel.StationId);

                    channel.Service = service.Id;
                    service.Name = channel.MatchName;
                    service.MxfScheduleEntries.Service = service.Id;
                    service.IsHd = channel.IsHd;
                }
            }

            // complete the service information
            foreach (var channel in xmltv.Channels)
            {
                var service = Common.Mxf.With[0].GetService(GetStationIdFromChannelId(channel.Id));

                // add guide image if available
                if (!Common.NoLogos && channel.Icons != null && channel.Icons.Count > 0)
                {
                    service.LogoImage = Common.Mxf.With[0].GetGuideImage(channel.Icons[0].Src).Id;
                }

                // this makes the assumption that the callsign will be first
                service.CallSign = channel.DisplayNames.First().Text;

                //// this makes the assumption that the affiliate will be last
                //string text = channel.DisplayNames.Last().Text.Split(' ')[0];
                //if (!int.TryParse(text, out int dummy1) && !double.TryParse(text, out double dummy2))
                //{
                //    service.Affiliate = Common.mxf.With[0].getAffiliateId(channel.DisplayNames.Last().Text);
                //}
            }

            // clean up the channels
            foreach (var lineup in Common.Mxf.With[0].Lineups)
            {
                var channelsToRemove = new List<MxfChannel>();
                foreach (var channel in lineup.channels)
                {
                    if (!string.IsNullOrEmpty(channel.Match))
                    {
                        channel.MatchName = channel.Match;
                    }
                    if (string.IsNullOrEmpty(channel.Service))
                    {
                        channelsToRemove.Add(channel);
                    }
                }
                if (channelsToRemove.Count > 0)
                {
                    foreach (var channel in channelsToRemove)
                    {
                        lineup.channels.Remove(channel);
                    }
                }
            }
        }

        private static MxfProgram.ProgramEpisodeInfo GetProgramEpisodeInformation(List<XmltvEpisodeNum> xmltvEpisodeNumbers)
        {
            var ret = new MxfProgram.ProgramEpisodeInfo();
            foreach (var xmltvEpisodeNum in xmltvEpisodeNumbers)
            {
                switch (xmltvEpisodeNum.System.ToLower())
                {
                    case "dd_progid":
                        var m = Regex.Match(xmltvEpisodeNum.Text, @"(MV|SH|EP|SP)[0-9]{8}.[0-9]{4}");
                        if (m.Length > 0)
                        {
                            ret.TmsId = xmltvEpisodeNum.Text.ToUpper().Replace(".", "").Substring(0, 14);
                            ret.Type = ret.TmsId.Substring(0, 2);
                            ret.SeriesId = ret.TmsId.Substring(2, 8);
                            ret.ProductionNumber = int.Parse(ret.TmsId.Substring(10, 4));
                        }
                        break;
                    case "xmltv_ns":
                        var se1 = xmltvEpisodeNum.Text.Split('.');
                        int.TryParse((se1[0].Split('/'))[0], out ret.SeasonNumber);
                        ++ret.SeasonNumber;
                        int.TryParse((se1[1].Split('/'))[0], out ret.EpisodeNumber);
                        ++ret.EpisodeNumber;
                        int.TryParse((se1[2].Split('/'))[0], out ret.PartNumber);
                        ++ret.PartNumber;
                        if (!se1[2].Contains("/") || !int.TryParse((se1[2].Split('/'))[1], out ret.NumberOfParts))
                        {
                            ret.NumberOfParts = 1;
                        }
                        break;
                    case "onscreen":
                    case "common":
                        var se2 = xmltvEpisodeNum.Text.ToLower().Substring(1).Split('e');
                        if (se2.Length == 2)
                        {
                            int.TryParse(se2[0], out ret.SeasonNumber);
                            int.TryParse(se2[1], out ret.EpisodeNumber);
                        }
                        break;
                }
            }
            return ret;
        }

        #region ========== Schedule Entries ==========
        private static void BuildScheduleEntries()
        {
            foreach (var program in xmltv.Programs)
            {
                // determine which service the schedule entry is for
                var mxfService = Common.Mxf.With[0].Services.SingleOrDefault(arg => arg.StationId == GetStationIdFromChannelId(program.Channel));
                if (mxfService == null) continue;

                // determine start time
                var dtStart = DateTime.ParseExact(program.Start, "yyyyMMddHHmmss zzz", CultureInfo.InvariantCulture).ToUniversalTime();
                var dtStop = DateTime.ParseExact(program.Stop, "yyyyMMddHHmmss zzz", CultureInfo.InvariantCulture).ToUniversalTime();
                var startTime = dtStart.ToString("yyyy-MM-ddTHH:mm:ss");
                if (dtStart == mxfService.MxfScheduleEntries.EndTime)
                {
                    startTime = null;
                }

                // prepopulate some of the program
                var mxfProgram = new MxfProgram()
                {
                    Index = Common.Mxf.With[0].Programs.Count + 1,
                    EpisodeInfo = GetProgramEpisodeInformation(program.EpisodeNums),
                    IsPremiere = (program.Premiere != null) ? "true" : null,
                    IsSeasonPremiere = (program.Premiere?.Text != null && program.Premiere.Text.ToLower().Equals("season premiere")) ? "true" : null,
                    IsSeriesPremiere = (program.Premiere?.Text != null && program.Premiere.Text.ToLower().Equals("series premiere")) ? "true" : null,
                    NewDate = (program.New != null) ? dtStart.ToLocalTime().ToString("yyyy-MM-dd") : null,
                    ActorRole = new List<MxfPersonRank>(),
                    WriterRole = new List<MxfPersonRank>(),
                    GuestActorRole = new List<MxfPersonRank>(),
                    HostRole = new List<MxfPersonRank>(),
                    ProducerRole = new List<MxfPersonRank>(),
                    DirectorRole = new List<MxfPersonRank>()
                    //IsSeasonFinale = ,
                    //IsSeriesFinale = ,
                };

                // if dd_progid is not valid, don't add it
                if (string.IsNullOrEmpty(mxfProgram.EpisodeInfo.TmsId)) continue;

                // populate the schedule entry and create program entry as required
                mxfService.MxfScheduleEntries.ScheduleEntry.Add(new MxfScheduleEntry()
                {
                    AudioFormat = GetAudioFormat(program.Audio),
                    Duration = (int)((dtStop - dtStart).TotalSeconds),
                    IsCc = program.Subtitles.Count > 0 ? "true" : null,
                    IsHdtv = (program.Video?.Quality != null && program.Video.Quality.ToLower().Equals("hdtv")) ? "true" : mxfService.IsHd ? "true" : null,
                    IsLive = (program.Live != null) ? "true" : null,
                    IsPremiere = (program.Premiere != null) ? "true" : null,
                    IsRepeat = (program.New == null && !mxfProgram.EpisodeInfo.Type.Equals("MV")) ? "true" : null,
                    Part = (mxfProgram.EpisodeInfo.NumberOfParts > 1) ? mxfProgram.EpisodeInfo.PartNumber.ToString() : null,
                    Parts = (mxfProgram.EpisodeInfo.NumberOfParts > 1) ? mxfProgram.EpisodeInfo.NumberOfParts.ToString() : null,
                    Program = Common.Mxf.With[0].GetProgram(mxfProgram.EpisodeInfo.TmsId, mxfProgram).Id,
                    StartTime = startTime,
                    TvRating = GetUsTvRating(program.Rating)
                    //Is3D = ,
                    //IsBlackout = ,
                    //IsClassroom = ,
                    //IsDelay = ,
                    //IsDvs = ,
                    //IsEnhanced = ,
                    //IsFinale = ,
                    //IsHdtvSimulCast = ,
                    //IsInProgress = ,
                    //IsLetterbox = ,
                    //IsLiveSports = ,
                    //IsSap = ,
                    //IsSubtitled = ,
                    //IsTape = ,
                    //IsSigned = 
                });

                BuildProgram(mxfProgram.EpisodeInfo.TmsId, program);
            }
        }

        private static string GetStationIdFromChannelId(string channelid)
        {
            return channelid.Split('.')[0];
        }

        #endregion

        #region ========== Program ==========
        private static void BuildProgram(string tmsId, XmltvProgramme program)
        {
            var mxfProgram = Common.Mxf.With[0].GetProgram(tmsId);
            if (!string.IsNullOrEmpty(mxfProgram.Title)) return;

            mxfProgram.Title = program.Titles[0].Text;
            if (mxfProgram.EpisodeInfo.NumberOfParts > 1)
            {
                var partofparts = $" ({mxfProgram.EpisodeInfo.PartNumber}/{mxfProgram.EpisodeInfo.NumberOfParts})";
                mxfProgram.Title = mxfProgram.Title.Replace(partofparts, "") + partofparts;
            }
            mxfProgram.EpisodeTitle = (program.SubTitles.Count > 0) ? program.SubTitles[0].Text : null;
            mxfProgram.Description = (program.Descriptions.Count > 0) ? program.Descriptions[0].Text : null;
            mxfProgram.Language = program.Language?.Text ?? program.Titles[0].Language;
            mxfProgram.SeasonNumber = (mxfProgram.EpisodeInfo.SeasonNumber > 0) ? mxfProgram.EpisodeInfo.SeasonNumber.ToString() : null;
            mxfProgram.EpisodeNumber = (mxfProgram.EpisodeInfo.EpisodeNumber > 0) ? mxfProgram.EpisodeInfo.EpisodeNumber.ToString() : null;

            mxfProgram.IsAction = Common.ListContains(program.Categories, "Action");
            mxfProgram.IsComedy = Common.ListContains(program.Categories, "Comedy");
            mxfProgram.IsDocumentary = Common.ListContains(program.Categories, "Documentary");
            mxfProgram.IsDrama = Common.ListContains(program.Categories, "Drama");
            mxfProgram.IsEducational = Common.ListContains(program.Categories, "Educational");
            mxfProgram.IsHorror = Common.ListContains(program.Categories, "Horror");
            //mxfProgram.IsIndy = null;
            mxfProgram.IsKids = Common.ListContains(program.Categories, "Children") ?? Common.ListContains(program.Categories, "Family");
            mxfProgram.IsMusic = Common.ListContains(program.Categories, "Music");
            mxfProgram.IsNews = Common.ListContains(program.Categories, "News");
            mxfProgram.IsReality = Common.ListContains(program.Categories, "Reality");
            mxfProgram.IsRomance = Common.ListContains(program.Categories, "Romance");
            mxfProgram.IsScienceFiction = Common.ListContains(program.Categories, "Science Fiction");
            mxfProgram.IsSoap = Common.ListContains(program.Categories, "Soap");
            mxfProgram.IsThriller = Common.ListContains(program.Categories, "Suspense") ?? Common.ListContains(program.Categories, "Thriller");

            //mxfProgram.IsPremiere = ;
            //mxfProgram.IsSeasonFinale = ;
            //mxfProgram.IsSeasonPremiere = ;
            //mxfProgram.IsSeriesFinale = ;
            //mxfProgram.IsSeriesPremiere = ;

            //mxfProgram.IsLimitedSeries = null;
            mxfProgram.IsMiniseries = Common.ListContains(program.Categories, "Miniseries");
            mxfProgram.IsMovie = mxfProgram.EpisodeInfo.Type.Equals("MV") ? "true" : Common.ListContains(program.Categories, "Movie");
            mxfProgram.IsPaidProgramming = Common.ListContains(program.Categories, "Paid Programming");
            //mxfProgram.IsProgramEpisodic = null;
            //mxfProgram.IsSerial = null;
            mxfProgram.IsSeries = Common.ListContains(program.Categories, "Series") ?? Common.ListContains(program.Categories, "Sports non-event");
            mxfProgram.IsShortFilm = Common.ListContains(program.Categories, "Short Film");
            mxfProgram.IsSpecial = Common.ListContains(program.Categories, "Special");
            mxfProgram.IsSports = Common.ListContains(program.Categories, "Sports event");
            if (string.IsNullOrEmpty(mxfProgram.IsSeries + mxfProgram.IsMiniseries + mxfProgram.IsMovie + mxfProgram.IsPaidProgramming + mxfProgram.IsShortFilm + mxfProgram.IsSpecial + mxfProgram.IsSports))
            {
                mxfProgram.IsSeries = "true";
            }

            // determine if generic
            if (mxfProgram.EpisodeInfo.Type.Equals("SH") && (!string.IsNullOrEmpty(mxfProgram.IsSeries) || !string.IsNullOrEmpty(mxfProgram.IsSports)) && string.IsNullOrEmpty(mxfProgram.IsMiniseries) && string.IsNullOrEmpty(mxfProgram.IsPaidProgramming))
            {
                mxfProgram.IsGeneric = "true";
            }

            // determine keywords
            Common.DetermineProgramKeywords(ref mxfProgram, program.Categories);

            // take care of episode original air date or movie year
            if (!string.IsNullOrEmpty(mxfProgram.IsMovie) && !string.IsNullOrEmpty(program.Date))
            {
                mxfProgram.Year = program.Date.Substring(0, 4);
            }
            else if (!string.IsNullOrEmpty(program.Date) && program.Date.Length >= 8)
            {
                mxfProgram.OriginalAirdate = program.Date.Substring(0, 4) + "-" + program.Date.Substring(4, 2) + "-" + program.Date.Substring(6, 2);
            }
            else if (program.PreviouslyShown != null)
            {
                mxfProgram.OriginalAirdate = "1970-01-01";
            }

            // handle series specific info
            if (string.IsNullOrEmpty(mxfProgram.IsMovie))
            {
                var mxfSeriesInfo = Common.Mxf.With[0].GetSeriesInfo(mxfProgram.EpisodeInfo.SeriesId);
                if (string.IsNullOrEmpty(mxfSeriesInfo.Title))
                {
                    mxfSeriesInfo.Title = mxfProgram.Title;
                    if (program.Icons.Count > 0)
                    {
                        mxfSeriesInfo.GuideImage = Common.Mxf.With[0].GetGuideImage(program.Icons[0].Src)?.Id;
                    }
                }
                if (mxfProgram.EpisodeInfo.Type.Equals("SH"))
                {
                    if (string.IsNullOrEmpty(mxfSeriesInfo.Description))
                    {
                        mxfSeriesInfo.Description = mxfProgram.Description;
                    }
                }
                mxfProgram.Series = mxfSeriesInfo.Id;

                // if there is a season number, create as seasonInfo entry
                if (!string.IsNullOrEmpty(mxfProgram.SeasonNumber))
                {
                    mxfProgram.Season = Common.Mxf.With[0].GetSeasonId(mxfProgram.EpisodeInfo.SeriesId, mxfProgram.SeasonNumber);
                }
            }
            // handle movie specific info
            else
            {
                // grab the movie poster
                var icon = program.Icons.SingleOrDefault(arg => arg.Src.Contains("/posters/"));
                if (icon != null)
                {
                    mxfProgram.GuideImage = Common.Mxf.With[0].GetGuideImage(icon.Src).Id;
                }
                else if (program.Icons.Count > 0)
                {
                    mxfProgram.GuideImage = Common.Mxf.With[0].GetGuideImage(program.Icons[0].Src).Id;
                }

                // get the star ratings
                if (program.StarRating.Count > 0)
                {
                    mxfProgram.HalfStars = GetStarRating(program.StarRating);
                }
                else if (program.Descriptions.Count > 0)
                {
                    var m = Regex.Match(program.Descriptions[0].Text, @"\d\.\d/\d\.\d");
                    {
                        var nums = m.Value.Split('/');
                        if (nums.Length == 2)
                        {
                            if (double.TryParse(nums[0], out var numerator) && double.TryParse(nums[1], out var denominator))
                            {
                                var halfstars = (int)((numerator / denominator) * 8);
                                mxfProgram.HalfStars = halfstars.ToString();
                                mxfProgram.Description = mxfProgram.Description.Replace($" ({m.Value})", "");
                            }
                        }
                    }
                }

                // handle movie ratings and advisories
                GetUsMovieRatings(program.Rating, mxfProgram);
            }

            // handle cast and crew
            if (program.Credits == null) return;
            foreach (var person in program.Credits.Directors)
            {
                mxfProgram.DirectorRole.Add(new MxfPersonRank()
                {
                    Person = Common.Mxf.With[0].GetPersonId(person),
                });
            }
            foreach (var person in program.Credits.Actors)
            {
                mxfProgram.ActorRole.Add(new MxfPersonRank()
                {
                    Person = Common.Mxf.With[0].GetPersonId(person.Actor),
                    Character = person.Role,
                });
            }
            foreach (var person in program.Credits.Writers)
            {
                mxfProgram.WriterRole.Add(new MxfPersonRank()
                {
                    Person = Common.Mxf.With[0].GetPersonId(person),
                });
            }
            foreach (var person in program.Credits.Adapters)
            {
                mxfProgram.WriterRole.Add(new MxfPersonRank()
                {
                    Person = Common.Mxf.With[0].GetPersonId(person),
                });
            }
            foreach (var person in program.Credits.Producers)
            {
                mxfProgram.ProducerRole.Add(new MxfPersonRank()
                {
                    Person = Common.Mxf.With[0].GetPersonId(person),
                });
            }
            foreach (var person in program.Credits.Composers)
            {
                mxfProgram.ProducerRole.Add(new MxfPersonRank()
                {
                    Person = Common.Mxf.With[0].GetPersonId(person),
                });
            }
            foreach (var person in program.Credits.Editors)
            {
                mxfProgram.HostRole.Add(new MxfPersonRank()
                {
                    Person = Common.Mxf.With[0].GetPersonId(person),
                });
            }
            foreach (var person in program.Credits.Presenters)
            {
                mxfProgram.HostRole.Add(new MxfPersonRank()
                {
                    Person = Common.Mxf.With[0].GetPersonId(person),
                });
            }
            foreach (var person in program.Credits.Commentators)
            {
                mxfProgram.HostRole.Add(new MxfPersonRank()
                {
                    Person = Common.Mxf.With[0].GetPersonId(person),
                });
            }
            foreach (var person in program.Credits.Guests)
            {
                mxfProgram.GuestActorRole.Add(new MxfPersonRank()
                {
                    Person = Common.Mxf.With[0].GetPersonId(person),
                });
            }
        }

        private static string GetAudioFormat(XmltvAudio audio)
        {
            if (audio == null) return null;
            switch (audio.Stereo.ToLower())
            {
                case "mono":
                    return "1";
                case "stereo":
                    return "2";
                case "dolby":
                    return "3";
                case "dolby digital":
                    return "4";
                case "thx":
                    return "5";
            }
            return null;
        }

        private static void GetUsMovieRatings(List<XmltvRating> ratings, MxfProgram mxfProgram)
        {
            foreach (var rating in ratings.Where(rating => !string.IsNullOrEmpty(rating.System)))
            {
                if (rating.System.ToLower().Equals("motion picture association of america") || rating.System.ToLower().Equals("mpaa"))
                {
                    switch (rating.Value.ToLower().Replace("-", ""))
                    {
                        case "g":
                            mxfProgram.MpaaRating = "1";
                            break;
                        case "pg":
                            mxfProgram.MpaaRating = "2";
                            break;
                        case "pg13":
                            mxfProgram.MpaaRating = "3";
                            break;
                        case "r":
                            mxfProgram.MpaaRating = "4";
                            break;
                        case "nc17":
                            mxfProgram.MpaaRating = "5";
                            break;
                        case "x":
                            mxfProgram.MpaaRating = "6";
                            break;
                        case "nr":
                            mxfProgram.MpaaRating = "7";
                            break;
                        case "ao":
                            mxfProgram.MpaaRating = "8";
                            break;
                    }
                }
                else if (rating.System.ToLower().Equals("advisory"))
                {
                    switch (rating.Value.ToLower())
                    {
                        case "adult situations":
                            mxfProgram.HasAdult = "true";
                            break;
                        case "brief nudity":
                            mxfProgram.HasBriefNudity = "true";
                            break;
                        case "graphic language":
                            mxfProgram.HasGraphicLanguage = "true";
                            break;
                        case "graphic violence":
                            mxfProgram.HasGraphicViolence = "true";
                            break;
                        case "adult language":
                            mxfProgram.HasLanguage = "true";
                            break;
                        case "mild violence":
                            mxfProgram.HasMildViolence = "true";
                            break;
                        case "nudity":
                            mxfProgram.HasNudity = "true";
                            break;
                        case "rape":
                            mxfProgram.HasRape = "true";
                            break;
                        case "strong sexual content":
                            mxfProgram.HasStrongSexualContent = "true";
                            break;
                        case "violence":
                            mxfProgram.HasViolence = "true";
                            break;
                    }
                }
            }
        }

        private static string GetStarRating(IEnumerable<XmltvRating> ratings)
        {
            foreach (var rating in ratings)
            {
                if (rating.Value == null) continue;
                var nums = rating.Value.Split('/');
                if (nums.Length != 2) continue;
                var denominator = double.Parse(nums[1]);
                if (denominator == 0) return null;

                var halfStars = (int)((double.Parse(nums[0]) / denominator) * 8 + 0.125);
                if (halfStars > 0) return halfStars.ToString();
            }
            return null;
        }

        private static string GetUsTvRating(List<XmltvRating> ratings)
        {
            foreach (var rating in ratings.Where(rating => string.IsNullOrEmpty(rating.System) || rating.System.ToLower().Equals("vchip") || rating.System.ToLower().Equals("usa parental rating")))
            {
                switch (rating.Value.ToLower().Replace("-", ""))
                {
                    case "tvy":
                        return "1";
                    case "tvy7":
                        return "2";
                    case "tvg":
                        return "3";
                    case "tvpg":
                        return "4";
                    case "tv14":
                        return "5";
                    case "tvma":
                        return "6";
                }
            }

            return null;
        }
        #endregion
    }
}
