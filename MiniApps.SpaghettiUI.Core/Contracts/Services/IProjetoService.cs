﻿using MiniApps.SpaghettiUI.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniApps.SpaghettiUI.Core.Contracts.Services
{
    public interface IProjetoService
    {
        Task<IList<Projeto>> ListarProjetos();
        Task<Projeto> ObterProjeto(Guid id);
        Task<bool> RemoverProjeto(Guid id);
        Task<bool> SalvarProjeto(Projeto projeto);
    }
}
