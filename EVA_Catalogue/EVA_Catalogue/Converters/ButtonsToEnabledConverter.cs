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
    public class ButtonsToEnabledConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            PathHelper pathHelper = new PathHelper();
            bool link = pathHelper.CheckLinkForDB();
            if (values.Length < 1)
            { return false; }
            else
            { return link; }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
