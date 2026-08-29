using Microsoft.Win32;
using System;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Documents;
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
            ResultTextBox.Document = CreateResultDocument(ResultText);
            ResultTextBox.Document.FontFamily = ResultTextBox.FontFamily;
            ResultTextBox.Document.FontSize = ResultTextBox.FontSize;
        }

        private static FlowDocument CreateResultDocument(string text)
        {
            var paragraph = new Paragraph { Margin = new Thickness(0) };
            string normalizedText = (text ?? string.Empty).Replace("\r\n", "\n").Replace('\r', '\n');
            string[] lines = normalizedText.Split('\n');

            for (int index = 0; index < lines.Length; index++)
            {
                string line = lines[index];
                Inline inline = IsHeading(line) ? (Inline)new Bold(new Run(line)) : new Run(line);
                paragraph.Inlines.Add(inline);
                if (index < lines.Length - 1)
                    paragraph.Inlines.Add(new LineBreak());
            }

            var document = new FlowDocument(paragraph) { PagePadding = new Thickness(0) };
            return document;
        }

        private static bool IsHeading(string line)
        {
            string value = (line ?? string.Empty).Trim();
            return value.StartsWith("Не удалось подобрать:", StringComparison.CurrentCulture) ||
                   value.StartsWith("Модульные автоматические выключатели -", StringComparison.CurrentCulture) ||
                   value.StartsWith("Модульные дифференциальные автоматические выключатели -", StringComparison.CurrentCulture) ||
                   string.Equals(value, "Что необходимо проверить", StringComparison.CurrentCulture) ||
                   string.Equals(value, "Для АВ:", StringComparison.CurrentCulture) ||
                   string.Equals(value, "Для АВДТ:", StringComparison.CurrentCulture);
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
