using SharpDX.DirectInput;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ControlsNexus
{
    public class JoystickControl
    {
        private Joystick joystick = null;

        public void JoystickInit()
        {
            // 初始化 DirectInput
            var directInput = new DirectInput();
            // 找到第一個連接的搖桿設備
            var joystickGuid = Guid.Empty;
            foreach (var deviceInstance in directInput.GetDevices(DeviceType.Gamepad, DeviceEnumerationFlags.AllDevices))
            {
                joystickGuid = deviceInstance.InstanceGuid;
            }
            if (joystickGuid == Guid.Empty)
            {
                Console.WriteLine("未偵測到搖桿設備！");
                return;
            }
            // 初始化搖桿
            joystick = new Joystick(directInput, joystickGuid);
            joystick.Acquire();
        }

        public void JoystickArrowkey()
        {
            joystick.Poll();
            var state = joystick.GetCurrentState();
            switch (state.PointOfViewControllers[0])
            {
                case 0:
                    Console.WriteLine("方向鍵：上");
                    break;
                case 4500:
                    Console.WriteLine("方向鍵：右上");
                    break;
                case 9000:
                    Console.WriteLine("方向鍵：右");
                    break;
                case 13500:
                    Console.WriteLine("方向鍵：右下");
                    break;
                case 18000:
                    Console.WriteLine("方向鍵：下");
                    break;
                case 22500:
                    Console.WriteLine("方向鍵：左下");
                    break;
                case 27000:
                    Console.WriteLine("方向鍵：左");
                    break;
                case 31500:
                    Console.WriteLine("方向鍵：左上");
                    break;
                default:
                    Console.WriteLine("方向鍵：未按下");
                    break;
            }
        }

        public void JoystickButton()
        {
            joystick.Poll();
            var state = joystick.GetCurrentState();
            if (state.Buttons[0])
            {
                Console.WriteLine("A 按鈕被按下！");
            }
            if (state.Buttons[1])
            {
                Console.WriteLine("B 按鈕被按下！");
            }
            if (state.Buttons[2])
            {
                Console.WriteLine("X 按鈕被按下！");
            }
            if (state.Buttons[3])
            {
                Console.WriteLine("Y 按鈕被按下！");
            }

        }

    }
}
