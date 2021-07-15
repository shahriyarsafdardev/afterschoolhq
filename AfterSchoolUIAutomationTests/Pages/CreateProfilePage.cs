using AfterSchoolUIAutomationTests.Helpers.Extensions;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace AfterSchoolUIAutomationTests.Pages
{
    public class CreateProfilePage : IDisposable
    {
        private IWebDriver webDriver;

        public CreateProfilePage(IWebDriver WebDriver)
        {
            webDriver = WebDriver;
        }

        ////------------Create Account Profile WebElements-----------
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

        ////------------Create Account Profile Helper Methods-----------

        public CreateProfilePage CreateYourFreeProfile()
        {
            webDriver.WaitInSeconds(2);
            CreateYourFreeProfileElement.Click();
            return this;
        }

        public CreateProfilePage PopulateShortDescription(string name)
        {
            webDriver.WaitInSeconds(2);
            ShortDescriptionElement.SendKeys(name);
            return this;
        }

        public CreateProfilePage SelectGender()
        {
            webDriver.WaitInSeconds(2);
            GenderElement.Click();
            return this;
        }
        public CreateProfilePage SelectTimeZone()
        {
            webDriver.WaitInSeconds(2);
            var timezone = TimeZoneElement;
            var selectElement = new SelectElement(TimeZoneElement);
            selectElement.SelectByValue("Alaska - AKST");
            return this;
        }

        public CreateProfilePage SaveAndContinueSubmit()
        {
            webDriver.WaitInSeconds(2);
            SaveAndContinueElement.Click();
            return this;
        }

        public CreateProfilePage SelectGradeOrAge()
        {
            webDriver.WaitInSeconds(2);
            GradeOrAgeElement.Click();
            return this;
        }

        public CreateProfilePage SelectMinGrade()
        {
            webDriver.WaitInSeconds(2);
            var minAge = MinGradeElement;
            var selectElement = new SelectElement(MinGradeElement);
            selectElement.SelectByValue("5");
            return this;
        }

        public CreateProfilePage SelectMaxAge()
        {
            webDriver.WaitInSeconds(2);
            var maxAge = MaxGradeElement;
            var selectElement = new SelectElement(MaxGradeElement);
            selectElement.SelectByValue("13");
            return this;
        }

        public CreateProfilePage ShareInfoWithParentsSubmit()
        {
            webDriver.WaitInSeconds(2);
            ShareInfoWithParentsElement.Click();
            return this;
        }

        public CreateProfilePage PopulatePhone(string name)
        {
            webDriver.WaitInSeconds(2);
            PhoneElement.SendKeys(name);
            return this;
        }

        public CreateProfilePage PopulateLocation(string name)
        {
            LocationElement.SendKeys(name);
            return this;
        }

        public CreateProfilePage SelectAutoComplete(string name)
        {
            webDriver.WaitInSeconds(2);
            LocationElement.SendKeys(name);
            Thread.Sleep(5000);
            WebDriverWait wait = new WebDriverWait(webDriver, new TimeSpan(0, 0, 5));
            wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.XPath("(//div[@class='pac-container pac-logo']//div[@class='pac-item'])[1]"))).Click();
            return this;
        }

        public CreateProfilePage ContactInfoSubmit()
        {
            webDriver.WaitInSeconds(5);
            ContactInfoElement.Click();
            return this;
        }


        public CreateProfilePage UploadProfileImage(string name)
        {
            UploadProfileElement.SendKeys(name);
            return this;
        }

        public CreateProfilePage HeaderImageImage(string name)
        {
            webDriver.WaitInSeconds(2);
            HeaderImageElement.SendKeys(name);
            return this;
        }

        public CreateProfilePage CustomizeProfileSubmit()
        {
            webDriver.WaitInSeconds(2);
            CustomizeProfileElement.Click();
            return this;
        }

        public CreateProfilePage PopulateProgramCategories(string name)
        {
            webDriver.WaitInSeconds(3);
            ProgramCategoriesElement.SendKeys(name);
            return this;
        }

        public CreateProfilePage EasyActivitySubmit()
        {
            webDriver.WaitInSeconds(2);
            EasyActivityElement.Click();
            return this;
        }

        public CreateProfilePage EnterLinksSubmit()
        {
            webDriver.WaitInSeconds(2);
            EnterLinksElement.Click();
            return this;
        }

        public CreateProfilePage ClickGoToProfile()
        {
            webDriver.WaitInSeconds(15);
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
