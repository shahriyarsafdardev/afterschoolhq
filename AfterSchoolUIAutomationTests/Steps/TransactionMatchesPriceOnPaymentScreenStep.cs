using AfterSchoolUIAutomationTests.Drivers;
using AfterSchoolUIAutomationTests.Helpers.Extensions;
using AfterSchoolUIAutomationTests.Pages;
using AfterSchoolUIAutomationTests.Parameters;
using FluentAssertions;
using OpenQA.Selenium.Chrome;
using System;
using TechTalk.SpecFlow;

namespace AfterSchoolUIAutomationTests.Steps
{
    [Binding]
    public class TransactionMatchesPriceOnPaymentScreenStep
    {
        private readonly ScenarioContext _scenarioContext;
        private readonly NewParentAccountActivityRegistrationPage newParentAccountActivityRegistrationPage;
        private readonly TransactionMatchesPriceOnPaymentScreenPage transactionMatchesPriceOnPaymentScreenPage;
        private readonly NewParentAccountActivityPaymentPage newParentAccountActivityPaymentPage;

        private double activityPrice;
        public double ActivityPrice
        {
            get { return activityPrice; }
            set { activityPrice = value; }
        }

        public TransactionMatchesPriceOnPaymentScreenStep(ScenarioContext scenarioContext)
        {
            _scenarioContext = scenarioContext;
            newParentAccountActivityRegistrationPage = new NewParentAccountActivityRegistrationPage(UIDriver.WebDriver);
            transactionMatchesPriceOnPaymentScreenPage = new TransactionMatchesPriceOnPaymentScreenPage(UIDriver.WebDriver);
            newParentAccountActivityPaymentPage = new NewParentAccountActivityPaymentPage(UIDriver.WebDriver);
            var options = new ChromeOptions();
            options.AddArgument("no-sandbox");
        }

        [Given(@"Navigate to the session publish url, read activity cost and click register now")]
        public void NavigateToActivityUrlAndClickRegister()
        {
            UIDriver.WebDriver.Navigate().GoToUrl(Parameter.Get("ActivityPublishedURL", "urls"));
            UIDriver.WebDriver.WaitInSeconds(10);
            UIDriver.WebDriver.Manage().Window.Maximize();
            UIDriver.WebDriver.WaitInSeconds(2);
            ActivityPrice = transactionMatchesPriceOnPaymentScreenPage.ActivityPrice();
            newParentAccountActivityRegistrationPage.ClickRegisterNow();
        }

