using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using Thoth.Models;
using Thoth.Services;
using Xamarin.Forms;

namespace Thoth.Views.ValueConverters
{
    public class IsDownloadedBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // The value parameter is the data from the source object.
            var result = false;
            IsDownloadedEnum isDownloaded = (IsDownloadedEnum)value;
            if (isDownloaded == IsDownloadedEnum.Downloaded)
                result = true;
            return result;
        }

        // ConvertBack is not implemented for a OneWay binding.
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isDownloaded = (bool)value;
            if (isDownloaded)
                return IsDownloadedEnum.Downloaded;
            else
                return IsDownloadedEnum.NotDownloaded;
            //throw new NotImplementedException();
        }
    }
}
