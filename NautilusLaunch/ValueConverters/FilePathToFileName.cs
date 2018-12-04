using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Data;
using System.Linq;
using System.Collections.Generic;

namespace OmniLaunch.ValueConverters
{
    class FilePathToFileName : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            IEnumerable<string> temp;
            if (value is ObservableCollection<ProgramModel>)
            {
                temp = ((ObservableCollection<ProgramModel>)value).Select(p=>p.Path.Substring(p.Path.LastIndexOf(@"Thermo\Nautilus\") + 16));
            }
            else if (value is ObservableCollection<string>)
            {
                temp = ((ObservableCollection<string>)value).Select(p=>p.Substring(p.LastIndexOf(@"Thermo\Nautilus\") + 16));
            }
            else
            {
                return string.Empty;
            }

            return temp;

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return string.Empty;
        }
    }
}
