using Microsoft.EntityFrameworkCore;
using MiniApps.SpaghettiUI.Core.Models;
using MiniApps.SpaghettiUI.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniApps.SpaghettiUI.Core.Contracts.Services
{
    public class ProjetoService : IProjetoService
    {
        private readonly IApplicationDbContext _context;

        public ProjetoService(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IList<Projeto>> ListarProjetos()
        {
            return await _context.Projetos.Include(x=>x.Items).ThenInclude(x=>x.Respostas).ToListAsync();
        }

        public async Task<Projeto> ObterProjeto(Guid id)
        {
            var entity = await _context.Projetos.FirstOrDefaultAsync(x => x.Id == id);
            return entity;
        }
    }
}
