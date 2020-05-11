using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using epg123.MxfXml;

namespace epg123
{
    public static partial class sdJson2mxf
    {
        private static List<string> programQueue;
        private static ConcurrentBag<sdProgram> programResponses = new ConcurrentBag<sdProgram>();

        private static bool buildAllProgramEntries()
        {
            // reset counters
            processedObjects = 0;
            Logger.WriteMessage(string.Format("Entering buildAllProgramEntries() for {0} programs.",
                                totalObjects = sdMxf.With[0].Programs.Count));
            ++processStage; reportProgress();

            // fill mxf programs with cached values and queue the rest
            programQueue = new List<string>();
            for (int i = 0; i < sdMxf.With[0].Programs.Count; ++i)
            {
                string filepath = string.Format("{0}\\{1}", Helper.Epg123CacheFolder, safeFilename(sdMxf.With[0].Programs[i].md5));
                FileInfo file = new FileInfo(filepath);
                if (file.Exists && (file.Length > 0) && !epgCache.JsonFiles.ContainsKey(sdMxf.With[0].Programs[i].md5))
                {
                    using (StreamReader reader = File.OpenText(filepath))
                    {
                        epgCache.AddAsset(sdMxf.With[0].Programs[i].md5, reader.ReadToEnd());
                    }
                }

                if (epgCache.JsonFiles.ContainsKey(sdMxf.With[0].Programs[i].md5))
                {
                    ++processedObjects; reportProgress();
                    using (StringReader reader = new StringReader(epgCache.GetAsset(sdMxf.With[0].Programs[i].md5)))
                    {
                        JsonSerializer serializer = new JsonSerializer();
                        sdProgram program = (sdProgram)serializer.Deserialize(reader, typeof(sdProgram));
                        sdMxf.With[0].Programs[i] = buildMxfProgram(sdMxf.With[0].Programs[i], program);
                    }
                }
                else
                {
                    programQueue.Add(sdMxf.With[0].Programs[i].tmsId);
                }
            }
            Logger.WriteVerbose(string.Format("Found {0} cached program descriptions.", processedObjects));

            // maximum 5000 queries at a time
            if (programQueue.Count > 0)
            {
                Parallel.For(0, (programQueue.Count / MAXQUERIES + 1), new ParallelOptions { MaxDegreeOfParallelism = MAXPARALLELDOWNLOADS }, i =>
                {
                    downloadProgramResponses(i * MAXQUERIES);
                });

                processProgramResponses();
                if (processedObjects != totalObjects)
                {
                    Logger.WriteWarning("Problem occurred during buildAllProgramEntries(). Did not process all program description responses.");
                }
            }
            Logger.WriteInformation(string.Format("Processed {0} program descriptions.", processedObjects));
            Logger.WriteMessage("Exiting buildAllProgramEntries(). SUCCESS.");
            programQueue = null; programResponses = null;
            return true;
        }

        private static void downloadProgramResponses(int start)
        {
            // build the array of programs to request for
            string[] programs = new string[Math.Min(programQueue.Count - start, MAXQUERIES)];
            for (int i = 0; i < programs.Length; ++i)
            {
                programs[i] = programQueue[start + i];
            }

            // request programs from Schedules Direct
            IList<sdProgram> responses = sdAPI.sdGetPrograms(programs);
            if (responses != null)
            {
                Parallel.ForEach(responses, (response) =>
                {
                    programResponses.Add(response);
                });
            }
        }

        private static void processProgramResponses()
        {
            // process request response
            foreach (sdProgram response in programResponses)
            {
                ++processedObjects; reportProgress();

                // determine which program this belongs to
                MxfProgram mxfProgram = sdMxf.With[0].getProgram(response.ProgramID);

                // build a standalone program
                mxfProgram = buildMxfProgram(mxfProgram, response);

                // serialize JSON directly to a file
                if (response.Md5 != null)
                {
                    using (StringWriter writer = new StringWriter())
                    {
                        try
                        {
                            JsonSerializer serializer = new JsonSerializer();
                            serializer.Serialize(writer, response);
                            epgCache.AddAsset(response.Md5, writer.ToString());
                        }
                        catch { }
                    }
                }
                else
                {
                    Logger.WriteWarning(string.Format("Did not cache program {0} due to missing Md5 hash.", mxfProgram.tmsId));
                }
            }
        }

