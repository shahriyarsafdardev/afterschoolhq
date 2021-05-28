using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TechTalk.SpecFlow;

namespace AfterSchoolUIAutomationTests.Steps
{
    [Binding]
    public sealed class StepBrowser
    {
        // For additional details on SpecFlow step definitions see https://go.specflow.org/doc-stepdef

        private readonly ScenarioContext _scenarioContext;

        public StepBrowser(ScenarioContext scenarioContext)
        {
            _scenarioContext = scenarioContext;
        }

        //[Given(@"Open the browser and navigate to the Invoice App")]
        //public void GivenNavigateToPage()
        //{
        //    UIDriver.WebDriver.Navigate().GoToUrl(Parameter.Get<string>("InvoiceAppUrl"));
        //}
    }
}
