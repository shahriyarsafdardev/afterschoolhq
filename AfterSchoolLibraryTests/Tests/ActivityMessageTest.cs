using System;
using Xunit;
using Xunit.Abstractions;
using AfterSchoolLibrary.Models;
using AfterSchoolLibraryTests.Factories;

namespace AfterSchoolLibraryTests.Tests
{
    public class ActivityMessageTest : BaseTest
    {
        private WebUser user { get; set; }
        private Account account { get; set; }

        public ActivityMessageTest(ITestOutputHelper output) : base(output)
        {
            user = WebUserFactory.Create(msgs, dc);
            account = AccountFactory.Create(user, msgs, dc);
        }        

        [Fact]
        public void LoadByUidByAccountIdTest()
        {
            try
            {
                // Arrange
                var activity = ActivityFactory.Create(account: account, msgs: msgs, dc: dc);
                var activityMessage = ActivityMessageFactory.Create(account, activity, msgs, dc);

                // Act
                var expected = ActivityMessage.LoadByUidByAccountId(Uid: activityMessage.Uid, AccountId: account.ID.Value, msgs: msgs, dc: dc);

                // Assert
                Assert.Equal(expected: expected.ID, actual: activityMessage.ID);
                Assert.Equal(expected: expected.Uid, actual: activityMessage.Uid);
                Assert.Equal(expected: expected.AccountId, actual: activityMessage.AccountId);
                Assert.Equal(expected: expected.ActivityId, actual: activityMessage.ActivityId);
            }
            catch (Exception ex)
            {
                //Assert
                Assert.False(false, ex.Message);
            }
        }
    }
}
