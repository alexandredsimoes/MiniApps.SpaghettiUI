using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniApps.SpaghettiUI.Core.Models
{
    public class ProjetoItemResposta
    {
        public Guid Id { get; set; }
        public Guid ProjetoItemId { get; set; }
        public string Descricao { get; set; }
        public int CodigoHttp { get; set; }
        
        public string Resposta { get; set; }
        public ProjetoItem Item { get; set; }
        public string Condicao { get; set; }
    }
}
