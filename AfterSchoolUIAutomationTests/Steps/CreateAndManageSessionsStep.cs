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

        [Given(@"Navigate to the sessions section and click on add session")]
        public void NavigateToSessions()
        {
            createAndManageSessionsPage.HoverActivityCenterAndClickSession();
            createAndManageSessionsPage.ClickAddSessionBtn();
        }

        [Given(@"Populate new session form and click save")]
        public void PopulateNewSessionForm()
        {
            createAndManageSessionsPage.PopulateSessionName(Parameter.Get("CreateAndManageSessions", "SessionName"));
            createAndManageSessionsPage.PopulateSessionDetails(Parameter.Get("CreateAndManageSessions", "SessionDetails"));
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
            createAndManageSessionsPage.PopulateActivityName(Parameter.Get("CreateAndManageSessions", "SessionActivityName"));
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
            createAndManageSessionsPage.HasSchedule(Parameter.Get("CreateAndManageSessions", "HasSchedule"));
            createAndManageSessionsPage.RepeatsWeekly(Parameter.Get("CreateAndManageSessions", "IsWeekly"));
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
            createAndManageSessionsPage.ActivityDetails(Parameter.Get("CreateAndManageSessions", "AboutActivity"));
            createAndManageSessionsPage.ClickAboutSaveAndNextBtn();
        }

        [Given(@"Populate activity contact form and click save")]
        public void PopulateActivityContact()
        {
            createAndManageSessionsPage.ActivityLedBy();
            createAndManageSessionsPage.ContactName(Parameter.Get("CreateAndManageSessions", "ContactName"));
            createAndManageSessionsPage.ContactPhone(Parameter.Get("CreateAndManageSessions", "ContactPhone"));
            createAndManageSessionsPage.ContactEmail(Parameter.Get("CreateAndManageSessions", "ContactEmail"));
            createAndManageSessionsPage.ClickContactSaveAndNextBtn();
        }

        [Given(@"Populate activity location form and click save")]
        public void PopulateLocation()
        {
            createAndManageSessionsPage.PhysicalLocation();
            createAndManageSessionsPage.SelectAutoComplete(Parameter.Get("CreateAndManageSessions", "Location"));
            createAndManageSessionsPage.ClickLocationSaveAndNextBtn();
        }

        [Given(@"Populate price and click save")]
        public void PopulatePrice()
        {
            createAndManageSessionsPage.DoParentsPay();
            createAndManageSessionsPage.RegistrationFee(Parameter.Get("CreateAndManageSessions", "Fee"));
            createAndManageSessionsPage.DoPriceParentpay();
            createAndManageSessionsPage.ClickPriceSaveAndNextBtn();
        }

        //ToDo: Verify




        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
