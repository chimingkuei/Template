using Basler.Pylon;
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
        public bool save_image;
        public string filepath;

        private void OnImageGrabbed(Object sender, ImageGrabbedEventArgs e)
        {
            try
            {
                // Acquire the image from the camera. Only show the latest image. The camera may acquire images faster than the images can be displayed.

                // Get the grab result.
                IGrabResult grabResult = e.GrabResult;

                // Check if the image can be displayed.
                if (grabResult.IsValid)
                {
                    // Reduce the number of displayed images to a reasonable amount if the camera is acquiring images very fast.
                    if (!stopWatch.IsRunning || stopWatch.ElapsedMilliseconds > 33)
                    {
                        stopWatch.Restart();

                        Bitmap bitmap = new Bitmap(grabResult.Width, grabResult.Height, System.Drawing.Imaging.PixelFormat.Format32bppRgb);
                        // Lock the bits of the bitmap.
                        System.Drawing.Imaging.BitmapData bmpData = bitmap.LockBits(new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height), System.Drawing.Imaging.ImageLockMode.ReadWrite, bitmap.PixelFormat);
                        // Place the pointer to the buffer of the bitmap.
                        converter.OutputPixelFormat = PixelType.BGRA8packed;
                        IntPtr ptrBmp = bmpData.Scan0;
                        converter.Convert(ptrBmp, bmpData.Stride * bitmap.Height, grabResult);
                        bitmap.UnlockBits(bmpData);
                        #region DIP
                        //OpenCvSharp.Mat src = OpenCvSharp.Extensions.BitmapConverter.ToMat(bitmap);
                        //OpenCvSharp.Mat result = src.Threshold(150, 255, OpenCvSharp.ThresholdTypes.Binary);
                        #endregion
                        if (save_image)
                        {
                            bitmap.Save(Path.Combine(filepath, DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + ".bmp"));
                            save_image = false;
                        }
                        // Assign a temporary variable to dispose the bitmap after assigning the new bitmap to the display control.
                        Bitmap bitmapOld = display.Image as Bitmap;
                        // Provide the display control with the new bitmap. This action automatically updates the display.
                        display.Image = bitmap;
                        #region Show DIP Result
                        //Display_Windows.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(result);
                        #endregion
                        if (bitmapOld != null)
                        {
                            // Dispose the bitmap.
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

        public override void SaveImage(string filepath)
        {
            save_image = true;
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
