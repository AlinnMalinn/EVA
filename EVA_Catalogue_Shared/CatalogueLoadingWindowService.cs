using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace EVA_Catalogue_Shared
{
    public static class CatalogueLoadingWindowService
    {
        public static IDisposable Show(string message)
        {
            return new LoadingWindowHandle(message);
        }

        private sealed class LoadingWindowHandle : IDisposable
        {
            private readonly ManualResetEventSlim ready = new ManualResetEventSlim(false);
            private readonly Thread thread;
            private Dispatcher dispatcher;
            private Window window;
            private bool disposed;

            public LoadingWindowHandle(string message)
            {
                thread = new Thread(() => RunWindow(message))
                {
                    IsBackground = true,
                    Name = "EVA loading window"
                };
                thread.SetApartmentState(ApartmentState.STA);
                thread.Start();
                ready.Wait(TimeSpan.FromSeconds(3));
            }

            private void RunWindow(string message)
            {
                try
                {
                    dispatcher = Dispatcher.CurrentDispatcher;
                    var text = new TextBlock
                    {
                        Text = message,
                        FontFamily = new FontFamily("Segoe UI"),
                        FontSize = 16,
                        Foreground = Brushes.Black,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        TextAlignment = TextAlignment.Center
                    };
                    var border = new Border
                    {
                        Background = new SolidColorBrush(Color.FromRgb(245, 245, 245)),
                        BorderBrush = Brushes.Gray,
                        BorderThickness = new Thickness(1),
                        CornerRadius = new CornerRadius(10),
                        Padding = new Thickness(24),
                        Child = text
                    };
                    window = new Window
                    {
                        Width = 390,
                        Height = 120,
                        WindowStyle = WindowStyle.None,
                        ResizeMode = ResizeMode.NoResize,
                        WindowStartupLocation = WindowStartupLocation.CenterScreen,
                        ShowInTaskbar = false,
                        ShowActivated = false,
                        Topmost = true,
                        AllowsTransparency = true,
                        Background = Brushes.Transparent,
                        Content = border
                    };
                    window.Closed += (sender, args) => dispatcher.BeginInvokeShutdown(DispatcherPriority.Background);
                    window.Show();
                    ready.Set();
                    Dispatcher.Run();
                }
                finally
                {
                    ready.Set();
                }
            }

            public void Dispose()
            {
                if (disposed)
                    return;
                disposed = true;
                Dispatcher currentDispatcher = dispatcher;
                if (currentDispatcher != null && !currentDispatcher.HasShutdownStarted)
                {
                    currentDispatcher.BeginInvoke(new Action(() => window?.Close()));
                    thread.Join(TimeSpan.FromSeconds(2));
                }
                ready.Dispose();
            }
        }
    }
}
