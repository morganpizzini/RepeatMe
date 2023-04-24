using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace RepeatMe.Models
{
    public class User
    {

        [DllImport("user32.dll")]
        public static extern int SetForegroundWindow(int id);

        [DllImport("user32")]
        public static extern int GetWindow(int id, int uCmd);

        [DllImport("user32")]
        public static extern int GetDesktopWindow();

        [DllImport("user32")]
        public static extern int IsWindowVisible(int id);

        [DllImport("User32.Dll")]
        public static extern void GetWindowText(int id, StringBuilder sBuilder, int sBuilderCapacity);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool ShowWindow(int hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        public static extern int GetForegroundWindow();

    }
    public static class HandleCustom {
        private delegate bool EnumWindowsProc(IntPtr hWnd, int lParam);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("USER32.DLL")]
        private static extern bool EnumWindows(EnumWindowsProc enumFunc, int lParam);

        [DllImport("USER32.DLL")]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("USER32.DLL")]
        private static extern int GetWindowTextLength(IntPtr hWnd);

        [DllImport("USER32.DLL")]
        private static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("USER32.DLL")]
        private static extern IntPtr GetShellWindow();

        public static IDictionary<int, uint> GetOpenWindowsHndlId()
        {
            IntPtr hShellWindow = GetShellWindow();
            Dictionary<int, uint> dictWindows = new Dictionary<int, uint>();

            EnumWindows(delegate (IntPtr hWnd, int lParam)
            {
                if (hWnd == hShellWindow) return true;
                if (!IsWindowVisible(hWnd)) return true;
                
                int length = GetWindowTextLength(hWnd);
                if (length == 0) return true;

                GetWindowThreadProcessId(hWnd, out uint windowPid);
                //if (windowPid != processID) return true;
                
                StringBuilder stringBuilder = new StringBuilder(length);
                GetWindowText(hWnd, stringBuilder, length + 1);
                dictWindows.Add(hWnd.ToInt32(), windowPid);
                return true;
            }, 0);

            return dictWindows;
        }
    }
}
