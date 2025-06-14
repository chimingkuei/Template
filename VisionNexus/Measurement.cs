using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Point = OpenCvSharp.Point;

namespace VisionNexus
{
    public class Measurement : SharpVision
    {
        /// <summary>
        /// slash or backslash length = magnify_length * 2 + 1, Unit:px
        /// </summary>
        public void DrawCross(Mat src, Point p, Scalar color, int thickness, int magnifyLength)
        {
            Point backslash_p1 = new Point(Convert.ToInt32(p.X + (Math.Sqrt(2) * Math.Cos(Math.PI / 4) * magnifyLength)), Convert.ToInt32(p.Y + (Math.Sqrt(2) * Math.Sin(Math.PI / 4) * magnifyLength)));
            Point backslash_p2 = new Point(Convert.ToInt32(p.X + (Math.Sqrt(2) * Math.Cos(Math.PI * 5 / 4) * magnifyLength)), Convert.ToInt32(p.Y + (Math.Sqrt(2) * Math.Sin(Math.PI * 5 / 4) * magnifyLength)));
            Cv2.Line(src, backslash_p1, backslash_p2, color, thickness);
            Point slash_p1 = new Point(Convert.ToInt32(p.X + (Math.Sqrt(2) * Math.Cos(Math.PI * 3 / 4) * magnifyLength)), Convert.ToInt32(p.Y + (Math.Sqrt(2) * Math.Sin(Math.PI * 3 / 4) * magnifyLength)));
            Point slash_p2 = new Point(Convert.ToInt32(p.X + (Math.Sqrt(2) * Math.Cos(Math.PI * 7 / 4) * magnifyLength)), Convert.ToInt32(p.Y + (Math.Sqrt(2) * Math.Sin(Math.PI * 7 / 4) * magnifyLength)));
            Cv2.Line(src, slash_p1, slash_p2, color, thickness);
        }

        public Mat ConvertBinaryInv(Mat img, double threshold)
        {
            Mat grayImg = new Mat();
            Cv2.CvtColor(img, grayImg, ColorConversionCodes.BGR2GRAY);
            Mat binaryInv = new Mat();
            Cv2.Threshold(grayImg, binaryInv, threshold, 255, ThresholdTypes.BinaryInv);
            return binaryInv;
        }

        #region Fit Line
        private bool IsWhite(Mat src, Point p)
        {
            if (p.X < 0 || p.X >= src.Cols || p.Y < 0 || p.Y >= src.Rows)
                return false;
            return src.At<byte>(p.Y, p.X) == 255;
        }

        private Point? FLNearestWhite(Mat src, Point start, int nx, int ny, int maxDist, bool direction)
        {
            int x = start.X;
            int y = start.Y;
            int step = direction ? 1 : -1;
            for (int d = 1; d <= maxDist; d++)
            {
                x = start.X + d * nx * step;
                y = start.Y + d * ny * step;
                if (x < 0 || x >= src.Width || y < 0 || y >= src.Height)
                    break;
                Point p = new Point(x, y);
                if (IsWhite(src, p))
                    return p;
            }
            return null;
        }

        private (Point startPoint, Point endPoint) FitLine(List<Point> points, Mat src, bool extendToBorders)
        {
            // 轉換影像座標點為數學坐標系 (Y = img.Height - Y)
            var x = points.Select(p => (double)p.X).ToArray();
            var y = points.Select(p => (double)(src.Height - p.Y)).ToArray();
            // 使用 MathNet.Numerics 進行最小二乘法擬合直線
            var result = Fit.Line(x, y);
            double intercept = result.Item1; // 截距
            double slope = result.Item2; // 斜率
            // 輸出斜率和截距
            Console.WriteLine($"斜率: {slope}");
            Console.WriteLine($"截距: {intercept}");
            // 計算 x 範圍
            double xMin, xMax;
            if (extendToBorders)
            {
                // 影像的左右邊界
                xMin = 0;
                xMax = src.Width - 1;
            }
            else
            {
                // 只在輸入點的範圍內畫線
                xMin = x.Min();
                xMax = x.Max();
            }
            // 根據擬合的直線公式計算對應的 y 值
            double yMinMath = slope * xMin + intercept;
            double yMaxMath = slope * xMax + intercept;
            // 轉換數學座標系的 y 值回影像座標系 (Y = img.Height - Y)
            double yMin = src.Height - yMinMath;
            double yMax = src.Height - yMaxMath;
            // 確保 y 值在影像範圍內
            yMin = Math.Max(0, Math.Min(yMin, src.Height - 1));
            yMax = Math.Max(0, Math.Min(yMax, src.Height - 1));
            return (new Point((int)xMin, (int)yMin), new Point((int)xMax, (int)yMax));
        }

