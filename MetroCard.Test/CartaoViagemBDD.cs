using MetroCard.Data;
using MetroCard.Domain.Entities;
using MetroCard.Domain.Entities.Enums;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Gherkin.Quick;

namespace MetroCard.Test
{
    [FeatureFile("./CartaoViagemBDD.feature")]
    public sealed class CartaoViagemBDD : Feature
    {
        public Zona Zona { get; set; }
        public Jornada Jornada { get; set; }
        public Proprietario Proprietario { get; set; }
        public ContaBancaria ContaBancaria { get; set; }
        public CartaoViagem CartaoViagem { get; set; }


        private readonly GenericRepository<Proprietario> _proprietarioRepository;
        private readonly GenericRepository<ContaBancaria> _contaBancariaRepository;
        private readonly GenericRepository<CartaoViagem> _cartaoViagemRepository;
        private readonly GenericRepository<Tarifa> _tarifaRepository;
        private readonly GenericRepository<Viagem> _viagemRepository;
        private readonly MetroCardService _metroCardService;

        public CartaoViagemBDD()
        {
            //var optionsBuilder = new DbContextOptionsBuilder<DataContext>();
            //optionsBuilder.UseInMemoryDatabase("InMemoryDatabase");
            var db = new DataContext();
            _proprietarioRepository = new GenericRepository<Proprietario>(db);
            _contaBancariaRepository = new GenericRepository<ContaBancaria>(db);
            _cartaoViagemRepository = new GenericRepository<CartaoViagem>(db);
            _tarifaRepository = new GenericRepository<Tarifa>(db);
            _viagemRepository = new GenericRepository<Viagem>(db);
            _metroCardService = new MetroCardService(db);
        }

        [Given(@"Um novo proprietario (.+) as nome")]
        public void Um_novo_proprietario_nome(string nome)
        {
            Proprietario = new Proprietario(nome);
            _proprietarioRepository.Create(Proprietario);
            _proprietarioRepository.Commit();
        }

        [And(@"Com uma conta bancaria (.+) as numero")]
        public void Com_uma_conta_bancaria_numero(string numero)
        {
            ContaBancaria = new ContaBancaria(numero, Proprietario);
            _contaBancariaRepository.Create(ContaBancaria);
            _contaBancariaRepository.Commit();
        }

        [And(@"Eu escolho (.+) as zona")]
        public void Eu_escolho_zona(char zona)
        {
            Zona = (Zona)Enum.Parse(typeof(Zona), $"{zona}".ToUpper());
        }

        [And(@"Eu escolho (.+) as jornada")]
        public void Eu_escolho_jornada(string jornada)
        {
            Jornada = (Jornada)Enum.Parse(typeof(Jornada), jornada);

            var tarifa = _tarifaRepository.Read().Where(t => t.Zona == Zona && t.Jornada == Jornada).FirstOrDefault();

            CartaoViagem = new CartaoViagem(Proprietario, tarifa);

            _cartaoViagemRepository.Create(CartaoViagem);
            _cartaoViagemRepository.Commit();

        }

        [When(@"Eu realizo uma viagem (.+) as zona")]
        public void Eu_realizo_uma_viagem(string zona)
        {
            var dataEntrada = DateTime.Now;
            Zona = (Zona)Enum.Parse(typeof(Zona), $"{zona}".ToUpper());
            if ((CartaoViagem.Tarifa.Zona == Zona.A && Zona == Zona.A) ||
                (CartaoViagem.Tarifa.Zona == Zona.B && (Zona == Zona.A || Zona == Zona.B)))
            {
                RealizarUmaViagem(CartaoViagem, dataEntrada, Zona);
            }
        }

        [When("Eu entrar na estacao com um cartao invalido")]
        public void Eu_entrar_na_estacao_com_um_cartao_invalido()
        {
            Zona = Zona.A;
            Jornada = Jornada.Unica;

            var tarifa = _tarifaRepository.Read().Where(t => t.Zona == Zona && t.Jornada == Jornada).FirstOrDefault();

            CartaoViagem = new CartaoViagem(Proprietario, tarifa);


            _cartaoViagemRepository.Create(CartaoViagem);
            _proprietarioRepository.Update(Proprietario);

            switch (Jornada)
            {
                case Jornada.Unica:
                    {
                        //Consumir cartão com jornada unica
                        RealizarUmaViagem(CartaoViagem, DateTime.Now, Zona);
                        break;
                    }
            }
        }

        private void RealizarUmaViagem(CartaoViagem cartaoViagem, DateTime dataEntrada, Zona zona)
        {
            _metroCardService.EntrarNaEstacao(cartaoViagem, dataEntrada, zona);
            Task.Delay(3000);
            var dataSaida = DateTime.Now;
            _metroCardService.SairDaEstacao(cartaoViagem, dataSaida);
        }

        [Then(@"Ao Realizar Uma Viagem Deve ser informado uma excecao ""(\w+)"" as mensagem")]
        public void Ao_Realizar_Uma_ViagemDeve_ser_informado_uma_excecao(string mensagem)
        {
            var resultado = "";

            try
            {
                RealizarUmaViagem(CartaoViagem, DateTime.Now, Zona);
            }
            catch (Exception ex)
            {
                resultado = ex.Message;
            }

            Assert.Equal(mensagem, resultado);
        }

        [Then(@"O valor cobrado na conta bancaria deve ser ([\d\.]+) as valorTarifa")]
        public void O_valor_cobrado_na_conta_bancaria_deve_ser(decimal valorTarifa)
        {
            Assert.Equal(valorTarifa, ContaBancaria.SaldoDevedor);
        }
    }
}