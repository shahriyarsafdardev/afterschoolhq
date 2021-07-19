using System;
using System.Threading;
using AfterSchoolUIAutomationTests.Helpers.Extensions;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace AfterSchoolUIAutomationTests.Pages
{
    public class EmailConfirmationPage : IDisposable
    {
        private IWebDriver webDriver;

        public EmailConfirmationPage(IWebDriver WebDriver)
        {
            webDriver = WebDriver;
        }

        //------------Finding Email Confirmation WebElements-----------//

        private IWebElement EmailConfirmationModalCloseElement => webDriver.FindElement(By.XPath("(//div[@class='modal-body account-getting-started-padding'])[1]//a[@class='close close-icon-position']//span"));
        private IWebElement ActivityProviderEmailElement => webDriver.FindElement(By.XPath("(//div[@class='activity-detail-block'])[3]//a"));
        private IWebElement YopEmailTextInputElement => webDriver.FindElement(By.XPath("//div[@class='tooltip click']//input"));
        private IWebElement YopEmailRefreshBtnElement => webDriver.FindElement(By.Id("refreshbut"));
        private IWebElement ConfirmationLinkmailElement => webDriver.FindElement(By.XPath("(//div[@class='m'])[2]"));
        private IWebElement EmailLinkElement => webDriver.FindElement(By.Id("(//table[2]//a)[2]"));

        //------------Finding Email Confirmation Helper Methods-----------//

        public EmailConfirmationPage CloseEmailCofirmationModal()
        {
            webDriver.WaitInSeconds(2);
            EmailConfirmationModalCloseElement.Click();
            return this;
        }

        public EmailConfirmationPage OpenYopEmailAndPopulateEmailInput()
        {
            webDriver.WaitInSeconds(2);
            var emailAddress = ActivityProviderEmailElement.GetAttribute("href");
            var properEmailAddress = emailAddress.Replace("mailto:", "");
            webDriver.FindElement(By.CssSelector("body")).SendKeys(Keys.Control + "t");
            webDriver.WaitInSeconds(1);
            webDriver.Navigate().GoToUrl("https://yopmail.com/en/");
            webDriver.WaitInSeconds(4);
            YopEmailTextInputElement.Clear();
            YopEmailTextInputElement.SendKeys(properEmailAddress);
            return this;
        }

        public EmailConfirmationPage ClickYopEmailTextInputBtn()
        {
            webDriver.WaitInSeconds(1);
            YopEmailRefreshBtnElement.Click();
            return this;
        }

        public EmailConfirmationPage ClickConfirmationLinkmail()
        {
            webDriver.WaitInSeconds(2);
            ConfirmationLinkmailElement.Click();
            return this;
        }

        public EmailConfirmationPage ClickEmailConfirmationLink()
        {
            webDriver.WaitInSeconds(2);
            EmailLinkElement.Click();
            return this;
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
