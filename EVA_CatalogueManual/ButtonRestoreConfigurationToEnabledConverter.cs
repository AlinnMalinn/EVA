using System;
using System.Globalization;
using System.Windows.Data;

namespace EVA_CatalogueManual.Converters
{
    public class ButtonRestoreConfigurationToEnabledConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            // Проверяем, что значение передано и это bool
            if (values != null && values.Length > 0 && values[0] is bool doesConfigurationExist)
            {
                return doesConfigurationExist; // true — кнопка активна, false — не активна
            }

            // По умолчанию кнопка не активна
            return false;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}