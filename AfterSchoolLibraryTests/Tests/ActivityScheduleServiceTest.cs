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

        [Fact(DisplayName = "IsClosingInTwoDaysTest")]
        public void IsClosingInTwoDays()
        {
            
            var schedule = ActivityScheduleFactory.CreateIsClosing(ActivityOwnerAccount, msgs, dc);

            var IsClosingInTwoDays = ActivityScheduleService.IsClosingInTwoDays(schedule, ActivityOwnerAccount);

            Assert.True(IsClosingInTwoDays);
        }



    }
}