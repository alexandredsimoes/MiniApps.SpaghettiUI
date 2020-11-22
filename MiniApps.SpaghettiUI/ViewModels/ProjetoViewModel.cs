using MiniApps.SpaghettiUI.Constants;
using MiniApps.SpaghettiUI.Core.Contracts.Services;
using MiniApps.SpaghettiUI.Core.Models;
using MiniApps.SpaghettiUI.Models;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace MiniApps.SpaghettiUI.ViewModels
{
    public class ProjetoViewModel : BindableBase, INavigationAware
    {
        private readonly IProjetoService _projetoService;
        private readonly IDialogService _dialogService;
        private readonly IRegionNavigationService _navigationService;

        private DelegateCommand _addEndpointCommand;
        private DelegateCommand<object> _removeEndpointCommand;
        private DelegateCommand _salvarCommand;
        private DelegateCommand _removerCommand;
        private DelegateCommand<ProjetoItemDto> _selecionarRespostaCommand;


        private ProjetoDto _selecionado;
        public ProjetoDto Selecionado
        {
            get { return _selecionado; }
            set { SetProperty(ref _selecionado, value); }
        }



        public ProjetoViewModel(IProjetoService projetoService,
                                IDialogService dialogService,
                                IRegionManager regionManager)
        {
            _projetoService = projetoService;
            _dialogService = dialogService;
            _navigationService = regionManager.Regions[Regions.Main].NavigationService;
        }

        public bool IsNavigationTarget(NavigationContext navigationContext) => true;
        public DelegateCommand AddEndpointCommand =>
            _addEndpointCommand ?? (_addEndpointCommand = new DelegateCommand(ExecuteAddEndpointCommand));

        public DelegateCommand<object> RemoveEndpointCommand =>
            _removeEndpointCommand ?? (_removeEndpointCommand = new DelegateCommand<object>(ExecuteRemoveEndpointCommand));

        public DelegateCommand SalvarCommand =>
            _salvarCommand ?? (_salvarCommand = new DelegateCommand(ExecuteSalvarCommand));

        public DelegateCommand RemoverCommand =>
            _removerCommand ?? (_removerCommand = new DelegateCommand(ExecuteRemoverCommand));

        public DelegateCommand<ProjetoItemDto> SelecionarRespostaCommand =>
            _selecionarRespostaCommand ?? (_selecionarRespostaCommand = new DelegateCommand<ProjetoItemDto>(ExecuteSelecionarRespostaCommand));

        private void ExecuteSelecionarRespostaCommand(ProjetoItemDto dto)
        {
            var parameters = new DialogParameters();
            parameters.Add("detalhe", dto);
            _dialogService.ShowDialog("ProjetoItemDialogPage", parameters, result =>
             {

             });
        }

        private async void ExecuteRemoverCommand()
        {
            var result = MessageBox.Show("Deseja realmente remover esse endpoint?", "", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                if (await _projetoService.RemoverProjeto(_selecionado.Id))
                {
                    _navigationService.Journal.GoBack();
                }

            }
        }

        private async void ExecuteSalvarCommand()
        {

            //
            var result = await _projetoService.SalvarProjeto(ToEntity(Selecionado));

        }

        void ExecuteRemoveEndpointCommand(object item)
        {
            Selecionado.Items.Remove((ProjetoItemDto)item);
        }

        void ExecuteAddEndpointCommand()
        {
            Selecionado.Items.Insert(0, new ProjetoItemDto()
            {
                CodigoHttpPadrao = 200,
                Endpoint = "/",
                Metodo = Core.MetodoHttp.MhPost,
            });
            RaisePropertyChanged(nameof(Selecionado));
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
                        Descricao = x.Descricao,
                        CodigoHttp = x.CodigoHttp,
                        Condicao = x.Condicao,
                        Resposta = x.Resposta,
                    }))
                }))
            };
        }

        private Projeto ToEntity(ProjetoDto x)
        {
            return new Projeto()
            {
                Icone = x.Icone,
                Id = x.Id,
                Nome = x.Nome,
                PortaPadrao = x.PortaPadrao,
                ExibirLog = x.ExibirLog,
                Items = new List<ProjetoItem>(x.Items.Select(x => new ProjetoItem()
                {
                    Id = x.Id,
                    Metodo = x.Metodo,
                    ProjetoId = x.ProjetoId,
                    CodigoHttpPadrao = x.CodigoHttpPadrao,
                    Descricao = x.Descricao,
                    Endpoint = x.Endpoint,
                    RespostaPadrao = x.RespostaPadrao,
                    RespostaHeader = x.RespostaHeader,
                    Respostas = new List<ProjetoItemResposta>(x.Respostas.Select(x => new ProjetoItemResposta()
                    {
                        CodigoHttp = x.CodigoHttp,
                        Condicao = x.Condicao,
                        Resposta = x.Resposta,
                        Descricao = x.Descricao
                    }))
                }))
            };
        }
        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
        }
    }
}
