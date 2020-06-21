using System;
using System.Collections.Generic;
using System.Linq;
using epg123.MxfXml;
using epg123.XmltvXml;

namespace epg123
{
    public static partial class sdJson2mxf
    {
        private static XMLTV xmltv;

        private static bool CreateXmltvFile()
        {
            try
            {
                xmltv = new XMLTV()
                {
                    Date = DateTime.UtcNow.ToString(),
                    SourceInfoUrl = "http://schedulesdirect.org",
                    SourceInfoName = "Schedules Direct",
                    GeneratorInfoName = "EPG123",
                    GeneratorInfoUrl = "http://epg123.garyan2.net",
                    Channels = new List<XmltvChannel>(),
                    Programs = new List<XmltvProgramme>()
                };

                foreach (MxfLineup lineup in sdMxf.With[0].Lineups)
                {
                    foreach (MxfChannel channel in lineup.channels)
                    {
                        xmltv.Channels.Add(buildXmltvChannel(channel));
                    }
                }

                foreach (MxfService service in sdMxf.With[0].Services)
                {
                    DateTime startTime = new DateTime();
                    if (service.mxfScheduleEntries.ScheduleEntry.Count == 0 && config.XmltvAddFillerData)
                    {
                        // add a program specific for this service
                        MxfProgram program = new MxfProgram()
                        {
                            Description = config.XmltvFillerProgramDescription,
                            IsGeneric = "true",
                            Title = service.Name,
                            tmsId = string.Format("EPG123FILL{0}", service.StationID),
                            index = sdMxf.With[0].Programs.Count + 1,
                            jsonProgramData = new sdProgram()
                        };

                        // populate the schedule entries
                        startTime = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day, 0, 0, 0);
                        DateTime stopTime = startTime + TimeSpan.FromDays(config.DaysToDownload);
                        do
                        {
                            service.mxfScheduleEntries.ScheduleEntry.Add(new MxfScheduleEntry()
                            {
                                Duration = config.XmltvFillerProgramLength * 60 * 60,
                                Program = sdMxf.With[0].getProgram(program.tmsId, program).Id,
                                StartTime = startTime.ToString("yyyy-MM-ddTHH:mm:ss"),
                                IsRepeat = "true"
                            });
                            startTime += TimeSpan.FromHours((double)config.XmltvFillerProgramLength);
                        } while (startTime < stopTime);
                    }

                    foreach (MxfScheduleEntry scheduleEntry in service.mxfScheduleEntries.ScheduleEntry)
                    {
                        if (!string.IsNullOrEmpty(scheduleEntry.StartTime))
                        {
                            startTime = DateTime.Parse(scheduleEntry.StartTime + "Z").ToUniversalTime();
                        }
                        xmltv.Programs.Add(buildXmltvProgram(scheduleEntry, startTime, service.xmltvChannelID, out startTime));
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                Logger.WriteInformation("Failed to create the XMLTV file. Message : " + ex.Message);
            }
            return false;
        }

        #region ========== XMLTV Channels and Functions ==========
        public static XmltvChannel buildXmltvChannel(MxfChannel mxfChannel)
        {
            // determine what service this channel belongs to
            MxfService mxfService = sdMxf.With[0].Services.Where(arg => arg.Id.Equals(mxfChannel.Service)).Single();

            // initialize the return channel
            XmltvChannel ret = new XmltvChannel()
            {
                Id = mxfService.xmltvChannelID,
                DisplayNames = new List<XmltvText>()
            };

            // minimum display names
            // 5MAXHD
            // 5 StarMAX HD East
            ret.DisplayNames.Add(new XmltvText() { Text = mxfService.CallSign });
            if (!mxfService.Name.Equals(mxfService.CallSign))
            {
                ret.DisplayNames.Add(new XmltvText() { Text = mxfService.Name });
            }
            
            // add channel number if requested
            if (config.XmltvIncludeChannelNumbers)
            {
                if (mxfChannel.Number > 0)
                {
                    string num = mxfChannel.Number.ToString();
                    num += (mxfChannel.SubNumber > 0) ? "." + mxfChannel.SubNumber.ToString() : string.Empty;

                    ret.DisplayNames.Add(new XmltvText() { Text = num + " " + mxfService.CallSign });
                    ret.DisplayNames.Add(new XmltvText() { Text = num });
                }
            }

            // add affiliate if present
            if (!string.IsNullOrEmpty(mxfService.Affiliate)) ret.DisplayNames.Add(new XmltvText() { Text = mxfService.Affiliate.Substring(11) });

            // add logo if available
            if (mxfService.logoImage != null)
            {
                ret.Icons = new List<XmltvIcon>()
                {
                    new XmltvIcon()
                    {
                        src = mxfService.logoImage.URL,
                        height = mxfService.logoImage.Height > 0 ? mxfService.logoImage.Height.ToString() : null,
                        width = mxfService.logoImage.Width > 0 ? mxfService.logoImage.Width.ToString() : null
                    }
                };
            }
            return ret;
        }
        #endregion

        #region ========== XMLTV Programmes and Functions ==========
        private static XmltvProgramme buildXmltvProgram(MxfScheduleEntry scheduleEntry, DateTime startTime, string channelId, out DateTime endTime)
        {
            MxfProgram mxfProgram = sdMxf.With[0].Programs[int.Parse(scheduleEntry.Program) - 1];
            if (!mxfProgram.Id.Equals(scheduleEntry.Program))
            {
                mxfProgram = sdMxf.With[0].Programs.Where(arg => arg.Id.Equals(scheduleEntry.Program)).SingleOrDefault();
            }
            endTime = startTime + TimeSpan.FromSeconds((scheduleEntry.Duration));

            return new XmltvProgramme()
            {
                // added +0000 for NPVR; otherwise it would assume local time instead of UTC
                Start = startTime.ToString("yyyyMMddHHmmss") + " +0000",
                Stop = endTime.ToString("yyyyMMddHHmmss") + " +0000",
                Channel = channelId,

                Titles = mxfStringToXmlTextArray(mxfProgram.Title),
                SubTitles = mxfStringToXmlTextArray(mxfProgram.EpisodeTitle),
                Descriptions = mxfStringToXmlTextArray(mxfProgram.Description),
                Credits = buildProgramCredits(mxfProgram),
                Date = buildProgramDate(mxfProgram),
                Categories = buildProgramCategories(mxfProgram, scheduleEntry),
                Language = mxfStringToXmlText(!string.IsNullOrEmpty(mxfProgram.Language) ? mxfProgram.Language.Substring(0, 2) : null),
                Icons = buildProgramIcons(mxfProgram),
                Sport = grabSportEvent(mxfProgram),
                Teams = buildSportTeams(mxfProgram),
                EpisodeNums = buildEpisodeNumbers(mxfProgram, scheduleEntry, startTime, channelId),
                Video = buildProgramVideo(scheduleEntry),
                Audio = buildProgramAudio(scheduleEntry),
                PreviouslyShown = buildProgramPreviouslyShown(mxfProgram, scheduleEntry),
                Premiere = buildProgramPremiere(mxfProgram, scheduleEntry),
                Live = buildLiveFlag(scheduleEntry),
                New = string.IsNullOrEmpty(scheduleEntry.IsRepeat) ? string.Empty : null,
                Subtitles = buildProgramSubtitles(scheduleEntry),
                Rating = buildProgramRatings(mxfProgram, scheduleEntry),
                StarRating = buildProgramStarRatings(mxfProgram)
            };
        }

        // Titles, SubTitles, and Descriptions
        private static List<XmltvText> mxfStringToXmlTextArray(string mxfString)
        {
            if (!string.IsNullOrEmpty(mxfString))
            {
                List<XmltvText> ret = new List<XmltvText>();
                ret.Add(new XmltvText() { Text = mxfString });
                return ret;
            }
            return null;
        }

        // Credits
        private static XmltvCredit buildProgramCredits(MxfProgram mxfProgram)
        {
            if ((mxfProgram.DirectorRole != null && mxfProgram.DirectorRole.Count > 0) || (mxfProgram.ActorRole != null && mxfProgram.ActorRole.Count > 0) ||
                (mxfProgram.WriterRole != null && mxfProgram.WriterRole.Count > 0) || (mxfProgram.ProducerRole != null && mxfProgram.ProducerRole.Count > 0) ||
                (mxfProgram.HostRole != null && mxfProgram.HostRole.Count > 0) || (mxfProgram.GuestActorRole != null && mxfProgram.GuestActorRole.Count > 0))
            {
                return new XmltvCredit()
                {
                    Directors = mxfPersonRankToXmltvCrew(mxfProgram.DirectorRole),
                    Actors = mxfPersonRankToXmltvActors(mxfProgram.ActorRole),
                    Writers = mxfPersonRankToXmltvCrew(mxfProgram.WriterRole),
                    Producers = mxfPersonRankToXmltvCrew(mxfProgram.ProducerRole),
                    Presenters = mxfPersonRankToXmltvCrew(mxfProgram.HostRole),
                    Guests = mxfPersonRankToXmltvCrew(mxfProgram.GuestActorRole)
                };
            }
            return null;
        }
        private static List<string> mxfPersonRankToXmltvCrew(List<MxfPersonRank> mxfPersons)
        {
            if (mxfPersons != null)
            {
                List<string> ret = new List<string>();
                foreach (MxfPersonRank person in mxfPersons)
                {
                    ret.Add(sdMxf.With[0].People[int.Parse(person.Person.Substring(1)) - 1].Name);
                }
                return ret;
            }
            return null;
        }
        private static List<XmltvActor> mxfPersonRankToXmltvActors(List<MxfPersonRank> mxfPersons)
        {
            if (mxfPersons != null)
            {
                List<XmltvActor> ret = new List<XmltvActor>();
                foreach (MxfPersonRank person in mxfPersons)
                {
                    ret.Add(new XmltvActor()
                    {
                        Actor = sdMxf.With[0].People[int.Parse(person.Person.Substring(1)) - 1].Name,
                        Role = person.Character
                    });
                }
                return ret;
            }
            return null;
        }

        // Date
        private static string buildProgramDate(MxfProgram mxfProgram)
        {
            if (!string.IsNullOrEmpty(mxfProgram.IsMovie))
            {
                return mxfProgram.Year;
            }
            else if (!string.IsNullOrEmpty(mxfProgram.OriginalAirdate))
            {
                return mxfProgram.OriginalAirdate.Replace("-", "");
            }
            return null;
        }

        // Categories
        private static List<XmltvText> buildProgramCategories(MxfProgram mxfProgram, MxfScheduleEntry mxfScheduleEntry)
        {
            if (!string.IsNullOrEmpty(mxfProgram.Keywords))
            {
                HashSet<string> categories = new HashSet<string>();
                foreach (string keywordId in mxfProgram.Keywords.Split(','))
                {
                    foreach (MxfKeyword keyword in sdMxf.With[0].Keywords)
                    {
                        if (keyword.Word.ToLower().Contains("premiere")) continue;
                        if (keyword.Id == keywordId)
                        {
                            categories.Add(keyword.Word.Equals("Movies") ? "Movie" : keyword.Word);
                            break;
                        }
                    }
                }

                if (!string.IsNullOrEmpty(mxfScheduleEntry.IsLive))
                {
                    //categories.Add("Live");
                }

                if (categories.Contains("Kids") && categories.Contains("Children"))
                {
                    categories.Remove("Kids");
                }

                if (categories.Count > 0)
                {
                    List<XmltvText> ret = new List<XmltvText>();
                    foreach (string category in categories)
                    {
                        ret.Add(new XmltvText() { Text = category });
                    }
                    return ret;
                }
            }
            return null;
        }

        // Language
        private static XmltvText mxfStringToXmlText(string mxfString)
        {
            if (!string.IsNullOrEmpty(mxfString))
            {
                return new XmltvText() { Text = mxfString };
            }
            return null;
        }

        // Icons
        private static List<XmltvIcon> buildProgramIcons(MxfProgram mxfProgram)
        {
            // a movie will have a guide image from the program
            if (mxfProgram.programImages != null)
            {
                List<XmltvIcon> ret = new List<XmltvIcon>();
                foreach (sdImage image in mxfProgram.programImages)
                {
                    ret.Add(new XmltvIcon()
                    {
                        src = image.Uri,
                        height = image.Height,
                        width = image.Width
                    });
                }
                return ret;
            }

            // get the series info class from the program if it is a series
            if (!string.IsNullOrEmpty(mxfProgram.Series))
            {
                MxfSeriesInfo mxfSeriesInfo = sdMxf.With[0].SeriesInfos[int.Parse(mxfProgram.Series.Substring(2)) - 1];
                if (!mxfSeriesInfo.Id.Equals(mxfProgram.Series))
                {
                    mxfSeriesInfo = sdMxf.With[0].SeriesInfos.Where(arg => arg.Id.Equals(mxfProgram.Series)).SingleOrDefault();
                }
                if (mxfSeriesInfo != null && mxfSeriesInfo.seriesImages != null)
                {
                    List<XmltvIcon> ret = new List<XmltvIcon>();
                    foreach (sdImage image in mxfSeriesInfo.seriesImages)
                    {
                        ret.Add(new XmltvIcon()
                        {
                            src = image.Uri,
                            height = image.Height,
                            width = image.Width
                        });
                    }
                    return ret;
                }
            }
            return null;
        }

        private static XmltvText grabSportEvent(MxfProgram program)
        {
            if (!string.IsNullOrEmpty(program.IsSports) && program.jsonProgramData.Genres != null)
            {
                foreach (string category in program.jsonProgramData.Genres)
                {
                    if (!category.ToLower().StartsWith("sport"))
                    {
                        return new XmltvText() { Text = category };
                    }
                }
            }
            return null;
        }

        private static List<XmltvText> buildSportTeams(MxfProgram program)
        {
            if (!string.IsNullOrEmpty(program.IsSports) && program.jsonProgramData.EventDetails?.Teams != null)
            {
                List<XmltvText> ret = new List<XmltvText>();
                foreach (sdProgramEventDetailsTeam team in program.jsonProgramData.EventDetails.Teams)
                {
                    ret.Add(new XmltvText() { Text = team.Name });
                }
                return ret;
            }
            return null;
        }

        // EpisodeNums
        private static Random randomNumber = new Random();
        private static List<XmltvEpisodeNum> buildEpisodeNumbers(MxfProgram mxfProgram, MxfScheduleEntry mxfScheduleEntry, DateTime startTime, string channelId)
        {
            List<XmltvEpisodeNum> list = new List<XmltvEpisodeNum>();
            if (!mxfProgram.tmsId.StartsWith("EPG123"))
            {
                list.Add(new XmltvEpisodeNum()
                {
                    System = "dd_progid",
                    Text = mxfProgram.Uid.Substring(9).Replace("_", ".")
                });
            }

            if (!string.IsNullOrEmpty(mxfProgram.EpisodeNumber) || !string.IsNullOrEmpty(mxfScheduleEntry.Part))
            {
                string text = string.Format("{0}.{1}.{2}{3}",
                    (mxfProgram.SeasonNumber != null) ? (int.Parse(mxfProgram.SeasonNumber) - 1).ToString() : string.Empty,
                    (mxfProgram.EpisodeNumber != null) ? (int.Parse(mxfProgram.EpisodeNumber) - 1).ToString() : string.Empty,
                    (mxfScheduleEntry.Part != null) ? (int.Parse(mxfScheduleEntry.Part) - 1).ToString() + "/" : "0/",
                    (mxfScheduleEntry.Parts != null) ? (int.Parse(mxfScheduleEntry.Parts)).ToString() : "1");
                list.Add(new XmltvEpisodeNum() { System = "xmltv_ns", Text = text });
            }
            else if (mxfProgram.tmsId.StartsWith("EPG123"))
            {
                // filler data - create oad of scheduled start time
                list.Add(new XmltvEpisodeNum() { System = "original-air-date", Text = startTime.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss") });
            }
            else if (!mxfProgram.tmsId.StartsWith("MV"))
            {
                // add this entry due to Plex identifying anything without an episode number as being a movie
                string oad = mxfProgram.OriginalAirdate;
                if (string.IsNullOrEmpty(mxfScheduleEntry.IsRepeat))
                {
                    oad = startTime.ToLocalTime().ToString("yyyy-MM-dd HH:mm:") + randomNumber.Next(1, 60).ToString("00");
                }
                else if (string.IsNullOrEmpty(oad))
                {
                    oad = "1900-01-01";
                }
                list.Add(new XmltvEpisodeNum() { System = "original-air-date", Text = oad });
            }

            if (mxfProgram.jsonProgramData.Metadata != null)
            {
                foreach (Dictionary<string, sdProgramMetadataProvider> providers in mxfProgram.jsonProgramData.Metadata)
                {
                    foreach (KeyValuePair<string, sdProgramMetadataProvider> provider in providers)
                    {
                        if (provider.Key.ToLower().Equals("thetvdb"))
                        {
                            if (provider.Value.SeriesID > 0)
                            {
                                list.Add(new XmltvEpisodeNum() { System = "thetvdb.com", Text = "series/" + provider.Value.SeriesID.ToString() });
                            }
                            if (provider.Value.EpisodeID > 0)
                            {
                                list.Add(new XmltvEpisodeNum() { System = "thetvdb.com", Text = "episode/" + provider.Value.EpisodeID.ToString() });
                            }
                        }
                    }
                }
            }
            return list;
        }

        // Video
        private static XmltvVideo buildProgramVideo(MxfScheduleEntry mxfScheduleEntry)
        {
            if (!string.IsNullOrEmpty(mxfScheduleEntry.IsHdtv))
            {
                return new XmltvVideo() { Quality = "HDTV" };
            }
            return null;
        }

        // Audio
        private static XmltvAudio buildProgramAudio(MxfScheduleEntry mxfScheduleEntry)
        {
            if (!string.IsNullOrEmpty(mxfScheduleEntry.AudioFormat))
            {
                string format = string.Empty;
                switch (mxfScheduleEntry.AudioFormat)
                {
                    case "1": format = "mono"; break;
                    case "2": format = "stereo"; break;
                    case "3": format = "dolby"; break;
                    case "4": format = "dolby digital"; break;
                    case "5": format = "surround"; break;
                    default: break;
                }
                if (!string.IsNullOrEmpty(format))
                {
                    return new XmltvAudio() { Stereo = format };
                }
            }
            return null;
        }

        // Previously Shown
        private static XmltvPreviouslyShown buildProgramPreviouslyShown(MxfProgram mxfProgram, MxfScheduleEntry mxfScheduleEntry)
        {
            if (!string.IsNullOrEmpty(mxfScheduleEntry.IsRepeat) && string.IsNullOrEmpty(mxfProgram.IsMovie))
            {
                //if (!string.IsNullOrEmpty(mxfProgram.OriginalAirdate))
                //{
                //    return new XmltvPreviouslyShown() { Start = mxfProgram.OriginalAirdate.Replace("-", "") };
                //}
                return new XmltvPreviouslyShown() { Text = string.Empty };
            }
            return null;
        }

        // Premiere
        private static XmltvText buildProgramPremiere(MxfProgram mxfProgram, MxfScheduleEntry mxfScheduleEntry)
        {
            if (!string.IsNullOrEmpty(mxfScheduleEntry.IsPremiere))
            {
                string text = string.Empty;
                if (!string.IsNullOrEmpty(mxfProgram.IsMovie)) text = "Movie Premiere";
                else if (!string.IsNullOrEmpty(mxfProgram.IsSeriesPremiere)) text = "Series Premiere";
                else if (!string.IsNullOrEmpty(mxfProgram.IsSeasonPremiere)) text = "Season Premiere";
                else text = "Miniseries Premiere";

                return new XmltvText() { Text = text };
            }
            return null;
        }

        // New
        private static string buildNewFlag(MxfProgram mxfProgram, MxfScheduleEntry mxfScheduleEntry)
        {
            if (!string.IsNullOrEmpty(mxfProgram.IsMovie) || !string.IsNullOrEmpty(mxfScheduleEntry.IsRepeat)) return null;
            return string.Empty;
        }

        private static string buildLiveFlag(MxfScheduleEntry mxfScheduleEntry)
        {
            if (string.IsNullOrEmpty(mxfScheduleEntry.IsLive)) return null;
            return string.Empty;
        }

        // Subtitles
        private static List<XmltvSubtitles> buildProgramSubtitles(MxfScheduleEntry mxfScheduleEntry)
        {
            if (!string.IsNullOrEmpty(mxfScheduleEntry.IsCC) || !string.IsNullOrEmpty(mxfScheduleEntry.IsSubtitled) || !string.IsNullOrEmpty(mxfScheduleEntry.IsSigned))
            {
                List<XmltvSubtitles> list = new List<XmltvSubtitles>();
                if (!string.IsNullOrEmpty(mxfScheduleEntry.IsCC))
                {
                    list.Add(new XmltvSubtitles() { Type = "teletext" });
                }
                if (!string.IsNullOrEmpty(mxfScheduleEntry.IsSubtitled))
                {
                    list.Add(new XmltvSubtitles() { Type = "onscreen" });
                }
                if (!string.IsNullOrEmpty(mxfScheduleEntry.IsSigned))
                {
                    list.Add(new XmltvSubtitles() { Type = "deaf-signed" });
                }
                return list;
            }
            return null;
        }

        // Rating
        private static List<XmltvRating> buildProgramRatings(MxfProgram mxfProgram, MxfScheduleEntry mxfScheduleEntry)
        {
            if (!string.IsNullOrEmpty(mxfProgram.MpaaRating) || !string.IsNullOrEmpty(mxfScheduleEntry.TvRating) || (mxfProgram.contentRatings != null))
            {
                List<XmltvRating> ret = new List<XmltvRating>();
                addProgramRatingAdvisory(mxfProgram.HasAdult, ret, "Adult Situations");
                addProgramRatingAdvisory(mxfProgram.HasBriefNudity, ret, "Brief Nudity");
                addProgramRatingAdvisory(mxfProgram.HasGraphicLanguage, ret, "Graphic Language");
                addProgramRatingAdvisory(mxfProgram.HasGraphicViolence, ret, "Graphic Violence");
                addProgramRatingAdvisory(mxfProgram.HasLanguage, ret, "Language");
                addProgramRatingAdvisory(mxfProgram.HasMildViolence, ret, "Mild Violence");
                addProgramRatingAdvisory(mxfProgram.HasNudity, ret, "Nudity");
                addProgramRatingAdvisory(mxfProgram.HasRape, ret, "Rape");
                addProgramRatingAdvisory(mxfProgram.HasStrongSexualContent, ret, "Strong Sexual Content");
                addProgramRatingAdvisory(mxfProgram.HasViolence, ret, "Violence");
                addProgramRating(mxfScheduleEntry, ret);
                return ret;
            }
            return null;
        }
        private static void addProgramRating(MxfScheduleEntry mxfScheduleEntry, List<XmltvRating> list)
        {
            if (mxfScheduleEntry.Ratings != null)
            {
                foreach (KeyValuePair<string, string> rating in mxfScheduleEntry.Ratings)
                {
                    list.Add(new XmltvRating() { System = rating.Key, Value = rating.Value });
                }
            }

            if (!string.IsNullOrEmpty(mxfScheduleEntry.TvRating))
            {
                string rating = string.Empty;
                switch (mxfScheduleEntry.TvRating)
                {
                    // v-chip is only for US, Canada, and Brazil
                    case "1": rating = "TV-Y"; break;
                    case "2": rating = "TV-Y7"; break;
                    case "3": rating = "TV-G"; break;
                    case "4": rating = "TV-PG"; break;
                    case "5": rating = "TV-14"; break;
                    case "6": rating = "TV-MA"; break;
                    default: break;
                }
                if (!string.IsNullOrEmpty(rating))
                {
                    list.Add(new XmltvRating() { System = "VCHIP", Value = rating });
                }
            }
        }
        private static void addProgramRatingAdvisory(string mxfProgramAdvise, List<XmltvRating> list, string advisory)
        {
            if (!string.IsNullOrEmpty(mxfProgramAdvise))
            {
                list.Add(new XmltvRating() { System = "advisory", Value = advisory });
            }
        }

        // StarRating
        private static List<XmltvRating> buildProgramStarRatings(MxfProgram mxfProgram)
        {
            if (!string.IsNullOrEmpty(mxfProgram.HalfStars))
            {
                List<XmltvRating> list = new List<XmltvRating>
                {
                    new XmltvRating()
                    {
                        Value = string.Format("{0:N1}/4", int.Parse(mxfProgram.HalfStars) * 0.5)
                    }
                };
                return list;
            }
            return null;
        }
        #endregion

    }
}