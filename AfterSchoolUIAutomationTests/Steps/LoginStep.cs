using AfterSchoolUIAutomationTests.Pages;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TechTalk.SpecFlow;

namespace AfterSchoolUIAutomationTests.Steps
{
    [Binding]
    public sealed class LoginStep
    {
        // For additional details on SpecFlow step definitions see https://go.specflow.org/doc-stepdef

        private readonly ScenarioContext _scenarioContext;
        private readonly LoginPage _loginPage = new LoginPage();
        private readonly IWebDriver _driver = new ChromeDriver();
        private string CurrentURL;

        public LoginStep(ScenarioContext scenarioContext)
        {
            _scenarioContext = scenarioContext;
        }

        [Given(@"Open the browser and navigate to the Invoice App")]
        public void GivenNavigateToPage()
        {
            _driver.Navigate().GoToUrl("http://localhost:54849/");
        }

        [Given(@"Manage the browser window size")]
        public void ManageWindowSize()
        {
            _driver.Manage().Window.Maximize();
        }

        [Given(@"Enter username")]
        public void EnterUserName()
        {
            _loginPage.PopulateName("parent3_hq@yopmail.com");
        }

        [Given(@"Enter Password")]
        public void EnterPassword()
        {
            _loginPage.PopulatePassword("123456789");
        }


        [When(@"Click on login")]
        public void GivenEnterUserNameAndPassword()
        {
            _loginPage.ClickSubmit();
        }

        [Then(@"Verify Navigated URL")]
        public void VerifyNavigatedURL()
        {
            String currentURL = _driver.Url;
            CurrentURL.Equals(currentURL);
        }
        
    }
}
