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
            //activityPayLaterRegistrationPage.ClickRegisterNow();
        }

        [Given(@"Enter already registered parent email and click Next")]
        public void ParentLogin()
        {
            //activityPayLaterRegistrationPage.ClickRegisterNow();
        }

        [Given(@"Enter password and click Log me in")]
        public void VerifyLoginPassword()
        {
            //activityPayLaterRegistrationPage.ClickRegisterNow();
        }

        [Given(@"Click add child button")]
        public void AddChild()
        {
            //activityPayLaterRegistrationPage.ClickRegisterNow();
        }

        [Given(@"Fill in the student information form and click save & continue button")]
        public void PopulateStudentInformationFormAndClickNext()
        {
            //activityPayLaterRegistrationPage.ClickRegisterNow();
        }

        [Given(@"Fill in the health information form and click save & continue button")]
        public void PopulateHealthInformationFormAndClickNext()
        {
            //activityPayLaterRegistrationPage.ClickRegisterNow();
        }

        [Given(@"Enter parent e-signature and click e-sign and continue to payment button")]
        public void EnterParentESignature()
        {
            //activityPayLaterRegistrationPage.ClickRegisterNow();
        }

        [Given(@"Click on request to pay later link")]
        public void RequestPayLater()
        {
            //activityPayLaterRegistrationPage.ClickRegisterNow();
        }

        [Then(@"Verify complete registration message")]
        public void VerifyThankyouMessage()
        {
            //string completed = "Thank you";
            //completed.Contains(activityPayLaterRegistrationPage.RegistrationComplete().Trim());
            //webDriver.WaitInSeconds(2);
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
