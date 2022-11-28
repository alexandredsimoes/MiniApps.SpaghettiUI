using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace MiniApps.SpaghettiUI.Converters
{
    public class AtivoTextEffectConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return null;

            var result = bool.Parse(value.ToString());

            if (!result)
            {
                
                var be = new BlurEffect();
                be.KernelType = KernelType.Box;
                be.Radius = 2.3;
                be.RenderingBias = RenderingBias.Performance;
                //var te = new TextEffectCollection();
                //te.Add(be as TextEffect);
                return be;
            }

            return null;

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
