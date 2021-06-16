using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using AfterSchoolLibraryTests.Factories;
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

        [Fact(DisplayName = "IsEndingInTwoDays")]
        public void IsEndingInTwoDaysTest()
        {
            
            var schedule = ActivityScheduleFactory.CreateIsClosing(ActivityOwnerAccount, msgs, dc);

            var IsClosingInTwoDays = ActivityScheduleService.IsEndingInTwoDays(schedule, ActivityOwnerAccount);

            Assert.True(IsClosingInTwoDays);
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
    }
}