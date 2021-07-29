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
    public sealed class AcceptPaymentsStep : IDisposable
    {
        private readonly ScenarioContext _scenarioContext;
        private IWebDriver webDriver;
        //private NewProviderSignUpFlowPage newProviderSignUpFlowPage;
        //private CreateProfilePage createProfilePage;
        //private CreateActivityPage createActivityPage;
        //private EmailConfirmationPage emailConfirmationPage;
        private AcceptPaymentsPage acceptPaymentsPage;
        private LoginPage loginPage;

        public AcceptPaymentsStep(ScenarioContext scenarioContext)
        {
            _scenarioContext = scenarioContext;
            webDriver = new ChromeDriver(AppDomain.CurrentDomain.BaseDirectory);
            //newProviderSignUpFlowPage = new NewProviderSignUpFlowPage(webDriver);
            //createProfilePage = new CreateProfilePage(webDriver);
            //createActivityPage = new CreateActivityPage(webDriver);
            //emailConfirmationPage = new EmailConfirmationPage(webDriver);
            acceptPaymentsPage = new AcceptPaymentsPage(webDriver);
            loginPage = new LoginPage(webDriver);
        }

        //[Given(@"Go through new provider signup flow for accept payments")]
        //public void GoThroughNewProviderSignupFlow()
        //{
        //    webDriver.Navigate().GoToUrl(Parameter.Get("AccountSigUp", "SignUpUrl"));
        //    webDriver.WaitInSeconds(2);
        //    webDriver.Manage().Window.Maximize();
        //    newProviderSignUpFlowPage.ClickSignUp();
        //    newProviderSignUpFlowPage.ClickIndividualAccount();
        //    newProviderSignUpFlowPage.ProvideProgramsForStudents();
        //    newProviderSignUpFlowPage.PopulateProgramName(Parameter.Get("AccountSigUp", "ProgramName"));
        //    newProviderSignUpFlowPage.ClickProgramSubmit();
        //    newProviderSignUpFlowPage.PopulateFirstName(Parameter.Get("AccountSigUp", "FirstName"));
        //    newProviderSignUpFlowPage.PopulateLasttName(Parameter.Get("AccountSigUp", "LasttName"));
        //    newProviderSignUpFlowPage.PopulateEmail("testemail" + DateTime.Now.ToString("yyyyMMddHHmmss") + "@yopmail.com");
        //    newProviderSignUpFlowPage.PopulateUserPassword(Parameter.Get("AccountSigUp", "UserPassword"));
        //    newProviderSignUpFlowPage.PopulateUserPassword2(Parameter.Get("AccountSigUp", "UserPassword2"));
        //    //newProviderSignUpFlowPage.CheckCaptcha();
        //    newProviderSignUpFlowPage.ClickBasicInfoSubmit();
        //}

        //[Given(@"Go through create profile flow for accept payments")]
        //public void GoThroughCreateProfileFlow()
        //{
        //    createProfilePage.CreateYourFreeProfile();
        //    createProfilePage.PopulateShortDescription(Parameter.Get("AccountSigUp", "ShortDescription"));
        //    createProfilePage.SelectGender();
        //    createProfilePage.SelectTimeZone();
        //    createProfilePage.SaveAndContinueSubmit();
        //    createProfilePage.SelectGradeOrAge();
        //    createProfilePage.SelectMinGrade();
        //    createProfilePage.SelectMaxAge();
        //    createProfilePage.ShareInfoWithParentsSubmit();
        //    createProfilePage.PopulatePhone(Parameter.Get("AccountSigUp", "Phone"));
        //    createProfilePage.SelectAutoComplete(Parameter.Get("AccountSigUp", "AutoComplete"));
        //    createProfilePage.ContactInfoSubmit();
        //    string fileName = "musical-instruments-english.jpg";
        //    //returns url of main directory which contains "/bin/Debug"
        //    var url = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);
        //    //correction in path to point it in Root directory
        //    var mainpath = url.Replace("\\bin\\Debug\\netcoreapp3.1", "") + "\\img\\" + fileName;
        //    var imagepath = mainpath.Replace("file:\\", "");
        //    createProfilePage.UploadProfileImage(imagepath);
        //    createProfilePage.HeaderImageImage(imagepath);
        //    createProfilePage.CustomizeProfileSubmit();
        //    createProfilePage.PopulateProgramCategories(Parameter.Get("AccountSigUp", "ProgramCategories"));
        //    createProfilePage.EasyActivitySubmit();
        //    createProfilePage.EnterLinksSubmit();
        //    createProfilePage.ClickGoToProfile();
        //}

        //[Given(@"Go through create activity flow for accept payments")]
        //public void GoThroughCreateActivityFlow()
        //{
        //    createActivityPage.CloseGettingStartedModal();
        //    createActivityPage.ClickGotItButton();
        //    createActivityPage.ClickCreateFirstActivity();
        //    createActivityPage.PopulateActivityName(Parameter.Get("CreateNewActivity", "ActivityName"));
        //    createActivityPage.PopulateTotalAvailableSeats(Parameter.Get("CreateNewActivity", "TotalAvailableSeats"));
        //    createActivityPage.ToggleInviteOnly(Parameter.Get("CreateNewActivity", "InviteOnly"));
        //    createActivityPage.SetSignUpPeriodOpen();
        //    createActivityPage.SetSignUpPeriodClose();
        //    createActivityPage.ClickSaveAndNext();
        //    createActivityPage.SelectGender();
        //    createActivityPage.RestrictedBy();
        //    createActivityPage.MinimumGrade();
        //    createActivityPage.MaximumGrade();
        //    createActivityPage.ClickSaveAndNext();
        //    createActivityPage.HasSchedule(Parameter.Get("CreateNewActivity", "HasSchedule"));
        //    //createActivityPage.RepeatsWeekly(Parameter.Get("CreateNewActivity", "IsWeekly"));
        //    createActivityPage.WeekDays();
        //    createActivityPage.SetScheduleStartDate();
        //    createActivityPage.SetScheduleEndDate();
        //    createActivityPage.SetScheduleStartTime();
        //    createActivityPage.SetScheduleEndTime();
        //    createActivityPage.ClickSaveAndNext();
        //    createActivityPage.ActivityDetails(Parameter.Get("CreateNewActivity", "AboutActivity"));
        //    createActivityPage.ClickSaveAndNext();
        //    //createActivityPage.ActivityLedBy();
        //    createActivityPage.ContactName(Parameter.Get("CreateNewActivity", "ContactName"));
        //    createActivityPage.ContactPhone(Parameter.Get("CreateNewActivity", "ContactPhone"));
        //    //createActivityPage.ContactEmail(Parameter.Get("CreateNewActivity", "ContactEmail"));
        //    createActivityPage.ClickSaveAndNext();
        //    createActivityPage.PhysicalLocation();
        //    createActivityPage.SelectAutoComplete(Parameter.Get("CreateNewActivity", "ActivityLocation"));
        //    createActivityPage.ClickSaveAndNext();
        //    createActivityPage.DoParentsPay();
        //    createActivityPage.RegistrationFee(Parameter.Get("CreateNewActivity", "RegistrationFee"));
        //    createActivityPage.DoPriceParentpay();
        //    createActivityPage.ClickSaveAndNext();
        //    createActivityPage.SaveAndReviewButton();
        //}

        //[Given(@"Go through email confirmation flow for accept payments")]
        //public void GoThroughEmailConfirmationFlow()
        //{
        //    emailConfirmationPage.CloseEmailCofirmationModal();
        //    emailConfirmationPage.OpenYopEmailAndPopulateEmailInput();
        //    emailConfirmationPage.ClickYopEmailTextInputBtn();
        //    emailConfirmationPage.ClickConfirmationMailAndLink();
        //}

        [Given(@"Go through login flow for accept payments")]
        public void GoThroughEmailConfirmationFlow()
        {
            webDriver.Navigate().GoToUrl(Parameter.Get("Login", "LoginUrl"));
            webDriver.WaitInSeconds(3);
            webDriver.Manage().Window.Maximize();
            loginPage.PopulateName("testemail20210729182628@yopmail.com");
            loginPage.PopulatePassword("12345678");
            loginPage.ClickSubmit();
        }

        [Given(@"Navigate to the finish setup page")]
        public void NavigateToFinishSetup()
        {
            acceptPaymentsPage.NavigateToFinishSetup("http://localhost:54849/RockClimbing53");
        }

        [Given(@"Click close button of modal")]
        public void CloseModal()
        {
            acceptPaymentsPage.ClickCloseModal();
        }

        [Given(@"Click the got it button")]
        public void ClickGotItButton()
        {
            acceptPaymentsPage.ClickGotItButton();
        }

        [Given(@"Click more dropdown option button")]
        public void MoreDropdownOptions()
        {
            acceptPaymentsPage.ClickMoreDropdownButton();
        }

        [Given(@"Click enable payments button")]
        public void EnablePayments()
        {
            acceptPaymentsPage.ClickEnablePaymentsButton();
        }

        [Given(@"Click choose direct deposits button")]
        public void ChooseDirectDeposit()
        {
            acceptPaymentsPage.ClickChooseDirectDepositButton();
        }

        [Given(@"Populate get paid by after school hq form details and click next")]
        public void PopulateGetPaidByAfterSchoolHQForm()
        {
            acceptPaymentsPage.ClickIndividualSoleProprietorRadio();
            acceptPaymentsPage.ClickTestPhoneNumber();
            acceptPaymentsPage.ClickNextBtnGetPaid();
        }

        [Given(@"Click use test code button")]
        public void UseTestCase()
        {
            acceptPaymentsPage.ClickUseTestCode();
        }

        [Given(@"Populate personal form details and click next")]
        public void PopulatePersonalFormDetailsForm()
        {
            acceptPaymentsPage.PopulateFirstName(Parameter.Get("StripeAccountPayments", "FirstName"));
            acceptPaymentsPage.PopulateLastName(Parameter.Get("StripeAccountPayments", "LastName"));
            acceptPaymentsPage.PopulateDay(Parameter.Get("StripeAccountPayments", "DD"));
            acceptPaymentsPage.PopulateMonth(Parameter.Get("StripeAccountPayments", "MM"));
            acceptPaymentsPage.PopulateYear(Parameter.Get("StripeAccountPayments", "YYYY"));
            acceptPaymentsPage.PopulateAddressLine1(Parameter.Get("StripeAccountPayments", "AddressLine1"));
            acceptPaymentsPage.PopulateAddressLine2(Parameter.Get("StripeAccountPayments", "AddressLine2"));
            acceptPaymentsPage.PopulateCity(Parameter.Get("StripeAccountPayments", "City"));
            acceptPaymentsPage.PopulateState(Parameter.Get("StripeAccountPayments", "State"));
            acceptPaymentsPage.PopulateZip(Parameter.Get("StripeAccountPayments", "ZIP"));
            acceptPaymentsPage.PopulatePhoneNumber(Parameter.Get("StripeAccountPayments", "PhoneNumber"));
            acceptPaymentsPage.PopulateSSN(Parameter.Get("StripeAccountPayments", "SSN"));
            acceptPaymentsPage.ClickNextBtnPersonalDetail();
        }

        [Given(@"Populate bussiness details form and click next")]
        public void PopulateBussinessDetailsForm()
        {
            acceptPaymentsPage.ClickBussinessWebsiteDetails();
            acceptPaymentsPage.ClickNextBtnBussinessDetail();
        }

        [Given(@"Populate select an account for payouts and click next")]
        public void PopulateSelectAccountForPayoutsForm()
        {
            acceptPaymentsPage.ClickUsetestAccount();
        }

        [Given(@"Review your details and click save")]
        public void ReviewYourDetailsAndSave()
        {
            acceptPaymentsPage.ClickReviewYourDetails();
        }

        [Then(@"Verify account success message")]
        public void VerifyAccountSuccessMessage()
        {
            string create = "Account setup complete!";
            create.Equals(acceptPaymentsPage.AccountSetupComplete().Trim());
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
