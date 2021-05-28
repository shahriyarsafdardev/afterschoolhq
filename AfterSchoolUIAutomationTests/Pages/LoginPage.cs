using System;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace AfterSchoolUIAutomationTests.Pages
{
    public class LoginPage
    {
        public LoginPage()
        {
        }

        //The URL of the calculator to be opened in the browser
        private const string LoginUrl = "https://qa.afterschoolhq.com/login";

        //The Selenium web driver to automate the browser
        private readonly IWebDriver _webDriver;

        //The default wait time in seconds for wait.Until
        public const int DefaultWaitInSeconds = 5;

        public LoginPage(IWebDriver webDriver)
        {
            _webDriver = webDriver;
        }

        //------------Finding Login WebElements-----------
        private IWebElement UserNameElement => _webDriver.FindElement(By.Id("username"));
        private IWebElement UserPasswordElement => _webDriver.FindElement(By.Id("password"));
        private IWebElement SubmitElement => _webDriver.FindElement(By.ClassName("btn-primary"));

        //------------Login Helper Methods-----------
        public void Navigate(string url)
        {
            _webDriver.Navigate().GoToUrl(url);
        }

        public LoginPage PopulateName(string name)
        {
            UserNameElement.SendKeys(name);
            return this;
        }
        //public void PopulateName(string name) => UserNameElement.SendKeys(name);

        public LoginPage PopulatePassword(string name)
        {
            UserPasswordElement.SendKeys(name);
            return this;
        }
        //public void PopulatePassword(string password) => UserPasswordElement.SendKeys(password);

        public LoginPage ClickSubmit()
        {
            SubmitElement.Click();
            return this;
        }
        //public void ClickSubmit() => SubmitElement.Click();
    }
}
