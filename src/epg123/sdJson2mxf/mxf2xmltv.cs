using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using epg123.MxfXml;
using epg123.XmltvXml;

namespace epg123.sdJson2mxf
{
    internal static partial class sdJson2Mxf
    {
        private static xmltv xmltv;

        private static bool CreateXmltvFile()
        {
            try
            {
                xmltv = new xmltv
                {
                    Date = DateTime.UtcNow.ToString(CultureInfo.InvariantCulture),
                    SourceInfoUrl = "http://schedulesdirect.org",
                    SourceInfoName = "Schedules Direct",
                    GeneratorInfoName = "EPG123",
                    GeneratorInfoUrl = "http://epg123.garyan2.net",
                    Channels = new List<XmltvChannel>(),
                    Programs = new List<XmltvProgramme>()
                };

                foreach (var channel in SdMxf.With[0].Lineups.SelectMany(lineup => lineup.channels))
                {
                    xmltv.Channels.Add(BuildXmltvChannel(channel));
                }

                foreach (var service in SdMxf.With[0].Services)
                {
                    var startTime = new DateTime();
                    if (service.MxfScheduleEntries.ScheduleEntry.Count == 0 && config.XmltvAddFillerData)
                    {
                        // add a program specific for this service
                        var program = new MxfProgram()
                        {
                            Description = config.XmltvFillerProgramDescription,
                            IsGeneric = true,
                            Title = service.Name,
                            TmsId = $"EPG123FILL{service.StationId}",
                            Index = SdMxf.With[0].Programs.Count + 1,
                            //jsonProgramData = new sdProgram()
                        };

                        // populate the schedule entries
                        startTime = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day, 0, 0, 0);
                        var stopTime = startTime + TimeSpan.FromDays(config.DaysToDownload);
                        do
                        {
                            service.MxfScheduleEntries.ScheduleEntry.Add(new MxfScheduleEntry()
                            {
                                Duration = config.XmltvFillerProgramLength * 60 * 60,
                                Program = SdMxf.With[0].GetProgram(program.TmsId, program).Id,
                                StartTime = startTime,
                                IsRepeat = true
                            });
                            startTime += TimeSpan.FromHours(config.XmltvFillerProgramLength);
                        } while (startTime < stopTime);
                    }

                    foreach (var scheduleEntry in service.MxfScheduleEntries.ScheduleEntry)
                    {
                        if (scheduleEntry.StartTime != DateTime.MinValue)
                        {
                            startTime = scheduleEntry.StartTime;
                        }
                        xmltv.Programs.Add(BuildXmltvProgram(scheduleEntry, startTime, service.XmltvChannelId, out startTime));
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
        public static XmltvChannel BuildXmltvChannel(MxfChannel mxfChannel)
        {
            // determine what service this channel belongs to
            var mxfService = SdMxf.With[0].Services.Single(arg => arg.Id.Equals(mxfChannel.Service));

            // initialize the return channel
            var ret = new XmltvChannel()
            {
                Id = mxfService.XmltvChannelId,
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
                    var num = mxfChannel.Number.ToString();
                    num += (mxfChannel.SubNumber > 0) ? "." + mxfChannel.SubNumber.ToString() : string.Empty;

                    ret.DisplayNames.Add(new XmltvText() { Text = num + " " + mxfService.CallSign });
                    ret.DisplayNames.Add(new XmltvText() { Text = num });
                }
            }

            // add affiliate if present
            if (!string.IsNullOrEmpty(mxfService.Affiliate)) ret.DisplayNames.Add(new XmltvText() { Text = mxfService.Affiliate.Substring(11) });

            // add logo if available
            if (mxfService.ServiceLogo != null)
            {
                ret.Icons = new List<XmltvIcon>()
                {
                    new XmltvIcon()
                    {
                        Src = mxfService.ServiceLogo.Url,
                        Height = mxfService.ServiceLogo.Height,
                        Width = mxfService.ServiceLogo.Width
                    }
                };
            }
            return ret;
        }
        #endregion

        #region ========== XMLTV Programmes and Functions ==========
        private static XmltvProgramme BuildXmltvProgram(MxfScheduleEntry scheduleEntry, DateTime startTime, string channelId, out DateTime endTime)
        {
            var mxfProgram = SdMxf.With[0].Programs[int.Parse(scheduleEntry.Program) - 1];
            if (!mxfProgram.Id.Equals(scheduleEntry.Program))
            {
                mxfProgram = SdMxf.With[0].Programs.SingleOrDefault(arg => arg.Id.Equals(scheduleEntry.Program));
            }
            endTime = startTime + TimeSpan.FromSeconds((scheduleEntry.Duration));
            if (mxfProgram == null) return null;

            var descriptionExtended = string.Empty;
            if (!config.XmltvExtendedInfoInTitleDescriptions || mxfProgram.IsPaidProgramming)
                return new XmltvProgramme()
                {
                    // added +0000 for NPVR; otherwise it would assume local time instead of UTC
                    Start = startTime.ToString("yyyyMMddHHmmss") + " +0000",
                    Stop = endTime.ToString("yyyyMMddHHmmss") + " +0000",
                    Channel = channelId,

                    Titles = MxfStringToXmlTextArray(mxfProgram.Title),
                    SubTitles = MxfStringToXmlTextArray(mxfProgram.EpisodeTitle),
                    Descriptions = MxfStringToXmlTextArray((descriptionExtended + mxfProgram.Description).Trim()),
                    Credits = BuildProgramCredits(mxfProgram),
                    Date = BuildProgramDate(mxfProgram),
                    Categories = BuildProgramCategories(mxfProgram, scheduleEntry),
                    Language = MxfStringToXmlText(!string.IsNullOrEmpty(mxfProgram.Language) ? mxfProgram.Language.Substring(0, 2) : null),
                    Icons = BuildProgramIcons(mxfProgram),
                    Sport = GrabSportEvent(mxfProgram),
                    Teams = BuildSportTeams(mxfProgram),
                    EpisodeNums = BuildEpisodeNumbers(mxfProgram, scheduleEntry, startTime),
                    Video = BuildProgramVideo(scheduleEntry),
                    Audio = BuildProgramAudio(scheduleEntry),
                    PreviouslyShown = BuildProgramPreviouslyShown(mxfProgram, scheduleEntry),
                    Premiere = BuildProgramPremiere(mxfProgram, scheduleEntry),
                    Live = BuildLiveFlag(scheduleEntry),
                    New = (!scheduleEntry.IsRepeat) ? string.Empty : null,
                    Subtitles = BuildProgramSubtitles(scheduleEntry),
                    Rating = BuildProgramRatings(mxfProgram, scheduleEntry),
                    StarRating = BuildProgramStarRatings(mxfProgram)
                };

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
                
            if (mxfProgram.ContentAdvisories != null)
            {
                var advisories = string.Empty;
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

            return new XmltvProgramme()
            {
                // added +0000 for NPVR; otherwise it would assume local time instead of UTC
                Start = startTime.ToString("yyyyMMddHHmmss") + " +0000",
                Stop = endTime.ToString("yyyyMMddHHmmss") + " +0000",
                Channel = channelId,

                Titles = MxfStringToXmlTextArray(mxfProgram.Title),
                SubTitles = MxfStringToXmlTextArray(mxfProgram.EpisodeTitle),
                Descriptions = MxfStringToXmlTextArray((descriptionExtended + mxfProgram.Description).Trim()),
                Credits = BuildProgramCredits(mxfProgram),
                Date = BuildProgramDate(mxfProgram),
                Categories = BuildProgramCategories(mxfProgram, scheduleEntry),
                Language = MxfStringToXmlText(!string.IsNullOrEmpty(mxfProgram.Language) ? mxfProgram.Language.Substring(0, 2) : null),
                Icons = BuildProgramIcons(mxfProgram),
                Sport = GrabSportEvent(mxfProgram),
                Teams = BuildSportTeams(mxfProgram),
                EpisodeNums = BuildEpisodeNumbers(mxfProgram, scheduleEntry, startTime),
                Video = BuildProgramVideo(scheduleEntry),
                Audio = BuildProgramAudio(scheduleEntry),
                PreviouslyShown = BuildProgramPreviouslyShown(mxfProgram, scheduleEntry),
                Premiere = BuildProgramPremiere(mxfProgram, scheduleEntry),
                Live = BuildLiveFlag(scheduleEntry),
                New = (!scheduleEntry.IsRepeat) ? string.Empty : null,
                Subtitles = BuildProgramSubtitles(scheduleEntry),
                Rating = BuildProgramRatings(mxfProgram, scheduleEntry),
                StarRating = BuildProgramStarRatings(mxfProgram)
            };
        }

        // Titles, SubTitles, and Descriptions
        private static List<XmltvText> MxfStringToXmlTextArray(string mxfString)
        {
            return string.IsNullOrEmpty(mxfString) ? null : new List<XmltvText> {new XmltvText() {Text = mxfString}};
        }

        // Credits
        private static XmltvCredit BuildProgramCredits(MxfProgram mxfProgram)
        {
            if ((mxfProgram.DirectorRole != null && mxfProgram.DirectorRole.Count > 0) || (mxfProgram.ActorRole != null && mxfProgram.ActorRole.Count > 0) ||
                (mxfProgram.WriterRole != null && mxfProgram.WriterRole.Count > 0) || (mxfProgram.ProducerRole != null && mxfProgram.ProducerRole.Count > 0) ||
                (mxfProgram.HostRole != null && mxfProgram.HostRole.Count > 0) || (mxfProgram.GuestActorRole != null && mxfProgram.GuestActorRole.Count > 0))
            {
                return new XmltvCredit()
                {
                    Directors = MxfPersonRankToXmltvCrew(mxfProgram.DirectorRole),
                    Actors = MxfPersonRankToXmltvActors(mxfProgram.ActorRole),
                    Writers = MxfPersonRankToXmltvCrew(mxfProgram.WriterRole),
                    Producers = MxfPersonRankToXmltvCrew(mxfProgram.ProducerRole),
                    Presenters = MxfPersonRankToXmltvCrew(mxfProgram.HostRole),
                    Guests = MxfPersonRankToXmltvCrew(mxfProgram.GuestActorRole)
                };
            }
            return null;
        }
        private static List<string> MxfPersonRankToXmltvCrew(IEnumerable<MxfPersonRank> mxfPersons)
        {
            return mxfPersons?.Select(person => SdMxf.With[0].People[int.Parse(person.Person.Substring(1)) - 1].Name).ToList();
        }
        private static List<XmltvActor> MxfPersonRankToXmltvActors(IEnumerable<MxfPersonRank> mxfPersons)
        {
            return mxfPersons?.Select(person => new XmltvActor() {Actor = SdMxf.With[0].People[int.Parse(person.Person.Substring(1)) - 1].Name, Role = person.Character}).ToList();
        }

        // Date
        private static string BuildProgramDate(MxfProgram mxfProgram)
        {
            if (mxfProgram.IsMovie && mxfProgram.Year > 0)
            {
                return mxfProgram.Year.ToString();
            }

            return !string.IsNullOrEmpty(mxfProgram.OriginalAirdate) ? DateTime.Parse(mxfProgram.OriginalAirdate).ToString("yyyyMMdd") : null;
        }

        // Categories
        private static List<XmltvText> BuildProgramCategories(MxfProgram mxfProgram, MxfScheduleEntry mxfScheduleEntry)
        {
            if (string.IsNullOrEmpty(mxfProgram.Keywords)) return null;
            var categories = new HashSet<string>();
            foreach (var keywordId in mxfProgram.Keywords.Split(','))
            {
                foreach (var keyword in SdMxf.With[0].Keywords.Where(keyword => !keyword.Word.ToLower().Contains("premiere")).Where(keyword => keyword.Id == keywordId))
                {
                    if (keyword.Word.Equals("Uncategorized")) continue;
                    categories.Add(keyword.Word.Equals("Movies") ? "Movie" : keyword.Word);
                    break;
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

            return categories.Count <= 0 ? null : categories.Select(category => new XmltvText() {Text = category}).ToList();
        }

        // Language
        private static XmltvText MxfStringToXmlText(string mxfString)
        {
            return !string.IsNullOrEmpty(mxfString) ? new XmltvText() { Text = mxfString } : null;
        }

        // Icons
        private static List<XmltvIcon> BuildProgramIcons(MxfProgram mxfProgram)
        {
            // a movie will have a guide image from the program
            if (mxfProgram.ProgramImages != null)
            {
                return mxfProgram.ProgramImages.Select(image => new XmltvIcon() {Src = image.Uri, Height = image.Height, Width = image.Width}).ToList();
            }

            // get the series info class from the program if it is a series
            if (string.IsNullOrEmpty(mxfProgram.Series)) return null;
            var mxfSeriesInfo = SdMxf.With[0].SeriesInfos[int.Parse(mxfProgram.Series.Substring(2)) - 1];
            if (!mxfSeriesInfo.Id.Equals(mxfProgram.Series))
            {
                mxfSeriesInfo = SdMxf.With[0].SeriesInfos.SingleOrDefault(arg => arg.Id.Equals(mxfProgram.Series));
            }

            return mxfSeriesInfo?.SeriesImages?.Select(image => new XmltvIcon() {Src = image.Uri, Height = image.Height, Width = image.Width}).ToList();
        }

        private static XmltvText GrabSportEvent(MxfProgram program)
        {
            if (!program.IsSports || program.Genres == null) return null;
            return (from category in program.Genres where !category.ToLower().StartsWith("sport") select new XmltvText() {Text = category}).FirstOrDefault();
        }

        private static List<XmltvText> BuildSportTeams(MxfProgram program)
        {
            if (!program.IsSports || program.Teams == null) return null;
            return program.Teams.Select(team => new XmltvText() {Text = team}).ToList();
        }

        // EpisodeNums
        private static readonly Random RandomNumber = new Random();
        private static List<XmltvEpisodeNum> BuildEpisodeNumbers(MxfProgram mxfProgram, MxfScheduleEntry mxfScheduleEntry, DateTime startTime)
        {
            var list = new List<XmltvEpisodeNum>();
            if (!mxfProgram.TmsId.StartsWith("EPG123"))
            {
                list.Add(new XmltvEpisodeNum()
                {
                    System = "dd_progid",
                    Text = mxfProgram.Uid.Substring(9).Replace("_", ".")
                });
            }

            if (mxfProgram.EpisodeNumber != 0 || mxfScheduleEntry.Part != 0)
            {
                var text =
                    $"{((mxfProgram.SeasonNumber != 0) ? (mxfProgram.SeasonNumber - 1).ToString() : string.Empty)}.{((mxfProgram.EpisodeNumber != 0) ? (mxfProgram.EpisodeNumber - 1).ToString() : string.Empty)}.{((mxfScheduleEntry.Part != 0) ? (mxfScheduleEntry.Part - 1).ToString() + "/" : "0/")}{((mxfScheduleEntry.Parts != 0) ? (mxfScheduleEntry.Parts).ToString() : "1")}";
                list.Add(new XmltvEpisodeNum() { System = "xmltv_ns", Text = text });
            }
            else if (mxfProgram.TmsId.StartsWith("EPG123"))
            {
                // filler data - create oad of scheduled start time
                list.Add(new XmltvEpisodeNum() { System = "original-air-date", Text = startTime.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss") });
            }
            else if (!mxfProgram.TmsId.StartsWith("MV"))
            {
                // add this entry due to Plex identifying anything without an episode number as being a movie
                var oad = mxfProgram.OriginalAirdate;
                if (!mxfScheduleEntry.IsRepeat)
                {
                    oad = startTime.ToLocalTime().ToString("yyyy-MM-dd HH:mm:") + RandomNumber.Next(1, 60).ToString("00");
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
            if (mxfProgram.Series == null) return list;

            var mxfSeriesInfo = SdMxf.With[0].SeriesInfos[int.Parse(mxfProgram.Series.Substring(2)) - 1];
            if (mxfSeriesInfo.TvdbSeriesId != null)
            {
                list.Add(new XmltvEpisodeNum() { System = "thetvdb.com", Text = $"series/{mxfSeriesInfo.TvdbSeriesId}" });
            }
            return list;
        }

        // Video
        private static XmltvVideo BuildProgramVideo(MxfScheduleEntry mxfScheduleEntry)
        {
            return mxfScheduleEntry.IsHdtv ? new XmltvVideo() { Quality = "HDTV" } : null;
        }

        // Audio
        private static XmltvAudio BuildProgramAudio(MxfScheduleEntry mxfScheduleEntry)
        {
            if (mxfScheduleEntry.AudioFormat <= 0) return null;
            var format = string.Empty;
            switch (mxfScheduleEntry.AudioFormat)
            {
                case 1: format = "mono"; break;
                case 2: format = "stereo"; break;
                case 3: format = "dolby"; break;
                case 4: format = "dolby digital"; break;
                case 5: format = "surround"; break;
            }
            return !string.IsNullOrEmpty(format) ? new XmltvAudio() { Stereo = format } : null;
        }

        // Previously Shown
        private static XmltvPreviouslyShown BuildProgramPreviouslyShown(MxfProgram mxfProgram, MxfScheduleEntry mxfScheduleEntry)
        {
            if (mxfScheduleEntry.IsRepeat && !mxfProgram.IsMovie)
            {
                return new XmltvPreviouslyShown() { Text = string.Empty };
            }
            return null;
        }

        // Premiere
        private static XmltvText BuildProgramPremiere(MxfProgram mxfProgram, MxfScheduleEntry mxfScheduleEntry)
        {
            if (!mxfScheduleEntry.IsPremiere) return null;
            string text;
            if (mxfProgram.IsMovie) text = "Movie Premiere";
            else if (mxfProgram.IsSeriesPremiere) text = "Series Premiere";
            else if (mxfProgram.IsSeasonPremiere) text = "Season Premiere";
            else text = "Miniseries Premiere";

            return new XmltvText() { Text = text };
        }

        private static string BuildLiveFlag(MxfScheduleEntry mxfScheduleEntry)
        {
            return !mxfScheduleEntry.IsLive ? null : string.Empty;
        }

        // Subtitles
        private static List<XmltvSubtitles> BuildProgramSubtitles(MxfScheduleEntry mxfScheduleEntry)
        {
            if (!mxfScheduleEntry.IsCc && !mxfScheduleEntry.IsSubtitled && !mxfScheduleEntry.IsSigned) return null;

            var list = new List<XmltvSubtitles>();
            if (mxfScheduleEntry.IsCc)
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

        // Rating
        private static List<XmltvRating> BuildProgramRatings(MxfProgram mxfProgram, MxfScheduleEntry mxfScheduleEntry)
        {
            if (mxfProgram.MpaaRating == 0 && mxfScheduleEntry.TvRating == 0 &&
                mxfProgram.ContentRatings == null) return null;

            var ret = new List<XmltvRating>();
            AddProgramRatingAdvisory(mxfProgram.HasAdult, ret, "Adult Situations");
            AddProgramRatingAdvisory(mxfProgram.HasBriefNudity, ret, "Brief Nudity");
            AddProgramRatingAdvisory(mxfProgram.HasGraphicLanguage, ret, "Graphic Language");
            AddProgramRatingAdvisory(mxfProgram.HasGraphicViolence, ret, "Graphic Violence");
            AddProgramRatingAdvisory(mxfProgram.HasLanguage, ret, "Language");
            AddProgramRatingAdvisory(mxfProgram.HasMildViolence, ret, "Mild Violence");
            AddProgramRatingAdvisory(mxfProgram.HasNudity, ret, "Nudity");
            AddProgramRatingAdvisory(mxfProgram.HasRape, ret, "Rape");
            AddProgramRatingAdvisory(mxfProgram.HasStrongSexualContent, ret, "Strong Sexual Content");
            AddProgramRatingAdvisory(mxfProgram.HasViolence, ret, "Violence");
            AddProgramRating(mxfScheduleEntry, ret);
            return ret;
        }
        private static void AddProgramRating(MxfScheduleEntry mxfScheduleEntry, ICollection<XmltvRating> list)
        {
            if (mxfScheduleEntry.Ratings != null)
            {
                foreach (var rating in mxfScheduleEntry.Ratings)
                {
                    list.Add(new XmltvRating() { System = rating.Key, Value = rating.Value });
                }
            }

            if (mxfScheduleEntry.TvRating != 0)
            {
                var rating = string.Empty;
                switch (mxfScheduleEntry.TvRating)
                {
                    // v-chip is only for US, Canada, and Brazil
                    case 1: rating = "TV-Y"; break;
                    case 2: rating = "TV-Y7"; break;
                    case 3: rating = "TV-G"; break;
                    case 4: rating = "TV-PG"; break;
                    case 5: rating = "TV-14"; break;
                    case 6: rating = "TV-MA"; break;
                }
                if (!string.IsNullOrEmpty(rating))
                {
                    list.Add(new XmltvRating() { System = "VCHIP", Value = rating });
                }
            }
        }
        private static void AddProgramRatingAdvisory(bool mxfProgramAdvise, List<XmltvRating> list, string advisory)
        {
            if (mxfProgramAdvise)
            {
                list.Add(new XmltvRating() { System = "advisory", Value = advisory });
            }
        }

        // StarRating
        private static List<XmltvRating> BuildProgramStarRatings(MxfProgram mxfProgram)
        {
            if (mxfProgram.HalfStars == 0) return null;
            return new List<XmltvRating>
            {
                new XmltvRating()
                {
                    Value = $"{mxfProgram.HalfStars * 0.5:N1}/4"
                }
            };
        }
        #endregion

    }
}