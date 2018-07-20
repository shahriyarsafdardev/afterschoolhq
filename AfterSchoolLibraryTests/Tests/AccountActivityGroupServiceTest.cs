using System;
using Xunit;
using Xunit.Abstractions;
using AfterSchoolLibrary;
using AfterSchoolLibraryTests.Factories;
using AfterSchoolLibrary.Services;

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
            ActivityGroupOwner = AccountFactory.Create(AccountOwner, msgs, dc);
            ActivityGroup = ActivityGroupFactory.Create(ActivityGroupOwner, ActivityGroupType.Session, msgs, dc);
        }

        [Fact]
        public void CreateNewTest()
        {
            var aag = AccountActivityGroupService.CreateNew(ActivityGroupOwner, ActivityGroup, InviteStatus.Invited, LoggedInUser, msgs, dc);

            Assert.NotNull(aag);
        }

        [Fact]
        public void GetActivityGroupTest()
        {
            var aag = AccountActivityGroupFactory.Create(ActivityGroupOwner, ActivityGroup, msgs, dc);

            Assert.NotNull(aag.GetActivityGroup(msgs, dc));
        }

        [Fact]
        public void ToParticipationObjectTestSimple()
        {
            var aag = AccountActivityGroupFactory.Create(ActivityGroupOwner, ActivityGroup, msgs, dc);

            Assert.NotNull(aag.ToParticipationObject(msgs, dc));
        }

        [Fact]
        public void ToParticipationObjectTestFull()
        {
            var schedule = ActivityScheduleFactory.Create(ActivityGroupOwner, msgs, dc);
            var aag = AccountActivityGroupFactory.Create(ActivityGroupOwner, ActivityGroup, 35, InviteStatus.Invited, false, msgs, dc);
            var activityInstance = ActivityInstanceFactory.Create(ActivityGroup, schedule, msgs, dc);

            Assert.NotNull(aag.ToParticipationObject(msgs, dc));
        }
    }
}
