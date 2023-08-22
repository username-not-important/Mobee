using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using Localization;
using Localization.Windows;
using Localization.Windows.Converters;

namespace Mobee.Client.WPF.Utilities
{
    public class MobeeTranslateExtension : TranslateExtension
    {
        public bool IsOneWay { get; set; } = false;

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            Binding binding = new Binding();
            binding.Source = (object) LocalizationManager.Instance;
            binding.Path = new PropertyPath("[" + this.ResourceKey + "]", Array.Empty<object>());
            binding.Converter = (IValueConverter) new NullToDefaultConverter();
            binding.ConverterParameter = (object) (this.DefaultValue ?? this.ResourceKey);
            binding.StringFormat = this.StringFormat;

            if (IsOneWay)
                binding.Mode = BindingMode.OneWay;

            return binding.ProvideValue(serviceProvider);
        }
    }
}
