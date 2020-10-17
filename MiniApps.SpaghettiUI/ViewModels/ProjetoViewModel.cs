using MiniApps.SpaghettiUI.Core.Contracts.Services;
using MiniApps.SpaghettiUI.Core.Models;
using MiniApps.SpaghettiUI.Models;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace MiniApps.SpaghettiUI.ViewModels
{
    public class ProjetoViewModel : BindableBase, INavigationAware
    {
        private readonly IProjetoService _projetoService;
        
        private DelegateCommand<string> _endpointCommand;

        private ProjetoDto _selecionado;
        public ProjetoDto Selecionado
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
                Selecionado.Items.Insert(0, new ProjetoItemDto()
                {
                    CodigoHttpPadrao = 200,
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
            Selecionado = ToDto(await _projetoService.ObterProjeto(id));
        }

        private ProjetoDto ToDto(Projeto x)
        {
            return new ProjetoDto()
            {
                Icone = x.Icone,
                Id = x.Id,
                Nome = x.Nome,
                PortaPadrao = x.PortaPadrao,
                ExibirLog = x.ExibirLog,
                Items = new ObservableCollection<ProjetoItemDto>(x.Items.Select(x => new ProjetoItemDto()
                {
                    Id = x.Id,
                    Metodo = x.Metodo,
                    ProjetoId = x.ProjetoId,
                    CodigoHttpPadrao = x.CodigoHttpPadrao,
                    Descricao = x.Descricao,
                    Endpoint = x.Endpoint,
                    RespostaPadrao = x.RespostaPadrao,
                    Respostas = new ObservableCollection<ProjetoItemRespostaDto>(x.Respostas.Select(x => new ProjetoItemRespostaDto()
                    {
                        CodigoHttp = x.CodigoHttp,
                        Condicao = x.Condicao,
                        Resposta = x.Resposta,
                    }))
                }))
            };
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
        }        
    }
}
