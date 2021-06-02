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
        public void ClickSignUp() => SignUpGetStartedElement.Click();
        public void ClickIndividualAccount() => CreateIndividualAccountElement.Click();
        public void ProvideProgramsForStudents() => ProvideProgramsForStudentsElement.Click();
        public void PopulateProgramName(string name) => SignUpAccountSearchElement.SendKeys(name);
        public void ClickProgramSubmit() => ProgramNameSubmitElement.Click();
        public void PopulateFirstName(string name) => FirstNameElement.SendKeys(name);
        public void PopulateLasttName(string name) => LasttNameElement.SendKeys(name);
        public void PopulateEmail(string name) => EmailElement.SendKeys(name);
        public void PopulateUserPassword(string name) => PasswordElement.SendKeys(name);
        public void PopulateUserPassword2(string name) => Password2Element.SendKeys(name);
        public void CheckCaptcha() => CaptchaElement.Click();
        public void ClickBasicInfoSubmit() => BasicInfoSubmitElement.Click();

        public void CreateYourFreeProfile() => CreateYourFreeProfileElement.Click();

        public void PopulateShortDescription(string name) => ShortDescriptionElement.SendKeys(name);
        public void SelectGender() => GenderElement.Click();
        public void SelectTimeZone()
        {
            var timezone = TimeZoneElement;
            var selectElement = new SelectElement(TimeZoneElement);
            selectElement.SelectByValue("Alaska - AKST");
        }
        public void SaveAndContinueSubmit() => SaveAndContinueElement.Click();


        public void SelectGradeOrAge() => GradeOrAgeElement.Click();
        public void SelectMinGrade()
        {
            var minAge = MinGradeElement;
            var selectElement = new SelectElement(MinGradeElement);
            selectElement.SelectByValue("5");
        }
        public void SelectMaxAge()
        {
            var maxAge = MaxGradeElement;
            var selectElement = new SelectElement(MaxGradeElement);
            selectElement.SelectByValue("13");
        }
        public void ShareInfoWithParentsSubmit() => ShareInfoWithParentsElement.Click();

        public void PopulatePhone(string name) => PhoneElement.SendKeys(name);
        public void PopulateLocation(string name) => LocationElement.SendKeys(name);

        public void SelectAutoComplete(string name)
        {
            LocationElement.SendKeys(name);
            Thread.Sleep(5000);
            WebDriverWait wait = new WebDriverWait(webDriver, new TimeSpan(0, 0, 5));
            wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.XPath("(//div[@class='pac-container pac-logo']//div[@class='pac-item'])[1]"))).Click();
        }
        public void ContactInfoSubmit() => ContactInfoElement.Click();


        public void UploadProfileImage(string name) => UploadProfileElement.SendKeys(name);
        public void HeaderImageImage(string name) => HeaderImageElement.SendKeys(name);
        public void CustomizeProfileSubmit() => CustomizeProfileElement.Click();

        public void PopulateProgramCategories(string name) => ProgramCategoriesElement.SendKeys(name);
        public void EasyActivitySubmit() => EasyActivityElement.Click();


        public void EnterLinksSubmit() => EnterLinksElement.Click();

        public void ClickGoToProfile() => GoToProfileElement.Click();

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
