using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using hdhr2mxf.HDHR;
using hdhr2mxf.MXF;

namespace hdhr2mxf
{
    public static class SliceMxf
    {
        public static bool BuildMxfFromSliceGuide(List<hdhrDiscover> homeruns)
        {
            // scan each device for tuned channels and associated programs
            foreach (var device in homeruns.Select(homerun => Common.Api.ConnectDevice(homerun.DiscoverUrl)))
            {
                if (device == null || string.IsNullOrEmpty(device.LineupUrl)) continue;
                Console.WriteLine($"Processing {device.FriendlyName} {device.ModelNumber} ({device.DeviceId}) with firmware {device.FirmwareVersion}.");

                // get channels
                var channels = Common.Api.GetDeviceChannels(device.LineupUrl);
                if (channels == null) continue;

                // determine lineup
                var model = device.ModelNumber.Split('-')[1];
                var deviceModel = model.Substring(model.Length - 2);
                var mxfLineup = Common.Mxf.With[0].GetLineup(deviceModel);

                foreach (var channel in channels)
                {
                    var startTime = 0;
                    var programs = Common.Api.GetChannelGuide(device.DeviceAuth, channel.GuideNumber, startTime);
                    if (programs == null) continue;

                    // build the service
                    var mxfService = Common.Mxf.With[0].GetService(programs[0].GuideName);
                    if (string.IsNullOrEmpty(mxfService.CallSign))
                    {
                        mxfService.CallSign = programs[0].GuideName;
                        mxfService.Name = channel.GuideName;
                        mxfService.Affiliate = (!string.IsNullOrEmpty(programs[0].Affiliate)) ? Common.Mxf.With[0].GetAffiliateId(programs[0].Affiliate) : null;
                        mxfService.LogoImage = (!Common.NoLogos && !string.IsNullOrEmpty(programs[0].ImageUrl)) ? Common.Mxf.With[0].GetGuideImage(programs[0].ImageUrl).Id : null;
                        mxfService.MxfScheduleEntries.Service = mxfService.Id;
                    }

                    // add channel to the lineup
                    if (int.TryParse(channel.GuideNumber, out _) || double.TryParse(channel.GuideNumber, out _))
                    {
                        var digits = channel.GuideNumber.Split('.');
                        var number = int.Parse(digits[0]);
                        var subnumber = 0;
                        if (digits.Length > 1)
                        {
                            subnumber = int.Parse(digits[1]);
                        }

                        // add the channel to the lineup and make sure we don't duplicate channels
                        var vchan = mxfLineup.Channels
                            .Where(arg => arg.Service == mxfService.Id)
                            .Where(arg => arg.Number == number)
                            .SingleOrDefault(arg => arg.SubNumber == subnumber);
                        if (vchan == null)
                        {
                            string matchname;
                            switch (deviceModel)
                            {
                                case "US":  // ATSC
                                    matchname = $"OC:{number}:{subnumber}";
                                    break;
                                case "DT":  // DVB-T
                                    matchname =
                                        $"DVBT:{channel.OriginalNetworkId}:{channel.TransportStreamId}:{channel.ProgramNumber}";
                                    break;
                                case "CC":  // US CableCARD
                                    matchname = mxfService.Name;
                                    break;
                                //case "IS":  // ISDB
                                //case "DC":  // DVB-C
                                default:
                                    matchname = mxfService.Name;
                                    break;
                            }

                            mxfLineup.Channels.Add(new MxfChannel()
                            {
                                Lineup = mxfLineup.Id,
                                LineupUid = mxfLineup.Uid,
                                StationId = mxfService.CallSign,
                                Service = mxfService.Id,
                                Number = number,
                                SubNumber = subnumber,
                                MatchName = matchname
                            });
                            Console.WriteLine($"--Processing station {mxfService.CallSign} on channel {number}{((subnumber > 0) ? "." + subnumber : string.Empty)}.");
                        }
                    }

                    // if this channel's listings are already done, stop here
                    if (!Common.ChannelsDone.Add(mxfService.CallSign)) continue;

                    // build the programs
                    do
                    {
                        foreach (var program in programs[0].Guide)
                        {
                            // establish the program ID
                            var programId = program.SeriesId;
                            if (!string.IsNullOrEmpty(program.EpisodeNumber))
                            {
                                programId += "_" + program.EpisodeNumber;
                            }
                            else if (!programId.StartsWith("MV"))
                            {
                                programId += "_" + program.GetHashCode();
                            }

                            // create an mxf program
                            var mxfProgram = new MxfProgram
                            {
                                Index = Common.Mxf.With[0].Programs.Count + 1, ProgramId = programId
                            };

                            // create the schedule entry and program if needed
                            var start = program.StartDateTime.ToString("yyyy-MM-ddTHH:mm:ss");
                            if (program.StartDateTime == mxfService.MxfScheduleEntries.EndTime)
                            {
                                start = null;
                            }
                            mxfService.MxfScheduleEntries.ScheduleEntry.Add(new MxfScheduleEntry()
                            {
                                Duration = program.EndTime - program.StartTime,
                                IsHdtv = channel.Hd ? "true" : null,
                                Program = Common.Mxf.With[0].GetProgram(programId, mxfProgram).Id,
                                StartTime = start
                            });

                            // build the program
                            BuildProgram(programId, program);

                            startTime = program.EndTime;
                        }
                    } while ((programs = Common.Api.GetChannelGuide(device.DeviceAuth, channel.GuideNumber, startTime)) != null);
                }
            }

            return true;
        }

