using GaRyan2.PlutoTvAPI;
using GaRyan2.Utilities;
using GaRyan2.XmltvXml;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace plutotv
{
    internal class Program
    {
        private static readonly API api = new API() { BaseAddress = "https://api.pluto.tv/v2/" };
        
        static void Main(string[] args)
        {
            Logger.Initialize(Helper.Epg123TraceLogPath, "Beginning PlutoTV update execution", false);
            api.Initialize();
            Build();
        }

        private static void Build()
        {
            var startTime = DateTime.UtcNow;
            try
            {
                var channels = api.GetPlutoChannels()?.OrderBy(arg => arg.Number).ThenBy(arg => arg.Name).ToList();
                if (channels == null) return;

                WriteM3u(channels, Helper.PlutoTvM3uPath);
                WriteXmltv(channels, Helper.PlutoTvXmltvPath);
            }
            catch (Exception e)
            {
                Logger.WriteError($"Failed to create PlutoTV M3U/XMLTV files. {e}");
            }
            Logger.WriteVerbose($"PlutoTV update execution time was {DateTime.UtcNow - startTime}.");
        }

        private static void WriteM3u(List<PlutoChannel> channels, string path)
        {
            using (var writer = new StreamWriter(path))
            {
                writer.WriteLine("#EXTM3U");
                foreach (var channel in channels.Where(channel => channel.IsStitched))
                {
                    var url = channel.Stitched.Urls[0].Url
                        .Replace("deviceType=&", "deviceType=web&")
                        .Replace("deviceMake=&", "deviceMake=Chrome&")
                        .Replace("deviceModel=&", "deviceModel=Chrome&")
                        .Replace("appName=&", "appName=web&")
                        .Replace("sid=&", $"sid={Guid.NewGuid()}&");

                    writer.WriteLine($"#EXTINF:-1 tvg-id=\"{channel.ID}\" tvg-chno=\"{channel.Number}\" tvg-name=\"{channel.Name}\" tvg-logo=\"{channel.ColorLogoPNG.Path}\" group-title=\"{channel.Category}\",{channel.Name}");
                    writer.WriteLine(url);
                }
            }
            var fi = new FileInfo(path);
            Logger.WriteVerbose($"Completed save of the M3U file to \"{path}\". ({Helper.BytesToString(fi.Length)})");
        }

        private static void WriteXmltv(List<PlutoChannel> channels, string path)
        {
            var xmltv = new XMLTV() { GeneratorInfoName = "EPG123 PlutoTV", GeneratorInfoUrl = "https://garyan2.github.io/", SourceInfoName = "PlutoTV", SourceInfoUrl = "http://pluto.tv" };
            foreach (PlutoChannel channel in channels)
            {
                if (!channel.IsStitched) continue;

                xmltv.Channels.Add(new XmltvChannel()
                {
                    Id = channel.ID,
                    DisplayNames = new List<XmltvText>()
                    {
                        new XmltvText() { Text = channel.Name },
                        new XmltvText() { Text = $"{channel.Number} {channel.Name}" },
                        new XmltvText() { Text = $"{channel.Number}" }
                    },
                    Lcn = new List<XmltvText>()
                    {
                        new XmltvText() { Text = $"{channel.Number}" }
                    },
                    Icons = new List<XmltvIcon>()
                    {
                        new XmltvIcon() { Src = channel.ColorLogoPNG.Path }
                    }
                });

                foreach (PlutoTimeline timeline in channel.Timelines)
                {
                    // initialize a program
                    XmltvProgramme program = new XmltvProgramme()
                    {
                        Start = DateTime.Parse(timeline.Start.Trim('Z')).ToString("yyyyMMddHHmmss") + " +0000",
                        Stop = DateTime.Parse(timeline.Stop.Trim('Z')).ToString("yyyyMMddHHmmss") + " +0000",
                        Channel = channel.ID,
                    };

                    // determine if it is a movie or episode
                    bool movie = timeline.Episode.Series.Type.Equals("film") && !timeline.Episode.LiveBroadcast;

                    // determine
                    if (movie)
                    {
                        program.Titles = new List<XmltvText>()
                        {
                            new XmltvText() { Text = timeline.Title }
                        };

                        if (timeline.Episode.Clip?.OriginalReleaseDate != null)
                        {
                            program.Date = DateTime.Parse(timeline.Episode.Clip.OriginalReleaseDate.Trim('Z')).ToString("yyyy");
                        }

                        if (timeline.Episode.Poster != null)
                        {
                            program.Icons = new List<XmltvIcon>()
                            {
                                new XmltvIcon() { Src = timeline.Episode.Poster.Path }
                            };
                        }
                    }
                    else
                    {
                        program.Titles = new List<XmltvText>()
                        {
                            new XmltvText() { Text = timeline.Episode.Series.Name }
                        };
                        program.SubTitles = new List<XmltvText>()
                        {
                            new XmltvText() { Text = timeline.Episode.Name }
                        };

                        if (timeline.Episode.Clip?.OriginalReleaseDate != null)
                        {
                            program.Date = DateTime.Parse(timeline.Episode.Clip.OriginalReleaseDate.Trim('Z')).ToString("yyyyMMdd");
                        }

                        if (timeline.Episode.Series.Tile?.Path != null)
                        {
                            program.Icons = new List<XmltvIcon>()
                            {
                                new XmltvIcon() { Src = timeline.Episode.Series.Tile.Path }
                            };
                        }
                    }

                    program.Descriptions = new List<XmltvText>()
                    {
                        new XmltvText() { Text = timeline.Episode.Description }
                    };

                    program.Categories = new List<XmltvText>()
                    {
                        new XmltvText() { Text = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(timeline.Episode.Series.Type) },
                        new XmltvText() { Text = timeline.Episode.Genre },
                        new XmltvText() { Text = timeline.Episode.SubGenre }
                    };

                    if (timeline.Episode.LiveBroadcast) program.Live = string.Empty;

                    program.Rating = new List<XmltvRating>()
                    {
                        new XmltvRating() { Value = timeline.Episode.Rating }
                    };

                    xmltv.Programs.Add(program);
                }
            }

            Helper.WriteXmlFile(xmltv, path, true);
            var fi = new FileInfo(path);
            Logger.WriteInformation($"Completed save of the XMLTV file to \"{path}\". ({Helper.BytesToString(fi.Length)})");
            Logger.WriteVerbose($"Generated XMLTV file contains {xmltv.Channels.Count} channels and {xmltv.Programs.Count} programs.");
        }
    }
}