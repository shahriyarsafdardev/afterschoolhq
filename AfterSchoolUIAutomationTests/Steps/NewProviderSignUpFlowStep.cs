using AfterSchoolUIAutomationTests.Helpers.Extensions;
using AfterSchoolUIAutomationTests.Pages;
using AfterSchoolUIAutomationTests.Parameters;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TechTalk.SpecFlow;

namespace AfterSchoolUIAutomationTests.Steps
{
    [Binding]
    public sealed class NewProviderSignUpFlowStep : IDisposable
    {
        private readonly ScenarioContext _scenarioContext;
        private IWebDriver webDriver;
        private NewProviderSignUpFlowPage newProviderSignUpFlowPage;

        public NewProviderSignUpFlowStep(ScenarioContext scenarioContext)
        {
            _scenarioContext = scenarioContext;
            webDriver = new ChromeDriver(AppDomain.CurrentDomain.BaseDirectory);
            newProviderSignUpFlowPage = new NewProviderSignUpFlowPage(webDriver);
        }

        [Given(@"Navigate to Signup Page")]
        public void NavigateToSignup()
        {
            webDriver.Navigate().GoToUrl(Parameter.Get("AccountSigUp", "SignUpUrl"));
            webDriver.WaitInSeconds(2);
        }

        [Given(@"Manage the browser window size for Signup")]
        public void NavigateAccountAndPrograms()
        {
            webDriver.Manage().Window.Maximize();
        }

        [Given(@"Click Get started button")]
        public void ClickGetStartedButton()
        {
            newProviderSignUpFlowPage.ClickSignUp();
        }


        [Given(@"Click create individual account button")]
        public void ClickIndividualAccountButton()
        {
            newProviderSignUpFlowPage.ClickIndividualAccount();
        }

        [Given(@"Click provide programs for student button")]
        public void ClickProvideProgramsButton()
        {
            newProviderSignUpFlowPage.ProvideProgramsForStudents();
        }

        [Given(@"Enter Program Name and click continue")]
        public void PopulateProgramName()
        {
            newProviderSignUpFlowPage.PopulateProgramName(Parameter.Get("AccountSigUp", "ProgramName"));
            newProviderSignUpFlowPage.ClickProgramSubmit();
        }

        [Given(@"Enter firstname, lastname, email, password and click continue")]
        public void PopulateBasicInfo()
        {
            newProviderSignUpFlowPage.PopulateFirstName(Parameter.Get("AccountSigUp", "FirstName"));
            newProviderSignUpFlowPage.PopulateLasttName(Parameter.Get("AccountSigUp", "LasttName"));
            newProviderSignUpFlowPage.PopulateEmail("testemail" + DateTime.Now.ToString("yyyyMMddHHmmss") + "@yopmail.com");
            newProviderSignUpFlowPage.PopulateUserPassword(Parameter.Get("AccountSigUp", "UserPassword"));
            newProviderSignUpFlowPage.PopulateUserPassword2(Parameter.Get("AccountSigUp", "UserPassword2"));
            //newProviderSignUpFlowPage.CheckCaptcha();
            newProviderSignUpFlowPage.ClickBasicInfoSubmit();
        }

        [Then("Match window current url with the expected url")]
        public void ThenTheResultShouldBe()
        {
            String curentUrl = webDriver.Url;
            string expectedUrl = "/Org/onboarding/GettingStarted";
            curentUrl.Contains(expectedUrl);
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
