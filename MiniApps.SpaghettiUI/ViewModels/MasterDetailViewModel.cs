using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using MiniApps.SpaghettiUI.Constants;
using MiniApps.SpaghettiUI.Core.Contracts.Services;
using MiniApps.SpaghettiUI.Core.Models;
using MiniApps.SpaghettiUI.Core.Services;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;

namespace MiniApps.SpaghettiUI.ViewModels
{
    public class MasterDetailViewModel : BindableBase, INavigationAware
    {
        private readonly IProjetoService _projetoService;
        private readonly IRegionNavigationService _navigationService;
        private Projeto _selected;

        private DelegateCommand _subirServidorCommand;
        private DelegateCommand _modificarProjetoCommand;

        public Projeto Selected
        {
            get { return _selected; }
            set { SetProperty(ref _selected, value); }
        }
        public ObservableCollection<Projeto> Projetos { get; private set; } = new ObservableCollection<Projeto>();

        public MasterDetailViewModel(IProjetoService projetoService, IRegionManager regionManager)
        {
            _projetoService = projetoService;
            _navigationService = regionManager.Regions[Regions.Main].NavigationService;
        }

        public async void OnNavigatedTo(NavigationContext navigationContext)
        {

            Projetos.Clear();

            var projetos = await _projetoService.ListarProjetos();

            foreach (var projeto in projetos)
            {
                Projetos.Add(projeto);
            }

            Selected = Projetos.First();
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
        }

        public bool IsNavigationTarget(NavigationContext navigationContext) => true;


        public DelegateCommand SubirServidorCommand => _subirServidorCommand ?? (_subirServidorCommand = new DelegateCommand(ExecuteSubirServidorCommand));

        public DelegateCommand ModificarProjetoCommand =>
            _modificarProjetoCommand ?? (_modificarProjetoCommand = new DelegateCommand(ExecuteModificarProjetoCommand));

        void ExecuteModificarProjetoCommand()
        {
            var np = new NavigationParameters();
            np.Add("Id", Selected.Id);
            _navigationService.RequestNavigate(PageKeys.Projeto, np);
        }

        async void ExecuteSubirServidorCommand()
        {
            //
            var app = WebApplication.Create();
            app.Listen($"https://localhost:{Selected.PortaPadrao}");

            foreach (var endpoint in Selected.Items)
            {
                if (endpoint.Metodo == Core.MetodoHttp.MhGet)
                {
                    app.MapGet(endpoint.Endpoint, async (context) =>
                    {
                        await ProcessarRequisicao(context, endpoint);

                    });
                }

                if (endpoint.Metodo == Core.MetodoHttp.MhPost)
                {
                    app.MapPost(endpoint.Endpoint, async (context) =>
                    {
                        await ProcessarRequisicao(context, endpoint);
                    });
                }

            }
            await app.RunAsync();
        }

        private async Task ProcessarRequisicao(HttpContext context, ProjetoItem endpoint)
        {
            if (endpoint.Respostas?.Count > 0)
            {
                
                var respostasComCondicoes = endpoint.Respostas.Where(x => x.Condicao != null);
                if (respostasComCondicoes.Count() == 0)
                {
                    var resposta = endpoint.Respostas.OrderBy(x => Guid.NewGuid()).FirstOrDefault();
                    context.Response.StatusCode = resposta.CodigoHttp;
                    await context.Response.WriteAsync(ProcessarResposta(context, resposta));
                }
                else
                {
                    //tpRequisicao=6
                    //idRequisicao=xxxxxxx
                    var query = context.Request.Query.Select(x => $"#query-{x.Key}#={x.Value}").ToList();

                    //Condicoes
                    //#query-tpRequisicao#=6
                    var resposta = endpoint.Respostas.Where(x => query.Contains(x.Condicao)).FirstOrDefault();
                    
                    //Achamos na query
                    if(resposta != null)
                    {
                        context.Response.StatusCode = resposta.CodigoHttp;
                        await context.Response.WriteAsync(ProcessarResposta(context, resposta));
                    }
                    else
                    {
                        //Tentaremos no header
                        var header = context.Request.Headers.Select(x => $"#header-{x.Key}#={x.Value}").ToList();
                        var respostaHeader = endpoint.Respostas.Where(x => header.Contains(x.Condicao)).FirstOrDefault();

                        if(respostaHeader != null)
                        {
                            context.Response.StatusCode = resposta.CodigoHttp;
                            await context.Response.WriteAsync(ProcessarResposta(context, resposta));
                        }
                        else
                        {
                            var respostaSemCondicoes = endpoint.Respostas.Where(x => x.Condicao == null).OrderBy(x => Guid.NewGuid()).FirstOrDefault();
                            if(respostasComCondicoes == null)
                            {
                                context.Response.StatusCode = resposta.CodigoHttp;
                                await context.Response.WriteAsync(ProcessarResposta(context, respostaSemCondicoes));
                            }
                            else
                            {
                                context.Response.StatusCode = 500;
                                await context.Response.WriteAsync("Falhamos miseravelmente em processar sua requisição.");
                            }
                            
                        }
                    }
                }


            }
            else
            {
                context.Response.StatusCode = endpoint.CodigoHttpPadrao;
                await context.Response.WriteAsync(ProcessarResposta(context, endpoint.RespostaPadrao));
            }
        }

        private string ProcessarResposta(HttpContext context, string respostaPadrao)
        {
            var result = respostaPadrao;
            foreach (var item in context.Request.Query)
            {
                result = result.Replace($"#query-{item.Key}#", item.Value);
            }

            //Hack: Usar Regex para processar multiplas entradas de macros
            //Processa outras macros
            result = result.Replace("#datenow#", DateTime.Now.ToString());
            result = result.Replace("#guid#", Guid.NewGuid().ToString());
            if (result.Contains("#datenowutc:#"))
            {
                result = result.Replace("#datenowutc#", DateTime.UtcNow.ToString());
            }
            else if (result.Contains("#datenowutc#"))
            {
                result = result.Replace("#datenowutc#", DateTime.UtcNow.ToString());
            }

            return result;
        }

        private string ProcessarResposta(HttpContext context, ProjetoItemResposta resposta)
        {
            return ProcessarResposta(context, resposta.Resposta);
        }
    }
}
