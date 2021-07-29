using System;
using System.Threading;
using AfterSchoolUIAutomationTests.Helpers.Extensions;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace AfterSchoolUIAutomationTests.Pages
{
    public class AcceptPaymentsPage : IDisposable
    {
        private IWebDriver webDriver;
        public AcceptPaymentsPage(IWebDriver WebDriver)
        {
            webDriver = WebDriver;
        }

        //------------Finding Accept Payments WebElements-----------//
        private IWebElement ContinueToYourAccountElement => webDriver.FindElement(By.XPath("//p[@class='text-white']//a"));
        private IWebElement CloseModalBtnElement => webDriver.FindElement(By.XPath("(//div[@class='close-icon'])[1]//a[@class='close close-icon-postion-2']"));
        private IWebElement GotItButtonElement => webDriver.FindElement(By.XPath("//button[@class='btn btn-success btn-xs']"));
        private IWebElement FinishSetupElement => webDriver.FindElement(By.Id("popover-container"));
        private IWebElement MoreDropdownElement => webDriver.FindElement(By.XPath("(//button[@class='btn btn-header-action dropdown-toggle'])[2]"));
        private IWebElement EnableBtnElement => webDriver.FindElement(By.XPath("(//div[@class='dropdown-menu pointer'])[2]//a"));
        private IWebElement ChooseDirectDepositElement => webDriver.FindElement(By.XPath("//table[@class='table table-bordered']//a"));

        //----------------Get Paid by Afterschool HQ Elements---------------------//
        private IWebElement IndividualSoleProprietorElement => webDriver.FindElement(By.XPath("(//div[@class='Box-root Padding-left--8'])[1]//label[@Class='PressableControlLabel-label']//span"));
        private IWebElement TestPhoneNumberElement => webDriver.FindElement(By.XPath("(//div[@class='Box-root Flex-flex Flex-alignItems--baseline Flex-direction--row Flex-justifyContent--flexStart'])[4]//span[@class='ButtonLink-label Text-color--blue Text-fontSize--14 Text-fontWeight--medium Text-lineHeight--20 Text-numericSpacing--proportional Text-typeface--base Text-wrap--noWrap Text-display--block']"));
        private IWebElement NextBtnGetPaidElement => webDriver.FindElement(By.XPath("//div[@class='Box-root Padding-left--8']//div[@class='SVGInline SVGInline--cleaned SVG Icon Icon--arrowRight Button-icon Icon-color Icon-color--white Box-root Flex-flex']"));
        private IWebElement UseTestCaseElement => webDriver.FindElement(By.XPath("//div[@class='Box-root Flex-flex Flex-alignItems--baseline Flex-direction--row Flex-justifyContent--center']//span[@class='Button-label Text-color--default Text-fontSize--14 Text-fontWeight--medium Text-lineHeight--20 Text-numericSpacing--proportional Text-typeface--base Text-wrap--noWrap Text-display--block']"));

        //----------------Personal Details Elements---------------------//
        private IWebElement FirstNameElement => webDriver.FindElement(By.Id("first_name"));
        private IWebElement LastNameElement => webDriver.FindElement(By.Id("last_name"));
        private IWebElement DayElement => webDriver.FindElement(By.XPath("(//div[@class='Box-root Flex-flex'])[4]//div[@class='Box-root Flex-flex']//input"));
        private IWebElement MonthElement => webDriver.FindElement(By.XPath("(//div[@class='Box-root Flex-flex'])[6]//div[@class='Box-root Flex-flex']//input"));
        private IWebElement YearElement => webDriver.FindElement(By.XPath("(//div[@class='Box-root Flex-flex'])[8]//div[@class='Box-root Flex-flex']//input"));
        private IWebElement Address1Element => webDriver.FindElement(By.XPath("(//div[@class='PressableCore-base Box-root'])[9]//input"));
        private IWebElement Address2Element => webDriver.FindElement(By.XPath("(//div[@class='PressableCore-base Box-root'])[10]//input"));
        private IWebElement CityElement => webDriver.FindElement(By.XPath("(//div[@class='PressableCore-base Box-root'])[11]//input"));
        private IWebElement StateSelectElement => webDriver.FindElement(By.XPath("(//div[@class='PressableCore-base Box-root'])[12]//select"));
        private IWebElement ZIPElement => webDriver.FindElement(By.XPath("(//div[@class='PressableCore-base Box-root'])[13]//input"));
        private IWebElement PhoneElement => webDriver.FindElement(By.XPath("(//div[@class='PressableCore-base Box-root'])[14]//input"));
        private IWebElement SSNElement => webDriver.FindElement(By.XPath("(//div[@class='PressableCore-base Box-root'])[16]//input"));
        private IWebElement NextBtnPersonalDetailElement => webDriver.FindElement(By.XPath("//div[@class='Box-root Flex-flex Flex-alignItems--baseline Flex-direction--rowReversed Flex-justifyContent--center']//span[@class='Button-label Text-color--white Text-fontSize--16 Text-fontWeight--medium Text-lineHeight--28 Text-numericSpacing--proportional Text-typeface--base Text-wrap--noWrap Text-display--block']"));

        //----------------Bussiness Details Elements---------------------//
        private IWebElement BussinessWebsiteElement => webDriver.FindElement(By.XPath("(//div[@class='PressableCore-base Box-root'])[1]//input"));
        private IWebElement AddProductDescriptionInsteadElement => webDriver.FindElement(By.XPath("(//div[@class='Box-root Box-hideIfEmpty'])[2]//span[2]"));
        private IWebElement ProductDescriptionTextareaElement => webDriver.FindElement(By.XPath("//div[@class='PressableCore-base Box-root']//textarea"));
        private IWebElement NextBtnBussinessDetailElement => webDriver.FindElement(By.XPath("//div[@class='Button-align Box-root Flex-flex Flex-alignItems--baseline Flex-direction--rowReversed']//span"));

        //----------------Select Account For Payouts Elements---------------------//
        private IWebElement UsetestAccountBtnElement => webDriver.FindElement(By.XPath("(//div[@class='Box-root Flex-flex Flex-alignItems--baseline Flex-direction--row Flex-justifyContent--center'])[1]//span[1]"));

        //----------------Review Your Details Elements---------------------//
        private IWebElement ReviewYourDetailsElement => webDriver.FindElement(By.XPath("//div[@class='Box-root Flex-flex Flex-alignItems--baseline Flex-direction--row Flex-justifyContent--center']"));
        private IWebElement AccountSetupCompleteElement => webDriver.FindElement(By.Id("modal-email-confirmation-text"));


        //------------Finding Accept Payments Helper Methods-----------//
        #region AcceptPaymentsHelperMethods

        public AcceptPaymentsPage NavigateToFinishSetup(string url)
        {
            webDriver.WaitInSeconds(2);
            webDriver.Navigate().GoToUrl(url);
            return this;
        }

        public AcceptPaymentsPage ClickCloseModal()
        {
            webDriver.WaitInSeconds(2);
            CloseModalBtnElement.Click();
            return this;
        }
        public AcceptPaymentsPage ClickGotItButton()
        {
            GotItButtonElement.Click();
            webDriver.WaitInSeconds(2);
            return this;
        }
        public AcceptPaymentsPage ClickMoreDropdownButton()
        {
            webDriver.WaitInSeconds(6);
            MoreDropdownElement.Click();
            return this;
        }

        public AcceptPaymentsPage ClickEnablePaymentsButton()
        {
            webDriver.WaitInSeconds(2);
            EnableBtnElement.Click();
            return this;
        }

        public AcceptPaymentsPage ClickChooseDirectDepositButton()
        {
            webDriver.WaitInSeconds(4);
            ChooseDirectDepositElement.Click();
            return this;
        }

        public AcceptPaymentsPage ClickIndividualSoleProprietorRadio()
        {
            webDriver.WaitInSeconds(8);
            IndividualSoleProprietorElement.Click();
            return this;
        }

        public AcceptPaymentsPage ClickTestPhoneNumber()
        {
            webDriver.WaitInSeconds(2);
            TestPhoneNumberElement.Click();
            return this;
        }

        public AcceptPaymentsPage ClickNextBtnGetPaid()
        {
            webDriver.WaitInSeconds(2);
            NextBtnGetPaidElement.Click();
            return this;
        }

        public AcceptPaymentsPage ClickUseTestCode()
        {
            webDriver.WaitInSeconds(4);
            UseTestCaseElement.Click();
            return this;
        }

        public AcceptPaymentsPage PopulateFirstName(string FirstName)
        {
            webDriver.WaitInSeconds(14);
            FirstNameElement.SendKeys(FirstName);
            return this;
        }

        public AcceptPaymentsPage PopulateLastName(string LastName)
        {
            webDriver.WaitInSeconds(1);
            LastNameElement.SendKeys(LastName);
            return this;
        }

        public AcceptPaymentsPage PopulateDay(string Day)
        {
            webDriver.WaitInSeconds(1);
            DayElement.SendKeys(Day);
            return this;
        }

        public AcceptPaymentsPage PopulateMonth(string Month)
        {
            webDriver.WaitInSeconds(1);
            MonthElement.SendKeys(Month);
            return this;
        }

        public AcceptPaymentsPage PopulateYear(string Year)
        {
            webDriver.WaitInSeconds(1);
            YearElement.SendKeys(Year);
            return this;
        }

        public AcceptPaymentsPage PopulateAddressLine1(string addressLine1)
        {
            webDriver.WaitInSeconds(1);
            Address1Element.SendKeys(addressLine1);
            return this;
        }

        public AcceptPaymentsPage PopulateAddressLine2(string addressLine2)
        {
            webDriver.WaitInSeconds(1);
            Address2Element.SendKeys(addressLine2);
            return this;
        }

        public AcceptPaymentsPage PopulateCity(string city)
        {
            webDriver.WaitInSeconds(1);
            CityElement.SendKeys(city);
            return this;
        }

        public AcceptPaymentsPage PopulateState(string americanState)
        {
            webDriver.WaitInSeconds(1);
            var state = StateSelectElement;
            var selectElement = new SelectElement(StateSelectElement);
            selectElement.SelectByValue(americanState);
            return this;
        }

        public AcceptPaymentsPage PopulateZip(string zip)
        {
            webDriver.WaitInSeconds(1);
            ZIPElement.SendKeys(zip);
            return this;
        }

        public AcceptPaymentsPage PopulatePhoneNumber(string phone)
        {
            webDriver.WaitInSeconds(1);
            PhoneElement.Clear();
            PhoneElement.SendKeys(phone);
            return this;
        }

        public AcceptPaymentsPage PopulateSSN(string ssn)
        {
            webDriver.WaitInSeconds(1);
            SSNElement.SendKeys(ssn);
            return this;
        }

        public AcceptPaymentsPage ClickNextBtnPersonalDetail()
        {
            webDriver.WaitInSeconds(2);
            NextBtnPersonalDetailElement.Click();
            return this;
        }

        #endregion

        public AcceptPaymentsPage ClickBussinessWebsiteDetails()
        {
            webDriver.WaitInSeconds(6);
            string weburl = NextBtnPersonalDetailElement.Text.Trim();
            int pos = weburl.LastIndexOf("/") + 1;
            string activityName = weburl.Substring(pos, weburl.Length - pos);
            NextBtnPersonalDetailElement.Click();
            webDriver.WaitInSeconds(1);
            AddProductDescriptionInsteadElement.Click();
            webDriver.WaitInSeconds(2);
            ProductDescriptionTextareaElement.SendKeys(activityName);
            return this;
        }
        public AcceptPaymentsPage ClickNextBtnBussinessDetail()
        {
            webDriver.WaitInSeconds(2);
            NextBtnPersonalDetailElement.Click();
            return this;
        }

        //-----Select Account For Payouts
        public AcceptPaymentsPage ClickUsetestAccount()
        {
            webDriver.WaitInSeconds(2);
            UsetestAccountBtnElement.Click();
            return this;
        }

        public AcceptPaymentsPage ClickReviewYourDetails()
        {
            webDriver.WaitInSeconds(8);
            ReviewYourDetailsElement.Click();
            return this;
        }

        public string AccountSetupComplete()
        {
            webDriver.WaitInSeconds(12);
            return AccountSetupCompleteElement.Text;
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
