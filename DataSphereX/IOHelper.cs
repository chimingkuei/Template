using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DataSphereX
{
    public class IOHelper
    {
        public void GetAllSubdirectories(DirectoryInfo dir, List<DirectoryInfo> subdir, bool system = false, bool hidden = false)
        {
            DirectoryInfo[] sub = dir.GetDirectories();
            if (sub.Length > 0)
            {
                subdir.Add(dir);
            }
            else if (sub.Length == 0)
            {
                subdir.Add(dir);
                return;
            }
            foreach (DirectoryInfo subDir in sub)
            {
                // 跳過系統目錄
                if (system && (subDir.Attributes & FileAttributes.System) == FileAttributes.System)
                    continue;
                // 跳過隱藏目錄
                if (hidden && (subDir.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden)
                    continue;
                GetAllSubdirectories(subDir, subdir);
            }
        }

        /// <summary>
        /// string root = @"E:\DIP Temp\Image Temp";<para/>
        /// IOHelper walker = new IOHelper();<para/>
        ///foreach (var result in walker.Walk(root))<para/>
        ///{<para/>
        ///    Console.WriteLine($"Directory: {result.Item1}");<para/>
        ///    Console.WriteLine("Subdirectories:");<para/>
        ///    foreach (var subDir in result.Item2)<para/>
        ///    {<para/>
        ///        Console.WriteLine($" - {subDir}");<para/>
        ///    }<para/>
        ///    Console.WriteLine("Files:");<para/>
        ///    foreach (var file in result.Item3)<para/>
        ///    {<para/>
        ///        Console.WriteLine($" - {file}");<para/>
        ///    }<para/>
        ///    Console.WriteLine();<para/>
        ///}<para/>
        /// </summary>
        public IEnumerable<(string, string[], string[])> Walk(string root)
        {
            // 遍歷當前目錄下的所有文件和子目錄         
            string[] files = Directory.GetFiles(root);
            string[] subDirs = Directory.GetDirectories(root);
            yield return (root, subDirs, files);
            // 遞歸遍歷子目錄      
            foreach (string subDir in subDirs)
            {
                foreach (var tuple in Walk(subDir))
                {
                    yield return tuple;
                }
            }
        }

        #region Software Lock
        private string GetBoardSerialNumber()
        {
            string boardSerial = string.Empty;
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT SerialNumber FROM Win32_BaseBoard");
            foreach (ManagementObject mo in searcher.Get())
            {
                boardSerial = mo["SerialNumber"].ToString();
                break;
            }
            return boardSerial;
        }

        public void Lock(string boardSerial)
        {
            if (GetBoardSerialNumber() != boardSerial)
            {
                Console.WriteLine("軟體2秒後關閉，請聯繫廠商提供Licence!");
                Thread.Sleep(2000);
                Environment.Exit(0);
            }
        }
        #endregion

        #region Find windows
        [DllImport("user32.dll")]
        static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
        public IntPtr FindWindowWrapper(string lpClassName, string lpWindowName)
        {
            return FindWindow(lpClassName, lpWindowName);
        }
        #endregion

        #region Set foreground windows
        [DllImport("user32.dll")]
        static extern bool SetForegroundWindow(IntPtr hWnd);
        public bool SetForegroundWindowWrapper(IntPtr hWnd)
        {
            return SetForegroundWindow(hWnd);
        }
        #endregion

        #region Show windows
        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        const int SW_HIDE = 0;
        const int SW_SHOWNORMAL = 1;
        const int SW_SHOWMINIMIZED = 2;
        const int SW_SHOWMAXIMIZED = 3;
        const int SW_SHOWNOACTIVATE = 4;
        const int SW_SHOW = 5;
        const int SW_RESTORE = 9;
        public bool ShowWindowWrapper(IntPtr hWnd)
        {
            return ShowWindow(hWnd, SW_RESTORE);
        }
        #endregion

        /// <summary>
        /// If the software window is found, bring it to the foreground.
        /// </summary>
        /// <param name="lpWindowName"></param>
        public void ActivateWindow(string lpWindowName)
        {
            IntPtr targetWindowHandle = FindWindowWrapper(null, lpWindowName);
            if (targetWindowHandle != IntPtr.Zero)
            {
                ShowWindowWrapper(targetWindowHandle);
                SetForegroundWindowWrapper(targetWindowHandle);
            }
            else
            {
                Console.WriteLine($"未找到{lpWindowName}視窗!");
            }
        }

        #region Set windows position
        [DllImport("user32.dll")]
        static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);
        public bool SetWindowsPosWrapper(string lpWindowName, Rectangle position)
        {
            IntPtr hWnd = FindWindow(null, lpWindowName);
            if (hWnd != IntPtr.Zero)
            {
                SetWindowPos(hWnd, IntPtr.Zero, position.X, position.Y, position.Width, position.Height, 0);
                return true;
            }
            else
            {
                Console.WriteLine($"未找到{lpWindowName}視窗!");
                return false;
            }
        }
        #endregion

        #region Mouse action
        #region SetCursorPos
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool SetCursorPos(int x, int y);
        #endregion
        #region GetCursorPos
        [DllImport("user32.dll")]
        static extern bool GetCursorPos(out POINT lpPoint);
        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;
            public int Y;
        }
        public POINT GetMousePosition()
        {
            POINT point;
            GetCursorPos(out point);
            return point;
        }
        #endregion
        #region mouse_event
        [DllImport("user32.dll")]
        static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint dwData, IntPtr dwExtraInfo);
        const uint MOUSEEVENTF_LEFTDOWN = 0x0002;
        const uint MOUSEEVENTF_LEFTUP = 0x0004;
        const uint MOUSEEVENTF_RIGHTDOWN = 0x0008;
        const uint MOUSEEVENTF_RIGHTUP = 0x0010;
        #endregion
        public bool SimulateLeftMouseClick(int x, int y, string action_annotation = null)
        {
            SetCursorPos(x, y);
            mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, IntPtr.Zero);
            Thread.Sleep(100);
            mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, IntPtr.Zero);
            POINT point;
            GetCursorPos(out point);
            return (x - point.X != 0 && y - point.Y != 0) ? false : true;
        }

        public bool SimulateRightMouseClick(int x, int y, string action_annotation = null)
        {
            SetCursorPos(x, y);
            mouse_event(MOUSEEVENTF_RIGHTDOWN, 0, 0, 0, IntPtr.Zero);
            Thread.Sleep(100);
            mouse_event(MOUSEEVENTF_RIGHTUP, 0, 0, 0, IntPtr.Zero);
            POINT point;
            GetCursorPos(out point);
            return (x - point.X != 0 && y - point.Y != 0) ? false : true;
        }

        public void SimulateInputText(string keys, string action_annotation = null)
        {
            System.Windows.Forms.SendKeys.SendWait(keys);
        }
        #endregion



    }
}
