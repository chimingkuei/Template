using LiveCharts.Wpf;
using LiveCharts;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace ControlsNexus.Chart
{

    public partial class LineChart : UserControl
    {
        // 用於儲存摺線圖資料的集合
        public ObservableCollection<ChartValues<double>> Values1 { get; set; }
        public ObservableCollection<ChartValues<int>> Values2 { get; set; }
        private DispatcherTimer _timer;

        // 初始化 ObservableCollection
        private ObservableCollection<ChartValues<T>> CreateChartValues<T>(IEnumerable<T> values)
        {
            return new ObservableCollection<ChartValues<T>> { new ChartValues<T>(values) };
        }

        // 建立 LineSeries 方法
        private LineSeries CreateLineSeries<T>(string title, ChartValues<T> values)
        {
            return new LineSeries
            {
                Title = title,
                Values = values,
                PointGeometrySize = 10,  // 数据点大小
                StrokeThickness = 2,     // 线条宽度
                LineSmoothness = 0.5f    // 线条平滑度
            };
        }

        private void LineChartInit()
        {
            Values1 = CreateChartValues(new List<double> { 0, 0, 0, 0, 0 });
            Values2 = CreateChartValues(new List<int> { 0, 0, 0, 0, 0 });
            // 创建 LineSeries 并绑定数据
            chart.Series = new SeriesCollection
            {
                CreateLineSeries("產品A", Values1[0]),
                CreateLineSeries("產品B", Values2[0])
            };
        }

        public LineChart()
        {
            InitializeComponent();
            LineChartInit();
            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(500) // 設置更新間隔為 500 毫秒
            };
            _timer.Tick += Timer_Tick;
            _timer.Start(); 
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            Random random = new Random();
            // 每次在折線圖上新增一個新的資料點
            var newValue1 = random.Next(1, 100);
            var newValue2 = random.Next(1, 100);
            // 新增值到 ObservableCollection
            if (Values1.Count > 0)
            {
                // 只保持最多 20 點
                if (Values1[0].Count >= 20)
                    Values1[0].RemoveAt(0);
                Values1[0].Add(newValue1);
            }
            if (Values2.Count > 0)
            {
                // 只保持最多 20 點
                if (Values2[0].Count >= 20)
                    Values2[0].RemoveAt(0);
                Values2[0].Add(newValue2);
            }
            chart.Update();
        }

    }
}
