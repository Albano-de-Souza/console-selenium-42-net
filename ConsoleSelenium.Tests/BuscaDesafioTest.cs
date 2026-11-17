using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace ConsoleSelenium.Tests;

[TestClass]
public class BuscaDesafioTest
{
    private IWebDriver _driver = null!;
    private DuckDuckGoPage _pagina = null!;

    [TestInitialize]
    public void Iniciar()
    {
        var opcoes = new ChromeOptions();
        opcoes.AddArgument("--start-maximized");

        _driver = new ChromeDriver(opcoes);
        _pagina = new DuckDuckGoPage(_driver);
        _pagina.Abrir();
    }

    [TestCleanup]
    public void Encerrar()
    {
        _driver?.Quit();
    }

    [TestMethod]
    public void Exercicio1_PesquisaBasica_ValidaTituloDaPaginaDeResultados()
    {
        const string termo = "Selenium WebDriver C#";

        _pagina.Pesquisar(termo);

        var tituloContemTermo = _pagina.Titulo.Contains(termo, StringComparison.OrdinalIgnoreCase);
        var urlContemTermo = _pagina.ObterTermoDaUrl().Contains(termo, StringComparison.OrdinalIgnoreCase);

        Assert.IsTrue(
            tituloContemTermo || urlContemTermo,
            $"Título ou URL deveriam conter '{termo}'. Título: '{_pagina.Titulo}'. URL: '{_pagina.Url}'.");
    }

    [TestMethod]
    public void Exercicio2_Autocompletar_SelecionaTerceiraSugestao()
    {
        const string termo = "C# unit testing";
        const int posicaoDaSugestao = 3;

        _pagina.DigitarTermo(termo);
        var sugestaoEscolhida = _pagina.ClicarSugestao(posicaoDaSugestao);

        Assert.IsFalse(
            string.IsNullOrWhiteSpace(sugestaoEscolhida),
            "A terceira sugestão do autocompletar deveria ter um texto.");

        Assert.IsTrue(
            _pagina.ObterTermoDaUrl().Contains(sugestaoEscolhida, StringComparison.OrdinalIgnoreCase),
            $"A busca deveria ser pela sugestão '{sugestaoEscolhida}'. URL: '{_pagina.Url}'.");
    }

    [TestMethod]
    public void Exercicio3_NavegacaoHistorico_VoltaParaPaginaDeResultados()
    {
        const string termo = "Automated Testing";

        _pagina.Pesquisar(termo);
        _pagina.ClicarPrimeiroResultadoOrganico();

        Assert.IsTrue(
            _pagina.EstaEmPaginaExterna(),
            $"Deveria estar em uma página externa ao DuckDuckGo. URL: '{_driver.Url}'.");

        _pagina.VoltarParaResultados();

        Assert.IsTrue(
            _pagina.EstaNaPaginaDeResultados(),
            $"Deveria ter voltado para a página de resultados. URL: '{_driver.Url}'.");

        Assert.AreEqual(
            termo,
            _pagina.ObterTermoDaUrl(),
            ignoreCase: true,
            "O termo pesquisado deveria ser restaurado ao voltar.");

        Assert.IsTrue(
            _pagina.ExibeResultados(),
            "Os resultados da busca deveriam estar visíveis ao voltar.");
    }
}
