using AfterSchoolUIAutomationTests.Drivers;
using OpenQA.Selenium.Chrome;
using System.IO;
using System.Reflection;
using TechTalk.SpecFlow;

namespace AfterSchoolUIAutomationTests.Hooks
{
    [Binding]
    public class Setup
    {
        private readonly ScenarioContext _scenarioContext;

        public Setup(ScenarioContext scenarioContext)
        {
            _scenarioContext = scenarioContext;
        }

        [BeforeScenario]
        public static void RunDriver()
        {
            var chromeOptions = new ChromeOptions();
            chromeOptions.AddArguments("headless");
            UIDriver.WebDriver = new ChromeDriver(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
        }

        [AfterScenario]
        public static void CloseDriver()
        {
            UIDriver.CloseDriver();
        }
    }
}