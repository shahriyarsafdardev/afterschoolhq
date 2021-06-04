using System;
using AfterSchoolUIAutomationTests.Helpers.Extensions;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace AfterSchoolUIAutomationTests.Pages
{
    public class LoginPage : IDisposable
    {

        private IWebDriver webDriver;

        public const int DefaultWaitInSeconds = 5;

        public LoginPage(IWebDriver WebDriver)
        {
            webDriver = WebDriver;
        }

        //------------Finding Login WebElements-----------
        private IWebElement UserNameElement => webDriver.FindElement(By.Id("username"));
        private IWebElement UserPasswordElement => webDriver.FindElement(By.Id("password"));
        private IWebElement SubmitElement => webDriver.FindElement(By.ClassName("btn-primary"));

        //------------Login Helper Methods-----------
        public void Navigate(string url)
        {
            webDriver.Navigate().GoToUrl(url);
        }

        public LoginPage PopulateName(string name)
        {
            webDriver.WaitInSeconds(2);
            UserNameElement.SendKeys(name);
            webDriver.WaitInSeconds(2);
            return this;
        }
        //public void PopulateName(string name) => UserNameElement.SendKeys(name);

        public LoginPage PopulatePassword(string name)
        {
            UserPasswordElement.SendKeys(name);
            webDriver.WaitInSeconds(2);
            return this;
        }
        //public void PopulatePassword(string password) => UserPasswordElement.SendKeys(password);

        public LoginPage ClickSubmit()
        {
            SubmitElement.Click();
            webDriver.WaitInSeconds(5);
            return this;
        }
        //public void ClickSubmit() => SubmitElement.Click();

        public void Dispose()
        {
            if (webDriver != null)
            {
                webDriver.Dispose();
                webDriver = null;
            }
        }
    }
}
