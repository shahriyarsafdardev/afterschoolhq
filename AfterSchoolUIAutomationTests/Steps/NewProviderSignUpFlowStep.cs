using AfterSchoolUIAutomationTests.Drivers;
using AfterSchoolUIAutomationTests.Helpers.Extensions;
using AfterSchoolUIAutomationTests.Pages;
using AfterSchoolUIAutomationTests.Parameters;
using FluentAssertions;
using OpenQA.Selenium;
using System;
using TechTalk.SpecFlow;

namespace AfterSchoolUIAutomationTests.Steps
{
    [Binding]
    public sealed class NewProviderSignUpFlowStep
    {
        private readonly ScenarioContext _scenarioContext;
        private IWebDriver webDriver;
        private readonly NewProviderSignUpFlowPage newProviderSignUpFlowPage;

        public NewProviderSignUpFlowStep(ScenarioContext scenarioContext)
        {
            _scenarioContext = scenarioContext;
            webDriver = UIDriver.WebDriver;
            newProviderSignUpFlowPage = new NewProviderSignUpFlowPage(webDriver);
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
            webDriver.WaitInSeconds(5);
            newProviderSignUpFlowPage.ClickBasicInfoSubmit();
        }

        [Then("Match window current url with the expected url")]
        public void ThenTheResultShouldBe()
        {
            string curentUrl = webDriver.Url;
            string expectedUrl = "/Org/onboarding/GettingStarted";
            //curentUrl.Should().Contain(expectedUrl);
            curentUrl.Should().Contain(expectedUrl);
        }
    }
}