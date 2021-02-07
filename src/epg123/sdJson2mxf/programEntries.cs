using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using epg123.MxfXml;
using epg123.SchedulesDirectAPI;
using Newtonsoft.Json;

namespace epg123.sdJson2mxf
{
    internal static partial class sdJson2Mxf
    {
        private static List<string> programQueue;
        private static ConcurrentBag<sdProgram> programResponses = new ConcurrentBag<sdProgram>();

        private static bool BuildAllProgramEntries()
        {
            // reset counters
            processedObjects = 0;
            Logger.WriteMessage($"Entering buildAllProgramEntries() for {totalObjects = SdMxf.With[0].Programs.Count} programs.");
            ++processStage; ReportProgress();

            // fill mxf programs with cached values and queue the rest
            programQueue = new List<string>();
            for (var i = 0; i < SdMxf.With[0].Programs.Count; ++i)
            {
                var filepath = $"{Helper.Epg123CacheFolder}\\{SafeFilename(SdMxf.With[0].Programs[i].Md5)}";
                var file = new FileInfo(filepath);
                if (file.Exists && (file.Length > 0) && !epgCache.JsonFiles.ContainsKey(SdMxf.With[0].Programs[i].Md5))
                {
                    using (var reader = File.OpenText(filepath))
                    {
                        epgCache.AddAsset(SdMxf.With[0].Programs[i].Md5, reader.ReadToEnd());
                    }
                }

                if (epgCache.JsonFiles.ContainsKey(SdMxf.With[0].Programs[i].Md5))
                {
                    try
                    {
                        using (var reader = new StringReader(epgCache.GetAsset(SdMxf.With[0].Programs[i].Md5)))
                        {
                            var serializer = new JsonSerializer();
                            var program = (sdProgram)serializer.Deserialize(reader, typeof(sdProgram));
                            SdMxf.With[0].Programs[i] = BuildMxfProgram(SdMxf.With[0].Programs[i], program);
                        }
                        ++processedObjects; ReportProgress();
                    }
                    catch
                    {
                        programQueue.Add(SdMxf.With[0].Programs[i].TmsId);
                    }
                }
                else
                {
                    programQueue.Add(SdMxf.With[0].Programs[i].TmsId);
                }
            }
            Logger.WriteVerbose($"Found {processedObjects} cached program descriptions.");

            // maximum 5000 queries at a time
            if (programQueue.Count > 0)
            {
                Parallel.For(0, (programQueue.Count / MaxQueries + 1), new ParallelOptions { MaxDegreeOfParallelism = MaxParallelDownloads }, i =>
                {
                    DownloadProgramResponses(i * MaxQueries);
                });

                ProcessProgramResponses();
                if (processedObjects != totalObjects)
                {
                    Logger.WriteWarning("Problem occurred during buildAllProgramEntries(). Did not process all program description responses.");
                }
            }
            Logger.WriteInformation($"Processed {processedObjects} program descriptions.");
            Logger.WriteMessage("Exiting buildAllProgramEntries(). SUCCESS.");
            programQueue = null; programResponses = null;
            return true;
        }

        private static void DownloadProgramResponses(int start)
        {
            // reject 0 requests
            if (programQueue.Count - start < 1) return;

            // build the array of programs to request for
            var programs = new string[Math.Min(programQueue.Count - start, MaxQueries)];
            for (var i = 0; i < programs.Length; ++i)
            {
                programs[i] = programQueue[start + i];
            }

            // request programs from Schedules Direct
            var responses = sdApi.SdGetPrograms(programs);
            if (responses != null)
            {
                Parallel.ForEach(responses, (response) =>
                {
                    programResponses.Add(response);
                });
            }
        }

        private static void ProcessProgramResponses()
        {
            // process request response
            foreach (var response in programResponses)
            {
                ++processedObjects; ReportProgress();

                // determine which program this belongs to
                var mxfProgram = SdMxf.With[0].GetProgram(response.ProgramId);

                // build a standalone program
                mxfProgram = BuildMxfProgram(mxfProgram, response);

                // serialize JSON directly to a file
                if (response.Md5 != null)
                {
                    using (var writer = new StringWriter())
                    {
                        try
                        {
                            var serializer = new JsonSerializer();
                            serializer.Serialize(writer, response);
                            epgCache.AddAsset(response.Md5, writer.ToString());
                        }
                        catch
                        {
                            // ignored
                        }
                    }
                }
                else
                {
                    Logger.WriteWarning($"Did not cache program {mxfProgram.TmsId} due to missing Md5 hash.");
                }
            }
        }

