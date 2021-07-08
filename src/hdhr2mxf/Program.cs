using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using epg123;
using hdhr2mxf.MXF;

namespace hdhr2mxf
{
    class Program
    {
        private static bool automaticallyImport;
        private static string outputFile = "hdhr2mxf.mxf";

        private static int Main(string[] args)
        {
            // set the base path and the working directory
            Helper.ExecutablePath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            if (!string.IsNullOrEmpty(Helper.ExecutablePath)) Directory.SetCurrentDirectory(Helper.ExecutablePath);

            Logger.WriteMessage("===============================================================================");
            Logger.WriteMessage($" Beginning hdhr2mxf update execution. version {Helper.Epg123Version}");
            Logger.WriteMessage("===============================================================================");

            var starttime = DateTime.UtcNow;
            if (args != null)
            {
                for (var i = 0; i < args.Length; ++i)
                {
                    switch (args[i].ToLower())
                    {
                        case "-o":
                            if ((i + 1) < args.Length && string.IsNullOrEmpty(Helper.OutputPathOverride))
                            {
                                outputFile = args[++i].Replace("\"", "");
                                var path = Path.GetDirectoryName(outputFile);
                                if (!string.IsNullOrEmpty(path))
                                {
                                    Helper.OutputPathOverride = path;
                                    if (!Directory.Exists(path)) Directory.CreateDirectory(path);
                                }
                            }
                            else if ((i + 1) >= args.Length)
                            {
                                Logger.WriteError("Missing output filename and path.");
                                return -1;
                            }
                            break;
                        case "-nologos":
                            Common.NoLogos = true;
                            break;
                        case "-import":
                            automaticallyImport = true;
                            break;
                        case "-update":
                            Helper.OutputPathOverride = Helper.Epg123OutputFolder;
                            outputFile = Helper.Epg123MxfPath;
                            
                            string[] folders = { Helper.Epg123OutputFolder };
                            foreach (var folder in folders)
                            {
                                if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
                            }
                            break;
                    }
                }
            }
            if (string.IsNullOrEmpty(Helper.OutputPathOverride))
            {
                Helper.OutputPathOverride = Helper.ExecutablePath;
            }

            // initialize keyword groups
            Common.InitializeKeywordGroups();

            try
            {
                Helper.SendPipeMessage("Downloading|Requesting XMLTV from SiliconDust...");
                if (DetermineUpdateMethod())
                {
                    Common.BuildKeywords();
                    WriteMxf();
                    Helper.SendPipeMessage("Download Complete");

                    if (automaticallyImport)
                    {
                        ImportMxfStream();
                    }
                }
                else
                    Helper.SendPipeMessage("Download Complete");
            }
            catch (Exception ex)
            {
                Logger.WriteError("Exception Thrown:");
                Logger.WriteError(ex.Message);
                Logger.WriteError(ex.StackTrace);
            }

            Logger.WriteInformation(string.Format("Generated .mxf file contains {5} lineups, {0} services, {1} series, {2} programs, and {3} people with {4} image links.", Common.Mxf.With[0].Services.Count, Common.Mxf.With[0].SeriesInfos.Count, Common.Mxf.With[0].Programs.Count, Common.Mxf.With[0].People.Count, Common.Mxf.With[0].GuideImages.Count, Common.Mxf.With[0].Lineups.Count));
            Logger.WriteInformation($"Execution time was {DateTime.UtcNow - starttime}");

            return 0;
        }

        private static void ImportMxfStream()
        {
            var startInfo = new ProcessStartInfo()
            {
                Arguments = "-i " + outputFile,
                FileName = Environment.GetEnvironmentVariable("systemroot") + @"\ehome\loadmxf.exe",
            };
            Process.Start(startInfo)?.WaitForExit();

            startInfo = new ProcessStartInfo()
            {
                FileName = "schtasks.exe",
                Arguments = "/run /tn \"Microsoft\\Windows\\Media Center\\ReindexSearchRoot\"",
            };
            Process.Start(startInfo)?.WaitForExit();
        }

        private static bool DetermineUpdateMethod()
        {
            // find all HDHomeRun tuning devices on the network
            var homeruns = Common.Api.DiscoverDevices();
            if (homeruns == null || !homeruns.Any())
            {
                Logger.WriteError("No HDHomeRun devices were found.");
                return false;
            }

            // determine if DVR Service is active
            var dvrActive = false;
            foreach (var device in homeruns.Select(homerun => Common.Api.ConnectDevice(homerun.DiscoverUrl)))
            {
                if (device == null) continue;
                Logger.WriteInformation($"Found {device.FriendlyName} {device.ModelNumber} ({device.DeviceId}) with firmware {device.FirmwareVersion}.");
                dvrActive |= Common.Api.IsDvrActive(device.DeviceAuth);
            }
            Logger.WriteInformation($"HDHomeRun DVR Service is {(dvrActive ? string.Empty : "not ")}active.");

            // if DVR Service is active, use XMLTV; otherwise use iterative load from slice guide
            if (dvrActive)
            {
                Logger.WriteInformation("Downloading available 14-day XMLTV file from SiliconDust.");
                Helper.SendPipeMessage("Downloading|Building and saving MXF file...");
                return XmltvMxf.BuildMxfFromXmltvGuide(homeruns.ToList());
            }
            //else
            //{
            //    Console.WriteLine("Using available 24-hour slice guide data from SiliconDust.");
            //    return SliceMxf.BuildMxfFromSliceGuide(homeruns.ToList());
            //}

            Logger.WriteError("HDHR2MXF is not configured to download guide data using JSON from SiliconDust.");
            return false;
        }

        private static void WriteMxf()
        {
            Logger.WriteInformation($"Writing the MXF file to \"{outputFile}\"");
            try
            {
                using (var stream = new StreamWriter(outputFile, false, Encoding.UTF8))
                {
                    var serializer = new XmlSerializer(typeof(mxf));
                    var ns = new XmlSerializerNamespaces();
                    ns.Add("", "");
                    TextWriter writer = stream;
                    serializer.Serialize(writer, Common.Mxf, ns);
                }
            }
            catch (Exception ex)
            {
                Logger.WriteError($"Failed to save the MXF file to \"{outputFile}\". Message: {ex.Message}");
            }
        }
    }
}