using AfterSchoolUIAutomationTests.Helpers.Extensions;
using OpenQA.Selenium;
using System;

namespace AfterSchoolUIAutomationTests.Pages
{
    public class TransactionMatchesPriceOnPaymentScreenPage
    {
        private readonly IWebDriver webDriver;
        public TransactionMatchesPriceOnPaymentScreenPage(IWebDriver WebDriver)
        {
            webDriver = WebDriver;
        }

        //------------Finding NewParentAccountActivityRegistrationPage WebElements-----------//

        private IWebElement ActivityPriceElement => webDriver.FindElement(By.XPath("(((//div[@class='col-lg-4 col-md-12'])[2]//div[@class='activity-detail-block'])[2]//strong)[1]"));
        private IWebElement PayViaBankElement => webDriver.FindElement(By.XPath("//span[@class='total-due-today-cc']"));


        public string ActivityPrice()
        {
            webDriver.WaitInSeconds(1);
            return ActivityPriceElement.Text.Trim();
        }

        public int PriceInPayViaBankButton()
        {
            webDriver.WaitInSeconds(1);
            return Convert.ToInt32(PayViaBankElement.Text.Trim());
        }
    }
}