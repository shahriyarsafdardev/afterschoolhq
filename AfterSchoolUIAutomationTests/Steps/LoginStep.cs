using AfterSchoolUIAutomationTests.Drivers;
using AfterSchoolUIAutomationTests.Pages;
using AfterSchoolUIAutomationTests.Parameters;
using FluentAssertions;
using TechTalk.SpecFlow;

namespace AfterSchoolUIAutomationTests.Steps
{
    [Binding]
    public sealed class LoginStep
    {

        private readonly ScenarioContext _scenarioContext;
        private readonly LoginPage loginPage;

        public LoginStep(ScenarioContext scenarioContext)
        {
            _scenarioContext = scenarioContext;
            loginPage = new LoginPage(UIDriver.WebDriver);
        }

        [Given(@"Enter username")]
        public void EnterUserName()
        {
            loginPage.PopulateName(Parameter.Get("UserName", "users", "Login"));
        }

        [Given(@"Enter Password")]
        public void EnterPassword()
        {
            loginPage.PopulatePassword(Parameter.Get("Password", "users", "Login"));
        }

        [When(@"Click on login")]
        public void ClickLogin()
        {
            loginPage.ClickSubmit();
        }

        [Then(@"Verify Navigated URL")]
        public void VerifyNavigatedURL()
        {
            string currentURL = UIDriver.WebDriver.Url;
            currentURL.Should().Be(Parameter.Get("LoginRedirectUrl", "urls"));
        }
    }
}