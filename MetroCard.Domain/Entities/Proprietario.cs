using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MetroCard.Domain.Entities
{
    public class Proprietario
    {
        [Key]
        public Guid Id { get; set; }

        public string Nome { get; set; }

        //[ForeignKey("ContaBancariaId")]
        //public virtual ContaBancaria ContaBancaria { get; set; }

        //[ForeignKey("CartaoViagemId")]
        //public virtual CartaoViagem CartaoViagem { get; set; }

        public Proprietario(string nome)
        {
            Id = Guid.NewGuid();
            Nome = nome;
        }
    }
}
