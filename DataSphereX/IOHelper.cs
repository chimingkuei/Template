using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management;
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
        /// string rootPath = @"E:\DIP Temp\Image Temp";<para/>
        /// IOHelper walker = new IOHelper();<para/>
        ///foreach (var result in walker.Walk(rootPath))<para/>
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
        public IEnumerable<(string, string[], string[])> Walk(string rootPath)
        {
            // 遍歷當前目錄下的所有文件和子目錄         
            string[] files = Directory.GetFiles(rootPath);
            string[] subDirs = Directory.GetDirectories(rootPath);
            yield return (rootPath, subDirs, files);
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
    }
}
