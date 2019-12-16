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
        public List<HDHRDiscover> DiscoverDevices()
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://my.hdhomerun.com/discover");
                request.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                using (StreamReader sr = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
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
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
                request.Timeout = 1000;
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                using (StreamReader sr = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
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
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url + "?tuning");
                request.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                using (StreamReader sr = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
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
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                using (StreamReader sr = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
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

                string url = "http://my.hdhomerun.com/api/xmltv.php?DeviceAuth=" + auth;
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                using (StreamReader sr = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(XMLTV));
                    string firstLine = sr.ReadLine();
                    TextReader reader = new StringReader((firstLine.StartsWith("<?") ? string.Empty : firstLine) + sr.ReadToEnd());
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
                string url = "http://api.hdhomerun.com/api/account?DeviceAuth=" + deviceAuth;
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                using (StreamReader sr = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
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