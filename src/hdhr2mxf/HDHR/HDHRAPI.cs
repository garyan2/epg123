using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Xml.Serialization;
using epg123;
using hdhr2mxf.XMLTV;
using Newtonsoft.Json;

namespace hdhr2mxf.HDHR
{
    public class hdhrapi
    {
        private static string UserAgent
        {
            get
            {
                var version = System.Reflection.Assembly.GetEntryAssembly()?.GetName().Version;
                return $"HDHR2MXF/{version.Major}.{version.Minor}.{version.Build}";
            }
        }

        private const string BaseUrl = "http://api.hdhomerun.com";

        private static StreamReader GetRequestResponse(string url, int timeout = 0)
        {
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.UserAgent = UserAgent;
            request.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
            if (timeout > 0)
            {
                request.Timeout = timeout;
            }
            var response = (HttpWebResponse)request.GetResponse();
            return new StreamReader(response.GetResponseStream() ?? throw new InvalidOperationException(), Encoding.UTF8);
        }

        public List<hdhrDiscover> DiscoverDevices()
        {
            try
            {
                using (var sr = GetRequestResponse(BaseUrl + "/discover"))
                {
                    return JsonConvert.DeserializeObject<List<hdhrDiscover>>(sr.ReadToEnd());
                }
            }
            catch (WebException wex)
            {
                Logger.WriteError("DiscoverDevices(): " + wex.Message);
                if (wex.Response == null) return null;
                using (var sr = new StreamReader(wex.Response.GetResponseStream(), Encoding.UTF8))
                {
                    Console.WriteLine(sr.ReadToEnd());
                }
            }
            catch (Exception e)
            {
                Logger.WriteError("DiscoverDevices(): " + e.Message);
            }
            return null;
        }

        public hdhrDevice ConnectDevice(string url)
        {
            try
            {
                using (var sr = GetRequestResponse(url, 1000))
                {
                    return JsonConvert.DeserializeObject<hdhrDevice>(sr.ReadToEnd());
                }
            }
            catch (WebException wex)
            {
                Logger.WriteError("ConnectDevice(): " + wex.Message);
                if (wex.Response == null) return null;
                using (var sr = new StreamReader(wex.Response.GetResponseStream(), Encoding.UTF8))
                {
                    Logger.WriteError(sr.ReadToEnd());
                }
            }
            catch (Exception e)
            {
                Logger.WriteError("ConnectDevice(): " + e.Message);
            }
            return null;
        }

        public List<hdhrChannel> GetDeviceChannels(string url)
        {
            try
            {
                using (var sr = GetRequestResponse(url + "?tuning"))
                {
                    return JsonConvert.DeserializeObject<List<hdhrChannel>>(sr.ReadToEnd());
                }
            }
            catch (WebException wex)
            {
                Logger.WriteError("GetDeviceChannels(): " + wex.Message);
                if (wex.Response == null) return null;
                using (var sr = new StreamReader(wex.Response.GetResponseStream(), Encoding.UTF8))
                {
                    Logger.WriteError(sr.ReadToEnd());
                }
            }
            catch (Exception e)
            {
                Logger.WriteError("GetDeviceChannels(): " + e.Message);
            }
            return null;
        }

        public List<hdhrChannelGuide> GetChannelGuide(string deviceAuth, string channel, int startTime)
        {
            try
            {
                var url = BaseUrl + "/api/guide.php?DeviceAuth=";
                url += Uri.EscapeDataString(deviceAuth) + "&Channel=" + channel + ((startTime > 0) ? "&Start=" + startTime : string.Empty);

                using (var sr = GetRequestResponse(url))
                {
                    return JsonConvert.DeserializeObject<List<hdhrChannelGuide>>(sr.ReadToEnd());
                }
            }
            catch (WebException wex)
            {
                Logger.WriteError("GetChannelGuide(): " + wex.Message);
                if (wex.Response == null) return null;
                using (var sr = new StreamReader(wex.Response.GetResponseStream(), Encoding.UTF8))
                {
                    Console.WriteLine(sr.ReadToEnd());
                }
            }
            catch (Exception e)
            {
                Logger.WriteError("GetChannelGuide(): " + e.Message);
            }
            return null;
        }

        public xmltv GetHdhrXmltvGuide(string deviceAuth)
        {
            try
            {
                if (deviceAuth == null) return null;
                using (var sr = GetRequestResponse(BaseUrl + "/api/xmltv.php?DeviceAuth=" + Uri.EscapeDataString(deviceAuth)))
                {
                    var serializer = new XmlSerializer(typeof(xmltv));
                    var firstLine = sr.ReadLine();
                    var xmltv = (firstLine.StartsWith("<?") ? string.Empty : firstLine) + sr.ReadToEnd();

                    // save the xmltv file
                    using (var sw = new StreamWriter(epg123.Helper.OutputPathOverride + "\\hdhr2mxf.xmltv", false, Encoding.UTF8))
                    {
                        sw.Write(firstLine + "\n" + xmltv);
                        sw.Close();
                    }

                    TextReader reader = new StringReader(xmltv);
                    return (xmltv)serializer.Deserialize(reader);
                }
            }
            catch (WebException wex)
            {
                Logger.WriteError("GetHdhrXmltvGuide(): " + wex.Message);
                if (wex.Response == null) return null;
                using (var sr = new StreamReader(wex.Response.GetResponseStream(), Encoding.UTF8))
                {
                    Logger.WriteError(sr.ReadToEnd());
                }
            }
            catch (Exception e)
            {
                Logger.WriteError("GetHdhrXmltvGuide(): " + e.Message);
            }
            return null;
        }

        public bool IsDvrActive(string deviceAuth)
        {
            try
            {
                if (deviceAuth == null) return false;
                using (var sr = GetRequestResponse(BaseUrl + "/api/account?DeviceAuth=" + Uri.EscapeDataString(deviceAuth)))
                {
                    var account = JsonConvert.DeserializeObject<hdhrAccount>(sr.ReadToEnd());
                    return account.DvrActive;
                }
            }
            catch (WebException wex)
            {
                Logger.WriteError("IsDvrActive(): " + wex.Message);
                if (wex.Response == null) return false;
                using (var sr = new StreamReader(wex.Response.GetResponseStream(), Encoding.UTF8))
                {
                    Console.WriteLine(sr.ReadToEnd());
                }
            }
            catch (Exception e)
            {
                Logger.WriteError("IsDvrActive(): " + e.Message);
            }
            return false;
        }
    }
}