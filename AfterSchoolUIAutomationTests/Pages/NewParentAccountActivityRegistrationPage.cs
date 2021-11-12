using System;
using System.Threading;
using AfterSchoolUIAutomationTests.Helpers.Extensions;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace AfterSchoolUIAutomationTests.Pages
{
    class NewParentAccountActivityRegistrationPage
    {
        private readonly IWebDriver webDriver;
        public NewParentAccountActivityRegistrationPage(IWebDriver WebDriver)
        {
            webDriver = WebDriver;
        }

        //------------Finding NewParentAccountActivityRegistrationPage WebElements-----------//

        #region Step 1 Parent Signup WebElements

        private IWebElement RegisterNowElement => webDriver.FindElement(By.XPath("//div[@class='col-lg-4 col-md-12']//a[@class=' btn btn-register btn-block']"));
        private IWebElement ParentEmailElement => webDriver.FindElement(By.Id("email-check"));
        private IWebElement EmailClickNextElement => webDriver.FindElement(By.Id("btn-check-email"));
        //private IWebElement FirstNnameElement => webDriver.FindElement(By.Id("question-9487"));
        private IWebElement FirstNnameElement => webDriver.FindElement(By.XPath("/html/body/div[1]/div/div[8]/div/div/div/div[2]/form/div[2]/div[2]/div[2]/input"));
        //private IWebElement LastNameElement => webDriver.FindElement(By.Id("question-9488"));
        private IWebElement LastNameElement => webDriver.FindElement(By.XPath("/html/body/div[1]/div/div[8]/div/div/div/div[2]/form/div[2]/div[3]/div[2]/input"));
        //private IWebElement PhoneElement => webDriver.FindElement(By.Id("question-9490"));
        private IWebElement PhoneElement => webDriver.FindElement(By.XPath("/html/body/div[1]/div/div[8]/div/div/div/div[2]/form/div[2]/div[5]/div[2]/input"));
        //private IWebElement Password1Element => webDriver.FindElement(By.Id("password1"));
        private IWebElement Password1Element => webDriver.FindElement(By.XPath("/html/body/div[1]/div/div[8]/div/div/div/div[2]/form/div[2]/div[9]/div/input"));
        //private IWebElement Password2Element => webDriver.FindElement(By.Id("password2"));
        private IWebElement Password2Element => webDriver.FindElement(By.XPath("/html/body/div[1]/div/div[8]/div/div/div/div[2]/form/div[2]/div[10]/div/input"));
        private IWebElement IAgreeElement => webDriver.FindElement(By.XPath("(//div[@class='am-checkbox text-center'])[2]//label/text()[1]"));
        //private IWebElement IAgreeElement => webDriver.FindElement(By.XPath("//input[@name='agree' and @id='agree']"));
        private IWebElement SaveAndContParentSignUpElement => webDriver.FindElement(By.XPath("//button[@id='btn-create-account']"));

        #endregion

        #region Step 2 Home Address WebElements

        //private IWebElement StreetAddressElement => webDriver.FindElement(By.Id("question-9499"));
        private IWebElement StreetAddressElement => webDriver.FindElement(By.XPath("/html/body/div[1]/div/div[8]/div/div/div/div[2]/form/div[1]/div[2]/input"));
        //private IWebElement CityElement => webDriver.FindElement(By.Id("question-9501"));
        private IWebElement CityElement => webDriver.FindElement(By.XPath("/html/body/div[1]/div/div[8]/div/div/div/div[2]/form/div[3]/div[2]/input"));
        //private IWebElement StateSelectElement => webDriver.FindElement(By.Id("question-9502"));
        private IWebElement StateSelectElement => webDriver.FindElement(By.XPath("/html/body/div[1]/div/div[8]/div/div/div/div[2]/form/div[4]/div[2]/div[1]/select"));
        //private IWebElement ZipCodeElement => webDriver.FindElement(By.Id("question-9503"));
        private IWebElement ZipCodeElement => webDriver.FindElement(By.XPath("/html/body/div[1]/div/div[8]/div/div/div/div[2]/form/div[5]/div[2]/input"));
        private IWebElement SaveAndContHomeAddressElement => webDriver.FindElement(By.XPath("//button[@id='btn-add-home-address']"));

        #endregion

        #region Step 3 Additional Parent, Guardian WebElements

        //private IWebElement GuardianFirstNameElement => webDriver.FindElement(By.Id("question-9487"));
        private IWebElement GuardianFirstNameElement => webDriver.FindElement(By.XPath("/html/body/div[1]/div/div[8]/div/div/div/div/form/div[1]/div[2]/input"));
        //private IWebElement GuardianLastNameElement => webDriver.FindElement(By.Id("question-9488"));
        private IWebElement GuardianLastNameElement => webDriver.FindElement(By.XPath("/html/body/div[1]/div/div[8]/div/div/div/div/form/div[2]/div[2]/input"));
        //private IWebElement GuardianEmailElement => webDriver.FindElement(By.Id("question-9489"));
        private IWebElement GuardianEmailElement => webDriver.FindElement(By.XPath("/html/body/div[1]/div/div[8]/div/div/div/div/form/div[3]/div[2]/input"));
        //private IWebElement GuardianMobileElement => webDriver.FindElement(By.Id("question-9490"));
        private IWebElement GuardianMobileElement => webDriver.FindElement(By.XPath("/html/body/div[1]/div/div[8]/div/div/div/div/form/div[4]/div[2]/input"));
        private IWebElement SaveAndContParentGuardianElement => webDriver.FindElement(By.XPath("//button[@id='btn-add-parent']"));

        #endregion

        #region Step 4 Emergency WebElements

        private IWebElement EmergencyContactFirstNameElement => webDriver.FindElement(By.Id("question-9526"));
        private IWebElement EmergencyContactLastNameElement => webDriver.FindElement(By.Id("question-9527"));
        private IWebElement EmergencyContactPhoneElement => webDriver.FindElement(By.Id("question-9528"));
        private IWebElement SaveAndContEmergencyElement => webDriver.FindElement(By.XPath("//button[@id='btn-emergency-contact']"));

        #endregion

        #region Step 5 Student Information WebElements

        private IWebElement StudentFirstNameElement => webDriver.FindElement(By.Id("question-9487"));
        private IWebElement StudentLastNameElement => webDriver.FindElement(By.Id("question-9488"));
        private IWebElement StudentPhoneElement => webDriver.FindElement(By.Id("question-9490"));
        private IWebElement StudentDOBElement => webDriver.FindElement(By.Id("question-9493"));
        private IWebElement StudentDOBDateElement => webDriver.FindElement(By.XPath("//div[@class='datetimepicker-days']//table/tbody/tr[3]/td[3]"));
        private IWebElement StudentGradeElement => webDriver.FindElement(By.Id("question-9494"));
        private IWebElement StudentGenderElement => webDriver.FindElement(By.XPath("(//div[@class='am-radio inline'])[1]//label"));
        private IWebElement StudentSchoolElement => webDriver.FindElement(By.Id("location"));
        private IWebElement SaveAndContStudentInfoElement => webDriver.FindElement(By.XPath("//button[@id='btn-add-student']"));

        #endregion

        #region Step 6 Student Health Information WebElements

        private IWebElement StudentDoctorNameElemnt => webDriver.FindElement(By.Id("question-9532"));
        private IWebElement StudentDoctorPhone => webDriver.FindElement(By.Id("question-9533"));
        private IWebElement StudentImmunizationElement => webDriver.FindElement(By.XPath("(//div[@class='am-radio inline'])[1]//label"));
        private IWebElement SaveAndContStudentHealthElement => webDriver.FindElement(By.XPath("//button[@id='btn-add-health']"));
        private IWebElement CompleteRegistrationElement => webDriver.FindElement(By.XPath("//h1[@class='activity-title']//span"));

        #endregion

        //------------Finding NewParentAccountActivityRegistrationPage Helper Methods-----------//

        #region Step1 Parent Signup Helper Methods

        public NewParentAccountActivityRegistrationPage ClickRegisterNow()
        {
            RegisterNowElement.Click();
            webDriver.WaitInSeconds(3);
            return this;
        }

        public NewParentAccountActivityRegistrationPage PopulateEmail(string email)
        {
            ParentEmailElement.SendKeys(email);
            webDriver.WaitInSeconds(2);
            return this;
        }

        public NewParentAccountActivityRegistrationPage ClickNextBtn()
        {
            EmailClickNextElement.Click();
            webDriver.WaitInSeconds(2);
            return this;
        }

        public NewParentAccountActivityRegistrationPage PopulateFirstName(string firstname)
        {
            FirstNnameElement.SendKeys(firstname);
            webDriver.WaitInSeconds(1);
            return this;
        }

        public NewParentAccountActivityRegistrationPage PopulateLastName(string lastname)
        {
            LastNameElement.SendKeys(lastname);
            webDriver.WaitInSeconds(1);
            return this;
        }

        public NewParentAccountActivityRegistrationPage PopulatePhone(string phone)
        {
            PhoneElement.SendKeys(WebDriverExtension.GetRandomTelNo());
            webDriver.WaitInSeconds(1);
            return this;
        }

        public NewParentAccountActivityRegistrationPage PopulatePassword1(string password1)
        {
            Password1Element.SendKeys(password1);
            webDriver.WaitInSeconds(1);
            return this;
        }

        public NewParentAccountActivityRegistrationPage PopulatePassword2(string password2)
        {
            Password2Element.SendKeys(password2);
            webDriver.WaitInSeconds(2);
            return this;
        }

        public NewParentAccountActivityRegistrationPage ClickIAgreeCheck()
        {
            //IAgreeElement.Click();
            webDriver.WaitInSeconds(1);
            return this;
        }

        public NewParentAccountActivityRegistrationPage ClickParentSignUpNextBtn()
        {
            SaveAndContParentSignUpElement.Click();
            webDriver.WaitInSeconds(4);
            return this;
        }

        #endregion

        #region Step2 Home Address Helper Methods

        public NewParentAccountActivityRegistrationPage PopulateStreetAddress(string streetAddress)
        {
            StreetAddressElement.SendKeys(streetAddress);
            webDriver.WaitInSeconds(1);
            return this;
        }

        public NewParentAccountActivityRegistrationPage PopulateCity(string city)
        {
            CityElement.SendKeys(city);
            webDriver.WaitInSeconds(1);
            return this;
        }

        public NewParentAccountActivityRegistrationPage SelectState(string state)
        {
            StateSelectElement.Click();
            webDriver.WaitInSeconds(2);
            var selectElement = new SelectElement(StateSelectElement);
            selectElement.SelectByValue(state);
            return this;
        }

        public NewParentAccountActivityRegistrationPage PopulateZipCode(string zipcode)
        {
            ZipCodeElement.SendKeys(zipcode);
            webDriver.WaitInSeconds(1);
            return this;
        }

        public NewParentAccountActivityRegistrationPage ClickHomeAddressNextBtn()
        {
            SaveAndContHomeAddressElement.Click();
            webDriver.WaitInSeconds(4);
            return this;
        }

        #endregion

        #region Step 3 Additional Parent, Guardian Helper Methods

        public NewParentAccountActivityRegistrationPage PopulateGuardianFirstName(string firstName)
        {
            GuardianFirstNameElement.Clear();
            GuardianFirstNameElement.SendKeys(firstName);
            webDriver.WaitInSeconds(1);
            return this;
        }

        public NewParentAccountActivityRegistrationPage PopulateGuardianLastName(string lastName)
        {
            GuardianLastNameElement.Clear();
            GuardianLastNameElement.SendKeys(lastName);
            webDriver.WaitInSeconds(1);
            return this;
        }

        public NewParentAccountActivityRegistrationPage PopulateGuardianEmail(string email)
        {
            GuardianEmailElement.SendKeys(email);
            webDriver.WaitInSeconds(1);
            return this;
        }

        public NewParentAccountActivityRegistrationPage PopulateGuardianMobile(string mobile)
        {
            GuardianMobileElement.SendKeys(mobile);
            webDriver.WaitInSeconds(1);
            return this;
        }

        public NewParentAccountActivityRegistrationPage ClickParentGuardianNextBtn()
        {
            SaveAndContParentGuardianElement.Click();
            webDriver.WaitInSeconds(4);
            return this;
        }

        #endregion

        #region Step 4 Emergency Helper Methods

        public NewParentAccountActivityRegistrationPage PopulateEmergencyContactFirstName(string firstName)
        {
            EmergencyContactFirstNameElement.SendKeys(firstName);
            webDriver.WaitInSeconds(1);
            return this;
        }

        public NewParentAccountActivityRegistrationPage PopulateEmergencyContactLastName(string lastName)
        {
            EmergencyContactLastNameElement.SendKeys(lastName);
            webDriver.WaitInSeconds(1);
            return this;
        }

        public NewParentAccountActivityRegistrationPage PopulateEmergencyContactPhone(string phone)
        {
            EmergencyContactPhoneElement.SendKeys(phone);
            webDriver.WaitInSeconds(1);
            return this;
        }

        public NewParentAccountActivityRegistrationPage ClickEmergencyNextBtn()
        {
            SaveAndContEmergencyElement.Click();
            webDriver.WaitInSeconds(4);
            return this;
        }

        #endregion

        #region Step 5 Student Information Helper Methods

        public NewParentAccountActivityRegistrationPage PopulateStudentFirstName(string firstName)
        {
            StudentFirstNameElement.SendKeys(firstName);
            webDriver.WaitInSeconds(1);
            return this;
        }

        public NewParentAccountActivityRegistrationPage PopulateStudentLastName(string lastName)
        {
            StudentLastNameElement.Clear();
            StudentLastNameElement.SendKeys(lastName);
            webDriver.WaitInSeconds(1);
            return this;
        }

        public NewParentAccountActivityRegistrationPage PopulateStudentPhone(string phone)
        {
            StudentPhoneElement.Clear();
            StudentPhoneElement.SendKeys(phone);
            webDriver.WaitInSeconds(1);
            return this;
        }

        public NewParentAccountActivityRegistrationPage PopulateStudentDOB(string dob)
        {
            webDriver.WaitInSeconds(2);
            StudentDOBElement.Click();
            webDriver.WaitInSeconds(2);
            StudentDOBDateElement.Click();
            webDriver.WaitInSeconds(2);
            return this;
        }

        public NewParentAccountActivityRegistrationPage SetStudentGrade(string grade)
        {
            StudentGradeElement.Click();
            var selectElement = new SelectElement(StudentGradeElement);
            selectElement.SelectByValue(grade);
            webDriver.WaitInSeconds(2);
            return this;
        }

        public NewParentAccountActivityRegistrationPage SetStudentGender()
        {
            StudentGenderElement.Click();
            webDriver.WaitInSeconds(2);
            return this;
        }

        public NewParentAccountActivityRegistrationPage SelectAutoCompleteSchool(string location)
        {
            StudentSchoolElement.SendKeys(location);
            webDriver.WaitInSeconds(1);
            Thread.Sleep(5000);
            WebDriverWait wait = new WebDriverWait(webDriver, new TimeSpan(0, 0, 5));
            wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.XPath("(//div[@class='pac-container pac-logo']//div[@class='pac-item'])[1]"))).Click();
            webDriver.WaitInSeconds(4);
            return this;
        }

        public NewParentAccountActivityRegistrationPage ClickStudentInfoBtn()
        {
            SaveAndContStudentInfoElement.Click();
            webDriver.WaitInSeconds(4);
            return this;
        }

        #endregion

        #region Step 6 Student Health Information Helper Methods

        public NewParentAccountActivityRegistrationPage PopulateDoctorName(string name)
        {
            StudentDoctorNameElemnt.SendKeys(name);
            webDriver.WaitInSeconds(1);
            return this;
        }

        public NewParentAccountActivityRegistrationPage PopulateDoctorPhone(string phone)
        {
            StudentDoctorPhone.SendKeys(phone);
            webDriver.WaitInSeconds(1);
            return this;
        }

        public NewParentAccountActivityRegistrationPage SetStudentImmunization()
        {
            StudentImmunizationElement.Click();
            webDriver.WaitInSeconds(2);
            return this;
        }

        public NewParentAccountActivityRegistrationPage ClickStudentHealthNextBtn()
        {
            SaveAndContStudentHealthElement.Click();
            webDriver.WaitInSeconds(4);
            return this;
        }

        public string RegistrationComplete()
        {
            webDriver.WaitInSeconds(1);
            return CompleteRegistrationElement.Text;
        }

        #endregion
    }
}