        private static void AddModernMediaUiPlusProgram(sdProgram sd)
        {
            // create entry in ModernMedia UI+ dictionary
            ModernMediaUiPlus.Programs.Add(sd.ProgramID, new ModernMediaUiPlusPrograms()
            {
                ContentRating = sd.ContentRating,
                EventDetails = sd.EventDetails,
                KeyWords = sd.KeyWords,
                Movie = sd.Movie,
                OriginalAirDate = (!string.IsNullOrEmpty(sd.ShowType) && sd.ShowType.ToLower().Contains("series") ? sd.OriginalAirDate : null),
                ShowType = sd.ShowType
            });
        }

        private static MxfProgram buildMxfProgram(MxfProgram prg, sdProgram sd)
        {
            prg.jsonProgramData = sd;

            // populate title, short title, description, and short description
            determineTitlesAndDescriptions(ref prg, sd);

            // set program flags
            setProgramFlags(ref prg, sd);

            // populate program keywords
            determineProgramKeywords(ref prg, sd);

            // determine movie or series information
            if (!string.IsNullOrEmpty(prg.IsMovie))
            {
                // populate mpaa and star rating as well as enable extended information
                determineMovieInfo(ref prg, sd);
            }
            else
            {
                // take care of series and episode fields
                DetermineSeriesInfo(ref prg, sd);
                determineEpisodeInfo(ref prg, sd);
                completeEpisodeTitle(ref prg);
            }

            // set content reason flags
            determineContentAdvisory(ref prg, sd);

            // populate the cast and crew
            determineCastAndCrew(ref prg, sd);

            // add program to array for ModernMedia UI+
            if (config.ModernMediaUiPlusSupport)
            {
                AddModernMediaUiPlusProgram(sd);
            }

            return prg;
        }

        private static void determineTitlesAndDescriptions(ref MxfProgram prg, sdProgram sd)
        {
            // populate titles
            if (sd.Titles != null)
            {
                prg.Title = sd.Titles[0].Title120;
            }
            else
            {
                Logger.WriteWarning(string.Format("Program {0} is missing required content.", sd.ProgramID));
            }
            prg.EpisodeTitle = sd.EpisodeTitle150;

            // populate descriptions and language
            if (sd.Descriptions != null)
            {
                string lang = string.Empty;
                prg.ShortDescription = getDescriptions(sd.Descriptions.Description100, out lang);
                prg.Description = getDescriptions(sd.Descriptions.Description1000, out lang);

                // if short description is empty, not a movie, and append episode option is enabled
                // copy long description into short description
                if (string.IsNullOrEmpty(prg.ShortDescription) && !sd.EntityType.ToLower().Equals("movie") && config.AppendEpisodeDesc)
                {
                    prg.ShortDescription = prg.Description;
                }

                // populate language
                if (!string.IsNullOrEmpty(lang))
                {
                    prg.Language = lang.ToLower();
                }
            }

            prg.OriginalAirdate = sd.OriginalAirDate;
        }

        private static string getDescriptions(IList<sdProgramDescription> description, out string language)
        {
            string ret = string.Empty;
            language = string.Empty;

            if (description != null)
            {
                for (int i = 0; i < description.Count; ++i)
                {
                    if (description[i].DescriptionLanguage.Substring(0, 2) == CultureInfo.CurrentUICulture.TwoLetterISOLanguageName)
                    {
                        // optimal selection ... description language matches computer culture settings
                        language = description[i].DescriptionLanguage;
                        ret = description[i].Description;
                        break;
                    }
                    else if ((description[i].DescriptionLanguage.Substring(0, 2).ToLower() == "en") || (description[i].DescriptionLanguage.ToLower() == "und"))
                    {
                        // without culture match above, english is acceptable alternate
                        language = description[i].DescriptionLanguage;
                        ret = description[i].Description;
                    }
                    else if (string.IsNullOrEmpty(ret))
                    {
                        // first language not of the same culture or english
                        language = description[i].DescriptionLanguage;
                        ret = description[i].Description;
                    }
                }
            }
            return ret;
        }

