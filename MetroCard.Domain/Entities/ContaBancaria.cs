using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MetroCard.Domain.Entities
{
    public class ContaBancaria
    {
        [Key]
        public Guid Id { get; set; }
        public string Numero { get; set; }
        public decimal SaldoDevedor { get; set; }

        [ForeignKey("ProprietarioId")]
        public virtual Proprietario Proprietario { get; set; }

        public void AtualizarSaldoDevedor(decimal valor) => SaldoDevedor += valor;

        public ContaBancaria(string numero, Proprietario proprietario)
        {
            Id = Guid.NewGuid();
            Numero = numero;
            Proprietario = proprietario;
        }

        public ContaBancaria()
        {

        }
    }
}