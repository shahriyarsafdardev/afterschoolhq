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

        //------------Finding Create Activity WebElements-----------//

        //------------Settings Section Web Elements-----------//
        private IWebElement AccountButtonElement => webDriver.FindElement(By.XPath("(//div[@id='accounts']//button)[1]"));
        private IWebElement AddActivityElement => webDriver.FindElement(By.XPath("//div[@class='pull-right']"));
        private IWebElement ActivityName => webDriver.FindElement(By.Id("name"));
        private IWebElement TotalAvailableSeats => webDriver.FindElement(By.Id("seatsavailable"));
        private IWebElement InviteOnly => webDriver.FindElement(By.Id("inviteonly"));
        private IWebElement SignUpPeriodOpen => webDriver.FindElement(By.Id("registrationopendate"));
        private IWebElement StartDayElement => webDriver.FindElement(By.XPath("//div[@class='datetimepicker-days']//table/tbody/tr[2]/td[7]"));
        private IWebElement SignUpPeriodOpenTime => webDriver.FindElement(By.Id("registrationopentime"));
        private IWebElement SignUpPeriodClose => webDriver.FindElement(By.Id("registrationclosedate"));
        private IWebElement CloseDayElement => webDriver.FindElement(By.XPath("(//div[@class='datetimepicker-days']//table/tbody/tr[5]/td[4])[2]"));
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

        //------------About Your Activity Section Web Elements-----------
        private IWebElement AboutYourActivityElement => webDriver.FindElement(By.XPath("//div[@class='ql-editor ql-blank']"));

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
        public CreateActivityPage SetSignUpPeriodOpen()
        {
            webDriver.WaitInSeconds(2);
            SignUpPeriodOpen.Click();
            webDriver.WaitInSeconds(2);
            StartDayElement.Click();
            webDriver.WaitInSeconds(2);
            SignUpPeriodOpenTime.Click();
            var selectElement = new SelectElement(SignUpPeriodOpenTime);
            selectElement.SelectByValue("9:30 am");
            return this;
        }
        public CreateActivityPage SetSignUpPeriodClose()
        {
            webDriver.WaitInSeconds(3);
            SignUpPeriodClose.Click();
            webDriver.WaitInSeconds(2);
            CloseDayElement.Click();
            webDriver.WaitInSeconds(2);
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

        //------------Restrictions Section-----------//
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
            selectElement.SelectByValue("13");
            RestrictionHeadingElement.Click();
            return this;
        }

        //------------Schedule Section-----------
        public CreateActivityPage HasSchedule(string hasSchedule)
        {
            if (hasSchedule.Equals("Yes"))
            {
                webDriver.WaitInSeconds(2);
                HasScheduleElement.Click();
                return this;
            }
            return this;
        }
        public CreateActivityPage RepeatsWeekly(string isWeekly)
        {
            if (isWeekly.Equals("Yes"))
            {
                webDriver.WaitInSeconds(2);
                RepeatsWeeklyElement.Click();
                return this;
            }
            return this;
        }
        public CreateActivityPage WeekDays()
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
        public CreateActivityPage SetScheduleStartDate()
        {
            webDriver.WaitInSeconds(2);
            ScheduleStartDateElement.Click();
            webDriver.WaitInSeconds(2);
            ScheduleStartDayElement.Click();
            webDriver.WaitInSeconds(2);
            return this;
        }
        public CreateActivityPage SetScheduleEndDate()
        {
            webDriver.WaitInSeconds(3);
            ScheduleEndDateElement.Click();
            webDriver.WaitInSeconds(2);
            ScheduleEndDayElement.Click();
            webDriver.WaitInSeconds(2);
            return this;
        }
        public CreateActivityPage SetScheduleStartTime()
        {
            webDriver.WaitInSeconds(2);
            ScheduleStartTimeElement.Click();
            var selectElement = new SelectElement(ScheduleStartTimeElement);
            selectElement.SelectByValue("3:30 pm");
            return this;
        }
        public CreateActivityPage SetScheduleEndTime()
        {
            webDriver.WaitInSeconds(2);
            ScheduleEndTimeElement.Click();
            var selectElement = new SelectElement(ScheduleEndTimeElement);
            selectElement.SelectByValue("6:30 pm");
            return this;
        }

        //------------About Your Activity Section-----------//
        public CreateActivityPage ActivityDetails(string aboutActivity)
        {
            webDriver.WaitInSeconds(3);
            AboutYourActivityElement.SendKeys("Lorem ipsum dolor sit amet, consectetur adipiscing elit");

            //((IJavaScriptExecutor)webDriver).ExecuteScript("alert('Hello');");
            //((IJavaScriptExecutor)webDriver).ExecuteScript("quill.setText('About Your Activity');");
            //((IJavaScriptExecutor)webDriver).ExecuteScript("quill.setText('About Your Activity');");
            return this;
        }

        //------------Contact Section-----------//
        public CreateActivityPage ActivityLedBy()
        {
            webDriver.WaitInSeconds(2);
            ActivityLedByElement.Click();
            var selectElement = new SelectElement(ActivityLedByElement);
            selectElement.SelectByValue("1");
            return this;
        }
        public CreateActivityPage ContactName(string contactName)
        {
            webDriver.WaitInSeconds(2);
            ContactNameElement.Clear();
            ContactNameElement.SendKeys(contactName);
            return this;
        }
        public CreateActivityPage ContactPhone(string contactPhone)
        {
            webDriver.WaitInSeconds(2);
            ContactPhoneElement.Clear();
            ContactPhoneElement.SendKeys(contactPhone);
            return this;
        }
        public CreateActivityPage ContactEmail(string contactEmail)
        {
            webDriver.WaitInSeconds(2);
            ContactEmailElement.Clear();
            ContactEmailElement.SendKeys(contactEmail);
            return this;
        }

        //------------Location Section-----------
        public CreateActivityPage PhysicalLocation()
        {
            webDriver.WaitInSeconds(2);
            PhysicalLocationElement.Click();
            return this;
        }
        public CreateActivityPage SelectAutoComplete(string location)
        {
            webDriver.WaitInSeconds(2);
            ActivityLocationElement.SendKeys(location);
            Thread.Sleep(5000);
            WebDriverWait wait = new WebDriverWait(webDriver, new TimeSpan(0, 0, 5));
            wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.XPath("(//div[@class='pac-container pac-logo']//div[@class='pac-item'])[1]"))).Click();
            return this;
        }

        //------------Price & Payment Section-----------
        public CreateActivityPage DoParentsPay()
        {
            webDriver.WaitInSeconds(2);
            DoParentsPayElement.Click();
            return this;
        }
        public CreateActivityPage RegistrationFee(string fee)
        {
            webDriver.WaitInSeconds(2);
            RegistrationFeeElement.SendKeys(fee);
            return this;
        }
        public CreateActivityPage DoPriceParentpay()
        {
            webDriver.WaitInSeconds(2);
            PriceParentpayElement.Click();
            return this;
        }

        //------------Email Section-----------
        public CreateActivityPage ShowAdvanceSettings()
        {
            webDriver.WaitInSeconds(2);
            ShowAdvanceSettingElement.Click();
            return this;
        }
        public CreateActivityPage SaveActivity()
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
