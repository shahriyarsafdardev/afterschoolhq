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
        private IWebElement PayViaBankElement => webDriver.FindElement(By.XPath("//span[@class='total-due-today-ach']"));


        public double ActivityPrice()
        {
            webDriver.WaitInSeconds(1);
            double activityFee = 0;
            string numberStr = string.Format("{0:C2}", ActivityPriceElement.Text.Trim());
            activityFee = Double.Parse(numberStr.Remove(0, 1)); // Remove the "$" from the number
            return activityFee;
        }

        public double PriceInPayViaBankButton()
        {
            webDriver.WaitInSeconds(1);
            double transactionFee = 0;
            string numberStr = string.Format("{0:C2}", PayViaBankElement.Text.Trim());
            transactionFee = Double.Parse(numberStr);
            return transactionFee;
        }
    }
}