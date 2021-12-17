using System;
using System.Threading;
using AfterSchoolUIAutomationTests.Helpers.Extensions;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;

namespace AfterSchoolUIAutomationTests.Pages
{
    public class CreateAndManageSessionsPage : IDisposable
    {
        private IWebDriver webDriver;
        public CreateAndManageSessionsPage(IWebDriver WebDriver)
        {
            webDriver = WebDriver;
        }

        #region Navigate to Sessions
        private IWebElement AccountBtnElement => webDriver.FindElement(By.XPath("(//div[@id='accounts']//button[@class='btn btn-primary btn-xl btn-space btn-block'])[1]"));
        private IWebElement ActivityCenterElement => webDriver.FindElement(By.XPath("//ul[@class='sidebar-elements']//li[3]"));
        private IWebElement SessionLinkElement => webDriver.FindElement(By.XPath("(//div[@class='content nano-content'])[1]//li[5]//a"));
        private IWebElement AddSessionBtnElement => webDriver.FindElement(By.XPath("(//div[@class='pull-right'])[1]//a"));

        public CreateAndManageSessionsPage HoverActivityCenterAndClickSession()
        {
            webDriver.WaitInSeconds(1);
            AccountBtnElement.Click();
            webDriver.WaitInSeconds(4);
            Actions action = new Actions(webDriver);
            action.MoveToElement(ActivityCenterElement).Perform();
            webDriver.WaitInSeconds(2);
            SessionLinkElement.Click();
            webDriver.WaitInSeconds(4);
            return this;
        }
        public CreateAndManageSessionsPage ClickAddSessionBtn()
        {
            webDriver.WaitInSeconds(1);
            AddSessionBtnElement.Click();
            webDriver.WaitInSeconds(4);
            return this;
        }
        #endregion

        #region Session Form
        private IWebElement SessionNameBtnElement => webDriver.FindElement(By.Id("name"));
        private IWebElement SessionDetailsElement => webDriver.FindElement(By.XPath("//div[@class='ql-editor ql-blank']"));
        private IWebElement RegistrationFormSelectElement => webDriver.FindElement(By.XPath("//select[@id='form']"));
        private IWebElement SessionFormSubmitElement => webDriver.FindElement(By.XPath("//button[@type='submit']"));

        public CreateAndManageSessionsPage PopulateSessionName(string sessionName)
        {
            SessionNameBtnElement.SendKeys(sessionName + DateTime.Now.ToString("yyyyMMddHHmmss").Substring(8));
            webDriver.WaitInSeconds(1);
            return this;
        }
        public CreateAndManageSessionsPage PopulateSessionDetails(string sessionDetails)
        {
            SessionDetailsElement.SendKeys(sessionDetails);
            webDriver.WaitInSeconds(1);
            return this;
        }
        public CreateAndManageSessionsPage SelectRegistrationForm()
        {
            RegistrationFormSelectElement.Click();
            webDriver.WaitInSeconds(2);
            var selectElement = new SelectElement(RegistrationFormSelectElement);
            selectElement.SelectByText("Standard Reg Form");
            webDriver.WaitInSeconds(1);
            return this;
        }
        public CreateAndManageSessionsPage SubmitForm()
        {
            SessionFormSubmitElement.Click();
            webDriver.WaitInSeconds(3);
            return this;
        }
        #endregion

        #region SignupPeriodsTab
        private IWebElement SignupPeriodsTabElement => webDriver.FindElement(By.XPath("//div[@class='btn-group hidden-xs']//a[4]"));
        private IWebElement AddSignupPeriodsBtnElement => webDriver.FindElement(By.XPath("//button[@class='btn btn-primary btn-space margin-r-10 md-trigger']"));
        private IWebElement SignupPeriodSubmitBtnElement => webDriver.FindElement(By.XPath("(//button[@type='submit'][@class='btn btn-primary'])[1]"));
        public CreateAndManageSessionsPage NavigateToSignupPeriodsTab()
        {
            SignupPeriodsTabElement.Click();
            webDriver.WaitInSeconds(2);
            return this;
        }
        public CreateAndManageSessionsPage BringSignUpPeriodsModal()
        {
            AddSignupPeriodsBtnElement.Click();
            webDriver.WaitInSeconds(2);
            return this;
        }
        public CreateAndManageSessionsPage AddSignUpPeriodOpenCloseDate()
        {
            SignupPeriodSubmitBtnElement.Click();
            webDriver.WaitInSeconds(4);
            return this;
        }
        #endregion

