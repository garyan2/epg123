using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using HDHomeRunTV;
using XmltvXml;
using MxfXml;

namespace hdhr2mxf
{
    public class XmltvMxf
    {
        private static XMLTV xmltv;

        public static bool BuildMxfFromXmltvGuide(List<HDHRDiscover> homeruns)
        {
            string concatenatedDeviceAuth = string.Empty;
            HashSet<string> ChannelsDone = new HashSet<string>();
            foreach (HDHRDiscover homerun in homeruns)
            {
                // connect to the device
                HDHRDevice device = Common.api.ConnectDevice(homerun.DiscoverURL);
                if (device == null || string.IsNullOrEmpty(device.LineupURL)) continue;

                // determine lineup
                string model = device.ModelNumber.Split('-')[1];
                string deviceModel = model.Substring(model.Length - 2);
                MxfLineup mxfLineup = Common.mxf.With[0].getLineup(deviceModel);

                // get channels
                List<HDHRChannel> channels = Common.api.GetDeviceChannels(device.LineupURL);
                if (channels == null) continue;
                foreach (HDHRChannel channel in channels)
                {
                    // skip if channel has already been added
                    if (!ChannelsDone.Add(string.Format("{0} {1} {2}", deviceModel, channel.GuideNumber, channel.GuideName))) continue;

                    // determine number and subnumber
                    string[] digits = channel.GuideNumber.Split('.');
                    int number = int.Parse(digits[0]);
                    int subnumber = 0;
                    if (digits.Length > 1)
                    {
                        subnumber = int.Parse(digits[1]);
                    }

                    // determine final matchname
                    string matchName = null;
                    switch (deviceModel)
                    {
                        case "US":
                            matchName = string.Format("OC:{0}:{1}", number, subnumber);
                            break;
                        case "DT":
                            matchName = string.Format("DVBT:{0}:{1}:{2}", 
                                channel.OriginalNetworkID.Replace(":", ""), channel.TransportStreamID.Replace(":", ""), channel.ProgramNumber.Replace(":", ""));
                            break;
                        case "CC":
                        case "DC":
                        case "IS":
                        default:
                            break;
                    }

                    // add channel to the lineup
                    mxfLineup.channels.Add(new MxfChannel()
                    {
                        Lineup = mxfLineup.Id,
                        lineupUid = mxfLineup.uid_,
                        MatchName = channel.GuideName.ToUpper(),
                        Number = number,
                        SubNumber = subnumber,
                        match = matchName,
                        isHD = channel.HD
                    });
                }

                concatenatedDeviceAuth += device.DeviceAuth;
            }

            if ((xmltv = Common.api.GetHdhrXmltvGuide(concatenatedDeviceAuth)) == null)
            {
                return false;
            }

            BuildLineupServices();
            BuildScheduleEntries();
            return true;
        }

        private static bool BuildLineupServices()
        {
            // build a comparison dictionary to determine service id for each channel from XMLTV
            Dictionary<string, string> displayNameChannelIDs = new Dictionary<string, string>();
            foreach (XmltvChannel channel in xmltv.Channels)
            {
                foreach (XmltvText displayName in channel.DisplayNames)
                {
                    try
                    {
                        displayNameChannelIDs.Add(displayName.Text.ToUpper(), channel.Id);
                    }
                    catch { }
                }
            }

            // find the stationid for each tuner channel for the MXF
            foreach (MxfLineup lineup in Common.mxf.With[0].Lineups)
            {
                foreach (MxfChannel channel in lineup.channels)
                {
                    string match = string.Format("{0}{1} {2}", channel.Number, (channel.SubNumber > 0) ? "." + channel.SubNumber.ToString() : string.Empty, channel.MatchName.ToUpper());
                    if (displayNameChannelIDs.TryGetValue(match, out string serviceid))
                    {
                        channel.stationId = GetStationIdFromChannelId(serviceid);
                        MxfService service = Common.mxf.With[0].getService(channel.stationId);

                        channel.Service = service.Id;
                        service.Name = channel.MatchName;
                        service.mxfScheduleEntries.Service = service.Id;
                        service.isHD = channel.isHD;
                    }
                }
            }

            // complete the service information
            foreach (XmltvChannel channel in xmltv.Channels)
            {
                MxfService service = Common.mxf.With[0].getService(GetStationIdFromChannelId(channel.Id));

                // add guide image if available
                if (channel.Icons != null && channel.Icons.Count > 0)
                {
                    service.LogoImage = Common.mxf.With[0].getGuideImage(channel.Icons[0].src).Id;
                }

                // this makes the assumption that the callsign will be first
                service.CallSign = channel.DisplayNames.First().Text;

                // this makes the assumption that the affiliate will be last
                string text = channel.DisplayNames.Last().Text.Split(' ')[0];
                if (!int.TryParse(text, out int dummy1) && !double.TryParse(text, out double dummy2))
                {
                    service.Affiliate = Common.mxf.With[0].getAffiliateId(channel.DisplayNames.Last().Text);
                }
            }

            // clean up the channels
            foreach (MxfLineup lineup in Common.mxf.With[0].Lineups)
            {
                List<MxfChannel> channelsToRemove = new List<MxfChannel>();
                foreach (MxfChannel channel in lineup.channels)
                {
                    if (!string.IsNullOrEmpty(channel.match))
                    {
                        channel.MatchName = channel.match;
                    }
                    if (string.IsNullOrEmpty(channel.Service))
                    {
                        channelsToRemove.Add(channel);
                    }
                }
                if (channelsToRemove.Count > 0)
                {
                    foreach (MxfChannel channel in channelsToRemove)
                    {
                        lineup.channels.Remove(channel);
                    }
                }
            }

            return true;
        }

