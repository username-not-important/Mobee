using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace Mobee.Client.WPF.Converters
{
    public class MessageTextToDirectionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not string || string.IsNullOrWhiteSpace(value.ToString()))
                return FlowDirection.LeftToRight;

            var mostlyRTLChars = (value.ToString() ?? string.Empty).Select(c => c is >= 'ؠ' and <= '৳' ? 1 : 0).Average() > 0.3;

            return mostlyRTLChars ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
