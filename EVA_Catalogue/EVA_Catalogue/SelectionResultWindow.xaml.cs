using Microsoft.Win32;
using System;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

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
            var document = new FlowDocument { PagePadding = new Thickness(0) };
            string normalizedText = (text ?? string.Empty).Replace("\r\n", "\n").Replace('\r', '\n');
            string[] lines = normalizedText.Split('\n');

            for (int index = 0; index < lines.Length; index++)
            {
                string line = lines[index];
                if (IsPriorityMarker(line))
                {
                    string parameter = index + 2 < lines.Length ? lines[index + 2].Trim() : string.Empty;
                    string condition = index + 4 < lines.Length ? lines[index + 4].Trim() : string.Empty;
                    string threePhaseCurrent = index + 6 < lines.Length ? lines[index + 6].Trim() : string.Empty;
                    string singlePhaseCurrent = index + 7 < lines.Length ? lines[index + 7].Trim() : string.Empty;
                    index = Math.Min(index + 7, lines.Length - 1);
                    document.Blocks.Add(CreatePriorityBlock(
                        line.Trim(), parameter, condition, threePhaseCurrent, singlePhaseCurrent));
                    continue;
                }

                var paragraph = new Paragraph { Margin = new Thickness(0) };
                paragraph.Inlines.Add(IsHeading(line) ? (Inline)new Bold(new Run(line)) : new Run(line));
                document.Blocks.Add(paragraph);
            }

            return document;
        }

        private static BlockUIContainer CreatePriorityBlock(string marker, string parameter,
            string condition, string threePhaseCurrent, string singlePhaseCurrent)
        {
            var panel = new StackPanel();
            panel.Children.Add(new TextBlock
            {
                Text = marker,
                FontWeight = FontWeights.SemiBold,
                Foreground = new SolidColorBrush(Color.FromRgb(133, 82, 0))
            });
            panel.Children.Add(new TextBlock
            {
                Text = parameter,
                Margin = new Thickness(22, 10, 0, 0),
                FontWeight = FontWeights.Normal,
                Foreground = Brushes.Black,
                TextWrapping = TextWrapping.Wrap
            });
            panel.Children.Add(new TextBlock
            {
                Text = condition,
                Margin = new Thickness(0, 10, 0, 0),
                Foreground = Brushes.Black,
                TextWrapping = TextWrapping.Wrap
            });
            panel.Children.Add(new TextBlock
            {
                Text = threePhaseCurrent,
                Margin = new Thickness(22, 5, 0, 0),
                Foreground = Brushes.Black,
                TextWrapping = TextWrapping.Wrap
            });
            panel.Children.Add(new TextBlock
            {
                Text = singlePhaseCurrent,
                Margin = new Thickness(22, 2, 0, 0),
                Foreground = Brushes.Black,
                TextWrapping = TextWrapping.Wrap
            });

            return new BlockUIContainer(new Border
            {
                Margin = new Thickness(0, 3, 0, 3),
                Padding = new Thickness(10, 8, 10, 8),
                Background = new SolidColorBrush(Color.FromRgb(255, 247, 222)),
                BorderBrush = new SolidColorBrush(Color.FromRgb(214, 158, 46)),
                BorderThickness = new Thickness(4, 0, 0, 0),
                CornerRadius = new CornerRadius(4),
                Child = panel
            });
        }

        private static bool IsPriorityMarker(string line)
        {
            return string.Equals((line ?? string.Empty).Trim(),
                "⚠ ПРОВЕРИТЬ В ПЕРВУЮ ОЧЕРЕДЬ", StringComparison.CurrentCulture);
        }

        private static bool IsHeading(string line)
        {
            string value = (line ?? string.Empty).Trim();
            return value.StartsWith("Не удалось подобрать:", StringComparison.CurrentCulture) ||
                   value.StartsWith("Модульные автоматические выключатели -", StringComparison.CurrentCulture) ||
                   value.StartsWith("Модульные дифференциальные автоматические выключатели -", StringComparison.CurrentCulture) ||
                   string.Equals(value, "Что необходимо проверить", StringComparison.CurrentCulture) ||
                   string.Equals(value, "Для модульных АВ:", StringComparison.CurrentCulture) ||
                   string.Equals(value, "Для модульных АВДТ:", StringComparison.CurrentCulture) ||
                   string.Equals(value, "Также проверьте:", StringComparison.CurrentCulture);
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
