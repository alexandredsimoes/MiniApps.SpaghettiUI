using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using MiniApps.SpaghettiUI.Core.Contracts;
using MiniApps.SpaghettiUI.Core.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MiniApps.SpaghettiUI.Core
{
    public class ApplicationDbContext : DbContext, IApplicationDbContext
    {
        private IDbContextTransaction _currentTransaction;
        public DbSet<Projeto> Projetos { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {            
            optionsBuilder.UseSqlite(@"Data Source=SpaghettiUI.db");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Projeto>().ToTable("Projeto").HasKey(x => x.Id);
            modelBuilder.Entity<Projeto>().HasMany(x => x.Items).WithOne(x => x.Projeto);


            modelBuilder.Entity<ProjetoItem>().ToTable("ProjetoItem").HasKey(x => x.Id);
            modelBuilder.Entity<ProjetoItem>().HasMany(x => x.Respostas).WithOne(x => x.Item);

            modelBuilder.Entity<ProjetoItemResposta>().ToTable("ProjetoItemResposta").HasKey(x => x.Id);


        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            //foreach (var entry in ChangeTracker.Entries<AuditableEntity>())
            //{
            //    switch (entry.State)
            //    {
            //        case EntityState.Added:
            //            entry.Entity.CriadoPor = _currentUserService.UserId;
            //            entry.Entity.Criacao = _dateTime.Now;
            //            break;
            //        case EntityState.Modified:
            //            entry.Entity.AlteradoPor = _currentUserService.UserId;
            //            entry.Entity.Alteracao = _dateTime.Now;
            //            break;
            //    }
            //}

            return base.SaveChangesAsync(cancellationToken);
        }

        public async Task BeginTransactionAsync()
        {
            if (_currentTransaction != null)
            {
                return;
            }

            _currentTransaction = await base.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted).ConfigureAwait(false);
        }

        public async Task CommitTransactionAsync()
        {
            try
            {
                await SaveChangesAsync().ConfigureAwait(false);

                _currentTransaction?.Commit();
            }
            catch
            {
                RollbackTransaction();
                throw;
            }
            finally
            {
                if (_currentTransaction != null)
                {
                    _currentTransaction.Dispose();
                    _currentTransaction = null;
                }
            }
        }

        public void RollbackTransaction()
        {
            try
            {
                _currentTransaction?.Rollback();
            }
            finally
            {
                if (_currentTransaction != null)
                {
                    _currentTransaction.Dispose();
                    _currentTransaction = null;
                }
            }
        }
        //public ApplicationDbContext(
        //   DbContextOptionsBuilder<ApplicationDbContext> options)           
        //{
        //    options.UseSqlite("Filename:SpaghettiUIDb.db");
        //    options.
        //}
    }
}
