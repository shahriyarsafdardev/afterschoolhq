using AfterSchoolUIAutomationTests.Drivers;
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
    public sealed class CreateProfileStep
    {
        private readonly ScenarioContext _scenarioContext;
        private IWebDriver webDriver;
        private NewProviderSignUpFlowPage newProviderSignUpFlowPage;
        private CreateProfilePage createProfilePage;

        public CreateProfileStep(ScenarioContext scenarioContext)
        {
            _scenarioContext = scenarioContext;
            webDriver = UIDriver.WebDriver;
            createProfilePage = new CreateProfilePage(webDriver);
            newProviderSignUpFlowPage = new NewProviderSignUpFlowPage(webDriver);
        }

        [Given(@"Go through new provider signup flow")]
        public void NewProviderSignUpFlow()
        {
            webDriver.Navigate().GoToUrl(Parameter.Get("SignUpUrl", "urls"));
            webDriver.WaitInSeconds(3);
            newProviderSignUpFlowPage.ClickSignUp();
            newProviderSignUpFlowPage.ClickIndividualAccount();
            newProviderSignUpFlowPage.ProvideProgramsForStudents();
            newProviderSignUpFlowPage.PopulateProgramName(Parameter.Get("ProgramName", "users" , "AccountSigUp"));
            newProviderSignUpFlowPage.ClickProgramSubmit();
            newProviderSignUpFlowPage.PopulateFirstName(Parameter.Get("FirstName", "users", "AccountSigUp"));
            newProviderSignUpFlowPage.PopulateLasttName(Parameter.Get("LasttName", "users" , "AccountSigUp"));
            newProviderSignUpFlowPage.PopulateEmail("testemail" + DateTime.Now.ToString("yyyyMMddHHmmss") + "@yopmail.com");
            newProviderSignUpFlowPage.PopulateUserPassword(Parameter.Get("UserPassword", "users", "AccountSigUp"));
            newProviderSignUpFlowPage.PopulateUserPassword2(Parameter.Get("UserPassword2", "users", "AccountSigUp"));
            webDriver.WaitInSeconds(10);
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
            createProfilePage.PopulateShortDescription(Parameter.Get("ShortDescription", "users", "AccountSigUp"));
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
            createProfilePage.PopulatePhone(Parameter.Get("Phone", "users" , "AccountSigUp"));
            createProfilePage.SelectAutoComplete(Parameter.Get("AutoComplete", "users", "AccountSigUp"));
            createProfilePage.ContactInfoSubmit();
        }

        [Given(@"Populate customize your profile images fields and click continue")]
        public void PopulateCustomizeYourProfleAndClickContinue()
        {
            //string fileName = "musical-instruments-english.jpg";
            //returns url of main directory which contains "/bin/Debug"
            //var url = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);
            //correction in path to point it in Root directory
            //var mainpath = url.Replace("\\bin\\Debug\\netcoreapp3.1", "") + "\\img\\" + fileName;
            //var imagepath = mainpath.Replace("file:\\", "");
            //createProfilePage.UploadProfileImage(imagepath);
            //createProfilePage.HeaderImageImage(imagepath);
            createProfilePage.CustomizeProfileSubmit();
        }

        [Given(@"Populate finding your activities fields and click continue")]
        public void PopulateParentsFindYourActivityAndClickContinue()
        {
            createProfilePage.PopulateProgramCategories(Parameter.Get("ProgramCategories", "users", "AccountSigUp"));
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
            string expectedUrl = Parameter.Get("ProgramName", "users", "AccountSigUp");
            curentUrl.Contains(Regex.Replace(expectedUrl, @"\s+", ""));
        }
    }
}
