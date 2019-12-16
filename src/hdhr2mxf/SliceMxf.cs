using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using HDHomeRunTV;
using MxfXml;

namespace hdhr2mxf
{
    public static class SliceMxf
    {
        public static bool BuildMxfFromSliceGuide(List<HDHRDiscover> homeruns)
        {
            // scan each device for tuned channels and associated programs
            foreach (HDHRDiscover homerun in homeruns)
            {
                HDHRDevice device = Common.api.ConnectDevice(homerun.DiscoverURL);
                if (device == null || string.IsNullOrEmpty(device.LineupURL)) continue;
                else Console.WriteLine(string.Format("Processing {0} {1} ({2}) with firmware {3}.", device.FriendlyName, device.ModelNumber, device.DeviceID, device.FirmwareVersion));

                // get channels
                List<HDHRChannel> channels = Common.api.GetDeviceChannels(device.LineupURL);
                if (channels == null) continue;

                // determine lineup
                string model = device.ModelNumber.Split('-')[1];
                string deviceModel = model.Substring(model.Length - 2);
                MxfLineup mxfLineup = Common.mxf.With[0].getLineup(deviceModel);

                foreach (HDHRChannel channel in channels)
                {
                    int startTime = 0;
                    List<HDHRChannelGuide> programs = Common.api.GetChannelGuide(device.DeviceAuth, channel.GuideNumber, startTime);
                    if (programs == null) continue;

                    // build the service
                    MxfService mxfService = Common.mxf.With[0].getService(programs[0].GuideName);
                    if (string.IsNullOrEmpty(mxfService.CallSign))
                    {
                        mxfService.CallSign = programs[0].GuideName;
                        mxfService.Name = channel.GuideName;
                        mxfService.Affiliate = (!string.IsNullOrEmpty(programs[0].Affiliate)) ? Common.mxf.With[0].getAffiliateId(programs[0].Affiliate) : null;
                        mxfService.LogoImage = (!Common.noLogos && !string.IsNullOrEmpty(programs[0].ImageURL)) ? Common.mxf.With[0].getGuideImage(programs[0].ImageURL).Id : null;
                        mxfService.mxfScheduleEntries.Service = mxfService.Id;
                    }

                    // add channel to the lineup
                    if (int.TryParse(channel.GuideNumber, out int iChannel) || double.TryParse(channel.GuideNumber, out double dChannel))
                    {
                        string[] digits = channel.GuideNumber.Split('.');
                        int number = int.Parse(digits[0]);
                        int subnumber = 0;
                        if (digits.Length > 1)
                        {
                            subnumber = int.Parse(digits[1]);
                        }

                        // add the channel to the lineup and make sure we don't duplicate channels
                        var vchan = mxfLineup.channels.Where(arg => arg.Service == mxfService.Id)
                                                      .Where(arg => arg.Number == number)
                                                      .Where(arg => arg.SubNumber == subnumber)
                                                      .SingleOrDefault();
                        if (vchan == null)
                        {
                            string matchname = null;
                            switch (deviceModel)
                            {
                                case "US":  // ATSC
                                    matchname = string.Format("OC:{0}:{1}", number, subnumber);
                                    break;
                                case "DT":  // DVB-T
                                    matchname = string.Format("DVBT:{0}:{1}:{2}", channel.OriginalNetworkID, channel.TransportStreamID, channel.ProgramNumber);
                                    break;
                                case "CC":  // US CableCARD
                                    matchname = mxfService.Name;
                                    break;
                                case "IS":  // ISDB
                                case "DC":  // DVB-C
                                default:
                                    matchname = mxfService.Name;
                                    break;
                            }

                            mxfLineup.channels.Add(new MxfChannel()
                            {
                                Lineup = mxfLineup.Id,
                                lineupUid = mxfLineup.Uid,
                                stationId = mxfService.CallSign,
                                Service = mxfService.Id,
                                Number = number,
                                SubNumber = subnumber,
                                MatchName = matchname
                            });
                            Console.WriteLine(string.Format("--Processing station {0} on channel {1}{2}.",
                                mxfService.CallSign, number, (subnumber > 0) ? "." + subnumber.ToString() : string.Empty));
                        }
                    }

                    // if this channel's listings are already done, stop here
                    if (!Common.channelsDone.Add(mxfService.CallSign)) continue;

                    // build the programs
                    do
                    {
                        foreach (HDHRProgram program in programs[0].Guide)
                        {
                            // establish the program ID
                            string programID = program.SeriesID;
                            if (!string.IsNullOrEmpty(program.EpisodeNumber))
                            {
                                programID += "_" + program.EpisodeNumber;
                            }
                            else if (!programID.StartsWith("MV"))
                            {
                                programID += "_" + program.GetHashCode().ToString();
                            }

                            // create an mxf program
                            MxfProgram mxfProgram = new MxfProgram()
                            {
                                index = Common.mxf.With[0].Programs.Count + 1
                            };
                            mxfProgram.programId = programID;

                            // create the schedule entry and program if needed
                            string start = program.StartDateTime.ToString("yyyy-MM-ddTHH:mm:ss");
                            if (program.StartDateTime == mxfService.mxfScheduleEntries.endTime)
                            {
                                start = null;
                            }
                            mxfService.mxfScheduleEntries.ScheduleEntry.Add(new MxfScheduleEntry()
                            {
                                Duration = program.EndTime - program.StartTime,
                                IsHdtv = channel.HD ? "true" : null,
                                Program = Common.mxf.With[0].getProgram(programID, mxfProgram).Id,
                                StartTime = start
                            });

                            // build the program
                            BuildProgram(programID, program);

                            startTime = program.EndTime;
                        }
                    } while ((programs = Common.api.GetChannelGuide(device.DeviceAuth, channel.GuideNumber, startTime)) != null);
                }
            }
            return true;
        }

