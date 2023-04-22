using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace trayer
{
    internal class Native32
    {
        [DllImport("user32", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", CharSet = CharSet.Ansi, EntryPoint = "GetWindowTextLengthA", ExactSpelling = true, SetLastError = true)]
        private static extern int GetWindowTextLength(IntPtr hwnd);

        [DllImport("user32", CharSet = CharSet.Ansi, EntryPoint = "GetWindowTextA", ExactSpelling = true, SetLastError = true)]
        private static extern int GetWindowText(IntPtr hwnd, [MarshalAs(UnmanagedType.VBByRefStr)] ref string lpString, int cch);

        [DllImport("user32", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
        private static extern int ShowWindow(IntPtr hwnd, int nCmdShow);
        private const int WS_EX_TOOLWINDOW = 0x00000080; // hides from taskbar
        private const int WS_EX_APPWINDOW = 0x00040000;
        private const int GWL_EXSTYLE = -0x14;

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out int lpdwProcessId);

        [DllImport("User32.dll")]
        public static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
        [DllImport("User32.dll")]
        public static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        public static IntPtr GetActiveWindow() { return GetForegroundWindow(); }
        public static string GetActiveWindowTitle() => GetActiveWindowTitle(IntPtr.Zero);
        public static Process GetActiveWindowProcess(IntPtr hWnd)
        {
            if (IntPtr.Zero == hWnd)
                hWnd = GetForegroundWindow();
            GetWindowThreadProcessId(hWnd, out int lpdwProcessId);
            var p = Process.GetProcessById(lpdwProcessId);
            return p;
        }
        public static string GetActiveWindowTitle(IntPtr hWnd)
        {
            if (IntPtr.Zero == hWnd)
                hWnd = GetForegroundWindow();
            int len = GetWindowTextLength(hWnd);
            if (1 > len++) return "";
            string result = "".PadLeft(len, '\0');
            len = GetWindowText(hWnd, ref result, len);
            return result.Substring(0, len);
        }
        public static Icon GetActiveWindowIcon() => GetActiveWindowIcon(IntPtr.Zero);
        public static Icon GetActiveWindowIcon(IntPtr hWnd)
        {
            if (hWnd == IntPtr.Zero)
                hWnd = GetForegroundWindow();
            var msg = SendMessage(hWnd, 127, IntPtr.Zero, IntPtr.Zero);
            if (IntPtr.Zero != msg)
                return Icon.FromHandle(msg);
            return null;
        }
        public static object[] GetActiveWindowInfo(IntPtr hWnd, bool aIncludeProc=false)
        {
            // 0=wIcon, 1=wText, 2=wProc, 3=hWnd, 4=wStyle
            if (hWnd == IntPtr.Zero)
                hWnd = GetForegroundWindow();
            try
            {
                var wProc = aIncludeProc ? GetActiveWindowProcess(hWnd) : null;
                var wIcon = GetActiveWindowIcon(hWnd);
                var wText = GetActiveWindowTitle(hWnd);
                var wStyle = GetWindowLong(hWnd, GWL_EXSTYLE);
                return new object[] { wIcon, wText, wProc, hWnd, wStyle };
            } catch { }
            return null;
        }
        public static void ShowActiveWindow(object[] awInfo, bool aShow)
        {
            if ((IntPtr)awInfo[3] == IntPtr.Zero)
                awInfo[3] = GetForegroundWindow();
            ShowWindow((IntPtr)awInfo[3], aShow ? 5 : 0);
            if (!aShow)
                SetWindowLong((IntPtr)awInfo[3], GWL_EXSTYLE, (int)awInfo[4] | WS_EX_TOOLWINDOW);
            else SetWindowLong((IntPtr)awInfo[3], GWL_EXSTYLE, (int)awInfo[4] & ~WS_EX_TOOLWINDOW);
        }
    }
}
