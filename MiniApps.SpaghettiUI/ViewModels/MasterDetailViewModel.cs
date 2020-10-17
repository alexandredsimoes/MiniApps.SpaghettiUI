using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MahApps.Metro.Controls;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using MiniApps.SpaghettiUI.Constants;
using MiniApps.SpaghettiUI.Core;
using MiniApps.SpaghettiUI.Core.Contracts.Services;
using MiniApps.SpaghettiUI.Core.Models;
using MiniApps.SpaghettiUI.Core.Services;
using MiniApps.SpaghettiUI.Models;
using MiniApps.SpaghettiUI.Services;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;

namespace MiniApps.SpaghettiUI.ViewModels
{
    public class MasterDetailViewModel : BindableBase, INavigationAware
    {
        private readonly AppState _appState;
        private readonly IProjetoService _projetoService;
        private readonly IRegionNavigationService _navigationService;
        private ProjetoDto _selected;

        private DelegateCommand _subirServidorCommand;
        private DelegateCommand _modificarProjetoCommand;
        private string _logs;

        public ProjetoDto Selected
        {
            get { return _selected; }
            set { SetProperty(ref _selected, value); }
        }
        public ObservableCollection<ProjetoDto> Projetos { get; private set; } = new ObservableCollection<ProjetoDto>();

        public MasterDetailViewModel(IProjetoService projetoService,
                                     AppState appState,
                                     IRegionManager regionManager)
        {
            _appState = appState;
            _projetoService = projetoService;
            _navigationService = regionManager.Regions[Regions.Main].NavigationService;
        }

        public async void OnNavigatedTo(NavigationContext navigationContext)
        {

            Projetos.Clear();

            var projetos = await _projetoService.ListarProjetos();

            foreach (var projeto in projetos.Select(x => ToDto(x)).ToList())
            {
                Projetos.Add(projeto);
            }

            Selected = Projetos.First();
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
                    Projeto = new ProjetoDto()
                    {
                      ExibirLog = x.Projeto.ExibirLog,
                      Icone = x.Projeto.Icone,
                      Id = x.Projeto.Id,
                      Nome = x.Projeto.Nome,
                      PortaPadrao = x.Projeto.PortaPadrao
                    },
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


        public string Logs
        {
            get { return _logs; }
            set { SetProperty(ref _logs, value); }
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
            if (_appState.Applications.Select(x => x.Item1).Any(x => x == Selected.Id))
            {
                return;
            }

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

            _appState.Applications.Add((Selected.Id, app));
            //await app.RunAsync(_cancellationToken);
        }

        private async Task ProcessarRequisicao(HttpContext context, ProjetoItemDto endpoint)
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
                    if (resposta != null)
                    {
                        context.Response.StatusCode = resposta.CodigoHttp;
                        await context.Response.WriteAsync(ProcessarResposta(context, resposta));
                    }
                    else
                    {
                        //Tentaremos no header
                        var header = context.Request.Headers.Select(x => $"#header-{x.Key}#={x.Value}").ToList();
                        var respostaHeader = endpoint.Respostas.Where(x => header.Contains(x.Condicao)).FirstOrDefault();

                        if (respostaHeader != null)
                        {
                            context.Response.StatusCode = resposta.CodigoHttp;
                            await context.Response.WriteAsync(ProcessarResposta(context, resposta));
                        }
                        else
                        {
                            var respostaSemCondicoes = endpoint.Respostas.Where(x => x.Condicao == null).OrderBy(x => Guid.NewGuid()).FirstOrDefault();
                            if (respostasComCondicoes == null)
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

            if (endpoint.Projeto.ExibirLog)
            {
                var sb = new StringBuilder();
                using var stream = new StreamReader(context.Request.Body);


                var metodo = endpoint.Metodo switch
                {
                    MetodoHttp.MhDelete => "DELETE",
                    MetodoHttp.MhGet => "GET",
                    MetodoHttp.MhPatch => "PATCH",
                    MetodoHttp.MhPost => "POST",
                    MetodoHttp.MhPut => "PUT",
                    _ => "MÉTODO DESCONHECIDO"
                };

                sb.AppendLine($"{metodo} {context.Request.GetEncodedUrl()}");
                sb.AppendLine("---------------------------------------------");


                var body = await stream.ReadToEndAsync();
                if (!string.IsNullOrWhiteSpace(body))
                {
                    sb.AppendLine("Body content");
                    sb.AppendLine("---------------------------------------------");
                    sb.AppendLine(await stream.ReadToEndAsync());
                    sb.AppendLine("---------------------------------------------");
                }

                var headers = context.Request.Headers;
                if (headers?.Count > 0)
                {
                    sb.AppendLine("Header content");
                    sb.AppendLine("---------------------------------------------");
                    foreach (var item in context.Request.Headers)
                    {
                        sb.AppendLine($"{item.Key} = {item.Value}");
                    }
                    sb.AppendLine("---------------------------------------------");
                }

                var queries = context.Request.Query;

                if (queries?.Count > 0)
                {
                    sb.AppendLine("Query content");
                    sb.AppendLine("---------------------------------------------");
                    foreach (var item in context.Request.Query)
                    {
                        sb.AppendLine($"{item.Key} = {item.Value}");
                    }
                    sb.AppendLine("---------------------------------------------");
                }

                Logs += sb.ToString();
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

        private string ProcessarResposta(HttpContext context, ProjetoItemRespostaDto resposta)
        {
            return ProcessarResposta(context, resposta.Resposta);
        }
    }
}
