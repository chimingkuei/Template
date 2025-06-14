using Basler.Pylon;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CamNexus
{
    public class Webcam
    {
        private OpenCvSharp.VideoCapture capture { get; set; }
        public OpenCvSharp.Mat frame { get; set; }
        private bool camera_state { get; set; }

        public void OpenCamera(int CamID, PictureBox display_windows)
        {
            Task.Run(() =>
            {
                frame = new OpenCvSharp.Mat();
                capture = new OpenCvSharp.VideoCapture(CamID);
                capture.Set(VideoCaptureProperties.FrameWidth, 1280);
                capture.Set(VideoCaptureProperties.FrameHeight, 720);
                if (!capture.IsOpened())
                {
                    Console.WriteLine("開啟相機失敗!");
                    return;
                }
                camera_state = true;
                while (true)
                {
                    bool read_status = capture.Read(frame);
                    //防止狀態切換太快，讀到空值
                    if (frame.Height == 0) continue;
                    display_windows.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(frame);
                }
            });
        }

        public void SaveImage(string filepath)
        {
            if (camera_state)
            {
                Cv2.ImWrite(filepath, frame);
            }
            else
            {
                Console.WriteLine("請先開啟相機!");
            }

        }


    }
}
