using OpenCvSharp;
using OpenCvSharp.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace DataNexus
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

        #region Mouse Event
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

        public bool IsFileWriteComplete(string filepath, int timeout)
        {
            bool state = false;
            while (true)
            {
                if (File.Exists(filepath))
                {
                    DateTime lastWriteTime = File.GetLastWriteTime(filepath);
                    if (DateTime.Now - lastWriteTime > TimeSpan.FromSeconds(timeout))
                    {
                        state = true;
                        break;
                    }
                }
                Thread.Sleep(1000);
            }
            return state;
        }

        private BitmapImage ConvertBitmapToBitmapImage(Bitmap bitmap)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Png);
                memory.Position = 0;
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.StreamSource = memory;
                bitmapImage.EndInit();
                bitmapImage.Freeze();
                return bitmapImage;
            }
        }

        public T CaptureRegion<T>(int x, int y, int width, int height, bool show_cursor = false) where T : class
        {
            using (Bitmap bitmap = new Bitmap(width, height))
            {
                using (Graphics g = Graphics.FromImage(bitmap))
                {
                    g.CopyFromScreen(x, y, 0, 0, new System.Drawing.Size(width, height));
                    if (show_cursor)
                    {
                        System.Drawing.Point cursorPosition = System.Windows.Forms.Cursor.Position;
                        System.Windows.Forms.Cursor cursor = System.Windows.Forms.Cursors.Default;
                        System.Drawing.Rectangle cursorBounds = new System.Drawing.Rectangle(cursorPosition, cursor.Size);
                        cursor.Draw(g, cursorBounds);
                    }
                }
                switch (typeof(T))
                {
                    case Type t when t == typeof(BitmapImage):
                        return ConvertBitmapToBitmapImage(bitmap) as T;
                    case Type t when t == typeof(Mat):
                        return BitmapConverter.ToMat(bitmap) as T;
                    case Type t when t == typeof(Bitmap):
                        return bitmap as T;
                    default:
                        throw new NotSupportedException($"Type {typeof(T)} is not supported.");
                }
            }
        }

        // 導入調用下一個鉤子的 API
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);
        // 導入卸載鉤子的 API
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern bool UnhookWindowsHookEx(IntPtr hHook);
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);
        #region Lock Keyboard
        // 鉤子的常量
        private const int WH_KEYBOARD_LL = 13;  // 鍵盤鉤子的類型
        private const int WM_KEYDOWN = 0x0100;   // 鍵盤按鍵按下消息
        private const int VK_C = 0x43;

        // 將委託的訪問修飾符改為 public
        public delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        // 定義鉤子
        private static LowLevelKeyboardProc _keyboardProc = KeyboardHookCallback;
        private static IntPtr _keyboardHookID = IntPtr.Zero;

        // 導入設置鉤子的 API
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        // 導入獲取當前線程 ID 的 API
        [DllImport("kernel32.dll")]
        public static extern uint GetCurrentThreadId();

        // 結構體，存儲鍵盤信息
        [StructLayout(LayoutKind.Sequential)]
        private struct KBDLLHOOKSTRUCT
        {
            public int vkCode;  // 虛擬鍵碼
            public int scanCode;
            public int flags;
            public int time;
            public IntPtr dwExtraInfo;
        }

        // 鉤子回調函式
        private static IntPtr KeyboardHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
            {
                // 使用非泛型的 Marshal.PtrToStructure
                KBDLLHOOKSTRUCT kbStruct = (KBDLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(KBDLLHOOKSTRUCT));
                // 如果按下的是 C 鍵 (VK_C)，則允許輸入
                if (kbStruct.vkCode == VK_C)
                {
                    return CallNextHookEx(_keyboardHookID, nCode, wParam, lParam);
                }
                // 阻止其他按鍵
                return (IntPtr)1;
            }
            return CallNextHookEx(_keyboardHookID, nCode, wParam, lParam);
        }

        // 鎖定鍵盤的函式
        public void LockKeyboard()
        {
            if (_keyboardHookID == IntPtr.Zero)
            {
                IntPtr moduleHandle = GetModuleHandle(null); // 直接獲取當前模組句柄
                _keyboardHookID = SetWindowsHookEx(WH_KEYBOARD_LL, _keyboardProc, moduleHandle, 0);
            }
        }

        // 解鎖鍵盤的函式
        public void UnlockKeyboard()
        {
            if (_keyboardHookID != IntPtr.Zero)
            {
                UnhookWindowsHookEx(_keyboardHookID);
                _keyboardHookID = IntPtr.Zero;  // 重置變數，確保可以重新鎖定
            }
        }
        #endregion

        #region Lock Mouse
        private const int WH_MOUSE_LL = 14; // 低階滑鼠鉤子
        private static IntPtr _mouseHookID = IntPtr.Zero;
        private static HookProc _mouseProc = MouseHookCallback;

        // 定义委托
        private delegate IntPtr HookProc(int nCode, IntPtr wParam, IntPtr lParam);

        // 引入 API
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, HookProc lpfn, IntPtr hMod, uint dwThreadId);


        private static IntPtr SetHook(HookProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_MOUSE_LL, proc, GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        // 钩子回调函数
        private static IntPtr MouseHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                // 阻止所有滑鼠事件
                return (IntPtr)1;
            }
            return CallNextHookEx(_mouseHookID, nCode, wParam, lParam);
        }

        public void LockMouse()
        {
            _mouseHookID = SetHook(_mouseProc);
        }

        public void UnLockMouse()
        {
            UnhookWindowsHookEx(_mouseHookID);
            _mouseHookID = IntPtr.Zero;
        }
        #endregion

    }
}
