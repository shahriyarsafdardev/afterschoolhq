using AfterSchoolUIAutomationTests.Pages;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TechTalk.SpecFlow;
using AfterSchoolUIAutomationTests.Helpers.Extensions;

namespace AfterSchoolUIAutomationTests.Steps
{
    [Binding]
    public sealed class LoginStep
    {

        private readonly ScenarioContext _scenarioContext;
        private readonly IWebDriver webDriver;
        private readonly LoginPage loginPage;
        private string CurrentURL = "http://localhost:54849/login/select";

        public LoginStep(ScenarioContext scenarioContext)
        {
            _scenarioContext = scenarioContext;
            webDriver = new ChromeDriver(AppDomain.CurrentDomain.BaseDirectory);
            loginPage = new LoginPage(webDriver);
        }

        [Given(@"Open the browser and navigate to the Login Page")]
        public void GivenNavigateToPage()
        {
            webDriver.Navigate().GoToUrl("http://localhost:54849/login");
            webDriver.WaitInSeconds(3);
        }

        [Given(@"Manage the browser window size")]
        public void ManageWindowSize()
        {
            webDriver.Manage().Window.Maximize();
            webDriver.WaitInSeconds(2);
        }

        [Given(@"Enter username")]
        public void EnterUserName()
        {
            loginPage.PopulateName("parent3_hq@yopmail.com");
            webDriver.WaitInSeconds(2);
        }

        [Given(@"Enter Password")]
        public void EnterPassword()
        {
            loginPage.PopulatePassword("123456789");
            webDriver.WaitInSeconds(2);
        }


        [When(@"Click on login")]
        public void GivenEnterUserNameAndPassword()
        {
            loginPage.ClickSubmit();
            webDriver.WaitInSeconds(5);
        }

        [Then(@"Verify Navigated URL")]
        public void VerifyNavigatedURL()
        {
            String currentURL = webDriver.Url;
            CurrentURL.Equals(currentURL);
            webDriver.Quit();
            webDriver.Dispose();
        }
        
    }
}
