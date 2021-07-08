using System;
using System.Threading;
using AfterSchoolUIAutomationTests.Helpers.Extensions;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace AfterSchoolUIAutomationTests.Pages
{
    public class CreateActivityPage : IDisposable
    {
        private IWebDriver webDriver;

        public CreateActivityPage(IWebDriver WebDriver)
        {
            webDriver = WebDriver;
        }

        //------------Finding Create Activity WebElements-----------
        private IWebElement AccountButtonElement => webDriver.FindElement(By.XPath("(//div[@id='accounts']//button)[1]"));
        private IWebElement AddActivityElement => webDriver.FindElement(By.XPath("//div[@class='pull-right']"));
        private IWebElement ActivityName => webDriver.FindElement(By.Id("name"));
        private IWebElement TotalAvailableSeats => webDriver.FindElement(By.Id("seatsavailable"));
        private IWebElement InviteOnly => webDriver.FindElement(By.Id("inviteonly"));
        private IWebElement SignUpPeriodOpen => webDriver.FindElement(By.Id("registrationopendate"));
        private IWebElement SignUpPeriodOpenTime => webDriver.FindElement(By.Id("registrationopendate"));
        private IWebElement SignUpPeriodClose => webDriver.FindElement(By.Id("registrationclosedate"));
        private IWebElement SignUpPeriodCloseTime => webDriver.FindElement(By.Id("registrationclosedate"));
        private IWebElement SaveAndNextElement => webDriver.FindElement(By.XPath("//div[@class='col-sm-3']//button[text()='Save & Next »']"));
        private IWebElement ToastElement => webDriver.FindElement(By.XPath("//div[@class='gritter-content gritter-without-image']//span[@class='gritter-title']/text()"));
        private IWebElement RestrictionHeadingElement => webDriver.FindElement(By.XPath("//div[@class='panel-heading activity-heading']"));
        private IWebElement GenderElement => webDriver.FindElement(By.XPath("//div[@class='am-radio inline']//label[2]"));
        private IWebElement RestrictedAgeElement => webDriver.FindElement(By.XPath("//div[@class='col-lg-9 margin-left-sm']//div[@class='am-radio inline'][2]//label"));
        private IWebElement RestrictedGradeElement => webDriver.FindElement(By.XPath("//div[@class='col-lg-9 margin-left-sm']//div[@class='am-radio inline'][1]//label"));
        private IWebElement MinGradeDropDownElement => webDriver.FindElement(By.Id("mingrade"));
        private IWebElement MaxGradeDropDownElement => webDriver.FindElement(By.Id("maxgrade"));

        //------------Edit Account Profile Helper Methods-----------

        //------------Settings Section-----------
        public CreateActivityPage ClickAccountButton()
        {
            AccountButtonElement.Click();
            webDriver.WaitInSeconds(5);
            return this;
        }
        public CreateActivityPage ClickAddActivity()
        {
            AddActivityElement.Click();
            webDriver.WaitInSeconds(4);
            return this;
        }
        public CreateActivityPage PopulateActivityName(string activityName)
        {
            webDriver.WaitInSeconds(1);
            ActivityName.SendKeys(activityName);
            return this;
        }
        public CreateActivityPage PopulateTotalAvailableSeats(string availableSeats)
        {
            webDriver.WaitInSeconds(2);
            TotalAvailableSeats.SendKeys(availableSeats);
            return this;
        }
        public CreateActivityPage ToggleInviteOnly(string isInviteOnly)
        {
            webDriver.WaitInSeconds(2);
            if (isInviteOnly.Equals("Yes"))
            {
                InviteOnly.Click();
            }
            return this;
        }
        public CreateActivityPage SetSignUpPeriodOpen(string signUpPeriodOpenDate)
        {
            webDriver.WaitInSeconds(2);
            SignUpPeriodOpen.Clear();
            //SignUpPeriodOpen.SendKeys(Keys.Delete);
            SignUpPeriodOpen.SendKeys(signUpPeriodOpenDate);
            webDriver.WaitInSeconds(2);
            ActivityName.Click();
            //SignUpPeriodOpen.SendKeys(Keys.Tab);
            SignUpPeriodOpenTime.Click();
            var selectElement = new SelectElement(SignUpPeriodOpenTime);
            selectElement.SelectByValue("9:30 am");
            return this;
        }

        public CreateActivityPage SetSignUpPeriodClose(string signUpPeriodCloseDate)
        {
            webDriver.WaitInSeconds(2);
            SignUpPeriodClose.Clear();
            //SignUpPeriodClose.SendKeys(Keys.Delete);
            SignUpPeriodClose.SendKeys(signUpPeriodCloseDate);
            webDriver.WaitInSeconds(2);
            ActivityName.Click();
            //SignUpPeriodClose.SendKeys(Keys.Tab);
            SignUpPeriodCloseTime.Click();
            var selectElement = new SelectElement(SignUpPeriodCloseTime);
            selectElement.SelectByValue("11:55 am");
            return this;
        }

        public CreateActivityPage ClickSaveAndNext()
        {
            webDriver.WaitInSeconds(2);
            SaveAndNextElement.Click();
            webDriver.WaitInSeconds(4);
            return this;
        }

        //------------Restrictions Section-----------
        public CreateActivityPage SelectGender()
        {
            webDriver.WaitInSeconds(1);
            GenderElement.Click();
            return this;
        }

        public CreateActivityPage RestrictedBy()
        {
            webDriver.WaitInSeconds(1);
            RestrictedAgeElement.Click();
            webDriver.WaitInSeconds(2);
            RestrictedGradeElement.Click();
            return this;
        }

        public CreateActivityPage MinimumGrade()
        {
            webDriver.WaitInSeconds(2);
            MinGradeDropDownElement.Click();
            webDriver.WaitInSeconds(1);
            var selectElement = new SelectElement(MinGradeDropDownElement);
            selectElement.SelectByValue("4");
            RestrictionHeadingElement.Click();
            return this;
        }

        public CreateActivityPage MaximumGrade()
        {
            webDriver.WaitInSeconds(2);
            MaxGradeDropDownElement.Click();
            webDriver.WaitInSeconds(1);
            var selectElement = new SelectElement(MaxGradeDropDownElement);
            selectElement.SelectByValue("4");
            RestrictionHeadingElement.Click();
            return this;
        }



        //------------Schedule Section-----------
        //------------About Your Activity Section-----------
        //------------Contact Section-----------
        //------------Location Section-----------
        //------------Price & Payment Section-----------
        //------------Email Section-----------



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
