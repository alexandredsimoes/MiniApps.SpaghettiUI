using System;
using System.Threading.Tasks;
using GreenPipes;
using JD.PI.GestaoContaPI.Worker.Contracts;
using MassTransit;
using Prism.Commands;
using Prism.Mvvm;

namespace MiniApps.SpaghettiUI.ViewModels
{
    public class QueueManagerViewModel : BindableBase
    {
        private DelegateCommand _postarFilaCommand;
        private string nomeFila = "";
        private string servidor = "10.10.20.36";
        private string usuario = "jdpi";
        private string senha = "jdpi";


        public string NomeFila
        {
            get { return nomeFila; }
            set { SetProperty(ref nomeFila, value); }
        }
        public string Servidor
        {
            get { return servidor; }
            set { SetProperty(ref servidor, value); }
        }

        public string Usuario
        {
            get { return usuario; }
            set { SetProperty(ref usuario, value); }
        }

        public string Senha
        {
            get { return senha; }
            set { SetProperty(ref senha, value); }
        }

        public QueueManagerViewModel()
        {
        }




        public DelegateCommand PostarFilaCommand => _postarFilaCommand ?? (_postarFilaCommand = new DelegateCommand(ExecutePostarFilaCommandAsync));

        async void ExecutePostarFilaCommandAsync()
        {
            var bus = Bus.Factory.CreateUsingRabbitMq(sbc =>
            {
                sbc.Host(Servidor, 5672, "alexandre", c =>
                {
                    c.Username(Usuario);
                    c.Password(Senha);
                    c.ConfigureBatchPublish(b =>
                    {
                    });
                });

                
                sbc.Message<AporteRbCLCommand>(config =>
                {
                    config.SetEntityName(AporteRbCLCommand.EntityName);
                });

            });

            await bus.StartAsync(); // This is important!

            var aporte = new AporteRbCLCommand()
            {
                NumCtrlIf = $"JDPI{DateTime.Now:yyMMMddHHmmssfff}",
                DtMovimento = DateTime.Now,
                IspbIf = 12345678,
                IspbPspi = 87654321,
                Valor = 50000
            };

            await bus.Publish(aporte);

            //await bus.Send<AporteRbCLCommand>(aporte);

            await bus.StopAsync();
        }
    }
}
