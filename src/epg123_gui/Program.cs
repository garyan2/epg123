using GaRyan2.Utilities;
using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace epg123
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
        static int Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            using (var mutex = Helper.GetProgramMutex($"Global\\{AppGuid}", false))
            {
                if (mutex == null) return 0;

                var mainForm = new ConfigForm();
                Application.Run(mainForm);
                return Logger.Status;
            }
        }
    }
}
