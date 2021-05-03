using MetroCard.Data;
using MetroCard.Domain.Entities;
using MetroCard.Domain.Entities.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace MetroCard.Test
{
    public class TestesTDD
    {
        private readonly GenericRepository<Proprietario> _proprietarioRepository;
        private readonly GenericRepository<CartaoViagem> _cartaoViagemRepository;
        private readonly GenericRepository<ContaBancaria> _contaBancariaRepository;
        private readonly GenericRepository<Tarifa> _tarifaRepository;
        private readonly MetroCardService _metroCardService;

        public TestesTDD()
        {
            //var optionsBuilder = new DbContextOptionsBuilder<DataContext>();
            //optionsBuilder.UseInMemoryDatabase("InMemoryDatabase");
            var db = new DataContext();
            _proprietarioRepository = new GenericRepository<Proprietario>(db);
            _cartaoViagemRepository = new GenericRepository<CartaoViagem>(db);
            _contaBancariaRepository = new GenericRepository<ContaBancaria>(db);
            _tarifaRepository = new GenericRepository<Tarifa>(db);
            _metroCardService = new MetroCardService(db);
        }

        [Fact]
        public void DeveCobrarDiretamenteDaContaBancariaQuandoUsarOCartao()
        {
            CartaoViagem cartaoViagem = _cartaoViagemRepository.Read().Where(c => c.Tarifa != null && c.Tarifa.Jornada == Jornada.Unica && c.Tarifa.Zona == Zona.A).FirstOrDefault();
            ContaBancaria contaBancaria;

            if (cartaoViagem != null)
                contaBancaria = _contaBancariaRepository.Read().Where(c => c.Proprietario == cartaoViagem.Proprietario).FirstOrDefault();
            else
            {
                AdcionarProprietarioComUmCartaoTaxaUnicaZonaA(out cartaoViagem, out contaBancaria);
            }

            _metroCardService.EntrarNaEstacao(cartaoViagem, DateTime.Now, Zona.A);

            var resultado = cartaoViagem.Status == StatusCartaoViagem.Pago && contaBancaria.SaldoDevedor == cartaoViagem.Tarifa.Valor;

            Assert.True(resultado);
        }

        [Fact]
        public void ParaRealizarUmaViagemDeveEntrarESairDeUmaEstacao()
        {

            CartaoViagem cartaoViagem;
            ContaBancaria contaBancaria;

            AdcionarProprietarioComUmCartaoTaxaUnicaZonaA(out cartaoViagem, out contaBancaria);

            _metroCardService.EntrarNaEstacao(cartaoViagem, DateTime.Now, Zona.A);
            _metroCardService.SairDaEstacao(cartaoViagem, DateTime.Now);

            var viagens = cartaoViagem.Viagens.Where(v => v.DataEntrada >= cartaoViagem.DataVigente && v.DataSaida.HasValue).ToList();

            var resultado = viagens.Count == 1;

            Assert.True(resultado);
        }

        [Theory]
        [InlineData(Jornada.Unica)]
        [InlineData(Jornada.Dia)]
        [InlineData(Jornada.Mes)]
        [InlineData(Jornada.Semana)]
        public void ViajarNaZonaBDeveSerMaisCaroDoQueViajarNaZonaA(Jornada jornada)
        {
            var tarifas = _tarifaRepository.Read().Where(t => t.Jornada == jornada);
            var tarifaZonaB = tarifas.Where(t => t.Zona == Zona.B).First();
            var tarifaZonaA = tarifas.Where(t => t.Zona == Zona.A).First();

            var resultado = tarifaZonaB.Valor > tarifaZonaA.Valor;

            Assert.True(resultado);
        }

        [Fact]
        public void OPrecoDaZonaBDeveIncluirViagensDentroDaZonaA()
        {
            CartaoViagem cartaoViagem;
            ContaBancaria contaBancaria;

            AdcionarProprietarioComUmCartaoTaxaDiaZonaB(out cartaoViagem, out contaBancaria);

            _metroCardService.EntrarNaEstacao(cartaoViagem, DateTime.Now, Zona.B);
            _metroCardService.SairDaEstacao(cartaoViagem, DateTime.Now);

            Task.Delay(3000);

            _metroCardService.EntrarNaEstacao(cartaoViagem, DateTime.Now, Zona.A);
            _metroCardService.SairDaEstacao(cartaoViagem, DateTime.Now);

            var resultado = cartaoViagem.Viagens.Any(v => v.Zona == Zona.A || v.Zona == Zona.B);

            Assert.True(resultado);
        }

        [Theory]
        [InlineData(Jornada.Unica, Zona.A)]
        [InlineData(Jornada.Unica, Zona.B)]
        [InlineData(Jornada.Dia, Zona.A)]
        [InlineData(Jornada.Dia, Zona.B)]
        [InlineData(Jornada.Semana, Zona.A)]
        [InlineData(Jornada.Semana, Zona.B)]
        [InlineData(Jornada.Mes, Zona.A)]
        [InlineData(Jornada.Mes, Zona.B)]
        public void ATarifaDaJornadaIncluiAsViagensFeitasNoPeriodoDaJornada(Jornada jornada, Zona zona)
        {
            Tarifa tarifa = _tarifaRepository.Read().Where(t => t.Jornada == jornada && t.Zona == zona).FirstOrDefault();

            CartaoViagem cartaoViagem;
            ContaBancaria contaBancaria;

            CriarUsuario(tarifa, out contaBancaria, out cartaoViagem);

            var resultado = false;
            switch (jornada)
            {
                case Jornada.Unica:
                    {

                        _metroCardService.EntrarNaEstacao(cartaoViagem, DateTime.Now, zona);
                        _metroCardService.SairDaEstacao(cartaoViagem, DateTime.Now);

                        var tarifaValida = tarifa.Valor == contaBancaria.SaldoDevedor;
                        var viagensValidas =
                            cartaoViagem.Viagens.Where(v => v.DataEntrada >= cartaoViagem.DataVigente).ToList().Count == 1 &&
                            cartaoViagem.Viagens.Where(v => v.DataEntrada > cartaoViagem.DataVigente).ToList().Count == 0
                            ;
                        resultado = tarifaValida && viagensValidas;
                        break;
                    };
                case Jornada.Dia:
                    {
                        var numeroDeViagens = 5;
                        for (int i = 0; i < numeroDeViagens; i++)
                        {
                            _metroCardService.EntrarNaEstacao(cartaoViagem, DateTime.Now, zona);
                            _metroCardService.SairDaEstacao(cartaoViagem, DateTime.Now);
                        }

                        var tarifaValida = tarifa.Valor == contaBancaria.SaldoDevedor;
                        var viagensNoDiaDaDataVigente = cartaoViagem.Viagens.Where(v => v.DataEntrada.Day == cartaoViagem.DataVigente.Value.Day).ToList();
                        var viagensForaDoDiaDaDataVigente = cartaoViagem.Viagens.Where(v => v.DataEntrada.Day == cartaoViagem.DataVigente.Value.AddDays(1).Day).ToList();
                        var viagensValidas = viagensForaDoDiaDaDataVigente.Count == 0 && viagensNoDiaDaDataVigente.Count > 0;
                        resultado = tarifaValida && viagensValidas;
                        break;
                    };
                case Jornada.Semana:
                    {

                        var dia = 1;
                        var data = DateTime.Now;
                        var dataLimite = data.AddDays(7);
                        do
                        {
                            if (dia > 1) data.AddDays(dia);
                            _metroCardService.EntrarNaEstacao(cartaoViagem, data, zona);
                            _metroCardService.SairDaEstacao(cartaoViagem, data);
                            dia++;
                        }
                        while (dia <= 7);

                        var tarifaValida = tarifa.Valor == contaBancaria.SaldoDevedor;
                        var viagensEmUmaSemana = cartaoViagem.Viagens.Where(v => v.DataEntrada >= cartaoViagem.DataVigente && v.DataEntrada <= dataLimite).ToList();
                        resultado = viagensEmUmaSemana.Count == 7 && !viagensEmUmaSemana.Any(v => v.DataEntrada > dataLimite.AddDays(1));
                        break;
                    };
                case Jornada.Mes:
                    {
                        _metroCardService.EntrarNaEstacao(cartaoViagem, DateTime.Now, zona);
                        _metroCardService.SairDaEstacao(cartaoViagem, DateTime.Now);

                        var tarifaValida = tarifa.Valor == contaBancaria.SaldoDevedor;
                        var viagensValidas =
                            cartaoViagem.Viagens.Where(v => v.DataEntrada >= cartaoViagem.DataVigente).ToList().Count >= 0 &&
                            cartaoViagem.Viagens.Where(v => v.DataEntrada >= cartaoViagem.DataVigente.Value.AddMonths(1)).ToList().Count == 0
                            ;
                        resultado = tarifaValida && viagensValidas;
                        break;
                    };
            }

            Assert.True(resultado);
        }

        [Theory]
        [InlineData(Jornada.Unica, Zona.A)]
        [InlineData(Jornada.Unica, Zona.B)]
        [InlineData(Jornada.Dia, Zona.A)]
        [InlineData(Jornada.Dia, Zona.B)]
        [InlineData(Jornada.Semana, Zona.A)]
        [InlineData(Jornada.Semana, Zona.B)]
        [InlineData(Jornada.Mes, Zona.A)]
        [InlineData(Jornada.Mes, Zona.B)]
        public void OPrecoDeveSerLimitadoATarifaDaquelePeriodoDeTempo(Jornada jornada, Zona zona)
        {
            Tarifa tarifa = _tarifaRepository.Read().Where(t => t.Jornada == jornada && t.Zona == zona).FirstOrDefault();

            CartaoViagem cartaoViagem;
            ContaBancaria contaBancaria;

            CriarUsuario(tarifa, out contaBancaria, out cartaoViagem);

            var resultado = false;
            switch (jornada)
            {
                case Jornada.Unica:
                    {
                        _metroCardService.EntrarNaEstacao(cartaoViagem, DateTime.Now, zona);
                        _metroCardService.SairDaEstacao(cartaoViagem, DateTime.Now);
                        resultado = contaBancaria.SaldoDevedor == tarifa.Valor;
                        break;
                    };
                case Jornada.Dia:
                    {
                        var numeroDeViagens = 5;
                        for (int i = 0; i < numeroDeViagens; i++)
                        {
                            _metroCardService.EntrarNaEstacao(cartaoViagem, DateTime.Now, zona);
                            _metroCardService.SairDaEstacao(cartaoViagem, DateTime.Now);
                        }

                        resultado = contaBancaria.SaldoDevedor == tarifa.Valor;
                        break;
                    };
                case Jornada.Semana:
                    {
                        var dia = 1;
                        var data = DateTime.Now;
                        var dataLimite = cartaoViagem.DataVigente = data.AddDays(7);
                        do
                        {
                            if (dia > 1) data.AddDays(dia);
                            _metroCardService.EntrarNaEstacao(cartaoViagem, data, zona);
                            _metroCardService.SairDaEstacao(cartaoViagem, data);
                            dia++;
                        }
                        while (dia <= 7);
                        resultado = contaBancaria.SaldoDevedor == tarifa.Valor;
                        break;
                    };
                case Jornada.Mes:
                    {
                        var dia = 1;
                        var data = DateTime.Now;
                        do
                        {
                            if (dia > 1) data.AddDays(dia);
                            _metroCardService.EntrarNaEstacao(cartaoViagem, data, zona);
                            _metroCardService.SairDaEstacao(cartaoViagem, data);
                            dia++;
                        }
                        while (dia <= 30);
                        resultado = contaBancaria.SaldoDevedor == tarifa.Valor;
                        break;
                    };

            }
            Assert.True(resultado);
        }

        [Theory]
        [InlineData(Jornada.Unica, Zona.A, 6)]
        [InlineData(Jornada.Unica, Zona.B, 7)]
        [InlineData(Jornada.Dia, Zona.A, 10)]
        [InlineData(Jornada.Dia, Zona.B, 12)]
        [InlineData(Jornada.Semana, Zona.A, 30)]
        [InlineData(Jornada.Semana, Zona.B, 45)]
        [InlineData(Jornada.Mes, Zona.A, 130)]
        [InlineData(Jornada.Mes, Zona.B, 170)]
        public void ATarifaDeveTerOSeguinteValor(Jornada jornada, Zona zona, decimal valor)
        {
            var tarifa = _tarifaRepository.Read().Where(t => t.Zona == zona && t.Jornada == jornada).FirstOrDefault();
            var resultado = tarifa.Valor == valor;
            Assert.True(resultado);
        }

        private void AdcionarProprietarioComUmCartaoTaxaUnicaZonaA(out CartaoViagem cartaoViagem, out ContaBancaria contaBancaria)
        {
            var proprietario = new Proprietario("Igor Silva");
            _proprietarioRepository.Create(proprietario);
            _proprietarioRepository.Commit();

            var tarifa = _tarifaRepository.Read().Where(t => t.Jornada == Jornada.Unica && t.Zona == Zona.A).First();
            cartaoViagem = new CartaoViagem(proprietario, tarifa);
            _cartaoViagemRepository.Create(cartaoViagem);
            _cartaoViagemRepository.Commit();

            contaBancaria = new ContaBancaria("111111111111111", proprietario);
            _contaBancariaRepository.Create(contaBancaria);
            _contaBancariaRepository.Commit();
        }
        private void AdcionarProprietarioComUmCartaoTaxaDiaZonaB(out CartaoViagem cartaoViagem, out ContaBancaria contaBancaria)
        {
            var proprietario = new Proprietario("Igor Silva");
            _proprietarioRepository.Create(proprietario);
            _proprietarioRepository.Commit();

            var tarifa = _tarifaRepository.Read().Where(t => t.Jornada == Jornada.Dia && t.Zona == Zona.B).First();
            cartaoViagem = new CartaoViagem(proprietario, tarifa);
            _cartaoViagemRepository.Create(cartaoViagem);
            _cartaoViagemRepository.Commit();

            contaBancaria = new ContaBancaria("111111111111111", proprietario);
            _contaBancariaRepository.Create(contaBancaria);
            _contaBancariaRepository.Commit();
        }
        private void CriarUsuario(Tarifa tarifa, out ContaBancaria contaBancaria, out CartaoViagem cartaoViagem)
        {
            var proprietario = new Proprietario("Franscisco");
            _proprietarioRepository.Create(proprietario);
            _proprietarioRepository.Commit();

            contaBancaria = new ContaBancaria("333333333333333", proprietario);
            _contaBancariaRepository.Create(contaBancaria);
            _contaBancariaRepository.Commit();

            cartaoViagem = new CartaoViagem(proprietario, tarifa);
            _cartaoViagemRepository.Create(cartaoViagem);
            _cartaoViagemRepository.Commit();
        }
    }
}
