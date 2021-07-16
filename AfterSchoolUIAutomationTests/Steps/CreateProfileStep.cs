using AfterSchoolUIAutomationTests.Helpers.Extensions;
using AfterSchoolUIAutomationTests.Pages;
using AfterSchoolUIAutomationTests.Parameters;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using TechTalk.SpecFlow;

namespace AfterSchoolUIAutomationTests.Steps
{
    [Binding]
    public sealed class CreateProfileStep : IDisposable
    {
        private readonly ScenarioContext _scenarioContext;
        private IWebDriver webDriver;
        private NewProviderSignUpFlowPage newProviderSignUpFlowPage;
        private CreateProfilePage createProfilePage;

        public CreateProfileStep(ScenarioContext scenarioContext)
        {
            _scenarioContext = scenarioContext;
            webDriver = new ChromeDriver(AppDomain.CurrentDomain.BaseDirectory);
            createProfilePage = new CreateProfilePage(webDriver);
            newProviderSignUpFlowPage = new NewProviderSignUpFlowPage(webDriver);
        }

        [Given(@"Go through new provider signup flow")]
        public void NewProviderSignUpFlow()
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

        [Given(@"Click create your free profile button")]
        public void ClickFreeProfile()
        {
            createProfilePage.CreateYourFreeProfile();
        }

        [Given(@"Populate about your organization fields and click continue")]
        public void PoulateOrganizationInfo()
        {
            createProfilePage.PopulateShortDescription(Parameter.Get("AccountSigUp", "ShortDescription"));
            createProfilePage.SelectGender();
            createProfilePage.SelectTimeZone();
            createProfilePage.SaveAndContinueSubmit();
        }

        [Given(@"Populate share your organization info fields with parents and click continue")]
        public void PupulateShareInfoAndPouplateForm()
        {
            createProfilePage.SelectGradeOrAge();
            createProfilePage.SelectMinGrade();
            createProfilePage.SelectMaxAge();
            createProfilePage.ShareInfoWithParentsSubmit();
        }

        [Given(@"Populate your contact info fields and click continue")]
        public void PopulateComtactInfoAndClickContinue()
        {
            createProfilePage.PopulatePhone(Parameter.Get("AccountSigUp", "Phone"));
            createProfilePage.SelectAutoComplete(Parameter.Get("AccountSigUp", "AutoComplete"));
            createProfilePage.ContactInfoSubmit();
        }

        [Given(@"Populate customize your profile images fields and click continue")]
        public void PopulateCustomizeYourProfleAndClickContinue()
        {
            string fileName = "musical-instruments-english.jpg";
            //returns url of main directory which contains "/bin/Debug"
            var url = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);
            //correction in path to point it in Root directory
            var mainpath = url.Replace("\\bin\\Debug\\netcoreapp3.1", "") + "\\img\\" + fileName;
            var imagepath = mainpath.Replace("file:\\", "");
            createProfilePage.UploadProfileImage(imagepath);
            createProfilePage.HeaderImageImage(imagepath);
            createProfilePage.CustomizeProfileSubmit();
        }

        [Given(@"Populate finding your activities fields and click continue")]
        public void PopulateParentsFindYourActivityAndClickContinue()
        {
            createProfilePage.PopulateProgramCategories(Parameter.Get("AccountSigUp", "ProgramCategories"));
            createProfilePage.EasyActivitySubmit();
        }

        [Given(@"Populate enter links fields and click continue")]
        public void PopulateEnterLinksAndContinue()
        {
            createProfilePage.EnterLinksSubmit();
        }

        [When(@"Click go to your profile button")]
        public void ClickOnGoToYourProfile()
        {
            createProfilePage.ClickGoToProfile();
        }

        [Then("Match current window url with the expected url")]
        public void ThenTheResultShouldBe()
        {
            String curentUrl = webDriver.Url;
            string expectedUrl = Parameter.Get("AccountSigUp", "ProgramName");
            curentUrl.Contains(Regex.Replace(expectedUrl, @"\s+", ""));
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
