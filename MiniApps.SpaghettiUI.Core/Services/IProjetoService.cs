using MiniApps.SpaghettiUI.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniApps.SpaghettiUI.Core.Services
{
    public interface IProjetoService
    {
        Task<IList<Projeto>> ListarProjetos();
    }
}
