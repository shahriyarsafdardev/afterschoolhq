using AfterSchoolUIAutomationTests.Helpers.Extensions;
using OpenQA.Selenium;

namespace AfterSchoolUIAutomationTests.Pages
{
    public class NewParentAccountActivityPaymentPage
    {
        private readonly IWebDriver webDriver;
        public NewParentAccountActivityPaymentPage(IWebDriver WebDriver)
        {
            webDriver = WebDriver;
        }

        //------------Finding NewParentAccountActivityPaymentPage WebElements-----------//
        private IWebElement ParentESignElement => webDriver.FindElement(By.Id("signature"));
        private IWebElement EsignContinueBtnElement => webDriver.FindElement(By.Id("btn-register"));
        private IWebElement CreditCardElement => webDriver.FindElement(By.XPath("//div[@class='CardNumberField-input-wrapper']//span[@class='InputContainer']//input"));
        private IWebElement CreditCardMonthYearElement => webDriver.FindElement(By.XPath("//div[@class='CardNumberField-input-wrapper']//span[@class='InputContainer']"));
        private IWebElement CreditCardCVCElement => webDriver.FindElement(By.XPath("//span[@class='CardField-cvc CardField-child']//input"));
        private IWebElement CreditCardZIPElement => webDriver.FindElement(By.XPath("//span[@class='CardField-postalCode CardField-child']//input"));
        private IWebElement PayViaCreditBtnElement => webDriver.FindElement(By.Id("btn-paywithnewcard"));
        private IWebElement CompleteRegistrationElement => webDriver.FindElement(By.XPath("//div[@class='panel-body']//h1"));


        #region Parennt E-Sign

        public NewParentAccountActivityPaymentPage PopulateParentESign(string esign)
        {
            ParentESignElement.SendKeys(esign);
            webDriver.WaitInSeconds(1);
            return this;
        }

        public NewParentAccountActivityPaymentPage ClickESignNextBtn()
        {
            EsignContinueBtnElement.Click();
            webDriver.WaitInSeconds(4);
            return this;
        }

        #endregion

        #region Stripe Payment

        public NewParentAccountActivityPaymentPage PopulateCreditCardNumber(string cardNumber)
        {
            CreditCardElement.SendKeys(cardNumber);
            webDriver.WaitInSeconds(1);
            return this;
        }

        public NewParentAccountActivityPaymentPage PopulateCreditCardExpiry(string expireDate)
        {
            CreditCardMonthYearElement.SendKeys(expireDate);
            webDriver.WaitInSeconds(1);
            return this;
        }

        public NewParentAccountActivityPaymentPage PopulateCreditCardCVC(string cvc)
        {
            CreditCardCVCElement.SendKeys(cvc);
            webDriver.WaitInSeconds(1);
            return this;
        }

        public NewParentAccountActivityPaymentPage PopulateCreditCardZIP(string zip)
        {
            CreditCardZIPElement.SendKeys(zip);
            webDriver.WaitInSeconds(1);
            return this;
        }

        public NewParentAccountActivityPaymentPage ClickPayViaCreditBtn()
        {
            PayViaCreditBtnElement.Click();
            webDriver.WaitInSeconds(14);
            return this;
        }

        public string RegistrationComplete()
        {
            webDriver.WaitInSeconds(1);
            return CompleteRegistrationElement.Text;
        }

        #endregion
    }
}