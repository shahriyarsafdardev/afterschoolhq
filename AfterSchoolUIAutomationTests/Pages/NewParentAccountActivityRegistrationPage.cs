using System;
using System.Threading;
using AfterSchoolUIAutomationTests.Helpers.Extensions;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace AfterSchoolUIAutomationTests.Pages
{
    class NewParentAccountActivityRegistrationPage : IDisposable
    {
        private IWebDriver webDriver;
        public NewParentAccountActivityRegistrationPage(IWebDriver WebDriver)
        {
            webDriver = WebDriver;
        }

        //------------Finding NewParentAccountActivityRegistrationPage WebElements-----------//

        #region Step 1 Parent Signup WebElements
        private IWebElement RegisterNowElement => webDriver.FindElement(By.XPath("//div[@class='col-lg-4 col-md-12']//a[@class=' btn btn-register btn-block']"));
        private IWebElement ParentEmailElement => webDriver.FindElement(By.Id("email-check"));
        private IWebElement EmailClickNextElement => webDriver.FindElement(By.Id("btn-check-email"));
        private IWebElement FirstNnameElement => webDriver.FindElement(By.Id("question-9487"));
        private IWebElement LastNameElement => webDriver.FindElement(By.Id("question-9488"));
        private IWebElement PhoneElement => webDriver.FindElement(By.Id("question-9490"));
        private IWebElement Password1Element => webDriver.FindElement(By.Id("password1"));
        private IWebElement Password2Element => webDriver.FindElement(By.Id("password2"));
        private IWebElement IAgreeElement => webDriver.FindElement(By.XPath("//input[@id='agree']"));
        private IWebElement SaveAndContParentSignUpElement => webDriver.FindElement(By.XPath("//button[@id='btn-create-account']"));
        #endregion

        #region Step 2 Home Address WebElements
        private IWebElement StreetAddressElement => webDriver.FindElement(By.Id("question-9499"));
        private IWebElement CityElement => webDriver.FindElement(By.Id("question-9501"));
        private IWebElement StateSelectElement => webDriver.FindElement(By.Id("question-9502"));
        private IWebElement ZipCodeElement => webDriver.FindElement(By.Id("question-9503"));
        private IWebElement SaveAndContHomeAddressElement => webDriver.FindElement(By.XPath("//button[@id='btn-add-home-address']"));
        #endregion

        #region Step 3 Additional Parent, Guardian WebElements
        private IWebElement GuardianFirstNameElement => webDriver.FindElement(By.Id("question-9487"));
        private IWebElement GuardianLastNameElement => webDriver.FindElement(By.Id("question-9488"));
        private IWebElement GuardianEmailElement => webDriver.FindElement(By.Id("question-9489"));
        private IWebElement GuardianMobileElement => webDriver.FindElement(By.Id("question-9490"));
        #endregion

        #region Step 4 Emergency WebElements
        private IWebElement EmergencyContactFirstName => webDriver.FindElement(By.Id("question-9487"));
        private IWebElement EmergencyContactLastName => webDriver.FindElement(By.Id("question-9488"));
        private IWebElement EmergencyContactPhone => webDriver.FindElement(By.Id("question-9489"));
        #endregion

        #region Step 5 Student Information
        private IWebElement StudentFirstName => webDriver.FindElement(By.Id("question-9487"));
        private IWebElement StudentLastName => webDriver.FindElement(By.Id("question-9488"));
        private IWebElement StudentPhone => webDriver.FindElement(By.Id("question-9490"));
        private IWebElement StudentDOB => webDriver.FindElement(By.Id("question-9494"));
        private IWebElement StudentGrade => webDriver.FindElement(By.Id("question-9494"));
        private IWebElement StudentGender => webDriver.FindElement(By.Id("question-9495-male"));
        private IWebElement StudentSchool => webDriver.FindElement(By.Id("location"));
        #endregion






        //------------Finding NewParentAccountActivityRegistrationPage Helper Methods-----------//

        #region Step1 Parent Signup Helper Methods
        public NewParentAccountActivityRegistrationPage ClickRegisterNow()
        {
            RegisterNowElement.Click();
            webDriver.WaitInSeconds(3);
            return this;
        }
        public NewParentAccountActivityRegistrationPage PopulateEmail(string email)
        {
            ParentEmailElement.SendKeys(email);
            webDriver.WaitInSeconds(2);
            return this;
        }
        public NewParentAccountActivityRegistrationPage ClickNextBtn()
        {
            EmailClickNextElement.Click();
            webDriver.WaitInSeconds(2);
            return this;
        }
        public NewParentAccountActivityRegistrationPage PopulateFirstName(string firstname)
        {
            FirstNnameElement.SendKeys(firstname);
            webDriver.WaitInSeconds(1);
            return this;
        }
        public NewParentAccountActivityRegistrationPage PopulateLastName(string lastname)
        {
            LastNameElement.SendKeys(lastname);
            webDriver.WaitInSeconds(1);
            return this;
        }
        public NewParentAccountActivityRegistrationPage PopulatePhone(string phone)
        {
            PhoneElement.SendKeys(WebDriverExtension.GetRandomTelNo());
            webDriver.WaitInSeconds(1);
            return this;
        }
        public NewParentAccountActivityRegistrationPage PopulatePassword1(string password1)
        {
            Password1Element.SendKeys(password1);
            webDriver.WaitInSeconds(1);
            return this;
        }
        public NewParentAccountActivityRegistrationPage PopulatePassword2(string password2)
        {
            Password2Element.SendKeys(password2);
            webDriver.WaitInSeconds(2);
            return this;
        }
        public NewParentAccountActivityRegistrationPage ClickIAgreeCheck()
        {
            //IAgreeElement.Click();
            webDriver.WaitInSeconds(4);
            return this;
        }
        public NewParentAccountActivityRegistrationPage ClickParentSignUpNextBtn()
        {
            SaveAndContParentSignUpElement.Click();
            webDriver.WaitInSeconds(4);
            return this;
        }
        #endregion

        #region Step2 Home Address Helper Methods
        public NewParentAccountActivityRegistrationPage PopulateStreetAddress(string streetAddress)
        {
            StreetAddressElement.SendKeys(streetAddress);
            webDriver.WaitInSeconds(1);
            return this;
        }
        public NewParentAccountActivityRegistrationPage PopulateCity(string city)
        {
            CityElement.SendKeys(city);
            webDriver.WaitInSeconds(1);
            return this;
        }
        public NewParentAccountActivityRegistrationPage SelectState(string state)
        {
            StateSelectElement.Click();
            webDriver.WaitInSeconds(2);
            var selectElement = new SelectElement(StateSelectElement);
            selectElement.SelectByValue(state);
            return this;
        }
        public NewParentAccountActivityRegistrationPage PopulateZipCode(string zipcode)
        {
            ZipCodeElement.SendKeys(zipcode);
            webDriver.WaitInSeconds(1);
            return this;
        }
        public NewParentAccountActivityRegistrationPage ClickHomeAddressNextBtn()
        {
            SaveAndContHomeAddressElement.Click();
            webDriver.WaitInSeconds(4);
            return this;
        }

        #endregion








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