        [Given(@"Go through parent login and activity registration steps")]
        public void GoThroughParentLoginAndActivityRegistration()
        {
            newParentAccountActivityRegistrationPage.PopulateEmail("parent" + DateTime.Now.ToString("yyyyMMddHHmmss") + "@yopmail.com");
            newParentAccountActivityRegistrationPage.ClickNextBtn();
            newParentAccountActivityRegistrationPage.PopulateFirstName(Parameter.Get("FirstName", "users", "NewParentAccountActivityRegistration"));
            newParentAccountActivityRegistrationPage.PopulateLastName(Parameter.Get("LastName", "users", "NewParentAccountActivityRegistration"));
            newParentAccountActivityRegistrationPage.PopulatePhone(Parameter.Get("Phone", "users", "NewParentAccountActivityRegistration"));
            newParentAccountActivityRegistrationPage.PopulatePassword1(Parameter.Get("Password1", "users", "NewParentAccountActivityRegistration"));
            newParentAccountActivityRegistrationPage.PopulatePassword2(Parameter.Get("Password2", "users", "NewParentAccountActivityRegistration"));
            newParentAccountActivityRegistrationPage.ClickIAgreeCheck();
            newParentAccountActivityRegistrationPage.ClickParentSignUpNextBtn();

            newParentAccountActivityRegistrationPage.PopulateStreetAddress(Parameter.Get("StreetAddress", "users", "NewParentAccountActivityRegistration"));
            newParentAccountActivityRegistrationPage.PopulateCity(Parameter.Get("City", "users", "NewParentAccountActivityRegistration"));
            newParentAccountActivityRegistrationPage.SelectState(Parameter.Get("State", "users", "NewParentAccountActivityRegistration"));
            newParentAccountActivityRegistrationPage.PopulateZipCode(Parameter.Get("Zipcode", "users", "NewParentAccountActivityRegistration"));
            newParentAccountActivityRegistrationPage.ClickHomeAddressNextBtn();

            newParentAccountActivityRegistrationPage.PopulateGuardianFirstName(Parameter.Get("GuardianFirstName", "users", "NewParentAccountActivityRegistration"));
            newParentAccountActivityRegistrationPage.PopulateGuardianLastName(Parameter.Get("GuardianLastName", "users", "NewParentAccountActivityRegistration"));
            newParentAccountActivityRegistrationPage.PopulateGuardianEmail(Parameter.Get("GuardianEmail", "users", "NewParentAccountActivityRegistration"));
            newParentAccountActivityRegistrationPage.PopulateGuardianMobile(Parameter.Get("GuardianMobile", "users", "NewParentAccountActivityRegistration"));
            newParentAccountActivityRegistrationPage.ClickParentGuardianNextBtn();

            newParentAccountActivityRegistrationPage.PopulateEmergencyContactFirstName(Parameter.Get("EmergencyContactFirstName", "users", "NewParentAccountActivityRegistration"));
            newParentAccountActivityRegistrationPage.PopulateEmergencyContactLastName(Parameter.Get("EmergencyContactLastName", "users", "NewParentAccountActivityRegistration"));
            newParentAccountActivityRegistrationPage.PopulateEmergencyContactPhone(Parameter.Get("EmergencyContactPhone", "users", "NewParentAccountActivityRegistration"));
            newParentAccountActivityRegistrationPage.ClickEmergencyNextBtn();

            newParentAccountActivityRegistrationPage.PopulateStudentFirstName(Parameter.Get("StudentFirstName", "users", "NewParentAccountActivityRegistration"));
            newParentAccountActivityRegistrationPage.PopulateStudentLastName(Parameter.Get("StudentLastName", "users", "NewParentAccountActivityRegistration"));
            newParentAccountActivityRegistrationPage.PopulateStudentPhone(Parameter.Get("StudentPhone", "users", "NewParentAccountActivityRegistration"));
            newParentAccountActivityRegistrationPage.PopulateStudentDOB(Parameter.Get("StudentDOB", "users", "NewParentAccountActivityRegistration"));
            newParentAccountActivityRegistrationPage.SetStudentGrade(Parameter.Get("StudentGrade", "users", "NewParentAccountActivityRegistration"));
            newParentAccountActivityRegistrationPage.SetStudentGender();
            newParentAccountActivityRegistrationPage.SelectAutoCompleteSchool(Parameter.Get("StudentSchool", "users", "NewParentAccountActivityRegistration"));
            newParentAccountActivityRegistrationPage.ClickStudentInfoBtn();

            newParentAccountActivityRegistrationPage.PopulateDoctorName(Parameter.Get("DoctorName", "users", "NewParentAccountActivityRegistration"));
            newParentAccountActivityRegistrationPage.PopulateDoctorPhone(Parameter.Get("DoctorPhone", "users", "NewParentAccountActivityRegistration"));
            newParentAccountActivityRegistrationPage.SetStudentImmunization();
            newParentAccountActivityRegistrationPage.ClickStudentHealthNextBtn();
        }

        [Given(@"Enter Parent E Signature And Continue")]
        public void PopulateESignatureAndContinue()
        {
            newParentAccountActivityPaymentPage.PopulateParentESign(Parameter.Get("ParentESign", "users", "NewParentAccountActivityPayment"));
            newParentAccountActivityPaymentPage.ClickESignNextBtn();
        }

        [Then(@"Verify price on payment page with activity price")]
        public void VerifyCompleteRegistrationMessage()
        {
            activityPrice.Should().Equals(transactionMatchesPriceOnPaymentScreenPage.PriceInPayViaBankButton());
            UIDriver.WebDriver.WaitInSeconds(2);
        }
    }
}