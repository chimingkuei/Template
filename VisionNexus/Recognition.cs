﻿using OpenCvSharp;
using OpenCvSharp.Extensions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using ZXing;
using Point = OpenCvSharp.Point;
using Size = OpenCvSharp.Size;

namespace VisionNexus
{

    public class Recognition : SharpVision
    {
        public Mat Binarization(Mat src, double threshold, bool saveImg = true)
        {
            Mat grayImg = new Mat();
            Cv2.CvtColor(src, grayImg, ColorConversionCodes.BGR2GRAY);
            Mat dst = new Mat();
            Cv2.Threshold(grayImg, dst, threshold, 255, ThresholdTypes.Binary);
            if (saveImg)
                Cv2.ImWrite(Path.Combine(outputFolder, Path.GetFileNameWithoutExtension(fileName) + "_Binarization" + fileExtension), dst);
            return dst;
        }

        /// <summary>
        /// The example of a Scalar parameter is new Scalar(x, x, x).
        /// </summary>
        public Mat ExtractHSVColor(Mat src, Scalar lowerColor, Scalar upperColor, Scalar background = default, bool saveImg = true)
        {
            Mat hsv = new Mat();
            Cv2.CvtColor(src, hsv, ColorConversionCodes.BGR2HSV);
            Mat mask = new Mat();
            Cv2.InRange(hsv, lowerColor, upperColor, mask);
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
            if (saveImg)
                Cv2.ImWrite(Path.Combine(outputFolder, Path.GetFileNameWithoutExtension(fileName) + "_ExtractHSVColor" + fileExtension), result);
            return result;
        }

        public Mat EqualizeHist(Mat src, bool saveImg = true)
        {
            Mat grayImage = new Mat();
            Cv2.CvtColor(src, grayImage, ColorConversionCodes.BGR2GRAY);
            Mat dst = new Mat();
            Cv2.EqualizeHist(grayImage, dst);
            if (saveImg)
                Cv2.ImWrite(Path.Combine(outputFolder, Path.GetFileNameWithoutExtension(fileName) + "_EqualizeHist" + fileExtension), dst);
            return dst;
        }

        public double CalculateGrayAverage(Mat src)
        {
            Mat grayImage = new Mat();
            Cv2.CvtColor(src, grayImage, ColorConversionCodes.BGR2GRAY);
            Scalar meanValue = Cv2.Mean(grayImage);
            return meanValue.Val0;
        }

        public Mat GrabRect(Mat src, double threshold, bool saveImg = true)
        {
            Mat binaryImg = Binarization(src, threshold);
            Cv2.FindContours(binaryImg, out Point[][] contours, out HierarchyIndex[] hierarchy, RetrievalModes.Tree, ContourApproximationModes.ApproxSimple);
            Mat dst = src.Clone();
            foreach (var contour in contours)
            {
                Rect boundingRect = Cv2.BoundingRect(contour);
                Cv2.Rectangle(dst, boundingRect, Scalar.Red, 2);
            }
            if (saveImg)
                Cv2.ImWrite(Path.Combine(outputFolder, Path.GetFileNameWithoutExtension(fileName) + "_GrabRect" + fileExtension), dst);
            return dst;
        }

        public void VerticalProjection(Mat src, double threshold, string filepath = null)
        {
            Mat binaryImg = Binarization(src, threshold);
            int[] verticalProjection = new int[binaryImg.Cols];
            for (int i = 0; i < binaryImg.Cols; i++)
            {
                Mat col = binaryImg.Col(i);
                int nonZeroCount = Cv2.CountNonZero(col);
                verticalProjection[i] = binaryImg.Rows - nonZeroCount;
            }
            if (filepath!=null)
            {
                Mat projectionImage = new Mat(new Size(binaryImg.Cols, binaryImg.Rows), MatType.CV_8UC3, Scalar.White);
                for (int j = 0; j < binaryImg.Cols; j++)
                {
                    int startPoint = projectionImage.Rows;
                    int endPoint = projectionImage.Rows - verticalProjection[j];
                    Cv2.Line(projectionImage, new Point(j, startPoint), new Point(j, endPoint), Scalar.Black);
                }
                Cv2.ImWrite(filepath, projectionImage);
            }
        }

        public void HorizontalProjection(Mat src, double threshold, string filepath = null)
        {
            Mat binaryImg = Binarization(src, threshold);
            int[] horizontalProjection = new int[binaryImg.Rows];
            for (int i = 0; i < binaryImg.Rows; i++)
            {
                Mat row = binaryImg.Row(i);
                int nonZeroCount = Cv2.CountNonZero(row);
                horizontalProjection[i] = binaryImg.Cols - nonZeroCount;
            }
            if (filepath != null)
            {
                Mat projectionImage = new Mat(new Size(binaryImg.Cols, binaryImg.Rows), MatType.CV_8UC3, Scalar.White);
                for (int j = 0; j < binaryImg.Rows; j++)
                {
                    int startPoint = projectionImage.Cols;
                    int endPoint = projectionImage.Cols - horizontalProjection[j];
                    Cv2.Line(projectionImage, new Point(startPoint, j), new Point(endPoint, j), Scalar.Black);
                }
                Cv2.ImWrite(filepath, projectionImage);
            }
        }

        







    }
}
