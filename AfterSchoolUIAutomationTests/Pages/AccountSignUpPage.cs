using AfterSchoolUIAutomationTests.Helpers.Extensions;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace AfterSchoolUIAutomationTests.Pages
{
    public class AccountSignUpPage : IDisposable
    {
        //The Selenium web driver to automate the browser

        private IWebDriver webDriver;

        public AccountSignUpPage(IWebDriver WebDriver)
        {
            webDriver = WebDriver;
        }

        //------------Individual Account WebElements-----------

        private IWebElement SignUpGetStartedElement => webDriver.FindElement(By.XPath("(//button[@id='btn-GetStarted'])[1]"));
        private IWebElement CreateIndividualAccountElement => webDriver.FindElement(By.XPath("(//div[@class='row margin-t-35']//button[text()='Create an Individual Account'])[1]"));
        private IWebElement ProvideProgramsForStudentsElement => webDriver.FindElement(By.XPath("(//div[@class='row margin-t-35']//button[text()='Provide programs for students'])[1]"));
        private IWebElement SignUpAccountSearchElement => webDriver.FindElement(By.Id("signup-account-search"));
        private IWebElement ProgramNameSubmitElement => webDriver.FindElement(By.XPath("(//div[@class='visible-md visible-lg']//button[text()='Continue'])[1]"));
        private IWebElement FirstNameElement => webDriver.FindElement(By.Name("firstname"));
        private IWebElement LasttNameElement => webDriver.FindElement(By.Name("lastname"));
        private IWebElement EmailElement => webDriver.FindElement(By.Name("email"));
        private IWebElement PasswordElement => webDriver.FindElement(By.Name("password1"));
        private IWebElement Password2Element => webDriver.FindElement(By.Name("password2"));
        private IWebElement CaptchaElement => webDriver.FindElement(By.Id("recaptcha-anchor"));
        private IWebElement BasicInfoSubmitElement => webDriver.FindElement(By.XPath("(//div[@class='visible-md visible-lg margin-v-30']//button[text()='Continue'])[1]"));
        private IWebElement CreateYourFreeProfileElement => webDriver.FindElement(By.LinkText("Create Your Free Profile"));
        private IWebElement ShortDescriptionElement => webDriver.FindElement(By.Name("headline"));
        private IWebElement TimeZoneElement => webDriver.FindElement(By.Id("select-program-timezone"));
        private IWebElement SaveAndContinueElement => webDriver.FindElement(By.XPath("(//div[@class='visible-md visible-lg margin-v-30']//button[text()='Save and Continue'])[1]"));
        private IWebElement GenderElement => webDriver.FindElement(By.XPath("(//div[@class='am-radio program-radio'][2]//label)"));
        private IWebElement GradeOrAgeElement => webDriver.FindElement(By.XPath("(//div[@class='am-radio inline'][1]//label)"));
        private IWebElement MinGradeElement => webDriver.FindElement(By.Id("mingrade"));
        private IWebElement MaxGradeElement => webDriver.FindElement(By.Id("maxgrade"));
        private IWebElement ShareInfoWithParentsElement => webDriver.FindElement(By.XPath("(//div[@class='visible-md visible-lg margin-v-30']//button[text()='Save and Continue'])[1]"));

        private IWebElement PhoneElement => webDriver.FindElement(By.Name("phone"));
        private IWebElement LocationElement => webDriver.FindElement(By.Name("location"));
        private IWebElement GoogleMapElement => webDriver.FindElement(By.ClassName("pac-container"));
        private IWebElement ContactInfoElement => webDriver.FindElement(By.XPath("(//div[@class='visible-md visible-lg margin-v-30']//button[text()='Save and Continue'])[1]"));

        private IWebElement UploadProfileElement => webDriver.FindElement(By.Name("logo"));
        private IWebElement HeaderImageElement => webDriver.FindElement(By.Name("headerimage"));
        private IWebElement CustomizeProfileElement => webDriver.FindElement(By.XPath("(//div[@class='visible-md visible-lg margin-v-30']//button[text()='Save and Continue'])[1]"));

        private IWebElement ProgramCategoriesElement => webDriver.FindElement(By.Name("programCategories"));
        private IWebElement EasyActivityElement => webDriver.FindElement(By.XPath("(//div[@class='visible-md visible-lg margin-v-30']//button[text()='Save and Continue'])[1]"));

        private IWebElement EnterLinksElement => webDriver.FindElement(By.XPath("(//div[@class='visible-md visible-lg margin-v-30']//button[text()='Save and Continue'])[1]"));

        private IWebElement GoToProfileElement => webDriver.FindElement(By.Id("btnGoProfile"));

        private IWebElement location => webDriver.FindElement(By.XPath(""));
        private IWebElement H1GreatJob => webDriver.FindElement(By.XPath("//div[@class='col-md-12 onboarding-form-group text-center']//h1[@class='text-center register-thankyou-2 onboarding-phrase-1']"));

        //------------Individual Account Helper Methods-----------
        public AccountSignUpPage ClickSignUp()
        {
            webDriver.WaitInSeconds(2);
            SignUpGetStartedElement.Click();
            webDriver.WaitInSeconds(2);
            return this;
        }
        public AccountSignUpPage ClickIndividualAccount()
        {
            webDriver.WaitInSeconds(3);
            CreateIndividualAccountElement.Click();
            return this;
        }
        public AccountSignUpPage ProvideProgramsForStudents()
        {
            webDriver.WaitInSeconds(3);
            ProvideProgramsForStudentsElement.Click();
            return this;
        }
        public AccountSignUpPage PopulateProgramName(string name)
        {
            webDriver.WaitInSeconds(2);
            SignUpAccountSearchElement.SendKeys(name);
            return this;
        }

        public AccountSignUpPage ClickProgramSubmit()
        {
            webDriver.WaitInSeconds(6);
            ProgramNameSubmitElement.Click();
            return this;
        }
        public AccountSignUpPage PopulateFirstName(string name)
        {
            webDriver.WaitInSeconds(10);
            FirstNameElement.SendKeys(name);
            return this;
        }

        public AccountSignUpPage PopulateLasttName(string name)
        {
            webDriver.WaitInSeconds(2);
            LasttNameElement.SendKeys(name);
            return this;
        }

        public AccountSignUpPage PopulateEmail(string name)
        {
            webDriver.WaitInSeconds(2);
            EmailElement.SendKeys(name);
            return this;
        }

        public AccountSignUpPage PopulateUserPassword(string name)
        {
            webDriver.WaitInSeconds(2);
            PasswordElement.SendKeys(name);
            return this;
        }

        public AccountSignUpPage PopulateUserPassword2(string name)
        {
            webDriver.WaitInSeconds(2);
            Password2Element.SendKeys(name);
            return this;
        }

        public AccountSignUpPage CheckCaptcha()
        {
            CaptchaElement.Click();
            return this;
        }

        public AccountSignUpPage ClickBasicInfoSubmit()
        {
            webDriver.WaitInSeconds(2);
            BasicInfoSubmitElement.Click();
            return this;
        }

        public AccountSignUpPage CreateYourFreeProfile()
        {
            webDriver.WaitInSeconds(3);
            CreateYourFreeProfileElement.Click();
            return this;
        }

        public AccountSignUpPage PopulateShortDescription(string name)
        {
            webDriver.WaitInSeconds(3);
            ShortDescriptionElement.SendKeys(name);
            return this;
        }

        public AccountSignUpPage SelectGender()
        {
            webDriver.WaitInSeconds(5);
            GenderElement.Click();
            return this;
        }
        public AccountSignUpPage SelectTimeZone()
        {
            webDriver.WaitInSeconds(2);
            var timezone = TimeZoneElement;
            var selectElement = new SelectElement(TimeZoneElement);
            selectElement.SelectByValue("Alaska - AKST");
            return this;
        }

        public AccountSignUpPage SaveAndContinueSubmit()
        {
            webDriver.WaitInSeconds(2);
            SaveAndContinueElement.Click();
            return this;
        }

        public AccountSignUpPage SelectGradeOrAge()
        {
            webDriver.WaitInSeconds(5);
            GradeOrAgeElement.Click();
            return this;
        }

        public AccountSignUpPage SelectMinGrade()
        {
            webDriver.WaitInSeconds(3);
            var minAge = MinGradeElement;
            var selectElement = new SelectElement(MinGradeElement);
            selectElement.SelectByValue("5");
            return this;
        }

        public AccountSignUpPage SelectMaxAge()
        {
            webDriver.WaitInSeconds(2);
            var maxAge = MaxGradeElement;
            var selectElement = new SelectElement(MaxGradeElement);
            selectElement.SelectByValue("13");
            return this;
        }

        public AccountSignUpPage ShareInfoWithParentsSubmit()
        {
            webDriver.WaitInSeconds(2);
            ShareInfoWithParentsElement.Click();
            return this;
        }

        public AccountSignUpPage PopulatePhone(string name)
        {
            webDriver.WaitInSeconds(2);
            PhoneElement.SendKeys(name);
            return this;
        }

        public AccountSignUpPage PopulateLocation(string name)
        {
            LocationElement.SendKeys(name);
            return this;
        }

        public AccountSignUpPage SelectAutoComplete(string name)
        {
            webDriver.WaitInSeconds(2);
            LocationElement.SendKeys(name);
            Thread.Sleep(5000);
            WebDriverWait wait = new WebDriverWait(webDriver, new TimeSpan(0, 0, 5));
            wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.XPath("(//div[@class='pac-container pac-logo']//div[@class='pac-item'])[1]"))).Click();
            return this;
        }

        public AccountSignUpPage ContactInfoSubmit()
        {
            webDriver.WaitInSeconds(5);
            ContactInfoElement.Click();
            return this;
        }


        public AccountSignUpPage UploadProfileImage(string name)
        {
            UploadProfileElement.SendKeys(name);
            return this;
        }

        public AccountSignUpPage HeaderImageImage(string name)
        {
            webDriver.WaitInSeconds(2);
            HeaderImageElement.SendKeys(name);
            return this;
        }

        public AccountSignUpPage CustomizeProfileSubmit()
        {
            webDriver.WaitInSeconds(2);
            CustomizeProfileElement.Click();
            return this;
        }

        public AccountSignUpPage PopulateProgramCategories(string name)
        {
            webDriver.WaitInSeconds(4);
            ProgramCategoriesElement.SendKeys(name);
            return this;
        }

        public AccountSignUpPage EasyActivitySubmit()
        {
            webDriver.WaitInSeconds(2);
            EasyActivityElement.Click();
            return this;
        }

        public AccountSignUpPage EnterLinksSubmit()
        {
            webDriver.WaitInSeconds(4);
            EnterLinksElement.Click();
            return this;
        }

        public AccountSignUpPage ClickGoToProfile()
        {
            webDriver.WaitInSeconds(20);
            GoToProfileElement.Click();
            webDriver.WaitInSeconds(10);
            return this;
        }

        public string GetH1Text()
        {
            return H1GreatJob.Text;
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
