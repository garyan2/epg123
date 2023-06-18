using GaRyan2.StirrTvApi;
using GaRyan2.Utilities;
using GaRyan2.XmltvXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace stirrtv
{
    internal class Program
    {
        private class MyStirrClass
        {
            public StirrChannelStatus Status { get; set; }
            public StirrChannelGuide Guide { get; set; }
        }
        private static readonly API api = new API { BaseAddress = "https://ott-gateway-stirr.sinclairstoryline.com/api/rest/v3/" };
        private static Dictionary<string, MyStirrClass> _channels = new Dictionary<string, MyStirrClass>();

        static void Main(string[] args)
        {
            try
            {
                Logger.Initialize(Helper.Epg123TraceLogPath);
                api.Initialize();
                Build();
            }
            catch (Exception e)
            {
                Logger.WriteError($"Failed to create Stirr M3U/XMLTV files. {e}");
            }
        }

        private static void Build()
        {
            var startTime = DateTime.UtcNow;
            Logger.WriteMessage("===============================================================================");
            Logger.WriteMessage($" Beginning StirrTV update execution. version {Helper.Epg123Version}");
            Logger.WriteMessage("===============================================================================");

            // determine lineup channels and groups
            Logger.WriteInformation("Determining Stirr TV lineup...");
            var lineup = api.GetStirrLineup();
            foreach (var channel in lineup.Channels)
            {
                _channels.Add(channel.ID, new MyStirrClass());
            }

            if (_channels.Count > 0)
            {
                Logger.WriteInformation($"Downloading guide listings for {_channels.Count} channels...");
                Parallel.For(0, _channels.Count, new ParallelOptions { MaxDegreeOfParallelism = 4 }, i =>
                {
                    DownloadChannel(_channels.ElementAt(i).Key);
                });
            }
            else
            {
                Logger.WriteError("No channels to download. Exiting.");
                return;
            }

            // initialize xmltv file
            var xmltv = new XMLTV() { GeneratorInfoName = "EPG123 Stirr TV", GeneratorInfoUrl = "https://garyan2.github.io/", SourceInfoName = "Stirr TV", SourceInfoUrl = "https://stirr.com" };

            // start m3u file
            using (var m3uWrite = new StreamWriter(Helper.StirrTvM3uPath))
            {
                m3uWrite.WriteLine("#EXTM3U");
                foreach (var channel in lineup.Channels)
                {
                    var status = _channels[channel.ID].Status;
                    var group = lineup.Categories.FirstOrDefault(arg => arg.UUID.Equals(channel.Categories[0]?.UUID));
                    m3uWrite.WriteLine($"#EXTINF:-1 tvg-id=\"{channel.ID}\" tvg-chno=\"{channel.ChannelNumber}\" tvg-name=\"{status.ChannelRss.Channel.Title.Trim()}\" tvg-logo=\"{channel.Icon.Source}\" group-title=\"{group?.Name}\",{status.ChannelRss.Channel.Title.Trim()}");
                    m3uWrite.WriteLine($"{status.ChannelRss.Channel.Item.StreamUrl}");

                    xmltv.Channels.Add(new XmltvChannel
                    {
                        Id = channel.ID,
                        DisplayNames = new List<XmltvText> { new XmltvText { Text = status.ChannelRss.Channel.Title.Trim() }, new XmltvText { Text = $"{channel.ChannelNumber} {status.ChannelRss.Channel.Title.Trim()}" }, new XmltvText { Text = channel.ChannelNumber } },
                        Lcn = new List<XmltvText> { new XmltvText { Text = channel.ChannelNumber } },
                        Icons = new List<XmltvIcon> { new XmltvIcon { Src = channel.Icon.Source } }
                    });

                    foreach (var program in _channels[channel.ID].Guide.Programs)
                    {
                        xmltv.Programs.Add(new XmltvProgramme
                        {
                            Channel = channel.ID,
                            Start = $"{program.Start} +0000",
                            Stop = $"{program.End} +0000",
                            Titles = new List<XmltvText> { new XmltvText { Text = program.Title.Value } },
                            Descriptions = new List<XmltvText> { new XmltvText { Text = program.Description.Value } }
                        });
                    }
                }
                m3uWrite.Flush();
            }

            if (File.Exists(Helper.StirrTvM3uPath))
            {
                var fi = new FileInfo(Helper.StirrTvM3uPath);
                Logger.WriteInformation($"Completed save of the M3U file to \"{Helper.StirrTvM3uPath}\". ({Helper.BytesToString(fi.Length)})");
            }

            if (xmltv != null)
            {
                Helper.WriteXmlFile(xmltv, Helper.StirrTvXmltvPath, true);
                var fi = new FileInfo(Helper.StirrTvXmltvPath);
                Logger.WriteInformation($"Completed save of the XMLTV file to \"{Helper.StirrTvXmltvPath}\". ({Helper.BytesToString(fi.Length)})");
                Logger.WriteVerbose($"Generated XMLTV file contains {xmltv.Channels.Count} channels and {xmltv.Programs.Count} programs.");
            }

            Logger.WriteVerbose($"Stirr update execution time was {DateTime.UtcNow - startTime}.");
        }

        private static void DownloadChannel(string channelId)
        {
            _channels[channelId].Status = api.GetChannelDetail(channelId);
            _channels[channelId].Guide = api.GetChannelGuide(channelId);
        }
    }
}