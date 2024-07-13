using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TitanVision
{
    interface IFileManager
    {
        void BatchImageTransform(string folderpath, string file_extension, double threshold, Func<Mat, double, Mat> fun);
    }

    public abstract class SharpVision : IFileManager
    {
        public void BatchImageTransform(string folderpath, string file_extension, double threshold, Func<Mat, double, Mat> fun)
        {
            string[] imageFiles = Directory.GetFiles(folderpath, file_extension);
            foreach (string imageFile in imageFiles)
            {
                Mat image = Cv2.ImRead(imageFile, ImreadModes.Color);
                Mat dst = fun(image, threshold);
                Cv2.ImWrite(Path.Combine(@"E:\DIP Temp\Image Output", DateTime.Now.ToString("yyyyMMddHHmmssfff") + ".bmp"), dst);
            }
        }
    }
}
