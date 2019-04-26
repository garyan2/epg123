using System;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Windows.Forms;

namespace epg123
{
    /// <summary>
    /// Is a button with the UAC shield
    /// </summary>
    public partial class ElevatedButton : Button
    {
        /// <summary>
        /// The constructor to create the button with a UAC shield if necessary.
        /// </summary>
        public ElevatedButton()
        {
            FlatStyle = FlatStyle.System;
            if (!IsElevated()) ShowShield();
        }

        private uint BCM_SETSHIELD = 0x0000160C;

        private bool IsElevated()
        {
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        public void ShowShield()
        {
            IntPtr wParam = new IntPtr(0);
            IntPtr lParam = new IntPtr(1);
            NativeMethods.SendMessage(new HandleRef(this, Handle), BCM_SETSHIELD, wParam, lParam);
        }
    }
}
