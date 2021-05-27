using System;
using Xunit;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using AfterSchoolAutomationTests.Helpers.Extensions;
using System.IO;
using System.Reflection;
using System.Net.Mail;

namespace AfterSchoolAutomationTests.UITests
{
    public class AutomationTest : IDisposable
    {
        private readonly IWebDriver _driver;
        private readonly Helper _helper;

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
            //--Step 1 --Welcome Get Started--
            _driver.Navigate().GoToUrl("http://localhost:54849/signup/Welcome#");
            //_driver.Navigate().GoToUrl("https://qa.afterschoolhq.com/signup/Welcome");
            _driver.Manage().Window.Maximize();
            //_driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);
            //_driver.WaitToBeDisplayed(By.XPath("//div[@class='text-center margin-v-40']//button[text()='Get Started']"));
            _driver.WaitInSeconds(3);
            _helper.ClickSignUp();

            //--Step 2 --Create Individual Account--
            _driver.WaitInSeconds(3);
            _helper.ClickIndividualAccount();

            //--Step 3 --Provide Programs for Students--
            _driver.WaitInSeconds(3);
            _helper.ProvideProgramsForStudents();

            //--Step 4 --Activity Name--
            _driver.WaitInSeconds(2);
            _helper.PopulateProgramName("Writing Classes 3");
            _driver.WaitInSeconds(10);
            _helper.ClickProgramSubmit();

            //--Step 5 
            _helper.PopulateFirstName("Alex");
            _driver.WaitInSeconds(2);
            _helper.PopulateLasttName("Stuart");
            _driver.WaitInSeconds(2);
            _helper.PopulateEmail("testemail"+ GetTimestamp(DateTime.Now) + "@yopmail.com");
            _driver.WaitInSeconds(2);
            _helper.PopulateUserPassword("12345678");
            _driver.WaitInSeconds(2);
            _helper.PopulateUserPassword2("12345678");
            _driver.WaitInSeconds(3);
            //_helper.CheckCaptcha();
            _helper.ClickBasicInfoSubmit();

            //--Step 6 
            _driver.WaitInSeconds(3);
            _helper.CreateYourFreeProfile();

            //--Step 7 
            _driver.WaitInSeconds(3);
            _helper.PopulateShortDescription("Test Short Desc.");
            _driver.WaitInSeconds(5);
            _helper.SelectGender();
            _driver.WaitInSeconds(3);
            _helper.SelectTimeZone();
            _driver.WaitInSeconds(2);
            _helper.SaveAndContinueSubmit();

            //--Step 8 
            _driver.WaitInSeconds(5);
            _helper.SelectGradeOrAge();
            _driver.WaitInSeconds(3);
            _helper.SelectMinGrade();
            _driver.WaitInSeconds(2);
            _helper.SelectMaxAge();
            _driver.WaitInSeconds(2);
            _helper.ShareInfoWithParentsSubmit();

            //--Step 9 
            _driver.WaitInSeconds(2);
            _helper.PopulatePhone("(202) 555 0155");
            _driver.WaitInSeconds(2);
            //_helper.PopulateLocation("New York University, New York, NY, USA");
            //_driver.WaitInSeconds(5);
            _helper.SelectAutoComplete("New York, New York, USA");
            _driver.WaitInSeconds(5);
            _helper.ContactInfoSubmit();

            //--Step 10 
            //var outPutDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase);
            //var profileImage = Path.Combine(outPutDirectory, "img\\musical-instruments-english.jpg");
            //string relLogo = new Uri(profileImage).LocalPath;
            //var logoImage = new LinkedResource(relLogo);

            //string fileName = "musical-instruments-english.jpg";
            //string path = Path.Combine(Environment.CurrentDirectory, @"img\", fileName);

            _helper.UploadProfileImage(@"D:\AfterSchool\AfterSchool\img\musical-instruments-english.jpg");
            _driver.WaitInSeconds(2);
            _helper.HeaderImageImage(@"D:\AfterSchool\AfterSchool\img\musical-instruments-english.jpg");
            _helper.CustomizeProfileSubmit();

            //--Step 11 
            _driver.WaitInSeconds(4);
            _helper.PopulateProgramCategories("Karate, Sports, Leadership");
            _helper.EasyActivitySubmit();

            //--Step 12 
            _driver.WaitInSeconds(4);
            _helper.EnterLinksSubmit();

            //--Step 13 
            _driver.WaitInSeconds(20);
            _helper.ClickGoToProfile();
            _driver.WaitInSeconds(10);

            Assert.Equal("Passed", "Passed");

        }

        public void Dispose()
        {
            _driver.Quit();
            _driver.Dispose();
        }

        public static String GetTimestamp(DateTime value)
        {
            return value.ToString("yyyyMMddHHmmssffff");
        }
    }
}
