using Xunit;
using Xunit.Abstractions;
using AfterSchoolLibrary.Models;
using AfterSchoolLibraryTests.Factories;
using AfterSchoolLibrary.Services;
using System;

namespace AfterSchoolLibraryTests.Tests
{
    public class AccountActivityGroupServiceTest : BaseTest
    {
        private WebUser AccountOwner { get; set; }
        private WebUser LoggedInUser { get; set; }
        private Account ActivityGroupOwner { get; set; }
        private WebUser InvitedAccountOwner { get; set; }
        private Account InvitedAccount { get; set; }
        private ActivityGroup ActivityGroup { get; set; }

        public AccountActivityGroupServiceTest(ITestOutputHelper output) : base(output)
        {
            AccountOwner = WebUserFactory.Create(msgs, dc);
            LoggedInUser = WebUserFactory.Create(msgs, dc);
            ActivityGroupOwner = AccountFactory.CreateWithFreePackage(AccountOwner, AccountType.SchoolDistrict, msgs, dc);
            ActivityGroup = ActivityGroupFactory.Create(ActivityGroupOwner, ActivityGroupType.Session, msgs, dc);
        }

        [Fact(DisplayName = "CreateNewTest")]
        public void CreateNewTest()
        {
            var aag = AccountActivityGroupService.CreateNew(ActivityGroupOwner, ActivityGroup, InviteStatus.Invited, LoggedInUser, msgs, dc);

            Assert.NotNull(aag);
        }

        [Fact(DisplayName = "GetActivityGroupTest")]
        public void GetActivityGroupTest()
        {
            var aag = AccountActivityGroupFactory.Create(ActivityGroupOwner, ActivityGroup, msgs, dc);

            Assert.NotNull(aag.GetActivityGroup(msgs, dc));
        }

        [Fact(DisplayName = "GetParticipationObjectTestSimple")]
        public void GetParticipationObjectTestSimple()
        {
            var aag = AccountActivityGroupFactory.Create(ActivityGroupOwner, ActivityGroup, msgs, dc);

            Assert.NotNull(aag.GetParticipationObject(msgs, dc));
        }

        [Fact(DisplayName = "GetParticipationObjectTestFull")]
        public void GetParticipationObjectTestFull()
        {
            var schedule = ActivityScheduleFactory.Create(ActivityGroupOwner, msgs, dc);
            var aag = AccountActivityGroupFactory.Create(ActivityGroupOwner, ActivityGroup, 35, InviteStatus.Invited, false, msgs, dc);
            var activityInstance = ActivityInstanceFactory.Create(ActivityGroup, schedule, msgs, dc);

            Assert.NotNull(aag.GetParticipationObject(msgs, dc));
        }
    }
}
