using epg123_gui.Properties;
using GaRyan2.Utilities;
using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace epg123_gui
{
    internal static class NativeMethods
    {
        [DllImport("user32.dll")]
        internal static extern IntPtr SendMessage(HandleRef hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll")]
        internal static extern uint SetThreadExecutionState(uint esFlags);
    }

    internal static class Program
    {
        private const string AppGuid = "{0C584C83-45D4-4255-BBB8-E4119911C50E}";

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static int Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // copy over window size and location from previous version if needed
            if (Settings.Default.UpgradeRequired)
            {
                Settings.Default.Upgrade();
                Settings.Default.UpgradeRequired = false;
                Settings.Default.Save();
            }

            bool clientSetup = false;
            if (args != null)
            {
                foreach (string arg in args)
                {
                    if (arg.StartsWith("http") || arg.Contains(Helper.Epg123CfgPath))
                    {
                        Settings.Default.CfgLocation = arg;
                        clientSetup = true;
                    }
                }
            }

            using (var mutex = Helper.GetProgramMutex($"Global\\{AppGuid}", clientSetup))
            {
                if (mutex == null) return 0;

                var mainForm = new ConfigForm(clientSetup);
                Application.Run(mainForm);
                return Logger.Status;
            }
        }
    }
}
