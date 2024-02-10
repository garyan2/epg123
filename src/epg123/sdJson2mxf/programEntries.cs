using GaRyan2.MxfXml;
using GaRyan2.SchedulesDirectAPI;
using GaRyan2.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using api = GaRyan2.SchedulesDirect;

namespace epg123.sdJson2mxf
{
    internal static partial class sdJson2Mxf
    {
        private static List<string> programQueue;
        private static ConcurrentBag<Programme> programResponses;

        private static bool BuildAllProgramEntries()
        {
            // reset counters
            programQueue = new List<string>();
            programResponses = new ConcurrentBag<Programme>();
            sportsSeries = new System.Collections.Specialized.NameValueCollection();
            //sportsEvents = new List<MxfProgram>();
            IncrementNextStage(mxf.ProgramsToProcess.Count);
            Logger.WriteMessage($"Entering BuildAllProgramEntries() for {totalObjects} programs.");

            // fill mxf programs with cached values and queue the rest
            programQueue = new List<string>();
            foreach (var mxfProgram in mxf.With.Programs)
            {
                if (epgCache.JsonFiles.ContainsKey(mxfProgram.extras["md5"]))
                {
                    try
                    {
                        using (var reader = new StringReader(epgCache.GetAsset(mxfProgram.extras["md5"])))
                        {
                            var serializer = new JsonSerializer();
                            var sdProgram = (Programme)serializer.Deserialize(reader, typeof(Programme)) ?? throw new Exception();
                            BuildMxfProgram(mxfProgram, sdProgram);
                        }
                        IncrementProgress();
                    }
                    catch
                    {
                        programQueue.Add(mxfProgram.ProgramId);
                    }
                }
                else
                {
                    programQueue.Add(mxfProgram.ProgramId);
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
                    Logger.WriteWarning($"Failed to download and process {mxf.ProgramsToProcess.Count - processedObjects} program descriptions.");
                }
            }
            Logger.WriteMessage("Exiting BuildAllProgramEntries(). SUCCESS.");
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
            var responses = api.GetPrograms(programs);
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
            foreach (var sdProgram in programResponses)
            {
                IncrementProgress();

                // determine which program this belongs to
                var mxfProgram = mxf.FindOrCreateProgram(sdProgram.ProgramId);

                // build a standalone program
                BuildMxfProgram(mxfProgram, sdProgram);

                // add JSON to cache
                if (sdProgram.Md5 != null)
                {
                    mxfProgram.extras["md5"] = sdProgram.Md5;
                    using (var writer = new StringWriter())
                    {
                        try
                        {
                            var serializer = new JsonSerializer();
                            serializer.Serialize(writer, sdProgram);
                            epgCache.AddAsset(sdProgram.Md5, writer.ToString());
                        }
                        catch
                        {
                            // ignored
                        }
                    }
                }
                else
                {
                    Logger.WriteInformation($"Did not cache program {mxfProgram.ProgramId} due to missing Md5 hash.");
                }
            }
        }

        private static void AddModernMediaUiPlusProgram(Programme sdProgram)
        {
            // create entry in ModernMedia UI+ dictionary
            ModernMediaUiPlus.Programs.Add(sdProgram.ProgramId, new ModernMediaUiPlusPrograms
            {
                ContentRating = sdProgram.ContentRating,
                EventDetails = sdProgram.EventDetails,
                Movie = sdProgram.Movie,
                OriginalAirDate = !string.IsNullOrEmpty(sdProgram.ShowType) && sdProgram.ShowType.ToLower().Contains("series") ? sdProgram.OriginalAirDate.ToString("s") : null,
                ShowType = sdProgram.ShowType
            });
        }

