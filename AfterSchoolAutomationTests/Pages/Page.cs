using OpenQA.Selenium;
using System;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using AfterSchoolAutomationTests.Helpers.Extensions;
using AfterSchoolAutomationTests.UITests;

namespace AfterSchoolAutomationTests.Pages
{
    public class Page : IDisposable
    {
        private readonly IWebDriver _driver;
        private readonly Helper _helper;
        public Page()
        {
            _driver = new ChromeDriver();
            _helper = new Helper(_driver);
        }

        public void Dispose()
        {
            _driver.Quit();
            _driver.Dispose();
        }
    }
}
