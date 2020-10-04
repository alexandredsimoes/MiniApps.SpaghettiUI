using Microsoft.AspNetCore.Builder;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniApps.SpaghettiUI.Services
{
    public class AppState
    {
        public ObservableCollection<(Guid,WebApplication)> Applications { get; set; } = new ObservableCollection<(Guid,WebApplication)>();

        public AppState()
        {
            Applications.CollectionChanged += Applications_CollectionChanged;
        }

        ~AppState()
        {
            Applications.CollectionChanged -= Applications_CollectionChanged;
        }

        private async void Applications_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if( e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                await (((Guid,WebApplication))e.NewItems[0]).Item2.RunAsync();
            }
            else if(e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
            {
                
            }
        }
    }
}
