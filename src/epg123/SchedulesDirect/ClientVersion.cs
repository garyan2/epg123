using System;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace epg123.SchedulesDirect
{
    public static partial class SdApi
    {
        public static ClientVersion GetClientVersion()
        {
            try
            {
                ServicePointManager.SecurityProtocol |= (SecurityProtocolType)3072;
                const string url = @"https://raw.github.com/garyan2/epg123/master/src/epg123/Properties/AssemblyInfo.cs";
                var req = (HttpWebRequest)WebRequest.Create(url);
                req.Timeout = 1000;
                req.Method = "GET";
                var sr = new StreamReader(req.GetResponse().GetResponseStream(), Encoding.UTF8).ReadToEnd();

                var m = Regex.Match(sr, @"AssemblyVersion\(""(?<version>[0-9]{1,}.[0-9]{1,}.[0-9]{1,}.[0-9]{1,})""\)");
                if (m.Success)
                {
                    if (Helper.Epg123Version != m.Groups["version"].Value)
                    {
                        Logger.WriteInformation($"epg123 is not up to date. Latest version is {m.Groups["version"].Value} and can be downloaded from http://epg123.garyan2.net/download.");
                    }

                    if (!m.Groups["version"].Value.Contains(grabberVersion))
                    {
                        return new ClientVersion { Version = m.Groups["version"].Value };
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.WriteInformation($"GetClientVersion() Unknown exception thrown. Message: {ex.Message}");
            }
            return null;
        }
    }

    public class ClientVersion : BaseResponse
    {
        [JsonProperty("client")]
        public string Client { get; set; }

        [JsonProperty("version")]
        public string Version { get; set; }
    }
}