        public void FLByWhiteDot(Mat src, int threshold, Tuple<Point, Point> line, int stepSize, int maxDist, bool direction, bool saveImg = true)
        {
            Mat binaryInv = ConvertBinaryInv(src, threshold);
            if (saveImg)
                Cv2.ImWrite(Path.Combine(outputFolder, Path.GetFileNameWithoutExtension(fileName) + "_binaryInv" + fileExtension), binaryInv);
            List<Point> fitLinePoints = new List<Point>();
            Point p1 = line.Item1;
            Point p2 = line.Item2;
            int dx = p2.X - p1.X;
            int dy = p2.Y - p1.Y;
            double length = Math.Sqrt(dx * dx + dy * dy);
            // 計算法向量（確保方向正確）
            double nxf = -dy / length;
            double nyf = dx / length;
            int nx = (int)Math.Round(nxf);
            int ny = (int)Math.Round(nyf);
            // 沿著線段取樣
            int steps = (int)(length / stepSize);
            for (int i = 0; i <= steps; i++)
            {
                int x = p1.X + i * dx / steps;
                int y = p1.Y + i * dy / steps;
                Point start = new Point(x, y);
                // 標記採樣點
                if (saveImg)
                    Cv2.Circle(src, start, 2, Scalar.Green, -1);
                // 前進尋找白點
                Point? whitePoint = FLNearestWhite(binaryInv, start, nx, ny, maxDist, direction);
                if (whitePoint.HasValue)
                {
                    fitLinePoints.Add(whitePoint.Value);
                    Cv2.Circle(src, whitePoint.Value, 3, Scalar.Red, -1);
                }
                Point end = new Point(start.X + (direction ? 1 : -1) * maxDist * nx,
                                      start.Y + (direction ? 1 : -1) * maxDist * ny);
                // 畫出搜尋路徑 (黃色)
                if (saveImg)
                    Cv2.Line(src, start, end, Scalar.Yellow, 1);
            }
            if (fitLinePoints.Count < 2)
            {
                Console.WriteLine("Not enough points to fit a line.");
                return;
            }
            (Point startPoint, Point endPoint) = FitLine(fitLinePoints, src, direction);
            if (saveImg)
            {
                Cv2.Line(src, startPoint, endPoint, Scalar.Blue, 2);
                Cv2.ImWrite(Path.Combine(outputFolder, Path.GetFileNameWithoutExtension(fileName) + "_Result" + fileExtension), src);
            }
        }
        #endregion

        #region Fit Circle
        private double Cal2PtDist(PointF p1, PointF p2)
        {
            return Math.Sqrt(Math.Pow(p1.X - p2.X, 2) + Math.Pow(p1.Y - p2.Y, 2));
        }

        private void FitCircle(List<PointF> pointFs, out float CenterX, out float CenterY, out float CenterR)
        {
            Matrix<float> YMat;
            Matrix<float> RMat;
            Matrix<float> AMat;
            List<float> YLit = new List<float>();
            List<float[]> RLit = new List<float[]>();
            // 構建Y矩陣
            foreach (var pointF in pointFs)
                YLit.Add(pointF.X * pointF.X + pointF.Y * pointF.Y);
            float[,] Yarray = new float[YLit.Count, 1];
            for (int i = 0; i < YLit.Count; i++)
                Yarray[i, 0] = YLit[i];
            YMat = CreateMatrix.DenseOfArray<float>(Yarray);
            // 構建R矩陣
            foreach (var pointF in pointFs)
                RLit.Add(new float[] { -pointF.X, -pointF.Y, -1 });
            float[,] Rarray = new float[RLit.Count, 3];
            for (int i = 0; i < RLit.Count; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    Rarray[i, j] = RLit[i][j];
                }
            }
            RMat = CreateMatrix.DenseOfArray<float>(Rarray);
            Matrix<float> RTMat = RMat.Transpose();
            Matrix<float> RRTInvMat = (RTMat.Multiply(RMat)).Inverse();
            AMat = RRTInvMat.Multiply(RTMat.Multiply(YMat));
            float[,] Aarray = AMat.ToArray();
            float A = Aarray[0, 0];
            float B = Aarray[1, 0];
            float C = Aarray[2, 0];
            CenterX = A / -2.0f;
            CenterY = B / -2.0f;
            CenterR = (float)(Math.Sqrt((A * A + B * B - 4 * C)) / 2.0f);
        }

