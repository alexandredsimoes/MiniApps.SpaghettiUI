using MiniApps.SpaghettiUI.Models;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MiniApps.SpaghettiUI.ViewModels
{
    public class ProjetoItemDialogPageViewModel : BindableBase, IDialogAware
    {
        private readonly IDialogService _dialogService;
        private DelegateCommand<ProjetoItemRespostaDto> _respostaCommand;

        public ProjetoItemDialogPageViewModel(IDialogService dialogService = null)
        {
            _dialogService = dialogService;
        }

        public DelegateCommand<ProjetoItemRespostaDto> RespostaCommand =>
            _respostaCommand ?? (_respostaCommand = new DelegateCommand<ProjetoItemRespostaDto>(ExecuteRespostaCommand));

        public string Title => "Projeto Item";

        public ProjetoItemDto Item { get; private set; }

        public event Action<IDialogResult> RequestClose;

        public bool CanCloseDialog()
        {
            return true;
        }

        public void OnDialogClosed()
        {

        }

        public void OnDialogOpened(IDialogParameters parameters)
        {
            Item = parameters.GetValue<ProjetoItemDto>("detalhe");
            RaisePropertyChanged(nameof(Item));
        }

        void ExecuteRespostaCommand(ProjetoItemRespostaDto dto)
        {
            var parameters = new DialogParameters();
            parameters.Add("detalhe", dto);
            _dialogService.ShowDialog("ProjetoItemRespostaDialogPage", parameters, result =>
            {

            });
        }
    }
}
