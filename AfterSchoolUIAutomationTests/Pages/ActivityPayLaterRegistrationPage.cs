using System;
using System.Threading;
using AfterSchoolUIAutomationTests.Helpers.Extensions;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System.Text;

namespace AfterSchoolUIAutomationTests.Pages
{
    public class ActivityPayLaterRegistrationPage : IDisposable
    {
        private IWebDriver webDriver;
        public ActivityPayLaterRegistrationPage(IWebDriver WebDriver)
        {
            webDriver = WebDriver;
        }

        //------------Finding ActivityPayLaterRegistrationPage WebElements-----------//

        private IWebElement RegisterNowElement => webDriver.FindElement(By.XPath("(//div[@class='col-lg-4 col-md-12'])[1]//a"));
        private IWebElement ParentEmailElement => webDriver.FindElement(By.Id("email-check"));
        private IWebElement EmailClickNextElement => webDriver.FindElement(By.Id("btn-check-email"));
        private IWebElement ParentEmailPasswordElement => webDriver.FindElement(By.Id("lm-login-password"));
        private IWebElement LoginButtonElement => webDriver.FindElement(By.XPath("//button[@id='btn-login']"));
        private IWebElement AddNewChildButtonElement => webDriver.FindElement(By.XPath("//div[@class='panel-body']//p//a"));
        private IWebElement FirstNameElement => webDriver.FindElement(By.Id("question-14505"));
        private IWebElement StudentDOBElement => webDriver.FindElement(By.Id("question-14511"));
        private IWebElement StudentDOBDateElement => webDriver.FindElement(By.XPath("//div[@class='datetimepicker-days']//table/tbody/tr[3]/td[3]"));
        private IWebElement StudentDOBYearSelectionElement => webDriver.FindElement(By.XPath("(//div[@class='datetimepicker-days']//table//thead//tr)[1]//th[2]"));
        private IWebElement StudentDOBYearBackElement => webDriver.FindElement(By.XPath("(//div[@class='datetimepicker-months']//table//thead//tr)[1]//th[1]"));
        private IWebElement StudentDOBMonthElement => webDriver.FindElement(By.XPath("(//div[@class='datetimepicker-months']//table//tbody//tr)[1]//td[1]//span[6]"));
        private IWebElement StudentDOBDayElement => webDriver.FindElement(By.XPath("//div[@class='datetimepicker-days']//table/tbody/tr[3]/td[3]"));
        private IWebElement StudentGradeElement => webDriver.FindElement(By.Id("question-14512"));
        private IWebElement StudentGenderElement => webDriver.FindElement(By.XPath("(//div[@class='am-radio inline'])[1]//label"));
        private IWebElement StudentSchoolElement => webDriver.FindElement(By.Id("location"));



        private IWebElement LastNameElement => webDriver.FindElement(By.Id("question-14506"));
        private IWebElement PhoneElement => webDriver.FindElement(By.Id("question-9490"));



        public ActivityPayLaterRegistrationPage ClickRegisterNow()
        {
            RegisterNowElement.Click();
            webDriver.WaitInSeconds(5);
            return this;
        }
        public ActivityPayLaterRegistrationPage PopulateEmail(string email)
        {
            ParentEmailElement.SendKeys(email);
            webDriver.WaitInSeconds(2);
            return this;
        }
        public ActivityPayLaterRegistrationPage ClickNextBtn()
        {
            EmailClickNextElement.Click();
            webDriver.WaitInSeconds(4);
            return this;
        }
        public ActivityPayLaterRegistrationPage PopulatePassword(string password)
        {
            ParentEmailPasswordElement.Clear();
            ParentEmailPasswordElement.SendKeys(password);
            webDriver.WaitInSeconds(2);
            return this;
        }
        public ActivityPayLaterRegistrationPage ClickLoginBtn()
        {
            LoginButtonElement.Click();
            webDriver.WaitInSeconds(12);
            return this;
        }
        public ActivityPayLaterRegistrationPage ClickAddNewChildBtn()
        {
            AddNewChildButtonElement.Click();
            webDriver.WaitInSeconds(4);
            return this;
        }
        public ActivityPayLaterRegistrationPage PopulateStudentFirstname(string firstName)
        {
            FirstNameElement.SendKeys(firstName);
            webDriver.WaitInSeconds(2);
            return this;
        }

        public ActivityPayLaterRegistrationPage PopulateStudentDOB(string dob)
        {
            webDriver.WaitInSeconds(2);
            StudentDOBElement.Click();
            webDriver.WaitInSeconds(2);
            StudentDOBYearSelectionElement.Click();
            webDriver.WaitInSeconds(2);
            for (int i = 0; i < 7; i++)
            {
                StudentDOBYearBackElement.Click();
                webDriver.WaitInSeconds(2);
            }
            StudentDOBDateElement.Click();
            webDriver.WaitInSeconds(2);
            return this;
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
