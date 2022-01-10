using AfterSchoolUIAutomationTests.Drivers;
using AfterSchoolUIAutomationTests.Helpers.Extensions;
using AfterSchoolUIAutomationTests.Pages;
using AfterSchoolUIAutomationTests.Parameters;
using OpenQA.Selenium.Chrome;
using System;
using TechTalk.SpecFlow;

namespace AfterSchoolUIAutomationTests.Steps
{
    [Binding]
    public sealed class NewParentAccountActivityRegistrationStep
    {
        private readonly ScenarioContext _scenarioContext;
        private readonly NewParentAccountActivityRegistrationPage newParentAccountActivityRegistrationPage;

        public NewParentAccountActivityRegistrationStep(ScenarioContext scenarioContext)
        {
            _scenarioContext = scenarioContext;
            newParentAccountActivityRegistrationPage = new NewParentAccountActivityRegistrationPage(UIDriver.WebDriver);
            var options = new ChromeOptions();
            options.AddArgument("no-sandbox");
        }

        [Given(@"Navigate to the session publish url and click register now")]
        public void NavigateToActivityUrlAndClickRegister()
        {
            UIDriver.WebDriver.Navigate().GoToUrl(Parameter.Get("ActivityPublishedURL", "urls"));
            UIDriver.WebDriver.WaitInSeconds(10);
            UIDriver.WebDriver.Manage().Window.Maximize();
            UIDriver.WebDriver.WaitInSeconds(2);
            newParentAccountActivityRegistrationPage.ClickRegisterNow();
        }

        [Given(@"Enter parent email address and click next")]
        public void PopulateEmailAndClickNext()
        {
            newParentAccountActivityRegistrationPage.PopulateEmail("parent" + DateTime.Now.ToString("yyyyMMddHHmmss") + "@yopmail.com");
            newParentAccountActivityRegistrationPage.ClickNextBtn();
        }

        [Given(@"Populate fields in step 1 parent signup form details and click save & continue")]
        public void PopulateParentSignupFormAndClickNext()
        {
            newParentAccountActivityRegistrationPage.PopulateFirstName(Parameter.Get("FirstName", "users", "NewParentAccountActivityRegistration"));
            newParentAccountActivityRegistrationPage.PopulateLastName(Parameter.Get("LastName", "users", "NewParentAccountActivityRegistration"));
            newParentAccountActivityRegistrationPage.PopulatePhone(Parameter.Get("Phone", "users", "NewParentAccountActivityRegistration"));
            newParentAccountActivityRegistrationPage.PopulatePassword1(Parameter.Get("Password1", "users", "NewParentAccountActivityRegistration"));
            newParentAccountActivityRegistrationPage.PopulatePassword2(Parameter.Get("Password2", "users", "NewParentAccountActivityRegistration"));
            newParentAccountActivityRegistrationPage.ClickIAgreeCheck();
            newParentAccountActivityRegistrationPage.ClickParentSignUpNextBtn();
        }

        [Given(@"Populate fields in step 2 home address form details and click save & continue")]
        public void PopulateHomeAddressFormAndClickNext()
        {
            newParentAccountActivityRegistrationPage.PopulateStreetAddress(Parameter.Get("StreetAddress", "users", "NewParentAccountActivityRegistration"));
            newParentAccountActivityRegistrationPage.PopulateCity(Parameter.Get("City", "users", "NewParentAccountActivityRegistration"));
            newParentAccountActivityRegistrationPage.SelectState(Parameter.Get("State", "users", "NewParentAccountActivityRegistration"));
            newParentAccountActivityRegistrationPage.PopulateZipCode(Parameter.Get("Zipcode", "users", "NewParentAccountActivityRegistration"));
            newParentAccountActivityRegistrationPage.ClickHomeAddressNextBtn();
        }