        private static void BuildMxfProgram(MxfProgram mxfProgram, Programme sdProgram)
        {
            // set program flags
            SetProgramFlags(mxfProgram, sdProgram);

            // populate title, short title, description, and short description
            DetermineTitlesAndDescriptions(mxfProgram, sdProgram);

            // populate program keywords
            DetermineProgramKeywords(mxfProgram, sdProgram);

            // determine movie or series information
            if (mxfProgram.IsMovie)
            {
                DetermineMovieInfo(mxfProgram, sdProgram);
            }
            else
            {
                DetermineSeriesInfo(mxfProgram, sdProgram);
                DetermineEpisodeInfo(mxfProgram, sdProgram);
                CompleteEpisodeTitle(mxfProgram);
            }

            // set content reason flags
            DetermineContentAdvisory(mxfProgram, sdProgram);

            // populate the cast and crew
            DetermineCastAndCrew(mxfProgram, sdProgram);

            // populate stuff for xmltv
            if (config.CreateXmltv)
            {
                if (sdProgram.Genres != null && sdProgram.Genres.Length > 0) mxfProgram.extras.Add("genres", sdProgram.Genres.Clone());
                if (sdProgram.EventDetails?.Teams != null)
                {
                    mxfProgram.extras.Add("teams", sdProgram.EventDetails.Teams.Select(team => team.Name).ToList());
                }
            }

            // add program to array for ModernMedia UI+
            if (config.ModernMediaUiPlusSupport)
            {
                AddModernMediaUiPlusProgram(sdProgram);
            }
        }

        private static void DetermineTitlesAndDescriptions(MxfProgram mxfProgram, Programme sdProgram)
        {
            // populate titles
            if (sdProgram.Titles != null)
            {
                mxfProgram.Title = sdProgram.Titles[0].Title120;
            }
            else
            {
                Logger.WriteWarning($"Program {sdProgram.ProgramId} is missing required content.");
            }
            mxfProgram.EpisodeTitle = sdProgram.EpisodeTitle150;

            // populate descriptions and language
            if (sdProgram.Descriptions != null)
            {
                mxfProgram.ShortDescription = GetDescriptions(sdProgram.Descriptions.Description100, out var lang);
                mxfProgram.Description = GetDescriptions(sdProgram.Descriptions.Description1000, out lang);

                // if short description is empty, not a movie, and append episode option is enabled
                // copy long description into short description
                if (config.AppendEpisodeDesc && !mxfProgram.IsMovie && string.IsNullOrEmpty(mxfProgram.ShortDescription))
                {
                    mxfProgram.ShortDescription = mxfProgram.Description;
                }

                // populate language
                if (!string.IsNullOrEmpty(lang))
                {
                    mxfProgram.Language = lang.ToLower();
                }
            }

            mxfProgram.OriginalAirdate = sdProgram.OriginalAirDate.ToString();
        }

        private static string GetDescriptions(List<ProgramDescription> descriptions, out string language)
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

        private static void SetProgramFlags(MxfProgram prg, Programme sd)
        {
            var types = new[] { sd.EntityType, sd.ShowType };

            // transfer genres to mxf program
            prg.IsAction = Helper.TableContains(sd.Genres, "Action") || Helper.TableContains(sd.Genres, "Adventure");
            prg.IsAdultOnly = Helper.TableContains(sd.Genres, "Adults Only");
            prg.IsComedy = Helper.TableContains(sd.Genres, "Comedy");
            prg.IsDocumentary = Helper.TableContains(sd.Genres, "Documentary");
            prg.IsDrama = Helper.TableContains(sd.Genres, "Drama");
            prg.IsEducational = Helper.TableContains(sd.Genres, "Educational");
            prg.IsHorror = Helper.TableContains(sd.Genres, "Horror");
            prg.IsIndy = Helper.TableContains(sd.Genres, "Independent") || Helper.TableContains(sd.Genres, "Indy");
            prg.IsKids = Helper.TableContains(sd.Genres, "Children") || Helper.TableContains(sd.Genres, "Kids");
            prg.IsMusic = Helper.TableContains(sd.Genres, "Music") || Helper.TableContains(types, "Music");
            prg.IsNews = Helper.TableContains(sd.Genres, "News");
            prg.IsReality = Helper.TableContains(sd.Genres, "Reality");
            prg.IsRomance = Helper.TableContains(sd.Genres, "Romance") || Helper.TableContains(sd.Genres, "Romantic");
            prg.IsScienceFiction = Helper.TableContains(sd.Genres, "Science Fiction");
            prg.IsSoap = Helper.TableContains(sd.Genres, "Soap");
            prg.IsThriller = Helper.TableContains(sd.Genres, "Suspense") || Helper.TableContains(sd.Genres, "Thriller");

            // below flags are populated when creating the program in ProcessMd5ScheduleEntry(string md5)
            // prg.IsSeasonFinale
            // prg.IsSeasonPremiere
            // prg.IsSeriesFinale
            // prg.IsSeriesPremiere

            // transfer show types to mxf program
            //prg.IsLimitedSeries = null;
            prg.IsMiniseries = Helper.TableContains(types, "Miniseries");
            prg.IsMovie = sd.ProgramId.StartsWith("MV") || Helper.TableContains(types, "Movie");
            prg.IsPaidProgramming = Helper.TableContains(types, "Paid Program");
            //prg.IsProgramEpisodic = null;
            //prg.IsSerial = null;
            prg.IsSeries = Helper.TableContains(types, "Series") && !Helper.TableContains(sd.Genres, "Sports talk");
            prg.IsShortFilm = Helper.TableContains(types, "Short Film");
            prg.IsSpecial = Helper.TableContains(types, "Special");
            prg.IsSports = sd.ProgramId.StartsWith("SP") ||
                           Helper.TableContains(types, "Event") ||
                           Helper.TableContains(sd.Genres, "Sports talk");

            // set isGeneric flag if programID starts with "SH", is a series, is not a miniseries, and is not paid programming
            if (prg.ProgramId.StartsWith("SH") && (prg.IsSports && !Helper.TableContains(types, "Sports") ||
                                                   prg.IsSeries && !prg.IsMiniseries && !prg.IsPaidProgramming))
            {
                prg.IsGeneric = true;
            }

            // queue up the sport event to get the event image
            if (Helper.TableContains(types, "Event"))// && (sd.HasSportsArtwork | sd.HasEpisodeArtwork | sd.HasSeriesArtwork | sd.HasImageArtwork))
            {
                sportEvents.Add(prg);
            }
        }

