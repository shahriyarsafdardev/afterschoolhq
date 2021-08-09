using AfterSchoolUIAutomationTests.Pages;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TechTalk.SpecFlow;
using AfterSchoolUIAutomationTests.Helpers.Extensions;
using AfterSchoolUIAutomationTests.Parameters;

namespace AfterSchoolUIAutomationTests2.Steps
{
    [Binding]
    public sealed class NewParentAccountActivityRegistrationStep : IDisposable
    {
        private readonly ScenarioContext _scenarioContext;
        private IWebDriver webDriver;
        private NewParentAccountActivityRegistrationPage newParentAccountActivityRegistrationPage;

        public NewParentAccountActivityRegistrationStep(ScenarioContext scenarioContext)
        {
            _scenarioContext = scenarioContext;
            webDriver = new ChromeDriver(AppDomain.CurrentDomain.BaseDirectory);
            newParentAccountActivityRegistrationPage = new NewParentAccountActivityRegistrationPage(webDriver);
        }

        [Given(@"Navigate to the session publish url and click register now")]
        public void NavigateToActivityUrlAndClickRegister()
        {
            webDriver.Navigate().GoToUrl(Parameter.Get("NewParentAccountActivityRegistration", "ActivityPublishedURL"));
            webDriver.WaitInSeconds(4);
            webDriver.Manage().Window.Maximize();
            webDriver.WaitInSeconds(2);
            newParentAccountActivityRegistrationPage.ClickRegisterNow();
        }

        [Given(@"Enter parent email address and click next")]
        public void PopulateEmailAndClickNext()
        {
            newParentAccountActivityRegistrationPage.PopulateEmail("parent" + DateTime.Now.ToString("yyyyMMddHHmmss") + "@yopmail.com");
            newParentAccountActivityRegistrationPage.ClickNextBtn();
        }

        [Given(@"Populate fields in step 1 parent signup form details and click save & continue")]
        public void PopulateParentSignupFormAndClickNext()
        {
            newParentAccountActivityRegistrationPage.PopulateFirstName(Parameter.Get("NewParentAccountActivityRegistration", "FirstName"));
            newParentAccountActivityRegistrationPage.PopulateLastName(Parameter.Get("NewParentAccountActivityRegistration", "LastName"));
            newParentAccountActivityRegistrationPage.PopulatePhone(Parameter.Get("NewParentAccountActivityRegistration", "Phone"));
            newParentAccountActivityRegistrationPage.PopulatePassword1(Parameter.Get("NewParentAccountActivityRegistration", "Password1"));
            newParentAccountActivityRegistrationPage.PopulatePassword2(Parameter.Get("NewParentAccountActivityRegistration", "Password2"));
            newParentAccountActivityRegistrationPage.ClickIAgreeCheck();
            newParentAccountActivityRegistrationPage.ClickParentSignUpNextBtn();
        }

        [Given(@"Populate fields in step 2 home address form details and click save & continue")]
        public void PopulateHomeAddressFormAndClickNext()
        {
            newParentAccountActivityRegistrationPage.PopulateStreetAddress(Parameter.Get("NewParentAccountActivityRegistration", "StreetAddress"));
            newParentAccountActivityRegistrationPage.PopulateCity(Parameter.Get("NewParentAccountActivityRegistration", "City"));
            newParentAccountActivityRegistrationPage.SelectState(Parameter.Get("NewParentAccountActivityRegistration", "State"));
            newParentAccountActivityRegistrationPage.PopulateZipCode(Parameter.Get("NewParentAccountActivityRegistration", "Zipcode"));
            newParentAccountActivityRegistrationPage.ClickHomeAddressNextBtn();
        }

        [Given(@"Populate fields in step 3 additional parent, guardian form details and click save & continue")]
        public void PopulateAdditionalParentGuardianFormAndClickNext()
        {
            //newParentAccountActivityRegistrationPage.GuardianFirstName(Parameter.Get("NewParentAccountActivityRegistration", ""));
            //newParentAccountActivityRegistrationPage.GuardianLastName(Parameter.Get("NewParentAccountActivityRegistration", ""));
            //newParentAccountActivityRegistrationPage.GuardianEmailElement(Parameter.Get("NewParentAccountActivityRegistration", ""));
            //newParentAccountActivityRegistrationPage.GuardianMobileElement(Parameter.Get("NewParentAccountActivityRegistration", ""));
            //newParentAccountActivityRegistrationPage.ClickHomeAddressNextBtn();
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
