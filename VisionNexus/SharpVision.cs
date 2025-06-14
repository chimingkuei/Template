using OpenCvSharp;
using OpenCvSharp.Extensions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisionNexus
{
    /// <summary>
    /// items.Remove(new Item { X = 10, Y = 20, Width = 50, Length = 30 });
    /// items.Add(new Item { X = 10, Y = 20, Width = 50, Length = 30 });
    /// </summary>
    public class Box
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Length { get; set; }

        public override bool Equals(object obj)
        {
            if (obj is Box other)
            {
                return X == other.X && Y == other.Y && Width == other.Width && Length == other.Length;
            }
            return false;
        }

        public override int GetHashCode()
        {
            unchecked // 防止整數溢出
            {
                int hash = 17; // 初始值
                hash = hash * 23 + X.GetHashCode(); // 計算 X 的哈希碼
                hash = hash * 23 + Y.GetHashCode(); // 計算 Y 的哈希碼
                hash = hash * 23 + Width.GetHashCode(); // 計算 Width 的哈希碼
                hash = hash * 23 + Length.GetHashCode(); // 計算 Length 的哈希碼
                return hash;
            }
        }

        public override string ToString()
        {
            return $"Position: ({X}, {Y}), Width: {Width}, Length: {Length}";
        }
    }

    interface IFileManager
    {
        void BatchTransform(string inputFolder, double threshold, Func<Mat, double, Mat> fun);
    }

    public abstract class SharpVision : IFileManager
    {
        public string outputFolder;
        public string fileName;
        public string fileExtension;

        public void BatchTransform(string inputFolder, double threshold, Func<Mat, double, Mat> fun)
        {
            string[] imageFiles = Directory.GetFiles(inputFolder, fileExtension);
            foreach (string imageFile in imageFiles)
            {
                Mat image = Cv2.ImRead(imageFile, ImreadModes.Color);
                Mat dst = fun(image, threshold);
                Cv2.ImWrite(Path.Combine(outputFolder, fileName + fileExtension), dst);
            }
        }
    }

    
}
