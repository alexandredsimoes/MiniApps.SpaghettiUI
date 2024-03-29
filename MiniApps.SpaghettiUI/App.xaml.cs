﻿using System;
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
            containerRegistry.RegisterDialog<ProjetoItemDialogPage, ProjetoItemDialogPageViewModel>();
            containerRegistry.RegisterDialog<ProjetoItemRespostaDialogPage, ProjetoItemRespostaDialogPageViewModel>();


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



#if DEBUG

            var db = containerRegistry.GetContainer().Resolve<ApplicationDbContext>();
            db.Database.EnsureDeleted();
            db.Database.EnsureCreated();
            //if (!db.Database.EnsureCreated())
            //{
            //    return;
            //}

            //""dtHrRequisicao"":""2020-01-23T22:10:05.025Z""
            var projetoId = Guid.NewGuid();
            var respostaAporte = @"{
	                                            ""idRequisicao"":""#json-numCtrlIf#"",
	                                            ""dtHrRequisicao"":""#datenowutc#"",
                                            }";

            var respostConsulta = @"{
	                                            ""idRequisicao"":""#json-NumCtrlIfLdl#"",
	                                            ""dtHrRequisicao"":""#datenowutc#"",
                                            }";
            db.Projetos.AddRange(
                new Projeto()
                {
                    Id = projetoId,
                    Nome = "Gestão Conta PI",
                    PortaPadrao = 5082,
                    PortaPadraoHttps = 5081,
                    ExibirLog = true,
                    DataCriacao = DateTime.Now,
                    Items = new List<ProjetoItem>()
                {
                    new ProjetoItem()
                    {
                        ProjetoId = projetoId,
                        Descricao = "Consulta Saldo RB ou CL",
                        RespostaPadrao = respostConsulta,
                        CodigoHttpPadrao = 200,
                        Metodo = MetodoHttp.MhPost,
                        Endpoint = "/jdspb/conta/api/v1/consultar-saldo/rbcl"
                    },
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
                        Descricao = "Consulta Aportes/Retiradas",
                        RespostaPadrao = respostaAporte,
                        CodigoHttpPadrao = 200,
                        Metodo = MetodoHttp.MhGet,
                        Endpoint = "/conta/api/v1/listar/aporte-retirada",
                        Respostas = new List<ProjetoItemResposta>()
                        {
                            new ProjetoItemResposta()
                            {
                                Descricao = "Listagem de aportes e retiradas",
                                CodigoHttp = 200,
                                Resposta =  @"{
	                                            ""dtHrResultado"": ""2021-12-15T17:10:05.025Z"",
	                                            ""resultado"": [
		                                            {
			                                            ""tpOperacao"": 1,
			                                            ""situacao"": 0,
			                                            ""numCtrlSTR"": ""STR20211215000000001"",
			                                            ""numCtrl"": ""c61f126d-efa1-4ff0-8"",
			                                            ""dtHrSitBC"": ""2021-12-15T16:34:22""
		                                            },
		                                            {
			                                            ""tpOperacao"": 1,
			                                            ""situacao"": 0,
			                                            ""numCtrlSTR"": ""STR20211215000000002"",
			                                            ""numCtrl"": ""JDPI21DEZ15182615002"",
			                                            ""dtHrSitBC"": ""2021-12-15T16:35:23""
		                                            },
		                                            {
			                                            ""tpOperacao"": 0,
			                                            ""situacao"": 0,
			                                            ""numCtrlSTR"": ""STR20211215000000003"",
			                                            ""numCtrl"": ""JDPI21DEZ15182615003"",
			                                            ""dtHrSitBC"": ""2021-12-15T16:35:23""
		                                            },
		                                            {
			                                            ""tpOperacao"": 4,
			                                            ""situacao"": 1,
			                                            ""numCtrl"": ""JDPI21DEZ15182615004"",
			                                            ""dtHrSitBC"": ""2021-12-15T16:35:23""
		                                            },
		                                            {
			                                            ""tpOperacao"": 3,
			                                            ""situacao"": 2,
			                                            ""numCtrl"": ""JDPI21DEZ15182615005"",
			                                            ""dtHrSitBC"": ""2021-12-15T16:35:23""
		                                            },
		                                            {
			                                            ""tpOperacao"": 4,
			                                            ""situacao"": 0,
			                                            ""numCtrlSTR"": ""STR20211215000000005"",
			                                            ""numCtrl"": ""JDPI21DEZ15182615006"",
			                                            ""dtHrSitBC"": ""2021-12-15T16:35:23""
		                                            }
	                                            ]
                                            }"
                            },
                        }
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
                                Descricao = "Aporte RBCL com sucesso",
                                Condicao = "#query-tpRequisicao#=0",
                                CodigoHttp = 200,
                                //Resposta =  @"{
	                               //         ""idRequisicao"":""#query-idRequisicao#"",
	                               //         ""tpRequisicao"":#query-tpRequisicao#,
	                               //         ""aporteRBCL"": {
		                              //          ""dtHrSituacao"":""#datenow#"",
		                              //          ""situacao"":3,
		                              //          ""descSituacao"":""EGEN0300 - Mensagem Fora do Horário"",
		                              //          ""numCtrlIF"":""#query-idRequisicao#"",
		                              //          ""ispbIF"":32997490,		                                        
		                              //          ""valor"":10000,
	                               //         }
                                //        } ",
                                Resposta =  @"{
	                                        ""idRequisicao"":""#query-idRequisicao#"",
	                                        ""tpRequisicao"":#query-tpRequisicao#,
	                                        ""aporteRBCL"": {
		                                        ""dtHrSituacao"":""#datenow#"",
                                                ""dtHrSitBC"" : ""#datenowutc#"",
		                                        ""situacao"":0,
		                                        ""descSituacao"":"""",
		                                        ""numCtrlIF"":""#query-idRequisicao#"",
		                                        ""ispbIF"":32997490,		                                        
		                                        ""valor"":50000.25,
                                                ""sitLancSTR"":1,
                                                ""dtMovimento"":""#datenow#""
	                                        }
                                        } ",
                            },
                            new ProjetoItemResposta()
                            {
                                Descricao = "Consulta saldo RBCL",
                                Condicao = "#query-tpRequisicao#=7",
                                CodigoHttp = 200,
                                Resposta =  @"{
	                                        ""idRequisicao"":""#query-idRequisicao#"",
	                                        ""tpRequisicao"":#query-tpRequisicao#,
	                                        ""consultaSaldoRBCL"": {
		                                        ""dtHrSituacao"":""#datenow#"",
		                                        ""situacao"":0,
		                                        ""descSituacao"":"""",
		                                        ""numCtrlIfLDL"":""#query-idRequisicao#"",
		                                        ""ispbIfLDL"":32997490,		                                        
		                                        ""saldoRBCL"":1000000,
		                                        ""dtHrBC"":""#datenow#"",
		                                        ""dtMovimento"":""#datenow#""
	                                        }
                                        }",
                            },
                            new ProjetoItemResposta()
                            {
                                Descricao = "Aporte CCME com erro",
                                Condicao = "#query-tpRequisicao#=1",
                                CodigoHttp = 200,
                                Resposta =  @"{
	                                        ""idRequisicao"":""#query-idRequisicao#"",
	                                        ""tpRequisicao"":#query-tpRequisicao#,
	                                        ""aporteCCME"": {
		                                        ""dtHrSituacao"":""#datenow#"",
		                                        ""situacao"":0,
		                                        ""descSituacao"":"""",
		                                        ""numCtrlIEME"":""#query-idRequisicao#"",
		                                        ""ispbIEME"":32997490,
		                                        ""numCtrlSTR"":""STR20200124000000001"",
		                                        ""sitLancSTR"":1,
		                                        ""valor"":1000000,
		                                        ""dtHrSitBC"":""#datenow#"",
		                                        ""dtMovimento"":""#datenow#""
	                                        }
                                        }",
                            },
                            new ProjetoItemResposta()
                            {
                                Descricao = "Aporte TPF com sucesso",
                                Condicao = "#query-tpRequisicao#=2",
                                CodigoHttp = 200,
                                Resposta = @"{
	                                            ""idRequisicao"":""#query-idRequisicao#"",
	                                            ""tpRequisicao"":#query-tpRequisicao#,
	                                            ""aporteTPF"": {
		                                            ""dtHrSituacao"":""#datenow#"",
		                                            ""situacao"":0,
		                                            ""descSituacao"":"""",
		                                            ""numCtrlIF"":""#query-idRequisicao#"",
		                                            ""ispbIF"":32997490,
		                                            ""numOperacao"":123456,
		                                            ""numOperacaoRet"":234567,
		                                            ""numCtrlSTR"":""STR20200124000000001"",
		                                            ""sitOpSEL"":""ATU"",
		                                            ""valor"":50000.25,
		                                            ""dtHrSitBC"":""#datenow#"",
		                                            ""dtMovimento"":""#datenow#""
	                                            }
                                            }
                                            "
                            },
                            new ProjetoItemResposta()
                            {
                                Descricao = "Saque RBCL com sucesso",
                                Condicao = "#query-tpRequisicao#=3",
                                CodigoHttp = 200,
                                Resposta = @"{
	                                            ""idRequisicao"":""#query-idRequisicao#"",
	                                            ""tpRequisicao"":#query-tpRequisicao#,
	                                            ""saqueRBCL"": {
		                                            ""dtHrSituacao"":""#datenow#"",
		                                            ""situacao"":3,
		                                            ""descSituacao"":""EGEN0008 - ISPB Destinatário Não Informado; EGEN1001 - ISPB Inválido; EGEN0005 - ISPB Emissor Inválido;"",
		                                            ""numCtrlPSPI"":""#query-idRequisicao#"",
		                                            ""ispbPSPI"":32997490,		                                            
		                                            ""valor"":2000,
		                                            ""dtHrSitBC"":""#datenow#"",
		                                            ""dtMovimento"":""#datenow#""
	                                            }
                                            }
                                            "
                                //Resposta = @"{
	                               //             ""idRequisicao"":""#query-idRequisicao#"",
	                               //             ""tpRequisicao"":#query-tpRequisicao#,
	                               //             ""saqueRBCL"": {
		                              //              ""dtHrSituacao"":""#datenow#"",
		                              //              ""situacao"":3,
		                              //              ""descSituacao"":""ERRO BACEN"",
		                              //              ""numCtrlPSPI"":""#query-idRequisicao#"",
		                              //              ""ispbPSPI"":32997490,
		                              //              ""numCtrlSTR"":""STR2020333665889"",
		                              //              ""sitLancSTR"":1,
		                              //              ""valor"":2000,
		                              //              ""dtHrSitBC"":""#datenow#"",
		                              //              ""dtMovimento"":""#datenow#""
	                               //             }
                                //            }
                                //            "
                            },
                            new ProjetoItemResposta()
                            {
                                Descricao = "Saque CCME com sucesso",
                                Condicao = "#query-tpRequisicao#=4",
                                CodigoHttp = 200,
                                Resposta = @"{
	                                            ""idRequisicao"":""#query-idRequisicao#"",
	                                            ""tpRequisicao"":#query-tpRequisicao#,
	                                            ""saqueCCME"": {
		                                            ""dtHrSituacao"":""#datenow#"",
		                                            ""situacao"":0, 
		                                            ""descSituacao"":""Saque efetuado com sucesso"",
		                                            ""numCtrlPSPI"":""#query-idRequisicao#"",
		                                            ""ispbPSPI"":32997490,
		                                            ""sitLancSTR"":1,
		                                            ""valor"":5000,
                                                    ""dtHrSitBC"":""#datenow#"",
		                                            ""dtMovimento"":""#datenow#""
	                                            }
                                            }
                                            "
                            } ,
                            new ProjetoItemResposta()
                            {
                                Descricao = "Saque TPF com sucesso",
                                Condicao = "#query-tpRequisicao#=5",
                                CodigoHttp = 200,
                                Resposta = @"{
	                                            ""idRequisicao"":""#query-idRequisicao#"",
	                                            ""tpRequisicao"":#query-tpRequisicao#,
	                                            ""saqueTPF"": {
		                                            ""dtHrSituacao"":""#datenow#"",
		                                            ""situacao"":0,
		                                            ""descSituacao"":"""",
		                                            ""numCtrlIF"":""#query-idRequisicao#"",
		                                            ""ispbIF"":32997490,
		                                            ""numOperacao"":123456,
		                                            ""numCtrlSTR"":""STR20200124000000001"",
		                                            ""sitOpSEL"":""CON"",
		                                            ""valor"":50000.25,
		                                            ""dtHrSitBC"":""#datenow#"",
		                                            ""dtMovimento"":""#datenow#""
	                                            }
                                            }
                                            "
                            },
                            new ProjetoItemResposta()
                            {
                                Descricao = "Aporte automatico com sucesso",
                                Condicao = "#query-tpRequisicao#=6",
                                CodigoHttp = 200,
                                Resposta = @"{
	                                            ""idRequisicao"":""#query-idRequisicao#"",
	                                            ""tpRequisicao"":#query-tpRequisicao#,
	                                            ""configAporteAuto"": {
		                                            ""dtHrSituacao"":""#datenow#"",
		                                            ""situacao"":0,
		                                            ""descSituacao"":"""",
		                                            ""numCtrlPSPI"":""#query-idRequisicao#"",
		                                            ""ispbPSPI"":32997490,
		                                            ""percSaldoRBCL"":20.75,
		                                            ""valorRBCL"":0,
		                                            ""percSaldoCCME"":0,
		                                            ""valorCCME"":570000.50,
		                                            ""dtHrSitBC"":""#datenow#"",
		                                            ""dtMovimento"":""#datenow#""
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
                    Nome = "Bacen Messages",
                    PortaPadrao = 5013,
                    PortaPadraoHttps = 5012,
                    ExibirLog = true,
                    DataCriacao = DateTime.Now,
                    Items = new List<ProjetoItem>()
                {
                    new ProjetoItem()
                    {
                        Metodo = MetodoHttp.MhGet,
                        CodigoHttpPadrao = 200,
                        Descricao = "Saida de mensagem do BACEN PIX" ,
                        Endpoint ="/api/v1/out/{ispb}/stream/start",
                        RespostaPadrao = "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"no\"?>\n<Envelope xmlns=\"https://www.bcb.gov.br/pi/pacs.002/1.4\">\n    <AppHdr>\n        <Fr>\n            <FIId>\n                <FinInstnId>\n                    <Othr>\n                        <Id>00038166</Id>\n                    </Othr>\n                </FinInstnId>\n            </FIId>\n        </Fr>\n        <To>\n            <FIId>\n                <FinInstnId>\n                    <Othr>\n                        <Id>4358798</Id>\n                    </Othr>\n                </FinInstnId>\n            </FIId>\n        </To>\n        <BizMsgIdr>M99999010655f476b4435483f9578cf8</BizMsgIdr>\n        <MsgDefIdr>pacs.002.spi.1.4</MsgDefIdr>\n        <CreDt>2020-01-01T08:30:12.000Z</CreDt>\n        <Sgntr/>\n    </AppHdr>\n    <Document>\n        <FIToFIPmtStsRpt>\n            <GrpHdr>\n                <MsgId>M99999010655f476b4435483f9578cf8</MsgId>\n                <CreDtTm>2020-04-07T14:01:20.343Z</CreDtTm>\n            </GrpHdr>\n            <TxInfAndSts>\n                <OrgnlInstrId>E0435879820201119175707308958669</OrgnlInstrId>\n                <OrgnlEndToEndId>E0435879820201119175707308958669</OrgnlEndToEndId>\n                <TxSts>ACSP</TxSts>\n            </TxInfAndSts>\n        </FIToFIPmtStsRpt>\n    </Document>\n</Envelope>",
                    },
                    new ProjetoItem()
                    {
                        Metodo = MetodoHttp.MhPost,
                        CodigoHttpPadrao = 201,
                        Descricao = "Envio de mensagem para o BACEN PIX" ,
                        Endpoint ="/api/v1/in/{ispb}/msgs",
                        RespostaPadrao = "",
                        RespostaHeader="PI-ResourceId=123456789"
                    }
                }
                },
                new Projeto()
                {
                    ExibirLog = true,
                    Nome = "Integrador SINQIA",
                    PortaPadrao = 6529,
                    PortaPadraoHttps = 6528,
                    DataCriacao = DateTime.Now,
                    Items = new List<ProjetoItem>()
                    {
                        new ProjetoItem()
                        {
                            Metodo = MetodoHttp.MhPost,
                            CodigoHttpPadrao = 200,
                            Descricao = "Criação de lançamento em conta corrente" ,
                            Endpoint ="/BJ08M01/BJ08M01/BJ08SS0103B/lancarValorCC",
                            RespostaPadrao = "{\"nrSeqLct\":1,\"tpLanc\":1}",
                            TipoConteudo  = "application/json",
                            Ativo = true,
                        },
                        new ProjetoItem()
                        {
                            Metodo = MetodoHttp.MhPost,
                            CodigoHttpPadrao = 200,
                            Descricao = "Criação de lançamento TEF/DOC" ,
                            Endpoint ="/BJ08M01/BJ08M01/BJ08SS0101D/lancamentoTransferencia",
                            RespostaPadrao = "{\"cdStatus\":1,\"dsMsg\":1, \"nmFav\": \"Teste\"}",
                            TipoConteudo  = "application/json",
                            Ativo = true,
                        },
                        new ProjetoItem()
                        {
                            Metodo = MetodoHttp.MhPost,
                            CodigoHttpPadrao = 200,
                            Descricao = "Autenticação" ,
                            Endpoint ="/BJ08M01/user",
                            RespostaPadrao = "{" + Environment.NewLine +
                                            "    \"AccessToken\": \"dasdasd\"," + Environment.NewLine +
                                            "    \"ExpiresIn\": 1234558," + Environment.NewLine +
                                            "    \"TokenType\": \"bearer\"," + Environment.NewLine +
                                            "    \"Scope\": \"read write\"," + Environment.NewLine +
                                            "    \"Id\": 5555," + Environment.NewLine +
                                            "    \"UserName\": \"jdpi\"," + Environment.NewLine +
                                            "    \"NrInst\": 1406," + Environment.NewLine +
                                            "    \"NrAgen\": 1," + Environment.NewLine +
                                            "    \"NrAccess\": 25" + Environment.NewLine +
                                            "}",
                            TipoConteudo  = "application/json"  ,
                            RespostaHeader = "auth=2222222" ,
                            Ativo = true,
                        },
                        //new ProjetoItem()
                        //{
                        //    Metodo = MetodoHttp.MhPost,
                        //    CodigoHttpPadrao = 200,
                        //    Descricao = "Efetivação de crédito" ,
                        //    Endpoint ="/sistema-gestor-auth/jdpi/spi/api/v2/credito",
                        //    RespostaPadrao = "{\"IdReqJdPi\": \"\", \"IdCreditoSgct\": \"\", \"DtHrCreditoSgct\": \"\"}",
                        //    TipoConteudo  = "application/json" ,
                        //    Ativo = true,
                        //},
                        //new ProjetoItem()
                        //{   Metodo = MetodoHttp.MhPost,
                        //    CodigoHttpPadrao = 200,
                        //    Descricao = "Devolução recebida",
                        //    TipoConteudo = "application/json",
                        //    RespostaPadrao = @"{""IdReqJdPi"": ""a"", ""IdCreditoSgct"": ""a"", ""DtHrCreditoSgct"": ""a""}",
                        //    Ativo = true,
                        //    Endpoint = "/sistema-gestor-auth/jdpi/spi/api/v2/devolucao"
                        //},
                        new ProjetoItem()
                        {
                            Endpoint = "/jdpi/spi/api/v2/op",
                            Descricao = "JDPI - Ordem de pagamento",
                            Metodo = MetodoHttp.MhPost,
                            TipoConteudo = "application/json",
                            CodigoHttpPadrao = 200,
                            Respostas = new List<ProjetoItemResposta>()
                            {
                                new ProjetoItemResposta()
                                {
                                    CodigoHttp = 200,
                                    TipoConteudo= "application/json",
                                    Resposta = @"{
                                        ""IdReqSistemaCliente"" : ""#json-idReqSistemaCliente#"",
                                        ""IdReqJdPi"": ""#guid#"",
                                        ""EndToEndId"": ""#json-endToEndId#"",
                                        ""DtHrReqJdPi"": ""#datenowutc#""
                                    }",
                                },
                            }

                        },
                        new ProjetoItem()
                        {
                            Endpoint = "/jdpi/spi/api/v2/op/{IdRespostaJdPiApi}",
                            Descricao = "JDPI - Consulta Ordem de pagamento",
                            Metodo = MetodoHttp.MhGet,
                            TipoConteudo = "application/json",
                            CodigoHttpPadrao = 200,
                            Respostas = new List<ProjetoItemResposta>()
                            {
                                new ProjetoItemResposta()
                                {
                                    CodigoHttp = 200,
                                    TipoConteudo= "application/json",
                                    Resposta = @"{
                                        ""StJdPi"" : 9                                        
                                    }",
                                },
                            }

                        },
                        new ProjetoItem()
                        {
                            Endpoint = "/jdpi/connect/token",
                            Descricao = "JDPI - Autenticação",
                            RespostaPadrao = "{" + Environment.NewLine +
                                             "    \"access_token\": \"dasdasd\"," + Environment.NewLine +
                                             "    \"expires_in\": 1234558," + Environment.NewLine +
                                             "    \"token_type\": \"bearer\"," + Environment.NewLine +
                                             "    \"scope\": \"read write\"," + Environment.NewLine +
                                             "}",
                            Metodo = MetodoHttp.MhPost,
                            TipoConteudo = "application/json",
                            CodigoHttpPadrao = 200
                        },
                        new ProjetoItem()
                        {
                            Descricao = "Consulta",
                            Endpoint = "/BJ08M01/BJ08M01/BJ08SS0101B/consultaSaldo",
                            CodigoHttpPadrao = 200,
                            TipoConteudo = "application/json",
                            Metodo = MetodoHttp.MhPost,
                            RespostaPadrao = "{\"saldo\": \"0.00\"}",
                            Ativo = true,
                            Respostas = new List<ProjetoItemResposta>()
                            {
                                new ProjetoItemResposta()
                                {
                                    CodigoHttp = 200,
                                    TipoConteudo= "application/json",
                                    Resposta = @"{
                                                    ""ConsultaSaldo"": [ {""vlSaldo"": #random_currency# }]
                                                }"
                                },
                                new ProjetoItemResposta()
                                {
                                    CodigoHttp = 400,
                                    //TipoConteudo= "application/json",                                    
                                    Condicao = "#json-nrInst#=0",
                                    Resposta = @"-10 - Número de instituição não encontrada."
                                }
                            }
                        }
                    }
                },
                new Projeto()
                {
                    ExibirLog = true,
                    Nome = "CIP",
                    PortaPadrao = 6530,
                    PortaPadraoHttps = 6531,
                    DataCriacao = DateTime.Now,
                    Items = new List<ProjetoItem>()
                    {
                        new()
                        {
                            Descricao = "Consulta Guia de Arrecadação - Sucesso",
                            Endpoint = "/api/v1/guia-arrecadacao/consulta",
                            CodigoHttpPadrao = 200,
                            TipoConteudo = "application/json",
                            Metodo = MetodoHttp.MhGet,
                            RespostaPadrao = @"{
                                                  ""grpHdr"": {
                                                    ""msgId"": ""20221107000000000030"",
                                                    ""creDtTm"": ""2022-11-07T13:39:54"",
                                                    ""initgPty"": {
                                                      ""nm"": ""CIP"",
                                                      ""id"": {
                                                        ""orgId"": {
                                                          ""othr"": [
                                                            {
                                                              ""id"": ""02992335""
                                                            }
                                                          ]
                                                        }
                                                      }
                                                    },
                                                    ""nbOfTxs"": ""1""
                                                  },
                                                  ""pmtInf"": [
                                                    {
                                                      ""pmtInfId"": ""20221107000000000031"",
                                                      ""pmtMtd"": ""CSH"",
                                                      ""reqdExctnDt"": ""2022-11-03"",
                                                      ""xpryDt"": ""2022-11-03"",
                                                      ""dbtr"": {
                                                        ""nm"": ""Cereja Prunus avium"",
                                                        ""id"": {
                                                          ""orgId"": {
                                                            ""othr"": [
                                                              {
                                                                ""id"": ""07239345198820"",
                                                                ""schmeNm"": {
                                                                  ""prtry"": ""CNPJ""
                                                                }
                                                              }
                                                            ]
                                                          }
                                                        }
                                                      },
                                                      ""dbtrAgt"": {
                                                        ""finInstnId"": {
                                                          ""othr"": {
                                                            ""id"": ""55555555""
                                                          }
                                                        }
                                                      },
                                                      ""cdtTrfTx"": [
                                                        {
                                                          ""pmtId"": {
                                                            ""instrId"": ""74863"",
                                                            ""endToEndId"": ""83640095074470011550000000000000000000000000""
                                                          },
                                                          ""amt"": {
                                                            ""instdAmt"": ""9507447.00""
                                                          },
                                                          ""chrgBr"": ""SLEV"",
                                                          ""intrmyAgt1"": {
                                                            ""finInstnId"": {
                                                              ""othr"": {
                                                                ""id"": ""55555555""
                                                              }
                                                            }
                                                          },
                                                          ""cdtr"": {
                                                            ""nm"": ""BA11111111RSOC"",
                                                            ""id"": {
                                                              ""orgId"": {
                                                                ""othr"": [
                                                                  {
                                                                    ""id"": ""11111111""
                                                                  }
                                                                ]
                                                              }
                                                            }
                                                          },
                                                          ""rltdRmtInf"": [
                                                            {
                                                              ""rmtId"": ""618820445199107""
                                                            }
                                                          ],
                                                          ""rmtInf"": [
                                                            {
                                                              ""ustrd"": [
                                                                ""83640095074470011550000000000000000000000000""
                                                              ],
                                                              ""strd"": [
                                                                {
                                                                  ""rfrdDocInf"": [
                                                                    {
                                                                      ""tp"": {
                                                                        ""cdOrPrtry"": {
                                                                          ""prtry"": ""3""
                                                                        }
                                                                      },
                                                                      ""nb"": ""2022032200000000020"",
                                                                      ""rltdDt"": ""2022-11-03"",
                                                                      ""lineDtls"": [
                                                                        {
                                                                          ""id"": [
                                                                            {
                                                                              ""tp"": {
                                                                                ""cdOrPrtry"": {
                                                                                  ""prtry"": ""URLGUIA""
                                                                                }
                                                                              }
                                                                            }
                                                                          ],
                                                                          ""desc"": ""https://endereco-guia.com.br/""
                                                                        },
                                                                        {
                                                                          ""id"": [
                                                                            {
                                                                              ""tp"": {
                                                                                ""cdOrPrtry"": {
                                                                                  ""prtry"": ""INFODOC""
                                                                                }
                                                                              }
                                                                            }
                                                                          ],
                                                                          ""desc"": ""rhIQPTQjROpvpsMIQHtRJypllvTqTlByXeRmiJgOzONcHpgOzOzvfzkUtJsnrAYCPmYGyOEUEzaZdhFobuxSHMBfHifwBeODuACy""
                                                                        }
                                                                      ]
                                                                    }
                                                                  ],
                                                                  ""rfrdDocAmt"": {
                                                                    ""adjstmntAmtAndRsn"": [
                                                                      {
                                                                        ""addtlInf"": ""09""
                                                                      },
                                                                      {
                                                                        ""amt"": ""9507447.00""
                                                                      }
                                                                    ]
                                                                  },
                                                                  ""cdtrRefInf"": {
                                                                    ""ref"": ""N""
                                                                  },
                                                                  ""invcr"": {
                                                                    ""nm"": ""BA11111111RSOC"",
                                                                    ""id"": {
                                                                      ""orgId"": {
                                                                        ""othr"": [
                                                                          {
                                                                            ""id"": ""11111111""
                                                                          }
                                                                        ]
                                                                      }
                                                                    }
                                                                  },
                                                                  ""invcee"": {
                                                                    ""nm"": ""Cereja Prunus avium"",
                                                                    ""pstlAdr"": {
                                                                      ""strtNm"": ""Travessa do Rubro"",
                                                                      ""pstCd"": ""63470425"",
                                                                      ""twnNm"": ""Rio Grande do Norte"",
                                                                      ""ctrySubDvsn"": ""PB""
                                                                    }
                                                                  },
                                                                  ""addtlRmtInf"": [
                                                                    ""01""
                                                                  ]
                                                                }
                                                              ]
                                                            }
                                                          ]
                                                        }
                                                      ]
                                                    }
                                                  ]
                                                }",
                            Ativo = true,
                            //Respostas = new List<ProjetoItemResposta>()
                            //{
                            //    new ProjetoItemResposta()
                            //    {
                            //        CodigoHttp = 200,
                            //        TipoConteudo= "application/json",
                            //        Resposta = @"{
                            //                        ""ConsultaSaldo"": [ {""vlSaldo"": #random_currency# }]
                            //                    }"
                            //    },
                            //    new ProjetoItemResposta()
                            //    {
                            //        CodigoHttp = 400,
                            //        //TipoConteudo= "application/json",                                    
                            //        Condicao = "#json-nrInst#=0",
                            //        Resposta = @"-10 - Número de instituição não encontrada."
                            //    }
                            //}
                        }
                    }
                }
            );
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
