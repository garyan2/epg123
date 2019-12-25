using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using HDHomeRunTV;
using MxfXml;

namespace hdhr2mxf
{
    class Program
    {
        static bool xmltvOnly = false;
        static bool automaticallyImport = false;
        static string outputFile = "hdhr2mxf.mxf";

        static int Main(string[] args)
        {
            // set the base path and the working directory
            epg123.Helper.ExecutablePath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            Directory.SetCurrentDirectory(epg123.Helper.ExecutablePath);

            DateTime starttime = DateTime.UtcNow;
            if (args != null)
            {
                for (int i = 0; i < args.Length; ++i)
                {
                    switch (args[i].ToLower())
                    {
                        case "-o":
                            if ((i + 1) < args.Length && string.IsNullOrEmpty(epg123.Helper.outputPathOverride))
                            {
                                outputFile = args[++i].Replace("\"", "");
                                string path = Path.GetDirectoryName(outputFile);
                                if (!string.IsNullOrEmpty(path))
                                {
                                    epg123.Helper.outputPathOverride = path;
                                    if (!Directory.Exists(path)) Directory.CreateDirectory(path);
                                }
                            }
                            else if ((i + 1) >= args.Length)
                            {
                                Console.WriteLine("Missing output filename and path.");
                                return -1;
                            }
                            break;
                        case "-nologos":
                            Common.noLogos = true;
                            break;
                        case "-import":
                            automaticallyImport = true;
                            break;
                        case "-update":
                            epg123.Helper.outputPathOverride = epg123.Helper.Epg123OutputFolder;
                            outputFile = epg123.Helper.Epg123MxfPath;
                            xmltvOnly = true;
                            
                            string[] folders = { epg123.Helper.Epg123OutputFolder };
                            foreach (string folder in folders)
                            {
                                if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
                            }
                            break;
                        default:
                            break;
                    }
                }
            }
            if (string.IsNullOrEmpty(epg123.Helper.outputPathOverride))
            {
                epg123.Helper.outputPathOverride = epg123.Helper.ExecutablePath;
            }

            // initialize keyword groups
            Common.initializeKeywordGroups();

            try
            {
                if (DetermineUpdateMethod())
                {
                    Common.buildKeywords();
                    writeMxf();
                }

                if (automaticallyImport)
                {
                    ImportMxfStream();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception Thrown:");
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }

            Console.WriteLine(string.Format("\nGenerated .mxf file contains {5} lineups, {0} services, {1} series, {2} programs, and {3} people with {4} image links.",
                    Common.mxf.With[0].Services.Count, Common.mxf.With[0].SeriesInfos.Count, Common.mxf.With[0].Programs.Count, Common.mxf.With[0].People.Count,
                    Common.mxf.With[0].GuideImages.Count, Common.mxf.With[0].Lineups.Count));
            Console.WriteLine(string.Format("Execution time was {0}", DateTime.UtcNow - starttime));

            return 0;
        }

        static void ImportMxfStream()
        {
            ProcessStartInfo startInfo = new ProcessStartInfo()
            {
                Arguments = "-i " + outputFile,
                FileName = Environment.GetEnvironmentVariable("systemroot") + @"\ehome\loadmxf.exe",
            };
            Process.Start(startInfo).WaitForExit();

            startInfo = new ProcessStartInfo()
            {
                FileName = "schtasks.exe",
                Arguments = "/run /tn \"Microsoft\\Windows\\Media Center\\ReindexSearchRoot\"",
            };
            Process.Start(startInfo).WaitForExit();

            startInfo = new ProcessStartInfo()
            {
                FileName = "schtasks.exe",
                Arguments = "/run /tn \"Microsoft\\Windows\\Media Center\\PvrScheduleTask\"",
            };
            Process.Start(startInfo).WaitForExit();
        }
        static bool DetermineUpdateMethod()
        {
            // find all HDHomeRun tuning devices on the network
            var homeruns = Common.api.DiscoverDevices();
            if (homeruns == null || homeruns.Count() == 0)
            {
                Console.WriteLine("No HDHomeRun devices were found.");
                return false;
            }

            // determine if DVR Service is active
            bool dvrActive = false;
            foreach (HDHRDiscover homerun in homeruns)
            {
                HDHRDevice device = Common.api.ConnectDevice(homerun.DiscoverURL);
                if (device == null) continue;
                else Console.WriteLine(string.Format("Found {0} {1} ({2}) with firmware {3}.", device.FriendlyName, device.ModelNumber, device.DeviceID, device.FirmwareVersion));

                dvrActive |= Common.api.IsDvrActive(device.DeviceAuth);
            }
            Console.WriteLine(string.Format("HDHomeRun DVR Service is {0}active.", dvrActive ? string.Empty : "not "));

            // if DVR Service is active, use XMLTV; otherwise use iterative load from slice guide
            if (dvrActive)
            {
                Console.WriteLine("Using available 14-day XMLTV file from SiliconDust.");
                return XmltvMxf.BuildMxfFromXmltvGuide(homeruns.ToList());
            }
            else if (false)// (!xmltvOnly)
            {
                Console.WriteLine("Using available 24-hour slice guide data from SiliconDust.");
                return SliceMxf.BuildMxfFromSliceGuide(homeruns.ToList());
            }
            else
            {
                Console.WriteLine("HDHR2MXF is not configured to download guide data using JSON from SiliconDust.");
                return false;
            }
        }

        private static bool writeMxf()
        {
            Console.WriteLine(string.Format("Writing the MXF file to \"{0}\"", outputFile));
            try
            {
                using (StreamWriter stream = new StreamWriter(outputFile, false, Encoding.UTF8))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(MXF));
                    XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
                    ns.Add("", "");
                    TextWriter writer = stream;
                    serializer.Serialize(writer, Common.mxf, ns);
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(string.Format("Failed to save the MXF file to \"{0}\". Message: {1}", outputFile, ex.Message));
            }
            return false;
        }
    }
}