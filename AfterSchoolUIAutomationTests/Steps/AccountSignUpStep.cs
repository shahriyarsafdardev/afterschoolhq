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
using TechTalk.SpecFlow;

namespace AfterSchoolUIAutomationTests.Steps
{
    [Binding]
    public sealed class AccountSignUpStep : IDisposable
    {

        private readonly ScenarioContext _scenarioContext;
        private IWebDriver webDriver;
        private AccountSignUpPage accountSignUpPage;

        public AccountSignUpStep(ScenarioContext scenarioContext)
        {
            _scenarioContext = scenarioContext;
            webDriver = new ChromeDriver(AppDomain.CurrentDomain.BaseDirectory);
            accountSignUpPage = new AccountSignUpPage(webDriver);
        }

        [Given(@"Open the browser and navigate to the Signup Page")]
        public void NavigateToSignup()
        {
            webDriver.Navigate().GoToUrl(Parameter.Get("AccountSigUp", "SignUpUrl"));
            webDriver.WaitInSeconds(2);
        }

        [Given(@"Manage the browser window size for Signup and click Get started")]
        public void NavigateAccountAndPrograms()
        {
            webDriver.Manage().Window.Maximize();
            accountSignUpPage.ClickSignUp();
        }

        [Given(@"Click individual Account and provide programs for student")]
        public void GivenTheSecondNumberIs()
        {   
            accountSignUpPage.ClickIndividualAccount();
            accountSignUpPage.ProvideProgramsForStudents();
        }

        [Given(@"Poulate Program Name and click continue")]
        public void PopulateProgramName()
        {
            accountSignUpPage.PopulateProgramName(Parameter.Get("AccountSigUp", "ProgramName"));
            accountSignUpPage.ClickProgramSubmit();
        }

        [Given(@"Poulate firstname, lastname, email, password and click continue")]
        public void PopulateBasicInfo()
        {
            accountSignUpPage.PopulateFirstName(Parameter.Get("AccountSigUp", "FirstName"));
            accountSignUpPage.PopulateLasttName(Parameter.Get("AccountSigUp", "LasttName"));
            accountSignUpPage.PopulateEmail("newemail" + DateTime.Now.ToString("yyyyMMddHHmmss") + "@yopmail.com");
            accountSignUpPage.PopulateUserPassword(Parameter.Get("AccountSigUp", "UserPassword"));
            accountSignUpPage.PopulateUserPassword2(Parameter.Get("AccountSigUp", "UserPassword2"));
            //accountSignUpPage.CheckCaptcha();
            accountSignUpPage.ClickBasicInfoSubmit();
        }

        [Given(@"Click create free profile and populate organization info form and click continue")]
        public void ClickFreeProfileAndPoulateOrganizationInfo()
        {
            accountSignUpPage.CreateYourFreeProfile();
            accountSignUpPage.PopulateShortDescription(Parameter.Get("AccountSigUp", "ShortDescription"));
            accountSignUpPage.SelectGender();
            accountSignUpPage.SelectTimeZone();
            accountSignUpPage.SaveAndContinueSubmit();
        }

        [Given(@"Populate share info with parent form and click continue")]
        public void PupulateShareInfoAndPouplateForm()
        {
            accountSignUpPage.SelectGradeOrAge();
            accountSignUpPage.SelectMinGrade();
            accountSignUpPage.SelectMaxAge();
            accountSignUpPage.ShareInfoWithParentsSubmit();
        }

        [Given(@"Populate contact info and click continue")]
        public void PopulateComtactInfoAndClickContinue()
        {
            accountSignUpPage.PopulatePhone(Parameter.Get("AccountSigUp", "Phone"));
            accountSignUpPage.SelectAutoComplete(Parameter.Get("AccountSigUp", "AutoComplete"));
            accountSignUpPage.ContactInfoSubmit();
        }

        [Given(@"Populate customize your profile images and click continue")]
        public void PopulateCustomizeYourProfleAndClickContinue()
        {
            string fileName = "musical-instruments-english.jpg";
            //returns url of main directory which contains "/bin/Debug"
            var url = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);
            //correction in path to point it in Root directory
            var mainpath = url.Replace("\\bin\\Debug\\netcoreapp3.1", "") + "\\img\\" + fileName;
            var imagepath = mainpath.Replace("file:\\", "");
            accountSignUpPage.UploadProfileImage(imagepath);
            accountSignUpPage.HeaderImageImage(imagepath);
            accountSignUpPage.CustomizeProfileSubmit();
        }

        [Given(@"Populate parents find your activity easily and click continue")]
        public void PopulateParentsFindYourActivityAndClickContinue()
        {
            accountSignUpPage.PopulateProgramCategories(Parameter.Get("AccountSigUp", "ProgramCategories"));
            accountSignUpPage.EasyActivitySubmit();
        }

        [Given(@"Populate enter links and click continue")]
        public void PopulateEnterLinksAndContinue()
        {
            accountSignUpPage.EnterLinksSubmit();
        }

        [When(@"Click on go to your profile")]
        public void ClickOnGoToYourProfile()
        {
            accountSignUpPage.ClickGoToProfile();
        }

        [Then("Match the current url with the expected url")]
        public void ThenTheResultShouldBe()
        {
            String text1 = accountSignUpPage.GetH1Text();
            string text2 = "Great Job!";
            text1.Equals(text2);
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
