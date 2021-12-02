using AfterSchoolUIAutomationTests.Drivers;
using AfterSchoolUIAutomationTests.Helpers.Extensions;
using AfterSchoolUIAutomationTests.Parameters;
using OpenQA.Selenium;
using TechTalk.SpecFlow;

namespace AfterSchoolUIAutomationTests.Steps
{
    [Binding]
    public sealed class BrowserStep
    {

        private readonly ScenarioContext _scenarioContext;

        public BrowserStep(ScenarioContext scenarioContext)
        {
            _scenarioContext = scenarioContext;
        }

        [Given(@"Open the browser and navigate to the AfterSchool App")]
        public void GivenNavigateToPage()
        {
            UIDriver.WebDriver.Navigate().GoToUrl(Parameter.Get("LoginUrl", "urls"));
            UIDriver.WebDriver.WaitInSeconds(3);
            UIDriver.WebDriver.Manage().Window.Maximize();
        }

        [Given(@"Navigate to Signup Page")]
        public void NavigateToSignup()
        {
            UIDriver.WebDriver.Navigate().GoToUrl(Parameter.Get("SignUpUrl", "urls"));
            UIDriver.WebDriver.WaitInSeconds(2);
            UIDriver.WebDriver.Manage().Window.Maximize();
        }

        [Given(@"Navigate to signup welcome and click Get started button")]
        public void ClickGetStartedButton()
        {
            UIDriver.WebDriver.Navigate().GoToUrl(Parameter.Get("SignUpUrl", "urls"));
            UIDriver.WebDriver.FindElement(By.XPath("(//button[@id='btn-GetStarted'])[1]")).Click();
        }
    }
}