using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DimmerNexus
{
    public class PD12V30W2CH
    {
        public SerialPort serialPort = new SerialPort();

        /// <summary>
        /// BaudRate:19200<para/>
        /// DataBits:8<para/>
        /// Parity:0<para/>
        /// StopBits:1
        /// </summary>
        public PD12V30W2CH(string _PortName, int _BaudRate, int _DataBits, int _Parity, int _StopBits)
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
                Console.WriteLine("連線失敗!");
            }

        }

        public void DisConnect()
        {
            serialPort.Close();
        }

        private string OneChannel(int ledvalue, int ch)
        {
            string Temp = (1 + 6 + ch + ledvalue).ToString("X");
            return Temp.Length >= 2 ? Temp.Substring(Temp.Length - 2, 2) : (1 + 6 + ch + ledvalue).ToString("X2");
        }

        private string TwoChannel(int led1value, int led2value)
        {
            string Temp = (1 + 16 + 1 + 2 + 4 + led1value + led2value).ToString("X");
            return Temp.Length >= 2 ? Temp.Substring(Temp.Length - 2, 2) : (1 + 16 + 1 + 2 + 4 + led1value + led2value).ToString("X2");
        }

        private string LRCCall(int ledvalue, int ch_or_ledvalue, Func<int, int, string> fun)
        {
            return (255 - Convert.ToInt32(fun(ledvalue, ch_or_ledvalue), 16) + 1).ToString("X2");
        }

        private string ProtocalFormat(int ledvalue, int ch_or_ledvalue, int channel)
        {
            string Header = ":";
            string command = "";
            string LRC = "";
            if (channel == 1)
            {
                command = "0106000" + ch_or_ledvalue.ToString() + "00" + ledvalue.ToString("X2");
                LRC = LRCCall(ledvalue, ch_or_ledvalue, OneChannel);
            }
            else if (channel == 2)
            {
                command = "01100001000204" + ledvalue.ToString("X2") + ch_or_ledvalue.ToString("X2");
                LRC = LRCCall(ledvalue, ch_or_ledvalue, TwoChannel);
            }
            return Header + command + LRC + "\r\n";
        }

        public void SetBrightness(int ledvalue, int ch_or_ledvalue, int type)
        {
            string msg = ProtocalFormat(ledvalue, ch_or_ledvalue, type);
            byte[] buf = Encoding.Default.GetBytes(msg);
            serialPort.Write(buf, 0, buf.Length);
        }

    }
}
