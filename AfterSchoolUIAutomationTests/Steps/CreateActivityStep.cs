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
        //private LoginPage loginPage;
        private NewProviderSignUpFlowPage newProviderSignUpFlowPage;
        private CreateProfilePage createProfilePage;
        private CreateActivityPage createActivityPage;

        public CreateActivityStep(ScenarioContext scenarioContext)
        {
            _scenarioContext = scenarioContext;
            webDriver = new ChromeDriver(AppDomain.CurrentDomain.BaseDirectory);
            //loginPage = new LoginPage(webDriver);
            newProviderSignUpFlowPage = new NewProviderSignUpFlowPage(webDriver);
            createProfilePage = new CreateProfilePage(webDriver);
            createActivityPage = new CreateActivityPage(webDriver);
        }

        [Given(@"Go through new provider signup flow for activity creation")]
        public void GoThroughNewProviderSignupFlow()
        {
            webDriver.Navigate().GoToUrl(Parameter.Get("AccountSigUp", "SignUpUrl"));
            webDriver.WaitInSeconds(2);
            webDriver.Manage().Window.Maximize();
            newProviderSignUpFlowPage.ClickSignUp();
            newProviderSignUpFlowPage.ClickIndividualAccount();
            newProviderSignUpFlowPage.ProvideProgramsForStudents();
            newProviderSignUpFlowPage.PopulateProgramName(Parameter.Get("AccountSigUp", "ProgramName"));
            newProviderSignUpFlowPage.ClickProgramSubmit();
            newProviderSignUpFlowPage.PopulateFirstName(Parameter.Get("AccountSigUp", "FirstName"));
            newProviderSignUpFlowPage.PopulateLasttName(Parameter.Get("AccountSigUp", "LasttName"));
            newProviderSignUpFlowPage.PopulateEmail("testemail" + DateTime.Now.ToString("yyyyMMddHHmmss") + "@yopmail.com");
            newProviderSignUpFlowPage.PopulateUserPassword(Parameter.Get("AccountSigUp", "UserPassword"));
            newProviderSignUpFlowPage.PopulateUserPassword2(Parameter.Get("AccountSigUp", "UserPassword2"));
            //newProviderSignUpFlowPage.CheckCaptcha();
            newProviderSignUpFlowPage.ClickBasicInfoSubmit();
        }

        [Given(@"Go through create profile flow for activity creation")]
        public void GoThroughCreateProfileFlow()
        {
            createProfilePage.CreateYourFreeProfile();
            createProfilePage.PopulateShortDescription(Parameter.Get("AccountSigUp", "ShortDescription"));
            createProfilePage.SelectGender();
            createProfilePage.SelectTimeZone();
            createProfilePage.SaveAndContinueSubmit();
            createProfilePage.SelectGradeOrAge();
            createProfilePage.SelectMinGrade();
            createProfilePage.SelectMaxAge();
            createProfilePage.ShareInfoWithParentsSubmit();
            createProfilePage.PopulatePhone(Parameter.Get("AccountSigUp", "Phone"));
            createProfilePage.SelectAutoComplete(Parameter.Get("AccountSigUp", "AutoComplete"));
            createProfilePage.ContactInfoSubmit();
            string fileName = "musical-instruments-english.jpg";
            //returns url of main directory which contains "/bin/Debug"
            var url = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);
            //correction in path to point it in Root directory
            var mainpath = url.Replace("\\bin\\Debug\\netcoreapp3.1", "") + "\\img\\" + fileName;
            var imagepath = mainpath.Replace("file:\\", "");
            createProfilePage.UploadProfileImage(imagepath);
            createProfilePage.HeaderImageImage(imagepath);
            createProfilePage.CustomizeProfileSubmit();
            createProfilePage.PopulateProgramCategories(Parameter.Get("AccountSigUp", "ProgramCategories"));
            createProfilePage.EasyActivitySubmit();
            createProfilePage.EnterLinksSubmit();
            createProfilePage.ClickGoToProfile();
        }
        [Given(@"Close getting started modal")]
        public void CloseGettingStartedModal()
        {
            createActivityPage.CloseGettingStartedModal();
        }
        [Given(@"Click got it button")]
        public void ClickGotItButton()
        {
            createActivityPage.ClickGotItButton();
        }

        //[Given(@"Click on Finish Setup")]
        //public void ClickFinishSetUpButton()
        //{
        //    createActivityPage.ClickFinishSetup();
        //}

        [Given(@"Click on Create First Activity")]
        public void ClickCreateFirstActivityButton()
        {
            createActivityPage.ClickCreateFirstActivity();
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
            createActivityPage.SaveAndReviewButton();
        }

        [Then(@"Verify Test")]
        public void VerifyAccountCreationContent()
        {
            string create = "Before we publish your activity lets confirm your email.";
            create.Equals(createActivityPage.EmailConfirmationModalElement.Text.Trim());
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
