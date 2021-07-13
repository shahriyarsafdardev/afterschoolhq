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
            editActivityPage.ClickManageButton();
        }

        [Given(@"Click on Edit Activity Button")]
        public void ClickEditActivityLink()
        {
            editActivityPage.ClickEditActivityLink();
        }

        [Given(@"Update settings section and save")]
        public void PopulateSettings()
        {
            editActivityPage.PopulateActivityName(Parameter.Get("CreateNewActivity", "ActivityName"));
            editActivityPage.PopulateTotalAvailableSeats(Parameter.Get("CreateNewActivity", "TotalAvailableSeats"));
            editActivityPage.ToggleInviteOnly(Parameter.Get("CreateNewActivity", "InviteOnly"));
            editActivityPage.SelectNewDates();
            editActivityPage.SetSignUpPeriodOpen();
            editActivityPage.SetSignUpPeriodClose();
            editActivityPage.ClickSaveAndNext();
        }

        [Given(@"Update restrictions section and save")]
        public void PopulateRestrictions()
        {
            editActivityPage.ParentalConsent();
            //editActivityPage.SelectGender();
            //editActivityPage.RestrictedBy();
            editActivityPage.MinimumGrade();
            editActivityPage.MaximumGrade();
            editActivityPage.ClickSaveAndNext();
        }

        [Given(@"Update schedule section and save")]
        public void PopulateSchedule()
        {
            //editActivityPage.HasSchedule(Parameter.Get("CreateNewActivity", "HasSchedule"));
            //editActivityPage.RepeatsWeekly(Parameter.Get("CreateNewActivity", "IsWeekly"));
            editActivityPage.WeekDays();
            editActivityPage.SetScheduleStartDate();
            editActivityPage.SetScheduleEndDate();
            editActivityPage.SetScheduleStartTime();
            editActivityPage.SetScheduleEndTime();
            editActivityPage.ClickSaveAndNext();
        }

        [Given(@"Update about your activity section and save")]
        public void PopulateAbout()
        {
            editActivityPage.ActivityDetails(Parameter.Get("CreateNewActivity", "AboutActivity"));
            editActivityPage.ClickSaveAndNext();
        }

        [Given(@"Update contact section and save")]
        public void PopulateContact()
        {
            //editActivityPage.ActivityLedBy();
            editActivityPage.ContactName(Parameter.Get("CreateNewActivity", "ContactName"));
            editActivityPage.ContactPhone(Parameter.Get("CreateNewActivity", "ContactPhone"));
            editActivityPage.ContactEmail(Parameter.Get("CreateNewActivity", "ContactEmail"));
            editActivityPage.ClickSaveAndNext();
        }

        [Given(@"Update location section and save")]
        public void PopulateLocation()
        {
            editActivityPage.PhysicalLocation();
            editActivityPage.SelectAutoComplete(Parameter.Get("CreateNewActivity", "ActivityLocation"));
            editActivityPage.ClickSaveAndNext();
        }

        [Given(@"Update price and payment section and save")]
        public void PopulatePriceAndPayment()
        {
            //editActivityPage.DoParentsPay();
            editActivityPage.RegistrationFee(Parameter.Get("CreateNewActivity", "RegistrationFee"));
            editActivityPage.DoPriceParentpay();
            editActivityPage.ClickSaveAndNext();
        }

        [Given(@"Update email content and save")]
        public void PopulateEmailContent()
        {

        }

        [Then(@"Verify Update Test")]
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
