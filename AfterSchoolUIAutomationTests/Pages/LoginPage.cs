using AfterSchoolUIAutomationTests.Helpers.Extensions;
using OpenQA.Selenium;

namespace AfterSchoolUIAutomationTests.Pages
{
    public class LoginPage
    {
        private readonly IWebDriver webDriver;

        public LoginPage(IWebDriver WebDriver)
        {
            webDriver = WebDriver;
        }

        //------------Finding Login WebElements-----------
        private IWebElement UserNameElement => webDriver.FindElement(By.Id("username"));
        private IWebElement UserPasswordElement => webDriver.FindElement(By.Id("password"));
        private IWebElement SubmitElement => webDriver.FindElement(By.ClassName("btn-primary"));

        public LoginPage PopulateName(string name)
        {
            webDriver.WaitInSeconds(2);
            UserNameElement.SendKeys(name);
            webDriver.WaitInSeconds(2);
            return this;
        }

        public LoginPage PopulatePassword(string name)
        {
            UserPasswordElement.SendKeys(name);
            webDriver.WaitInSeconds(2);
            return this;
        }

        public LoginPage ClickSubmit()
        {
            SubmitElement.Click();
            webDriver.WaitInSeconds(5);
            return this;
        }
    }
}