        private static void BuildProgram(string id, HDHRProgram program)
        {
            MxfProgram mxfProgram = Common.mxf.With[0].getProgram(id);
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
            mxfProgram.IsMovie = program.SeriesID.StartsWith("MV") ? "true" : Common.ListContains(program.Filters, "Movie");
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
            Common.determineProgramKeywords(ref mxfProgram, program.Filters);

            if (!program.SeriesID.StartsWith("MV"))
            {
                // write the language
                mxfProgram.Language = program.SeriesID.Substring(program.SeriesID.Length - 6, 2).ToLower();

                // add the series
                MxfSeriesInfo mxfSeriesInfo = Common.mxf.With[0].getSeriesInfo(program.SeriesID);
                mxfSeriesInfo.Title = program.Title;
                mxfProgram.Series = mxfSeriesInfo.Id;

                // add the image
                if (!string.IsNullOrEmpty(program.ImageURL))
                {
                    mxfSeriesInfo.GuideImage = Common.mxf.With[0].getGuideImage(program.ImageURL).Id;
                }

                // handle any season and episode numbers along with season entries
                if (!string.IsNullOrEmpty(program.EpisodeNumber))
                {
                    string[] se = program.EpisodeNumber.Substring(1).Split('E');
                    mxfProgram.SeasonNumber = int.Parse(se[0]).ToString();
                    mxfProgram.EpisodeNumber = int.Parse(se[1]).ToString();

                    mxfProgram.Season = Common.mxf.With[0].getSeasonId(program.SeriesID, mxfProgram.SeasonNumber);
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
                if (!string.IsNullOrEmpty(program.PosterURL))
                {
                    mxfProgram.GuideImage = Common.mxf.With[0].getGuideImage(program.PosterURL).Id;
                }

                Match m = Regex.Match(program.Synopsis, @"\d\.\d/\d\.\d");
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
        }
    }
}
