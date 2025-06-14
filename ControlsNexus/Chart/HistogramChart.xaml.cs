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

namespace ControlsNexus.Chart
{

    public partial class HistogramChart : UserControl
    {
        private ColumnSeries _histogramSeries;
        private ChartValues<double> _values;
        private Random _random;
        private DispatcherTimer _timer;

        private void HistogramInit()
        {
            _random = new Random();
            _values = new ChartValues<double> { 0, 0, 0, 0, 0 }; // 5 個區間
            _histogramSeries = new ColumnSeries
            {
                Title = "頻率",
                Values = _values
            };
            chart.Series = new SeriesCollection { _histogramSeries };
            chart.AxisX.Add(new Axis
            {
                Title = "區間",
                Labels = new[] { "A", "B", "C", "D", "E" }
            });
            chart.AxisY.Add(new Axis
            {
                Title = "次數"
            });
        }

        public HistogramChart()
        {
            InitializeComponent();
            HistogramInit();
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += UpdateHistogram;
            _timer.Start();
        }

        private void UpdateHistogram(object sender, EventArgs e)
        {
            var newData = Enumerable.Range(0, 5).Select(_ => _random.NextDouble() * 50).ToList();
            _values.Clear();
            _values.AddRange(newData);
        }
    }
}