        private static void setProgramFlags(ref MxfProgram prg, sdProgram sd)
        {
            // transfer genres to mxf program
            prg.IsAction = Helper.tableContains(sd.Genres, "Action");
            prg.IsComedy = Helper.tableContains(sd.Genres, "Comedy");
            prg.IsDocumentary = Helper.tableContains(sd.Genres, "Documentary");
            prg.IsDrama = Helper.tableContains(sd.Genres, "Drama");
            prg.IsEducational = Helper.tableContains(sd.Genres, "Educational");
            prg.IsHorror = Helper.tableContains(sd.Genres, "Horror");
            //prg.IsIndy = null;
            prg.IsKids = Helper.tableContains(sd.Genres, "Children");
            prg.IsMusic = Helper.tableContains(sd.Genres, "Music");
            prg.IsNews = Helper.tableContains(sd.Genres, "News");
            prg.IsReality = Helper.tableContains(sd.Genres, "Reality");
            prg.IsRomance = Helper.tableContains(sd.Genres, "Romance");
            prg.IsScienceFiction = Helper.tableContains(sd.Genres, "Science Fiction");
            prg.IsSoap = Helper.tableContains(sd.Genres, "Soap");
            prg.IsThriller = Helper.tableContains(sd.Genres, "Suspense");

            // below flags are populated when creating the program in processMd5ScheduleEntry(string md5)
            // prg.IsPremiere
            // prg.IsSeasonFinale
            // prg.IsSeasonPremiere
            // prg.IsSeriesFinale
            // prg.IsSeriesPremiere

            // transfer show types to mxf program
            //prg.IsLimitedSeries = null;
            prg.IsMiniseries = Helper.stringContains(sd.ShowType, "Miniseries");
            prg.IsMovie = Helper.stringContains(sd.EntityType, "Movie");
            prg.IsPaidProgramming = Helper.stringContains(sd.ShowType, "Paid Programming");
            //prg.IsProgramEpisodic = null;
            //prg.IsSerial = null;
            prg.IsSeries = Helper.stringContains(sd.ShowType, "Series") ?? Helper.stringContains(sd.ShowType, "Sports non-event");
            prg.IsShortFilm = Helper.stringContains(sd.ShowType, "Short Film");
            prg.IsSpecial = Helper.stringContains(sd.ShowType, "Special");
            prg.IsSports = Helper.stringContains(sd.ShowType, "Sports event");

            // set isGeneric flag if programID starts with "SH", is a series, is not a miniseries, and is not paid programming
            if (prg.tmsId.StartsWith("SH") && ((!string.IsNullOrEmpty(prg.IsSports) && string.IsNullOrEmpty(Helper.stringContains(sd.EntityType, "Sports"))) ||
                                               (!string.IsNullOrEmpty(prg.IsSeries) && string.IsNullOrEmpty(prg.IsMiniseries) && string.IsNullOrEmpty(prg.IsPaidProgramming))))
            {
                prg.IsGeneric = "true";
            }

            // special case to flag sports events ** I CURRENTLY SEE NO ADVANTAGE TO DOING THIS **
            //if (!string.IsNullOrEmpty(Helper.stringContains(sd.ShowType, "Sports Event")))
            //{
            //    // find all schedule entries that link to this program
            //    foreach (MxfScheduleEntries mxfScheduleEntries in sdMxf.With[0].ScheduleEntries)
            //    {
            //        foreach (MxfScheduleEntry mxfScheduleEntry in mxfScheduleEntries.ScheduleEntry)
            //        {
            //            if (mxfScheduleEntry.Program.Equals(prg.Id) && !string.IsNullOrEmpty(mxfScheduleEntry.IsLive))
            //            {
            //                mxfScheduleEntry.IsLiveSports = "true";
            //            }
            //        }
            //    }
            //}
        }

