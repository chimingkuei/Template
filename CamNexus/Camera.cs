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
}
