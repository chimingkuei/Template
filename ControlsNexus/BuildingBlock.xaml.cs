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

namespace ControlsNexus
{
    /// <summary>
    /// BuildingBlock.xaml 的互動邏輯
    /// </summary>
    public partial class BuildingBlock : UserControl
    {
        public BuildingBlock()
        {
            InitializeComponent();
        }

        private void DraggableButton_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Button draggedButton = sender as Button;
            if (draggedButton != null)
            {
                DragDrop.DoDragDrop(draggedButton, new DataObject("myButton", draggedButton), DragDropEffects.Move);
            }
        }

        // Canvas1 的拖入事件
        private void Canvas1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent("myButton"))
            {
                e.Effects = DragDropEffects.Move;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
        }

        // Canvas2 的拖入事件
        private void Canvas2_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent("myButton"))
            {
                e.Effects = DragDropEffects.Move;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
        }

        // 將按鈕拖放到 Canvas1 上
        private void Canvas1_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent("myButton"))
            {
                Button draggedButton = (Button)e.Data.GetData("myButton");
                Canvas parentCanvas = (Canvas)draggedButton.Parent;
                parentCanvas.Children.Remove(draggedButton);

                // 添加按鈕到 Canvas1
                Canvas1.Children.Add(draggedButton);

                // 設置按鈕的位置（相對於 Canvas1）
                Point dropPosition = e.GetPosition(Canvas1);
                Canvas.SetLeft(draggedButton, dropPosition.X - draggedButton.Width / 2);
                Canvas.SetTop(draggedButton, dropPosition.Y - draggedButton.Height / 2);
            }
        }

        // 將按鈕拖放到 Canvas2 上
        private void Canvas2_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent("myButton"))
            {
                Button draggedButton = (Button)e.Data.GetData("myButton");
                Canvas parentCanvas = (Canvas)draggedButton.Parent;
                parentCanvas.Children.Remove(draggedButton);

                // 添加按鈕到 Canvas2
                Canvas2.Children.Add(draggedButton);

                // 設置按鈕的位置（相對於 Canvas2）
                Point dropPosition = e.GetPosition(Canvas2);
                Canvas.SetLeft(draggedButton, dropPosition.X - draggedButton.Width / 2);
                Canvas.SetTop(draggedButton, dropPosition.Y - draggedButton.Height / 2);
                MessageBox.Show("移動到Canvas2");
            }
        }

    }
}
