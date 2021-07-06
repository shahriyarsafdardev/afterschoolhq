using System;
using System.Threading;
using AfterSchoolUIAutomationTests.Helpers.Extensions;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace AfterSchoolUIAutomationTests.Pages
{
    public class CreateActivityPage : IDisposable
    {
        private IWebDriver webDriver;

        public CreateActivityPage(IWebDriver WebDriver)
        {
            webDriver = WebDriver;
        }

        //------------Finding Create Activity WebElements-----------
        private IWebElement UserFirstNameElement => webDriver.FindElement(By.Id("name"));
        private IWebElement UserLastNameElement => webDriver.FindElement(By.Id("lastname"));
        private IWebElement UserEmailElement => webDriver.FindElement(By.Id("email"));
        private IWebElement UserPassword1Element => webDriver.FindElement(By.Id("password1"));
        private IWebElement UserPassword2Element => webDriver.FindElement(By.Id("password2"));
        private IWebElement SubmitElement => webDriver.FindElement(By.XPath("//div[@class='col-sm-6']//button[text()='Update']"));
        private IWebElement ToastElement => webDriver.FindElement(By.XPath("//div[@class='gritter-content gritter-without-image']//span[@class='gritter-title']/text()"));

        //------------Edit Account Profile Helper Methods-----------
        public void Navigate(string url)
        {
            webDriver.Navigate().GoToUrl(url);
        }

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
