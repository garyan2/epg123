using GaRyan2.Utilities;
using GaRyan2.XmltvXml;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace GaRyan2.SiliconDustApi
{
    internal class API : BaseAPI
    {
        public override async Task<T> GetHttpResponse<T>(HttpMethod method, string uri, object content = null)
        {
            var request = (HttpWebRequest)WebRequest.Create(uri);
            request.UserAgent = UserAgent;
            request.AutomaticDecompression = DecompressMethods;
            using (var response = (HttpWebResponse)(await request.GetResponseAsync().ConfigureAwait(false)))
            {
                using (var sr = new StreamReader(response.GetResponseStream()))
                {
                    return JsonConvert.DeserializeObject<T>(sr.ReadToEnd(), JsonOptions);
                }
            }
        }

        public XMLTV DownloadXmltvFile(string authString)
        {
            var response = _httpClient.GetAsync($"https://api.hdhomerun.com/api/xmltv.php?DeviceAuth={Uri.EscapeDataString(authString)}").Result;
            using (var stream = new StreamReader(response.Content.ReadAsStreamAsync().Result))
            {
                if (stream == null) return null;

                // discard first line which starts with <? and chokes xmltv class
                var firstline = stream.ReadLine();
                var xmltv = (firstline.StartsWith("<?") ? "" : firstline) + stream.ReadToEnd();

                // save raw xmltv file from SiliconDust
                using (var sw = new StreamWriter(Helper.Hdhr2mxfXmltvPath, false, Encoding.UTF8))
                {
                    sw.Write(firstline + "\n" + xmltv);
                }
                Helper.GZipCompressFile(Helper.Hdhr2mxfXmltvPath);
                Helper.DeflateCompressFile(Helper.Hdhr2mxfXmltvPath);

                var serializer = new XmlSerializer(typeof(XMLTV));
                TextReader reader = new StringReader(xmltv);
                return (XMLTV)serializer.Deserialize(reader);
            }
        }

        public List<HdhrDiscover> DiscoverDevices()
        {
            var ret = UDPDiscover.DiscoverDevicesUdp();
            if (ret.Count == 0) Logger.WriteError($"Did not find any HDHomeRun devices on the network.");
            else Logger.WriteInformation($"Discovered {ret.Count} HDHomeRun devices on the network.");
            return ret?.OrderBy(x => x.StorageID ?? "").ThenBy(x => x.DeviceId ?? "").ToList();
        }

        public HdhrDevice GetDeviceDetails(string discoverUrl)
        {
            var ret = GetApiResponse<HdhrDevice>(Method.GET, discoverUrl);
            if (ret == null) Logger.WriteInformation($"Failed to get details for device at {discoverUrl}.");
            return ret;
        }

        public HdhrAccount GetDeviceAccount(string deviceAuth)
        {
            var ret = GetApiResponse<HdhrAccount>(Method.GET, $"https://api.hdhomerun.com/api/account?DeviceAuth={deviceAuth}");
            if (ret == null) Logger.WriteInformation("Failed to get account details for device.");
            return ret;
        }

        public List<HdhrChannel> GetDeviceChannels(string lineupUrl, int legacy)
        {
            var ret = GetApiResponse<List<HdhrChannel>>(Method.GET, $"{lineupUrl}{(legacy > 0 ? "&" : "?")}tuning");
            if (ret == null) Logger.WriteInformation($"Failed to get lineup channels from {lineupUrl}.");
            return ret;
        }

        public List<HdhrChannelDetail> GetDeviceChannelDetails(string deviceAuth)
        {
            var ret = GetApiResponse<List<HdhrChannelDetail>>(Method.GET, $"https://api.hdhomerun.com/api/guide.php?DeviceAuth={deviceAuth}");
            if (ret == null) Logger.WriteInformation($"Failed to get channel services for device.");
            return ret;
        }

    }
}