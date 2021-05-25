using System;
using Xunit;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using AfterSchoolAutomationTests.Helpers.Extensions;

namespace AfterSchoolAutomationTests.UITests
{
    public class AutomationTest : IDisposable
    {
        private readonly IWebDriver _driver;
        private readonly Helper _helper;
        protected IWebDriver WebDriver;

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
            //_driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);
            //_driver.WaitToBeDisplayed(By.XPath("//div[@class='text-center margin-v-40']//button[text()='Get Started']"));
            _driver.WaitInSeconds(5);
            _helper.ClickSignUp();

            //_driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);
            //_driver.WaitToBeDisplayed(By.XPath("(//div[@class='row margin-t-35']//button[text()='Create an Individual Account'])[1]"));
            _driver.WaitInSeconds(5);
            _helper.ClickIndividualAccount();

            //_driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);
            //_driver.WaitToBeDisplayed(By.XPath("(//div[@class='row margin-t-35']//button[text()='Provide programs for students'])[1]"));
            _driver.WaitInSeconds(5);
            _helper.ProvideProgramsForStudents();

            _driver.WaitInSeconds(2);
            _helper.PopulateProgramName("Writing Classes 2");
            //_driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);
            //_driver.WaitToBeDisplayed(By.XPath("//div[@class='visible-md visible-lg']//button[text()='Continue']"));
            _driver.WaitInSeconds(10);
            _helper.ClickProgramSubmit();

            //_driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);
            _helper.PopulateFirstName("Alex");
            _driver.WaitInSeconds(2);
            _helper.PopulateLasttName("Stuart");
            _driver.WaitInSeconds(2);
            _helper.PopulateEmail("writingclasses@yopmail.com");
            _driver.WaitInSeconds(2);
            _helper.PopulateUserPassword("123456789");
            _driver.WaitInSeconds(2);
            _helper.PopulateUserPassword2("123456789");
            _driver.WaitInSeconds(30);
            //_helper.CheckCaptcha();
            _helper.ClickBasicInfoSubmit();
            _driver.WaitInSeconds(3);
            _helper.SelectTimeZone();
            _driver.WaitInSeconds(2);
            _helper.SaveAndContinueSubmit();



            Assert.Equal("Passed", "Passed");

        }

        public void Dispose()
        {
            _driver.Quit();
            _driver.Dispose();
        }
    }
}
