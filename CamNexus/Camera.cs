using Basler.Pylon;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CamNexus
{
    public abstract class Cam
    {
        abstract public void CameraInit(int CamID);
        abstract public void ContinueAcquisition();
        abstract public void StopAcquisition();
        abstract public void SaveImage(string filepath);
    }

    public class IDSUI:Cam
    {
        private uEye.Camera m_Camera;
        private bool m_bLive = false;
        public IntPtr m_displayHandle = IntPtr.Zero;
        private const int m_cnNumberOfSeqBuffers = 3;

        private uEye.Defines.Status AllocImageMems()
        {
            uEye.Defines.Status statusRet = uEye.Defines.Status.SUCCESS;

            for (int i = 0; i < m_cnNumberOfSeqBuffers; i++)
            {
                statusRet = m_Camera.Memory.Allocate();

                if (statusRet != uEye.Defines.Status.SUCCESS)
                {
                    FreeImageMems();
                }
            }

            return statusRet;
        }
        private uEye.Defines.Status FreeImageMems()
        {
            int[] idList;
            uEye.Defines.Status statusRet = m_Camera.Memory.GetList(out idList);

            if (uEye.Defines.Status.SUCCESS == statusRet)
            {
                foreach (int nMemID in idList)
                {
                    do
                    {
                        statusRet = m_Camera.Memory.Free(nMemID);

                        if (uEye.Defines.Status.SEQ_BUFFER_IS_LOCKED == statusRet)
                        {
                            Thread.Sleep(1);
                            continue;
                        }

                        break;
                    }
                    while (true);
                }
            }

            return statusRet;
        }
        private uEye.Defines.Status InitSequence()
        {
            int[] idList;
            uEye.Defines.Status statusRet = m_Camera.Memory.GetList(out idList);

            if (uEye.Defines.Status.SUCCESS == statusRet)
            {
                statusRet = m_Camera.Memory.Sequence.Add(idList);

                if (uEye.Defines.Status.SUCCESS != statusRet)
                {
                    ClearSequence();
                }
            }

            return statusRet;
        }
        private uEye.Defines.Status ClearSequence()
        {
            return m_Camera.Memory.Sequence.Clear();
        }
        private void OnFrameEvent(object sender, EventArgs e)
        {
            uEye.Camera Camera = sender as uEye.Camera;

            Int32 s32MemID;
            uEye.Defines.Status statusRet = Camera.Memory.GetLast(out s32MemID);

            if ((uEye.Defines.Status.SUCCESS == statusRet) && (0 < s32MemID))
            {
                if (uEye.Defines.Status.SUCCESS == Camera.Memory.Lock(s32MemID))
                {
                    Camera.Display.Render(s32MemID, m_displayHandle, uEye.Defines.DisplayRenderMode.FitToWindow);
                    Camera.Memory.Unlock(s32MemID);
                }
            }
        }

        public override void CameraInit(int CamID)
        {
            m_Camera = new uEye.Camera();
            uEye.Defines.Status statusRet = 0;
            // Open Camera
            statusRet = m_Camera.Init(CamID);
            if (statusRet != uEye.Defines.Status.Success)
            {
                Console.WriteLine("Camera initializing failed");
                Environment.Exit(-1);
            }
            //if (File.Exists(camera_parameter_file))
            //{
            //    //Load Camera Parameter File
            //    statusRet = m_Camera.Parameter.Load(camera_parameter_file);
            //    if (statusRet != uEye.Defines.Status.Success)
            //    {
            //        MessageBox.Show("Loading parameter failed: " + statusRet);
            //    }
            //}
            //else
            //{
            //    MessageBox.Show("Please confirm whether the camera file exists!");
            //}
            // Allocate Memory
            statusRet = AllocImageMems();
            if (statusRet != uEye.Defines.Status.Success)
            {
                Console.WriteLine("Allocate Memory failed");
                Environment.Exit(-1);
            }
            statusRet = InitSequence();
            if (statusRet != uEye.Defines.Status.Success)
            {
                Console.WriteLine("Add to sequence failed");
                Environment.Exit(-1);
            }
            // Start Live Video
            statusRet = m_Camera.Acquisition.Capture();
            if (statusRet != uEye.Defines.Status.Success)
            {
                Console.WriteLine("Start Live Video failed");
            }
            else
            {
                m_bLive = true;
            }
            // Connect Event
            m_Camera.EventFrame += OnFrameEvent;
        }

        public override void ContinueAcquisition()
        {
            // Open Camera and Start Live Video
            if (m_Camera.Acquisition.Capture() == uEye.Defines.Status.Success)
            {
                m_bLive = true;
            }
        }

        public override void StopAcquisition()
        {
            // Stop Live Video
            if (m_Camera.Acquisition.Stop() == uEye.Defines.Status.Success)
            {
                m_bLive = false;
            }
        }

        public override void SaveImage(string filepath)
        {
            // Method One
            m_Camera.Image.Save(filepath);

            // Method Two
            #region
            //Bitmap bitmap = new Bitmap(2456, 2054, System.Drawing.Imaging.PixelFormat.Format8bppIndexed);
            //if (m_Camera.Memory.ToBitmap(m_cnNumberOfSeqBuffers - 1, out bitmap) == uEye.Defines.Status.Success)
            //{
            //    switch (dip)
            //    {
            //        case "None":
            //            {
            //                OpenCvSharp.Mat src = OpenCvSharp.Extensions.BitmapConverter.ToMat(bitmap);
            //                OpenCvSharp.Cv2.ImWrite(image_file, src);
            //                break;
            //            }
            //        case "Binarization":
            //            {
            //                OpenCvSharp.Mat Binary = Binarization(bitmap);
            //                OpenCvSharp.Cv2.ImWrite(image_file, Binary);
            //                break;
            //            }

            //    }

            //}
            #endregion
        }

        public void FreezeFrame()
        {
            if (m_Camera.Acquisition.Freeze() == uEye.Defines.Status.Success)
            {
                m_bLive = false;
            }
        }

        public void LoadCameraParameter(string filepath)
        {
            uEye.Defines.Status statusRet = 0;
            m_Camera.Acquisition.Stop();
            ClearSequence();
            FreeImageMems();
            statusRet = m_Camera.Parameter.Load(filepath);
            if (statusRet != uEye.Defines.Status.Success)
            {
                Console.WriteLine("Loading parameter failed: " + statusRet);
            }
            // Allocate Memory
            statusRet = AllocImageMems();
            if (statusRet != uEye.Defines.Status.Success)
            {
                Console.WriteLine("Allocate Memory failed");
                Environment.Exit(-1);
            }
            statusRet = InitSequence();
            if (statusRet != uEye.Defines.Status.Success)
            {
                Console.WriteLine("Add to sequence failed");
                Environment.Exit(-1);
            }
            if (m_bLive == true)
            {
                m_Camera.Acquisition.Capture();
            }
        }

        public void CameraExit()
        {
            m_Camera.Exit();
        }

        public void SetGamma(int value)
        {
            m_Camera.Gamma.Software.Set(value);
        }

        public void SetExposure(double value)
        {
            m_Camera.Timing.Exposure.Set(value);
        }

        /// <summary>
        /// Sets the master channel gain factor (0...100).
        /// </summary>
        public void SetGain(int value)
        {
            m_Camera.Gain.Hardware.Scaled.SetMaster(value);
        }

        public void SetFramerate(double value)
        {
            m_Camera.Timing.Framerate.Set(value);
        }

        /// <summary>
        /// Returns the minimum, maximum and increment value of gamma.
        /// </summary>
        public void GetGammaRange()
        {
            int gamma_max;
            int gamma_min;
            int gamma_inc;
            m_Camera.Gamma.Software.GetRange(out gamma_min, out gamma_max, out gamma_inc);
            Console.WriteLine(gamma_min);
            Console.WriteLine(gamma_max);
            Console.WriteLine(gamma_inc);
        }

        /// <summary>
        /// Returns the minimum, maximum and increment value of exposure.
        /// </summary>
        public void GetExposureRange()
        {
            double exposure_max;
            double exposure_min;
            double exposure_inc;
            m_Camera.Timing.Exposure.GetRange(out exposure_max, out exposure_min, out exposure_inc);
            Console.WriteLine(exposure_min);
            Console.WriteLine(exposure_max);
            Console.WriteLine(exposure_inc);
        }

        /// <summary>
        /// Returns the minimum, maximum and increment value of framerate.
        /// </summary>
        public void GetFramerateRange()
        {
            double framerate_max;
            double framerate_min;
            double framerate_inc;
            m_Camera.Timing.Framerate.GetFrameRateRange(out framerate_min, out framerate_max, out framerate_inc);
            Console.WriteLine(framerate_min);
            Console.WriteLine(framerate_max);
            Console.WriteLine(framerate_inc);
        }

        
    }

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
