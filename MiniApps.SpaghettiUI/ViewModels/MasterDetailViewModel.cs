using System;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using MiniApps.SpaghettiUI.Core.Contracts.Services;
using MiniApps.SpaghettiUI.Core.Models;
using MiniApps.SpaghettiUI.Core.Services;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;

namespace MiniApps.SpaghettiUI.ViewModels
{
    public class MasterDetailViewModel : BindableBase, INavigationAware
    {
        private readonly IProjetoService _projetoService;
        private Projeto _selected;

        public Projeto Selected
        {
            get { return _selected; }
            set { SetProperty(ref _selected, value); }
        }
        public ObservableCollection<Projeto> Projetos { get; private set; } = new ObservableCollection<Projeto>();

        public MasterDetailViewModel(IProjetoService projetoService)
        {
            _projetoService = projetoService;
        }

        public async void OnNavigatedTo(NavigationContext navigationContext)
        {

            Projetos.Clear();

            var projetos = await _projetoService.ListarProjetos();

            foreach (var projeto in projetos)
            {
                Projetos.Add(projeto);
            }

            Selected = Projetos.First();
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
            => true;

        private DelegateCommand _subirServidorCommand;
        public DelegateCommand SubirServidorCommand =>
            _subirServidorCommand ?? (_subirServidorCommand = new DelegateCommand(ExecuteSubirServidorCommand));

        async void ExecuteSubirServidorCommand()
        {
            //
            var app = WebApplication.Create();
            foreach (var endpoint in Selected.Items)
            {
                if (endpoint.Metodo == Core.MetodoHttp.MhGet)
                {
                    app.MapGet(endpoint.Endpoint, async (context) =>
                    {
                        await context.Response.WriteAsync(endpoint.RespostaPadrao);
                    });
                }

                if (endpoint.Metodo == Core.MetodoHttp.MhPost)
                {
                    app.MapPost(endpoint.Endpoint, async (context) =>
                    {
                        await context.Response.WriteAsync(endpoint.RespostaPadrao);
                    });
                }

            }
            await app.RunAsync();
        }
    }
}
