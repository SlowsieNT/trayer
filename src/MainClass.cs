using Microsoft.VisualBasic.Devices;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;


namespace trayer
{
    public class Settings
    {
        // System.Web.Extensions.dll
        public static string FileName = "AppTrayer.json";
        public Settings() { }
        public static System.Web.Script.Serialization.JavaScriptSerializer jss = new System.Web.Script.Serialization.JavaScriptSerializer();
        static string[] Keys = new string[] { "hotkey" };
        public static object[] Data = null;
        public static object[] DefaultData = new object[] {2,0,0};
        public static bool LoadSettings() {
            if (!File.Exists(FileName))
                SaveSettings(true);
            try
            {
                Data = jss.Deserialize<object[]>(File.ReadAllText(FileName));
                return true;
            } catch { Data = null; return false; }
        }
        public static bool SaveSettings(bool aDefault=false) {
            try {
                File.WriteAllText(FileName, jss.Serialize(aDefault?DefaultData:Data));
                return true;
            } catch { return false; }
        }
        public static object GetValue(string aKey, object aDefault = null) {
            if (null == Data) LoadSettings();
            var idx = Array.IndexOf(Keys, aKey);
            if (-1 != idx && null != Data)
                return Data[idx];
            return aDefault;
        }
        public static bool SetValue(string aKey, object aValue) {
            if (null == Data) LoadSettings();
            var idx = Array.IndexOf(Keys, aKey);
            if (-1 != idx && null != Data) {
                Data[idx] = aValue;
                SaveSettings();
            }
            return false;
        }
    }
    public class MainClass
    {
        string appName = "";
        NotifyIcon TrayIcon = new NotifyIcon();
        ContextMenuStrip Menu1 = new ContextMenuStrip();
        Dictionary<IntPtr, object[]> Handles = new Dictionary<IntPtr, object[]>();
        IntPtr Handle = IntPtr.Zero;
        void UncheckExcept(int aIndex, ToolStripItemCollection aItems) {
            for (var i = 0; i < aItems.Count; i++)
                ((ToolStripMenuItem)aItems[i]).Checked = i == aIndex;
        }
        public MainClass(string[] args)
        {
            var vbk = new Keyboard();
            Handle = Process.GetCurrentProcess().Handle;
            appName = Application.ProductName;
            // Show tray icon
            TrayIcon.Text = appName;
            TrayIcon.Icon = Icon.ExtractAssociatedIcon(Assembly.GetExecutingAssembly().Location);
            TrayIcon.Visible = true;
            TrayIcon.ContextMenuStrip = Menu1;
            Menu1.Renderer = new ToolStripDarkRenderer();
            // Add context menu
            var optWindow = (ToolStripMenuItem)Menu1.Items.Add("Window");
            var optStartup = (ToolStripMenuItem)Menu1.Items.Add("Startup");
            var optAlthotkeys = (ToolStripMenuItem)Menu1.Items.Add("Alt Hotkeys");
            var optExit = (ToolStripMenuItem)Menu1.Items.Add("Exit");
            var optAlthotkeysOpt1 = (ToolStripMenuItem)optAlthotkeys.DropDownItems.Add("MButton");
            var optAlthotkeysOpt2 = (ToolStripMenuItem)optAlthotkeys.DropDownItems.Add("Ctrl+MButton");
            var optAlthotkeysOpt3 = (ToolStripMenuItem)optAlthotkeys.DropDownItems.Add("Ctrl+Shift+MButton");
            var hotkeyType = (int)Settings.GetValue("hotkey", 1) % 3;
            UncheckExcept(hotkeyType, optAlthotkeys.DropDownItems);
            optAlthotkeysOpt1.Click += (s, e) => {
                Settings.SetValue("hotkey", hotkeyType = 0);
                UncheckExcept(hotkeyType, optAlthotkeys.DropDownItems);
            };
            optAlthotkeysOpt2.Click += (s, e) => {
                Settings.SetValue("hotkey", hotkeyType = 1);
                UncheckExcept(hotkeyType, optAlthotkeys.DropDownItems);
            };
            optAlthotkeysOpt3.Click += (s, e) => {
                Settings.SetValue("hotkey", hotkeyType = 2);
                UncheckExcept(hotkeyType, optAlthotkeys.DropDownItems);
            };
            optStartup.Checked = ToggleStartup(true, true);
            optExit.Click += (s, e) => {
                foreach (var h in Handles.Keys)
                    Native32.ShowActiveWindow(Handles[h], true);
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
                if (Control.MouseButtons != MouseButtons.Middle)
                    return;
                if (0 != hotkeyType && !vbk.CtrlKeyDown)
                    return;
                if (2 == hotkeyType && !vbk.ShiftKeyDown)
                    return;
                var wHandle = Native32.GetActiveWindow();
                if (wHandle == Handle) return;
                var wInfo = Native32.GetActiveWindowInfo(wHandle, true);
                if (null == wInfo) return;
                var wProc = wInfo[2] as Process;
                var wText = "" + wInfo[1];
                var wIcon = wInfo[0] as Icon;
                if ("explorer" == wProc.ProcessName && "Program Manager" == wText)
                    return;
                if ("explorer" == wProc.ProcessName && "" == wText.Trim())
                    return;
                if ("" == wText.Trim() && null == wIcon)
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
                    Native32.ShowActiveWindow(wInfo, true);
                    optWindow.DropDownItems.Remove(optSubWindow);
                };
                Native32.ShowActiveWindow(wInfo, false);
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