        private static MxfProgram.ProgramEpisodeInfo GetProgramEpisodeInformation(List<XmltvEpisodeNum> xmltvEpisodeNumbers)
        {
            MxfProgram.ProgramEpisodeInfo ret = new MxfProgram.ProgramEpisodeInfo();
            foreach (XmltvEpisodeNum xmltvEpisodeNum in xmltvEpisodeNumbers)
            {
                switch (xmltvEpisodeNum.System.ToLower())
                {
                    case "dd_progid":
                        ret.TMSID = xmltvEpisodeNum.Text.ToUpper().Replace(".", "").Substring(0, 14);
                        ret.Type = ret.TMSID.Substring(0, 2);
                        ret.SeriesID = ret.TMSID.Substring(2, 8);
                        ret.ProductionNumber = int.Parse(ret.TMSID.Substring(10, 4));
                        break;
                    case "xmltv_ns":
                        string[] se1 = xmltvEpisodeNum.Text.Split('.');
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
                        string[] se2 = xmltvEpisodeNum.Text.ToLower().Substring(1).Split('e');
                        if (se2.Length == 2)
                        {
                            int.TryParse(se2[0], out ret.SeasonNumber);
                            int.TryParse(se2[1], out ret.EpisodeNumber);
                        }
                        break;
                    default:
                        break;
                }
            }
            return ret;
        }

