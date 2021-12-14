using AfterSchoolUIAutomationTests.Pages;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TechTalk.SpecFlow;
using AfterSchoolUIAutomationTests.Helpers.Extensions;
using AfterSchoolUIAutomationTests.Parameters;
using AfterSchoolUIAutomationTests.Drivers;
using FluentAssertions;

namespace AfterSchoolUIAutomationTests.Steps
{
    [Binding]
    public sealed class CreateAndManageSessionsStep
    {
        private readonly ScenarioContext _scenarioContext;
        private readonly IWebDriver webDriver;
        private readonly LoginPage loginPage;
        private readonly CreateAndManageSessionsPage createAndManageSessionsPage;

        public CreateAndManageSessionsStep(ScenarioContext scenarioContext)
        {
            _scenarioContext = scenarioContext;
            webDriver = UIDriver.WebDriver;
            loginPage = new LoginPage(webDriver);
            createAndManageSessionsPage = new CreateAndManageSessionsPage(webDriver);
        }

        [Given(@"Login to the school account")]
        public void LoginToSchoolAccount()
        {
            loginPage.PopulateName(Parameter.Get("UserName", "users", "CreateAndManageSessions"));
            loginPage.PopulatePassword(Parameter.Get("Password","users", "CreateAndManageSessions"));
            loginPage.ClickSubmit();
        }

        [Given(@"Navigate to the sessions section and click on add session")]
        public void NavigateToSessions()
        {
            createAndManageSessionsPage.HoverActivityCenterAndClickSession();
            createAndManageSessionsPage.ClickAddSessionBtn();
        }

        [Given(@"Populate new session form and click save")]
        public void PopulateNewSessionForm()
        {
            createAndManageSessionsPage.PopulateSessionName(Parameter.Get("SessionName", "users", "CreateAndManageSessions"));
            createAndManageSessionsPage.PopulateSessionDetails(Parameter.Get("SessionDetails", "users", "CreateAndManageSessions"));
            createAndManageSessionsPage.SelectRegistrationForm();
            createAndManageSessionsPage.SubmitForm();
        }

        [Given(@"Populate Signup Periods and click save")]
        public void PopulateSignupPeriods()
        {
            createAndManageSessionsPage.NavigateToSignupPeriodsTab();
            createAndManageSessionsPage.BringSignUpPeriodsModal();
            createAndManageSessionsPage.AddSignUpPeriodOpenCloseDate();
        }

        [Given(@"Populate setting form and click save")]
        public void PopulateActivitySettings()
        {
            createAndManageSessionsPage.NavigateToActivtiesTab();
            createAndManageSessionsPage.BringActivitySectionsForms();
            createAndManageSessionsPage.PopulateActivityName(Parameter.Get("SessionActivityName" , "users" , "CreateAndManageSessions"));
            createAndManageSessionsPage.ClickSettingsSaveAndNextBtn();
        }

        [Given(@"Populate restrictions form and click save")]
        public void PopulateRestrictions()
        {
            createAndManageSessionsPage.SelectGender();
            createAndManageSessionsPage.RestrictedBy();
            createAndManageSessionsPage.MinimumGrade();
            createAndManageSessionsPage.MaximumGrade();
            createAndManageSessionsPage.ClickRestrictionsSaveAndNextBtn();
        }

        [Given(@"Populate schedule form and click save")]
        public void PopulateSchduleForm()
        {
            createAndManageSessionsPage.HasSchedule(Parameter.Get("HasSchedule", "users" , "CreateAndManageSessions"));
            //createAndManageSessionsPage.RepeatsWeekly(Parameter.Get("CreateAndManageSessions", "IsWeekly")); 
            createAndManageSessionsPage.WeekDays();
            createAndManageSessionsPage.SetScheduleStartDate();
            createAndManageSessionsPage.SetScheduleEndDate();
            createAndManageSessionsPage.SetScheduleStartTime();
            createAndManageSessionsPage.SetScheduleEndTime();
            createAndManageSessionsPage.ClickScheduleSaveAndNextBtn();
        }

        [Given(@"Populate about activity form and click save")]
        public void PopulateAboutActivity()
        {
            createAndManageSessionsPage.ActivityDetails(Parameter.Get("AboutActivity", "users", "CreateAndManageSessions"));
            createAndManageSessionsPage.ClickAboutSaveAndNextBtn();
        }

        [Given(@"Populate activity contact form and click save")]
        public void PopulateActivityContact()
        {
            createAndManageSessionsPage.ActivityLedBy();
            createAndManageSessionsPage.ContactName(Parameter.Get("ContactName", "users", "CreateAndManageSessions"));
            createAndManageSessionsPage.ContactPhone(Parameter.Get("ContactPhone" , "users" , "CreateAndManageSessions"));
            createAndManageSessionsPage.ContactEmail(Parameter.Get("ContactEmail" , "users" , "CreateAndManageSessions"));
            createAndManageSessionsPage.ClickContactSaveAndNextBtn();
        }

        [Given(@"Populate activity location form and click save")]
        public void PopulateLocation()
        {
            createAndManageSessionsPage.PhysicalLocation();
            createAndManageSessionsPage.SelectAutoComplete(Parameter.Get("Location", "users" , "CreateAndManageSessions"));
            createAndManageSessionsPage.ClickLocationSaveAndNextBtn();
        }

        [Given(@"Populate price and click save")]
        public void PopulatePrice()
        {
            createAndManageSessionsPage.DoParentsPay();
            createAndManageSessionsPage.RegistrationFee(Parameter.Get("Fee" , "users", "CreateAndManageSessions"));
            createAndManageSessionsPage.DoPriceParentpay();
            createAndManageSessionsPage.ClickPriceSaveAndNextBtn();
        }

        [Then(@"Veriy the activity is published")]
        public void VerifyCompleteRegistrationMessage()
        {
            string completed = createAndManageSessionsPage.ActivityPublished().Trim();
            completed.Should().Be("Your Activity is published");
        }
    }
}
