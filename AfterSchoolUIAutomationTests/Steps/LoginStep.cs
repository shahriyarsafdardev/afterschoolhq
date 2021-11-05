using AfterSchoolUIAutomationTests.Pages;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TechTalk.SpecFlow;
using AfterSchoolUIAutomationTests.Helpers.Extensions;
using AfterSchoolUIAutomationTests.Parameters;
using System.IO;
using System.Reflection;

namespace AfterSchoolUIAutomationTests.Steps
{
    [Binding]
    public sealed class LoginStep : IDisposable
    {

        private readonly ScenarioContext _scenarioContext;
        private IWebDriver webDriver;
        private LoginPage loginPage;
        private string CurrentURL = "http://localhost:54849/login/select";

        public LoginStep(ScenarioContext scenarioContext)
        {
            _scenarioContext = scenarioContext;
            var chromeOptions = new ChromeOptions();
            chromeOptions.AddArguments("headless");
            webDriver = new ChromeDriver(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), chromeOptions);
            loginPage = new LoginPage(webDriver);
        }

        [Given(@"Open the browser and navigate to the Login Page")]
        public void GivenNavigateToPage()
        {
            webDriver.Navigate().GoToUrl(Parameter.Get("Login", "LoginUrl"));
            webDriver.WaitInSeconds(3);
        }

        [Given(@"Manage the browser window size")]
        public void ManageWindowSize()
        {
            webDriver.Manage().Window.Maximize();
        }

        [Given(@"Enter username")]
        public void EnterUserName()
        {
            loginPage.PopulateName(Parameter.Get("Login", "UserName"));
        }

        [Given(@"Enter Password")]
        public void EnterPassword()
        {
            loginPage.PopulatePassword(Parameter.Get("Login", "Password"));
        }

        [When(@"Click on login")]
        public void ClickLogin()
        {
            loginPage.ClickSubmit();
        }

        [Then(@"Verify Navigated URL")]
        public void VerifyNavigatedURL()
        {
            String currentURL = webDriver.Url;
            CurrentURL.Equals(currentURL);
            webDriver.Quit();
            webDriver.Dispose();
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
