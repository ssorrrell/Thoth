using System;
using System.Globalization;

using Xamarin.Forms;

using Thoth.Services;

namespace Thoth.Views.ValueConverters
{
    // Custom class implements the IValueConverter interface.
    public class PodcastEpisodeImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // The value parameter is the data from the source object.
            string file = (string)value;
            string filePath = null;
            if (!string.IsNullOrEmpty(file))
            {
                filePath = FileHelper.GetImagePath(file);
            }
            // Return the local filepath value to pass to the target.
            return filePath;
        }

        // ConvertBack is not implemented for a OneWay binding.
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
