﻿using System;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Windows.Forms;

namespace epg123_gui
{
    /// <summary>
    /// Is a button with the UAC shield
    /// </summary>
    public class ElevatedButton : Button
    {
        /// <summary>
        /// The constructor to create the button with a UAC shield if necessary.
        /// </summary>
        public ElevatedButton()
        {
            FlatStyle = FlatStyle.System;
            if (!IsElevated()) ShowShield();
        }

        private readonly uint BCM_SETSHIELD = 0x0000160C;

        private bool IsElevated()
        {
            var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        public void ShowShield()
        {
            var wParam = new IntPtr(0);
            var lParam = new IntPtr(1);
            NativeMethods.SendMessage(new HandleRef(this, Handle), BCM_SETSHIELD, wParam, lParam);
        }
    }
}