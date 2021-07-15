using AfterSchoolUIAutomationTests.Helpers.Extensions;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace AfterSchoolUIAutomationTests.Pages
{
    public class NewProviderSignUpFlowPage
    {
        private IWebDriver webDriver;

        public NewProviderSignUpFlowPage(IWebDriver WebDriver)
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
        private IWebElement CaptchaElement => webDriver.FindElement(By.XPath("//div[@class='rc-anchor-center-container']//label"));
        private IWebElement BasicInfoSubmitElement => webDriver.FindElement(By.XPath("(//div[@class='visible-md visible-lg margin-v-30']//button[text()='Continue'])[1]"));

        //------------Individual Account Helper Methods-----------
        public NewProviderSignUpFlowPage ClickSignUp()
        {
            webDriver.WaitInSeconds(1);
            SignUpGetStartedElement.Click();
            webDriver.WaitInSeconds(2);
            return this;
        }
        public NewProviderSignUpFlowPage ClickIndividualAccount()
        {
            CreateIndividualAccountElement.Click();
            webDriver.WaitInSeconds(2);
            return this;
        }
        public NewProviderSignUpFlowPage ProvideProgramsForStudents()
        {
            ProvideProgramsForStudentsElement.Click();
            webDriver.WaitInSeconds(2);
            return this;
        }
        public NewProviderSignUpFlowPage PopulateProgramName(string name)
        {
            SignUpAccountSearchElement.SendKeys(name);
            webDriver.WaitInSeconds(4);
            return this;
        }

        public NewProviderSignUpFlowPage ClickProgramSubmit()
        {
            ProgramNameSubmitElement.Click();
            webDriver.WaitInSeconds(3);
            return this;
        }
        public NewProviderSignUpFlowPage PopulateFirstName(string name)
        {
            FirstNameElement.SendKeys(name);
            webDriver.WaitInSeconds(1);
            return this;
        }

        public NewProviderSignUpFlowPage PopulateLasttName(string name)
        {
            LasttNameElement.SendKeys(name);
            webDriver.WaitInSeconds(1);
            return this;
        }

        public NewProviderSignUpFlowPage PopulateEmail(string name)
        {
            EmailElement.SendKeys(name);
            webDriver.WaitInSeconds(1);
            return this;
        }

        public NewProviderSignUpFlowPage PopulateUserPassword(string name)
        {
            PasswordElement.SendKeys(name);
            webDriver.WaitInSeconds(1);
            return this;
        }

        public NewProviderSignUpFlowPage PopulateUserPassword2(string name)
        {
            Password2Element.SendKeys(name);
            webDriver.WaitInSeconds(15);
            return this;
        }

        public NewProviderSignUpFlowPage CheckCaptcha()
        {
            CaptchaElement.Click();
            webDriver.WaitInSeconds(10);
            return this;
        }

        public NewProviderSignUpFlowPage ClickBasicInfoSubmit()
        {
            BasicInfoSubmitElement.Click();
            webDriver.WaitInSeconds(2);
            return this;
        }
    }
}
