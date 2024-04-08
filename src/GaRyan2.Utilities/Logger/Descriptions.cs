using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;

namespace GaRyan2.Utilities
{
    public static partial class Logger
    {
        public static void LogOsDescription()
        {
            var ver = string.Empty;
            try
            {
                using (var baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64))
                {
                    using (var key = baseKey.OpenSubKey(@"Software\Microsoft\Windows NT\CurrentVersion", false))
                    {
                        var kernel = FileVersionInfo.GetVersionInfo(Path.Combine(Environment.SystemDirectory, "Kernel32.dll"));
                        if (kernel.ProductMajorPart == 10)
                        {
                            if (kernel.ProductBuildPart < 22000)
                            {
                                ver = $"Windows 10 {(string)key.GetValue("EditionID", null)}";
                            }
                            else
                            {
                                ver = ((string)key.GetValue("ProductName")).Replace("10", "11");
                            }
                        }
                        else
                        {
                            ver = (string)key.GetValue("ProductName");
                        }
                        ver += $", {(Environment.Is64BitOperatingSystem ? 64 : 32)}-bit";
                        ver += $" [Version: {kernel.ProductMajorPart}.{kernel.ProductMinorPart}.{(string)key.GetValue("CurrentBuild", null) ?? "*"}.{(int)key.GetValue("UBR", 0)}]";

                        var dv = (string)key.GetValue("DisplayVersion", null);
                        if (dv != null) ver += $" ({dv})";
                    }
                }
                WriteMessage($"*** {ver} ***");
            }
            catch
            {
                WriteMessage("*** Failed to determine Windows OS version. ***");
            }
        }

        public static void LogWmcDescription()
        {
            var ver = "Windows Media Center is not installed.";
            if (File.Exists(Helper.EhshellExeFilePath))
            {
                var ehshell = FileVersionInfo.GetVersionInfo(Helper.EhshellExeFilePath);
                ver = $"Windows Media Center [Version: {ehshell.ProductVersion}] is installed.";
            }
            WriteMessage($"*** {ver} ***");
        }

        public static void LogDotNetDescription()
        {
            var ver = "Unknown";
            try
            {
                using (var ndpKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full\"))
                {
                    var release = (int)ndpKey.GetValue("Release", 0);
                    if (release >= 533320) ver = "4.8.1";
                    else if (release >= 528040) ver = "4.8";
                    else if (release >= 461808) ver = "4.7.2";
                    else if (release >= 461308) ver = "4.7.1";
                    else if (release >= 460798) ver = "4.7";
                    else if (release >= 394802) ver = "4.6.2";
                    else if (release >= 394254) ver = "4.6.1";
                    else if (release >= 393295) ver = "4.6";
                    else if (release >= 379893) ver = "4.5.2";
                    else if (release >= 378675) ver = "4.5.1";
                    else if (release >= 378389) ver = "4.5";
                    else ver = "?.?";
                    ver += $" ({ndpKey.GetValue("Version", "?.?.?????")})";
                }
                WriteMessage($"*** .NET Framework {ver} is installed. ***");
            }
            catch
            {
                WriteMessage("*** Failed to determine .NET Framework installed. ***");
            }
        }
    }
}