        #region ========== Schedule Entries ==========
        private static bool BuildScheduleEntries()
        {
            foreach (XmltvProgramme program in xmltv.Programs)
            {
                // determine which service the schedule entry is for
                MxfService mxfService = Common.mxf.With[0].getService(GetStationIdFromChannelId(program.Channel));

                // determine start time
                DateTime dtStart = DateTime.ParseExact(program.Start, "yyyyMMddHHmmss zzz", CultureInfo.InvariantCulture).ToUniversalTime();
                DateTime dtStop = DateTime.ParseExact(program.Stop, "yyyyMMddHHmmss zzz", CultureInfo.InvariantCulture).ToUniversalTime();
                string startTime = dtStart.ToString("yyyy-MM-ddTHH:mm:ss");
                if (dtStart == mxfService.mxfScheduleEntries.endTime)
                {
                    startTime = null;
                }

                // prepopulate some of the program
                MxfProgram mxfProgram = new MxfProgram()
                {
                    index = Common.mxf.With[0].Programs.Count + 1,
                    episodeInfo = GetProgramEpisodeInformation(program.EpisodeNums),
                    IsPremiere = (program.Premiere != null) ? "true" : null,
                    IsSeasonPremiere = (program.Premiere?.Text != null && program.Premiere.Text.ToLower().Equals("season premiere")) ? "true" : null,
                    IsSeriesPremiere = (program.Premiere?.Text != null && program.Premiere.Text.ToLower().Equals("series premiere")) ? "true" : null,
                    _newDate = (program.New != null) ? dtStart.ToLocalTime().ToString("yyyy-MM-dd") : null,
                    ActorRole = new List<MxfPersonRank>(),
                    WriterRole = new List<MxfPersonRank>(),
                    GuestActorRole = new List<MxfPersonRank>(),
                    HostRole = new List<MxfPersonRank>(),
                    ProducerRole = new List<MxfPersonRank>(),
                    DirectorRole = new List<MxfPersonRank>()
                    //IsSeasonFinale = ,
                    //IsSeriesFinale = ,
                };

                // populate the schedule entry and create program entry as required
                mxfService.mxfScheduleEntries.ScheduleEntry.Add(new MxfScheduleEntry()
                {
                    AudioFormat = GetAudioFormat(program.Audio),
                    Duration = (int)((dtStop - dtStart).TotalSeconds),
                    IsCC = program.Subtitles.Count > 0 ? "true" : null,
                    IsHdtv = (program.Video?.Quality != null && program.Video.Quality.ToLower().Equals("hdtv")) ? "true" : mxfService.isHD ? "true" : null,
                    IsLive = (program.Live != null) ? "true" : null,
                    IsPremiere = (program.Premiere != null) ? "true" : null,
                    IsRepeat = (program.New == null && !mxfProgram.episodeInfo.Type.Equals("MV")) ? "true" : null,
                    Part = (mxfProgram.episodeInfo.NumberOfParts > 1) ? mxfProgram.episodeInfo.PartNumber.ToString() : null,
                    Parts = (mxfProgram.episodeInfo.NumberOfParts > 1) ? mxfProgram.episodeInfo.NumberOfParts.ToString() : null,
                    Program = Common.mxf.With[0].getProgram(mxfProgram.episodeInfo.TMSID, mxfProgram).Id,
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

                BuildProgram(mxfProgram.episodeInfo.TMSID, program);
            }
            return true;
        }

        private static string GetStationIdFromChannelId(string channelid)
        {
            // just looking for a 5-6 digit number
            Match match = Regex.Match(channelid, "[0-9]{5,6}");
            if (match.Length > 0)
            {
                return match.Value;
            }
            return channelid;
        }

        #endregion

        #region ========== Program ==========
        private static bool BuildProgram(string tmsId, XmltvProgramme program)
        {
            MxfProgram mxfProgram = Common.mxf.With[0].getProgram(tmsId);
            if (!string.IsNullOrEmpty(mxfProgram.Title)) return true;

            mxfProgram.Title = program.Titles[0].Text;
            if (mxfProgram.episodeInfo.NumberOfParts > 1)
            {
                string partofparts = string.Format(" ({0}/{1})", mxfProgram.episodeInfo.PartNumber, mxfProgram.episodeInfo.NumberOfParts);
                mxfProgram.Title = mxfProgram.Title.Replace(partofparts, "") + partofparts;
            }
            mxfProgram.EpisodeTitle = (program.SubTitles.Count > 0) ? program.SubTitles[0].Text : null;
            mxfProgram.Description = (program.Descriptions.Count > 0) ? program.Descriptions[0].Text : null;
            mxfProgram.Language = program.Language?.Text ?? program.Titles[0].Language;

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
            mxfProgram.IsMovie = mxfProgram.episodeInfo.Type.Equals("MV") ? "true" : Common.ListContains(program.Categories, "Movie");
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
            if (mxfProgram.episodeInfo.Type.Equals("SH") && (!string.IsNullOrEmpty(mxfProgram.IsSeries) || !string.IsNullOrEmpty(mxfProgram.IsSports)) && string.IsNullOrEmpty(mxfProgram.IsMiniseries) && string.IsNullOrEmpty(mxfProgram.IsPaidProgramming))
            {
                mxfProgram.IsGeneric = "true";
            }

            // determine keywords
            Common.determineProgramKeywords(ref mxfProgram, program.Categories);

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
                MxfSeriesInfo mxfSeriesInfo = Common.mxf.With[0].getSeriesInfo(mxfProgram.episodeInfo.SeriesID);
                if (string.IsNullOrEmpty(mxfSeriesInfo.Title))
                {
                    mxfSeriesInfo.Title = mxfProgram.Title;
                    if (program.Icons.Count > 0)
                    {
                        mxfSeriesInfo.GuideImage = Common.mxf.With[0].getGuideImage(program.Icons[0].src)?.Id;
                    }
                }
                if (mxfProgram.episodeInfo.Type.Equals("SH"))
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
                    mxfProgram.Season = Common.mxf.With[0].getSeasonId(mxfProgram.episodeInfo.SeriesID, mxfProgram.SeasonNumber);
                }
            }
            // handle movie specific info
            else
            {
                // grab the movie poster
                foreach (XmltvIcon icon in program.Icons)
                {
                    if (icon.src.Contains("/posters/"))
                    {
                        mxfProgram.GuideImage = Common.mxf.With[0].getGuideImage(icon.src)?.Id;
                        break;
                    }
                    else if (string.IsNullOrEmpty(mxfProgram.GuideImage))
                    {
                        mxfProgram.GuideImage = Common.mxf.With[0].getGuideImage(icon.src)?.Id;
                    }
                }

                // get the star ratings
                if (program.StarRating.Count > 0)
                {
                    mxfProgram.HalfStars = GetStarRating(program.StarRating);
                }
                //else
                {
                    Match m = Regex.Match(program.Descriptions[0].Text, @"\d\.\d/\d\.\d");
                    if (m != null)
                    {
                        string[] nums = m.Value.Split('/');
                        if (double.TryParse(nums[0], out double numerator) && double.TryParse(nums[1], out double denominator))
                        {
                            int halfstars = (int)((numerator / denominator) * 8);
                            mxfProgram.HalfStars = halfstars.ToString();
                            mxfProgram.Description = mxfProgram.Description.Replace(string.Format(" ({0})", m.Value), "");
                        }
                    }
                }

                // handle movie ratings and advisories
                GetUsMovieRatings(program.Rating, mxfProgram);
            }

            // handle cast and crew
            if (program.Credits != null)
            {
                foreach (string person in program.Credits.Directors)
                {
                    mxfProgram.DirectorRole.Add(new MxfPersonRank()
                    {
                        Person = Common.mxf.With[0].getPersonId(person),
                    });
                }
                foreach (XmltvActor person in program.Credits.Actors)
                {
                    mxfProgram.ActorRole.Add(new MxfPersonRank()
                    {
                        Person = Common.mxf.With[0].getPersonId(person.Actor),
                        Character = person.Role,
                    });
                }
                foreach (string person in program.Credits.Writers)
                {
                    mxfProgram.WriterRole.Add(new MxfPersonRank()
                    {
                        Person = Common.mxf.With[0].getPersonId(person),
                    });
                }
                foreach (string person in program.Credits.Adapters)
                {
                    mxfProgram.WriterRole.Add(new MxfPersonRank()
                    {
                        Person = Common.mxf.With[0].getPersonId(person),
                    });
                }
                foreach (string person in program.Credits.Producers)
                {
                    mxfProgram.ProducerRole.Add(new MxfPersonRank()
                    {
                        Person = Common.mxf.With[0].getPersonId(person),
                    });
                }
                foreach (string person in program.Credits.Composers)
                {
                    mxfProgram.ProducerRole.Add(new MxfPersonRank()
                    {
                        Person = Common.mxf.With[0].getPersonId(person),
                    });
                }
                foreach (string person in program.Credits.Editors)
                {
                    mxfProgram.HostRole.Add(new MxfPersonRank()
                    {
                        Person = Common.mxf.With[0].getPersonId(person),
                    });
                }
                foreach (string person in program.Credits.Presenters)
                {
                    mxfProgram.HostRole.Add(new MxfPersonRank()
                    {
                        Person = Common.mxf.With[0].getPersonId(person),
                    });
                }
                foreach (string person in program.Credits.Commentators)
                {
                    mxfProgram.HostRole.Add(new MxfPersonRank()
                    {
                        Person = Common.mxf.With[0].getPersonId(person),
                    });
                }
                foreach (string person in program.Credits.Guests)
                {
                    mxfProgram.GuestActorRole.Add(new MxfPersonRank()
                    {
                        Person = Common.mxf.With[0].getPersonId(person),
                    });
                }
            }

            return true;
        }

