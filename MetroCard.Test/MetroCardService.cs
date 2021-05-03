using MetroCard.Data;
using MetroCard.Domain.Entities;
using MetroCard.Domain.Entities.Enums;
using System;
using System.Linq;

namespace MetroCard.Test
{
    public class MetroCardService
    {
        private readonly GenericRepository<ContaBancaria> _contaBancariaRepository;
        private readonly GenericRepository<CartaoViagem> _cartaoViagemRepository;
        private readonly GenericRepository<Viagem> _viagemRepository;

        public MetroCardService(DataContext db)
        {
            _contaBancariaRepository = new GenericRepository<ContaBancaria>(db);
            _cartaoViagemRepository = new GenericRepository<CartaoViagem>(db);
            _viagemRepository = new GenericRepository<Viagem>(db);

        }

        public void EntrarNaEstacao(CartaoViagem cartaoViagem, DateTime data, Zona zona)
        {
            if (cartaoViagem.Tarifa.Zona == Zona.A && zona == Zona.B)
            {
                throw new Exception("zona_cartao_invalida");
            }
            else if (cartaoViagem.Status == StatusCartaoViagem.Consumido)
            {
                throw new Exception("cartao_invalido");
            }
            else
            {
                cartaoViagem.DataVigente = (cartaoViagem.Status == StatusCartaoViagem.Pendente) ? data : cartaoViagem.DataVigente;

                var viagem = new Viagem() { DataEntrada = data, Zona = zona };
                _viagemRepository.Create(viagem);
                _viagemRepository.Commit();

                cartaoViagem.Viagens.Add(viagem);

                if (cartaoViagem.Status == StatusCartaoViagem.Pendente)
                {
                    var contaBancaria = _contaBancariaRepository.Read().Where(c => c.Proprietario == cartaoViagem.Proprietario).FirstOrDefault();
                    CobrarDaContaBancaria(contaBancaria, cartaoViagem.Tarifa.Valor);
                    cartaoViagem.Status = StatusCartaoViagem.Pago;
                }

                _cartaoViagemRepository.Update(cartaoViagem);
                _cartaoViagemRepository.Commit();
            }
        }

        public void SairDaEstacao(CartaoViagem cartaoViagem, DateTime dataSaida)
        {
            var dataVigente = cartaoViagem.DataVigente;
            var viagem = cartaoViagem.Viagens.Where(v => v.DataEntrada >= dataVigente && !v.DataSaida.HasValue).FirstOrDefault();
            viagem.DataSaida = dataSaida;
            _viagemRepository.Update(viagem);
            _viagemRepository.Commit();

            cartaoViagem.AtualizarStatus();
            _cartaoViagemRepository.Update(cartaoViagem);
            _cartaoViagemRepository.Commit();
        }

        private void CobrarDaContaBancaria(ContaBancaria contaBancaria, decimal valor)
        {
            contaBancaria.AtualizarSaldoDevedor(valor);
            _contaBancariaRepository.Update(contaBancaria);
            _contaBancariaRepository.Commit();
        }
    }
}
