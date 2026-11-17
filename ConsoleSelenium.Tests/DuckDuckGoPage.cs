using System;
using System.Linq;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace ConsoleSelenium.Tests;

/// <summary>
/// Page Object do DuckDuckGo: concentra os seletores e as ações da página,
/// deixando a classe de testes apenas com o cenário e as asserções.
/// </summary>
public class DuckDuckGoPage
{
    private const string UrlBase = "https://duckduckgo.com";
    private const string DominioDaBusca = "duckduckgo.com";

    private static readonly By CampoPesquisa = By.Name("q");

    private static readonly By Sugestoes = By.CssSelector(
        ".search-suggestions-list li, .sbox-suggestions li, [role='option']");

    private static readonly By LinksOrganicos = By.CssSelector(
        "li[data-layout='organic'] a[data-testid='result-title-a']");

    private static readonly By LinksDeResultado = By.CssSelector(
        "a[data-testid='result-title-a'], #links a.result__a");

    private readonly IWebDriver _driver;
    private readonly WebDriverWait _espera;

    public DuckDuckGoPage(IWebDriver driver)
    {
        _driver = driver;
        _espera = new WebDriverWait(driver, TimeSpan.FromSeconds(15));
        _espera.IgnoreExceptionTypes(
            typeof(NoSuchElementException),
            typeof(StaleElementReferenceException));
    }

    public string Titulo => _driver.Title;

    public string Url => _driver.Url;

    public void Abrir()
    {
        _driver.Navigate().GoToUrl(UrlBase);
    }

    /// <summary>Digita o termo no campo de pesquisa, sem enviar o formulário.</summary>
    public void DigitarTermo(string termo)
    {
        var campo = AguardarCampoPesquisa();
        campo.Clear();
        campo.SendKeys(termo);
    }

    /// <summary>Digita o termo, pressiona Enter e aguarda a página de resultados.</summary>
    public void Pesquisar(string termo)
    {
        DigitarTermo(termo);
        AguardarCampoPesquisa().SendKeys(Keys.Enter);
        AguardarPaginaDeResultados();
    }

    /// <summary>
    /// Aguarda a lista de autocompletar, clica na sugestão da posição informada
    /// (1 = primeira) e devolve o texto da sugestão escolhida.
    /// </summary>
    public string ClicarSugestao(int posicao)
    {
        var sugestao = _espera.Until(driver =>
        {
            var visiveis = driver.FindElements(Sugestoes)
                .Where(elemento => elemento.Displayed)
                .ToList();

            return visiveis.Count >= posicao ? visiveis[posicao - 1] : null;
        })!;

        var texto = sugestao.Text.Split('\n')[0].Trim();
        sugestao.Click();
        AguardarPaginaDeResultados();

        return texto;
    }

    /// <summary>Clica no primeiro link orgânico e aguarda a saída do DuckDuckGo.</summary>
    public void ClicarPrimeiroResultadoOrganico()
    {
        var link = _espera.Until(driver =>
        {
            var organicos = driver.FindElements(LinksOrganicos);
            var candidatos = organicos.Count > 0
                ? organicos
                : driver.FindElements(LinksDeResultado);

            return candidatos.FirstOrDefault(elemento => elemento.Displayed);
        })!;

        link.Click();
        _espera.Until(_ => EstaEmPaginaExterna());
    }

    /// <summary>Usa o botão "Voltar" do navegador e aguarda a página de resultados.</summary>
    public void VoltarParaResultados()
    {
        _driver.Navigate().Back();
        AguardarPaginaDeResultados();
    }

    public bool EstaEmPaginaExterna()
    {
        var url = _driver.Url;

        return url.StartsWith("http", StringComparison.OrdinalIgnoreCase)
            && !new Uri(url).Host.EndsWith(DominioDaBusca, StringComparison.OrdinalIgnoreCase);
    }

    public bool EstaNaPaginaDeResultados()
    {
        var url = _driver.Url;

        return url.StartsWith("http", StringComparison.OrdinalIgnoreCase)
            && new Uri(url).Host.EndsWith(DominioDaBusca, StringComparison.OrdinalIgnoreCase)
            && !string.IsNullOrWhiteSpace(ObterTermoDaUrl());
    }

    public bool ExibeResultados()
    {
        return _espera.Until(driver =>
            driver.FindElements(LinksDeResultado).Any(elemento => elemento.Displayed));
    }

    /// <summary>Lê o termo pesquisado no parâmetro "q" da URL, já decodificado.</summary>
    public string ObterTermoDaUrl()
    {
        var url = _driver.Url;
        if (!url.StartsWith("http", StringComparison.OrdinalIgnoreCase))
        {
            return string.Empty;
        }

        var parametros = new Uri(url).Query.TrimStart('?').Split('&');

        foreach (var parametro in parametros)
        {
            var partes = parametro.Split('=', 2);
            if (partes.Length == 2 && partes[0] == "q")
            {
                return Uri.UnescapeDataString(partes[1].Replace('+', ' '));
            }
        }

        return string.Empty;
    }

    private IWebElement AguardarCampoPesquisa()
    {
        return _espera.Until(driver =>
        {
            var campo = driver.FindElement(CampoPesquisa);
            return campo.Displayed && campo.Enabled ? campo : null;
        })!;
    }

    private void AguardarPaginaDeResultados()
    {
        _espera.Until(_ => EstaNaPaginaDeResultados());
    }
}
