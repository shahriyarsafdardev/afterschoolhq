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
            //ToDo:
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
