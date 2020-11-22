using Microsoft.EntityFrameworkCore;
using MiniApps.SpaghettiUI.Core.Contracts;
using MiniApps.SpaghettiUI.Core.Contracts.Services;
using MiniApps.SpaghettiUI.Core.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MiniApps.SpaghettiUI.Core.Services
{
    public class ProjetoService : IProjetoService
    {
        //private readonly IApplicationDbContext _context;

        //public ProjetoService(IApplicationDbContext context)
        //{
        //    _context = context;
        //}

        public async Task<IList<Projeto>> ListarProjetos()
        {
            using var context = new ApplicationDbContext();
            return await context.Projetos.AsNoTracking().Include(x=>x.Items).ThenInclude(x=>x.Respostas).ToListAsync();
        }

        public async Task<Projeto> ObterProjeto(Guid id)
        {
            using var context = new ApplicationDbContext();
            var entity = await context.Projetos.AsNoTracking()
                .Include(x=>x.Items).ThenInclude(x=>x.Respostas)
                .FirstOrDefaultAsync(x => x.Id == id);
            return entity;
        }

        public async Task<bool> RemoverProjeto(Guid id)
        {
            using var context = new ApplicationDbContext();
            var entity = await ObterProjeto(id);
            context.Projetos.Remove(entity);
            return await context.SaveChangesAsync(CancellationToken.None) > 0;
        }

        public async Task<bool> SalvarProjeto(Projeto projeto)
        {
            using var context = new ApplicationDbContext();
            if (projeto.Id == Guid.Empty)
                context.Projetos.Add(projeto);
            else
            {
                context.Projetos.Update(projeto);
                
            }

            
            var result =  await context.SaveChangesAsync(CancellationToken.None) > 0;
            context.Detach(projeto);
            return result;
        }
    }
}