        private static void determineProgramKeywords(ref MxfProgram prg, sdProgram sd)
        {
            // determine primary group of program
            GROUPS group = GROUPS.UNKNOWN;
            if (!string.IsNullOrEmpty(prg.IsMovie)) group = GROUPS.MOVIES;
            else if (!string.IsNullOrEmpty(prg.IsPaidProgramming)) group = GROUPS.PAIDPROGRAMMING;
            else if (!string.IsNullOrEmpty(prg.IsSports)) group = GROUPS.SPORTS;
            else if (!string.IsNullOrEmpty(prg.IsKids)) group = GROUPS.KIDS;
            else if (!string.IsNullOrEmpty(prg.IsEducational)) group = GROUPS.EDUCATIONAL;
            else if (!string.IsNullOrEmpty(prg.IsNews)) group = GROUPS.NEWS;
            else if (!string.IsNullOrEmpty(prg.IsSpecial)) group = GROUPS.SPECIAL;
            else if (!string.IsNullOrEmpty(prg.IsReality)) group = GROUPS.REALITY;
            else if (!string.IsNullOrEmpty(prg.IsSeries)) group = GROUPS.SERIES;

            // build the keywords/categories
            if (group != GROUPS.UNKNOWN)
            {
                prg.Keywords = string.Format("k{0}", (int)group + 1);

                // add premiere categories as necessary
                if (!string.IsNullOrEmpty(prg.IsSeasonPremiere) || !string.IsNullOrEmpty(prg.IsSeriesPremiere))
                {
                    prg.Keywords += string.Format(",k{0}", (int)GROUPS.PREMIERES + 1);
                    if (!string.IsNullOrEmpty(prg.IsSeasonPremiere)) prg.Keywords += "," + sdMxf.With[0].KeywordGroups[(int)GROUPS.PREMIERES].getKeywordId("Season Premiere");
                    if (!string.IsNullOrEmpty(prg.IsSeriesPremiere)) prg.Keywords += "," + sdMxf.With[0].KeywordGroups[(int)GROUPS.PREMIERES].getKeywordId("Series Premiere");
                }
                else if (!string.IsNullOrEmpty(prg.IsPremiere))
                {
                    if (group == GROUPS.MOVIES)
                    {
                        prg.Keywords += "," + sdMxf.With[0].KeywordGroups[(int)group].getKeywordId("Premiere");
                    }
                    else if (Helper.tableContains(sd.Genres, "miniseries") == "true")
                    {
                        prg.Keywords += string.Format(",k{0}", (int)GROUPS.PREMIERES + 1);
                        prg.Keywords += "," + sdMxf.With[0].KeywordGroups[(int)GROUPS.PREMIERES].getKeywordId("Miniseries Premiere");
                    }
                }

                // now add the real categories
                if (sd.Genres != null)
                {
                    foreach (string genre in sd.Genres)
                    {
                        string key = sdMxf.With[0].KeywordGroups[(int)group].getKeywordId(genre);
                        List<string> keys = prg.Keywords.Split(',').ToList();
                        if (!keys.Contains(key))
                        {
                            prg.Keywords += "," + key;
                        }
                    }
                }
                if (prg.Keywords.Length < 5)
                {
                    string key = sdMxf.With[0].KeywordGroups[(int)group].getKeywordId("Uncategorized");
                    prg.Keywords += "," + key;
                }
            }
        }

