using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniApps.SpaghettiUI.Core.Models
{
    public class Projeto
    {
        public int PortaSegura { get; set; }

        public Guid Id { get; set; }
        
        [NotMapped]
        public char Icone { get; set; } = (char)57643;
        
        public string Nome { get; set; }
        public int PortaPadrao { get; set; }
        public ICollection<ProjetoItem> Items { get; set; }
        public bool ExibirLog { get; set; }
        public int PortaPadraoHttps { get; set; }

        public Projeto()
        {
            
        }
    }
}
