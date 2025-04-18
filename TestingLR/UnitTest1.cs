using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TestingLR
{
    [TestFixture]
    public class WikiWebsiteTests
    {
        private IWebDriver driver;
        private WebDriverWait wait;
        private const string BaseUrl = "https://www.wikipedia.org/";
        private const string RuUrl = "https://ru.wikipedia.org/";

        [SetUp]
        public void Setup()
        {
            var options = new ChromeOptions();
            //var options = new FirefoxOptions();
            options.AddArgument("--start-maximized");
            driver = new ChromeDriver(options);
            //driver = new FirefoxOptions(options);
            wait = new WebDriverWait(driver, TimeSpan.FromSeconds(15));
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(5);
        }

        [Test]
        public void CheckPageTitle()
        {
            driver.Navigate().GoToUrl(BaseUrl);
            string expectedTitle = "Wikipedia";
            Assert.That(driver.Title, Is.EqualTo(expectedTitle),
                "Заголовок страницы не соответствует ожидаемому");
        }

        [Test]
        public void MainPageElementsAreVisible()
        {
            driver.Navigate().GoToUrl(BaseUrl);

            Assert.Multiple(() =>
            {
                Assert.That(wait.Until(e => 
                    e.FindElement(By.ClassName("central-textlogo"))).Displayed,
                    "Логотип Wikipedia не отображается");

                Assert.That(wait.Until(e => 
                    e.FindElement(By.ClassName("central-featured"))).Displayed,
                    "Центральные поля не отображаются");

                Assert.That(wait.Until(e =>
                    e.FindElement(By.ClassName("search-container"))).Displayed,
                    "Поисковая строка не отображаются");

                Assert.That(wait.Until(e => 
                    e.FindElement(By.ClassName("footer"))).Displayed,
                    "Футер не отображается");
            });
        }

        [Test]
        public void NavigationMenuWorks()
        {
            driver.Navigate().GoToUrl(RuUrl);

            var allFlowsMenu = wait.Until(e => e.FindElement(
                By.XPath("//*[@class=\"main-wrapper\"]/div/div//*[@class=\"fake-heading h2 main-header\"]/a")));    // //*[@class=\"main-wrapper\"]/div[2]/div/div/ul/li/b
            allFlowsMenu.Click();

            var developmentTab = wait.Until(e => e.FindElement(
                By.XPath("//*[@id=\"p-navigation\"]/div/ul/li")));
            developmentTab.Click();

            wait.Until(e => e.Url.Contains("https://ru.wikipedia.org/wiki/%D0%97%D0%B0%D0%B3%D0%BB%D0%B0%D0%B2%D0%BD%D0%B0%D1%8F_%D1%81%D1%82%D1%80%D0%B0%D0%BD%D0%B8%D1%86%D0%B0"));
            Assert.That(driver.Url, Does.Contain("https://ru.wikipedia.org/wiki/%D0%97%D0%B0%D0%B3%D0%BB%D0%B0%D0%B2%D0%BD%D0%B0%D1%8F_%D1%81%D1%82%D1%80%D0%B0%D0%BD%D0%B8%D1%86%D0%B0"),
                "Не удалось вернуться на начальную страницу");
        }

        [Test]
        public void SearchFunctionalityWorks()
        {
            driver.Navigate().GoToUrl(BaseUrl);

            var searchInput = wait.Until(e => e.FindElement(
                By.Id("searchInput")));
            searchInput.SendKeys("smth" + Keys.Enter);

            wait.Until(e => !e.Url.Contains(BaseUrl));

            var results = wait.Until(e => e.FindElements(
                By.CssSelector(".mw-search-result")));                   // /div[3]/div[3]/div[4]/div[4]/div[3]/ul/li

            Assert.That(results.Count, Is.GreaterThan(0),
                "Не найдены результаты поиска");
        }

        [Test]
        public void ArticleOpeningWorks()
        {
            driver.Navigate().GoToUrl(BaseUrl);

            var searchInput = wait.Until(e => e.FindElement(
                By.Id("searchInput")));
            searchInput.SendKeys("Selenium" + Keys.Enter);

            wait.Until(e => e.Url.Contains("https://ru.wikipedia.org/wiki/Selenium"));
            Assert.That(driver.Url, Does.Contain("https://ru.wikipedia.org/wiki/Selenium"),
                "Не удалось открыть статью Selenium");
        }

        [Test]
        public void HeaderDropdownMenuLinksWorkCorrectly()
        {
            driver.Navigate().GoToUrl(RuUrl);

            var dropdownButton = wait.Until(e => e.FindElement(
                By.CssSelector(".main-footer-menu")));
            dropdownButton.Click();

            var dropdownMenu = wait.Until(e => e.FindElement(
                By.CssSelector("div.main-footer-menuDropdown")));

            var menuLinks = dropdownMenu.FindElements(
                By.TagName("a"));


            //TestContext.Out.WriteLine(menuLinks[0]);

            Assert.That(menuLinks.Count, Is.EqualTo(2),
                "Количество ссылок в меню не равно 2");

            var expectedLinks = new List<(string Title, string Url)>
            {
                ("Кандидаты","https://ru.wikipedia.org/wiki/%D0%9F%D1%80%D0%BE%D0%B5%D0%BA%D1%82:%D0%98%D0%B7%D0%B1%D1%80%D0%B0%D0%BD%D0%BD%D1%8B%D0%B5_%D1%81%D1%82%D0%B0%D1%82%D1%8C%D0%B8/%D0%9A%D0%B0%D0%BD%D0%B4%D0%B8%D0%B4%D0%B0%D1%82%D1%8B"),
                ("Просмотр шаблона", "https://ru.wikipedia.org/wiki/%D0%A8%D0%B0%D0%B1%D0%BB%D0%BE%D0%BD:%D0%A2%D0%B5%D0%BA%D1%83%D1%89%D0%B0%D1%8F_%D0%B8%D0%B7%D0%B1%D1%80%D0%B0%D0%BD%D0%BD%D0%B0%D1%8F_%D1%81%D1%82%D0%B0%D1%82%D1%8C%D1%8F")
            };

            for (int i = 0; i < menuLinks.Count; i++)
            {
                var link = menuLinks[i];
                var expected = expectedLinks[i];

                string actualHref = link.GetAttribute("href").Split('?')[0];
                Assert.That(actualHref, Is.EqualTo(expected.Url),
                    $"Неверный URL для {expected.Title}");

            }

            if (TestLinksOpening)
            {
                TestLinkOpening(menuLinks[1]);
            }
        }

        private void TestLinkOpening(IWebElement link)
        {
            string originalWindow = driver.CurrentWindowHandle;
            string linkUrl = link.GetAttribute("href");

            ((IJavaScriptExecutor)driver).ExecuteScript(
                "arguments[0].setAttribute('target', '_blank'); arguments[0].click();", link);

            wait.Until(d => d.WindowHandles.Count == 2);

            foreach (string window in driver.WindowHandles)
            {
                if (window != originalWindow)
                {
                    driver.SwitchTo().Window(window);
                    break;
                }
            }

            Assert.That(driver.Url, Does.StartWith(linkUrl.Split('?')[0]),
                "Открылась неверная страница");

            driver.Close();
            driver.SwitchTo().Window(originalWindow);
        }

        private bool TestLinksOpening = true;

        [Test]
        public void LangNavigationWorks()
        {
            driver.Navigate().GoToUrl(RuUrl);

            var langs = wait.Until(e =>
                e.FindElements(By.CssSelector("li.interlanguage-link")));

            //TestContext.Out.WriteLine(langs.Count);

            for (int i = 0; i < langs.Count; i++) {
                var lang = driver.FindElements(By.CssSelector("li.interlanguage-link"))[i];

                var firstTag = lang.FindElement(By.CssSelector("a.interlanguage-link-target"));
            
                //TestContext.Out.WriteLine(firstTag.GetAttribute("data-language-local-name"));
                
                string tagName = firstTag.GetAttribute("data-language-local-name");
                string expectedUrl = firstTag.GetAttribute("href");

                firstTag.Click();

                Assert.That(driver.Url, Does.Contain(".wikipedia.org/wiki/"),
                    "Не удалось открыть вики на языке: " + tagName);
                
                driver.Navigate().GoToUrl(RuUrl);
            }
        }

        [Test]
        public void AuthRedirectsToLoginPage()
        {
            driver.Navigate().GoToUrl(RuUrl);

            var authButton = wait.Until(e => e.FindElement(
                By.Id("pt-login")));
            string originalWindow = driver.CurrentWindowHandle;
            authButton.Click();

            Assert.That(driver.Url, Does.Contain("https://auth.wikimedia.org/ruwiki/wiki/%D0%A1%D0%BB%D1%83%D0%B6%D0%B5%D0%B1%D0%BD%D0%B0%D1%8F:%D0%92%D1%85%D0%BE%D0%B4?useformat=desktop&usesul3=1&returnto=Main_Page&centralauthLoginToken="),
                "Не произошло перенаправление на страницу авторизации");

        }

        [Test]
        public void FooterLinksAreValid()
        {
            driver.Navigate().GoToUrl(RuUrl);

            var footerLinks = wait.Until(e => e.FindElement(
                By.Id("footer-places")).FindElements(By.TagName("a")));

            Assert.That(footerLinks.Count, Is.GreaterThan(1),
                "Нет ссылок в футере");

            foreach (var link in footerLinks)
            {
                //TestContext.Out.WriteLine(link.Text);

                string href = link.GetAttribute("href");
                Assert.That(href, Is.Not.Null.And.Not.Empty,
                    $"Ссылка '{link.Text}' не имеет атрибута href");
            }
        }

        [Test]
        public void LogoIsDisplayedCorrectly()
        {
            driver.Navigate().GoToUrl(RuUrl);

            var logo = wait.Until(e => e.FindElement(
                By.CssSelector("a.mw-wiki-logo")));

            Assert.That(logo.Displayed, Is.True,
                "Логотип не отображается на странице");

            string logoHref = logo.GetAttribute("href");
            Assert.That(logoHref, Is.EqualTo("https://ru.wikipedia.org/wiki/%D0%97%D0%B0%D0%B3%D0%BB%D0%B0%D0%B2%D0%BD%D0%B0%D1%8F_%D1%81%D1%82%D1%80%D0%B0%D0%BD%D0%B8%D1%86%D0%B0"),
                "Логотип ведет на неверный URL");
        }

        [TearDown]
        public void TearDown()
        {
            driver.Close();
        }
    }
}
