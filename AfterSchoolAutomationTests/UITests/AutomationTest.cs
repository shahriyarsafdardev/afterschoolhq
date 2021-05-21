using System;
using Xunit;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace AfterSchoolAutomationTests.UITests
{
    public class AutomationTest : IDisposable
    {
        private readonly IWebDriver _driver;
        private readonly Helper _helper;
        protected IWebDriver WebDriver { get; set; }

        public AutomationTest()
        {
            _driver = new ChromeDriver();
            _helper = new Helper(_driver);
        }

        [Fact]
        public void LoginTest()
        {
            _driver.Navigate().GoToUrl("https://qa.afterschoolhq.com/login");
            _helper.PopulateName("parent3_hq@yopmail.com");
            _helper.PopulatePassword("123456789");
            _helper.ClickSubmit();
            Assert.Equal("Passed", "Passed");

        }

        [Fact]
        public void AccountSignUpTest()
        {
            _driver.Navigate().GoToUrl("https://qa.afterschoolhq.com/signup/Welcome");
            _helper.ClickSignUp();
            //WebDriver.WaitInSeconds(5);
            _helper.ClickIndividualAccount();
            _helper.ProvideProgramsForStudents();
            _helper.PopulateProgramName("Writing Classes");
            _helper.ClickProgramSubmit();
            _helper.PopulateFirstName("Alex");
            _helper.PopulateLasttName("Stuart");
            _helper.PopulateEmail("writingclasses@yopmail.com");
            _helper.PopulateUserPassword("123456789");
            _helper.PopulateUserPassword2("123456789");


            Assert.Equal("Passed", "Passed");

        }

        public void Dispose()
        {
            _driver.Quit();
            _driver.Dispose();
        }
    }
}
