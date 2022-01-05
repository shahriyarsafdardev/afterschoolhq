using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AfterSchoolAutomationTests.Pages
{
    public class LoginPage : Page
    {

        //------------Login WebElements-----------
        private IWebElement UserNameElement => _driver.FindElement(By.Id("username"));
        private IWebElement UserPasswordElement => _driver.FindElement(By.Id("password"));
        private IWebElement SubmitElement => _driver.FindElement(By.ClassName("btn-primary"));

        //------------Login Helper Methods-----------
        public void Navigate(string url)
        {
            _driver.Navigate().GoToUrl(url);
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

        public LoginPage ClickSubmit(string name)
        {
            UserPasswordElement.SendKeys(name);
            return this;
        }
        public void ClickSubmit() => SubmitElement.Click();
    }
}
