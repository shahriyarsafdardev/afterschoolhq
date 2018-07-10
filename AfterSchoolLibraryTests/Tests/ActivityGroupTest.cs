using System;
using Xunit;
using Xunit.Abstractions;
using AfterSchoolLibrary;

namespace AfterSchoolLibraryTests.Tests
{
    public class ActivityGroupTest : BaseTest
    {
        private WebUser AccountOwner { get; set; }
        private Account ActivityGroupOwner { get; set; }

        public ActivityGroupTest(ITestOutputHelper output) : base(output)
        {
            AccountOwner = Factories.WebUserFactory.Create(msgs, dc);
            ActivityGroupOwner = Factories.AccountFactory.Create(AccountOwner, msgs, dc);
        }

        [Fact]
        public void LoadTest()
        {
            var activityGroup = Factories.ActivityGroupFactory.Create(ActivityGroupOwner, msgs, dc);

            Assert.NotNull(ActivityGroup.Load(activityGroup.ID.Value, msgs, dc));
        }

        [Theory]
        [InlineData(8, 10)]
        [InlineData(15, 10)]
        public void LoadPagingTest(int numberToCreate, int numberPerPage)
        {
            for (var i = 0; i < numberToCreate; i++)
            {
                var activityGroup = Factories.ActivityGroupFactory.Create(ActivityGroupOwner, msgs, dc);
            }

            Assert.Equal(
                Math.Min(numberPerPage, numberToCreate),
                ActivityGroup.Load(null, 0, numberPerPage, msgs, dc).Count
            );
        }

        [Theory]
        [InlineData(8)]
        [InlineData(2)]
        public void GetCountTest(int numberToCreate)
        {
            for (var i = 0; i < numberToCreate; i++)
            {
                var activityGroup = Factories.ActivityGroupFactory.Create(ActivityGroupOwner, msgs, dc);
            }

            Assert.Equal(numberToCreate, ActivityGroup.GetCount(msgs: msgs, dc: dc));
        }

        [Fact]
        public void SaveTest()
        {
            var activityGroup = Factories.ActivityGroupFactory.Create(ActivityGroupOwner, msgs, dc);
            activityGroup.ActivityGroupTypeId = ActivityGroupType.League;
            msgs.AddRange(activityGroup.Save(dc));

            Assert.True(msgs.HasNoErrors());
        }

        [Fact]
        public void DeleteTest()
        {
            var activityGroup = Factories.ActivityGroupFactory.Create(ActivityGroupOwner, msgs, dc);

            msgs.AddRange(activityGroup.Delete(dc));

            Assert.Equal(0, ActivityGroup.GetCount(msgs: msgs, dc: dc));
        }

        [Fact]
        public void ReloadTest()
        {
            var activityGroup = Factories.ActivityGroupFactory.Create(ActivityGroupOwner, msgs, dc);

            Assert.Equal(activityGroup.ID, activityGroup.Reload(msgs, dc).ID);
        }

        [Fact]
        public void InviteAccountTest()
        {
            var activityGroup = Factories.ActivityGroupFactory.Create(ActivityGroupOwner, msgs, dc);
            var accountToInviteOwner = Factories.WebUserFactory.Create(msgs, dc);
            var accountToInvite = Factories.AccountFactory.Create(accountToInviteOwner, msgs, dc);

            msgs.AddRange(activityGroup.InviteAccount(accountToInvite, AccountOwner, dc));

            Assert.True(msgs.HasNoErrors());

        }
    }
}