        private static string GetAudioFormat(XmltvAudio audio)
        {
            if (audio != null)
            {
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
                    default:
                        break;
                }
            }
            return null;
        }

        private static void GetUsMovieRatings(List<XmltvRating> ratings, MxfProgram mxfProgram)
        {
            foreach (XmltvRating rating in ratings)
            {
                if (string.IsNullOrEmpty(rating.System)) continue;
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
                        default:
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
                        default:
                            break;
                    }
                }
            }
        }

        private static string GetStarRating(List<XmltvRating> ratings)
        {
            foreach (XmltvRating rating in ratings)
            {
                if (rating.Value != null)
                {
                    string[] nums = rating.Value.Split('/');
                    if (nums.Length == 2)
                    {
                        double denominator = double.Parse(nums[1]);
                        if (denominator == 0) return null;

                        int halfStars = (int)((double.Parse(nums[0]) / denominator) * 8 + 0.125);
                        if (halfStars > 0) return halfStars.ToString();
                    }
                }
            }
            return null;
        }

        private static string GetUsTvRating(List<XmltvRating> ratings)
        {
            foreach (XmltvRating rating in ratings)
            {
                if (string.IsNullOrEmpty(rating.System) || rating.System.ToLower().Equals("vchip") || rating.System.ToLower().Equals("usa parental rating"))
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
                        default:
                            break;
                    }
                }
            }
            return null;
        }
        #endregion
    }
}
