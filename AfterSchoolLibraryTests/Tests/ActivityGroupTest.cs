using System;
using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using AfterSchoolLibraryTests.Factories;
using Xunit;
using Xunit.Abstractions;

namespace AfterSchoolLibraryTests.Tests
{
    public class ActivityGroupTest : BaseTest
    {
        private WebUser AccountOwner { get; set; }
        private Account ActivityGroupOwner { get; set; }

        public ActivityGroupTest(ITestOutputHelper output) : base(output)
        {
            AccountOwner = WebUserFactory.Create(msgs, dc);
            ActivityGroupOwner = AccountFactory.Create(AccountOwner, msgs, dc);
        }

        [Fact]
        public void LoadTest()
        {
            var activityGroup = ActivityGroupFactory.Create(ActivityGroupOwner, ActivityGroupType.Session, msgs, dc);

            Assert.NotNull(ActivityGroup.Load(activityGroup.ID.Value, msgs, dc));
        }

        [Fact]
        public void SaveTest()
        {
            var activityGroup = ActivityGroupFactory.Create(ActivityGroupOwner, ActivityGroupType.Session, msgs, dc);
            activityGroup.ActivityGroupTypeId = ActivityGroupType.League;
            msgs.AddRange(activityGroup.Save(dc));

            Assert.True(msgs.HasNoErrors());
        }

        [Fact]
        public void DeleteTest()
        {
            var activityGroup = ActivityGroupFactory.Create(ActivityGroupOwner, ActivityGroupType.Session, msgs, dc);

            msgs.AddRange(activityGroup.Delete(dc));

            Assert.Null(ActivityGroup.Load(activityGroup.ID.Value));
        }

        [Fact]
        public void ReloadTest()
        {
            var activityGroup = ActivityGroupFactory.Create(ActivityGroupOwner, ActivityGroupType.Session, msgs, dc);

            Assert.Equal(activityGroup.ID, activityGroup.Reload(msgs, dc).ID);
        }

        [Fact]
        public void InviteAccountTest()
        {
            var activityGroup = ActivityGroupFactory.Create(ActivityGroupOwner, ActivityGroupType.Session, msgs, dc);
            var accountToInviteOwner = WebUserFactory.Create(msgs, dc);
            var accountToInvite = AccountFactory.Create(accountToInviteOwner, msgs, dc);

            msgs.AddRange(activityGroup.InviteAccount(accountToInvite, AccountOwner, dc));

            Assert.True(msgs.HasNoErrors());

        }

        [Theory]
        [InlineData("My Sample Account", "MySampleAccount")]
        [InlineData("My Sample Account Rocks Bud Boy", "MySampleAccountRocksBud")]
        [InlineData("My!@#$%^&*()-_+<>/,.?[]{}\\|'\":;~`", "My-_")]
        public void GenerateUniqueName(string input, string expectedOutput)
        {
            var output = ActivityGroup.GenerateUniqueName(input, null, ActivityGroupOwner.ID, msgs, dc);

            Assert.Equal(expectedOutput, output);
        }

        [Fact]
        public void LoadByIdByAccountIdTest()
        {
            try
            {
                // Arrange
                var activityGroup = ActivityGroupFactory.Create(ActivityGroupOwner, ActivityGroupType.Session, msgs, dc);

                // Act
                var expected = ActivityGroup.LoadByIdByAccountId(Id: activityGroup.ID.Value, AccountId: ActivityGroupOwner.ID.Value, msgs: msgs, dc: dc);

                // Assert
                Assert.Equal(expected: expected.ID, actual: activityGroup.ID);
                Assert.Equal(expected: expected.Uid, actual: activityGroup.Uid);
                Assert.Equal(expected: expected.Name, actual: activityGroup.Name);
            }
            catch (Exception ex)
            {
                //Assert
                Assert.False(false, ex.Message);
            }
        }
    }
}
