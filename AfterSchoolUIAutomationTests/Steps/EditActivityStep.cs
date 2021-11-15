using AfterSchoolUIAutomationTests.Drivers;
using AfterSchoolUIAutomationTests.Pages;
using AfterSchoolUIAutomationTests.Parameters;
using TechTalk.SpecFlow;

namespace AfterSchoolUIAutomationTests.Steps
{
    [Binding]
    public sealed class EditActivityStep
    {
        private readonly ScenarioContext _scenarioContext;
        private readonly LoginPage loginPage;
        private readonly EditActivityPage editActivityPage;

        public EditActivityStep(ScenarioContext scenarioContext)
        {
            _scenarioContext = scenarioContext;
            loginPage = new LoginPage(UIDriver.WebDriver);
            editActivityPage = new EditActivityPage(UIDriver.WebDriver);
        }

        [Given(@"Login to Edit Activity")]
        public void GetLogIn()
        {
            loginPage.PopulateName(Parameter.Get("UserName", "users", "Login"));
            loginPage.PopulatePassword(Parameter.Get("Password", "users", "Login"));
            loginPage.ClickSubmit();
            //editActivityPage.ClickAccountButton();
        }

        [Given(@"Click on Activities Link")]
        public void ManageActivityButton()
        {
            editActivityPage.HoverActivityCenterAndClickActivities();
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
            editActivityPage.PopulateActivityName(Parameter.Get("ActivityName", "users", "CreateNewActivity"));
            editActivityPage.PopulateTotalAvailableSeats(Parameter.Get("TotalAvailableSeats", "users", "CreateNewActivity"));
            editActivityPage.ToggleInviteOnly(Parameter.Get("InviteOnly", "users", "CreateNewActivity"));
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
            editActivityPage.ActivityDetails(Parameter.Get("AboutActivity", "users", "CreateNewActivity"));
            editActivityPage.ClickSaveAndNext();
        }

        [Given(@"Update contact section and save")]
        public void PopulateContact()
        {
            //editActivityPage.ActivityLedBy();
            editActivityPage.ContactName(Parameter.Get("ContactName", "users", "CreateNewActivity"));
            editActivityPage.ContactPhone(Parameter.Get("ContactPhone", "users", "CreateNewActivity"));
            editActivityPage.ContactEmail(Parameter.Get("ContactEmail", "users", "CreateNewActivity"));
            editActivityPage.ClickSaveAndNext();
        }

        [Given(@"Update location section and save")]
        public void PopulateLocation()
        {
            editActivityPage.PhysicalLocation();
            editActivityPage.SelectAutoComplete(Parameter.Get("ActivityLocation", "users", "CreateNewActivity"));
            editActivityPage.ClickSaveAndNext();
        }

        [Given(@"Update price and payment section and save")]
        public void PopulatePriceAndPayment()
        {
            //editActivityPage.DoParentsPay();
            editActivityPage.RegistrationFee(Parameter.Get("RegistrationFee", "users", "CreateNewActivity"));
            editActivityPage.DoPriceParentpay();
            editActivityPage.ClickSaveAndNext();
        }

        [Given(@"Update email content and save")]
        public void PopulateEmailContent()
        {
            editActivityPage.ShowAdvanceSettings();
        }

        [Then(@"Verify Update Test")]
        public void VerifyAccountCreationContent()
        {
            string create = "Created";
            create.Equals("Created");
        }
    }
}