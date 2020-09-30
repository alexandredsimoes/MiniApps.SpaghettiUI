using System;
using MiniApps.SpaghettiUI.Core.Contracts;
using Prism.Mvvm;

namespace MiniApps.SpaghettiUI.ViewModels
{
    public class MainViewModel : BindableBase
    {
        private readonly IApplicationDbContext _context;

        public MainViewModel(IApplicationDbContext context)
        {
            _context = context;
        }
    }
}
