using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace EVA_Catalogue_DataBaseSetting
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private const int WM_NCLBUTTONDOWN = 0xA1;
        private const int HT_CAPTION = 0x2;
        private const int HT_LEFT = 10;
        private const int HT_RIGHT = 11;
        private const int HT_TOP = 12;
        private const int HT_TOPLEFT = 13;
        private const int HT_TOPRIGHT = 14;
        private const int HT_BOTTOM = 15;
        private const int HT_BOTTOMLEFT = 16;
        private const int HT_BOTTOMRIGHT = 17;

        [DllImport("user32.dll")]
        private static extern bool ReleaseCapture();

        [DllImport("user32.dll")]

        private static extern IntPtr SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);


        public MainWindow()
        {
            var previousCursor = Mouse.OverrideCursor;
            try
            {
                Mouse.OverrideCursor = Cursors.Wait;
                InitializeComponent();
            }
            finally
            {
                Mouse.OverrideCursor = previousCursor;
            }
        }


        /// <summary>
        /// Изменение размера окна
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ResizeBorder_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                var border = sender as Border;
                if (border != null)
                {
                    IntPtr windowHandle = new WindowInteropHelper(this).Handle;
                    int resizeDirection = 0;

                    switch (border.Name)
                    {
                        case "ResizeTop":
                            resizeDirection = HT_TOP;
                            break;
                        case "ResizeBottom":
                            resizeDirection = HT_BOTTOM;
                            break;
                        case "ResizeLeft":
                            resizeDirection = HT_LEFT;
                            break;
                        case "ResizeRight":
                            resizeDirection = HT_RIGHT;
                            break;
                        case "ResizeTopLeft":
                            resizeDirection = HT_TOPLEFT;
                            break;
                        case "ResizeTopRight":
                            resizeDirection = HT_TOPRIGHT;
                            break;
                        case "ResizeBottomLeft":
                            resizeDirection = HT_BOTTOMLEFT;
                            break;
                        case "ResizeBottomRight":
                            resizeDirection = HT_BOTTOMRIGHT;
                            break;
                    }

                    ReleaseCapture();
                    SendMessage(windowHandle, WM_NCLBUTTONDOWN, (IntPtr)resizeDirection, IntPtr.Zero);
                }
            }
        }
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // Проверяем, нажата ли левая кнопка мыши
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.DragMove(); // Позволяет перемещать окно
            }
        }

    }
}
