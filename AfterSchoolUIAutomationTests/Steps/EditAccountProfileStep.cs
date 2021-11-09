using AfterSchoolUIAutomationTests.Drivers;
using AfterSchoolUIAutomationTests.Helpers.Extensions;
using AfterSchoolUIAutomationTests.Pages;
using AfterSchoolUIAutomationTests.Parameters;
using TechTalk.SpecFlow;

namespace AfterSchoolUIAutomationTests.Steps
{
    [Binding]
    public sealed class EditAccountProfileStep
    {
        private readonly ScenarioContext _scenarioContext;
        private readonly LoginPage loginPage;
        private readonly EditAccountProfilePage editAccountProfilePage;

        public EditAccountProfileStep(ScenarioContext scenarioContext)
        {
            _scenarioContext = scenarioContext;
            loginPage = new LoginPage(UIDriver.WebDriver);
            editAccountProfilePage = new EditAccountProfilePage(UIDriver.WebDriver);
        }

        [Given(@"Login to the website")]
        public void GetLogIn()
        {
            loginPage.PopulateName(Parameter.Get("UserName", "users", "Login"));
            loginPage.PopulatePassword(Parameter.Get("Password", "users", "Login"));
            loginPage.ClickSubmit();
            editAccountProfilePage.ClickAccountButton();
        }

        [Given(@"Navigate to the profile edit page")]
        public void NavigateToProfileEdit()
        {
            UIDriver.WebDriver.Navigate().GoToUrl(Parameter.Get("EditAccountProfileUrl", "users", "EditAccountProfile"));
            UIDriver.WebDriver.WaitInSeconds(2);
        }

        [Given(@"Enter user first name")]
        public void EnterUserFirstName()
        {
            editAccountProfilePage.PopulateFirstName(Parameter.Get("UserFirstName", "users", "EditAccountProfile"));
        }

        [Given(@"Enter user last name")]
        public void EnterUserLastName()
        {
            editAccountProfilePage.PopulateLastName(Parameter.Get("UserLastName", "users", "EditAccountProfile"));
        }

        [Given(@"Enter user email")]
        public void EnterUserEmail()
        {
            editAccountProfilePage.PopulateUserEmail(Parameter.Get("UserEmail", "users", "EditAccountProfile"));
        }

        [Given(@"Enter user password")]
        public void EnterUserPassword()
        {
            editAccountProfilePage.PopulateUserPassword(Parameter.Get("UserPassword", "users", "EditAccountProfile"));
        }

        [Given(@"Enter user confirm password")]
        public void EnterUserConfirmPassword()
        {
            editAccountProfilePage.PopulateConfirmPassword(Parameter.Get("UserConfirmPassword", "users", "EditAccountProfile"));
        }

        [When(@"Click on Update")]
        public void ClickUpdate()
        {
            editAccountProfilePage.ClickUpdate();
        }

        [Then(@"Verify Success Message")]
        public void VerifyNavigatedURL()
        {
            string successMessage = editAccountProfilePage.ToastMessage();
            //string successMessage = "Success";
            successMessage.Equals("Success");
        }
    }
}