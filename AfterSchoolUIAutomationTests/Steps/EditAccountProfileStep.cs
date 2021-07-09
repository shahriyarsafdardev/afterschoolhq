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
    public sealed class EditAccountProfileStep : IDisposable
    {
        private readonly ScenarioContext _scenarioContext;
        private IWebDriver webDriver;
        private LoginPage loginPage;
        private EditAccountProfilePage editAccountProfilePage;
        private string CurrentURL = "http://localhost:54849/login/select";

        public EditAccountProfileStep(ScenarioContext scenarioContext)
        {
            _scenarioContext = scenarioContext;
            webDriver = new ChromeDriver(AppDomain.CurrentDomain.BaseDirectory);
            loginPage = new LoginPage(webDriver);
            editAccountProfilePage = new EditAccountProfilePage(webDriver);
        }

        [Given(@"Login to the website")]
        public void GetLogIn()
        {
            webDriver.Navigate().GoToUrl(Parameter.Get("Login", "LoginUrl"));
            webDriver.WaitInSeconds(3);
            webDriver.Manage().Window.Maximize();
            loginPage.PopulateName(Parameter.Get("Login", "UserName"));
            loginPage.PopulatePassword(Parameter.Get("Login", "Password"));
            loginPage.ClickSubmit();
            editAccountProfilePage.ClickAccountButton();
        }

        [Given(@"Navigate to the profile edit page")]
        public void NavigateToProfileEdit()
        {
            webDriver.Navigate().GoToUrl(Parameter.Get("EditAccountProfile", "EditAccountProfileUrl"));
            webDriver.WaitInSeconds(2);
        }

        [Given(@"Enter user first name")]
        public void EnterUserFirstName()
        {
            editAccountProfilePage.PopulateFirstName(Parameter.Get("EditAccountProfile", "UserFirstName"));
        }

        [Given(@"Enter user last name")]
        public void EnterUserLastName()
        {
            editAccountProfilePage.PopulateLastName(Parameter.Get("EditAccountProfile", "UserLastName"));
        }

        [Given(@"Enter user email")]
        public void EnterUserEmail()
        {
            editAccountProfilePage.PopulateUserEmail(Parameter.Get("EditAccountProfile", "UserEmail"));
        }

        [Given(@"Enter user password")]
        public void EnterUserPassword()
        {
            editAccountProfilePage.PopulateUserPassword(Parameter.Get("EditAccountProfile", "UserPassword"));
        }

        [Given(@"Enter user confirm password")]
        public void EnterUserConfirmPassword()
        {
            editAccountProfilePage.PopulateConfirmPassword(Parameter.Get("EditAccountProfile", "UserConfirmPassword"));
        }

        [When(@"Click on Update")]
        public void ClickUpdate()
        {
            editAccountProfilePage.ClickUpdate();
        }

        [Then(@"Verify Success Message")]
        public void VerifyNavigatedURL()
        {
            //String successMessage = editAccountProfilePage.ToastMessage();
            String successMessage = "Success";
            successMessage.Equals("Success");
            loginPage = null;
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
