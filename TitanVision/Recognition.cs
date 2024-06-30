using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace TitanVision
{
    public class Recognition
    {
        public Mat Binarization(Mat src, double threshold)
        {
            Mat grayImg = new Mat();
            Cv2.CvtColor(src, grayImg, ColorConversionCodes.BGR2GRAY);
            Mat dst = new Mat();
            Cv2.Threshold(grayImg, dst, threshold, 255, ThresholdTypes.Binary);
            return dst;
        }

        /// <summary>
        /// The example of a scalar input parameter is new Scalar(0, 0, 0).
        /// </summary>
        public Mat ExtractHSVColor(Mat src, Scalar lowercolor, Scalar uppercolor, Scalar background = default)
        {
            Mat hsv = new Mat();
            Cv2.CvtColor(src, hsv, ColorConversionCodes.BGR2HSV);
            Mat mask = new Mat();
            Cv2.InRange(hsv, lowercolor, uppercolor, mask);
            Mat result = new Mat();
            if (background != default)
            {
                Mat greenBackground = new Mat(src.Size(), MatType.CV_8UC3, new Scalar(0, 255, 0));
                src.CopyTo(result, mask);
                greenBackground.CopyTo(result, ~mask);
            }
            else
            {
                Cv2.BitwiseAnd(src, src, result, mask);
            }
            return result;
        }

        public Mat EqualizeHist(Mat src)
        {
            Mat grayImage = new Mat();
            Cv2.CvtColor(src, grayImage, ColorConversionCodes.BGR2GRAY);
            Mat dst = new Mat();
            Cv2.EqualizeHist(grayImage, dst);
            return dst;
        }

        public Mat BoundingBox(Mat src, double threshold)
        {
            Mat binaryImg = Binarization(src, threshold);
            Cv2.FindContours(binaryImg, out Point[][] contours, out HierarchyIndex[] hierarchy, RetrievalModes.Tree, ContourApproximationModes.ApproxSimple);
            Mat dst = src.Clone();
            foreach (var contour in contours)
            {
                Rect boundingRect = Cv2.BoundingRect(contour);
                Cv2.Rectangle(dst, boundingRect, Scalar.Red, 2);
            }
            return dst;
        }

       


    }
}
