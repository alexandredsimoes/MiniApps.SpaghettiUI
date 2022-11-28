using MiniApps.SpaghettiUI.Core;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniApps.SpaghettiUI.Models
{
    public class ProjetoDto : BindableBase
    {
        private Guid id;
        private char icone = (char)57643;
        private string nome;
        private int portaPadrao;
        private int portaPadraoHttps;
        private ObservableCollection<ProjetoItemDto> items;
       

        public Guid Id { get => id; set => SetProperty(ref id, value); }

        public char Icone { get => icone; set => SetProperty(ref icone, value); }
        public string Nome { get => nome; set => SetProperty(ref nome, value); }
        public int PortaPadrao { get => portaPadrao; set => SetProperty(ref portaPadrao, value); }
        public int PortaPadraoHttps { get => portaPadraoHttps; set => SetProperty(ref portaPadraoHttps, value); }
        public ObservableCollection<ProjetoItemDto> Items { get => items; set => SetProperty(ref items, value); }
        public bool ExibirLog { get; internal set; }
    }

    public class ProjetoItemDto : BindableBase
    {
        private Guid id;
        private Guid projetoId;
        private string endpoint;
        private MetodoHttp metodo;
        private string respostaPadrao;
        private int codigoHttpPadrao;
        private ProjetoDto projeto;
        private string descricao;
        private ObservableCollection<ProjetoItemRespostaDto> respostas;

        public Guid Id { get => id; set => SetProperty(ref id, value); }
        public Guid ProjetoId { get => projetoId; set => SetProperty(ref projetoId, value); }
        public string Endpoint { get => endpoint; set => SetProperty(ref endpoint, value); }
        public MetodoHttp Metodo { get => metodo; set => SetProperty(ref metodo, value); }
        public string RespostaPadrao { get => respostaPadrao; set => SetProperty(ref respostaPadrao, value); }
        public int CodigoHttpPadrao { get => codigoHttpPadrao; set => SetProperty(ref codigoHttpPadrao, value); }
        public ProjetoDto Projeto { get => projeto; set => SetProperty(ref projeto, value); }
        public string Descricao { get => descricao; set => SetProperty(ref descricao, value); }

        public ObservableCollection<ProjetoItemRespostaDto> Respostas { get => respostas; set => SetProperty(ref respostas, value); }
        public string RespostaHeader { get; internal set; }
        public string TipoConteudo { get; internal set; }
        public bool Ativo { get; set; }
    }

    public class ProjetoItemRespostaDto
    {
        public int CodigoHttp { get; set; }
        public string Condicao { get; set; }
        public string Resposta { get; set; }
        public string Descricao { get; set; }
        public string TipoConteudo { get; set; }
    }
}
