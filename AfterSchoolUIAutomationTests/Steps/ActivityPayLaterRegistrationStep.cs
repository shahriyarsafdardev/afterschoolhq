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

namespace AfterSchoolUIAutomationTests2.Steps
{
    [Binding]
    public sealed class ActivityPayLaterRegistrationStep : IDisposable
    {
        private readonly ScenarioContext _scenarioContext;
        private IWebDriver webDriver;
        private ActivityPayLaterRegistrationPage activityPayLaterRegistrationPage;

        public ActivityPayLaterRegistrationStep(ScenarioContext scenarioContext)
        {
            _scenarioContext = scenarioContext;
            webDriver = new ChromeDriver(AppDomain.CurrentDomain.BaseDirectory);
            activityPayLaterRegistrationPage = new ActivityPayLaterRegistrationPage(webDriver);
        }

        [Given(@"Navigate to the Pay later activity registration URL")]
        public void NavigateToActivityUrlAndClickRegister()
        {
            webDriver.Navigate().GoToUrl(Parameter.Get("ActivityPayLaterRegistration", "PayLaterActivityPublishedURL"));
            webDriver.WaitInSeconds(5);
            webDriver.Manage().Window.Maximize();
            webDriver.WaitInSeconds(2);
        }

        [Given(@"Click on Register Now")]
        public void ClickRegisterNow()
        {
            activityPayLaterRegistrationPage.ClickRegisterNow();
        }

        [Given(@"Enter already registered parent email and click Next")]
        public void ParentLogin()
        {
            activityPayLaterRegistrationPage.PopulateEmail(Parameter.Get("ActivityPayLaterRegistration", "ParentEmailAddress"));
            activityPayLaterRegistrationPage.ClickNextBtn();
        }

        [Given(@"Enter password and click Log me in")]
        public void VerifyLoginPassword()
        {
            activityPayLaterRegistrationPage.PopulatePassword(Parameter.Get("ActivityPayLaterRegistration", "ParentEmailPassword"));
            activityPayLaterRegistrationPage.ClickLoginBtn();
        }

        [Given(@"Click add child button")]
        public void AddChild()
        {
            activityPayLaterRegistrationPage.ClickAddNewChildBtn();
        }

        [Given(@"Fill in the student information form and click save & continue button")]
        public void PopulateStudentInformationFormAndClickNext()
        {
            activityPayLaterRegistrationPage.PopulateStudentFirstname(Parameter.Get("ActivityPayLaterRegistration", "StudentFirstName"));
            activityPayLaterRegistrationPage.PopulateStudentDOB("dob");
            activityPayLaterRegistrationPage.SetStudentGrade("5th");
            activityPayLaterRegistrationPage.SetStudentGender();
            activityPayLaterRegistrationPage.SelectAutoCompleteSchool(Parameter.Get("ActivityPayLaterRegistration", "StudentLocation"));
            activityPayLaterRegistrationPage.ClickStudentInfoBtn();
        }

        [Given(@"Fill in the health information form and click save & continue button")]
        public void PopulateHealthInformationFormAndClickNext()
        {
            activityPayLaterRegistrationPage.PopulateDoctorName(Parameter.Get("ActivityPayLaterRegistration", "DoctorName"));
            activityPayLaterRegistrationPage.PopulateDoctorPhone(Parameter.Get("ActivityPayLaterRegistration", "DoctorPhone"));
            activityPayLaterRegistrationPage.SetStudentImmunization();
            activityPayLaterRegistrationPage.ClickStudentHealthNextBtn();
        }

        [Given(@"Enter parent e-signature and click e-sign and continue to payment button")]
        public void EnterParentESignature()
        {
            activityPayLaterRegistrationPage.PopulateParentESign(Parameter.Get("ActivityPayLaterRegistration", "ParentESign"));
            activityPayLaterRegistrationPage.ClickESignNextBtn();
        }

        [Given(@"Click on request to pay later link")]
        public void RequestPayLater()
        {
            activityPayLaterRegistrationPage.ClickPayLaterNextBtn();
        }

        [Then(@"Verify registration success message")]
        public void VerifyThankyouMessage()
        {
            string completed = "Thank You";
            completed.Equals(activityPayLaterRegistrationPage.RegistrationComplete().Trim());
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