        private static void DetermineProgramKeywords(MxfProgram mxfProgram, Programme sdProgram)
        {
            // determine primary group of program
            var group = MXF.KeywordGroups.UNKNOWN;
            if (mxfProgram.IsMovie) group = MXF.KeywordGroups.MOVIES;
            else if (mxfProgram.IsPaidProgramming) group = MXF.KeywordGroups.PAIDPROGRAMMING;
            else if (mxfProgram.IsSports) group = MXF.KeywordGroups.SPORTS;
            else if (mxfProgram.IsKids) group = MXF.KeywordGroups.KIDS;
            else if (mxfProgram.IsEducational) group = MXF.KeywordGroups.EDUCATIONAL;
            else if (mxfProgram.IsNews) group = MXF.KeywordGroups.NEWS;
            else if (mxfProgram.IsMusic) group = MXF.KeywordGroups.MUSIC;
            else if (mxfProgram.IsSpecial) group = MXF.KeywordGroups.SPECIAL;
            else if (mxfProgram.IsReality) group = MXF.KeywordGroups.REALITY;
            else if (mxfProgram.IsSeries) group = MXF.KeywordGroups.SERIES;

            // build the keywords/categories
            if (group == MXF.KeywordGroups.UNKNOWN) return;
            var mxfKeyGroup = mxf.FindOrCreateKeywordGroup(group);
            mxfProgram.mxfKeywords.Add(new MxfKeyword((int)group, mxfKeyGroup.Index, mxf.KeywordGroupsText[(int)group]));

            // add premiere categories as necessary
            if (mxfProgram.IsSeasonPremiere || mxfProgram.IsSeriesPremiere)
            {
                var premiere = mxf.FindOrCreateKeywordGroup(MXF.KeywordGroups.PREMIERES);
                mxfProgram.mxfKeywords.Add(new MxfKeyword(premiere.Index, premiere.Index, mxf.KeywordGroupsText[(int)MXF.KeywordGroups.PREMIERES]));
                if (mxfProgram.IsSeriesPremiere) mxfProgram.mxfKeywords.Add(premiere.FindOrCreateKeyword("Series Premiere"));
                else if (mxfProgram.IsSeasonPremiere) mxfProgram.mxfKeywords.Add(premiere.FindOrCreateKeyword("Season Premiere"));
            }
            else if (mxfProgram.extras["premiere"])
            {
                if (group == MXF.KeywordGroups.MOVIES)
                {
                    mxfProgram.mxfKeywords.Add(mxfKeyGroup.FindOrCreateKeyword("Premiere"));
                }
                else if (Helper.TableContains(sdProgram.Genres, "miniseries"))
                {
                    var premiere = mxf.FindOrCreateKeywordGroup(MXF.KeywordGroups.PREMIERES);
                    mxfProgram.mxfKeywords.Add(new MxfKeyword(premiere.Index, premiere.Index, mxf.KeywordGroupsText[(int)MXF.KeywordGroups.PREMIERES]));
                    mxfProgram.mxfKeywords.Add(premiere.FindOrCreateKeyword("Miniseries Premiere"));
                }
            }

            // now add the real categories
            if (sdProgram.Genres != null)
            {
                foreach (var genre in sdProgram.Genres)
                {
                    if (genre == mxf.KeywordGroupsText[(int)group]) continue;
                    mxfProgram.mxfKeywords.Add(mxfKeyGroup.FindOrCreateKeyword(genre));
                }
            }

            // ensure there is at least 1 category to present in category search
            if (mxfProgram.mxfKeywords.Count > 1) return;
            mxfProgram.mxfKeywords.Add(mxfKeyGroup.FindOrCreateKeyword("Uncategorized"));
        }