        #region Create Activity Settings
        private IWebElement ActivitiesTabElement => webDriver.FindElement(By.XPath("//div[@class='btn-group hidden-xs']//a[5]"));
        private IWebElement AddActivtyBtnElement => webDriver.FindElement(By.XPath("//div[@class='pull-right']//a"));
        private IWebElement ActivityNameElement => webDriver.FindElement(By.Id("name"));
        private IWebElement SettingsSaveAndNextBtnElement => webDriver.FindElement(By.XPath("//div[@class='col-sm-3']//button[text()='Save & Next »']"));
        public CreateAndManageSessionsPage NavigateToActivtiesTab()
        {
            ActivitiesTabElement.Click();
            webDriver.WaitInSeconds(2);
            return this;
        }
        public CreateAndManageSessionsPage BringActivitySectionsForms()
        {
            AddActivtyBtnElement.Click();
            webDriver.WaitInSeconds(2);
            return this;
        }
        public CreateAndManageSessionsPage PopulateActivityName(string activityName)
        {
            ActivityNameElement.SendKeys(activityName);
            webDriver.WaitInSeconds(1);
            return this;
        }
        public CreateAndManageSessionsPage ClickSettingsSaveAndNextBtn()
        {
            SettingsSaveAndNextBtnElement.Click();
            webDriver.WaitInSeconds(4);
            return this;
        }
        #endregion

        #region Activity Restrictions
        private IWebElement GenderElement => webDriver.FindElement(By.XPath("//div[@class='am-radio inline']//label[2]"));
        private IWebElement RestrictedAgeElement => webDriver.FindElement(By.XPath("//div[@class='col-lg-9 margin-left-sm']//div[@class='am-radio inline'][2]//label"));
        private IWebElement RestrictedGradeElement => webDriver.FindElement(By.XPath("//div[@class='col-lg-9 margin-left-sm']//div[@class='am-radio inline'][1]//label"));
        private IWebElement MinGradeDropDownElement => webDriver.FindElement(By.Id("mingrade"));
        private IWebElement MaxGradeDropDownElement => webDriver.FindElement(By.Id("maxgrade"));
        private IWebElement RestrictionSubmitBtmElement => webDriver.FindElement(By.XPath("//button[@type='submit'][text()='Save & Next »']"));
        public CreateAndManageSessionsPage SelectGender()
        {
            webDriver.WaitInSeconds(1);
            GenderElement.Click();
            return this;
        }
        public CreateAndManageSessionsPage RestrictedBy()
        {
            webDriver.WaitInSeconds(1);
            RestrictedAgeElement.Click();
            webDriver.WaitInSeconds(2);
            RestrictedGradeElement.Click();
            return this;
        }
        public CreateAndManageSessionsPage MinimumGrade()
        {
            webDriver.WaitInSeconds(2);
            MinGradeDropDownElement.Click();
            webDriver.WaitInSeconds(1);
            var selectElement = new SelectElement(MinGradeDropDownElement);
            selectElement.SelectByValue("2");
            return this;
        }
        public CreateAndManageSessionsPage MaximumGrade()
        {
            webDriver.WaitInSeconds(2);
            MaxGradeDropDownElement.Click();
            webDriver.WaitInSeconds(1);
            var selectElement = new SelectElement(MaxGradeDropDownElement);
            selectElement.SelectByValue("14");
            webDriver.WaitInSeconds(1);
            return this;
        }
        public CreateAndManageSessionsPage ClickRestrictionsSaveAndNextBtn()
        {
            RestrictionSubmitBtmElement.Click();
            webDriver.WaitInSeconds(4);
            return this;
        }
        #endregion

