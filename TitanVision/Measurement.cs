using MathNet.Numerics.LinearAlgebra;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TitanVision
{
    public class Measurement
    {
        /// <summary>
        /// The example of a Point input parameter is new OpenCvSharp.Point(x, x).
        /// <returns></returns>
        public double CalculateDistance(Point p1, Point p2)
        {
            int deltaX = p1.X - p2.X;
            int deltaY = p1.Y - p2.Y;
            return Math.Sqrt(deltaX * deltaX + deltaY * deltaY);
        }

        /// <summary>
        /// slash or backslash length = magnify_length * 2 + 1, Unit:px
        /// </summary>
        public void DrawCross(Mat src, Point p, Scalar color, int thickness, int magnify_length)
        {
            Point backslash_p1 = new Point(Convert.ToInt32(p.X + (Math.Sqrt(2) * Math.Cos(Math.PI / 4) * magnify_length)), Convert.ToInt32(p.Y + (Math.Sqrt(2) * Math.Sin(Math.PI / 4) * magnify_length)));
            Point backslash_p2 = new Point(Convert.ToInt32(p.X + (Math.Sqrt(2) * Math.Cos(Math.PI * 5 / 4) * magnify_length)), Convert.ToInt32(p.Y + (Math.Sqrt(2) * Math.Sin(Math.PI * 5 / 4) * magnify_length)));
            Cv2.Line(src, backslash_p1, backslash_p2, color, thickness);
            Point slash_p1 = new Point(Convert.ToInt32(p.X + (Math.Sqrt(2) * Math.Cos(Math.PI * 3 / 4) * magnify_length)), Convert.ToInt32(p.Y + (Math.Sqrt(2) * Math.Sin(Math.PI * 3 / 4) * magnify_length)));
            Point slash_p2 = new Point(Convert.ToInt32(p.X + (Math.Sqrt(2) * Math.Cos(Math.PI * 7 / 4) * magnify_length)), Convert.ToInt32(p.Y + (Math.Sqrt(2) * Math.Sin(Math.PI * 7 / 4) * magnify_length)));
            Cv2.Line(src, slash_p1, slash_p2, color, thickness);
        }

        public void FitCircle(List<System.Drawing.PointF> pointFs, out float CenterX, out float CenterY, out float CenterR)
        {
            Matrix<float> YMat;
            Matrix<float> RMat;
            Matrix<float> AMat;
            List<float> YLit = new List<float>();
            List<float[]> RLit = new List<float[]>();
            //------构建Y矩阵
            foreach (var pointF in pointFs)
                YLit.Add(pointF.X * pointF.X + pointF.Y * pointF.Y);
            float[,] Yarray = new float[YLit.Count, 1];
            for (int i = 0; i < YLit.Count; i++)
                Yarray[i, 0] = YLit[i];
            YMat = CreateMatrix.DenseOfArray<float>(Yarray);

            //构建R矩阵
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

        public List<System.Drawing.PointF> RobustCircleFit(List<System.Drawing.PointF> pointFs, double tolerance)
        {
            List<System.Drawing.PointF> pointFs_without_outliers = new List<System.Drawing.PointF>();
            float CenterX;
            float CenterY;
            float CenterR;
            FitCircle(pointFs, out CenterX, out CenterY, out CenterR);
            for (int i = 0; i < pointFs.Count; i++)
            {
                if (Math.Abs(CalculateDistance(new Point(pointFs[i].X, pointFs[i].Y), new Point(CenterX, CenterY)) - CenterR) < tolerance)
                {
                    pointFs_without_outliers.Add(pointFs[i]);
                }
            }
            return pointFs_without_outliers;
        }

        public bool FitCircleBy255(Mat src, Point center, int Radius, bool direction, double tolerance)
        {
            //direction-->true:由內而外找白點;false:由外而內找白點
            List<System.Drawing.PointF> CircleFitLit = new List<System.Drawing.PointF>();
            if (direction)
            {
                for (int angle = 1; angle <= 360; angle++)
                {
                    for (int i = 1; i <= Radius; i++)
                    {
                        int x = Convert.ToInt32(center.X + i * Math.Cos((Math.PI / 180) * angle));
                        int y = Convert.ToInt32(center.Y - i * Math.Sin((Math.PI / 180) * angle));
                        if (src.At<byte>(y, x) == 255)//color use At<Vec3b>(y, x)
                        {
                            CircleFitLit.Add(new System.Drawing.PointF(x, y));
                            break;
                        }
                    }
                }
            }
            else
            {
                for (int angle = 1; angle <= 360; angle++)
                {
                    for (int i = Radius; i >= 1; i--)
                    {
                        int x = Convert.ToInt32(center.X + i * Math.Cos((Math.PI / 180) * angle));
                        int y = Convert.ToInt32(center.Y - i * Math.Sin((Math.PI / 180) * angle));
                        if (src.At<byte>(y, x) == 255)//color use At<Vec3b>(y, x)
                        {
                            CircleFitLit.Add(new System.Drawing.PointF(x, y));
                            break;
                        }
                    }
                }
            }
            if (CircleFitLit.Count >= 3)
            {
                CircleFitLit = RobustCircleFit(CircleFitLit, tolerance);
                if (CircleFitLit.Count >= 3)
                {
                    foreach (var point in CircleFitLit)
                    {
                        Cv2.Circle(src, Convert.ToInt32(point.X), Convert.ToInt32(point.Y), 1, Scalar.Blue, -1);
                    }
                    float CenterX;
                    float CenterY;
                    float CenterR;
                    FitCircle(CircleFitLit, out CenterX, out CenterY, out CenterR);
                    Cv2.Circle(src, new Point((int)CenterX, (int)CenterY), 5, Scalar.Green, -1);
                    Cv2.Circle(src, new Point((int)CenterX, (int)CenterY), (int)CenterR, Scalar.Red, 1);
                    Cv2.ImWrite(@"Result.bmp", src);
                    return true;
                }
                else
                {
                    return false;
                }

            }
            else
            {
                return false;
            }
        }

        public double CalcSlope(Point p1, Point p2)
        {
            double slope = -1;
            if (p1.X == p2.X)
            {
                Console.WriteLine("斜率不存在!");
            }
            else
            {
                slope = (p2.Y - p1.Y) / (p2.X - p1.X);
            }
            return slope;
        }

        /// <summary>
        /// Create the equation mx - y + y0 - mx0 = 0 from two points p1 and p2.
        /// </summary>
        public Tuple<double, double, double> CreateLinearEquation(Point p1, Point p2)
        {
            double m = CalcSlope(p1, p2);
            double c = p1.Y - m * p1.X;
            return Tuple.Create(m, -1.0, c);
        }

        /// <summary>
        /// Calculate the distance from the point p1 to the LE : ax + by + c = 0.
        /// </summary>
        public double DistanceFromPointToLine(Point p1, Tuple<double, double, double> LE)
        {
            return Math.Abs(LE.Item1 * p1.X + LE.Item2 * p1.Y + LE.Item3) / Math.Sqrt(LE.Item1 * LE.Item1 + LE.Item2 * LE.Item2);
        }



    }
}