        private static void determineMovieInfo(ref MxfProgram prg, sdProgram sd)
        {
            // fill MPAA rating
            prg.MpaaRating = decodeMpaaRating(sd.ContentRating);

            // populate movie specific attributes
            if (sd.Movie != null)
            {
                prg.Year = sd.Movie.Year;
                prg.HalfStars = decodeStarRating(sd.Movie.QualityRating);
            }
            else if (!string.IsNullOrEmpty(prg.OriginalAirdate))
            {
                prg.Year = prg.OriginalAirdate.Substring(0, 4);
            }

            //prg.HasExtendedCastAndCrew = "true";
            //prg.HasExtendedSynopsis = "true";
            //prg.HasReview = "true";
            //prg.HasSimilarPrograms = "true";
        }

        private static void DetermineSeriesInfo(ref MxfProgram mxfProgram, sdProgram sdProgram)
        {
            // do not extend cast and crew for paid programming
            if (string.IsNullOrEmpty(mxfProgram.IsPaidProgramming))
            {
                //mxfProgram.HasExtendedCastAndCrew = "true";
            }

            // for sports programs that start with "SP", create a series entry based on program title
            // this groups them all together as a series for recordings
            MxfSeriesInfo mxfSeriesInfo;
            if (mxfProgram.tmsId.StartsWith("SP"))
            {
                string name = mxfProgram.Title.Replace(' ', '_');
                mxfSeriesInfo = sdMxf.With[0].getSeriesInfo(name);
                sportsSeries.Add(name, mxfProgram.tmsId.Substring(0, 10));
            }
            else
            {
                // create a seriesInfo entry if needed
                mxfSeriesInfo = sdMxf.With[0].getSeriesInfo(mxfProgram.tmsId.Substring(2, 8));
            }

            mxfSeriesInfo.Title = mxfSeriesInfo.Title ?? mxfProgram.Title;
            mxfProgram.Series = mxfSeriesInfo.Id;
        }

        private static void determineEpisodeInfo(ref MxfProgram prg, sdProgram sd)
        {
            if (sd.EntityType == "Episode")
            {
                // use the last 4 numbers as a production number
                int episode = int.Parse(prg.tmsId.Substring(10));
                if (episode > 0)
                {
                    prg.EpisodeNumber = episode.ToString();
                }

                if (sd.Metadata != null)
                {
                    // grab season and episode numbers if available
                    foreach (Dictionary<string, sdProgramMetadataProvider> providers in sd.Metadata)
                    {
                        sdProgramMetadataProvider provider;
                        if (providers.TryGetValue("Gracenote", out provider))
                        {
                            if ((provider == null) || (provider.EpisodeNumber == 0)) continue;

                            prg.SeasonNumber = provider.SeasonNumber.ToString();
                            prg.EpisodeNumber = provider.EpisodeNumber.ToString();
                            if (!config.TheTVDBNumbers) break;
                        }
                        else if (providers.TryGetValue("TheTVDB", out provider))
                        {
                            if ((provider == null) || (provider.EpisodeNumber == 0) || (provider.SeasonNumber > 255)) continue;

                            prg.SeasonNumber = provider.SeasonNumber.ToString();
                            prg.EpisodeNumber = provider.EpisodeNumber.ToString();
                            if (config.TheTVDBNumbers) break;
                        }
                    }
                }

                // if there is a season number, create as seasonInfo entry
                if (!string.IsNullOrEmpty(prg.SeasonNumber))
                {
                    prg.Season = sdMxf.With[0].getSeasonId(prg.tmsId.Substring(2, 8), prg.SeasonNumber);
                }
            }
        }

