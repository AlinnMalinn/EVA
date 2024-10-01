using System.Runtime.InteropServices;
using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Interop;


namespace EVA_Catalogue
{
    /// <summary>
    /// Логика взаимодействия для WindowSettingsDataBases.xaml
    /// </summary>
    public partial class WindowSettingsDataBases : Window
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
        public WindowSettingsDataBases()
        {
            InitializeComponent();
        }
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
    }
}
