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



        //------------Finding Email Confirmation Helper Methods-----------//

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
