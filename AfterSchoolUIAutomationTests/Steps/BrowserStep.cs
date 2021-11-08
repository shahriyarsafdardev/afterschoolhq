using AfterSchoolUIAutomationTests.Drivers;
using AfterSchoolUIAutomationTests.Helpers.Extensions;
using AfterSchoolUIAutomationTests.Parameters;
using TechTalk.SpecFlow;

namespace AfterSchoolUIAutomationTests.Steps
{
    [Binding]
    public sealed class BrowserStep
    {
        // For additional details on SpecFlow step definitions see https://go.specflow.org/doc-stepdef

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
    }
}