        private static void completeEpisodeTitle(ref MxfProgram prg)
        {
            // by request, if there is no episode title, and the program is not generic, duplicate the program title in the episode title
            if (prg.tmsId.StartsWith("EP") && string.IsNullOrEmpty(prg.EpisodeTitle))
            {
                prg.EpisodeTitle = prg.Title;
            }
            else if (string.IsNullOrEmpty(prg.EpisodeTitle)) return;

            string se = config.AlternateSEFormat ? "S{0}:E{1} " : "s{0:D2}e{1:D2} ";
            if (!string.IsNullOrEmpty(prg.SeasonNumber))
            {
                se = string.Format(se, int.Parse(prg.SeasonNumber), int.Parse(prg.EpisodeNumber));
            }
            else if (!string.IsNullOrEmpty(prg.EpisodeNumber))
            {
                se = string.Format("#{0} ", int.Parse(prg.EpisodeNumber));
            }
            else se = string.Empty;

            // prefix episode title with season and episode numbers as configured
            if (config.PrefixEpisodeTitle)
            {
                prg.EpisodeTitle = se + prg.EpisodeTitle;
            }

            // prefix episode description with season and episode numbers as configured
            if (config.PrefixEpisodeDescription)
            {
                prg.Description = se + prg.Description;
                if (!string.IsNullOrEmpty(prg.ShortDescription))
                {
                    prg.ShortDescription = se + prg.ShortDescription;
                }
            }

            // append season and episode numbers to the program description as configured
            if (config.AppendEpisodeDesc)
            {
                if (!string.IsNullOrEmpty(prg.SeasonNumber) && !string.IsNullOrEmpty(prg.EpisodeNumber))
                {
                    prg.Description += string.Format("\u000D\u000ASeason {0}, Episode {1}",
                        int.Parse(prg.SeasonNumber), int.Parse(prg.EpisodeNumber));
                }
                else if (!string.IsNullOrEmpty(prg.EpisodeNumber))
                {
                    prg.Description += string.Format("\u000D\u000AProduction #{0}", prg.EpisodeNumber);
                }
            }

            // append part/parts to episode title as needed
            if (prg._part > 0)
            {
                prg.EpisodeTitle += string.Format(" ({0}/{1})", prg._part, prg._parts);
            }
        }

        private static void determineContentAdvisory(ref MxfProgram prg, sdProgram sd)
        {
            // fill content ratings and advisories; set flags
            HashSet<string> advisories = new HashSet<string>();
            if (sd.ContentRating != null)
            {
                string[] ratings = !string.IsNullOrEmpty(config.RatingsOrigin) ? config.RatingsOrigin.Split(',') : new string[] { RegionInfo.CurrentRegion.ThreeLetterISORegionName };
                prg.contentRatings = new Dictionary<string, string>();
                foreach (sdProgramContentRating rating in sd.ContentRating)
                {
                    if (string.IsNullOrEmpty(rating.Country) || !string.IsNullOrEmpty(Helper.tableContains(ratings, "ALL")) || !string.IsNullOrEmpty(Helper.tableContains(ratings, rating.Country)))
                    {
                        prg.contentRatings.Add(rating.Body, rating.Code);
                    }

                    if (rating.ContentAdvisory != null)
                    {
                        foreach (string reason in rating.ContentAdvisory)
                        {
                            advisories.Add(reason);
                        }
                    }
                }
            }
            if (sd.ContentAdvisory != null)
            {
                foreach (string reason in sd.ContentAdvisory)
                {
                    advisories.Add(reason);
                }
            }

            if (advisories.Count > 0)
            {
                string[] advisoryTable = advisories.ToArray();

                // set flags
                prg.HasAdult = Helper.tableContains(advisoryTable, "Adult Situations");
                prg.HasBriefNudity = Helper.tableContains(advisoryTable, "Brief Nudity");
                prg.HasGraphicLanguage = Helper.tableContains(advisoryTable, "Graphic Language");
                prg.HasGraphicViolence = Helper.tableContains(advisoryTable, "Graphic Violence");
                prg.HasLanguage = Helper.tableContains(advisoryTable, "Adult Language");
                prg.HasMildViolence = Helper.tableContains(advisoryTable, "Mild Violence");
                prg.HasNudity = Helper.tableContains(advisoryTable, "Nudity");
                prg.HasRape = Helper.tableContains(advisoryTable, "Rape");
                prg.HasStrongSexualContent = Helper.tableContains(advisoryTable, "Strong Sexual Content");
                prg.HasViolence = Helper.tableContains(advisoryTable, "Violence");

                prg.contentAdvisories = advisoryTable;
            }
        }

