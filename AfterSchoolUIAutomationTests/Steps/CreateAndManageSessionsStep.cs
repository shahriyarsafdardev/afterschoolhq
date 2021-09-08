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

namespace AfterSchoolUIAutomationTests.Steps
{
    [Binding]
    public sealed class CreateAndManageSessionsStep : IDisposable
    {
        private readonly ScenarioContext _scenarioContext;
        private IWebDriver webDriver;
        private LoginPage loginPage;
        private CreateAndManageSessionsPage createAndManageSessionsPage;

        public CreateAndManageSessionsStep(ScenarioContext scenarioContext)
        {
            _scenarioContext = scenarioContext;
            webDriver = new ChromeDriver(AppDomain.CurrentDomain.BaseDirectory);
            loginPage = new LoginPage(webDriver);
            createAndManageSessionsPage = new CreateAndManageSessionsPage(webDriver);
        }

        [Given(@"Login to the school account")]
        public void LoginToSchoolAccount()
        {
            webDriver.Navigate().GoToUrl(Parameter.Get("Login", "LoginUrl"));
            webDriver.WaitInSeconds(3);
            webDriver.Manage().Window.Maximize();
            loginPage.PopulateName(Parameter.Get("Login", "UserName"));
            loginPage.PopulatePassword(Parameter.Get("Login", "Password"));
            loginPage.ClickSubmit();
        }

        [Given(@"Navigate to the sessions section")]
        public void NavigateToSessions()
        {
            createAndManageSessionsPage.HoverActivityCenterAndClickSession();
        }

        [Given(@"Click on add session")]
        public void ClickAddSessions()
        {
            createAndManageSessionsPage.ClickAddSessionBtn();
        }

        [Given(@"Populate new session form and click save")]
        public void PopulateNewSessionForm()
        {
            createAndManageSessionsPage.ClickAddSessionBtn();
        }





        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
