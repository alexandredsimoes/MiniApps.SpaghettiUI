using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniApps.SpaghettiUI.Core.Models
{
    public class ProjetoItem
    {
        public Guid Id { get; set; }
        public Guid ProjetoId { get; set; }
        public string Endpoint { get; set; }
        public MetodoHttp Metodo { get; set; }
        public string RespostaPadrao { get; set; }
        public int CodigoHttpPadrao { get; set; }
        public Projeto Projeto { get;  set; }
        public string Descricao { get; set; }

        public ICollection<ProjetoItemResposta> Respostas { get; set; }
        public string RespostaHeader { get; set; }
        public string TipoConteudo { get; set; }
    }
}
