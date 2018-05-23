using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Data;

namespace OmniLaunch.ValueConverters
{
    class FilePathToFileName : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            ObservableCollection<string> temp = (ObservableCollection<string>)value;
            for (int i = 0; i < temp.Count; i++)
            {
                temp[i] = temp[i].Substring(temp[i].LastIndexOf(@"Thermo\Nautilus\") + 16);
            }            
            return temp;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return string.Empty;
        }
    }
}
