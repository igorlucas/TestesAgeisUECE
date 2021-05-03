using MetroCard.Domain.Entities.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MetroCard.Domain.Entities
{
    public class CartaoViagem
    {
        [Key]
        public Guid Id { get; set; }

        [ForeignKey("ProprietarioId")]
        public virtual Proprietario Proprietario { get; set; }

        [ForeignKey("TarifaId")]
        public Tarifa Tarifa { get; set; }

        public DateTime? DataVigente { get; set; }

        public StatusCartaoViagem Status { get; set; }

        public List<Viagem> Viagens { get; set; } = new List<Viagem>();

        public void AtualizarStatus()
        {
            switch (Tarifa.Jornada)
            {
                case Jornada.Unica:
                    {
                        Status = Viagens.Count == 1 ? StatusCartaoViagem.Consumido : Status;
                        break;
                    };
                case Jornada.Dia:
                    {
                        Status = DateTime.Now > DataVigente.Value.AddDays(1) ? StatusCartaoViagem.Consumido : Status;
                        break;
                    };
                case Jornada.Semana:
                    {
                        var dataLimite = DataVigente.Value.AddDays(7);
                        Status = DateTime.Now.Day > dataLimite.Day ? StatusCartaoViagem.Consumido : Status;
                        break;
                    };
                case Jornada.Mes:
                    {
                        Status = DataVigente.Value.Month < DateTime.Now.Month ? StatusCartaoViagem.Consumido : Status;
                        break;
                    };
                default:
                    {
                        throw new Exception($"Cartão invalido!: Id={Id}");
                    };
            }
        }

        public CartaoViagem(Proprietario proprietario, Tarifa tarifa)
        {
            Id = Guid.NewGuid();
            Status = StatusCartaoViagem.Pendente;
            Proprietario = proprietario;
            Tarifa = tarifa;
        }

        public CartaoViagem()
        {

        }
    }
}
