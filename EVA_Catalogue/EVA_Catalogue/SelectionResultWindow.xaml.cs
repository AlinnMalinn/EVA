using Microsoft.Win32;
using System;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace EVA_Catalogue
{
    public partial class SelectionResultWindow : Window
    {
        public static readonly DependencyProperty ResultTextProperty = DependencyProperty.Register(
            nameof(ResultText), typeof(string), typeof(SelectionResultWindow), new PropertyMetadata(string.Empty));

        public string ResultText
        {
            get => (string)GetValue(ResultTextProperty);
            set => SetValue(ResultTextProperty, value);
        }

        public SelectionResultWindow(string resultText)
        {
            ResultText = resultText ?? string.Empty;
            InitializeComponent();
        }

        private void Export_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new SaveFileDialog
            {
                Filter = "Текстовый файл (*.txt)|*.txt",
                DefaultExt = ".txt",
                AddExtension = true,
                FileName = "Результат_подбора_" + DateTime.Now.ToString("yyyy-MM-dd_HH-mm")
            };
            if (dialog.ShowDialog(this) != true) return;
            try
            {
                File.WriteAllText(dialog.FileName, ResultText, new UTF8Encoding(false));
                System.Windows.MessageBox.Show(this, "Отчёт сохранён.", "Выгрузка",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(this, "Не удалось сохранить отчёт: " + ex.Message,
                    "Выгрузка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e) => Close();

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed) DragMove();
        }
    }
}
