using MetroCard.Domain.Entities.Enums;
using System;

namespace MetroCard.Domain.Entities
{
    public class Viagem
    {
        public Guid Id { get; set; }
        public DateTime DataEntrada { get; set; }
        public DateTime? DataSaida { get; set; }
        public Zona Zona { get; set; }
    }
}
