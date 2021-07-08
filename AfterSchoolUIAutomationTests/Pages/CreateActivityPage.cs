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
        private IWebElement AccountButtonElement => webDriver.FindElement(By.XPath("//div[@id='accounts']//button[@class='btn btn-primary btn-xl btn-space btn-block']"));
        private IWebElement ActivityName => webDriver.FindElement(By.Id("name"));
        private IWebElement TotalAvailableSeats => webDriver.FindElement(By.Id("seatsavailable"));
        private IWebElement InviteOnly => webDriver.FindElement(By.Id("inviteonly"));
        private IWebElement SignUpPeriodOpen => webDriver.FindElement(By.Id("registrationopendate"));
        private IWebElement SignUpPeriodClose => webDriver.FindElement(By.Id("registrationclosedate"));
        private IWebElement SaveAndNextElement => webDriver.FindElement(By.XPath("//div[@class='col-sm-3']//button[text()='Save & Next »']"));
        private IWebElement ToastElement => webDriver.FindElement(By.XPath("//div[@class='gritter-content gritter-without-image']//span[@class='gritter-title']/text()"));

        //------------Edit Account Profile Helper Methods-----------

        //------------Settings Section-----------
        public CreateActivityPage ClickAccountButton()
        {
            AccountButtonElement.Click();
            webDriver.WaitInSeconds(2);
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
            webDriver.WaitInSeconds(1);
            TotalAvailableSeats.SendKeys(availableSeats);
            return this;
        }

        public CreateActivityPage ToggleInviteOnly(string isInviteOnly)
        {
            webDriver.WaitInSeconds(1);
            if (isInviteOnly.Equals("Yes"))
            {
                InviteOnly.Click();
            }
            return this;
        }

        public CreateActivityPage SetSignUpPeriodOpen(string signUpPeriodOpenDate)
        {
            webDriver.WaitInSeconds(1);
            SignUpPeriodOpen.SendKeys(signUpPeriodOpenDate);
            return this;
        }

        public CreateActivityPage SetSignUpPeriodClose(string signUpPeriodCloseDate)
        {
            webDriver.WaitInSeconds(1);
            SignUpPeriodClose.SendKeys(signUpPeriodCloseDate);
            return this;
        }

        public CreateActivityPage ClickSaveAndNext()
        {
            webDriver.WaitInSeconds(2);
            SaveAndNextElement.Click();
            return this;
        }

        //------------Restrictions Section-----------
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