        private static void DetermineMovieInfo(MxfProgram mxfProgram, Programme sdProgram)
        {
            // fill MPAA rating
            mxfProgram.MpaaRating = DecodeMpaaRating(sdProgram.ContentRating);

            // populate movie specific attributes
            if (sdProgram.Movie != null)
            {
                mxfProgram.Year = sdProgram.Movie.Year;
                mxfProgram.HalfStars = DecodeStarRating(sdProgram.Movie.QualityRating);
            }
            else if (!string.IsNullOrEmpty(mxfProgram.OriginalAirdate))
            {
                mxfProgram.Year = int.Parse(mxfProgram.OriginalAirdate.Substring(0, 4));
            }
        }

        private static void DetermineSeriesInfo(MxfProgram mxfProgram, Programme sdProgram)
        {
            // for sports programs that start with "SP", create a series entry based on program title
            // this groups them all together as a series for recordings
            MxfSeriesInfo mxfSeriesInfo;
            if (mxfProgram.ProgramId.StartsWith("SP"))
            {
                var name = mxfProgram.Title.Replace(' ', '_');
                mxfSeriesInfo = mxf.FindOrCreateSeriesInfo(name);
                sportsSeries.Add(name, mxfProgram.ProgramId);
            }
            else
            {
                // create a seriesInfo entry if needed
                mxfSeriesInfo = mxf.FindOrCreateSeriesInfo(mxfProgram.ProgramId.Substring(2, 8), mxfProgram.ProgramId);
                if (mxfProgram.ProgramId.StartsWith("SH"))
                {
                    // go ahead and create/update the cache entry as needed
                    if (epgCache.JsonFiles.ContainsKey(mxfProgram.ProgramId))
                    {
                        try
                        {
                            using (var reader = new StringReader(epgCache.GetAsset(mxfProgram.ProgramId)))
                            {
                                var serializer = new JsonSerializer();
                                var cached = (GenericDescription)serializer.Deserialize(reader, typeof(GenericDescription));
                                if (cached.StartAirdate == null)
                                {
                                    cached.StartAirdate = mxfProgram.OriginalAirdate ?? string.Empty;
                                    using (var writer = new StringWriter())
                                    {
                                        serializer.Serialize(writer, cached);
                                        epgCache.UpdateAssetJsonEntry(mxfProgram.ProgramId, writer.ToString());
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
                        var newEntry = new GenericDescription
                        {
                            Code = 0,
                            Description1000 = mxfProgram.IsGeneric ? mxfProgram.Description : null,
                            Description100 = mxfProgram.IsGeneric ? mxfProgram.ShortDescription : null,
                            StartAirdate = mxfProgram.OriginalAirdate ?? string.Empty
                        };

                        using (var writer = new StringWriter())
                        {
                            var serializer = new JsonSerializer();
                            serializer.Serialize(writer, newEntry);
                            epgCache.AddAsset(mxfProgram.ProgramId, writer.ToString());
                        }
                    }
                }
            }

            mxfSeriesInfo.Title = mxfSeriesInfo.Title ?? mxfProgram.Title;
            mxfProgram.mxfSeriesInfo = mxfSeriesInfo;
        }

        private static void DetermineEpisodeInfo(MxfProgram mxfProgram, Programme sdProgram)
        {
            if (sdProgram.EntityType != "Episode") return;

            // use the last 4 numbers as a production number
            mxfProgram.EpisodeNumber = int.Parse(mxfProgram.ProgramId.Substring(10));

            if (sdProgram.Metadata != null)
            {
                // grab season and episode numbers if available
                foreach (var providers in sdProgram.Metadata)
                {
                    ProgramMetadataProvider provider = null;
                    if (config.TheTvdbNumbers)
                    {
                        if (providers.TryGetValue("TheTVDB", out provider) || providers.TryGetValue("TVmaze", out provider))
                        {
                            if (provider.SeasonNumber == 0 && provider.EpisodeNumber == 0) continue;
                        }
                    }
                    if (provider == null && !providers.TryGetValue("Gracenote", out provider)) continue;

                    mxfProgram.SeasonNumber = provider.SeasonNumber;
                    mxfProgram.EpisodeNumber = provider.EpisodeNumber;
                }
            }

            // if there is a season number, create a season entry
            if (mxfProgram.SeasonNumber != 0)
            {
                mxfProgram.mxfSeason = mxf.FindOrCreateSeason(mxfProgram.mxfSeriesInfo.SeriesId, mxfProgram.SeasonNumber,
                    sdProgram.HasSeasonArtwork ? mxfProgram.ProgramId : null);

                if (config.AppendEpisodeDesc || config.PrefixEpisodeDescription || config.PrefixEpisodeTitle)
                {
                    mxfProgram.mxfSeason.HideSeasonTitle = true;
                }
            }
        }

        private static void CompleteEpisodeTitle(MxfProgram mxfProgram)
        {
            // by request, if there is no episode title, and the program is not generic, duplicate the program title in the episode title
            if (mxfProgram.ProgramId.StartsWith("EP") && string.IsNullOrEmpty(mxfProgram.EpisodeTitle))
            {
                mxfProgram.EpisodeTitle = mxfProgram.Title;
            }
            else if (string.IsNullOrEmpty(mxfProgram.EpisodeTitle)) return;

            var se = config.AlternateSEFormat ? "S{0}:E{1} " : "s{0:D2}e{1:D2} ";
            if (mxfProgram.SeasonNumber != 0)
            {
                se = string.Format(se, mxfProgram.SeasonNumber, mxfProgram.EpisodeNumber);
            }
            else if (mxfProgram.EpisodeNumber != 0)
            {
                se = $"#{mxfProgram.EpisodeNumber} ";
            }
            else se = string.Empty;

            // prefix episode title with season and episode numbers as configured
            if (config.PrefixEpisodeTitle)
            {
                mxfProgram.EpisodeTitle = se + mxfProgram.EpisodeTitle;
            }

            // prefix episode description with season and episode numbers as configured
            if (config.PrefixEpisodeDescription)
            {
                mxfProgram.Description = se + mxfProgram.Description;
                if (!string.IsNullOrEmpty(mxfProgram.ShortDescription))
                {
                    mxfProgram.ShortDescription = se + mxfProgram.ShortDescription;
                }
            }

            // append season and episode numbers to the program description as configured
            if (config.AppendEpisodeDesc)
            {
                // add space before appending season and episode numbers in case there is no short description
                if (mxfProgram.SeasonNumber != 0 && mxfProgram.EpisodeNumber != 0)
                {
                    mxfProgram.Description += $" \u000D\u000ASeason {mxfProgram.SeasonNumber}, Episode {mxfProgram.EpisodeNumber}";
                }
                else if (mxfProgram.EpisodeNumber != 0)
                {
                    mxfProgram.Description += $" \u000D\u000AProduction #{mxfProgram.EpisodeNumber}";
                }
            }

            // append part/parts to episode title as needed
            if (mxfProgram.extras.ContainsKey("multipart"))
            {
                mxfProgram.EpisodeTitle += $" ({mxfProgram.extras["multipart"]})";
            }
        }

        private static void DetermineContentAdvisory(MxfProgram mxfProgram, Programme sdProgram)
        {
            // fill content ratings and advisories; set flags
            var advisories = new HashSet<string>();
            if (sdProgram.ContentRating != null)
            {
                var origins = !string.IsNullOrEmpty(config.RatingsOrigin) ? config.RatingsOrigin.Split(',') : new[] { RegionInfo.CurrentRegion.ThreeLetterISORegionName };
                var contentRatings = new Dictionary<string, string>();
                if (Helper.TableContains(origins, "ALL"))
                {
                    foreach (var rating in sdProgram.ContentRating)
                    {
                        contentRatings.Add(rating.Body, rating.Code);
                    }
                }
                else
                {
                    foreach (var origin in origins)
                    {
                        foreach (var rating in sdProgram.ContentRating.Where(arg => arg.Country?.Equals(origin) ?? false))
                        {
                            contentRatings.Add(rating.Body, rating.Code);
                        }
                        if (contentRatings.Count > 0) break;
                    }
                }
                mxfProgram.extras.Add("ratings", contentRatings);
            }

            if (sdProgram.ContentAdvisory != null)
            {
                foreach (var reason in sdProgram.ContentAdvisory)
                {
                    advisories.Add(reason);
                }
            }
            if (advisories.Count == 0) return;
            var advisoryTable = advisories.ToArray();

            // set flags
            mxfProgram.HasAdult = Helper.TableContains(advisoryTable, "Adult Situations") || Helper.TableContains(advisoryTable, "Dialog");
            mxfProgram.HasBriefNudity = Helper.TableContains(advisoryTable, "Brief Nudity");
            mxfProgram.HasGraphicLanguage = Helper.TableContains(advisoryTable, "Graphic Language");
            mxfProgram.HasGraphicViolence = Helper.TableContains(advisoryTable, "Graphic Violence");
            mxfProgram.HasLanguage = Helper.TableContains(advisoryTable, "Adult Language") || Helper.TableContains(advisoryTable, "Language", true);
            mxfProgram.HasMildViolence = Helper.TableContains(advisoryTable, "Mild Violence");
            mxfProgram.HasNudity = Helper.TableContains(advisoryTable, "Nudity", true);
            mxfProgram.HasRape = Helper.TableContains(advisoryTable, "Rape");
            mxfProgram.HasStrongSexualContent = Helper.TableContains(advisoryTable, "Strong Sexual Content");
            mxfProgram.HasViolence = Helper.TableContains(advisoryTable, "Violence", true);
        }

        private static void DetermineCastAndCrew(MxfProgram prg, Programme sd)
        {
            if (config.ExcludeCastAndCrew) return;
            prg.ActorRole = GetPersons(sd.Cast, new[] { "Actor", "Voice", "Judge", "Self" });
            prg.DirectorRole = GetPersons(sd.Crew, new[] { "Director" });
            prg.GuestActorRole = GetPersons(sd.Cast, new[] { "Guest" }); // "Guest Star", "Guest"
            prg.HostRole = GetPersons(sd.Cast, new[] { "Anchor", "Host", "Presenter", "Narrator", "Correspondent" });
            prg.ProducerRole = GetPersons(sd.Crew, new[] { "Executive Producer" }); // "Producer", "Executive Producer", "Co-Executive Producer"
            prg.WriterRole = GetPersons(sd.Crew, new[] { "Writer", "Story" }); // "Screenwriter", "Writer", "Co-Writer"
        }

        private static List<MxfPersonRank> GetPersons(List<ProgramPerson> persons, string[] roles)
        {
            if (persons == null) return null;
            var personName = new List<string>();
            var ret = new List<MxfPersonRank>();
            foreach (var person in persons.Where(person => roles.Any(role => person.Role.ToLower().Contains(role.ToLower()) && !personName.Contains(person.Name))))
            {
                ret.Add(new MxfPersonRank(mxf.FindOrCreatePerson(person.Name))
                {
                    Rank = int.Parse(person.BillingOrder),
                    Character = person.CharacterName
                });
                personName.Add(person.Name);
            }
            return ret;
        }

        private static int DecodeMpaaRating(List<ProgramContentRating> sdProgramContentRatings)
        {
            if (sdProgramContentRatings == null) return 0;
            var maxValue = 0;
            foreach (var rating in sdProgramContentRatings.Where(rating => rating.Body.ToLower().StartsWith("motion picture association")))
            {
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

        private static int DecodeStarRating(List<ProgramQualityRating> sdProgramQualityRatings)
        {
            if (sdProgramQualityRatings == null) return 0;

            var maxValue = (from rating in sdProgramQualityRatings where !string.IsNullOrEmpty(rating.MaxRating) let numerator = double.Parse(rating.Rating, CultureInfo.InvariantCulture) let denominator = double.Parse(rating.MaxRating, CultureInfo.InvariantCulture) select numerator / denominator).Concat(new[] { 0.0 }).Max();

            // return rounded number of half stars in a 4 star scale
            if (maxValue > 0.0) return (int)(8.0 * maxValue + 0.125);
            return 0;
        }
    }
}