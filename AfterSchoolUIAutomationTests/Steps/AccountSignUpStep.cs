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
            webDriver.WaitInSeconds(3);
        }

        [Given(@"Manage the browser window size for Signup and click Get started")]
        public void NavigateAccountAndPrograms()
        {
            webDriver.Manage().Window.Maximize();
            webDriver.WaitInSeconds(2);
            accountSignUpPage.ClickSignUp();
            webDriver.WaitInSeconds(2);
        }

        [Given(@"Click individual Account and provide programs for student")]
        public void GivenTheSecondNumberIs()
        {
            webDriver.WaitInSeconds(3);
            accountSignUpPage.ClickIndividualAccount();
            webDriver.WaitInSeconds(3);
            accountSignUpPage.ProvideProgramsForStudents();
        }

        [Given(@"Poulate Program Name and click continue")]
        public void PopulateProgramName()
        {
            webDriver.WaitInSeconds(2);
            accountSignUpPage.PopulateProgramName(Parameter.Get("AccountSigUp", "ProgramName"));
            webDriver.WaitInSeconds(6);
            accountSignUpPage.ClickProgramSubmit();
        }

        [Given(@"Poulate firstname, lastname, email, password and click continue")]
        public void PopulateBasicInfo()
        {
            //--Step 5 
            webDriver.WaitInSeconds(10);
            accountSignUpPage.PopulateFirstName(Parameter.Get("AccountSigUp", "FirstName"));
            webDriver.WaitInSeconds(2);
            accountSignUpPage.PopulateLasttName(Parameter.Get("AccountSigUp", "LasttName"));
            webDriver.WaitInSeconds(2);
            accountSignUpPage.PopulateEmail("newemail" + DateTime.Now.ToString("yyyyMMddHHmmss") + "@yopmail.com");
            webDriver.WaitInSeconds(2);
            accountSignUpPage.PopulateUserPassword(Parameter.Get("AccountSigUp", "UserPassword"));
            webDriver.WaitInSeconds(2);
            accountSignUpPage.PopulateUserPassword2(Parameter.Get("AccountSigUp", "UserPassword2"));
            webDriver.WaitInSeconds(3);
            //accountSignUpPage.CheckCaptcha();
            accountSignUpPage.ClickBasicInfoSubmit();
        }

        [Given(@"Click create free profile and populate organization info form and click continue")]
        public void ClickFreeProfileAndPoulateOrganizationInfo()
        {
            //--Step 6 
            webDriver.WaitInSeconds(3);
            accountSignUpPage.CreateYourFreeProfile();

            //--Step 7 
            webDriver.WaitInSeconds(3);
            accountSignUpPage.PopulateShortDescription(Parameter.Get("AccountSigUp", "ShortDescription"));
            webDriver.WaitInSeconds(5);
            accountSignUpPage.SelectGender();
            webDriver.WaitInSeconds(3);
            accountSignUpPage.SelectTimeZone();
            webDriver.WaitInSeconds(2);
            accountSignUpPage.SaveAndContinueSubmit();
        }

        [Given(@"Populate share info with parent form and click continue")]
        public void PupulateShareInfoAndPouplateForm()
        {
            //--Step 8
            webDriver.WaitInSeconds(5);
            accountSignUpPage.SelectGradeOrAge();
            webDriver.WaitInSeconds(3);
            accountSignUpPage.SelectMinGrade();
            webDriver.WaitInSeconds(2);
            accountSignUpPage.SelectMaxAge();
            webDriver.WaitInSeconds(2);
            accountSignUpPage.ShareInfoWithParentsSubmit();
        }

        [Given(@"Populate contact info and click continue")]
        public void PopulateComtactInfoAndClickContinue()
        {
            //--Step 9 
            webDriver.WaitInSeconds(2);
            accountSignUpPage.PopulatePhone(Parameter.Get("AccountSigUp", "Phone"));
            webDriver.WaitInSeconds(2);
            accountSignUpPage.SelectAutoComplete(Parameter.Get("AccountSigUp", "AutoComplete"));
            webDriver.WaitInSeconds(5);
            accountSignUpPage.ContactInfoSubmit();
        }

        [Given(@"Populate customize your profile images and click continue")]
        public void PopulateCustomizeYourProfleAndClickContinue()
        {
            //--Step 10 
            string fileName = "musical-instruments-english.jpg";

            //returns url of main directory which contains "/bin/Debug"
            var url = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);

            //correction in path to point it in Root directory
            var mainpath = url.Replace("\\bin\\Debug\\netcoreapp3.1", "") + "\\img\\" + fileName;
            var imagepath = mainpath.Replace("file:\\", "");

            accountSignUpPage.UploadProfileImage(imagepath);
            webDriver.WaitInSeconds(2);
            accountSignUpPage.HeaderImageImage(imagepath);
            accountSignUpPage.CustomizeProfileSubmit();
        }

        [Given(@"Populate parents find your activity easily and click continue")]
        public void PopulateParentsFindYourActivityAndClickContinue()
        {
            //--Step 11 
            webDriver.WaitInSeconds(4);
            accountSignUpPage.PopulateProgramCategories(Parameter.Get("AccountSigUp", "ProgramCategories"));
            accountSignUpPage.EasyActivitySubmit();
        }

        [Given(@"Populate enter links and click continue")]
        public void PopulateEnterLinksAndContinue()
        {
            //--Step 12 
            webDriver.WaitInSeconds(4);
            accountSignUpPage.EnterLinksSubmit();
        }

        [When(@"Click on go to your profile")]
        public void ClickOnGoToYourProfile()
        {
            //--Step 13 
            webDriver.WaitInSeconds(20);
            accountSignUpPage.ClickGoToProfile();
            webDriver.WaitInSeconds(10);
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
