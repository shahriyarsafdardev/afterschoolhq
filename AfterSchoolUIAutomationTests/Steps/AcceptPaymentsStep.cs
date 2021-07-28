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
    public sealed class AcceptPaymentsStep : IDisposable
    {
        private readonly ScenarioContext _scenarioContext;
        private IWebDriver webDriver;
        private NewProviderSignUpFlowPage newProviderSignUpFlowPage;
        private CreateProfilePage createProfilePage;
        private CreateActivityPage createActivityPage;
        private EmailConfirmationPage emailConfirmationPage;
        private AcceptPaymentsPage acceptPaymentsPage;

        public AcceptPaymentsStep(ScenarioContext scenarioContext)
        {
            _scenarioContext = scenarioContext;
            webDriver = new ChromeDriver(AppDomain.CurrentDomain.BaseDirectory);
            newProviderSignUpFlowPage = new NewProviderSignUpFlowPage(webDriver);
            createProfilePage = new CreateProfilePage(webDriver);
            createActivityPage = new CreateActivityPage(webDriver);
            emailConfirmationPage = new EmailConfirmationPage(webDriver);
            acceptPaymentsPage = new AcceptPaymentsPage(webDriver);
        }

        [Given(@"Click continue to your account link")]
        public void ContinueToAccount()
        {
            acceptPaymentsPage.ClickContinueToAccountButton();
        }

        [Given(@"Click more dropdown option button")]
        public void MoreDropdownOptions()
        {
            acceptPaymentsPage.ClickMoreDropdownButton();
        }

        [Given(@"Click enable payments button")]
        public void EnablePayments()
        {
            acceptPaymentsPage.ClickEnablePaymentsButton();
        }

        [Given(@"Click choose direct deposits button")]
        public void ChooseDirectDeposit()
        {
            acceptPaymentsPage.ClickChooseDirectDepositButton();
        }

        [Given(@"Populate get paid by after school hq form details and click next")]
        public void PopulateGetPaidByAfterSchoolHQForm()
        {
            acceptPaymentsPage.ClickIndividualSoleProprietorRadio();
            acceptPaymentsPage.ClickTestPhoneNumber();
            acceptPaymentsPage.ClickNextBtnGetPaid();
        }

        [Given(@"Populate personal form details and click next")]
        public void PopulatePersonalFormDetailsForm()
        {
            acceptPaymentsPage.PopulateFirstName(Parameter.Get("StripeAccountPayments", "FirstName"));
            acceptPaymentsPage.PopulateLastName(Parameter.Get("StripeAccountPayments", "LastName"));
            acceptPaymentsPage.PopulateDay(Parameter.Get("StripeAccountPayments", "DD"));
            acceptPaymentsPage.PopulateMonth(Parameter.Get("StripeAccountPayments", "MM"));
            acceptPaymentsPage.PopulateYear(Parameter.Get("StripeAccountPayments", "YYYY"));
            acceptPaymentsPage.PopulateAddressLine1(Parameter.Get("StripeAccountPayments", "AddressLine1"));
            acceptPaymentsPage.PopulateAddressLine2(Parameter.Get("StripeAccountPayments", "AddressLine2"));
            acceptPaymentsPage.PopulateCity(Parameter.Get("StripeAccountPayments", "City"));
            acceptPaymentsPage.PopulateState(Parameter.Get("StripeAccountPayments", "State"));
            acceptPaymentsPage.PopulateZip(Parameter.Get("StripeAccountPayments", "ZIP"));
            acceptPaymentsPage.PopulatePhoneNumber(Parameter.Get("StripeAccountPayments", "PhoneNumber"));
            acceptPaymentsPage.PopulateSSN(Parameter.Get("StripeAccountPayments", "SSN"));
            acceptPaymentsPage.ClickNextBtnPersonalDetail();
        }

        [Given(@"Populate bussiness details form and click next")]
        public void PopulateBussinessDetailsForm()
        {
            acceptPaymentsPage.ClickBussinessWebsiteDetails();
            acceptPaymentsPage.ClickNextBtnBussinessDetail();
        }

        [Given(@"Populate select an account for payouts and click next")]
        public void PopulateSelectAccountForPayoutsForm()
        {
            acceptPaymentsPage.ClickUsetestAccount();
        }

        [Given(@"Review your details and click save")]
        public void ReviewYourDetailsAndSave()
        {
            acceptPaymentsPage.ClickReviewYourDetails();
        }

        [Then(@"Verify account success message")]
        public void VerifyAccountSuccessMessage()
        {
            string create = "Account setup complete!";
            create.Equals(acceptPaymentsPage.AccountSetupComplete().Trim());
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
