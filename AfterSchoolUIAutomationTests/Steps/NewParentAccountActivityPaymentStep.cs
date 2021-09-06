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
    public sealed class NewParentAccountActivityPaymentStep : IDisposable
    {
        private readonly ScenarioContext _scenarioContext;
        private IWebDriver webDriver;
        private NewParentAccountActivityRegistrationPage newParentAccountActivityRegistrationPage;
        private NewParentAccountActivityPaymentPage newParentAccountActivityPaymentPage;

        public NewParentAccountActivityPaymentStep(ScenarioContext scenarioContext)
        {
            _scenarioContext = scenarioContext;
            webDriver = new ChromeDriver(AppDomain.CurrentDomain.BaseDirectory);
            newParentAccountActivityRegistrationPage = new NewParentAccountActivityRegistrationPage(webDriver);
            newParentAccountActivityPaymentPage = new NewParentAccountActivityPaymentPage(webDriver);
            var options = new ChromeOptions();
            options.AddArgument("no-sandbox");
        }

        [Given(@"Navigate to the session publish url for registration and click register now")]
        public void NavigateToActivityUrlAndClickRegister()
        {
            webDriver.Navigate().GoToUrl(Parameter.Get("NewParentAccountActivityPayment", "ActivityPublishedURL"));
            webDriver.WaitInSeconds(10);
            webDriver.Manage().Window.Maximize();
            webDriver.WaitInSeconds(2);
            newParentAccountActivityRegistrationPage.ClickRegisterNow();
        }

        [Given(@"Enter parent email address for registration and click next")]
        public void PopulateEmailAndClickNext()
        {
            newParentAccountActivityRegistrationPage.PopulateEmail("parent" + DateTime.Now.ToString("yyyyMMddHHmmss").Substring(8) + "@yopmail.com");
            newParentAccountActivityRegistrationPage.ClickNextBtn();
        }

        [Given(@"Populate fields in step 1 parent signup form details for registration and click save & continue")]
        public void PopulateParentSignupFormAndClickNext()
        {
            newParentAccountActivityRegistrationPage.PopulateFirstName(Parameter.Get("NewParentAccountActivityRegistration", "FirstName"));
            newParentAccountActivityRegistrationPage.PopulateLastName(Parameter.Get("NewParentAccountActivityRegistration", "LastName"));
            newParentAccountActivityRegistrationPage.PopulatePhone(Parameter.Get("NewParentAccountActivityRegistration", "Phone"));
            newParentAccountActivityRegistrationPage.PopulatePassword1(Parameter.Get("NewParentAccountActivityRegistration", "Password1"));
            newParentAccountActivityRegistrationPage.PopulatePassword2(Parameter.Get("NewParentAccountActivityRegistration", "Password2"));
            newParentAccountActivityRegistrationPage.ClickIAgreeCheck();
            newParentAccountActivityRegistrationPage.ClickParentSignUpNextBtn();
        }

        [Given(@"Populate fields in step 2 home address form details for registration and click save & continue")]
        public void PopulateHomeAddressFormAndClickNext()
        {
            newParentAccountActivityRegistrationPage.PopulateStreetAddress(Parameter.Get("NewParentAccountActivityRegistration", "StreetAddress"));
            newParentAccountActivityRegistrationPage.PopulateCity(Parameter.Get("NewParentAccountActivityRegistration", "City"));
            newParentAccountActivityRegistrationPage.SelectState(Parameter.Get("NewParentAccountActivityRegistration", "State"));
            newParentAccountActivityRegistrationPage.PopulateZipCode(Parameter.Get("NewParentAccountActivityRegistration", "Zipcode"));
            newParentAccountActivityRegistrationPage.ClickHomeAddressNextBtn();
        }

        [Given(@"Populate fields in step 3 additional parent, guardian form details for registration and click save & continue")]
        public void PopulateAdditionalParentGuardianFormAndClickNext()
        {
            newParentAccountActivityRegistrationPage.PopulateGuardianFirstName(Parameter.Get("NewParentAccountActivityRegistration", "GuardianFirstName"));
            newParentAccountActivityRegistrationPage.PopulateGuardianLastName(Parameter.Get("NewParentAccountActivityRegistration", "GuardianLastName"));
            newParentAccountActivityRegistrationPage.PopulateGuardianEmail(Parameter.Get("NewParentAccountActivityRegistration", "GuardianEmail"));
            newParentAccountActivityRegistrationPage.PopulateGuardianMobile(Parameter.Get("NewParentAccountActivityRegistration", "GuardianMobile"));
            newParentAccountActivityRegistrationPage.ClickParentGuardianNextBtn();
        }

        [Given(@"Populate fields in step 4 emergency contacts form details for registration and click save & continue")]
        public void PopulateEmergencyContactsFormAndClickNext()
        {
            newParentAccountActivityRegistrationPage.PopulateEmergencyContactFirstName(Parameter.Get("NewParentAccountActivityRegistration", "EmergencyContactFirstName"));
            newParentAccountActivityRegistrationPage.PopulateEmergencyContactLastName(Parameter.Get("NewParentAccountActivityRegistration", "EmergencyContactLastName"));
            newParentAccountActivityRegistrationPage.PopulateEmergencyContactPhone(Parameter.Get("NewParentAccountActivityRegistration", "EmergencyContactPhone"));
            newParentAccountActivityRegistrationPage.ClickEmergencyNextBtn();
        }

        [Given(@"Populate fields in step 5 student information form details for registration and click save & continue")]
        public void PopulateStudentInformationFormAndClickNext()
        {
            newParentAccountActivityRegistrationPage.PopulateStudentFirstName(Parameter.Get("NewParentAccountActivityRegistration", "StudentFirstName"));
            newParentAccountActivityRegistrationPage.PopulateStudentLastName(Parameter.Get("NewParentAccountActivityRegistration", "StudentLastName"));
            newParentAccountActivityRegistrationPage.PopulateStudentPhone(Parameter.Get("NewParentAccountActivityRegistration", "StudentPhone"));
            newParentAccountActivityRegistrationPage.PopulateStudentDOB(Parameter.Get("NewParentAccountActivityRegistration", "StudentDOB"));
            newParentAccountActivityRegistrationPage.SetStudentGrade(Parameter.Get("NewParentAccountActivityRegistration", "StudentGrade"));
            newParentAccountActivityRegistrationPage.SetStudentGender();
            newParentAccountActivityRegistrationPage.SelectAutoCompleteSchool(Parameter.Get("NewParentAccountActivityRegistration", "StudentSchool"));
            newParentAccountActivityRegistrationPage.ClickStudentInfoBtn();
        }

        [Given(@"Populate fields in step 6 health information form details for registration and click save & continue")]
        public void PopulateHealthInformationFormAndClickNext()
        {
            newParentAccountActivityRegistrationPage.PopulateDoctorName(Parameter.Get("NewParentAccountActivityRegistration", "DoctorName"));
            newParentAccountActivityRegistrationPage.PopulateDoctorPhone(Parameter.Get("NewParentAccountActivityRegistration", "DoctorPhone"));
            newParentAccountActivityRegistrationPage.SetStudentImmunization();
            newParentAccountActivityRegistrationPage.ClickStudentHealthNextBtn();
        }

        [Given(@"Enter parent e-signature for registration and click e-sign and continue to payment button")]
        public void EnterParentESignature()
        {
            newParentAccountActivityPaymentPage.PopulateParentESign(Parameter.Get("NewParentAccountActivityPayment", "ParentESign"));
            newParentAccountActivityPaymentPage.ClickESignNextBtn();
        }

        [Given(@"Enter credit card info for registration and click PayViaCredit button")]
        public void EnterCreditCardInfo()
        {
            newParentAccountActivityPaymentPage.PopulateCreditCardNumber(Parameter.Get("NewParentAccountActivityPayment", "CreditCard"));
            newParentAccountActivityPaymentPage.PopulateCreditCardExpiry(Parameter.Get("NewParentAccountActivityPayment", "CreditCardExpireDate"));
            newParentAccountActivityPaymentPage.PopulateCreditCardCVC(Parameter.Get("NewParentAccountActivityPayment", "CreditCardCVC"));
            newParentAccountActivityPaymentPage.PopulateCreditCardZIP(Parameter.Get("NewParentAccountActivityPayment", "CreditCardZIP"));
            newParentAccountActivityPaymentPage.ClickPayViaCreditBtn();
        }

        [Then(@"Verify thank you message for NewParentAccountActivityPayment")]
        public void VerifyCompleteRegistrationMessage()
        {
            string completed = "Thank You";
            completed.Equals(newParentAccountActivityPaymentPage.RegistrationComplete().Trim());
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
