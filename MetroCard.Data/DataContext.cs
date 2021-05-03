using MetroCard.Domain.Entities;
using MetroCard.Domain.Entities.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;

namespace MetroCard.Data
{
    public class DataContext : IdentityDbContext
    {
        public DbSet<Proprietario> Proprietarios { get; set; }
        public DbSet<ContaBancaria> ContasBancarias { get; set; }
        public DbSet<CartaoViagem> CartoesViagens { get; set; }
        public DbSet<Viagem> Viagens { get; set; }
        public DbSet<Tarifa> Tarifas { get; set; }

        public DataContext()
        {
            Database.EnsureCreated();
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var tarifas = new List<Tarifa>()
                {
                new Tarifa(){Id = Guid.NewGuid(), Zona = Zona.A, Jornada = Jornada.Unica, Valor = 6 },
                new Tarifa(){Id = Guid.NewGuid(), Zona = Zona.A, Jornada = Jornada.Dia, Valor = 10 },
                new Tarifa(){Id = Guid.NewGuid(), Zona = Zona.A, Jornada = Jornada.Semana, Valor = 30 },
                new Tarifa(){Id = Guid.NewGuid(), Zona = Zona.A, Jornada = Jornada.Mes, Valor = 130 },
                new Tarifa(){Id = Guid.NewGuid(), Zona =  Zona.B, Jornada = Jornada.Unica, Valor = 7},
                new Tarifa(){Id = Guid.NewGuid(), Zona = Zona.B, Jornada = Jornada.Dia, Valor = 12 },
                new Tarifa(){Id = Guid.NewGuid(), Zona =  Zona.B, Jornada = Jornada.Semana, Valor = 45},
                new Tarifa(){Id = Guid.NewGuid(),Zona =  Zona.B, Jornada = Jornada.Mes, Valor = 170 }
                };

            modelBuilder.Entity<Tarifa>().HasData(tarifas);

            var user = new IdentityUser()
            {
                UserName = "Teste",
                Email = "teste@teste.com",
            };
            var passwordHash = new PasswordHasher<IdentityUser>().HashPassword(user, "Teste@123");
            user.PasswordHash = passwordHash;

            modelBuilder.Entity<IdentityUser>().HasData(user);

            base.OnModelCreating(modelBuilder);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) => optionsBuilder.UseInMemoryDatabase("InMemoryDatabase");
    }
}