using AfterSchoolUIAutomationTests.Drivers;
using AfterSchoolUIAutomationTests.Helpers.Extensions;
using AfterSchoolUIAutomationTests.Pages;
using AfterSchoolUIAutomationTests.Parameters;
using FluentAssertions;
using System;
using TechTalk.SpecFlow;

namespace AfterSchoolUIAutomationTests.Steps
{
    [Binding]
    public sealed class NewProviderSignUpFlowStep
    {
        private readonly ScenarioContext _scenarioContext;
        private readonly NewProviderSignUpFlowPage newProviderSignUpFlowPage;

        public NewProviderSignUpFlowStep(ScenarioContext scenarioContext)
        {
            _scenarioContext = scenarioContext;
            newProviderSignUpFlowPage = new NewProviderSignUpFlowPage(UIDriver.WebDriver);
        }

        [Given(@"Navigate to Signup Page")]
        public void NavigateToSignup()
        {
            UIDriver.WebDriver.Navigate().GoToUrl(Parameter.Get("SignUpUrl", "urls"));
            UIDriver.WebDriver.WaitInSeconds(2);
        }

        [Given(@"Manage the browser window size for Signup")]
        public void NavigateAccountAndPrograms()
        {
            UIDriver.WebDriver.Manage().Window.Maximize();
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
            newProviderSignUpFlowPage.PopulateProgramName(Parameter.Get("ProgramName", "users", "AccountSigUp"));
            newProviderSignUpFlowPage.ClickProgramSubmit();
        }

        [Given(@"Enter firstname, lastname, email, password and click continue")]
        public void PopulateBasicInfo()
        {
            newProviderSignUpFlowPage.PopulateFirstName(Parameter.Get("FirstName", "users", "AccountSigUp"));
            newProviderSignUpFlowPage.PopulateLasttName(Parameter.Get("LasttName", "users", "AccountSigUp"));
            newProviderSignUpFlowPage.PopulateEmail("testemail" + DateTime.Now.ToString("yyyyMMddHHmmss") + "@yopmail.com");
            newProviderSignUpFlowPage.PopulateUserPassword(Parameter.Get("UserPassword", "users", "AccountSigUp"));
            newProviderSignUpFlowPage.PopulateUserPassword2(Parameter.Get("UserPassword2", "users", "AccountSigUp"));
            //newProviderSignUpFlowPage.CheckCaptcha();
            newProviderSignUpFlowPage.ClickBasicInfoSubmit();
        }

        [Then("Match window current url with the expected url")]
        public void ThenTheResultShouldBe()
        {
            string curentUrl = UIDriver.WebDriver.Url;
            string expectedUrl = "/Org/onboarding/GettingStarted";
            curentUrl.Should().Contain(expectedUrl);
        }
    }
}