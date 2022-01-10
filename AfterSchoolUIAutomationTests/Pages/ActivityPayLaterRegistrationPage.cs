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
        private IWebElement FirstNameElement => webDriver.FindElement(By.XPath("/html/body/div[1]/div/div[8]/div/div/div/div/form/div[1]/div[2]/input"));
        private IWebElement StudentDOBElement => webDriver.FindElement(By.XPath("/html/body/div[1]/div/div[8]/div/div/div/div/form/div[7]/div[2]/div[1]/div/input"));
        private IWebElement StudentDOBYearSelectionElement => webDriver.FindElement(By.XPath("(//div[@class='datetimepicker-days']//table//thead//tr)[1]//th[2]"));
        private IWebElement StudentDOBYearBackElement => webDriver.FindElement(By.XPath("(//div[@class='datetimepicker-months']//table//thead//tr)[1]//th[1]"));
        private IWebElement StudentDOBMonthElement => webDriver.FindElement(By.XPath("(//div[@class='datetimepicker-months']//table//tbody//tr)[1]//td[1]//span[6]"));
        private IWebElement StudentDOBDateElement => webDriver.FindElement(By.XPath("//div[@class='datetimepicker-days']//table/tbody/tr[3]/td[3]"));
        private IWebElement StudentGradeElement => webDriver.FindElement(By.XPath("/html/body/div[1]/div/div[8]/div/div/div/div/form/div[8]/div[2]/div/select"));
        private IWebElement StudentGenderElement => webDriver.FindElement(By.XPath("(//div[@class='am-radio inline'])[1]//label"));
        private IWebElement StudentSchoolElement => webDriver.FindElement(By.XPath("/html/body/div[1]/div/div[8]/div/div/div/div/form/div[12]/div[2]/input[1]"));
        private IWebElement SaveAndContStudentInfoElement => webDriver.FindElement(By.XPath("//button[@id='btn-add-student']"));

        private IWebElement StudentDoctorNameElemnt => webDriver.FindElement(By.XPath("/html/body/div[1]/div/div[8]/div/div/div/div/form/div[1]/div[2]/input"));
        private IWebElement StudentDoctorPhone => webDriver.FindElement(By.XPath("/html/body/div[1]/div/div[8]/div/div/div/div/form/div[2]/div[2]/input"));
        private IWebElement StudentImmunizationElement => webDriver.FindElement(By.XPath("(//div[@class='am-radio inline'])[1]//label"));
        private IWebElement SaveAndContStudentHealthElement => webDriver.FindElement(By.XPath("//button[@id='btn-add-health']"));
        private IWebElement CompleteRegistrationElement => webDriver.FindElement(By.XPath("//h1[@class='activity-title']//span"));
        private IWebElement ParentESignElement => webDriver.FindElement(By.Id("signature"));
        private IWebElement EsignContinueBtnElement => webDriver.FindElement(By.Id("btn-register"));
        private IWebElement PayLaterElement => webDriver.FindElement(By.XPath("//div[@class='padding-v-30 text-center']//a[2]"));
        private IWebElement ThankyouElement => webDriver.FindElement(By.XPath("//div[@class='panel-body']//h1"));


        #region Paylater Activity Registration
        public ActivityPayLaterRegistrationPage ClickRegisterNow()
        {
            RegisterNowElement.Click();
            webDriver.WaitInSeconds(5);
            return this;
        }
        #endregion

        #region Parent Signin
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
        #endregion

        #region Child Selection
        public ActivityPayLaterRegistrationPage ClickAddNewChildBtn()
        {
            AddNewChildButtonElement.Click();
            webDriver.WaitInSeconds(4);
            return this;
        }
        #endregion

        #region Student Information
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
                webDriver.WaitInSeconds(1);
            }
            StudentDOBMonthElement.Click();
            webDriver.WaitInSeconds(2);
            StudentDOBDateElement.Click();
            webDriver.WaitInSeconds(2);
            return this;
        }
        public ActivityPayLaterRegistrationPage SetStudentGrade(string grade)
        {
            StudentGradeElement.Click();
            var selectElement = new SelectElement(StudentGradeElement);
            selectElement.SelectByText(grade);
            webDriver.WaitInSeconds(2);
            return this;
        }
        public ActivityPayLaterRegistrationPage SetStudentGender()
        {
            StudentGenderElement.Click();
            webDriver.WaitInSeconds(2);
            return this;
        }
        public ActivityPayLaterRegistrationPage SelectAutoCompleteSchool(string location)
        {
            StudentSchoolElement.SendKeys(location);
            webDriver.WaitInSeconds(1);
            Thread.Sleep(5000);
            WebDriverWait wait = new WebDriverWait(webDriver, new TimeSpan(0, 0, 5));
            wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.XPath("(//div[@class='pac-container pac-logo']//div[@class='pac-item'])[1]"))).Click();
            webDriver.WaitInSeconds(4);
            return this;
        }
        public ActivityPayLaterRegistrationPage ClickStudentInfoBtn()
        {
            SaveAndContStudentInfoElement.Click();
            webDriver.WaitInSeconds(4);
            return this;
        }
        #endregion

        #region Student Health Information Helper Methods
        public ActivityPayLaterRegistrationPage PopulateDoctorName(string name)
        {
            StudentDoctorNameElemnt.SendKeys(name);
            webDriver.WaitInSeconds(1);
            return this;
        }
        public ActivityPayLaterRegistrationPage PopulateDoctorPhone(string phone)
        {
            StudentDoctorPhone.SendKeys(phone);
            webDriver.WaitInSeconds(1);
            return this;
        }
        public ActivityPayLaterRegistrationPage SetStudentImmunization()
        {
            StudentImmunizationElement.Click();
            webDriver.WaitInSeconds(2);
            return this;
        }
        public ActivityPayLaterRegistrationPage ClickStudentHealthNextBtn()
        {
            SaveAndContStudentHealthElement.Click();
            webDriver.WaitInSeconds(4);
            return this;
        }

        #endregion

        #region Parennt E-Sign
        public ActivityPayLaterRegistrationPage PopulateParentESign(string esign)
        {
            ParentESignElement.SendKeys(esign);
            webDriver.WaitInSeconds(1);
            return this;
        }
        public ActivityPayLaterRegistrationPage ClickESignNextBtn()
        {
            EsignContinueBtnElement.Click();
            webDriver.WaitInSeconds(4);
            return this;
        }
        #endregion

        #region Payment PayLater
        public ActivityPayLaterRegistrationPage ClickPayLaterNextBtn()
        {
            PayLaterElement.Click();
            webDriver.WaitInSeconds(4);
            return this;
        }
        public string RegistrationComplete()
        {
            webDriver.WaitInSeconds(1);
            return ThankyouElement.Text;
        }
        #endregion

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