        /// <summary>
        /// k值調整：較小k值（例如k = 1.0），過濾更多點；較大k值（例如k = 3.0）會保留更多點，只過濾掉極端離群點。
        /// 過濾機制：若某個點與其它點的平均距離大於mean + k * std，則該點被視為離群點並被剔除。
        /// </summary>
        private List<PointF> FCOutlier(List<PointF> points, double k)
        {
            // 計算每個點與其他點的距離
            List<double> distances = new List<double>();
            for (int i = 0; i < points.Count; i++)
            {
                for (int j = i + 1; j < points.Count; j++)
                {
                    double distance = Cal2PtDist(points[i], points[j]);
                    distances.Add(distance);
                }
            }
            // 計算平均距離和標準差
            double meanDistance = distances.Average();
            double stdDevDistance = Math.Sqrt(distances.Average(d => Math.Pow(d - meanDistance, 2)));
            // 計算閾值
            double threshold = meanDistance + k * stdDevDistance;
            // 過濾離群點，根據與其他點的平均距離來剔除
            var filteredPoints = points.Where(p =>
            {
                // 計算當前點與其他點的平均距離
                double avgDistance = points.Where(other => !other.Equals(p))
                                            .Average(other => Cal2PtDist(p, other));
                return avgDistance <= threshold;  // 只保留在閾值內的點
            }).ToList();
            return filteredPoints;
        }

        public bool FCByWhiteDot(Mat src, int threshold, Point center, int Radius, bool direction, double k, bool saveImg = true)
        {
            Mat binaryInv = ConvertBinaryInv(src, threshold);
            if (saveImg)
                Cv2.ImWrite(Path.Combine(outputFolder, Path.GetFileNameWithoutExtension(fileName) + "_binaryInv" + fileExtension), binaryInv);
            List<PointF> fitCirclePoints = new List<PointF>();
            List<PointF> filterCirclePoints = new List<PointF>();
            for (int angle = 1; angle <= 360; angle++)
            {
                int start = direction ? 1 : Radius;
                int end = direction ? Radius : 1;
                int step = direction ? 1 : -1;
                for (int i = start; direction ? i <= end : i >= end; i += step)
                {
                    int x = Convert.ToInt32(center.X + i * Math.Cos((Math.PI / 180) * angle));
                    int y = Convert.ToInt32(center.Y - i * Math.Sin((Math.PI / 180) * angle));
                    if (binaryInv.At<byte>(y, x) == 255) // color use At<Vec3b>(y, x)
                    {
                        fitCirclePoints.Add(new PointF(x, src.Height - y)); // 轉換為笛卡爾座標
                        break;
                    }
                }
            }
            filterCirclePoints = FCOutlier(fitCirclePoints, k);
            if (filterCirclePoints.Count >= 3)
            {
                if (saveImg)
                {
                    // 在影像上標記過濾後的點(轉回影像座標)
                    foreach (var point in filterCirclePoints)
                    {
                        int imgX = (int)point.X;
                        int imgY = src.Height - (int)point.Y;
                        Cv2.Circle(src, imgX, imgY, 1, Scalar.Blue, 1);
                    }
                }
                float CenterX, CenterY, CenterR;
                FitCircle(filterCirclePoints, out CenterX, out CenterY, out CenterR);
                if (saveImg)
                {
                    // 轉回影像座標
                    int imgCenterY = src.Height - (int)CenterY;
                    Cv2.Circle(src, new Point((int)CenterX, imgCenterY), 5, Scalar.Green, -1);
                    Cv2.Circle(src, new Point((int)CenterX, imgCenterY), (int)CenterR, Scalar.Red, 1);
                    Cv2.ImWrite(Path.Combine(outputFolder, Path.GetFileNameWithoutExtension(fileName) + "_Result" + fileExtension), src);
                }
                return true;
            }
            else
            {
                return false;
            }
        }
        #endregion

    }
}
