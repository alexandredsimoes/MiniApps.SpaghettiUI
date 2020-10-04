using MiniApps.SpaghettiUI.Core.Contracts.Services;
using MiniApps.SpaghettiUI.Core.Models;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Threading.Tasks;

namespace MiniApps.SpaghettiUI.ViewModels
{
    public class ProjetoViewModel : BindableBase, INavigationAware
    {
        private readonly IProjetoService _projetoService;
        
        private DelegateCommand<string> _endpointCommand;

        private Projeto _selecionado;
        public Projeto Selecionado
        {
            get { return _selecionado; }
            set { SetProperty(ref _selecionado, value); }
        }

              

        public ProjetoViewModel(IProjetoService projetoService)
        {
            _projetoService = projetoService;
        }

        public bool IsNavigationTarget(NavigationContext navigationContext) => true;
        public DelegateCommand<string> EndpointCommand =>
            _endpointCommand ?? (_endpointCommand = new DelegateCommand<string>(ExecuteEndpointCommand));

        void ExecuteEndpointCommand(string acao)
        {
            if(acao == "+")
            {
                Selecionado.Items.Add(new ProjetoItem()
                {
                    Endpoint = "/",
                    Metodo = Core.MetodoHttp.MhPost,
                });
                RaisePropertyChanged(nameof(Selecionado));
            }
            else if(acao == "-")
            {

            }
        }

        public async void OnNavigatedTo(NavigationContext navigationContext)
        {
            await CarregarDados((Guid)navigationContext.Parameters["Id"]);
        }

        private async Task CarregarDados(Guid id)
        {
            Selecionado = await _projetoService.ObterProjeto(id);
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
        }        
    }
}
