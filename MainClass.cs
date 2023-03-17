using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace trayer
{
    public class MainClass
    {
        string appName = "";
        NotifyIcon TrayIcon = new NotifyIcon();
        ContextMenuStrip Menu1 = new ContextMenuStrip();
        Dictionary<IntPtr, object> Handles = new Dictionary<IntPtr, object>();
        IntPtr Handle = IntPtr.Zero;
        public MainClass(string[] args)
        {
            Handle = Process.GetCurrentProcess().Handle;
            appName = Application.ProductName;
            // Show tray icon
            TrayIcon.Text = appName;
            TrayIcon.Icon = Icon.ExtractAssociatedIcon(Assembly.GetExecutingAssembly().Location);
            TrayIcon.Visible = true;
            TrayIcon.ContextMenuStrip = Menu1;
            // Add context menu
            var optWindow = (ToolStripMenuItem)Menu1.Items.Add("Window");
            var optStartup = (ToolStripMenuItem)Menu1.Items.Add("Startup");
            var optExit = (ToolStripMenuItem)Menu1.Items.Add("Exit");
            optStartup.Checked = ToggleStartup(true, true);
            optExit.Click += (s, e) => {
                foreach (var h in Handles.Keys)
                    Native32.ShowActiveWindow(h, true);
                Application.Exit();
            };
            optStartup.Click += (s, e) => {
                optStartup.Checked = !optStartup.Checked;
                ToggleStartup(optStartup.Checked);
            };
            // Add timer
            var tmr = new System.Windows.Forms.Timer();
            tmr.Interval = 100;
            tmr.Tick += (s, e) => {
                var wHandle = Native32.GetActiveWindow();
                if (wHandle == Handle) return;
                var wInfo = Native32.GetActiveWindowInfo(wHandle, true);
                var wProc = wInfo[2] as Process;
                var wText = "" + wInfo[1];
                var wIcon = wInfo[0] as Icon;
                if ("explorer" == wProc.ProcessName && "Program Manager" == wText)
                    return;
                if ("explorer" == wProc.ProcessName && "" == wText.Trim())
                    return;
                if (Control.MouseButtons != MouseButtons.Middle)
                    return;
                if (Handles.ContainsKey(wHandle))
                    return;
                Handles.Add(wHandle, wInfo);
                string wTitle = wText;
                if (wText.Length > 16) wTitle = wTitle.Substring(0, 16) + "…"; 
                var optSubWindow = (ToolStripMenuItem)optWindow.DropDownItems.Add(wTitle);
                if (null != wIcon)
                    try { optSubWindow.Image = wIcon.ToBitmap(); } catch { }
                optSubWindow.Click += (s2, e2) => {
                    Handles.Remove(wHandle);
                    Native32.ShowActiveWindow(wHandle, true);
                    optWindow.DropDownItems.Remove(optSubWindow);
                };
                Native32.ShowActiveWindow(wHandle, false);
            };
            tmr.Enabled = true;
        }
        bool ToggleStartup(bool aAdd, bool aCheck=false)
        {
            // etc...
            var regName = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
            var key = Registry.CurrentUser.OpenSubKey(regName, true);
            if (aCheck)
                return null != key.GetValue(appName, null);
            if (aAdd)
                key.SetValue(appName, Application.ExecutablePath);
            else
                key.DeleteValue(appName, false);
            return !aAdd;
        }
    }
}
