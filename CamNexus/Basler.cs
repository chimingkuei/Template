using Basler.Pylon;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CamNexus
{
    public class Basler : Cam
    {
        public Camera camera = null;
        private PixelDataConverter converter = new PixelDataConverter();
        private Stopwatch stopWatch = new Stopwatch();
        public PictureBox display = new PictureBox();
        public Mat image { get; set; }
        private ImageFormat ImageFormatType { get; set; }

        private void OnImageGrabbed(Object sender, ImageGrabbedEventArgs e)
        {
            try
            {
                IGrabResult grabResult = e.GrabResult;
                if (grabResult.IsValid)
                {
                    if (!stopWatch.IsRunning || stopWatch.ElapsedMilliseconds > 33)
                    {
                        stopWatch.Restart();
                        Mat mat = null;
                        switch (ImageFormatType)
                        {
                            case ImageFormat.BGR8:
                                {
                                    mat = new Mat(grabResult.Height, grabResult.Width, MatType.CV_8UC3);
                                    converter.OutputPixelFormat = PixelType.BGR8packed;
                                    break;
                                }
                            case ImageFormat.Mono8:
                                {
                                    mat = new Mat(grabResult.Height, grabResult.Width, MatType.CV_8UC1);
                                    converter.OutputPixelFormat = PixelType.Mono8;
                                    break;
                                }
                        }
                        IntPtr ptrMat = mat.Data;
                        converter.Convert(ptrMat, mat.Step() * mat.Rows, grabResult);
                        image = mat;
                        Bitmap bitmapOld = display.Image as Bitmap;
                        display.Image = mat.ToBitmap();
                        if (bitmapOld != null)
                        {
                            bitmapOld.Dispose();
                        }





                    }
                }
            }
            catch (Exception exception)
            {
                ShowException(exception);
            }
            finally
            {
                // Dispose the grab result if needed for returning it to the grab loop.
                e.DisposeGrabResultIfClone();
            }
        }

        private void ShowException(Exception exception)
        {
            Console.WriteLine("Exception caught:\n" + exception.Message, "Error");
        }

        public override void CameraInit(int CamID)
        {
            try
            {
                camera = new Camera();
                camera.StreamGrabber.ImageGrabbed += OnImageGrabbed;
                camera.CameraOpened += Configuration.AcquireContinuous;
                //state = true;
            }
            catch (Exception exception)
            {
                ShowException(exception);
                //state = false;
            }
        }

        public void OpenCamera()
        {
            Console.WriteLine("Using camera {0}.", camera.CameraInfo[CameraInfoKey.ModelName]);
            camera.Open();
            Console.WriteLine("Camera Width {0}.", camera.Parameters[PLCamera.Width]);
            Console.WriteLine("Camera Height {0}.", camera.Parameters[PLCamera.Height]);
        }

        public override void ContinueAcquisition()
        {
            try
            {
                // Start the grabbing of images until grabbing is stopped.
                Configuration.AcquireContinuous(camera, null);
                camera.StreamGrabber.Start(GrabStrategy.OneByOne, GrabLoop.ProvidedByStreamGrabber);
            }
            catch (Exception exception)
            {
                ShowException(exception);
            }
        }

        public override void StopAcquisition()
        {
            // Stop the grabbing.
            try
            {
                camera.StreamGrabber.Stop();
                display.Image = null;
            }
            catch (Exception exception)
            {
                ShowException(exception);
            }
        }

        public void SetGamma(double value)
        {
            camera.Parameters[PLCamera.Gamma].SetValue(value);
        }

        public void SetExposure(double value)
        {
            camera.Parameters[PLCamera.ExposureTime].SetValue(value);
        }

        public void SetGain(double value)
        {
            camera.Parameters[PLCamera.Gain].SetValue(value);
        }

        public void GetGammaRange()
        {
            double gamma_min = camera.Parameters[PLCamera.Gamma].GetMinimum();
            double gamma_max = camera.Parameters[PLCamera.Gamma].GetMaximum();
            Console.WriteLine(gamma_min);
            Console.WriteLine(gamma_max);
        }

        public void GetExposureRange()
        {
            double exposure_min = camera.Parameters[PLCamera.ExposureTime].GetMinimum();
            double exposure_max = camera.Parameters[PLCamera.ExposureTime].GetMaximum();
            Console.WriteLine(exposure_min);
            Console.WriteLine(exposure_max);
        }

        public void GetGainRange()
        {
            double gain_min = camera.Parameters[PLCamera.Gain].GetMinimum();
            double gain_max = camera.Parameters[PLCamera.Gain].GetMaximum();
            Console.WriteLine(gain_min);
            Console.WriteLine(gain_max);
        }

    }
}
