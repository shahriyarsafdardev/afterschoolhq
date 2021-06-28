using System;
using Xunit;
using Xunit.Abstractions;
using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using AfterSchoolLibraryTests.Factories;

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
            var activityGroup = Factories.ActivityGroupFactory.Create(ActivityGroupOwner, ActivityGroupType.Session, msgs, dc);

            Assert.NotNull(ActivityGroup.Load(activityGroup.ID.Value, msgs, dc));
        }

        [Theory]
        [InlineData(8, 10)]
        [InlineData(15, 10)]
        public void LoadPagingTest(int numberToCreate, int numberPerPage)
        {
            for (var i = 0; i < numberToCreate; i++)
            {
                var activityGroup = Factories.ActivityGroupFactory.Create(ActivityGroupOwner, ActivityGroupType.Session, msgs, dc);
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
                var activityGroup = Factories.ActivityGroupFactory.Create(ActivityGroupOwner, ActivityGroupType.Session, msgs, dc);
            }

            Assert.Equal(numberToCreate, ActivityGroup.GetCount(msgs: msgs, dc: dc));
        }

        [Fact(DisplayName = "SaveTest")]
        public void SaveTest()
        {
            var activityGroup = Factories.ActivityGroupFactory.Create(ActivityGroupOwner, ActivityGroupType.Session, msgs, dc);
            activityGroup.ActivityGroupTypeId = ActivityGroupType.League;
            msgs.AddRange(activityGroup.Save(dc));

            Assert.True(msgs.HasNoErrors());
        }

        [Fact(DisplayName = "DeleteTest")]
        public void DeleteTest()
        {
            var activityGroup = Factories.ActivityGroupFactory.Create(ActivityGroupOwner, ActivityGroupType.Session, msgs, dc);

            msgs.AddRange(activityGroup.Delete(dc));

            Assert.Equal(0, ActivityGroup.GetCount(msgs: msgs, dc: dc));
        }

        [Fact(DisplayName = "DisplayName")]
        public void ReloadTest()
        {
            var activityGroup = Factories.ActivityGroupFactory.Create(ActivityGroupOwner, ActivityGroupType.Session, msgs, dc);

            Assert.Equal(activityGroup.ID, activityGroup.Reload(msgs, dc).ID);
        }

        [Fact(DisplayName = "Invite account to join league or session")]
        public void InviteAccountTest()
        {
            var activityGroup = Factories.ActivityGroupFactory.Create(ActivityGroupOwner, ActivityGroupType.Session, msgs, dc);
            var accountToInviteOwner = Factories.WebUserFactory.Create(msgs, dc);
            var accountToInvite = Factories.AccountFactory.Create(accountToInviteOwner, msgs, dc);

            msgs.AddRange(activityGroup.InviteAccount(accountToInvite, AccountOwner, dc));

            Assert.True(msgs.HasNoErrors());

        }

        [Theory(DisplayName = "Generate Unique Name")]
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
