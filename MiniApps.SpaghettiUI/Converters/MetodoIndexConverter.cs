using MiniApps.SpaghettiUI.Core;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace MiniApps.SpaghettiUI.Converters
{
    public class MetodoIndexConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is MetodoHttp metodo)
            {
                return metodo switch
                {
                    MetodoHttp.MhDelete => (int)metodo,
                    MetodoHttp.MhPost => (int)metodo,
                    MetodoHttp.MhPatch => (int)metodo,
                    MetodoHttp.MhGet => (int)metodo,
                    MetodoHttp.MhPut => (int)metodo,
                    _ => throw new NotImplementedException()
                };
            }

            return -1;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int metodo)
            {
                return metodo switch
                {
                    0 => MetodoHttp.MhPost,
                    1 => MetodoHttp.MhGet,
                    2 => MetodoHttp.MhPut,
                    3 => MetodoHttp.MhDelete,
                    4 => MetodoHttp.MhPatch,
                    _ => throw new NotImplementedException()
                };
            }

            return -1;
        }
    }
}
