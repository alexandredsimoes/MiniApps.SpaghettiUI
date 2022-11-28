using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Bogus;
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
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;

namespace MiniApps.SpaghettiUI.ViewModels
{
    public class MasterDetailViewModel : BindableBase, INavigationAware
    {
        private readonly Regex _regexParameters = new Regex("(?<=(#))(\\w|\\d|\\n|[().,\\-:;@#$%^&*\\[\\]\"'+–/\\/®°⁰!?{}|`~]| )+?(?=(#))", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private readonly AppState _appState;
        private readonly IProjetoService _projetoService;
        private readonly IRegionNavigationService _navigationService;
        private ProjetoDto _selected;
        private string _logs;


        private DelegateCommand _subirServidorCommand;
        private DelegateCommand _modificarProjetoCommand;
        private DelegateCommand _pararServidorCommand;



        public bool IsActive
        {
            get
            {
                return _appState.Applications.Any(x => x.Item1 == _selected?.Id);
            }
        }

        public ProjetoDto Selected
        {
            get { return _selected; }
            set { SetProperty(ref _selected, value); RaisePropertyChanged(nameof(IsActive)); }
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

            Selected = Projetos.FirstOrDefault();
        }

        private ProjetoDto ToDto(Projeto x)
        {
            return new ProjetoDto()
            {
                Icone = x.Icone,
                Id = x.Id,
                Nome = x.Nome,
                PortaPadrao = x.PortaPadrao,
                PortaPadraoHttps = x.PortaPadraoHttps,
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
                    RespostaHeader = x.RespostaHeader,
                    TipoConteudo = x.TipoConteudo,
                    Ativo = x.Ativo,
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
                        TipoConteudo = x.TipoConteudo,
                        Descricao = x.Descricao,
                    }))
                }))
            };
        }



        public async void OnNavigatedFrom(NavigationContext navigationContext)
        {

        }


        public string Logs
        {
            get { return _logs; }
            set { SetProperty(ref _logs, value); }
        }

        public bool IsNavigationTarget(NavigationContext navigationContext) => true;


        public DelegateCommand PararServidorCommand => _pararServidorCommand ?? (_pararServidorCommand = new DelegateCommand(ExecutePararServidorCommand));

        public DelegateCommand SubirServidorCommand => _subirServidorCommand ?? (_subirServidorCommand = new DelegateCommand(ExecuteSubirServidorCommand));

        public DelegateCommand ModificarProjetoCommand =>
            _modificarProjetoCommand ?? (_modificarProjetoCommand = new DelegateCommand(ExecuteModificarProjetoCommand));


        private async void ExecutePararServidorCommand()
        {
            if (_selected == null || _appState.Applications.Count == 0) return;

            var webApp = _appState.Applications.FirstOrDefault(x => x.Item1 == Selected.Id);
            await webApp.Item2.StopAsync();

            _appState.Applications.Remove(webApp);
            RaisePropertyChanged(nameof(IsActive));
        }

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
            var builder = Microsoft.AspNetCore.Builder.WebApplication.CreateBuilder();
            //var app = Microsoft.AspNetCore.Builder.WebApplication.Create();
            var app = builder.Build();


            app.Listen($"http://localhost:{Selected.PortaPadrao}", $"https://localhost:{Selected.PortaPadraoHttps}");


            foreach (var endpoint in Selected.Items.Where(x => x.Ativo))
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
                        //using (var reader = new StreamReader(context.Request.Body))
                        //{
                        //    var body = await reader.ReadToEndAsync();

                        //    var json = JsonConvert.DeserializeObject(body);
                        //}

                        await ProcessarRequisicao(context, endpoint);
                    });
                }

            }

            _appState.Applications.Add((Selected.Id, app));
            RaisePropertyChanged(nameof(IsActive));
            //await app.RunAsync(_cancellationToken);
        }

        private async Task ProcessarRequisicao(HttpContext context, ProjetoItemDto endpoint)
        {
            if (endpoint.Respostas?.Count > 0)
            {

                var respostasComCondicoes = endpoint.Respostas.Where(x => x.Condicao != null);
                if (respostasComCondicoes.Count() == 0)
                {
                    ProcessarHeaderResposta(context, endpoint);
                    var resposta = endpoint.Respostas.OrderBy(x => Guid.NewGuid()).FirstOrDefault();
                    context.Response.StatusCode = resposta.CodigoHttp;
                    context.Response.ContentType = endpoint.TipoConteudo;
                    await context.Response.WriteAsync(await ProcessarResposta(context, resposta));
                }
                else
                {
                    ProcessarHeaderResposta(context, endpoint);
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
                        await context.Response.WriteAsync(await ProcessarResposta(context, resposta));
                    }
                    else
                    {
                        //Tentaremos no header
                        var header = context.Request.Headers.Select(x => $"#header-{x.Key}#={x.Value}").ToList();
                        var respostaHeader = endpoint.Respostas.Where(x => header.Contains(x.Condicao)).FirstOrDefault();

                        if (respostaHeader != null)
                        {
                            context.Response.StatusCode = resposta.CodigoHttp;
                            await context.Response.WriteAsync(await ProcessarResposta(context, resposta));
                        }
                        else
                        {
                            var jsons = endpoint.Respostas
                                .Where(x => x.Condicao?.Contains("#json-") ?? false)
                                .ToList();


                            //Tentaremos no body
                            using var reader = new StreamReader(context.Request.Body);
                            var body = await reader.ReadToEndAsync();

                            if (!string.IsNullOrWhiteSpace(body))
                            {
                                JObject json = (JObject)JsonConvert.DeserializeObject(body);
                                if (json != null)
                                {

                                    foreach (var item in jsons)
                                    {
                                        var match = _regexParameters.Match(item.Condicao);
                                        var token = json.SelectToken(match.Value.Substring(match.Value.IndexOf('-') + 1));
                                        if (token != null && token.Value<string>() == item.Condicao.Substring(item.Condicao.IndexOf('=') + 1))

                                        {
                                            context.Response.StatusCode = item.CodigoHttp;
                                            await context.Response.WriteAsync(await ProcessarResposta(context, item));
                                            return;
                                        }
                                    }
                                }
                            }

                            var respostaSemCondicoes = endpoint.Respostas.Where(x => x.Condicao == null).OrderBy(x => Guid.NewGuid()).FirstOrDefault();
                            if (respostaSemCondicoes != null)
                            {
                                context.Response.StatusCode = respostaSemCondicoes.CodigoHttp;
                                context.Response.ContentType = respostaSemCondicoes.TipoConteudo;
                                await context.Response.WriteAsync(await ProcessarResposta(context, respostaSemCondicoes));
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
                ProcessarHeaderResposta(context, endpoint);
                context.Response.ContentType = endpoint.TipoConteudo;
                context.Response.StatusCode = endpoint.CodigoHttpPadrao;
                await context.Response.WriteAsync(await ProcessarResposta(context, endpoint.RespostaPadrao));

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

            static void ProcessarHeaderResposta(HttpContext context, ProjetoItemDto endpoint)
            {
                if (string.IsNullOrWhiteSpace(endpoint.RespostaHeader)) return;
                var headers = endpoint.RespostaHeader.Split("&", StringSplitOptions.RemoveEmptyEntries);
                if (headers.Length > 0)
                {
                    foreach (var item in headers)
                    {

                        var value = item.Split('=', StringSplitOptions.RemoveEmptyEntries);
                        context.Response.Headers[value[0]] = value[1];
                    }
                }
            }
        }

        private async ValueTask<string> ProcessarResposta(HttpContext context, string respostaPadrao)
        {
            var faker = new Faker();
            var result = new StringBuilder();

            //Le o corpo da requisição para processamento futuro
            using var reader = new StreamReader(context.Request.Body);
            var body = await reader.ReadToEndAsync();

            result.Append(respostaPadrao);

            var matches = _regexParameters.Matches(respostaPadrao);
            foreach (Match item in matches)
            {

                if (item.Value.Contains("query-"))
                {
                    var h = context.Request.Query.FirstOrDefault(x => x.Key == item.Value.Substring(item.Value.IndexOf("-") + 1));
                    if (h.Key != null)
                        result.Replace($"#{item.Value}#", h.Value);
                }

                if (item.Value.Contains("header-"))
                {
                    var h = context.Request.Headers.FirstOrDefault(x => x.Key == item.Value.Substring(item.Value.IndexOf("-") + 1));
                    if (h.Key != null)
                        result.Replace($"#{item.Value}#", h.Value);
                }

                if (item.Value.Contains("json-"))
                {
                    if (!string.IsNullOrWhiteSpace(body))
                    {
                        JObject json = (JObject)JsonConvert.DeserializeObject(body);
                        if (json != null)
                        {
                            var token = json.SelectToken(item.Value.Substring(item.Value.IndexOf('-') + 1));
                            if (token != null)
                                result.Replace($"#{item.Value}#", token.Value<string>());
                        }
                    }
                }

                if (item.Value.Contains("datenow"))
                {
                    result.Replace("#datenow#", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                }
                if (item.Value.Contains("datenowutc"))
                {
                    result.Replace("#datenowutc#", DateTime.UtcNow.ToString());
                }
                if (item.Value.Contains("guid"))
                {
                    result.Replace("#guid#", Guid.NewGuid().ToString());
                }


                if (item.Value.Contains("random_currency"))
                {
                    result.Replace("#random_currency#", faker.Finance.Amount(1_000, 10_000).ToString(CultureInfo.CreateSpecificCulture("en-US")));
                }

                result.Replace(item.Value, item.ToString());
            }
            //var result = respostaPadrao;
            //foreach (var item in context.Request.Query)
            //{
            //    result = result.Replace($"#query-{item.Key}#", item.Value);
            //}

            //Hack: Usar Regex para processar multiplas entradas de macros
            //Processa outras macros
            //result = result.Replace("#datenow#", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            //result = result.Replace("#jd-id#", "JDPI20out30110751841");

            //result = result.Replace("#guid#", Guid.NewGuid().ToString());
            //if (result.Contains("#datenowutc:#"))
            //{
            //    result = result.Replace("#datenowutc#", DateTime.UtcNow.ToString());
            //}
            //else if (result.Contains("#datenowutc#"))
            //{
            //    result = result.Replace("#datenowutc#", DateTime.UtcNow.ToString());
            //}

            //JDPI20out30110751841

            return result.ToString();
        }

        private async ValueTask<string> ProcessarResposta(HttpContext context, ProjetoItemRespostaDto resposta)
        {
            return await ProcessarResposta(context, resposta.Resposta);
        }
    }
}
