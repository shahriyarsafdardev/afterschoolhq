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
    public sealed class EditActivityStep : IDisposable
    {
        private readonly ScenarioContext _scenarioContext;
        private IWebDriver webDriver;
        private LoginPage loginPage;
        private EditActivityPage editActivityPage;

        public EditActivityStep(ScenarioContext scenarioContext)
        {
            _scenarioContext = scenarioContext;
            webDriver = new ChromeDriver(AppDomain.CurrentDomain.BaseDirectory);
            loginPage = new LoginPage(webDriver);
            editActivityPage = new EditActivityPage(webDriver);
        }

        [Given(@"Login to Edit Activity")]
        public void GetLogIn()
        {
            webDriver.Navigate().GoToUrl(Parameter.Get("Login", "LoginUrl"));
            webDriver.WaitInSeconds(2);
            webDriver.Manage().Window.Maximize();
            loginPage.PopulateName(Parameter.Get("Login", "UserName"));
            loginPage.PopulatePassword(Parameter.Get("Login", "Password"));
            loginPage.ClickSubmit();
            editActivityPage.ClickAccountButton();
        }

        [Given(@"Click on Activities Link")]
        public void ManageActivityButton()
        {
            editActivityPage.ClickActivitiesLink();
        }

        [Given(@"Click on Manage Activity Button")]
        public void ClickActivitiesLink()
        {
            editActivityPage.ClickActivitiesLink();
        }

        [Given(@"Click on Edit Activity Button")]
        public void ClickManageButton()
        {
            editActivityPage.ClickManageButton();
        }

        [Given(@"Click on Edit Activity Link")]
        public void ClickEditActivityLink()
        {
            editActivityPage.ClickEditActivityLink();
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
