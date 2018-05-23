using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using Thermo.Datapool;

namespace ManualRamosAddon.ValueConverters
{
    class DatapoolTagToString : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return string.Empty;
            return ((Datapool.DPGroupTagName)value).m_group + "/" + ((Datapool.DPGroupTagName)value).m_tag + ":" + ((Datapool.DPGroupTagName)value).m_type;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return null;
            string str = (string)value;
            Datapool.DPGroupTagName tag = new Datapool.DPGroupTagName()
            {
                m_group = str.Substring(0, str.IndexOf('/')),
                m_tag = str.Substring(str.IndexOf('/') + 1, str.IndexOf(':')),
                m_type = Datapool.dpTypes.STRING           
            };
            return tag;
        }
    }
}
