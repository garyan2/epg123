using System;
using System.Collections.Generic;
using System.Globalization;
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
                            IsGeneric = true,
                            Title = service.Name,
                            tmsId = string.Format("EPG123FILL{0}", service.StationID),
                            index = sdMxf.With[0].Programs.Count + 1,
                            //jsonProgramData = new sdProgram()
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
                                StartTime = startTime,
                                IsRepeat = true
                            });
                            startTime += TimeSpan.FromHours((double)config.XmltvFillerProgramLength);
                        } while (startTime < stopTime);
                    }

                    foreach (MxfScheduleEntry scheduleEntry in service.mxfScheduleEntries.ScheduleEntry)
                    {
                        if (scheduleEntry.StartTime != DateTime.MinValue)
                        {
                            startTime = scheduleEntry.StartTime;
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
                        height = mxfService.logoImage.Height,
                        width = mxfService.logoImage.Width
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

            string descriptionExtended = string.Empty;
            if (config.XmltvExtendedInfoInTitleDescriptions && !mxfProgram.IsPaidProgramming)
            {
                if (mxfProgram.IsMovie && mxfProgram.Year > 0) descriptionExtended = $"{mxfProgram.Year}";
                else if (!mxfProgram.IsMovie)
                {
                    if (scheduleEntry.IsLive) descriptionExtended = "[LIVE]";
                    else if (scheduleEntry.IsPremiere) descriptionExtended = "[PREMIERE]";
                    else if (scheduleEntry.IsFinale) descriptionExtended = "[FINALE]";
                    else if (!scheduleEntry.IsRepeat) descriptionExtended = "[NEW]";
                    else if (scheduleEntry.IsRepeat && !mxfProgram.IsGeneric) descriptionExtended = "[REPEAT]";

                    if (!config.PrefixEpisodeTitle && !config.PrefixEpisodeDescription && !config.AppendEpisodeDesc)
                    {
                        if (mxfProgram.SeasonNumber > 0 && mxfProgram.EpisodeNumber > 0) descriptionExtended += $" S{mxfProgram.SeasonNumber}:E{mxfProgram.EpisodeNumber}";
                        else if (mxfProgram.EpisodeNumber > 0) descriptionExtended += $" #{mxfProgram.EpisodeNumber}";
                    }
                }

                //if (scheduleEntry.IsHdtv) descriptionExtended += " HD";
                //if (!string.IsNullOrEmpty(mxfProgram.Language)) descriptionExtended += $" {new CultureInfo(mxfProgram.Language).DisplayName}";
                //if (scheduleEntry.IsCC) descriptionExtended += " CC";
                //if (scheduleEntry.IsSigned) descriptionExtended += " Signed";
                //if (scheduleEntry.IsSap) descriptionExtended += " SAP";
                //if (scheduleEntry.IsSubtitled) descriptionExtended += " SUB";

                string[] tvRatings = { "", "TV-Y", "TV-Y7", "TV-G", "TV-PG", "TV-14", "TV-MA",
                                       "", "Kinder bis 12 Jahren", "Freigabe ab 12 Jahren", "Freigabe ab 16 Jahren", "Keine Jugendfreigabe",
                                       "", "Déconseillé aux moins de 10 ans", "Déconseillé aux moins de 12 ans", "Déconseillé aux moins de 16 ans", "Déconseillé aux moins de 18 ans",
                                       "모든 연령 시청가", "7세 이상 시청가", "12세 이상 시청가", "15세 이상 시청가", "19세 이상 시청가",
                                       "SKY-UC", "SKY-U", "SKY-PG", "SKY-12", "SKY-15", "SKY-18", "SKY-R18" };
                string[] mpaaRatings = { "", "G", "PG", "PG-13", "R", "NC-17", "X", "NR", "AO" };

                if (!string.IsNullOrEmpty(tvRatings[scheduleEntry.TvRating]))
                {
                    descriptionExtended += $" {tvRatings[scheduleEntry.TvRating]}";
                    if (mxfProgram.MpaaRating > 0) descriptionExtended += ",";
                }
                if (mxfProgram.MpaaRating > 0) descriptionExtended += $" {mpaaRatings[mxfProgram.MpaaRating]}";
                
                if (mxfProgram.contentAdvisories != null)
                {
                    string advisories = string.Empty;
                    if (mxfProgram.HasAdult) advisories += "Adult Situations,";
                    if (mxfProgram.HasGraphicLanguage) advisories += "Graphic Language,";
                    else if (mxfProgram.HasLanguage) advisories += "Language,";
                    if (mxfProgram.HasStrongSexualContent) advisories += "Strong Sexual Content,";
                    if (mxfProgram.HasGraphicViolence) advisories += "Graphic Violence,";
                    else if (mxfProgram.HasMildViolence) advisories += "Mild Violence,";
                    else if (mxfProgram.HasViolence) advisories += "Violence,";
                    if (mxfProgram.HasNudity) advisories += "Nudity,";
                    else if (mxfProgram.HasBriefNudity) advisories += "Brief Nudity,";
                    if (mxfProgram.HasRape) advisories += "Rape,";

                    descriptionExtended += $" ({advisories.Trim().TrimEnd(',').Replace(",", ", ")})";
                }

                if (mxfProgram.IsMovie && mxfProgram.HalfStars > 0)
                {
                    descriptionExtended += $" {mxfProgram.HalfStars * 0.5:N1}/4.0";
                }
                else if (!mxfProgram.IsMovie)
                {
                    if (!mxfProgram.IsGeneric && !string.IsNullOrEmpty(mxfProgram.OriginalAirdate)) descriptionExtended += $" Original air date: {DateTime.Parse(mxfProgram.OriginalAirdate):d}";
                }

                if (!string.IsNullOrEmpty(descriptionExtended)) descriptionExtended = descriptionExtended.Trim() + "\u000D\u000A";
            }

            return new XmltvProgramme()
            {
                // added +0000 for NPVR; otherwise it would assume local time instead of UTC
                Start = startTime.ToString("yyyyMMddHHmmss") + " +0000",
                Stop = endTime.ToString("yyyyMMddHHmmss") + " +0000",
                Channel = channelId,

                Titles = mxfStringToXmlTextArray(mxfProgram.Title),
                SubTitles = mxfStringToXmlTextArray(mxfProgram.EpisodeTitle),
                Descriptions = mxfStringToXmlTextArray((descriptionExtended + mxfProgram.Description).Trim()),
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
                New = (!scheduleEntry.IsRepeat) ? string.Empty : null,
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
            if (mxfProgram.IsMovie && mxfProgram.Year > 0)
            {
                return mxfProgram.Year.ToString();
            }
            else if (!string.IsNullOrEmpty(mxfProgram.OriginalAirdate))
            {
                return DateTime.Parse(mxfProgram.OriginalAirdate).ToString("yyyyMMdd");
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

                if (mxfScheduleEntry.IsLive)
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
            if (program.IsSports && program.genres != null)
            {
                foreach (string category in program.genres)
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
            if (program.IsSports && program.teams != null)
            {
                List<XmltvText> ret = new List<XmltvText>();
                foreach (string team in program.teams)
                {
                    ret.Add(new XmltvText() { Text = team });
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

            if (mxfProgram.EpisodeNumber != 0 || mxfScheduleEntry.Part != 0)
            {
                string text = string.Format("{0}.{1}.{2}{3}",
                    (mxfProgram.SeasonNumber != 0) ? (mxfProgram.SeasonNumber - 1).ToString() : string.Empty,
                    (mxfProgram.EpisodeNumber != 0) ? (mxfProgram.EpisodeNumber - 1).ToString() : string.Empty,
                    (mxfScheduleEntry.Part != 0) ? (mxfScheduleEntry.Part - 1).ToString() + "/" : "0/",
                    (mxfScheduleEntry.Parts != 0) ? (mxfScheduleEntry.Parts).ToString() : "1");
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
                if (!mxfScheduleEntry.IsRepeat)
                {
                    oad = startTime.ToLocalTime().ToString("yyyy-MM-dd HH:mm:") + randomNumber.Next(1, 60).ToString("00");
                }
                else if (!string.IsNullOrEmpty(oad))
                {
                    oad = DateTime.Parse(oad).ToString("yyyy-MM-dd");
                }
                else
                {
                    oad = "1900-01-01";
                }
                list.Add(new XmltvEpisodeNum() { System = "original-air-date", Text = oad });
            }

            //if (mxfProgram.jsonProgramData.Metadata != null)
            //{
            //    foreach (Dictionary<string, sdProgramMetadataProvider> providers in mxfProgram.jsonProgramData.Metadata)
            //    {
            //        foreach (KeyValuePair<string, sdProgramMetadataProvider> provider in providers)
            //        {
            //            if (provider.Key.ToLower().Equals("thetvdb"))
            //            {
            //                if (provider.Value.SeriesID > 0)
            //                {
            //                    list.Add(new XmltvEpisodeNum() { System = "thetvdb.com", Text = "series/" + provider.Value.SeriesID.ToString() });
            //                }
            //                if (provider.Value.EpisodeID > 0)
            //                {
            //                    list.Add(new XmltvEpisodeNum() { System = "thetvdb.com", Text = "episode/" + provider.Value.EpisodeID.ToString() });
            //                }
            //            }
            //        }
            //    }
            //}
            return list;
        }

        // Video
        private static XmltvVideo buildProgramVideo(MxfScheduleEntry mxfScheduleEntry)
        {
            if (mxfScheduleEntry.IsHdtv)
            {
                return new XmltvVideo() { Quality = "HDTV" };
            }
            return null;
        }

        // Audio
        private static XmltvAudio buildProgramAudio(MxfScheduleEntry mxfScheduleEntry)
        {
            if (mxfScheduleEntry.AudioFormat > 0)
            {
                string format = string.Empty;
                switch (mxfScheduleEntry.AudioFormat)
                {
                    case 1: format = "mono"; break;
                    case 2: format = "stereo"; break;
                    case 3: format = "dolby"; break;
                    case 4: format = "dolby digital"; break;
                    case 5: format = "surround"; break;
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
            if (mxfScheduleEntry.IsRepeat && !mxfProgram.IsMovie)
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
            if (mxfScheduleEntry.IsPremiere)
            {
                string text = string.Empty;
                if (mxfProgram.IsMovie) text = "Movie Premiere";
                else if (mxfProgram.IsSeriesPremiere) text = "Series Premiere";
                else if (mxfProgram.IsSeasonPremiere) text = "Season Premiere";
                else text = "Miniseries Premiere";

                return new XmltvText() { Text = text };
            }
            return null;
        }

        // New
        private static string buildNewFlag(MxfProgram mxfProgram, MxfScheduleEntry mxfScheduleEntry)
        {
            if (mxfProgram.IsMovie || mxfScheduleEntry.IsRepeat) return null;
            return string.Empty;
        }

        private static string buildLiveFlag(MxfScheduleEntry mxfScheduleEntry)
        {
            if (!mxfScheduleEntry.IsLive) return null;
            return string.Empty;
        }

        // Subtitles
        private static List<XmltvSubtitles> buildProgramSubtitles(MxfScheduleEntry mxfScheduleEntry)
        {
            if (mxfScheduleEntry.IsCC || mxfScheduleEntry.IsSubtitled || mxfScheduleEntry.IsSigned)
            {
                List<XmltvSubtitles> list = new List<XmltvSubtitles>();
                if (mxfScheduleEntry.IsCC)
                {
                    list.Add(new XmltvSubtitles() { Type = "teletext" });
                }
                if (mxfScheduleEntry.IsSubtitled)
                {
                    list.Add(new XmltvSubtitles() { Type = "onscreen" });
                }
                if (mxfScheduleEntry.IsSigned)
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
            if (mxfProgram.MpaaRating != 0 || mxfScheduleEntry.TvRating != 0 || (mxfProgram.contentRatings != null))
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

            if (mxfScheduleEntry.TvRating != 0)
            {
                string rating = string.Empty;
                switch (mxfScheduleEntry.TvRating)
                {
                    // v-chip is only for US, Canada, and Brazil
                    case 1: rating = "TV-Y"; break;
                    case 2: rating = "TV-Y7"; break;
                    case 3: rating = "TV-G"; break;
                    case 4: rating = "TV-PG"; break;
                    case 5: rating = "TV-14"; break;
                    case 6: rating = "TV-MA"; break;
                    default: break;
                }
                if (!string.IsNullOrEmpty(rating))
                {
                    list.Add(new XmltvRating() { System = "VCHIP", Value = rating });
                }
            }
        }
        private static void addProgramRatingAdvisory(bool mxfProgramAdvise, List<XmltvRating> list, string advisory)
        {
            if (mxfProgramAdvise)
            {
                list.Add(new XmltvRating() { System = "advisory", Value = advisory });
            }
        }

        // StarRating
        private static List<XmltvRating> buildProgramStarRatings(MxfProgram mxfProgram)
        {
            if (mxfProgram.HalfStars != 0)
            {
                List<XmltvRating> list = new List<XmltvRating>
                {
                    new XmltvRating()
                    {
                        Value = string.Format("{0:N1}/4", mxfProgram.HalfStars * 0.5)
                    }
                };
                return list;
            }
            return null;
        }
        #endregion

    }
}