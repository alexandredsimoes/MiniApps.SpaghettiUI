using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Threading;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

using MiniApps.SpaghettiUI.Constants;
using MiniApps.SpaghettiUI.Contracts.Services;
using MiniApps.SpaghettiUI.Core;
using MiniApps.SpaghettiUI.Core.Contracts;
using MiniApps.SpaghettiUI.Core.Contracts.Services;
using MiniApps.SpaghettiUI.Core.Models;
using MiniApps.SpaghettiUI.Core.Services;
using MiniApps.SpaghettiUI.Models;
using MiniApps.SpaghettiUI.Services;
using MiniApps.SpaghettiUI.ViewModels;
using MiniApps.SpaghettiUI.Views;

using Prism.Ioc;
using Prism.Mvvm;
using Prism.Unity;

using Unity;

namespace MiniApps.SpaghettiUI
{
    // For more inforation about application lifecyle events see https://docs.microsoft.com/dotnet/framework/wpf/app-development/application-management-overview
    // For docs about using Prism in WPF see https://prismlibrary.com/docs/wpf/introduction.html

    // WPF UI elements use language en-US by default.
    // If you need to support other cultures make sure you add converters and review dates and numbers in your UI to ensure everything adapts correctly.
    // Tracking issue for improving this is https://github.com/dotnet/wpf/issues/1946
    public partial class App : PrismApplication
    {
        private string[] _startUpArgs;

        public App()
        {
        }

        protected override Window CreateShell()
            => Container.Resolve<ShellWindow>();

