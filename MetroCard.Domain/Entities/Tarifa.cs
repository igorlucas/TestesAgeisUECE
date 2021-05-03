using MetroCard.Domain.Entities.Enums;
using System;

namespace MetroCard.Domain.Entities
{
    public class Tarifa
    {
        public Guid Id { get; set; }
        public Zona Zona { get; set; }
        public Jornada Jornada { get; set; }
        public decimal Valor { get; set; }
    }
}