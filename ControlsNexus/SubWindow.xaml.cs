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
using System.Windows.Shapes;

namespace ControlsNexus
{

    public partial class SubWindow : Window
    {
        public SubWindow()
        {
            InitializeComponent();
        }

        #region  Parameter and Init
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            
        }
        #endregion

        // 宣告委派
        public delegate void ActionEventHandller(string msg);
        // 宣告事件
        public event ActionEventHandller Action;
        // 事件觸發位置
        private void Demo_Click(object sender, RoutedEventArgs e)
        {
            Action?.Invoke(InputBox.Text);
            this.Close();
        }
        #region 主視窗使用如下︰
        //public void DisplayMSG(string msg)
        //{
        //    MessageBox.Show(msg, "訊息", MessageBoxButton.OK, MessageBoxImage.Information);
        //}
        //SubWindow win = new SubWindow();
        //win.Action += DisplayMSG;
        //win.ShowDialog();
        #endregion

        
    }
}
