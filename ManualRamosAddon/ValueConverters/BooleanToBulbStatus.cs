using System;
using System.Globalization;
using System.Windows.Data;

namespace ManualRamosAddon.ValueConverters
{
    class BooleanToBulbStatus : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool)
                return ((bool)value == true) ? ThermoWpfWidgets.BulbWidget.BulbStatus.Ok : ThermoWpfWidgets.BulbWidget.BulbStatus.Error;

            return ThermoWpfWidgets.BulbWidget.BulbStatus.Unknown;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter is ThermoWpfWidgets.BulbWidget.BulbStatus)
            {
                switch ((ThermoWpfWidgets.BulbWidget.BulbStatus)parameter)
                {
                    case ThermoWpfWidgets.BulbWidget.BulbStatus.Ok:
                        return true;
                    default:
                        break;
                }
            }
            return false;
        }
    }
}
