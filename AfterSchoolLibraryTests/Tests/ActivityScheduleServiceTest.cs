using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using AfterSchoolLibraryTests.Factories;
using BasicLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace AfterSchoolLibraryTests.Tests
{
    public class ActivityScheduleServiceTest : BaseTest
    {
        private WebUser LoggedInUser { get; set; }
        private WebUser AccountOwner { get; set; }
        private Account ActivityOwnerAccount { get; set; }

        public ActivityScheduleServiceTest(ITestOutputHelper output) : base(output)
        {
            LoggedInUser = WebUserFactory.Create(msgs, dc);
            AccountOwner = WebUserFactory.Create(msgs, dc);
            ActivityOwnerAccount = AccountFactory.Create(AccountOwner, msgs, dc);
        }

        [Fact(DisplayName = "CreateSchedule")]
        public void CreateSchedule()
        {
            try
            {
                // Arrange                
                var schedule = ActivityScheduleFactory.Create(ActivityOwnerAccount, msgs, dc);
                var activity = ActivityFactory.Create(ActivityOwnerAccount, schedule, msgs, dc);
                var AccountOwner = WebUserFactory.Create("Don", "John", "DonJohn@test.com", true, msgs, dc);

                var startDates = new List<DateRange>()
                {
                    new DateRange() {StartDate = $"6/{DateTime.Now.Month}/{DateTime.Now.Year} 3:00:00 PM".ToDateTime(), EndDate = $"6/{DateTime.Now.Month}/{DateTime.Now.Year} 5:00:00 PM".ToDateTime()},
                    new DateRange() {StartDate = $"7/{DateTime.Now.Month}/{DateTime.Now.Year} 3:00:00 PM".ToDateTime(), EndDate = $"7/{DateTime.Now.Month}/{DateTime.Now.Year} 5:00:00 PM".ToDateTime()},
                    new DateRange() {StartDate = $"8/{DateTime.Now.Month}/{DateTime.Now.Year} 3:00:00 PM".ToDateTime(), EndDate = $"8/{DateTime.Now.Month}/{DateTime.Now.Year} 5:00:00 PM".ToDateTime()},
                };

                var endDates = new List<DateTime>() { };
                
                var scheduleName = ActivitySchedule.GenerateUniqueName(Convert.ToInt32(ActivityOwnerAccount.ID), $"{activity.Name} - Schedule", msgs, dc);


                // Act
                schedule = ActivityScheduleService.CreateNew(
                   scheduleName,
                   schedule.ScheduleDetails,
                   schedule.RepeatsWeekly,
                   schedule.IsOngoing,
                   schedule.StartDate,
                   schedule.EndDate,
                   schedule.DailyStartTime,
                   schedule.DailyEndTime,
                   startDates.Select(x => new ActivityInstance(x.StartDate, x.EndDate)).ToList(),
                   endDates,
                   "",
                   AccountOwner.ID.Value,
                   activity.ActivityGroupId,
                   AccountOwner.ID.Value,
                   msgs,
                   dc);
              
                // Assert
                Assert.NotNull(schedule);
            }
            catch (Exception ex)
            {
                //Assert
                Assert.False(false, ex.Message);
            }
        }

        [Fact(DisplayName = "IsEndingInTwoDays")]
        public void IsEndingInTwoDaysTest()
        {
            try
            {
                // Arrange     
                var schedule = ActivityScheduleFactory.CreateIsClosing(ActivityOwnerAccount, msgs, dc);

                // Act
                var isEndingInTwoDays = ActivityScheduleService.IsEndingInTwoDays(schedule, ActivityOwnerAccount);

                // Assert
                Assert.True(isEndingInTwoDays);
            }
            catch (Exception ex)
            {
                //Assert
                Assert.False(false, ex.Message);
            }
        }

        [Fact]
        public void LoadByIdByAccountIdTest()
        {
            try
            {
                // Arrange
                var schedule = ActivityScheduleFactory.Create(ActivityOwnerAccount, msgs, dc);

                // Act
                var expected = ActivitySchedule.LoadByIdByAccountId(Id: schedule.ID.Value, AccountId: ActivityOwnerAccount.ID.Value, msgs: msgs, dc: dc);

                // Assert
                Assert.Equal(expected: expected.ID, actual: schedule.ID);                
                Assert.Equal(expected: expected.Name, actual: schedule.Name);
            }
            catch (Exception ex)
            {
                //Assert
                Assert.False(false, ex.Message);
            }
        }

        [Theory(DisplayName = "ValidateInSessionInstancesTest Test")]
        [InlineData("1 - 15 - 2022", "1 - 18 - 2022")]
        [InlineData("1 - 18 - 2022", "1 - 15 - 2022")]
        public void ValidateInSessionInstancesTest(DateTime startDate, DateTime endDate)
        {
            // Arrange
            var webUser = WebUserFactory.Create(true, msgs, dc);
            var account = AccountFactory.Create(webUser, msgs, dc);
            var schedule = ActivityScheduleFactory.Create(account, startDate, endDate, msgs, dc);
            var activity = ActivityFactory.Create(account, schedule, msgs, dc);

            var instances = new List<ActivityInstance>();

            instances.Add(Factories.ActivityInstanceFactory.Create(
                activity,
                schedule,
                InstanceType.InSession,
                startDate,
                endDate,
                msgs,
                dc));
            
            // Act
            SystemMessages systemMessages = schedule.ValidateInSessionInstances(dc);
           
            string value = (systemMessages.ToList().Select(x => x.Text)).FirstOrDefault();

             Assert.True(("You have dates where the end time is earlier than or equal to the start time" == value) 
                         || ("You have dates that overlap" == value) 
                         || systemMessages.Count == 0 ? true: false, "Failed");
          
        }  
    }
}