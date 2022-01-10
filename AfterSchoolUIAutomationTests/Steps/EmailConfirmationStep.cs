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
using AfterSchoolUIAutomationTests.Drivers;

namespace AfterSchoolUIAutomationTests2.Steps
{
    [Binding]
    public sealed class EmailConfirmationStep
    {
        private readonly ScenarioContext _scenarioContext;
        private IWebDriver webDriver;
        //private LoginPage loginPage;
        private NewProviderSignUpFlowPage newProviderSignUpFlowPage;
        private CreateProfilePage createProfilePage;
        private CreateActivityPage createActivityPage;
        private EmailConfirmationPage emailConfirmationPage;

        private string email;
        public string AccountEmail
        {
            get { return email; }
            set { email = value; }
        }

        public EmailConfirmationStep(ScenarioContext scenarioContext)
        {
            _scenarioContext = scenarioContext;
            webDriver = UIDriver.WebDriver;
            //loginPage = new LoginPage(webDriver);
            newProviderSignUpFlowPage = new NewProviderSignUpFlowPage(webDriver);
            createProfilePage = new CreateProfilePage(webDriver);
            createActivityPage = new CreateActivityPage(webDriver);
            emailConfirmationPage = new EmailConfirmationPage(webDriver);
        }

        [Given(@"Go through new provider signup flow for email confirmation")]
        public void GoThroughNewProviderSignupFlow()
        {
            newProviderSignUpFlowPage.NavigateToSignUp(Parameter.Get("SignUpUrl", "urls"));
            newProviderSignUpFlowPage.ClickSignUp();
            newProviderSignUpFlowPage.ClickIndividualAccount();
            newProviderSignUpFlowPage.ProvideProgramsForStudents();
            newProviderSignUpFlowPage.PopulateProgramName(Parameter.Get("ProgramName", "users", "AccountSigUp"));
            newProviderSignUpFlowPage.ClickProgramSubmit();
            newProviderSignUpFlowPage.PopulateFirstName(Parameter.Get("FirstName", "users", "AccountSigUp"));
            newProviderSignUpFlowPage.PopulateLasttName(Parameter.Get("LasttName", "users", "AccountSigUp"));
            AccountEmail = "testemail" + DateTime.Now.ToString("yyyyMMddHHmmss") + "@yopmail.com";
            newProviderSignUpFlowPage.PopulateEmail(AccountEmail);
            newProviderSignUpFlowPage.PopulateUserPassword(Parameter.Get("UserPassword", "users", "AccountSigUp"));
            newProviderSignUpFlowPage.PopulateUserPassword2(Parameter.Get("UserPassword2", "users", "AccountSigUp"));
            //newProviderSignUpFlowPage.CheckCaptcha();
            newProviderSignUpFlowPage.ClickBasicInfoSubmit();
        }

        [Given(@"Go through create profile flow for email confirmation")]
        public void GoThroughCreateProfileFlow()
        {
            createProfilePage.CreateYourFreeProfile();
            createProfilePage.PopulateShortDescription(Parameter.Get("ShortDescription", "users" , "AccountSigUp"));
            createProfilePage.SelectGender();
            createProfilePage.SelectTimeZone();
            createProfilePage.SaveAndContinueSubmit();
            createProfilePage.SelectGradeOrAge();
            createProfilePage.SelectMinGrade();
            createProfilePage.SelectMaxAge();
            createProfilePage.ShareInfoWithParentsSubmit();
            createProfilePage.PopulatePhone(Parameter.Get("Phone", "users", "AccountSigUp"));
            createProfilePage.SelectAutoComplete(Parameter.Get("AutoComplete", "users" , "AccountSigUp"));
            createProfilePage.ContactInfoSubmit();
            //string fileName = "musical-instruments-english.jpg";
            //returns url of main directory which contains "/bin/Debug"
            //var url = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);
            //correction in path to point it in Root directory
            //var mainpath = url.Replace("\\bin\\Debug\\netcoreapp2.1", "") + "\\img\\" + fileName;
            //var imagepath = mainpath.Replace("file:\\", "");
            //createProfilePage.UploadProfileImage(imagepath);
            //createProfilePage.HeaderImageImage(imagepath);
            createProfilePage.CustomizeProfileSubmit();
            createProfilePage.PopulateProgramCategories(Parameter.Get("ProgramCategories", "users", "AccountSigUp"));
            createProfilePage.EasyActivitySubmit();
            createProfilePage.EnterLinksSubmit();
            createProfilePage.ClickGoToProfile();
        }

