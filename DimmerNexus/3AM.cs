using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DimmerNexus
{
    abstract class RS232Series
    {
        public SerialPort serialPort = new SerialPort();

        protected RS232Series(string _PortName, int _BaudRate, int _DataBits, int _Parity, int _StopBits)
        {
            serialPort.PortName = _PortName;
            serialPort.BaudRate = _BaudRate;
            serialPort.DataBits = _DataBits;
            serialPort.Parity = (Parity)_Parity;
            serialPort.StopBits = (StopBits)_StopBits;
        }

        public void Connect()
        {
            try
            {
                serialPort.Open();
            }
            catch
            {
                MessageBox.Show("連線失敗!", "警告");
            }

        }

        public void DisConnect()
        {
            serialPort.Close();
        }

        abstract public void OneChannelSetBrightness(int led_value, int ch);
        abstract public void TwoChannelSetBrightness(int led1_value, int led2_value);

        abstract public void FourChannelSetBrightness(int led1_value, int led2_value, int led3_value, int led4_value);
    }

    class GLCPD12V30W : RS232Series //PD12V30W("COM5", 19200, 8, 0, 1);
    {
        public GLCPD12V30W(string _PortName, int _BaudRate, int _DataBits, int _Parity, int _StopBits) : base(_PortName, _BaudRate, _DataBits, _Parity, _StopBits)
        {
        }

        private string OneChannelLRC(int led_value, int ch)
        {
            string Temp = (1 + 6 + ch + led_value).ToString("X");
            string Data_sum = Temp.Length >= 2 ? Temp.Substring(Temp.Length - 2, 2) : Temp;
            string LRC = (255 - Convert.ToInt32(Data_sum, 16) + 1).ToString("X2");
            return LRC;
        }

        private string OneChannelProtocalFormat(int led_value, int ch)
        {
            string Header = ":";
            string command = "0106000" + ch.ToString() + "00" + led_value.ToString("X2");
            string LRC = OneChannelLRC(led_value, ch);
            string msg = Header + command + LRC + "\r\n";
            //Console.WriteLine(msg);
            return msg;
        }

        public override void OneChannelSetBrightness(int led_value, int ch)
        {
            string msg = OneChannelProtocalFormat(led_value, ch);
            byte[] buf = Encoding.Default.GetBytes(msg);
            serialPort.Write(buf, 0, buf.Length);
        }

        private string TwoChannelLRC(int led1_value, int led2_value)
        {
            string Temp = (1 + 16 + 1 + 2 + 4 + led1_value + led2_value).ToString("X");
            string Data_sum = Temp.Length >= 2 ? Temp.Substring(Temp.Length - 2, 2) : Temp;
            string LRC = (255 - Convert.ToInt32(Data_sum, 16) + 1).ToString("X2");
            return LRC;
        }

        private string TwoChannelProtocalFormat(int led1_value, int led2_value)
        {
            string Header = ":";
            string command = "01100001000204" + led1_value.ToString("X2") + led2_value.ToString("X2");
            string LRC = TwoChannelLRC(led1_value, led2_value);
            string msg = Header + command + LRC + "\r\n";
            //Console.WriteLine(msg);
            return msg;
        }

        public override void TwoChannelSetBrightness(int led1_value, int led2_value)
        {
            string msg = TwoChannelProtocalFormat(led1_value, led2_value);
            byte[] buf = Encoding.Default.GetBytes(msg);
            serialPort.Write(buf, 0, buf.Length);
        }

        private string FourChannelLRC(int led1_value, int led2_value, int led3_value, int led4_value)
        {
            string Temp = (1 + 16 + 1 + 4 + 8 + led1_value + led2_value + led3_value + led4_value).ToString("X");
            string Data_sum = Temp.Length >= 2 ? Temp.Substring(Temp.Length - 2, 2) : Temp;
            string LRC = (255 - Convert.ToInt32(Data_sum, 16) + 1).ToString("X2");
            return LRC;
        }

        private string FourChannelProtocalFormat(int led1_value, int led2_value, int led3_value, int led4_value)
        {
            string Header = ":";
            string command = "01100001000408" + led1_value.ToString("X2") + led2_value.ToString("X2") + led3_value.ToString("X2") + led4_value.ToString("X2");
            string LRC = FourChannelLRC(led1_value, led2_value, led3_value, led4_value);
            string msg = Header + command + LRC + "\r\n";
            //Console.WriteLine(msg);
            return msg;
        }

        public override void FourChannelSetBrightness(int led1_value, int led2_value, int led3_value, int led4_value)
        {
            string msg = FourChannelProtocalFormat(led1_value, led2_value, led3_value, led4_value);
            byte[] buf = Encoding.Default.GetBytes(msg);
            serialPort.Write(buf, 0, buf.Length);
        }
    }

    // 待測試4CH
    class GLCPD24V24W : RS232Series //GLCPD24V24W("COM5", 115200, 8, 0, 1);
    {
        public GLCPD24V24W(string _PortName, int _BaudRate, int _DataBits, int _Parity, int _StopBits) : base(_PortName, _BaudRate, _DataBits, _Parity, _StopBits)
        {
        }

        private string OneChannelLRC(int led_value, int ch)
        {
            string firstTwo_Register_Value = led_value.ToString("X4").Substring(0, 2);
            string lastTwo_Register_Value = led_value.ToString("X4").Substring(2, 2);
            string Temp = (1 + 6 + ch + Convert.ToInt32(firstTwo_Register_Value, 16) + Convert.ToInt32(lastTwo_Register_Value, 16)).ToString("X");
            string Data_sum = Temp.Length >= 2 ? Temp.Substring(Temp.Length - 2, 2) : Temp;
            string LRC = (255 - Convert.ToInt32(Data_sum, 16) + 1).ToString("X2");
            return LRC;
        }

        private string OneChannelProtocalFormat(int led_value, int ch)
        {
            string Header = ":";
            string command = "0106" + ch.ToString("X4") + led_value.ToString("X4");
            string LRC = OneChannelLRC(led_value, ch);
            string msg = Header + command + LRC + "\r\n";
            //Console.WriteLine(msg);
            return msg;
        }

        public override void OneChannelSetBrightness(int led_value, int ch)
        {
            string msg = OneChannelProtocalFormat(led_value, ch);
            byte[] buf = Encoding.Default.GetBytes(msg);
            serialPort.Write(buf, 0, buf.Length);
        }

        private string TwoChannelLRC(int led1_value, int led2_value)
        {
            string firstTwo_Register_Value1 = led1_value.ToString("X4").Substring(0, 2);
            string lastTwo_Register_Value1 = led1_value.ToString("X4").Substring(2, 2);
            string firstTwo_Register_Value2 = led2_value.ToString("X4").Substring(0, 2);
            string lastTwo_Register_Value2 = led2_value.ToString("X4").Substring(2, 2);
            string Temp = (1 + 16 + 1 + 2 + 4 + Convert.ToInt32(firstTwo_Register_Value1, 16) + Convert.ToInt32(lastTwo_Register_Value1, 16) + Convert.ToInt32(firstTwo_Register_Value2, 16) + Convert.ToInt32(lastTwo_Register_Value2, 16)).ToString("X");
            string Data_sum = Temp.Length >= 2 ? Temp.Substring(Temp.Length - 2, 2) : Temp;
            string LRC = (255 - Convert.ToInt32(Data_sum, 16) + 1).ToString("X2");
            return LRC;
        }

        private string TwoChannelProtocalFormat(int led1_value, int led2_value)
        {
            string Header = ":";
            string command = "01100001000204" + led1_value.ToString("X4") + led2_value.ToString("X4");
            string LRC = TwoChannelLRC(led1_value, led2_value);
            string msg = Header + command + LRC + "\r\n";
            //Console.WriteLine(msg);
            return msg;
        }

        public override void TwoChannelSetBrightness(int led1_value, int led2_value)
        {
            string msg = TwoChannelProtocalFormat(led1_value, led2_value);
            byte[] buf = Encoding.Default.GetBytes(msg);
            serialPort.Write(buf, 0, buf.Length);
        }

        private string FourChannelLRC(int led1_value, int led2_value, int led3_value, int led4_value)
        {
            string firstTwo_Register_Value1 = led1_value.ToString("X4").Substring(0, 2);
            string lastTwo_Register_Value1 = led1_value.ToString("X4").Substring(2, 2);
            string firstTwo_Register_Value2 = led2_value.ToString("X4").Substring(0, 2);
            string lastTwo_Register_Value2 = led2_value.ToString("X4").Substring(2, 2);
            string firstTwo_Register_Value3 = led3_value.ToString("X4").Substring(0, 2);
            string lastTwo_Register_Value3 = led3_value.ToString("X4").Substring(2, 2);
            string firstTwo_Register_Value4 = led4_value.ToString("X4").Substring(0, 2);
            string lastTwo_Register_Value4 = led4_value.ToString("X4").Substring(2, 2);
            string Temp = (1 + 16 + 1 + 4 + 8 + Convert.ToInt32(firstTwo_Register_Value1, 16)
                                              + Convert.ToInt32(lastTwo_Register_Value1, 16)
                                              + Convert.ToInt32(firstTwo_Register_Value2, 16)
                                              + Convert.ToInt32(lastTwo_Register_Value2, 16)
                                              + Convert.ToInt32(firstTwo_Register_Value3, 16)
                                              + Convert.ToInt32(lastTwo_Register_Value3, 16)
                                              + Convert.ToInt32(firstTwo_Register_Value4, 16)
                                              + Convert.ToInt32(lastTwo_Register_Value4, 16)
                                              ).ToString("X");
            string Data_sum = Temp.Length >= 2 ? Temp.Substring(Temp.Length - 2, 2) : Temp;
            string LRC = (255 - Convert.ToInt32(Data_sum, 16) + 1).ToString("X2");
            return LRC;
        }

        private string FourChannelProtocalFormat(int led1_value, int led2_value, int led3_value, int led4_value)
        {
            string Header = ":";
            string command = "01100001000408" + led1_value.ToString("X4") + led2_value.ToString("X4") + led3_value.ToString("X4") + led4_value.ToString("X4");
            string LRC = FourChannelLRC(led1_value, led2_value, led3_value, led4_value);
            string msg = Header + command + LRC + "\r\n";
            //Console.WriteLine(msg);
            return msg;
        }

        public override void FourChannelSetBrightness(int led1_value, int led2_value, int led3_value, int led4_value)
        {
            string msg = FourChannelProtocalFormat(led1_value, led2_value, led3_value, led4_value);
            byte[] buf = Encoding.Default.GetBytes(msg);
            serialPort.Write(buf, 0, buf.Length);
        }
    }
}