        private static void AddModernMediaUiPlusProgram(sdProgram sd)
        {
            // create entry in ModernMedia UI+ dictionary
            ModernMediaUiPlus.Programs.Add(sd.ProgramId, new ModernMediaUiPlusPrograms()
            {
                ContentRating = sd.ContentRating,
                EventDetails = sd.EventDetails,
                KeyWords = sd.KeyWords,
                Movie = sd.Movie,
                OriginalAirDate = (!string.IsNullOrEmpty(sd.ShowType) && sd.ShowType.ToLower().Contains("series") ? sd.OriginalAirDate.ToString("s") : null),
                ShowType = sd.ShowType
            });
        }

        private static MxfProgram BuildMxfProgram(MxfProgram prg, sdProgram sd)
        {
            // populate stuff for xmltv
            prg.Genres = sd.Genres;
            if (sd.EventDetails?.Teams != null)
            {
                prg.Teams = new List<string>();
                foreach (var team in sd.EventDetails.Teams)
                {
                    prg.Teams.Add(team.Name);
                }
            }

            // populate title, short title, description, and short description
            DetermineTitlesAndDescriptions(ref prg, sd);

            // set program flags
            SetProgramFlags(ref prg, sd);

            // populate program keywords
            DetermineProgramKeywords(ref prg, sd);

            // determine movie or series information
            if (prg.IsMovie)
            {
                // populate mpaa and star rating as well as enable extended information
                DetermineMovieInfo(ref prg, sd);
            }
            else
            {
                // take care of series and episode fields
                DetermineSeriesInfo(ref prg, sd);
                DetermineEpisodeInfo(ref prg, sd);
                CompleteEpisodeTitle(ref prg);
            }

            // set content reason flags
            DetermineContentAdvisory(ref prg, sd);

            // populate the cast and crew
            DetermineCastAndCrew(ref prg, sd);

            // add program to array for ModernMedia UI+
            if (config.ModernMediaUiPlusSupport)
            {
                AddModernMediaUiPlusProgram(sd);
            }

            return prg;
        }

