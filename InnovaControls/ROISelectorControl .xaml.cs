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
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace InnovaControls
{

    public partial class ROISelectorControl : UserControl
    {
        public ROISelectorControl()
        {
            InitializeComponent();
        }

        private bool _started;
        private Point _downPoint;

        private void DrawROI_MouseDown(object sender, MouseButtonEventArgs e)
        {
            _started = true;
            _downPoint = e.GetPosition(Display_Screen);
            //Console.WriteLine($"X座標:{e.GetPosition(image).X}");
            //Console.WriteLine($"X座標:{e.GetPosition(image).Y}");
        }

        private void DrawROI_MouseUp(object sender, MouseButtonEventArgs e)
        {
            _started = false;
        }

        private void DrawROI_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (_started)
                {
                    #region draw rectangle
                    var point = e.GetPosition(Display_Screen);
                    var rect = new Rect(_downPoint, point);
                    Rectangle.Margin = new Thickness(rect.Left, rect.Top, 0, 0);
                    Rectangle.Width = rect.Width;
                    Rectangle.Height = rect.Height;
                    #endregion
                }
            }
            if (e.RightButton == MouseButtonState.Pressed)
            {
                #region Draw Rectangle
                var point = e.GetPosition(Display_Screen);
                var rect = new Rect(_downPoint, point);
                Rectangle.Margin = new Thickness(rect.Left, rect.Top, 0, 0);
                #endregion
            }
        }

        #region Image Control Register
        public static readonly DependencyProperty ImageSourceProperty =
        DependencyProperty.Register(nameof(ImageSource), typeof(BitmapImage), typeof(ROISelectorControl), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

        public BitmapImage ImageSource
        {
            get { return (BitmapImage)GetValue(ImageSourceProperty); }
            set { SetValue(ImageSourceProperty, value); }
        }
        #region Use
        //BitmapImage bitmapImage = new BitmapImage();
        //bitmapImage.BeginInit();
        //bitmapImage.UriSource = new Uri("pack://application:,,,/Icon/Image.png");
        //bitmapImage.EndInit();
        //Control.ImageSource = bitmapImage;
        #endregion
        #endregion


    }
}
