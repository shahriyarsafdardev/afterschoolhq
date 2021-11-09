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
using FluentAssertions;
using AfterSchoolUIAutomationTests.Drivers;

namespace AfterSchoolUIAutomationTests.Steps
{
    [Binding]
    public sealed class CreateActivityStep
    {
        private readonly ScenarioContext _scenarioContext;
        private IWebDriver webDriver;
        private NewProviderSignUpFlowPage newProviderSignUpFlowPage;
        private CreateProfilePage createProfilePage;
        private CreateActivityPage createActivityPage;

        public CreateActivityStep(ScenarioContext scenarioContext)
        {
            _scenarioContext = scenarioContext;
            webDriver = UIDriver.WebDriver;
            newProviderSignUpFlowPage = new NewProviderSignUpFlowPage(webDriver);
            createProfilePage = new CreateProfilePage(webDriver);
            createActivityPage = new CreateActivityPage(webDriver);
        }

        [Given(@"Open the browser and navigate to AfterSchool home page")]
        public void NavigateToHomePage()
        {
            webDriver.Navigate().GoToUrl(Parameter.Get("HomePageUrl", "urls"));
            webDriver.WaitInSeconds(3);
            webDriver.Manage().Window.Maximize();
            webDriver.Navigate().GoToUrl(Parameter.Get("SignUpUrl", "urls"));
            webDriver.WaitInSeconds(3);
        }

        [Given(@"Go through new provider signup flow for activity creation")]
        public void GoThroughNewProviderSignupFlow()
        {
            newProviderSignUpFlowPage.ClickSignUp();
            newProviderSignUpFlowPage.ClickIndividualAccount();
            newProviderSignUpFlowPage.ProvideProgramsForStudents();
            newProviderSignUpFlowPage.PopulateProgramName(Parameter.Get("ProgramName", "users" , "AccountSigUp" ));
            newProviderSignUpFlowPage.ClickProgramSubmit();
            newProviderSignUpFlowPage.PopulateFirstName(Parameter.Get("FirstName", "users", "AccountSigUp"));
            newProviderSignUpFlowPage.PopulateLasttName(Parameter.Get("LasttName", "users", "AccountSigUp"));
            newProviderSignUpFlowPage.PopulateEmail("testemail" + DateTime.Now.ToString("yyyyMMddHHmmss") + "@yopmail.com");
            newProviderSignUpFlowPage.PopulateUserPassword(Parameter.Get("UserPassword", "users", "AccountSigUp"));
            newProviderSignUpFlowPage.PopulateUserPassword2(Parameter.Get("UserPassword2" , "users" , "AccountSigUp"));
            //newProviderSignUpFlowPage.CheckCaptcha();
            newProviderSignUpFlowPage.ClickBasicInfoSubmit();
        }

        [Given(@"Go through create profile flow for activity creation")]
        public void GoThroughCreateProfileFlow()
        {
            createProfilePage.CreateYourFreeProfile();
            createProfilePage.PopulateShortDescription(Parameter.Get("ShortDescription","users", "AccountSigUp"));
            createProfilePage.SelectGender();
            createProfilePage.SelectTimeZone();
            createProfilePage.SaveAndContinueSubmit();
            createProfilePage.SelectGradeOrAge();
            createProfilePage.SelectMinGrade();
            createProfilePage.SelectMaxAge();
            createProfilePage.ShareInfoWithParentsSubmit();
            createProfilePage.PopulatePhone(Parameter.Get("Phone", "users", "AccountSigUp"));
            createProfilePage.SelectAutoComplete(Parameter.Get("AutoComplete" , "users" , "AccountSigUp"));
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
            createProfilePage.PopulateProgramCategories(Parameter.Get("ProgramCategories", "users" , "AccountSigUp"));
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
            createActivityPage.PopulateActivityName(Parameter.Get("ActivityName", "users" , "CreateNewActivity"));
            createActivityPage.PopulateTotalAvailableSeats(Parameter.Get("TotalAvailableSeats", "users" , "CreateNewActivity"));
            createActivityPage.ToggleInviteOnly(Parameter.Get("InviteOnly" , "users" , "CreateNewActivity"));
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
            createActivityPage.HasSchedule(Parameter.Get("HasSchedule", "users" , "CreateNewActivity"));
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
            createActivityPage.ActivityDetails(Parameter.Get("AboutActivity", "users" , "CreateNewActivity"));
            createActivityPage.ClickSaveAndNext();
        }

        [Given(@"Populate contact section and save")]
        public void PopulateContact()
        {
            //createActivityPage.ActivityLedBy();
            createActivityPage.ContactName(Parameter.Get("ContactName", "users" , "CreateNewActivity"));
            createActivityPage.ContactPhone(Parameter.Get("ContactPhone" , "users" , "CreateNewActivity"));
            createActivityPage.ContactEmail(Parameter.Get("ContactEmail", "users" , "CreateNewActivity"));
            createActivityPage.ClickSaveAndNext();
        }

        [Given(@"Populate location section and save")]
        public void PopulateLocation()
        {
            createActivityPage.PhysicalLocation();
            createActivityPage.SelectAutoComplete(Parameter.Get("ActivityLocation" , "users" , "CreateNewActivity"));
            createActivityPage.ClickSaveAndNext();
        }

        [Given(@"Populate price and payment section and save")]
        public void PopulatePriceAndPayment()
        {
            createActivityPage.DoParentsPay();
            createActivityPage.RegistrationFee(Parameter.Get("RegistrationFee" , "users" , "CreateNewActivity"));
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
            create.Should().Be(createActivityPage.EmailConfirmationModalElement.Text.Trim());
        }
    }
}