        private static void BuildProgram(string id, hdhrProgram program)
        {
            var mxfProgram = Common.Mxf.With[0].GetProgram(id);
            if (!string.IsNullOrEmpty(mxfProgram.Title)) return;

            mxfProgram.Title = program.Title;
            mxfProgram.EpisodeTitle = program.EpisodeTitle;
            mxfProgram.Description = program.Synopsis;

            mxfProgram.IsAction = Common.ListContains(program.Filters, "Action");
            mxfProgram.IsComedy = Common.ListContains(program.Filters, "Comedy");
            mxfProgram.IsDocumentary = Common.ListContains(program.Filters, "Documentary");
            mxfProgram.IsDrama = Common.ListContains(program.Filters, "Drama");
            mxfProgram.IsEducational = Common.ListContains(program.Filters, "Educational");
            mxfProgram.IsHorror = Common.ListContains(program.Filters, "Horror");
            //mxfProgram.IsIndy = null;
            mxfProgram.IsKids = Common.ListContains(program.Filters, "Children") ?? Common.ListContains(program.Filters, "Kids") ?? Common.ListContains(program.Filters, "Family");
            mxfProgram.IsMusic = Common.ListContains(program.Filters, "Music");
            mxfProgram.IsNews = Common.ListContains(program.Filters, "News");
            mxfProgram.IsReality = Common.ListContains(program.Filters, "Reality");
            mxfProgram.IsRomance = Common.ListContains(program.Filters, "Romance");
            mxfProgram.IsScienceFiction = Common.ListContains(program.Filters, "Science Fiction");
            mxfProgram.IsSoap = Common.ListContains(program.Filters, "Soap");
            mxfProgram.IsThriller = Common.ListContains(program.Filters, "Suspense") ?? Common.ListContains(program.Filters, "Thriller");

            //mxfProgram.IsPremiere = ;
            //mxfProgram.IsSeasonFinale = ;
            //mxfProgram.IsSeasonPremiere = ;
            //mxfProgram.IsSeriesFinale = ;
            //mxfProgram.IsSeriesPremiere = ;

            //mxfProgram.IsLimitedSeries = null;
            mxfProgram.IsMiniseries = Common.ListContains(program.Filters, "Miniseries");
            mxfProgram.IsMovie = program.SeriesId.StartsWith("MV") ? "true" : Common.ListContains(program.Filters, "Movie");
            mxfProgram.IsPaidProgramming = Common.ListContains(program.Filters, "Paid Programming") ?? Common.ListContains(program.Filters, "Shopping") ?? Common.ListContains(program.Filters, "Consumer");
            //mxfProgram.IsProgramEpisodic = null;
            //mxfProgram.IsSerial = null;
            mxfProgram.IsSeries = Common.ListContains(program.Filters, "Series") ?? Common.ListContains(program.Filters, "Sports non-event");
            mxfProgram.IsShortFilm = Common.ListContains(program.Filters, "Short Film");
            mxfProgram.IsSpecial = Common.ListContains(program.Filters, "Special");
            mxfProgram.IsSports = Common.ListContains(program.Filters, "Sports event") ?? Common.ListContains(program.Filters, "Sports", true);
            if (string.IsNullOrEmpty(mxfProgram.IsSeries + mxfProgram.IsMiniseries + mxfProgram.IsMovie + mxfProgram.IsPaidProgramming + mxfProgram.IsShortFilm + mxfProgram.IsSpecial + mxfProgram.IsSports))
            {
                mxfProgram.IsSeries = "true";
            }

            // determine keywords
            Common.DetermineProgramKeywords(ref mxfProgram, program.Filters);

            if (!program.SeriesId.StartsWith("MV"))
            {
                // write the language
                mxfProgram.Language = program.SeriesId.Substring(program.SeriesId.Length - 6, 2).ToLower();

                // add the series
                var mxfSeriesInfo = Common.Mxf.With[0].GetSeriesInfo(program.SeriesId);
                mxfSeriesInfo.Title = program.Title;
                mxfProgram.Series = mxfSeriesInfo.Id;

                // add the image
                if (!string.IsNullOrEmpty(program.ImageUrl))
                {
                    mxfSeriesInfo.GuideImage = Common.Mxf.With[0].GetGuideImage(program.ImageUrl).Id;
                }

                // handle any season and episode numbers along with season entries
                if (!string.IsNullOrEmpty(program.EpisodeNumber))
                {
                    var se = program.EpisodeNumber.Substring(1).Split('E');
                    mxfProgram.SeasonNumber = int.Parse(se[0]).ToString();
                    mxfProgram.EpisodeNumber = int.Parse(se[1]).ToString();

                    mxfProgram.Season = Common.Mxf.With[0].GetSeasonId(program.SeriesId, mxfProgram.SeasonNumber);
                }
                else if (string.IsNullOrEmpty(mxfProgram.EpisodeTitle))
                {
                    mxfProgram.IsGeneric = "true";
                }

                if (string.IsNullOrEmpty(mxfProgram.IsGeneric))
                {
                    mxfProgram.OriginalAirdate = program.OriginalAirDateTime.ToString("yyyy-MM-dd");
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(program.PosterUrl))
                {
                    mxfProgram.GuideImage = Common.Mxf.With[0].GetGuideImage(program.PosterUrl).Id;
                }

                if (!string.IsNullOrEmpty(program.Synopsis))
                {
                    var m = Regex.Match(program.Synopsis, @"\d\.\d/\d\.\d");
                    {
                        var nums = m.Value.Split('/');
                        if (double.TryParse(nums[0], out var numerator) && double.TryParse(nums[1], out var denominator))
                        {
                            var halfstars = (int)((numerator / denominator) * 8);
                            mxfProgram.HalfStars = halfstars.ToString();
                            mxfProgram.Description = mxfProgram.Description.Replace($" ({m.Value})", "");
                        }
                    }
                }
            }
        }
    }
}
