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

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out int lpdwProcessId);

        [DllImport("user32", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
        public static extern short GetAsyncKeyState(int vKey);
        [DllImport("user32", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
        public static extern short GetKeyState(int vKey);
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
            if (hWnd == IntPtr.Zero)
                hWnd = GetForegroundWindow();
            try
            {
                var wProc = aIncludeProc ? GetActiveWindowProcess(hWnd) : null;
                var wIcon = GetActiveWindowIcon(hWnd);
                var wText = GetActiveWindowTitle(hWnd);
                return new object[] { wIcon, wText, wProc };
            } catch { }
            return null;
        }
        public static void ShowActiveWindow(IntPtr hWnd, bool aShow)
        {
            if (hWnd == IntPtr.Zero)
                hWnd = GetForegroundWindow();
            ShowWindow(hWnd, aShow ? 5 : 0);
        }
    }
}
