using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Pipes;
using System.Reflection;
using System.Security.Principal;
using System.Threading;

namespace GaRyan2.Utilities
{
    public static partial class Helper
    {
        public enum PreferredLogos
        {
            WHITE,
            DARK,
            LIGHT,
            GRAY,
            NONE
        };
        public enum Installation
        {
            FULL,
            SERVER,
            CLIENT,
            PORTABLE,
            UNKNOWN
        }

        private static AssemblyName _assembly => Assembly.GetEntryAssembly()?.GetName();
        public static string Epg123Version => _assembly.Version.ToString();
        public static string Epg123AssemblyName => _assembly.Name.ToUpper();
        public static bool Standalone => !File.Exists(TokenServer);

        public static Installation InstallMethod
        {
            get
            {
                if (File.Exists(TokenServer) && File.Exists(Epg123ExePath)) return File.Exists(Epg123ClientExePath) ? Installation.FULL : Installation.SERVER;
                if (!File.Exists(TokenServer) && File.Exists(Epg123ClientExePath)) return File.Exists(Epg123ExePath) ? Installation.PORTABLE : Installation.CLIENT;
                return File.Exists(Epg123ExePath) ? Installation.PORTABLE : Installation.UNKNOWN;
            }
        }

        public static string BackupZipFile { get; set; }
        public static string OutputPathOverride { get; set; }

        public const int TcpUdpPort = 9009;

        public static Mutex GetProgramMutex(string uid, bool take)
        {
            var mutex = new Mutex(false, uid);
            try
            {
                var tryAgain = true;
                while (tryAgain)
                {
                    bool result;
                    try
                    {
                        result = mutex.WaitOne(0, false);
                    }
                    catch (AbandonedMutexException)
                    {
                        result = true;
                    }

                    if (result)
                    {
                        tryAgain = false;
                    }
                    else if (take)
                    {
                        foreach (var proc in Process.GetProcesses())
                        {
                            if (!proc.ProcessName.Equals(Process.GetCurrentProcess().ProcessName) ||
                                proc.Id == Process.GetCurrentProcess().Id) continue;
                            Logger.WriteInformation($"Killing process {proc.ProcessName}[{proc.Id}] to continue execution of new instance.");
                            proc.Kill();
                            proc.WaitForExit(2000);
                            break;
                        }
                    }
                    else
                    {
                        Logger.WriteMessage("===============================================================================");
                        Logger.WriteError($"An instance of {Process.GetCurrentProcess().ProcessName} is already running. Aborting.");
                        Logger.WriteMessage("===============================================================================");
                        //Logger.Close();

                        mutex.Close();
                        return null;
                    }
                }
            }
            finally
            {
                // do nothing
            }

            return mutex;
        }

        /// <summary>
        /// Determines whether program is run with elevated rights
        /// </summary>
        /// <returns></returns>
        public static bool UserHasElevatedRights
        {
            get
            {
                var identity = WindowsIdentity.GetCurrent();
                var principal = new WindowsPrincipal(identity);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
        }

        public static bool TableContains(string[] table, string text, bool exactMatch = false)
        {
            if (table == null) return false;
            foreach (var str in table)
            {
                if (!exactMatch && str.ToLower().Contains(text.ToLower())) return true;
                if (str.ToLower().Equals(text.ToLower())) return true;
            }

            return false;
        }

        public static bool StringContains(string str, string text)
        {
            return str != null && str.ToLower().Contains(text.ToLower());
        }

        public static void WriteEButtonFile(string action)
        {
            using (var sw = new StreamWriter(EButtonPath, false))
            {
                sw.WriteLine(action);
                sw.Flush();
                sw.Close();
            }
        }

        public static void SendPipeMessage(string message)
        {
            var client = new NamedPipeClientStream(".", "Epg123StatusPipe", PipeDirection.Out);
            try
            {
                client.Connect(100);
                var writer = new StreamWriter(client);
                writer.WriteLine(message);
                writer.Flush();
            }
            catch
            {
                // ignored
            }
        }

        public static Bitmap CropAndResizeImage(Bitmap origImg)
        {
            if (origImg == null) return null;

            // set target image size
            const int tgtWidth = 360;
            const int tgtHeight = 270;

            // set target aspect/image size
            const double tgtAspect = 3.0;

            // Find the min/max non-transparent pixels
            var min = new Point(int.MaxValue, int.MaxValue);
            var max = new Point(int.MinValue, int.MinValue);

            for (var x = 0; x < origImg.Width; ++x)
            {
                for (var y = 0; y < origImg.Height; ++y)
                {
                    var pixelColor = origImg.GetPixel(x, y);
                    if (pixelColor.A <= 0) continue;
                    if (x < min.X) min.X = x;
                    if (y < min.Y) min.Y = y;

                    if (x > max.X) max.X = x;
                    if (y > max.Y) max.Y = y;
                }
            }

            // Create a new bitmap from the crop rectangle and increase canvas size if necessary
            var offsetY = 0;
            var cropRectangle = new Rectangle(min.X, min.Y, max.X - min.X + 1, max.Y - min.Y + 1);
            if ((max.X - min.X + 1) / tgtAspect > (max.Y - min.Y + 1))
            {
                offsetY = (int)((max.X - min.X + 1) / tgtAspect - (max.Y - min.Y + 1) + 0.5) / 2;
            }

            var cropImg = new Bitmap(cropRectangle.Width, cropRectangle.Height + offsetY * 2);
            cropImg.SetResolution(origImg.HorizontalResolution, origImg.VerticalResolution);
            using (var g = Graphics.FromImage(cropImg))
            {
                g.DrawImage(origImg, 0, offsetY, cropRectangle, GraphicsUnit.Pixel);
            }

            if (tgtHeight >= cropImg.Height && tgtWidth >= cropImg.Width) return cropImg;

            // resize image if needed
            var scale = Math.Min((double)tgtWidth / cropImg.Width, (double)tgtHeight / cropImg.Height);
            var destWidth = (int)(cropImg.Width * scale);
            var destHeight = (int)(cropImg.Height * scale);
            return new Bitmap(cropImg, new Size(destWidth, destHeight));
        }

        public static void ViewLogFile()
        {
            if (!File.Exists(Epg123TraceLogPath)) return;
            Process.Start(new ProcessStartInfo
            {
                FileName = LogViewer,
                Arguments = $"\"{Epg123TraceLogPath}\""
            });
        }

        public static string BytesToString(long bytes)
        {
            string[] unit = { "", "K", "M", "G", "T" };
            for (var i = 0; i < unit.Length; ++i)
            {
                double calc;
                if ((calc = bytes / Math.Pow(1024, i)) < 1024)
                {
                    return $"{calc:N3} {unit[i]}B";
                }
            }
            return "0 bytes";
        }

        public static string ReportExceptionMessages(Exception ex)
        {
            var ret = string.Empty;
            var cnt = 0;
            var innerException = ex;
            do
            {
                ret += $"\n Exception {cnt}: {ex.Message}";
                innerException = innerException.InnerException;
            } while (innerException != null);
            return ret;
        }
    }
}