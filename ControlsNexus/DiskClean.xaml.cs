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
using System.Windows.Shapes;

namespace ControlsNexus
{
    public class ScheduleItem
    {
        public int Index { get; set; }
        public bool Act { get; set; }      // 勾勾欄位
        public string Type { get; set; }
        public DateTime Time { get; set; }
        public string Directory { get; set; }
    }

    public partial class DiskClean : Window
    {
        public DiskClean()
        {
            InitializeComponent();
        }

        #region Function
        private void WindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (MessageBox.Show("請問是否要關閉？", "確認", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                e.Cancel = false;
            }
            else
            {
                e.Cancel = true;
            }
        }
        #endregion

        #region Parameter and Init
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            data = new ObservableCollection<ScheduleItem>
            {
                new ScheduleItem
                {
                    Index = 1,
                    Act = true,
                    Type = "Meeting",
                    Time = DateTime.Now,
                    Directory = @"C:\Temp"
                }
            };
            Schedule.ItemsSource = data;
        }
        private ObservableCollection<ScheduleItem> data;
        #endregion

        
    }
}
