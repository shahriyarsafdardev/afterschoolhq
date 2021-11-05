using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;
using System.IO;
using System.Reflection;

namespace AfterSchoolUISeleniumTests
{
    [TestClass]
    public class SearchGoogle
    {
        [TestMethod]
        public void SearchCheese()
        {
            var chromeOptions = new ChromeOptions();
            chromeOptions.AddArguments("headless");

            using (var driver = new ChromeDriver(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), chromeOptions))
            {
                driver.Navigate().GoToUrl("http://www.google.com/");

                // find the text input element by its name
                IWebElement query = driver.FindElement(By.Name("q"));

                // enter something to search for
                query.SendKeys("Cheese");
                query.Submit();

                var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(20));
                wait.Until(d => d.Title.StartsWith("cheese", StringComparison.OrdinalIgnoreCase));

                Assert.AreEqual(driver.Title, "Cheese - Google Search");
            }
        }
    }
}