        #region Activity Schedule
        private IWebElement HasScheduleElement => webDriver.FindElement(By.XPath("(//div[@class='switch-button switch-button-yesno ']//label)[1]"));
        private IWebElement RepeatsWeeklyElement => webDriver.FindElement(By.XPath("(//div[@class='switch-button switch-button-yesno ']//label)[2]"));
        private IWebElement DaysofWeekMondayElement => webDriver.FindElement(By.XPath("(//div[@class='am-checkbox display-days']//label)[2]"));
        private IWebElement DaysofWeekTuesdayElement => webDriver.FindElement(By.XPath("(//div[@class='am-checkbox display-days']//label)[3]"));
        private IWebElement DaysofWeekThursdayElement => webDriver.FindElement(By.XPath("(//div[@class='am-checkbox display-days']//label)[5]"));
        private IWebElement DaysofWeekFridayElement => webDriver.FindElement(By.XPath("(//div[@class='am-checkbox display-days']//label)[6]"));
        private IWebElement ScheduleStartDateElement => webDriver.FindElement(By.Id("startdate"));
        private IWebElement ScheduleStartDayElement => webDriver.FindElement(By.XPath("//div[@class='datetimepicker-days']//table/tbody/tr[3]/td[5]"));
        private IWebElement ScheduleEndDateElement => webDriver.FindElement(By.Id("enddate"));
        private IWebElement ScheduleEndDayElement => webDriver.FindElement(By.XPath("(//div[@class='datetimepicker-days']//table/tbody/tr[3]/td[6])[2]"));
        private IWebElement ScheduleStartTimeElement => webDriver.FindElement(By.Id("dailystarttime"));
        private IWebElement ScheduleEndTimeElement => webDriver.FindElement(By.Id("dailyendtime"));
        private IWebElement ScheduleSubmitBtnElement => webDriver.FindElement(By.XPath("//button[@type='submit'][text()='Save & Next »']"));

        public CreateAndManageSessionsPage HasSchedule(string hasSchedule)
        {
            if (hasSchedule.Equals("Yes"))
            {
                webDriver.WaitInSeconds(2);
                HasScheduleElement.Click();
                return this;
            }
            return this;
        }
        public CreateAndManageSessionsPage RepeatsWeekly(string isWeekly)
        {
            if (isWeekly.Equals("Yes"))
            {
                webDriver.WaitInSeconds(2);
                RepeatsWeeklyElement.Click();
                return this;
            }
            return this;
        }
        public CreateAndManageSessionsPage WeekDays()
        {
            webDriver.WaitInSeconds(2);
            DaysofWeekMondayElement.Click();
            webDriver.WaitInSeconds(1);
            DaysofWeekTuesdayElement.Click();
            webDriver.WaitInSeconds(1);
            DaysofWeekThursdayElement.Click();
            webDriver.WaitInSeconds(1);
            DaysofWeekFridayElement.Click();
            webDriver.WaitInSeconds(1);
            return this;
        }
        public CreateAndManageSessionsPage SetScheduleStartDate()
        {
            webDriver.WaitInSeconds(2);
            ScheduleStartDateElement.Click();
            webDriver.WaitInSeconds(2);
            ScheduleStartDayElement.Click();
            webDriver.WaitInSeconds(2);
            return this;
        }
        public CreateAndManageSessionsPage SetScheduleEndDate()
        {
            webDriver.WaitInSeconds(3);
            ScheduleEndDateElement.Click();
            webDriver.WaitInSeconds(2);
            ScheduleEndDayElement.Click();
            webDriver.WaitInSeconds(2);
            return this;
        }
        public CreateAndManageSessionsPage SetScheduleStartTime()
        {
            webDriver.WaitInSeconds(2);
            ScheduleStartTimeElement.Click();
            var selectElement = new SelectElement(ScheduleStartTimeElement);
            selectElement.SelectByValue("3:30 pm");
            return this;
        }
        public CreateAndManageSessionsPage SetScheduleEndTime()
        {
            webDriver.WaitInSeconds(2);
            ScheduleEndTimeElement.Click();
            var selectElement = new SelectElement(ScheduleEndTimeElement);
            selectElement.SelectByValue("6:30 pm");
            return this;
        }
        public CreateAndManageSessionsPage ClickScheduleSaveAndNextBtn()
        {
            ScheduleSubmitBtnElement.Click();
            webDriver.WaitInSeconds(4);
            return this;
        }
        #endregion

        #region About Your Activity
        private IWebElement AboutYourActivityElement => webDriver.FindElement(By.XPath("//div[@class='ql-editor ql-blank']"));
        private IWebElement AboutSubmitBtnElement => webDriver.FindElement(By.XPath("//button[@type='submit'][text()='Save & Next »']"));
        public CreateAndManageSessionsPage ActivityDetails(string aboutActivity)
        {
            webDriver.WaitInSeconds(3);
            AboutYourActivityElement.SendKeys(aboutActivity);
            return this;
        }
        public CreateAndManageSessionsPage ClickAboutSaveAndNextBtn()
        {
            AboutSubmitBtnElement.Click();
            webDriver.WaitInSeconds(4);
            return this;
        }
        #endregion

