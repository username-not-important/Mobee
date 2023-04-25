using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using Mobee.Client.WPF.Utilities;

namespace Mobee.Client.WPF.Converters
{
    public class MessageTextToFontSizeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double baseSize = 12;
            if (parameter is double d)
                baseSize = d;

            var messageText = value as string;
            if (string.IsNullOrWhiteSpace(messageText))
                return baseSize;

            return baseSize + calculateIncrement(messageText);
        }

        private double calculateIncrement(string messageText)
        {
            var stringInfo = new StringInfo(messageText);
            var length = stringInfo.LengthInTextElements;

            if (length == 1 && messageText.HasEmoji())
                return 20;

            if (length == 2 && messageText.HasEmoji())
                return 8;

            if (length is > 2 and < 5 && messageText.IsAllEmoji())
                return 6;

            return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
