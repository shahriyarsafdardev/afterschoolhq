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
    public sealed class CreateActivityStep : IDisposable
    {
        private readonly ScenarioContext _scenarioContext;
        private IWebDriver webDriver;
        private LoginPage loginPage;
        public CreateActivityPage createActivityPage;

        public CreateActivityStep(ScenarioContext scenarioContext)
        {
            _scenarioContext = scenarioContext;
            webDriver = new ChromeDriver(AppDomain.CurrentDomain.BaseDirectory);
            loginPage = new LoginPage(webDriver);
            createActivityPage = new CreateActivityPage(webDriver);
        }

        [Given(@"Login to Create Activity")]
        public void GetLogIn()
        {
            webDriver.Navigate().GoToUrl(Parameter.Get("Login", "LoginUrl"));
            webDriver.WaitInSeconds(2);
            webDriver.Manage().Window.Maximize();
            loginPage.PopulateName(Parameter.Get("Login", "UserName"));
            loginPage.PopulatePassword(Parameter.Get("Login", "Password"));
            loginPage.ClickSubmit();
        }

        [Given(@"Click on account button")]
        public void ClickAccountButton()
        {
            createActivityPage.ClickAccountButton();
        }

        [Given(@"Navigate to the new activity page")]
        public void NavigateToAddActivity()
        {
            var addActivityUrl = Parameter.Get("CreateNewActivity", "AddActivityUrl");
            //var correctedUrl = addActivityUrl.Replace("&amp;" , "&");
            webDriver.Navigate().GoToUrl(addActivityUrl);
            webDriver.WaitInSeconds(5);
        }

        [Given(@"Populate settings section and save")]
        public void PopulateSettings()
        {
            createActivityPage.PopulateActivityName(Parameter.Get("CreateNewActivity", "ActivityName"));
            createActivityPage.PopulateTotalAvailableSeats(Parameter.Get("CreateNewActivity", "TotalAvailableSeats"));
            createActivityPage.ToggleInviteOnly(Parameter.Get("CreateNewActivity", "InviteOnly"));
            createActivityPage.SetSignUpPeriodOpen("07/07/2021");
            createActivityPage.SetSignUpPeriodClose("08/31/2021");
            createActivityPage.ClickSaveAndNext();
        }

        //[Given(@"Populate restrictions section and save")]
        //public void PopulateRestrictions()
        //{

        //}

        //[Given(@"Populate schedule section and save")]
        //public void PopulateSchedule()
        //{

        //}

        //[Given(@"Populate about your activity section and save")]
        //public void PopulateAbout()
        //{

        //}

        //[Given(@"Populate contact section and save")]
        //public void PopulateContact()
        //{

        //}

        //[Given(@"Populate location section and save")]
        //public void PopulateLocation()
        //{

        //}

        //[Given(@"Populate price and payment section and save")]
        //public void PopulatePriceAndPayment()
        //{

        //}

        //[Given(@"Populate email content and save")]
        //public void PopulateEmailContent()
        //{

        //}

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
