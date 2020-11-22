using Microsoft.EntityFrameworkCore;
using MiniApps.SpaghettiUI.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MiniApps.SpaghettiUI.Core.Contracts
{
    public interface IApplicationDbContext
    {
        DbSet<Projeto> Projetos { get; set; }
        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
        void Detach(object item);
    }
}