        private static void DetermineTitlesAndDescriptions(ref MxfProgram prg, sdProgram sd)
        {
            // populate titles
            if (sd.Titles != null)
            {
                prg.Title = sd.Titles[0].Title120;
            }
            else
            {
                Logger.WriteWarning($"Program {sd.ProgramId} is missing required content.");
            }
            prg.EpisodeTitle = sd.EpisodeTitle150;

            // populate descriptions and language
            if (sd.Descriptions != null)
            {
                prg.ShortDescription = GetDescriptions(sd.Descriptions.Description100, out var lang);
                prg.Description = GetDescriptions(sd.Descriptions.Description1000, out lang);

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

            prg.OriginalAirdate = sd.OriginalAirDate.ToString();
        }

        private static string GetDescriptions(IList<sdProgramDescription> descriptions, out string language)
        {
            var ret = string.Empty;
            language = string.Empty;

            if (descriptions == null) return ret;

            foreach (var description in descriptions)
            {
                if (description.DescriptionLanguage.Substring(0, 2) == CultureInfo.CurrentUICulture.TwoLetterISOLanguageName)
                {
                    // optimal selection ... description language matches computer culture settings
                    language = description.DescriptionLanguage;
                    ret = description.Description;
                    break;
                }

                if (description.DescriptionLanguage.Substring(0, 2).ToLower() == "en" || description.DescriptionLanguage.ToLower() == "und")
                {
                    // without culture match above, english is acceptable alternate
                    language = description.DescriptionLanguage;
                    ret = description.Description;
                }
                else if (string.IsNullOrEmpty(ret))
                {
                    // first language not of the same culture or english
                    language = description.DescriptionLanguage;
                    ret = description.Description;
                }
            }
            return ret;
        }

        private static void SetProgramFlags(ref MxfProgram prg, sdProgram sd)
        {
            // transfer genres to mxf program
            prg.IsAction = Helper.TableContains(sd.Genres, "Action");
            prg.IsAdultOnly = Helper.TableContains(sd.Genres, "Adults Only");
            prg.IsComedy = Helper.TableContains(sd.Genres, "Comedy");
            prg.IsDocumentary = Helper.TableContains(sd.Genres, "Documentary");
            prg.IsDrama = Helper.TableContains(sd.Genres, "Drama");
            prg.IsEducational = Helper.TableContains(sd.Genres, "Educational");
            prg.IsHorror = Helper.TableContains(sd.Genres, "Horror");
            //prg.IsIndy = null;
            prg.IsKids = Helper.TableContains(sd.Genres, "Children");
            prg.IsMusic = Helper.TableContains(sd.Genres, "Music");
            prg.IsNews = Helper.TableContains(sd.Genres, "News");
            prg.IsReality = Helper.TableContains(sd.Genres, "Reality");
            prg.IsRomance = Helper.TableContains(sd.Genres, "Romance");
            prg.IsScienceFiction = Helper.TableContains(sd.Genres, "Science Fiction");
            prg.IsSoap = Helper.TableContains(sd.Genres, "Soap");
            prg.IsThriller = Helper.TableContains(sd.Genres, "Suspense");

            // below flags are populated when creating the program in processMd5ScheduleEntry(string md5)
            // prg.IsPremiere
            // prg.IsSeasonFinale
            // prg.IsSeasonPremiere
            // prg.IsSeriesFinale
            // prg.IsSeriesPremiere

            // transfer show types to mxf program
            //prg.IsLimitedSeries = null;
            prg.IsMiniseries = Helper.StringContains(sd.ShowType, "Miniseries");
            prg.IsMovie = Helper.StringContains(sd.EntityType, "Movie");
            prg.IsPaidProgramming = Helper.StringContains(sd.ShowType, "Paid Programming");
            //prg.IsProgramEpisodic = null;
            //prg.IsSerial = null;
            prg.IsSeries = Helper.StringContains(sd.ShowType, "Series") && !Helper.TableContains(sd.Genres, "Sports talk");
            prg.IsShortFilm = Helper.StringContains(sd.ShowType, "Short Film");
            prg.IsSpecial = Helper.StringContains(sd.ShowType, "Special");
            prg.IsSports = Helper.StringContains(sd.ShowType, "Sports event") || 
                           Helper.StringContains(sd.ShowType, "Sports non-event") || 
                           Helper.TableContains(sd.Genres, "Sports talk");

            // set isGeneric flag if programID starts with "SH", is a series, is not a miniseries, and is not paid programming
            if (prg.TmsId.StartsWith("SH") && (prg.IsSports && !Helper.StringContains(sd.EntityType, "Sports") ||
                                               prg.IsSeries && !prg.IsMiniseries && !prg.IsPaidProgramming))
            {
                prg.IsGeneric = true;
            }
        }

        private static void DetermineProgramKeywords(ref MxfProgram prg, sdProgram sd)
        {
            // determine primary group of program
            var group = keygroups.UNKNOWN;
            if (prg.IsMovie) group = keygroups.MOVIES;
            else if (prg.IsPaidProgramming) group = keygroups.PAIDPROGRAMMING;
            else if (prg.IsSports) group = keygroups.SPORTS;
            else if (prg.IsKids) group = keygroups.KIDS;
            else if (prg.IsEducational) group = keygroups.EDUCATIONAL;
            else if (prg.IsNews) group = keygroups.NEWS;
            else if (prg.IsSpecial) group = keygroups.SPECIAL;
            else if (prg.IsReality) group = keygroups.REALITY;
            else if (prg.IsSeries) group = keygroups.SERIES;

            // build the keywords/categories
            if (group == keygroups.UNKNOWN) return;
            prg.Keywords = $"k{(int) group + 1}";

            // add premiere categories as necessary
            if (prg.IsSeasonPremiere || prg.IsSeriesPremiere)
            {
                prg.Keywords += $",k{(int) keygroups.PREMIERES + 1}";
                if (prg.IsSeasonPremiere) prg.Keywords += "," + SdMxf.With[0].KeywordGroups[(int)keygroups.PREMIERES].GetKeywordId("Season Premiere");
                if (prg.IsSeriesPremiere) prg.Keywords += "," + SdMxf.With[0].KeywordGroups[(int)keygroups.PREMIERES].GetKeywordId("Series Premiere");
            }
            else if (prg.IsPremiere)
            {
                if (group == keygroups.MOVIES)
                {
                    prg.Keywords += "," + SdMxf.With[0].KeywordGroups[(int)group].GetKeywordId("Premiere");
                }
                else if (Helper.TableContains(sd.Genres, "miniseries"))
                {
                    prg.Keywords += $",k{(int) keygroups.PREMIERES + 1}";
                    prg.Keywords += "," + SdMxf.With[0].KeywordGroups[(int)keygroups.PREMIERES].GetKeywordId("Miniseries Premiere");
                }
            }

            // now add the real categories
            if (sd.Genres != null)
            {
                foreach (var genre in sd.Genres)
                {
                    var key = SdMxf.With[0].KeywordGroups[(int)group].GetKeywordId(genre);
                    var keys = prg.Keywords.Split(',').ToList();
                    if (!keys.Contains(key))
                    {
                        prg.Keywords += "," + key;
                    }
                }
            }

            if (prg.Keywords.Length >= 5) return;
            {
                var key = SdMxf.With[0].KeywordGroups[(int)group].GetKeywordId("Uncategorized");
                prg.Keywords += "," + key;
            }
        }

        private static void DetermineMovieInfo(ref MxfProgram prg, sdProgram sd)
        {
            // fill MPAA rating
            prg.MpaaRating = DecodeMpaaRating(sd.ContentRating);

            // populate movie specific attributes
            if (sd.Movie != null)
            {
                prg.Year = sd.Movie.Year;
                prg.HalfStars = DecodeStarRating(sd.Movie.QualityRating);
            }
            else if (!string.IsNullOrEmpty(prg.OriginalAirdate))
            {
                prg.Year = int.Parse(prg.OriginalAirdate.Substring(0, 4));
            }

            //prg.HasExtendedCastAndCrew = "true";
            //prg.HasExtendedSynopsis = "true";
            //prg.HasReview = "true";
            //prg.HasSimilarPrograms = "true";
        }

        private static void DetermineSeriesInfo(ref MxfProgram mxfProgram, sdProgram sdProgram)
        {
            // for sports programs that start with "SP", create a series entry based on program title
            // this groups them all together as a series for recordings
            MxfSeriesInfo mxfSeriesInfo;
            if (mxfProgram.TmsId.StartsWith("SP"))
            {
                var name = mxfProgram.Title.Replace(' ', '_');
                mxfSeriesInfo = SdMxf.With[0].GetSeriesInfo(name);
                sportsSeries.Add(name, mxfProgram.TmsId.Substring(0, 10));
            }
            else
            {
                // create a seriesInfo entry if needed
                mxfSeriesInfo = SdMxf.With[0].GetSeriesInfo(mxfProgram.TmsId.Substring(2, 8));
                if (mxfSeriesInfo.TvdbSeriesId == null && sdProgram.Metadata != null)
                {
                    foreach (var providers in sdProgram.Metadata)
                    {
                        if (providers.TryGetValue("TheTVDB", out var provider))
                        {
                            mxfSeriesInfo.TvdbSeriesId = provider.SeriesId.ToString();
                        }
                    }
                }

                if (mxfProgram.IsGeneric)
                {
                    // go ahead and create/update the cache entry as needed
                    if (epgCache.JsonFiles.ContainsKey(mxfProgram.TmsId))
                    {
                        try
                        {
                            using (var reader = new StringReader(epgCache.GetAsset(mxfProgram.TmsId)))
                            {
                                var serializer = new JsonSerializer();
                                var cached = (sdGenericDescriptions)serializer.Deserialize(reader, typeof(sdGenericDescriptions));
                                if (cached.StartAirdate == null)
                                {
                                    cached.StartAirdate = mxfProgram.OriginalAirdate ?? string.Empty;
                                    using (var writer = new StringWriter())
                                    {
                                        serializer.Serialize(writer, cached);
                                        epgCache.UpdateAssetJsonEntry(mxfProgram.TmsId, writer.ToString());
                                    }
                                }
                            }
                        }
                        catch
                        {
                            // ignored
                        }
                    }
                    else
                    {
                        var newEntry = new sdGenericDescriptions()
                        {
                            Code = 0,
                            Description1000 = mxfProgram.Description,
                            Description100 = mxfProgram.ShortDescription,
                            StartAirdate = mxfProgram.OriginalAirdate ?? string.Empty
                        };

                        var serializer = new JsonSerializer();
                        using (var writer = new StringWriter())
                        {
                            serializer.Serialize(writer, newEntry);
                            epgCache.AddAsset(mxfProgram.TmsId, writer.ToString());
                        }
                    }
                }
            }

            mxfSeriesInfo.Title = mxfSeriesInfo.Title ?? mxfProgram.Title;
            mxfProgram.Series = mxfSeriesInfo.Id;
        }

        private static void DetermineEpisodeInfo(ref MxfProgram prg, sdProgram sd)
        {
            if (sd.EntityType != "Episode") return;

            // use the last 4 numbers as a production number
            var episode = int.Parse(prg.TmsId.Substring(10));
            if (episode != 0)
            {
                prg.EpisodeNumber = episode;
            }

            if (sd.Metadata != null)
            {
                // grab season and episode numbers if available
                foreach (var providers in sd.Metadata)
                {
                    if (providers.TryGetValue("Gracenote", out var provider))
                    {
                        if (provider == null || provider.EpisodeNumber == 0) continue;

                        prg.SeasonNumber = provider.SeasonNumber;
                        prg.EpisodeNumber = provider.EpisodeNumber;
                        if (!config.TheTvdbNumbers) break;
                    }
                    else if (providers.TryGetValue("TheTVDB", out provider))
                    {
                        if (provider == null || provider.EpisodeNumber == 0 || provider.SeasonNumber > DateTime.Now.Year) continue;

                        prg.SeasonNumber = provider.SeasonNumber;
                        prg.EpisodeNumber = provider.EpisodeNumber;
                        if (config.TheTvdbNumbers) break;
                    }
                }
            }

            // if there is a season number, create as seasonInfo entry
            if (prg.SeasonNumber != 0)
            {
                prg.Season = SdMxf.With[0].GetSeasonId(prg.TmsId.Substring(2, 8), prg.SeasonNumber);
            }
        }

        private static void CompleteEpisodeTitle(ref MxfProgram prg)
        {
            // by request, if there is no episode title, and the program is not generic, duplicate the program title in the episode title
            if (prg.TmsId.StartsWith("EP") && string.IsNullOrEmpty(prg.EpisodeTitle))
            {
                prg.EpisodeTitle = prg.Title;
            }
            else if (string.IsNullOrEmpty(prg.EpisodeTitle)) return;

            var se = config.AlternateSEFormat ? "S{0}:E{1} " : "s{0:D2}e{1:D2} ";
            if (prg.SeasonNumber != 0)
            {
                se = string.Format(se, prg.SeasonNumber, prg.EpisodeNumber);
            }
            else if (prg.EpisodeNumber != 0)
            {
                se = $"#{prg.EpisodeNumber} ";
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
                // add space before appending season and episode numbers in case there is no short description
                if (prg.SeasonNumber != 0 && prg.EpisodeNumber != 0)
                {
                    prg.Description += $" \u000D\u000ASeason {prg.SeasonNumber}, Episode {prg.EpisodeNumber}";
                }
                else if (prg.EpisodeNumber != 0)
                {
                    prg.Description += $" \u000D\u000AProduction #{prg.EpisodeNumber}";
                }
            }

            // append part/parts to episode title as needed
            if (prg.Part > 0)
            {
                prg.EpisodeTitle += $" ({prg.Part}/{prg.Parts})";
            }
        }

        private static void DetermineContentAdvisory(ref MxfProgram prg, sdProgram sd)
        {
            // fill content ratings and advisories; set flags
            var advisories = new HashSet<string>();
            if (sd.ContentRating != null)
            {
                var ratings = !string.IsNullOrEmpty(config.RatingsOrigin) ? config.RatingsOrigin.Split(',') : new[] { RegionInfo.CurrentRegion.ThreeLetterISORegionName };
                prg.ContentRatings = new Dictionary<string, string>();
                foreach (var rating in sd.ContentRating)
                {
                    if (string.IsNullOrEmpty(rating.Country) || Helper.TableContains(ratings, "ALL") || Helper.TableContains(ratings, rating.Country))
                    {
                        prg.ContentRatings.Add(rating.Body, rating.Code);
                    }

                    if (rating.ContentAdvisory == null) continue;
                    foreach (var reason in rating.ContentAdvisory)
                    {
                        advisories.Add(reason);
                    }
                }
            }
            if (sd.ContentAdvisory != null)
            {
                foreach (var reason in sd.ContentAdvisory)
                {
                    advisories.Add(reason);
                }
            }

            if (advisories.Count <= 0) return;
            var advisoryTable = advisories.ToArray();

            // set flags
            prg.HasAdult = Helper.TableContains(advisoryTable, "Adult Situations") || Helper.TableContains(advisoryTable, "Dialog");
            prg.HasBriefNudity = Helper.TableContains(advisoryTable, "Brief Nudity");
            prg.HasGraphicLanguage = Helper.TableContains(advisoryTable, "Graphic Language");
            prg.HasGraphicViolence = Helper.TableContains(advisoryTable, "Graphic Violence");
            prg.HasLanguage = Helper.TableContains(advisoryTable, "Adult Language") || Helper.TableContains(advisoryTable, "Language", true);
            prg.HasMildViolence = Helper.TableContains(advisoryTable, "Mild Violence");
            prg.HasNudity = Helper.TableContains(advisoryTable, "Nudity", true);
            prg.HasRape = Helper.TableContains(advisoryTable, "Rape");
            prg.HasStrongSexualContent = Helper.TableContains(advisoryTable, "Strong Sexual Content");
            prg.HasViolence = Helper.TableContains(advisoryTable, "Violence", true);

            prg.ContentAdvisories = advisoryTable;
        }

        private static void DetermineCastAndCrew(ref MxfProgram prg, sdProgram sd)
        {
            prg.ActorRole = GetPersons(sd.Cast, new[] { "Actor", "Voice", "Judge" });
            prg.DirectorRole = GetPersons(sd.Crew, new[] { "Director" });
            prg.GuestActorRole = GetPersons(sd.Cast, new[] { "Guest" }); // "Guest Star", "Guest"
            prg.HostRole = GetPersons(sd.Cast, new[] { "Anchor", "Host", "Presenter", "Narrator", "Correspondent" });
            prg.ProducerRole = GetPersons(sd.Crew, new[] { "Executive Producer" }); // "Producer", "Executive Producer", "Co-Executive Producer"
            prg.WriterRole = GetPersons(sd.Crew, new[] { "Writer", "Story" }); // "Screenwriter", "Writer", "Co-Writer"
        }

        private static List<MxfPersonRank> GetPersons(IList<sdProgramPerson> persons, string[] roles)
        {
            if (persons == null) return null;
            var personName = new List<string>();
            var ret = new List<MxfPersonRank>();
            foreach (var person in persons)
            {
                if (!roles.Any(role => person.Role.ToLower().Contains(role.ToLower()) && !personName.Contains(person.Name))) continue;
                ret.Add(new MxfPersonRank()
                {
                    Person = SdMxf.With[0].GetPersonId(person.Name),
                    Rank = int.Parse(person.BillingOrder),
                    Character = person.CharacterName
                });
                personName.Add(person.Name);
            }
            return ret;
        }

        private static int DecodeMpaaRating(IList<sdProgramContentRating> sdProgramContentRatings)
        {
            if (sdProgramContentRatings == null) return 0;
            var maxValue = 0;
            foreach (var rating in sdProgramContentRatings)
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
                }
            }
            return maxValue;
        }

        private static int DecodeStarRating(IList<sdProgramQualityRating> sdProgramQualityRatings)
        {
            if (sdProgramQualityRatings == null) return 0;

            var maxValue = (from rating in sdProgramQualityRatings where !string.IsNullOrEmpty(rating.MaxRating) let numerator = double.Parse(rating.Rating, CultureInfo.InvariantCulture) let denominator = double.Parse(rating.MaxRating, CultureInfo.InvariantCulture) select numerator / denominator).Concat(new[] {0.0}).Max();

            // return rounded number of half stars in a 4 star scale
            if (maxValue > 0.0) return (int)(8.0 * maxValue + 0.125);
            return 0;
        }
    }
}