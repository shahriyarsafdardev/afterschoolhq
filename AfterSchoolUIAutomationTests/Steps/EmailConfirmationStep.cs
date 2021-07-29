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

namespace AfterSchoolUIAutomationTests2.Steps
{
    [Binding]
    public sealed class EmailConfirmationStep : IDisposable
    {
        private readonly ScenarioContext _scenarioContext;
        private IWebDriver webDriver;
        //private LoginPage loginPage;
        private NewProviderSignUpFlowPage newProviderSignUpFlowPage;
        private CreateProfilePage createProfilePage;
        private CreateActivityPage createActivityPage;
        private EmailConfirmationPage emailConfirmationPage;

        public EmailConfirmationStep(ScenarioContext scenarioContext)
        {
            _scenarioContext = scenarioContext;
            webDriver = new ChromeDriver(AppDomain.CurrentDomain.BaseDirectory);
            //loginPage = new LoginPage(webDriver);
            newProviderSignUpFlowPage = new NewProviderSignUpFlowPage(webDriver);
            createProfilePage = new CreateProfilePage(webDriver);
            createActivityPage = new CreateActivityPage(webDriver);
            emailConfirmationPage = new EmailConfirmationPage(webDriver);
        }

        [Given(@"Go through new provider signup flow for email confirmation")]
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

        [Given(@"Go through create profile flow for email confirmation")]
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

        [Given(@"Go through create activity flow for email confirmation")]
        public void GoThroughCreateActivityFlow()
        {
            createActivityPage.CloseGettingStartedModal();
            createActivityPage.ClickGotItButton();
            createActivityPage.ClickCreateFirstActivity();
            createActivityPage.PopulateActivityName(Parameter.Get("CreateNewActivity", "ActivityName"));
            createActivityPage.PopulateTotalAvailableSeats(Parameter.Get("CreateNewActivity", "TotalAvailableSeats"));
            createActivityPage.ToggleInviteOnly(Parameter.Get("CreateNewActivity", "InviteOnly"));
            createActivityPage.SetSignUpPeriodOpen();
            createActivityPage.SetSignUpPeriodClose();
            createActivityPage.ClickSaveAndNext();
            createActivityPage.SelectGender();
            createActivityPage.RestrictedBy();
            createActivityPage.MinimumGrade();
            createActivityPage.MaximumGrade();
            createActivityPage.ClickSaveAndNext();
            createActivityPage.HasSchedule(Parameter.Get("CreateNewActivity", "HasSchedule"));
            //createActivityPage.RepeatsWeekly(Parameter.Get("CreateNewActivity", "IsWeekly"));
            createActivityPage.WeekDays();
            createActivityPage.SetScheduleStartDate();
            createActivityPage.SetScheduleEndDate();
            createActivityPage.SetScheduleStartTime();
            createActivityPage.SetScheduleEndTime();
            createActivityPage.ClickSaveAndNext();
            createActivityPage.ActivityDetails(Parameter.Get("CreateNewActivity", "AboutActivity"));
            createActivityPage.ClickSaveAndNext();
            //createActivityPage.ActivityLedBy();
            createActivityPage.ContactName(Parameter.Get("CreateNewActivity", "ContactName"));
            createActivityPage.ContactPhone(Parameter.Get("CreateNewActivity", "ContactPhone"));
            //createActivityPage.ContactEmail(Parameter.Get("CreateNewActivity", "ContactEmail"));
            createActivityPage.ClickSaveAndNext();
            createActivityPage.PhysicalLocation();
            createActivityPage.SelectAutoComplete(Parameter.Get("CreateNewActivity", "ActivityLocation"));
            createActivityPage.ClickSaveAndNext();
            createActivityPage.DoParentsPay();
            createActivityPage.RegistrationFee(Parameter.Get("CreateNewActivity", "RegistrationFee"));
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
