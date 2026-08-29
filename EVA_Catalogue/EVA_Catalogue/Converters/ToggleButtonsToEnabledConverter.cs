using System;
using System.Windows;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media.Animation;




namespace EVA_Catalogue.Converters
{
    public class ToggleButtonsToEnabledConverter : IMultiValueConverter
    {
 
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length >= 2 &&
                values[0] is bool isChecked1 &&
                values[1] is bool isChecked2)
            {
                bool cataloguesAvailable = values.Length < 3 ||
                    (values[2] is bool available && available);
                return (isChecked1 || isChecked2) && cataloguesAvailable;
            }
            return false;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
