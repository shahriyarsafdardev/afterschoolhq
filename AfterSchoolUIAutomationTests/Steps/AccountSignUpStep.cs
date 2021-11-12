using AfterSchoolUIAutomationTests.Drivers;
using AfterSchoolUIAutomationTests.Helpers.Extensions;
using AfterSchoolUIAutomationTests.Pages;
using AfterSchoolUIAutomationTests.Parameters;
using FluentAssertions;
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
    public sealed class AccountSignUpStep
    {

        private readonly ScenarioContext _scenarioContext;
        private IWebDriver webDriver;
        private AccountSignUpPage accountSignUpPage;

        public AccountSignUpStep(ScenarioContext scenarioContext)
        {
            _scenarioContext = scenarioContext;
            webDriver = UIDriver.WebDriver;
            accountSignUpPage = new AccountSignUpPage(webDriver);
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
            accountSignUpPage.PopulateProgramName(Parameter.Get("ProgramName", "users", "AccountSigUp"));
            accountSignUpPage.ClickProgramSubmit();
        }

        [Given(@"Poulate firstname, lastname, email, password and click continue")]
        public void PopulateBasicInfo()
        {
            accountSignUpPage.PopulateFirstName(Parameter.Get("FirstName", "users", "AccountSigUp"));
            accountSignUpPage.PopulateLasttName(Parameter.Get("LasttName", "users", "AccountSigUp"));
            accountSignUpPage.PopulateEmail("newemail" + DateTime.Now.ToString("yyyyMMddHHmmss") + "@yopmail.com");
            accountSignUpPage.PopulateUserPassword(Parameter.Get("UserPassword", "users", "AccountSigUp"));
            accountSignUpPage.PopulateUserPassword2(Parameter.Get("UserPassword2", "users", "AccountSigUp"));
            //accountSignUpPage.CheckCaptcha();
            accountSignUpPage.ClickBasicInfoSubmit();
        }

        [Given(@"Click create free profile and populate organization info form and click continue")]
        public void ClickFreeProfileAndPoulateOrganizationInfo()
        {
            accountSignUpPage.CreateYourFreeProfile();
            accountSignUpPage.PopulateShortDescription(Parameter.Get("ShortDescription","users", "AccountSigUp"));
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
            accountSignUpPage.PopulatePhone(Parameter.Get("Phone", "users", "AccountSigUp"));
            accountSignUpPage.SelectAutoComplete(Parameter.Get("AutoComplete", "users", "AccountSigUp"));
            accountSignUpPage.ContactInfoSubmit();
        }

        [Given(@"Populate customize your profile images and click continue")]
        public void PopulateCustomizeYourProfleAndClickContinue()
        {
            string fileName = "musical-instruments-english.jpg";
            //returns url of main directory which contains "/bin/Debug"
            var url = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);
            //correction in path to point it in Root directory
            var mainpath = url.Replace("\\bin\\Debug\\netcoreapp2.1", "") + "\\img\\" + fileName;
            var imagepath = mainpath.Replace("file:\\", "");
            accountSignUpPage.UploadProfileImage(imagepath);
            accountSignUpPage.HeaderImageImage(imagepath);
            accountSignUpPage.CustomizeProfileSubmit();
        }

        [Given(@"Populate parents find your activity easily and click continue")]
        public void PopulateParentsFindYourActivityAndClickContinue()
        {
            accountSignUpPage.PopulateProgramCategories(Parameter.Get("ProgramCategories", "users", "AccountSigUp"));
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
            string successMessage = "Great Job!";
            successMessage.Should().Be(accountSignUpPage.GetH1Text().Trim());
        }
    }
}
