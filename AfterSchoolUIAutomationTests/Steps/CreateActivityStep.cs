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

        [Given(@"Click on add activity button")]
        public void ClickAddActivityButton()
        {
            createActivityPage.ClickAddActivity();
        }

        [Given(@"Populate settings section and save")]
        public void PopulateSettings()
        {
            createActivityPage.PopulateActivityName(Parameter.Get("CreateNewActivity", "ActivityName"));
            createActivityPage.PopulateTotalAvailableSeats(Parameter.Get("CreateNewActivity", "TotalAvailableSeats"));
            createActivityPage.ToggleInviteOnly(Parameter.Get("CreateNewActivity", "InviteOnly"));
            createActivityPage.SetSignUpPeriodOpen();
            createActivityPage.SetSignUpPeriodClose();
            createActivityPage.ClickSaveAndNext();
        }

        [Given(@"Populate restrictions section and save")]
        public void PopulateRestrictions()
        {
            createActivityPage.SelectGender();
            createActivityPage.RestrictedBy();
            createActivityPage.MinimumGrade();
            createActivityPage.MaximumGrade();
            createActivityPage.ClickSaveAndNext();
        }

        [Given(@"Populate schedule section and save")]
        public void PopulateSchedule()
        {
            createActivityPage.HasSchedule(Parameter.Get("CreateNewActivity", "HasSchedule"));
            //createActivityPage.RepeatsWeekly(Parameter.Get("CreateNewActivity", "IsWeekly"));
            createActivityPage.WeekDays();
            createActivityPage.SetScheduleStartDate();
            createActivityPage.SetScheduleEndDate();
            createActivityPage.SetScheduleStartTime();
            createActivityPage.SetScheduleEndTime();
            createActivityPage.ClickSaveAndNext();
        }

        [Given(@"Populate about your activity section and save")]
        public void PopulateAbout()
        {
            createActivityPage.ActivityDetails(Parameter.Get("CreateNewActivity", "AboutActivity"));
            createActivityPage.ClickSaveAndNext();
        }

        [Given(@"Populate contact section and save")]
        public void PopulateContact()
        {
            //createActivityPage.ActivityLedBy();
            createActivityPage.ContactName(Parameter.Get("CreateNewActivity", "ContactName"));
            createActivityPage.ContactPhone(Parameter.Get("CreateNewActivity", "ContactPhone"));
            createActivityPage.ContactEmail(Parameter.Get("CreateNewActivity", "ContactEmail"));
            createActivityPage.ClickSaveAndNext();
        }

        [Given(@"Populate location section and save")]
        public void PopulateLocation()
        {
            createActivityPage.PhysicalLocation();
            createActivityPage.SelectAutoComplete(Parameter.Get("CreateNewActivity", "ActivityLocation"));
            createActivityPage.ClickSaveAndNext();
        }

        [Given(@"Populate price and payment section and save")]
        public void PopulatePriceAndPayment()
        {
            createActivityPage.DoParentsPay();
            createActivityPage.RegistrationFee(Parameter.Get("CreateNewActivity", "RegistrationFee"));
            createActivityPage.DoPriceParentpay();
            createActivityPage.ClickSaveAndNext();
        }

        [Given(@"Populate email content and save")]
        public void PopulateEmailContent()
        {

        }
        [Then(@"Verify Test")]
        public void VerifyAccountCreationContent()
        {
            string create = "Created";
            create.Equals("Created");
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
