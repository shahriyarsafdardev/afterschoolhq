using System;
using System.Threading;
using AfterSchoolUIAutomationTests.Helpers.Extensions;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace AfterSchoolUIAutomationTests.Pages
{
    public class EditAccountProfilePage : IDisposable
    {
        private IWebDriver webDriver;

        public const int DefaultWaitInSeconds = 5;
        public EditAccountProfilePage(IWebDriver WebDriver)
        {
            webDriver = WebDriver;
        }

        //------------Finding Edit Account Profile WebElements-----------
        private IWebElement AccountButtonElement => webDriver.FindElement(By.XPath("(//div[@id='accounts']//button)[1]"));
        private IWebElement UserFirstNameElement => webDriver.FindElement(By.Id("firstname"));
        private IWebElement UserLastNameElement => webDriver.FindElement(By.Id("lastname"));
        private IWebElement UserEmailElement => webDriver.FindElement(By.Id("email"));
        private IWebElement UserPassword1Element => webDriver.FindElement(By.Id("password1"));
        private IWebElement UserPassword2Element => webDriver.FindElement(By.Id("password2"));
        private IWebElement SubmitElement => webDriver.FindElement(By.XPath("//div[@class='col-sm-6']//button[text()='Update']"));
        private IWebElement ToastElement => webDriver.FindElement(By.XPath("//div[@class='gritter-content gritter-without-image']//span[@class='gritter-title']/text()"));

        //------------Edit Account Profile Helper Methods-----------
        public EditAccountProfilePage ClickAccountButton()
        {
            AccountButtonElement.Click();
            webDriver.WaitInSeconds(5);
            return this;
        }

        public EditAccountProfilePage PopulateFirstName(string firstName)
        {
            UserFirstNameElement.Clear();
            UserFirstNameElement.SendKeys(firstName);
            webDriver.WaitInSeconds(1);
            return this;
        }

        public EditAccountProfilePage PopulateLastName(string lastName)
        {
            UserLastNameElement.Clear();
            UserLastNameElement.SendKeys(lastName);
            webDriver.WaitInSeconds(1);
            return this;
        }

        public EditAccountProfilePage PopulateUserEmail(string email)
        {
            UserEmailElement.Clear();
            UserEmailElement.SendKeys(email);
            webDriver.WaitInSeconds(1);
            return this;
        }

        public EditAccountProfilePage PopulateUserPassword(string password)
        {
            UserPassword1Element.Clear();
            UserPassword1Element.SendKeys(password);
            webDriver.WaitInSeconds(1);
            return this;
        }

        public EditAccountProfilePage PopulateConfirmPassword(string confirmPassword)
        {
            UserPassword2Element.Clear();
            UserPassword2Element.SendKeys(confirmPassword);
            webDriver.WaitInSeconds(1);
            return this;
        }

        public EditAccountProfilePage ClickUpdate()
        {
            SubmitElement.Click();
            webDriver.WaitInSeconds(2);
            return this;
        }

        public string ToastMessage()
        {
            //webDriver.WaitInSeconds(2);
            //Thread.Sleep(2000);
            //WebDriverWait wait = new WebDriverWait(webDriver, new TimeSpan(0, 0, 5));
            //wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.XPath("//div[@class='gritter-content gritter-without-image']//span[@class='gritter-title']/text()")));
            //return this;

            string message = ToastElement.Text;
            webDriver.WaitInSeconds(2);
            return message;
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
