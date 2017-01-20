using ARUP.IssueTracker.Classes;
using System;
using System.Globalization;
using System.Windows.Data;

namespace ARUP.IssueTracker.Converters
{
    [ValueConversion(typeof(int), typeof(string))]
    public class WatcherConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int count = (int)value;
            if (count == 0) 
            {
                return "no watcher";
            }
            else if (count == 1)
            {
                return "1 watcher";
            }
            else
            {
                return string.Format("{0} watchers", count);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

    }
}