        private static void determineCastAndCrew(ref MxfProgram prg, sdProgram sd)
        {
            prg.ActorRole = getPersons(sd.Cast, new string[] { "Actor", "Voice", "Judge" });
            prg.DirectorRole = getPersons(sd.Crew, new string[] { "Director" });
            prg.GuestActorRole = getPersons(sd.Cast, new string[] { "Guest" }); // "Guest Star", "Guest"
            prg.HostRole = getPersons(sd.Cast, new string[] { "Anchor", "Host", "Presenter", "Narrator", "Correspondent" });
            prg.ProducerRole = getPersons(sd.Crew, new string[] { "Executive Producer" }); // "Producer", "Executive Producer", "Co-Executive Producer"
            prg.WriterRole = getPersons(sd.Crew, new string[] { "Writer", "Story" }); // "Screenwriter", "Writer", "Co-Writer"
        }

        private static List<MxfPersonRank> getPersons(IList<sdProgramPerson> persons, string[] roles)
        {
            if (persons != null)
            {
                List<string> personName = new List<string>();
                List<MxfPersonRank> ret = new List<MxfPersonRank>();
                foreach (sdProgramPerson person in persons)
                {
                    foreach (string role in roles)
                    {
                        if (person.Role.ToLower().Contains(role.ToLower()) && !personName.Contains(person.Name))
                        {
                            ret.Add(new MxfPersonRank()
                            {
                                Person = sdMxf.With[0].getPersonId(person.Name),
                                Rank = int.Parse(person.BillingOrder),
                                Character = person.CharacterName
                            });
                            personName.Add(person.Name);
                            break;
                        }
                    }
                }
                return ret;
            }
            return null;
        }

        private static string decodeMpaaRating(IList<sdProgramContentRating> sdProgramContentRatings)
        {
            if (sdProgramContentRatings != null)
            {
                int maxValue = 0;
                foreach (sdProgramContentRating rating in sdProgramContentRatings)
                {
                    if (!rating.Body.ToLower().Equals("motion picture association of america")) continue;

                    switch (rating.Code.ToLower().Replace("-", ""))
                    {
                        case "g":
                            maxValue = Math.Max(maxValue, 1);
                            break;
                        case "pg":
                            maxValue = Math.Max(maxValue, 2);
                            break;
                        case "pg13":
                            maxValue = Math.Max(maxValue, 3);
                            break;
                        case "r":
                            maxValue = Math.Max(maxValue, 4);
                            break;
                        case "nc17":
                            maxValue = Math.Max(maxValue, 5);
                            break;
                        case "x":
                            maxValue = Math.Max(maxValue, 6);
                            break;
                        case "nr":
                            maxValue = Math.Max(maxValue, 7);
                            break;
                        case "ao":
                            maxValue = Math.Max(maxValue, 8);
                            break;
                        default:
                            break;
                    }
                }
                if (maxValue > 0) return maxValue.ToString();
            }
            return null;
        }

        private static string decodeStarRating(IList<sdProgramQualityRating> sdProgramQualityRatings)
        {
            if (sdProgramQualityRatings != null)
            {
                double maxValue = 0.0;
                foreach (sdProgramQualityRating rating in sdProgramQualityRatings)
                {
                    if (!string.IsNullOrEmpty(rating.MaxRating))
                    {
                        double numerator = double.Parse(rating.Rating, CultureInfo.InvariantCulture);
                        double denominator = double.Parse(rating.MaxRating, CultureInfo.InvariantCulture);
                        maxValue = Math.Max(numerator / denominator, maxValue);
                    }
                    else
                    {
                        // with no reference ...
                    }
                }

                // return rounded number of half stars in a 4 star scale
                if (maxValue > 0.0) return ((int)(8.0 * maxValue + 0.125)).ToString();
            }
            return null;
        }
    }
}