using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Xamarin.Forms;

namespace Thoth.Views.ValueConverters
{
    public class StringIsEmptyToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            bool result = false;
            try
            {
                var s = value.ToString();
                result = !String.IsNullOrEmpty(s);
            }
            catch { }
            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            Debug.WriteLine("StringIsEmptyToBool.ConvertBack");
            return value;
        }
    }
}
