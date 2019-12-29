using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Xml.Serialization;
using Newtonsoft.Json;
using XmltvXml;

namespace HDHomeRunTV
{
    public class HDHRAPI
    {
        private string UserAgent
        {
            get
            {
                Version version = System.Reflection.Assembly.GetEntryAssembly().GetName().Version;
                return string.Format("HDHR2MXF/{0}.{1}.{2}", version.Major, version.Minor, version.Build);
            }
        }
        private readonly string BaseUrl = "http://api.hdhomerun.com";

        private StreamReader GetRequestResponse(string url, int timeout = 0)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.UserAgent = UserAgent;
            request.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
            if (timeout > 0)
            {
                request.Timeout = timeout;
            }
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            return new StreamReader(response.GetResponseStream(), Encoding.UTF8);
        }

        public List<HDHRDiscover> DiscoverDevices()
        {
            try
            {
                using (StreamReader sr = GetRequestResponse(BaseUrl + "/discover"))
                {
                    return JsonConvert.DeserializeObject<List<HDHRDiscover>>(sr.ReadToEnd());
                }
            }
            catch (WebException wex)
            {
                Console.WriteLine("DiscoverDevices(): " + wex.Message);
                using (StreamReader sr = new StreamReader(wex.Response.GetResponseStream(), Encoding.UTF8))
                {
                    Console.WriteLine(sr.ReadToEnd());
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("DiscoverDevices(): " + e.Message);
            }
            return null;
        }

        public HDHRDevice ConnectDevice(string url)
        {
            try
            {
                using (StreamReader sr = GetRequestResponse(url, 1000))
                {
                    return JsonConvert.DeserializeObject<HDHRDevice>(sr.ReadToEnd());
                }
            }
            catch (WebException wex)
            {
                Console.WriteLine("ConnectDevice(): " + wex.Message);
                using (StreamReader sr = new StreamReader(wex.Response.GetResponseStream(), Encoding.UTF8))
                {
                    Console.WriteLine(sr.ReadToEnd());
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("ConnectDevice(): " + e.Message);
            }
            return null;
        }

        public List<HDHRChannel> GetDeviceChannels(string url)
        {
            try
            {
                using (StreamReader sr = GetRequestResponse(url + "?tuning"))
                {
                    return JsonConvert.DeserializeObject<List<HDHRChannel>>(sr.ReadToEnd());
                }
            }
            catch (WebException wex)
            {
                Console.WriteLine("GetDeviceChannels(): " + wex.Message);
                using (StreamReader sr = new StreamReader(wex.Response.GetResponseStream(), Encoding.UTF8))
                {
                    Console.WriteLine(sr.ReadToEnd());
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("GetDeviceChannels(): " + e.Message);
            }
            return null;
        }

        public List<HDHRChannelGuide> GetChannelGuide(string deviceAuth, string channel, int startTime)
        {
            try
            {
                string url = BaseUrl + "/api/guide.php?DeviceAuth=";
                url += Uri.EscapeDataString(deviceAuth) + "&Channel=" + channel + ((startTime > 0) ? "&Start=" + startTime.ToString() : string.Empty);

                using (StreamReader sr = GetRequestResponse(url))
                {
                    return JsonConvert.DeserializeObject<List<HDHRChannelGuide>>(sr.ReadToEnd());
                }
            }
            catch (WebException wex)
            {
                Console.WriteLine("GetChannelGuide(): " + wex.Message);
                using (StreamReader sr = new StreamReader(wex.Response.GetResponseStream(), Encoding.UTF8))
                {
                    Console.WriteLine(sr.ReadToEnd());
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("GetChannelGuide(): " + e.Message);
            }
            return null;
        }

        public XMLTV GetHdhrXmltvGuide(string deviceAuth)
        {
            try
            {
                //using (StreamReader sr = new StreamReader(@"C:\Temp\backup\hawleytoner\hdhr2mxf.xmltv"))
                //{
                //    XmlSerializer serializer = new XmlSerializer(typeof(XMLTV));
                //    TextReader reader = new StringReader(sr.ReadToEnd());
                //    return (XMLTV)serializer.Deserialize(reader);
                //}

                if (deviceAuth == null) return null;
                using (StreamReader sr = GetRequestResponse(BaseUrl + "/api/xmltv.php?DeviceAuth=" + Uri.EscapeDataString(deviceAuth)))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(XMLTV));
                    string firstLine = sr.ReadLine();
                    string xmltv = (firstLine.StartsWith("<?") ? string.Empty : firstLine) + sr.ReadToEnd();

                    // save the xmltv file
                    using (StreamWriter sw = new StreamWriter(epg123.Helper.outputPathOverride + "\\hdhr2mxf.xmltv", false, Encoding.UTF8))
                    {
                        sw.Write(firstLine + "\n" + xmltv);
                        sw.Close();
                    }

                    TextReader reader = new StringReader(xmltv);
                    return (XMLTV)serializer.Deserialize(reader);
                }
            }
            catch (WebException wex)
            {
                Console.WriteLine("GetHdhrXmltvGuide(): " + wex.Message);
                using (StreamReader sr = new StreamReader(wex.Response.GetResponseStream(), Encoding.UTF8))
                {
                    Console.WriteLine(sr.ReadToEnd());
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("GetHdhrXmltvGuide(): " + e.Message);
            }
            return null;
        }

        public bool IsDvrActive(string deviceAuth)
        {
            try
            {
                if (deviceAuth == null) return false;
                using (StreamReader sr = GetRequestResponse(BaseUrl + "/api/account?DeviceAuth=" + Uri.EscapeDataString(deviceAuth)))
                {
                    HDHRAccount account = JsonConvert.DeserializeObject<HDHRAccount>(sr.ReadToEnd());
                    return account.DvrActive;
                }
            }
            catch (WebException wex)
            {
                Console.WriteLine("IsDvrActive(): " + wex.Message);
                using (StreamReader sr = new StreamReader(wex.Response.GetResponseStream(), Encoding.UTF8))
                {
                    Console.WriteLine(sr.ReadToEnd());
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("IsDvrActive(): " + e.Message);
            }
            return false;
        }
    }
}