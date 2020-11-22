using MiniApps.SpaghettiUI.Models;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MiniApps.SpaghettiUI.ViewModels
{
    public class ProjetoItemRespostaDialogPageViewModel : BindableBase , IDialogAware
    {
        public ProjetoItemRespostaDialogPageViewModel()
        {

        }

        public string Title => "Resposta";

        public ProjetoItemRespostaDto Item { get; private set; }

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
            Item = parameters.GetValue<ProjetoItemRespostaDto>("detalhe");
            RaisePropertyChanged(nameof(Item));
        }
    }
}