        #region Actvity Contact
        private IWebElement ActivityLedByElement => webDriver.FindElement(By.Id("leadertype"));
        private IWebElement ContactNameElement => webDriver.FindElement(By.Id("contactname"));
        private IWebElement ContactPhoneElement => webDriver.FindElement(By.Id("contactphone"));
        private IWebElement ContactEmailElement => webDriver.FindElement(By.Id("contactemail"));
        private IWebElement ContactSubmitBtnElement => webDriver.FindElement(By.XPath("//button[@type='submit'][text()='Save & Next »']"));

        public CreateAndManageSessionsPage ActivityLedBy()
        {
            webDriver.WaitInSeconds(2);
            ActivityLedByElement.Click();
            var selectElement = new SelectElement(ActivityLedByElement);
            selectElement.SelectByValue("1");
            return this;
        }
        public CreateAndManageSessionsPage ContactName(string contactName)
        {
            webDriver.WaitInSeconds(1);
            ContactNameElement.Clear();
            ContactNameElement.SendKeys(contactName);
            return this;
        }
        public CreateAndManageSessionsPage ContactPhone(string contactPhone)
        {
            webDriver.WaitInSeconds(1);
            ContactPhoneElement.Clear();
            ContactPhoneElement.SendKeys(contactPhone);
            return this;
        }
        public CreateAndManageSessionsPage ContactEmail(string contactEmail)
        {
            webDriver.WaitInSeconds(1);
            ContactEmailElement.Clear();
            ContactEmailElement.SendKeys(contactEmail);
            return this;
        }
        public CreateAndManageSessionsPage ClickContactSaveAndNextBtn()
        {
            ContactSubmitBtnElement.Click();
            webDriver.WaitInSeconds(4);
            return this;
        }
        #endregion

        #region Location
        private IWebElement PhysicalLocationElement => webDriver.FindElement(By.XPath("//div[@class='am-radio']//span[2]//label"));
        private IWebElement ActivityLocationElement => webDriver.FindElement(By.Id("location"));
        private IWebElement LocationSubmitBtnElement => webDriver.FindElement(By.XPath("//button[@type='submit'][text()='Save & Next »']"));
        public CreateAndManageSessionsPage PhysicalLocation()
        {
            webDriver.WaitInSeconds(2);
            PhysicalLocationElement.Click();
            return this;
        }
        public CreateAndManageSessionsPage SelectAutoComplete(string location)
        {
            webDriver.WaitInSeconds(2);
            ActivityLocationElement.Clear();
            ((IJavaScriptExecutor)webDriver).ExecuteScript("document.getElementById('location').setAttribute('value', '')");
            ActivityLocationElement.SendKeys(location);
            Thread.Sleep(5000);
            WebDriverWait wait = new WebDriverWait(webDriver, new TimeSpan(0, 0, 5));
            wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.XPath("(//div[@class='pac-container pac-logo']//div[@class='pac-item'])[1]"))).Click();
            return this;
        }
        public CreateAndManageSessionsPage ClickLocationSaveAndNextBtn()
        {
            LocationSubmitBtnElement.Click();
            webDriver.WaitInSeconds(4);
            return this;
        }
        #endregion

        #region Price & Payment
        private IWebElement DoParentsPayElement => webDriver.FindElement(By.XPath("//div[@class='col-sm-9']//div[@class='am-radio inline'][2]//label"));
        private IWebElement RegistrationFeeElement => webDriver.FindElement(By.XPath("//div[@class='input-group input-group-lg xs-mb-15']//input"));
        private IWebElement PriceParentpayElement => webDriver.FindElement(By.XPath("//div[@class='am-checkbox']//label"));
        private IWebElement PriceSubmitBtnElement => webDriver.FindElement(By.XPath("//button[@type='submit'][text()='Save & Review »']"));
        private IWebElement ActivityPublicationElement => webDriver.FindElement(By.XPath("//div[@class='widget-head']//p"));

        public CreateAndManageSessionsPage DoParentsPay()
        {
            webDriver.WaitInSeconds(2);
            DoParentsPayElement.Click();
            return this;
        }
        public CreateAndManageSessionsPage RegistrationFee(string fee)
        {
            webDriver.WaitInSeconds(2);
            RegistrationFeeElement.SendKeys(fee);
            return this;
        }
        public CreateAndManageSessionsPage DoPriceParentpay()
        {
            webDriver.WaitInSeconds(2);
            PriceParentpayElement.Click();
            return this;
        }
        public CreateAndManageSessionsPage ClickPriceSaveAndNextBtn()
        {
            PriceSubmitBtnElement.Click();
            webDriver.WaitInSeconds(4);
            return this;
        }
        public string ActivityPublished()
        {
            webDriver.WaitInSeconds(1);
            return ActivityPublicationElement.Text;
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
