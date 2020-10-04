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

    public class MetodoToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is MetodoHttp metodo)
            {
                return metodo switch
                {
                    MetodoHttp.MhDelete => "DELETE",
                    MetodoHttp.MhPost => "POST",
                    MetodoHttp.MhPatch => "PATCH",
                    MetodoHttp.MhGet => "GET",
                    MetodoHttp.MhPut => "PUT",
                    _ => throw new NotImplementedException()
                };
            }

            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
