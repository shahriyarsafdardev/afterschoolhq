using System;
using System.Threading;
using AfterSchoolUIAutomationTests.Helpers.Extensions;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace AfterSchoolUIAutomationTests.Pages
{
    public class TransactionMatchesPriceOnPaymentScreenPage : IDisposable
    {
        private IWebDriver webDriver;
        public TransactionMatchesPriceOnPaymentScreenPage(IWebDriver WebDriver)
        {
            webDriver = WebDriver;
        }

        //------------Finding NewParentAccountActivityRegistrationPage WebElements-----------//

        private IWebElement ActivityPriceElement => webDriver.FindElement(By.XPath("(((//div[@class='col-lg-4 col-md-12'])[2]//div[@class='activity-detail-block'])[2]//strong)[1]"));
        private IWebElement PayViaBankElement => webDriver.FindElement(By.XPath("//button[@id='plaid-button']"));


        public string ActivityPrice()
        {
            webDriver.WaitInSeconds(1);
            return ActivityPriceElement.Text.Trim();
        }

        public string PriceInPayViaBankButton()
        {
            webDriver.WaitInSeconds(1);
            return PayViaBankElement.Text.Trim();
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