        protected override async void OnInitialized()
        {
            var persistAndRestoreService = Container.Resolve<IPersistAndRestoreService>();
            persistAndRestoreService.RestoreData();

            var themeSelectorService = Container.Resolve<IThemeSelectorService>();
            themeSelectorService.SetTheme();

            base.OnInitialized();
            await Task.CompletedTask;
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            _startUpArgs = e.Args;
            base.OnStartup(e);
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            // Core Services
            containerRegistry.Register<IFileService, FileService>();

            // App Services
            containerRegistry.Register<IApplicationInfoService, ApplicationInfoService>();
            containerRegistry.Register<ISystemService, SystemService>();
            containerRegistry.Register<IPersistAndRestoreService, PersistAndRestoreService>();
            containerRegistry.Register<IThemeSelectorService, ThemeSelectorService>();
            containerRegistry.Register<ISampleDataService, SampleDataService>();
            containerRegistry.Register<IProjetoService, ProjetoService>();

            // Views
            containerRegistry.RegisterForNavigation<QueueManagerPage, QueueManagerViewModel>(PageKeys.QueueManager);
            containerRegistry.RegisterForNavigation<SettingsPage, SettingsViewModel>(PageKeys.Settings);
            containerRegistry.RegisterForNavigation<MasterDetailPage, MasterDetailViewModel>(PageKeys.MasterDetail);
            containerRegistry.RegisterForNavigation<MainPage, MainViewModel>(PageKeys.Main);
            containerRegistry.RegisterForNavigation<ShellWindow, ShellViewModel>();
            containerRegistry.RegisterForNavigation<ProjetoPage, ProjetoViewModel>(PageKeys.Projeto);


            //
            containerRegistry.RegisterSingleton<AppState>();

            // Configuration
            var configuration = BuildConfiguration();
            var appConfig = configuration
                .GetSection(nameof(AppConfig))
                .Get<AppConfig>();

            // Register configurations to IoC
            containerRegistry.RegisterInstance<IConfiguration>(configuration);
            containerRegistry.RegisterInstance<AppConfig>(appConfig);


            containerRegistry.RegisterSingleton<IApplicationDbContext, ApplicationDbContext>();

            var db = containerRegistry.GetContainer().Resolve<ApplicationDbContext>();
            db.Database.EnsureDeleted();
            db.Database.EnsureCreated();


#if DEBUG
            //""dtHrRequisicao"":""2020-01-23T22:10:05.025Z""
            var projetoId = Guid.NewGuid();
            var respostaAporte = @"{
	                                            ""idRequisicao"":""#guid#"",
	                                            ""dtHrRequisicao"":""#datenowutc#""
                                            }";
            db.Projetos.AddRange(new Projeto()
            {
                Id = projetoId,
                Nome = "Gestão Conta PI",
                PortaPadrao = 5001,
                ExibirLog = true,
                Items = new List<ProjetoItem>()
                {
                    new ProjetoItem()
                    {
                        ProjetoId = projetoId,
                        Descricao = "Aporte RB ou CL",
                        RespostaPadrao = respostaAporte,
                        CodigoHttpPadrao = 200,
                        Metodo = MetodoHttp.MhPost,
                        Endpoint = "/jdpi/conta/api/v1/aporte/rbcl"
                    },
                    new ProjetoItem()
                    {
                        ProjetoId = projetoId,
                        Descricao = "Aporte CCME",
                        RespostaPadrao = respostaAporte,
                        CodigoHttpPadrao = 200,
                        Metodo = MetodoHttp.MhPost,
                        Endpoint = "/jdpi/conta/api/v1/aporte/ccme"
                    },
                    new ProjetoItem()
                    {
                        ProjetoId = projetoId,
                        Descricao = "Aporte TPF",
                        RespostaPadrao = respostaAporte,
                        CodigoHttpPadrao = 200,
                        Metodo = MetodoHttp.MhPost,
                        Endpoint = "/jdpi/conta/api/v1/aporte/tpf"
                    },
                    new ProjetoItem()
                    {
                        ProjetoId = projetoId,
                        Descricao = "Aporte Automatico",
                        RespostaPadrao = respostaAporte,
                        CodigoHttpPadrao = 200,
                        Metodo = MetodoHttp.MhPost,
                        Endpoint = "/jdpi/conta/api/v1/aporte/automatico"
                    },
                    new ProjetoItem()
                    {
                        ProjetoId = projetoId,
                        Descricao = "Saque RbCl",
                        RespostaPadrao = respostaAporte,
                        CodigoHttpPadrao = 200,
                        Metodo = MetodoHttp.MhPost,
                        Endpoint = "/jdpi/conta/api/v1/saque/rbcl"
                    },
                    new ProjetoItem()
                    {
                        ProjetoId = projetoId,
                        Descricao = "Saque CcMe",
                        RespostaPadrao = respostaAporte,
                        CodigoHttpPadrao = 200,
                        Metodo = MetodoHttp.MhPost,
                        Endpoint = "/jdpi/conta/api/v1/saque/ccme"
                    },
                    new ProjetoItem()
                    {
                        ProjetoId = projetoId,
                        Descricao = "Saque Tpf",
                        RespostaPadrao = respostaAporte,
                        CodigoHttpPadrao = 200,
                        Metodo = MetodoHttp.MhPost,
                        Endpoint = "/jdpi/conta/api/v1/saque/tpf"
                    },
                    new ProjetoItem()
                    {
                        ProjetoId = projetoId,
                        Descricao = "Consulta situação Aporte RCBL",
                        CodigoHttpPadrao = 200,
                        Metodo = MetodoHttp.MhGet,
                        Endpoint = "/jdpi/conta/api/v1/consultarsitreq",
                        Respostas = new List<ProjetoItemResposta>()
                        {
                            new ProjetoItemResposta()
                            {
                                Condicao = "#query-tpRequisicao#=0",
                                CodigoHttp = 200,
                                Resposta =  @"{
	                                        ""idRequisicao"":""#query-idRequisicao#"",
	                                        ""tpRequisicao"":#query-tpRequisicao#,
	                                        ""aporteRBCL"": {
		                                        ""dtHrSituacao"":""2020-01-24T20:20:05.015Z"",
		                                        ""situacao"":0,
		                                        ""descSituacao"":"""",
		                                        ""numCtrlIF"":""#query-idRequisicao#"",
		                                        ""ispbIF"":4358798,
		                                        ""numCtrlSTR"":""#query-idRequisicao#"",
		                                        ""sitLancSTR"":1,
		                                        ""valor"":50000.25,
		                                        ""dtHrSitBC"":""2020-01-24T19:50:27.108Z"",
		                                        ""dtMovimento"":""2020-01-24""
	                                        }
                                        } ",
                            },
                            new ProjetoItemResposta()
                            {
                                Condicao = "#query-tpRequisicao#=1",
                                CodigoHttp = 200,
                                Resposta =  @"{
	                                        ""idRequisicao"":""#query-idRequisicao#"",
	                                        ""tpRequisicao"":#query-tpRequisicao#,
	                                        ""aporteCCME"": {
		                                        ""dtHrSituacao"":""2020-01-24T20:20:05.015Z"",
		                                        ""situacao"":1,
		                                        ""descSituacao"":"""",
		                                        ""numCtrlIEME"":""#query-idRequisicao#"",
		                                        ""ispbIEME"":4358798,
		                                        ""numCtrlSTR"":""STR20200124000000001"",
		                                        ""sitLancSTR"":1,
		                                        ""valor"":50000.25,
		                                        ""dtHrSitBC"":""2020-01-24T19:50:27.108Z"",
		                                        ""dtMovimento"":""2020-01-24""
	                                        }
                                        }",
                            },
                            new ProjetoItemResposta()
                            {
                                Condicao = "#query-tpRequisicao#=2",
                                CodigoHttp = 200,
                                Resposta = @"{
	                                            ""idRequisicao"":""#query-idRequisicao#"",
	                                            ""tpRequisicao"":#query-tpRequisicao#,
	                                            ""aporteTPF"": {
		                                            ""dtHrSituacao"":""2020-01-24T20:20:05.015Z"",
		                                            ""situacao"":2,
		                                            ""descSituacao"":"""",
		                                            ""numCtrlIF"":""#query-idRequisicao#"",
		                                            ""ispbIF"":4358798,
		                                            ""numOperacao"":123456,
		                                            ""numOperacaoRet"":234567,
		                                            ""numCtrlSTR"":""STR20200124000000001"",
		                                            ""sitOpSEL"":""ATU"",
		                                            ""valor"":50000.25,
		                                            ""dtHrSitBC"":""2020-01-24T19:50:27.108Z"",
		                                            ""dtMovimento"":""2020-01-24""
	                                            }
                                            }
                                            "
                            },
                            new ProjetoItemResposta()
                            {
                                Condicao = "#query-tpRequisicao#=3",
                                CodigoHttp = 200,
                                Resposta = @"{
	                                            ""idRequisicao"":""#query-idRequisicao#"",
	                                            ""tpRequisicao"":#query-tpRequisicao#,
	                                            ""saqueRBCL"": {
		                                            ""dtHrSituacao"":""2020-01-24T20:20:05.015Z"",
		                                            ""situacao"":0,
		                                            ""descSituacao"":"""",
		                                            ""numCtrlPSPI"":""#query-idRequisicao#"",
		                                            ""ispbPSPI"":4358798,
		                                            ""numCtrlSTR"":""STR20200124000000001"",
		                                            ""sitLancSTR"":1,
		                                            ""valor"":50000.25,
		                                            ""dtHrSitBC"":""2020-01-24T19:50:27.108Z"",
		                                            ""dtMovimento"":""2020-01-24""
	                                            }
                                            }
                                            "
                            },
                            new ProjetoItemResposta()
                            {
                                Condicao = "#query-tpRequisicao#=4",
                                CodigoHttp = 200,
                                Resposta = @"{
	                                            ""idRequisicao"":""#query-idRequisicao#"",
	                                            ""tpRequisicao"":#query-tpRequisicao#,
	                                            ""saqueCCME"": {
		                                            ""dtHrSituacao"":""2020-01-24T20:20:05.015Z"",
		                                            ""situacao"":0,
		                                            ""descSituacao"":"""",
		                                            ""numCtrlPSPI"":""#query-idRequisicao#"",
		                                            ""ispbPSPI"":4358798,
		                                            ""numCtrlSTR"":""STR20200124000000001"",
		                                            ""sitLancSTR"":1,
		                                            ""valor"":50000.25,
		                                            ""dtHrSitBC"":""2020-01-24T19:50:27.108Z"",
		                                            ""dtMovimento"":""2020-01-24""
	                                            }
                                            }
                                            "
                            } ,
                            new ProjetoItemResposta()
                            {
                                Condicao = "#query-tpRequisicao#=5",
                                CodigoHttp = 200,
                                Resposta = @"{
	                                            ""idRequisicao"":""#query-idRequisicao#"",
	                                            ""tpRequisicao"":#query-tpRequisicao#,
	                                            ""saqueTPF"": {
		                                            ""dtHrSituacao"":""2020-01-24T20:20:05.015Z"",
		                                            ""situacao"":0,
		                                            ""descSituacao"":"""",
		                                            ""numCtrlIF"":""#query-idRequisicao#"",
		                                            ""ispbIF"":4358798,
		                                            ""numOperacao"":123456,
		                                            ""numCtrlSTR"":""STR20200124000000001"",
		                                            ""sitOpSEL"":""CON"",
		                                            ""valor"":50000.25,
		                                            ""dtHrSitBC"":""2020-01-24T19:50:27.108Z"",
		                                            ""dtMovimento"":""2020-01-24""
	                                            }
                                            }
                                            "
                            },
                            new ProjetoItemResposta()
                            {
                                Condicao = "#query-tpRequisicao#=6",
                                CodigoHttp = 200,
                                Resposta = @"{
	                                            ""idRequisicao"":""#query-idRequisicao#"",
	                                            ""tpRequisicao"":#query-tpRequisicao#,
	                                            ""configAporteAuto"": {
		                                            ""dtHrSituacao"":""2020-01-24T20:20:05.015Z"",
		                                            ""situacao"":0,
		                                            ""descSituacao"":"""",
		                                            ""numCtrlPSPI"":""#query-idRequisicao#"",
		                                            ""ispbPSPI"":4358798,
		                                            ""percSaldoRBCL"":20.75,
		                                            ""valorRBCL"":0,
		                                            ""percSaldoCCME"":0,
		                                            ""valorCCME"":570000.50,
		                                            ""dtHrSitBC"":""2020-01-24T19:50:27.108Z"",
		                                            ""dtMovimento"":""2020-01-24""
	                                            }
                                            }
                                            "
                            }
                        }
                    }
                }
            },
            new Projeto()
            {
                Nome = "Bacen Messages" ,
                PortaPadrao = 5002,
                ExibirLog = true,
                Items = new List<ProjetoItem>()
                {
                    new ProjetoItem()
                    {
                        Metodo = MetodoHttp.MhGet,
                        CodigoHttpPadrao = 200,
                        Descricao = "Mensagens Pacs,Admi,Reda" ,
                        Endpoint ="/consulta/{ispb}/msgs",
                        RespostaPadrao = "Pacs008"
                    }
                }
            });
            db.SaveChanges();
#endif
        }

        private IConfiguration BuildConfiguration()
        {
            var appLocation = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            return new ConfigurationBuilder()
                .SetBasePath(appLocation)
                .AddJsonFile("appsettings.json")
                .AddCommandLine(_startUpArgs)
                .Build();
        }

        private void OnExit(object sender, ExitEventArgs e)
        {
            var appState = Container.Resolve<AppState>();
            foreach (var item in appState.Applications)
            {
                item.Item2.StopAsync().ConfigureAwait(false);
                item.Item2.Dispose();
            }
            var persistAndRestoreService = Container.Resolve<IPersistAndRestoreService>();
            persistAndRestoreService.PersistData();
        }

        private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            // TODO WTS: Please log and handle the exception as appropriate to your scenario
            // For more info see https://docs.microsoft.com/dotnet/api/system.windows.application.dispatcherunhandledexception?view=netcore-3.0
        }
    }
}
