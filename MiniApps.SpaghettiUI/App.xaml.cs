using System;
using System.Collections.Generic;
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
            containerRegistry.RegisterForNavigation<SettingsPage, SettingsViewModel>(PageKeys.Settings);
            containerRegistry.RegisterForNavigation<MasterDetailPage, MasterDetailViewModel>(PageKeys.MasterDetail);
            containerRegistry.RegisterForNavigation<MainPage, MainViewModel>(PageKeys.Main);
            containerRegistry.RegisterForNavigation<ShellWindow, ShellViewModel>();

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
            var projetoId = Guid.NewGuid();            
            db.Projetos.Add(new Projeto()
            {
                Id = projetoId,
                Nome = "Servidor BACEN PIX",
                Items = new List<ProjetoItem>()
                {
                    new ProjetoItem()
                    {
                        ProjetoId = projetoId,
                        Descricao = "Admi002",
                        RespostaPadrao = "<xml/>",
                        CodigoHttpPadrao = 200,
                        Metodo = MetodoHttp.MhGet,                        
                        Endpoint = "/api/bla"
                    }
                }
            });
            projetoId = Guid.NewGuid();
            db.Projetos.Add(new Projeto()
            {
                Id = projetoId,
                Nome = "Gestão Conta PI",
                Items = new List<ProjetoItem>()
                {
                    new ProjetoItem()
                    {
                        ProjetoId = projetoId,
                        Descricao = "Aporte RB ou CL",
                        RespostaPadrao = @"{
	                                            ""idRequisicao"":""69F963C6-7487-4363-9406-A1DE2A9636D4"",
	                                            ""dtHrRequisicao"":""2020-01-23T22:10:05.025Z""
                                            }
                                            ",
                        CodigoHttpPadrao = 200,
                        Metodo = MetodoHttp.MhPost,
                        Endpoint = "/12345678/conta/api/v1/aporte/rbcl"
                    },
                    new ProjetoItem()
                    {
                        ProjetoId = projetoId,
                        Descricao = "Aporte CCME",
                        RespostaPadrao = @"{
	                                            ""idRequisicao"":""69F963C6-7487-4363-9406-A1DE2A9636D4"",
	                                            ""dtHrRequisicao"":""2020-01-23T22:10:05.025Z""
                                            }
                                            ",
                        CodigoHttpPadrao = 200,
                        Metodo = MetodoHttp.MhPost,
                        Endpoint = "/12345678/conta/api/v1/aporte/ccme"
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
