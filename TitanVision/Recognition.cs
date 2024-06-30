using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TitanVision
{
    public class Recognition
    {
        public OpenCvSharp.Mat Binarization(OpenCvSharp.Mat src, double threshold)
        {
            OpenCvSharp.Mat grayImg = new OpenCvSharp.Mat();
            OpenCvSharp.Cv2.CvtColor(src, grayImg, OpenCvSharp.ColorConversionCodes.BGR2GRAY);
            OpenCvSharp.Mat dst = new OpenCvSharp.Mat();
            OpenCvSharp.Cv2.Threshold(grayImg, dst, threshold, 255, OpenCvSharp.ThresholdTypes.Binary);
            return dst;
        }

        public OpenCvSharp.Mat BoundingBox(OpenCvSharp.Mat src, double threshold)
        {
            OpenCvSharp.Mat binaryImg = Binarization(src, threshold);
            OpenCvSharp.Cv2.FindContours(binaryImg, out OpenCvSharp.Point[][] contours, out OpenCvSharp.HierarchyIndex[] hierarchy, OpenCvSharp.RetrievalModes.Tree, OpenCvSharp.ContourApproximationModes.ApproxSimple);
            OpenCvSharp.Mat dst = src.Clone();
            foreach (var contour in contours)
            {
                OpenCvSharp.Rect boundingRect = OpenCvSharp.Cv2.BoundingRect(contour);
                OpenCvSharp.Cv2.Rectangle(dst, boundingRect, OpenCvSharp.Scalar.Red, 2);
            }
            return dst;
        }


    }
}
