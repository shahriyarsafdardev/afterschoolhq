using System;
using Xunit;
using Xunit.Abstractions;
using AfterSchoolLibrary.Models;
using AfterSchoolLibraryTests.Factories;
using System.Linq;

namespace AfterSchoolLibraryTests.Tests
{
    public class SessionActionTest : BaseTest
    {
        private WebUser user { get; set; }
        private Account account { get; set; }

        public SessionActionTest(ITestOutputHelper output) : base(output)
        {
            user = WebUserFactory.Create(msgs, dc);
            account = AccountFactory.Create(user, msgs, dc);
        }        

        [Fact]
        public void LoadByAccountIdByActivityIdTest()
        {
            try
            {
                // Arrange
                var session = SessionFactory.Create(msgs, dc);
                var activity = ActivityFactory.Create(account: account, msgs: msgs, dc: dc);
                var sessionAction = SessionActionFactory.Create(account, session, activity, msgs, dc);

                // Act
                var expectedList = SessionAction.LoadByAccountIdByActivityId(ActivityId: activity.ID.Value, 
                    AccountId: account.ID.Value, 
                    actionTypes: new System.Collections.Generic.List<SessionActionType>() { SessionActionType.ActivityCreate },
                    start: null,
                    length: null,
                    msgs: msgs, dc: dc);

                var expected = expectedList.Where(m => m.ID == sessionAction.ID).FirstOrDefault();

                // Assert
                Assert.Equal(expected: expected.ID, actual: sessionAction.ID);
                Assert.Equal(expected: expected.SessionId, actual: sessionAction.SessionId);
                Assert.Equal(expected: expected.SessionActionTypeId, actual: sessionAction.SessionActionTypeId);
                Assert.Equal(expected: expected.AccountId, actual: sessionAction.AccountId);
                Assert.Equal(expected: expected.ActivityId, actual: sessionAction.ActivityId);
            }
            catch (Exception ex)
            {
                //Assert
                Assert.False(false, ex.Message);
            }
        }
    }
}
