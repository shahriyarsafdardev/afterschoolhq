using AfterSchoolUIAutomationTests.Pages;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TechTalk.SpecFlow;
using AfterSchoolUIAutomationTests.Helpers.Extensions;
using AfterSchoolUIAutomationTests.Parameters;

namespace AfterSchoolUIAutomationTests.Steps
{
    [Binding]
    public class TransactionMatchesPriceOnPaymentScreenStep : IDisposable
    {
        private readonly ScenarioContext _scenarioContext;
        private IWebDriver webDriver;
        private NewParentAccountActivityRegistrationPage newParentAccountActivityRegistrationPage;
        private TransactionMatchesPriceOnPaymentScreenPage transactionMatchesPriceOnPaymentScreenPage;
        private NewParentAccountActivityPaymentPage newParentAccountActivityPaymentPage;

        private string activityPrice;
        public string ActivityPrice
        {
            get { return activityPrice; }
            set { activityPrice = value; }
        }

        public TransactionMatchesPriceOnPaymentScreenStep(ScenarioContext scenarioContext)
        {
            _scenarioContext = scenarioContext;
            webDriver = new ChromeDriver(AppDomain.CurrentDomain.BaseDirectory);
            newParentAccountActivityRegistrationPage = new NewParentAccountActivityRegistrationPage(webDriver);
            transactionMatchesPriceOnPaymentScreenPage = new TransactionMatchesPriceOnPaymentScreenPage(webDriver);
            newParentAccountActivityPaymentPage = new NewParentAccountActivityPaymentPage(webDriver);
            var options = new ChromeOptions();
            options.AddArgument("no-sandbox");
        }

        [Given(@"Navigate to the session publish url, read activity cost and click register now")]
        public void NavigateToActivityUrlAndClickRegister()
        {
            webDriver.Navigate().GoToUrl(Parameter.Get("NewParentAccountActivityRegistration", "ActivityPublishedURL"));
            webDriver.WaitInSeconds(10);
            webDriver.Manage().Window.Maximize();
            webDriver.WaitInSeconds(2);
            ActivityPrice = transactionMatchesPriceOnPaymentScreenPage.ActivityPrice();
            newParentAccountActivityRegistrationPage.ClickRegisterNow();
        }

        [Given(@"Go through parent login and activity registration steps")]
        public void GoThroughParentLoginAndActivityRegistration()
        {
            newParentAccountActivityRegistrationPage.PopulateEmail("parent" + DateTime.Now.ToString("yyyyMMddHHmmss") + "@yopmail.com");
            newParentAccountActivityRegistrationPage.ClickNextBtn();
            newParentAccountActivityRegistrationPage.PopulateFirstName(Parameter.Get("NewParentAccountActivityRegistration", "FirstName"));
            newParentAccountActivityRegistrationPage.PopulateLastName(Parameter.Get("NewParentAccountActivityRegistration", "LastName"));
            newParentAccountActivityRegistrationPage.PopulatePhone(Parameter.Get("NewParentAccountActivityRegistration", "Phone"));
            newParentAccountActivityRegistrationPage.PopulatePassword1(Parameter.Get("NewParentAccountActivityRegistration", "Password1"));
            newParentAccountActivityRegistrationPage.PopulatePassword2(Parameter.Get("NewParentAccountActivityRegistration", "Password2"));
            newParentAccountActivityRegistrationPage.ClickIAgreeCheck();
            newParentAccountActivityRegistrationPage.ClickParentSignUpNextBtn();

            newParentAccountActivityRegistrationPage.PopulateStreetAddress(Parameter.Get("NewParentAccountActivityRegistration", "StreetAddress"));
            newParentAccountActivityRegistrationPage.PopulateCity(Parameter.Get("NewParentAccountActivityRegistration", "City"));
            newParentAccountActivityRegistrationPage.SelectState(Parameter.Get("NewParentAccountActivityRegistration", "State"));
            newParentAccountActivityRegistrationPage.PopulateZipCode(Parameter.Get("NewParentAccountActivityRegistration", "Zipcode"));
            newParentAccountActivityRegistrationPage.ClickHomeAddressNextBtn();

            newParentAccountActivityRegistrationPage.PopulateGuardianFirstName(Parameter.Get("NewParentAccountActivityRegistration", "GuardianFirstName"));
            newParentAccountActivityRegistrationPage.PopulateGuardianLastName(Parameter.Get("NewParentAccountActivityRegistration", "GuardianLastName"));
            newParentAccountActivityRegistrationPage.PopulateGuardianEmail(Parameter.Get("NewParentAccountActivityRegistration", "GuardianEmail"));
            newParentAccountActivityRegistrationPage.PopulateGuardianMobile(Parameter.Get("NewParentAccountActivityRegistration", "GuardianMobile"));
            newParentAccountActivityRegistrationPage.ClickParentGuardianNextBtn();

            newParentAccountActivityRegistrationPage.PopulateEmergencyContactFirstName(Parameter.Get("NewParentAccountActivityRegistration", "EmergencyContactFirstName"));
            newParentAccountActivityRegistrationPage.PopulateEmergencyContactLastName(Parameter.Get("NewParentAccountActivityRegistration", "EmergencyContactLastName"));
            newParentAccountActivityRegistrationPage.PopulateEmergencyContactPhone(Parameter.Get("NewParentAccountActivityRegistration", "EmergencyContactPhone"));
            newParentAccountActivityRegistrationPage.ClickEmergencyNextBtn();

            newParentAccountActivityRegistrationPage.PopulateStudentFirstName(Parameter.Get("NewParentAccountActivityRegistration", "StudentFirstName"));
            newParentAccountActivityRegistrationPage.PopulateStudentLastName(Parameter.Get("NewParentAccountActivityRegistration", "StudentLastName"));
            newParentAccountActivityRegistrationPage.PopulateStudentPhone(Parameter.Get("NewParentAccountActivityRegistration", "StudentPhone"));
            newParentAccountActivityRegistrationPage.PopulateStudentDOB(Parameter.Get("NewParentAccountActivityRegistration", "StudentDOB"));
            newParentAccountActivityRegistrationPage.SetStudentGrade(Parameter.Get("NewParentAccountActivityRegistration", "StudentGrade"));
            newParentAccountActivityRegistrationPage.SetStudentGender();
            newParentAccountActivityRegistrationPage.SelectAutoCompleteSchool(Parameter.Get("NewParentAccountActivityRegistration", "StudentSchool"));
            newParentAccountActivityRegistrationPage.ClickStudentInfoBtn();

            newParentAccountActivityRegistrationPage.PopulateDoctorName(Parameter.Get("NewParentAccountActivityRegistration", "DoctorName"));
            newParentAccountActivityRegistrationPage.PopulateDoctorPhone(Parameter.Get("NewParentAccountActivityRegistration", "DoctorPhone"));
            newParentAccountActivityRegistrationPage.SetStudentImmunization();
            newParentAccountActivityRegistrationPage.ClickStudentHealthNextBtn();
        }

        [Given(@"Enter Parent E Signature And Continue")]
        public void PopulateESignatureAndContinue()
        {
            newParentAccountActivityPaymentPage.PopulateParentESign(Parameter.Get("NewParentAccountActivityPayment", "ParentESign"));
            newParentAccountActivityPaymentPage.ClickESignNextBtn();
        }

        [Then(@"Verify price on payment page with activity price")]
        public void VerifyCompleteRegistrationMessage()
        {
            activityPrice.Equals(transactionMatchesPriceOnPaymentScreenPage.PriceInPayViaBankButton().Trim());
            webDriver.WaitInSeconds(2);
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
