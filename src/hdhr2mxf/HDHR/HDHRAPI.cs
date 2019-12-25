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
                using (StreamReader sr = GetRequestResponse("http://my.hdhomerun.com/discover"))
                {
                    return JsonConvert.DeserializeObject<List<HDHRDiscover>>(sr.ReadToEnd());
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
            catch (Exception e)
            {
                Console.WriteLine("GetDeviceChannels(): " + e.Message);
            }
            return null;
        }

        public List<HDHRChannelGuide> GetChannelGuide(string auth, string channel, int startTime)
        {
            try
            {
                string url = "http://my.hdhomerun.com/api/guide.php?DeviceAuth=";
                url += auth + "&Channel=" + channel + ((startTime > 0) ? "&Start=" + startTime.ToString() : string.Empty);

                using (StreamReader sr = GetRequestResponse(url))
                {
                    return JsonConvert.DeserializeObject<List<HDHRChannelGuide>>(sr.ReadToEnd());
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("GetChannelGuide(): " + e.Message);
            }
            return null;
        }

        public XMLTV GetHdhrXmltvGuide(string auth)
        {
            try
            {
                //using (StreamReader sr = new StreamReader(@"D:\xmltv\hdhomerun.xmltv"))
                //{
                //    XmlSerializer serializer = new XmlSerializer(typeof(XMLTV));
                //    TextReader reader = new StringReader(sr.ReadToEnd());
                //    return (XMLTV)serializer.Deserialize(reader);
                //}

                using (StreamReader sr = GetRequestResponse("http://my.hdhomerun.com/api/xmltv.php?DeviceAuth=" + auth))
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
                using (StreamReader sr = GetRequestResponse("http://api.hdhomerun.com/api/account?DeviceAuth=" + deviceAuth))
                {
                    HDHRAccount account = JsonConvert.DeserializeObject<HDHRAccount>(sr.ReadToEnd());
                    return account.DvrActive;
                }
            }
            catch { }
            return false;
        }
    }
}