        [Given(@"Go through create activity flow for email confirmation")]
        public void GoThroughCreateActivityFlow()
        {
            createActivityPage.CloseGettingStartedModal();
            createActivityPage.ClickGotItButton();
            createActivityPage.ClickCreateFirstActivity();
            createActivityPage.PopulateActivityName(Parameter.Get("ActivityName", "users", "CreateNewActivity"));
            createActivityPage.PopulateTotalAvailableSeats(Parameter.Get("TotalAvailableSeats", "users", "CreateNewActivity"));
            createActivityPage.ToggleInviteOnly(Parameter.Get("InviteOnly", "users", "CreateNewActivity"));
            createActivityPage.SetSignUpPeriodOpen();
            createActivityPage.SetSignUpPeriodClose();
            createActivityPage.ClickSaveAndNext();
            createActivityPage.SelectGender();
            createActivityPage.RestrictedBy();
            createActivityPage.MinimumGrade();
            createActivityPage.MaximumGrade();
            createActivityPage.ClickSaveAndNext();
            createActivityPage.HasSchedule(Parameter.Get("HasSchedule", "users", "CreateNewActivity"));
            //createActivityPage.RepeatsWeekly(Parameter.Get("CreateNewActivity", "IsWeekly"));
            createActivityPage.WeekDays();
            createActivityPage.SetScheduleStartDate();
            createActivityPage.SetScheduleEndDate();
            createActivityPage.SetScheduleStartTime();
            createActivityPage.SetScheduleEndTime();
            createActivityPage.ClickSaveAndNext();
            createActivityPage.ActivityDetails(Parameter.Get("AboutActivity", "users", "CreateNewActivity"));
            createActivityPage.ClickSaveAndNext();
            //createActivityPage.ActivityLedBy();
            createActivityPage.ContactName(Parameter.Get("ContactName","users", "CreateNewActivity"));
            createActivityPage.ContactPhone(Parameter.Get("ContactPhone","users" ,"CreateNewActivity"));
            //createActivityPage.ContactEmail(Parameter.Get("CreateNewActivity", "ContactEmail"));
            createActivityPage.ClickSaveAndNext();
            createActivityPage.PhysicalLocation();
            createActivityPage.SelectAutoComplete(Parameter.Get("ActivityLocation", "users", "CreateNewActivity"));
            createActivityPage.ClickSaveAndNext();
            createActivityPage.DoParentsPay();
            createActivityPage.RegistrationFee(Parameter.Get("RegistrationFee", "users" ,"CreateNewActivity"));
            createActivityPage.DoPriceParentpay();
            createActivityPage.ClickSaveAndNext();
            createActivityPage.SaveAndReviewButton();
        }

        [Given(@"Close the email confirmation modal by clicking close button")]
        public void CloseEmailModal()
        {
            emailConfirmationPage.CloseEmailCofirmationModal();
        }

        [Given(@"Open yopmail and populate email input box")]
        public void OpenYopEmailAndPopulateEmailInput()
        {
            emailConfirmationPage.OpenYopEmailAndPopulateEmailInput();
        }

        [Given(@"Click yopmail text input button")]
        public void ClickYopEmailTextInputBtn()
        {
            emailConfirmationPage.ClickYopEmailTextInputBtn();
        }

        [Given(@"Click confirmation email and link")]
        public void ClickConfirmationMailAndLink()
        {
            emailConfirmationPage.ClickConfirmationMailAndLink();
        }

        [Then(@"Verify the email verification message")]
        public void VerifyAccountCreationContent()
        {
            string create = "Thank you! Your email has been verified.";
            create.Contains(emailConfirmationPage.EmailConfirmationComplete());
        }
    }
}
