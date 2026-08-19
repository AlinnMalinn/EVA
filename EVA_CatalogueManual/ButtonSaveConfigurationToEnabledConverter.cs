using System;
using System.Globalization;
using System.Windows.Data;

namespace EVA_CatalogueManual.Converters2
{
    public class ButtonSaveConfigurationToEnabledConverter : IValueConverter
    {
        // Преобразуем bool из ViewModel в значение IsEnabled кнопки
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isAnyProducerSelected)
                return isAnyProducerSelected; // true — кнопка активна, false — не активна

            return false; // по умолчанию кнопка не активна
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException(); // для IsEnabled не нужен
        }
    }
}