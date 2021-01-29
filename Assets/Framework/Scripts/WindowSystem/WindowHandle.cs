using System.Runtime.InteropServices;
using System;

namespace Framework
{

    public class WindowHandle
    {

        delegate bool EnumWindowsCallBack(IntPtr hwnd, IntPtr lParam);

#if UNITY_EDITOR || UNITY_STANDALONE
        [DllImport("USER32.DLL")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);
        [DllImport("user32.dll")]
        public static extern bool ShowWindowAsync(IntPtr hWnd, int nCmdShow);
        [DllImport("user32.dll")]
        public static extern bool IsIconic(IntPtr hWnd);
        [DllImport("user32.dll")]
        public static extern void SwitchToThisWindow(IntPtr hWnd, bool fAltTab);
        [DllImport("user32.dll", EntryPoint = "SystemParametersInfo")]
        public static extern int SystemParametersInfo(int uAction, int uParam, IntPtr lpvParam, int fuWinIni);
        [DllImport("user32.dll", EntryPoint = "SetWindowText", CharSet = CharSet.Auto)]
        public static extern bool SetWindowText(System.IntPtr hwnd, System.String lpString);
        [DllImport("user32.dll", EntryPoint = "FindWindow")]
        public static extern System.IntPtr FindWindow(System.String className, System.String windowName);
        [DllImport("user32.dll")]
        public static extern System.IntPtr GetForegroundWindow();
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern int GetWindowText(IntPtr hWnd, System.Text.StringBuilder text, int count);
        [DllImport("user32.dll")]
        public static extern int ShowWindow(IntPtr hwnd, int nCmdShow); //这是显示任务栏

        [DllImport("user32.dll", EntryPoint = "GetWindowLong")]
        public static extern int GetWindowLong(IntPtr hwnd, int nIndex);
        [DllImport("user32.dll", EntryPoint = "SetWindowLong")]
        public static extern int SetWindowLong(IntPtr hMenu, int nIndex, int dwNewLong);
        [DllImport("user32.dll")]
        private static extern int SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, int uFlags);

        [DllImport("user32")]
        static extern int EnumWindows(EnumWindowsCallBack lpEnumFunc, IntPtr lParam);
        [DllImport("user32")]
        static extern uint GetWindowThreadProcessId(IntPtr hWnd, ref IntPtr lpdwProcessId);
        [DllImport("kernel32.dll", ExactSpelling = true, CharSet = CharSet.Auto)]
        public static extern int GetCurrentThreadId();
        [DllImport("user32")]
        static extern uint AttachThreadInput(IntPtr hWnd, ref IntPtr lpdwProcessId);

#endif

        /// 当前窗口句柄
        public static IntPtr mWindowHandle;

        static WindowHandle()
        {
#if UNITY_EDITOR || UNITY_STANDALONE
            IntPtr handle = (IntPtr)System.Diagnostics.Process.GetCurrentProcess().Id;
            EnumWindows(new EnumWindowsCallBack(EnumWindCallback), handle);
#endif
        }

        public static void SetWindowTop(IntPtr hWnd)
        {
#if UNITY_EDITOR || UNITY_STANDALONE
            IntPtr HWND_TOPMOST = new IntPtr(-1);
            IntPtr HWND_NOTOPMOST = new IntPtr(-2);
            int SWP_NOSIZE = 0x0001;
            int SWP_NOMOVE = 0x0002;
            int SWP_SHOWWINDOW = 0x0040;
            int SW_SHOWNORMAL = 1;
            //IntPtr hForeWnd = GetForegroundWindow();
            //int dwCurID = GetCurrentThreadId();
            //uint dwForeID = GetWindowThreadProcessId(hForeWnd, NULL);
            //AttachThreadInput(dwCurID, dwForeID, TRUE);
            ShowWindow(hWnd, SW_SHOWNORMAL);
            SetWindowPos(hWnd, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOSIZE | SWP_NOMOVE | SWP_SHOWWINDOW);
            SetWindowPos(hWnd, HWND_NOTOPMOST, 0, 0, 0, 0, SWP_NOSIZE | SWP_NOMOVE | SWP_SHOWWINDOW);
            //SetForegroundWindow(hWnd);
#endif
        }

        public static void SetWindowTitle(string text)
        {
#if UNITY_EDITOR || UNITY_STANDALONE
            if (mWindowHandle != IntPtr.Zero)
            {
                SetWindowText(mWindowHandle, text);
            }
#endif
        }

        private static bool EnumWindCallback(IntPtr hwnd, IntPtr lParam)
        {
#if UNITY_EDITOR || UNITY_STANDALONE
            IntPtr pid = IntPtr.Zero;
            GetWindowThreadProcessId(hwnd, ref pid);
            if (pid == lParam)  //判断当前窗口是否属于本进程
            {
                mWindowHandle = hwnd;
                return false;
            }
#endif
            return true;
        }

        public static void DisableMaxmizebox(bool isDisable)
        {
#if UNITY_EDITOR || UNITY_STANDALONE
            int GWL_STYLE = -16;
            int WS_MAXIMIZEBOX = 0x00010000;
            int SWP_NOSIZE = 0x0001;
            int SWP_NOMOVE = 0x0002;
            int SWP_FRAMECHANGED = 0x0020;
            IntPtr handle = GetForegroundWindow();
            int nStyle = GetWindowLong(handle, GWL_STYLE);
            if (isDisable)
            {
                nStyle &= ~(WS_MAXIMIZEBOX);
            }
            else
            {
                nStyle |= WS_MAXIMIZEBOX;
            }
            SetWindowLong(handle, GWL_STYLE, nStyle);
            SetWindowPos(handle, IntPtr.Zero, 0, 0, 0, 0, SWP_NOSIZE | SWP_NOMOVE | SWP_FRAMECHANGED);
#endif

        }
    }

}