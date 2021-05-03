Feature: CartaoViagem
	Para realizar uma viagem na estação de metro
	Eu presciso comprar um cartão de viagem
	E presciso escolher uma jornada e uma zona
	

Scenario: Obter Um Cartao Viagem e Realizar Uma Viagem
	Given Um novo proprietario Proprietario_Cartao_Viagem as nome
	And Com uma conta bancaria 123456789101213 as numero
	And Eu escolho A as zona
	And Eu escolho Unica as jornada
	When Eu realizo uma viagem A as zona
	Then O valor cobrado na conta bancaria deve ser 6 as valorTarifa

Scenario: Usar Cartao Viagem Invalido
	Given Um novo proprietario Proprietario_Cartao_Viagem as nome
	And Com uma conta bancaria 123456789101213 as numero
	When Eu entrar na estacao com um cartao invalido
	Then Ao Realizar Uma Viagem Deve ser informado uma excecao "cartao_invalido" as mensagem

Scenario: Usar Cartao Viagem Invalido Em uma Zona Invalida
	Given Um novo proprietario Proprietario_Cartao_Viagem as nome
	And Eu escolho A as zona
	And Eu escolho Unica as jornada
	And Com uma conta bancaria 123456789101213 as numero
	When Eu realizo uma viagem B as zona
	Then Ao Realizar Uma Viagem Deve ser informado uma excecao "zona_cartao_invalida" as mensagem