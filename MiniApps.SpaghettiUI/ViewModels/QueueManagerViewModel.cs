using System;
using System.Threading;
using System.Threading.Tasks;
using GreenPipes;
using JD.PI.GestaoContaPI.Contracts;
using JD.PI.GestaoContaPI.Contracts.Events;
using MassTransit;
using Prism.Commands;
using Prism.Mvvm;

namespace MiniApps.SpaghettiUI.ViewModels
{
    public class QueueManagerViewModel : BindableBase
    {
        private DelegateCommand<string> _postarFilaCommand;
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




        public DelegateCommand<string> PostarFilaCommand => _postarFilaCommand ?? (_postarFilaCommand = new DelegateCommand<string>(ExecutePostarFilaCommandAsync));

        async void ExecutePostarFilaCommandAsync(string tipo)
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

                sbc.Message<AporteCcMeCommand>(config =>
                {
                    config.SetEntityName(AporteCcMeCommand.EntityName);
                });

                sbc.Message<SaqueCcMeCommand>(config =>
                {
                    config.SetEntityName(SaqueCcMeCommand.EntityName);
                });

                sbc.Message<SaqueRbClCommand>(config =>
                {
                    config.SetEntityName(SaqueRbClCommand.EntityName);
                });

                sbc.Message<ConsultaSaldoRbClCommand>(config =>
                {
                    config.SetEntityName(ConsultaSaldoRbClCommand.EntityName);
                });

            });

            await bus.StartAsync(); // This is important!

            //var numCtrlIF = "JDPI20out30110751841";
            var numCtrlIF = $"JDPI{DateTime.Now.ToString("yyMMddHHmmssfff")}";// "JDPI20out30110751841";

            var aporteRbCl = new AporteRbCLCommand()
            {
                NumCtrlIf = numCtrlIF,
                DtMovimento = DateTime.Now,
                IspbIf = 04358798,
                IspbPspi = 04358798,
                Valor = 50000
            };
            
            numCtrlIF = $"JDPI{DateTime.Now.ToString("yyMMddHHmmss")}";// "JDPI20out30110751841";
            var aporteRbClManual = AporteRbCLCommand.CriarAporteManual(32997490,
                                                                       32997490,
                                                                       50_000,
                                                                       DateTime.Now,
                                                                       numCtrlIF,
                                                                       "123456",
                                                                       DateTime.Now.AddMinutes(10));

            numCtrlIF = $"JDPI{DateTime.Now.ToString("yyMMddHHmmss")}";// "JDPI20out30110751841";
            var aporteCcMe = new AporteCcMeCommand()
            {
                NumCtrlIEME = numCtrlIF,
                DtMovimento = DateTime.Now,
                IspbIeme = 04358798,
                Valor = 1_000_000,
            };

            var aporteCcMeManual = AporteCcMeCommand.CriarAporteManual(32997490,
                                                                       50_000,
                                                                       DateTime.Now,
                                                                       numCtrlIF,
                                                                       "12345678",
                                                                       DateTime.Now);

            numCtrlIF = $"JDPI{DateTime.Now.ToString("yyMMddHHmmss")}";// "JDPI20out30110751841";
            var saqueRbCl = new SaqueRbClCommand()
            {
                NumCtrlPSPI = numCtrlIF,
                DtMovimento = DateTime.Now,
                IspbIfCreditada = 32997490,
                IspbPspi = 32997490,
                FinalidadeLpi = JD.PI.GestaoContaPI.Contracts.Common.Enum.FinalidadeLpi.FlMovimentacaoLiquidanteStr,
                ClienteCreditado = new ClienteCreditado()
                {
                    //AgCreditada = "8510",
                    //CnpjCliCreditado = "76330826000180",
                    //CtCreditada = "18982",
                },
                Valor = 10_000
            };

            var saqueRbClManual = SaqueRbClCommand.CriarAporteManual(32997490,
                                                                     numCtrlIF,
                                                                     32997490,
                                                                     JD.PI.GestaoContaPI.Contracts.Common.Enum.FinalidadeLpi.FlMovimentacaoLiquidanteStr,
                                                                     "",
                                                                     10_000,
                                                                     DateTime.Now,
                                                                     "123456789",
                                                                     DateTime.Now);

            numCtrlIF = $"JDPI{DateTime.Now.ToString("yyMMddHHmmss")}";// "JDPI20out30110751841";
            var saqueCcMe = new SaqueCcMeCommand()
            {
                NumCtrlPSPI = numCtrlIF,
                DtMovimento = DateTime.Now,
                IspbPspi = 32997490,
                Valor = 40_000,
            };

            var saqueCcMeManual = SaqueCcMeCommand.CriarAporteManual(32997490,
                                                                     40_000,
                                                                     DateTime.Now,
                                                                     numCtrlIF,
                                                                     "123456789",
                                                                     DateTime.Now);

            var consultaSaldoRbCl = new ConsultaSaldoRbClCommand()
            {
                DtMovimento = DateTime.Now,
                IspbIfLdl = 12345678,
                NumCtrlIfLdl = numCtrlIF
            };

            if (tipo == "aporteccme")
                await bus.Publish(aporteCcMe);

            await Task.Delay(1000);

            if (tipo == "saqueccme")
                await bus.Publish(saqueCcMe);


            if (tipo == "aporterbcl")
                await bus.Publish(aporteRbCl);

            if (tipo == "saquerbcl")
                await bus.Publish(saqueRbCl);

            
            if (tipo == "aporterbcl-manual")
                await bus.Publish(aporteRbClManual);

            if (tipo == "aporteccme-manual")
                await bus.Publish(aporteCcMeManual);

            if (tipo == "saquerbcl-manual")
                await bus.Publish(saqueRbClManual);

            if (tipo == "saqueccme-manual")
                await bus.Publish(saqueCcMeManual);

            if (tipo == "consultasaldorbcl")
            {
                var requestClient = bus.CreateRequestClient<ConsultaSaldoRbClCommand>(
                    destinationAddress: new Uri($"queue:{ConsultaSaldoRbClCommand.QueueName}"),
                    timeout: TimeSpan.FromSeconds(60));

                using var requestHandler = requestClient.Create(consultaSaldoRbCl, CancellationToken.None);

                requestHandler.UseExecute(c =>
                {
                    c.Durable = true;
                });

                var response = await requestHandler.GetResponse<ConsultaSaldoRbClEvent>().ConfigureAwait(false);
            }

            await bus.StopAsync();
        }
    }
}
