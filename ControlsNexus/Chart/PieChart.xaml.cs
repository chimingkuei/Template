using LiveCharts.Wpf;
using LiveCharts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Management;

namespace ControlsNexus.Chart
{

    public partial class PieChart : UserControl
    {
        private DispatcherTimer timer;
        private ChartValues<double> cDriveusedSpaceValues;
        private ChartValues<double> cDrivefreeSpaceValues;
        private ChartValues<double> dDriveusedSpaceValues;
        private ChartValues<double> dDrivefreeSpaceValues;
        public SeriesCollection CDriveSeries { get; set; }
        public SeriesCollection DDriveSeries { get; set; }
        public PieChart()
        {
            InitializeComponent();
            cDriveusedSpaceValues = new ChartValues<double>();
            cDrivefreeSpaceValues = new ChartValues<double>();
            dDriveusedSpaceValues = new ChartValues<double>();
            dDrivefreeSpaceValues = new ChartValues<double>();
            // 初始化定時器
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(3); // 每5秒更新一次
            timer.Tick += Timer_Tick;
            timer.Start();
        }
        private void Timer_Tick(object sender, EventArgs e)
        {
            // 清除現有的數據
            cDriveusedSpaceValues.Clear();
            cDrivefreeSpaceValues.Clear();
            dDriveusedSpaceValues.Clear();
            dDrivefreeSpaceValues.Clear();
            // 獲取C槽存儲信息
            long cDriveTotalSpace = GetDriveTotalSpace("C:");
            long cDriveFreeSpace = GetDriveFreeSpace("C:");
            double cDriveusedSpaceTB = Math.Round((cDriveTotalSpace - cDriveFreeSpace) / (1024.0 * 1024.0 * 1024.0), 2);
            double cDrivefreeSpaceTB = Math.Round(cDriveFreeSpace / (1024.0 * 1024.0 * 1024.0), 2);
            cDriveusedSpaceValues.Add(cDriveusedSpaceTB);
            cDrivefreeSpaceValues.Add(cDrivefreeSpaceTB);
            // 獲取D槽存儲信息
            long dDriveTotalSpace = GetDriveTotalSpace("D:");
            long dDriveFreeSpace = GetDriveFreeSpace("D:");
            double dDriveusedSpaceTB = Math.Round((dDriveTotalSpace - dDriveFreeSpace) / (1024.0 * 1024.0 * 1024.0), 2);
            double dDrivefreeSpaceTB = Math.Round(dDriveFreeSpace / (1024.0 * 1024.0 * 1024.0), 2);
            dDriveusedSpaceValues.Add(dDriveusedSpaceTB);
            dDrivefreeSpaceValues.Add(dDrivefreeSpaceTB);
            // 更新圓餅圖數據
            CDriveSeries = new SeriesCollection
            {
                new PieSeries
                {
                    Title = "Used Space (GB)",
                    Values = cDriveusedSpaceValues,
                    DataLabels = true,
                    Fill = Brushes.Blue,
                    FontSize = 15,
                    //PushOut = 15
                    Foreground = Brushes.Black,
                },
                new PieSeries
                {
                    Title = "Free Space (GB)",
                    Values = cDrivefreeSpaceValues,
                    DataLabels = true,
                    Fill = Brushes.Green,
                    FontSize = 15,
                    //PushOut = 15
                    Foreground = Brushes.Black,
                }
            };
            CDrive.Series = CDriveSeries;
            DDriveSeries = new SeriesCollection
            {
                new PieSeries
                {
                    Title = "Used Space (GB)",
                    Values = dDriveusedSpaceValues,
                    DataLabels = true,
                    Fill = Brushes.Blue,
                    FontSize = 15,
                    //PushOut = 15
                    Foreground = Brushes.Black,
                },
                new PieSeries
                {
                    Title = "Free Space (GB)",
                    Values = dDrivefreeSpaceValues,
                    DataLabels = true,
                    Fill = Brushes.Green,
                    FontSize = 15,
                    //PushOut = 15
                    Foreground = Brushes.Black,
                }
            };
            DDrive.Series = DDriveSeries;
        }

        private long GetDriveTotalSpace(string driveLetter)
        {
            try
            {
                ManagementObject disk = new ManagementObject("Win32_LogicalDisk.DeviceID='" + driveLetter + "'");
                disk.Get();
                return Convert.ToInt64(disk["Size"]);
            }
            catch (ManagementException)
            {
                return 0;
            }
        }

        private long GetDriveFreeSpace(string driveLetter)
        {
            try
            {
                ManagementObject disk = new ManagementObject("Win32_LogicalDisk.DeviceID='" + driveLetter + "'");
                disk.Get();
                return Convert.ToInt64(disk["FreeSpace"]);
            }
            catch (ManagementException)
            {
                return 0;
            }
        }

        private void DisplayData_Click(object sender, ChartPoint chartPoint)
        {
            // 獲取被點擊的圓餅圖部分的信息
            var series = chartPoint.SeriesView as PieSeries;
            var partValue = chartPoint.Y;
            var partTitle = series.Title;

            // 創建並顯示一個新的視窗以顯示被點擊的圓餅圖部分的信息
            MessageBox.Show($"{partTitle}: {partValue} TB");
        }


    }
}
