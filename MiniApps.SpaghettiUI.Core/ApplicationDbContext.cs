using Microsoft.EntityFrameworkCore;
using MiniApps.SpaghettiUI.Core.Contracts;
using MiniApps.SpaghettiUI.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniApps.SpaghettiUI.Core
{
    public class ApplicationDbContext : DbContext, IApplicationDbContext
    {
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

        //public ApplicationDbContext(
        //   DbContextOptionsBuilder<ApplicationDbContext> options)           
        //{
        //    options.UseSqlite("Filename:SpaghettiUIDb.db");
        //    options.
        //}
    }
}
