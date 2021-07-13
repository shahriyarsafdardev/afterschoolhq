using System;
using System.Threading;
using AfterSchoolUIAutomationTests.Helpers.Extensions;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace AfterSchoolUIAutomationTests.Pages
{
    public class EditActivityPage
    {
        private IWebDriver webDriver;

        public EditActivityPage(IWebDriver WebDriver)
        {
            webDriver = WebDriver;
        }

        //------------Finding Create Activity WebElements-----------//

        //------------Settings Section Web Elements-----------//
        private IWebElement AccountButtonElement => webDriver.FindElement(By.XPath("(//div[@id='accounts']//button)[1]"));
        private IWebElement ActivitiesElement => webDriver.FindElement(By.XPath("//ul[@class='sidebar-elements']//li[3]"));
        private IWebElement ManageActivityElement => webDriver.FindElement(By.XPath("//div[@class='table-responsive']//table/tbody/tr[6]/td[1]"));
        private IWebElement EditActivityElement => webDriver.FindElement(By.XPath("//div[@class='pull-right visible-lg visible-md']"));
        private IWebElement ActivityName => webDriver.FindElement(By.Id("name"));
        private IWebElement TotalAvailableSeats => webDriver.FindElement(By.Id("seatsavailable"));
        private IWebElement InviteOnly => webDriver.FindElement(By.Id("inviteonly"));
        private IWebElement SignUpPeriodOpen => webDriver.FindElement(By.Id("registrationopendate"));
        private IWebElement StartDayElement => webDriver.FindElement(By.XPath("//div[@class='datetimepicker-days']//table/tbody/tr[2]/td[5]"));
        private IWebElement SignUpPeriodSelectElement => webDriver.FindElement(By.Id("registrationperiod"));
        private IWebElement ParentalConsentElement => webDriver.FindElement(By.XPath("//div[@class='switch-button switch-button-yesno ']//span//label"));
        private IWebElement SignUpPeriodOpenTime => webDriver.FindElement(By.Id("registrationopentime"));
        private IWebElement SignUpPeriodClose => webDriver.FindElement(By.Id("registrationclosedate"));
        private IWebElement CloseDayElement => webDriver.FindElement(By.XPath("(//div[@class='datetimepicker-days']//table/tbody/tr[5]/td[6])[2]"));
        private IWebElement SignUpPeriodCloseTime => webDriver.FindElement(By.Id("registrationclosetime"));
        private IWebElement SaveAndNextElement => webDriver.FindElement(By.XPath("//div[@class='col-sm-3']//button[text()='Save & Next »']"));

        //------------Restrictions Section Web Elements-----------//
        private IWebElement RestrictionHeadingElement => webDriver.FindElement(By.XPath("//div[@class='panel-heading activity-heading']"));
        private IWebElement GenderElement => webDriver.FindElement(By.XPath("//div[@class='am-radio inline']//label[2]"));
        private IWebElement RestrictedAgeElement => webDriver.FindElement(By.XPath("//div[@class='col-lg-9 margin-left-sm']//div[@class='am-radio inline'][2]//label"));
        private IWebElement RestrictedGradeElement => webDriver.FindElement(By.XPath("//div[@class='col-lg-9 margin-left-sm']//div[@class='am-radio inline'][1]//label"));
        private IWebElement MinGradeDropDownElement => webDriver.FindElement(By.Id("mingrade"));
        private IWebElement MaxGradeDropDownElement => webDriver.FindElement(By.Id("maxgrade"));

        //------------Schedule Section Web Elements-----------//
        private IWebElement HasScheduleElement => webDriver.FindElement(By.XPath("(//div[@class='switch-button switch-button-yesno ']//label)[1]"));
        private IWebElement RepeatsWeeklyElement => webDriver.FindElement(By.XPath("(//div[@class='switch-button switch-button-yesno ']//label)[2]"));
        private IWebElement DaysofWeekTuesdayElement => webDriver.FindElement(By.XPath("(//div[@class='am-checkbox display-days']//label)[3]"));
        private IWebElement DaysofWeekWednesdayElement => webDriver.FindElement(By.XPath("(//div[@class='am-checkbox display-days']//label)[4]"));
        private IWebElement DaysofWeekSaturdayElement => webDriver.FindElement(By.XPath("(//div[@class='am-checkbox display-days']//label)[7]"));
        private IWebElement ScheduleStartDateElement => webDriver.FindElement(By.Id("startdate"));
        private IWebElement ScheduleStartDayElement => webDriver.FindElement(By.XPath("//div[@class='datetimepicker-days']//table/tbody/tr[3]/td[3]"));
        private IWebElement ScheduleEndDateElement => webDriver.FindElement(By.Id("enddate"));
        private IWebElement ScheduleEndDayElement => webDriver.FindElement(By.XPath("(//div[@class='datetimepicker-days']//table/tbody/tr[3]/td[4])[2]"));
        private IWebElement ScheduleStartTimeElement => webDriver.FindElement(By.Id("dailystarttime"));
        private IWebElement ScheduleEndTimeElement => webDriver.FindElement(By.Id("dailyendtime"));

        //------------About Your Activity Section Web Elements-----------
        private IWebElement AboutYourActivityEmptyElement => webDriver.FindElement(By.XPath("//div[@class='ql-editor ql-blank']"));
        private IWebElement AboutYourActivityElement => webDriver.FindElement(By.XPath("//div[@class='ql-editor']"));

        //------------Contact Section Web Elements-----------
        private IWebElement ActivityLedByElement => webDriver.FindElement(By.Id("leadertype"));
        private IWebElement ContactNameElement => webDriver.FindElement(By.Id("contactname"));
        private IWebElement ContactPhoneElement => webDriver.FindElement(By.Id("contactphone"));
        private IWebElement ContactEmailElement => webDriver.FindElement(By.Id("contactemail"));

        //------------Location Section Web Elements-----------
        private IWebElement PhysicalLocationElement => webDriver.FindElement(By.XPath("//div[@class='am-radio']//span[2]//label"));
        private IWebElement ActivityLocationElement => webDriver.FindElement(By.Id("location"));

        //------------Price & Payment Section Web Elements-----------
        private IWebElement DoParentsPayElement => webDriver.FindElement(By.XPath("//div[@class='col-sm-9']//div[@class='am-radio inline'][2]//label"));
        private IWebElement RegistrationFeeElement => webDriver.FindElement(By.XPath("//div[@class='input-group input-group-lg xs-mb-15']//input"));
        private IWebElement PriceParentpayElement => webDriver.FindElement(By.XPath("//div[@class='am-checkbox']//label"));
        //------------Email Section Web Elements-----------
        private IWebElement ShowAdvanceSettingElement => webDriver.FindElement(By.Id("btn-advanced-settings"));
        private IWebElement SaveButtonElement => webDriver.FindElement(By.XPath("//div[@class='col-sm-6 col-sm-offset-3']//button[text()=' Save']"));


        //------------Edit Account Profile Helper Methods-----------//

        //------------Settings Section-----------//
        public EditActivityPage ClickAccountButton()
        {
            AccountButtonElement.Click();
            webDriver.WaitInSeconds(5);
            return this;
        }
        public EditActivityPage ClickActivitiesLink()
        {
            webDriver.WaitInSeconds(1);
            ActivitiesElement.Click();
            webDriver.WaitInSeconds(3);
            return this;
        }
        public EditActivityPage ClickManageButton()
        {
            ManageActivityElement.Click();
            webDriver.WaitInSeconds(3);
            return this;
        }
        public EditActivityPage ClickEditActivityLink()
        {
            EditActivityElement.Click();
            webDriver.WaitInSeconds(3);
            return this;
        }
        public EditActivityPage PopulateActivityName(string activityName)
        {
            webDriver.WaitInSeconds(1);
            ActivityName.Clear();
            webDriver.WaitInSeconds(1);
            ActivityName.SendKeys(activityName);
            return this;
        }
        public EditActivityPage PopulateTotalAvailableSeats(string availableSeats)
        {
            webDriver.WaitInSeconds(2);
            TotalAvailableSeats.Clear();
            webDriver.WaitInSeconds(1);
            TotalAvailableSeats.SendKeys(availableSeats);
            return this;
        }

        public EditActivityPage SelectNewDates()
        {
            SignUpPeriodSelectElement.Click();
            var selectElement = new SelectElement(SignUpPeriodSelectElement);
            selectElement.SelectByValue("0");
            return this;
        }

        public EditActivityPage ToggleInviteOnly(string isInviteOnly)
        {
            webDriver.WaitInSeconds(1);
            if (isInviteOnly.Equals("Yes"))
            {
                InviteOnly.Click();
            }
            return this;
        }
        public EditActivityPage SetSignUpPeriodOpen()
        {
            webDriver.WaitInSeconds(2);
            SignUpPeriodOpen.Click();
            webDriver.WaitInSeconds(2);
            StartDayElement.Click();
            webDriver.WaitInSeconds(2);
            SignUpPeriodOpenTime.Click();
            var selectElement = new SelectElement(SignUpPeriodOpenTime);
            selectElement.SelectByValue("10:30 am");
            return this;
        }
        public EditActivityPage SetSignUpPeriodClose()
        {
            webDriver.WaitInSeconds(3);
            SignUpPeriodClose.Click();
            webDriver.WaitInSeconds(2);
            CloseDayElement.Click();
            webDriver.WaitInSeconds(2);
            SignUpPeriodCloseTime.Click();
            var selectElement = new SelectElement(SignUpPeriodCloseTime);
            selectElement.SelectByValue("12:30 pm");
            return this;
        }
        public EditActivityPage ClickSaveAndNext()
        {
            webDriver.WaitInSeconds(2);
            SaveAndNextElement.Click();
            webDriver.WaitInSeconds(4);
            return this;
        }

        //------------Restrictions Section-----------//
        public EditActivityPage ParentalConsent()
        {
            webDriver.WaitInSeconds(1);
            ParentalConsentElement.Click();
            return this;
        }
        public EditActivityPage SelectGender()
        {
            webDriver.WaitInSeconds(1);
            GenderElement.Click();
            return this;
        }
        public EditActivityPage RestrictedBy()
        {
            webDriver.WaitInSeconds(1);
            RestrictedAgeElement.Click();
            webDriver.WaitInSeconds(2);
            RestrictedGradeElement.Click();
            return this;
        }
        public EditActivityPage MinimumGrade()
        {
            webDriver.WaitInSeconds(2);
            MinGradeDropDownElement.Click();
            webDriver.WaitInSeconds(1);
            var selectElement = new SelectElement(MinGradeDropDownElement);
            selectElement.SelectByValue("5");
            RestrictionHeadingElement.Click();
            return this;
        }
        public EditActivityPage MaximumGrade()
        {
            webDriver.WaitInSeconds(2);
            MaxGradeDropDownElement.Click();
            webDriver.WaitInSeconds(1);
            var selectElement = new SelectElement(MaxGradeDropDownElement);
            selectElement.SelectByValue("14");
            RestrictionHeadingElement.Click();
            return this;
        }

        //------------Schedule Section-----------
        public EditActivityPage HasSchedule(string hasSchedule)
        {
            if (hasSchedule.Equals("Yes"))
            {
                webDriver.WaitInSeconds(2);
                HasScheduleElement.Click();
                return this;
            }
            return this;
        }
        public EditActivityPage RepeatsWeekly(string isWeekly)
        {
            if (isWeekly.Equals("Yes"))
            {
                webDriver.WaitInSeconds(2);
                RepeatsWeeklyElement.Click();
                return this;
            }
            return this;
        }
        public EditActivityPage WeekDays()
        {
            webDriver.WaitInSeconds(2);
            DaysofWeekTuesdayElement.Click();
            webDriver.WaitInSeconds(1);
            DaysofWeekWednesdayElement.Click();
            webDriver.WaitInSeconds(1);
            DaysofWeekSaturdayElement.Click();
            webDriver.WaitInSeconds(1);
            return this;
        }
        public EditActivityPage SetScheduleStartDate()
        {
            webDriver.WaitInSeconds(2);
            ScheduleStartDateElement.Click();
            webDriver.WaitInSeconds(2);
            ScheduleStartDayElement.Click();
            webDriver.WaitInSeconds(2);
            return this;
        }
        public EditActivityPage SetScheduleEndDate()
        {
            webDriver.WaitInSeconds(3);
            ScheduleEndDateElement.Click();
            webDriver.WaitInSeconds(2);
            ScheduleEndDayElement.Click();
            webDriver.WaitInSeconds(2);
            return this;
        }
        public EditActivityPage SetScheduleStartTime()
        {
            webDriver.WaitInSeconds(2);
            ScheduleStartTimeElement.Click();
            var selectElement = new SelectElement(ScheduleStartTimeElement);
            selectElement.SelectByValue("4:30 pm");
            return this;
        }
        public EditActivityPage SetScheduleEndTime()
        {
            webDriver.WaitInSeconds(2);
            ScheduleEndTimeElement.Click();
            var selectElement = new SelectElement(ScheduleEndTimeElement);
            selectElement.SelectByValue("7:30 pm");
            return this;
        }

        //------------About Your Activity Section-----------//
        public EditActivityPage ActivityDetails(string aboutActivity)
        {
            webDriver.WaitInSeconds(2);
            AboutYourActivityElement.Clear();
            webDriver.WaitInSeconds(2);
            AboutYourActivityEmptyElement.SendKeys("Lorem ipsum dolor sit amet, consectetur adipiscing elit");
            return this;
        }

        //------------Contact Section-----------//
        public EditActivityPage ActivityLedBy()
        {
            webDriver.WaitInSeconds(2);
            ActivityLedByElement.Click();
            var selectElement = new SelectElement(ActivityLedByElement);
            selectElement.SelectByValue("1");
            return this;
        }
        public EditActivityPage ContactName(string contactName)
        {
            webDriver.WaitInSeconds(2);
            ContactNameElement.Clear();
            webDriver.WaitInSeconds(1);
            ContactNameElement.SendKeys(contactName);
            return this;
        }
        public EditActivityPage ContactPhone(string contactPhone)
        {
            webDriver.WaitInSeconds(2);
            ContactPhoneElement.Clear();
            webDriver.WaitInSeconds(1);
            ContactPhoneElement.SendKeys(contactPhone);
            return this;
        }
        public EditActivityPage ContactEmail(string contactEmail)
        {
            webDriver.WaitInSeconds(2);
            ContactEmailElement.Clear();
            webDriver.WaitInSeconds(1);
            ContactEmailElement.SendKeys(contactEmail);
            return this;
        }

        //------------Location Section-----------
        public EditActivityPage PhysicalLocation()
        {
            webDriver.WaitInSeconds(2);
            PhysicalLocationElement.Click();
            return this;
        }
        public EditActivityPage SelectAutoComplete(string location)
        {
            webDriver.WaitInSeconds(2);
            ActivityLocationElement.SendKeys(location);
            Thread.Sleep(5000);
            WebDriverWait wait = new WebDriverWait(webDriver, new TimeSpan(0, 0, 5));
            wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.XPath("(//div[@class='pac-container pac-logo']//div[@class='pac-item'])[1]"))).Click();
            return this;
        }

        //------------Price & Payment Section-----------
        public EditActivityPage DoParentsPay()
        {
            webDriver.WaitInSeconds(2);
            DoParentsPayElement.Click();
            return this;
        }
        public EditActivityPage RegistrationFee(string fee)
        {
            webDriver.WaitInSeconds(2);
            RegistrationFeeElement.Clear();
            webDriver.WaitInSeconds(2);
            RegistrationFeeElement.SendKeys(fee);
            return this;
        }
        public EditActivityPage DoPriceParentpay()
        {
            webDriver.WaitInSeconds(2);
            PriceParentpayElement.Click();
            webDriver.WaitInSeconds(2);
            PriceParentpayElement.Click();
            return this;
        }

        //------------Email Section-----------
        public EditActivityPage ShowAdvanceSettings()
        {
            webDriver.WaitInSeconds(2);
            ShowAdvanceSettingElement.Click();
            return this;
        }
        public EditActivityPage SaveActivity()
        {
            webDriver.WaitInSeconds(2);
            SaveButtonElement.Click();
            webDriver.WaitInSeconds(5);
            return this;
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