        [Given(@"Populate fields in step 3 additional parent, guardian form details and click save & continue")]
        public void PopulateAdditionalParentGuardianFormAndClickNext()
        {
            newParentAccountActivityRegistrationPage.PopulateGuardianFirstName(Parameter.Get("GuardianFirstName", "users", "NewParentAccountActivityRegistration"));
            newParentAccountActivityRegistrationPage.PopulateGuardianLastName(Parameter.Get("GuardianLastName", "users", "NewParentAccountActivityRegistration"));
            newParentAccountActivityRegistrationPage.PopulateGuardianEmail(Parameter.Get("GuardianEmail", "users", "NewParentAccountActivityRegistration"));
            newParentAccountActivityRegistrationPage.PopulateGuardianMobile(Parameter.Get("GuardianMobile", "users", "NewParentAccountActivityRegistration"));
            newParentAccountActivityRegistrationPage.ClickParentGuardianNextBtn();
        }

        [Given(@"Populate fields in step 4 emergency contacts form details and click save & continue")]
        public void PopulateEmergencyContactsFormAndClickNext()
        {
            newParentAccountActivityRegistrationPage.PopulateEmergencyContactFirstName(Parameter.Get("EmergencyContactFirstName", "users", "NewParentAccountActivityRegistration"));
            newParentAccountActivityRegistrationPage.PopulateEmergencyContactLastName(Parameter.Get("EmergencyContactLastName", "users", "NewParentAccountActivityRegistration"));
            newParentAccountActivityRegistrationPage.PopulateEmergencyContactPhone(Parameter.Get("EmergencyContactPhone", "users", "NewParentAccountActivityRegistration"));
            newParentAccountActivityRegistrationPage.ClickEmergencyNextBtn();
        }

        [Given(@"Populate fields in step 5 student information form details and click save & continue")]
        public void PopulateStudentInformationFormAndClickNext()
        {
            newParentAccountActivityRegistrationPage.PopulateStudentFirstName(Parameter.Get("StudentFirstName", "users", "NewParentAccountActivityRegistration"));
            newParentAccountActivityRegistrationPage.PopulateStudentLastName(Parameter.Get("StudentLastName", "users", "NewParentAccountActivityRegistration"));
            newParentAccountActivityRegistrationPage.PopulateStudentPhone(Parameter.Get("StudentPhone", "users", "NewParentAccountActivityRegistration"));
            newParentAccountActivityRegistrationPage.PopulateStudentDOB(Parameter.Get("StudentDOB", "users", "NewParentAccountActivityRegistration"));
            newParentAccountActivityRegistrationPage.SetStudentGrade(Parameter.Get("StudentGrade", "users", "NewParentAccountActivityRegistration"));
            newParentAccountActivityRegistrationPage.SetStudentGender();
            newParentAccountActivityRegistrationPage.SelectAutoCompleteSchool(Parameter.Get("StudentSchool", "users", "NewParentAccountActivityRegistration"));
            newParentAccountActivityRegistrationPage.ClickStudentInfoBtn();
        }

        [Given(@"Populate fields in step 6 health information form details and click save & continue")]
        public void PopulateHealthInformationFormAndClickNext()
        {
            newParentAccountActivityRegistrationPage.PopulateDoctorName(Parameter.Get("DoctorName", "users", "NewParentAccountActivityRegistration"));
            newParentAccountActivityRegistrationPage.PopulateDoctorPhone(Parameter.Get("DoctorPhone", "users", "NewParentAccountActivityRegistration"));
            newParentAccountActivityRegistrationPage.SetStudentImmunization();
            newParentAccountActivityRegistrationPage.ClickStudentHealthNextBtn();
        }

        [Then(@"Verify complete registration message")]
        public void VerifyCompleteRegistrationMessage()
        {
            string completed = "Complete registration for";
            completed.Contains(newParentAccountActivityRegistrationPage.RegistrationComplete().Trim());
            UIDriver.WebDriver.WaitInSeconds(2);
        }
    }
}
