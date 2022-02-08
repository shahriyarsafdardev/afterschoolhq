using AfterSchoolLibrary.Models;
using System;
using System.Collections.Generic;
using AfterSchoolLibraryTests.Factories;
using System.Linq;
using Xunit.Abstractions;
using Xunit;

namespace AfterSchoolLibraryTests.Tests
{
    public class WebSessionActionTest : BaseTest
    {
        private WebUser user { get; set; }
        private Account account { get; set; }

        public WebSessionActionTest(ITestOutputHelper output) : base(output)
        {
            user = WebUserFactory.Create(msgs, dc);
            account = AccountFactory.Create(user, msgs, dc);
        }

        [Fact]
        public void LoadByAccountIdByActivityIdTest()
        {

            var webSession = WebSessionFactory.Create(account, user, msgs, dc);
            var activity = ActivityFactory.Create(account: account, "Activity_"+DateTime.Now.Ticks, msgs: msgs, dc: dc);
            var webSessionAction = WebSessionActionFactory.Create(account, activity, webSession, msgs, dc);

            // Act
            var expectedList = WebSessionAction.LoadByAccountIdActivityId(activity.ID.Value,
                account.ID.Value,
                start: null,
                length: null,
                msgs: msgs, dc: dc);

            var expected = expectedList.Where(m => m.ID == webSessionAction.ID).FirstOrDefault();

            // Assert
            Assert.Equal(expected: expected.ID, actual: webSessionAction.ID);

        }

        [Fact]
        public void LoadByActivityGroupIdTest()
        {

            var webSession = WebSessionFactory.Create(account, user, msgs, dc);
            var activityGroup = ActivityGroupFactory.Create(account, ActivityGroupType.Session, msgs, dc);
            var webSessionAction = WebSessionActionFactory.CreateWithActivityGroup(account, activityGroup, webSession, msgs, dc);

            // Act
            var expectedList = WebSessionAction.LoadByActivityGroupId(activityGroup.ID.Value,
                account.ID.Value,
                start: null,
                length: null,
                msgs: msgs, dc: dc);

            var expected = expectedList.Where(m => m.ID == webSessionAction.ID).FirstOrDefault();

            // Assert
            Assert.Equal(expected: expected.ID, actual: webSessionAction.ID);

        }

        [Fact]
        public void LoadByAccountIdTest()
        {

            var webSession = WebSessionFactory.Create(account, user, msgs, dc);
            var activityGroup = ActivityGroupFactory.Create(account, ActivityGroupType.Session, msgs, dc);
            var webSessionAction = WebSessionActionFactory.CreateWithActivityGroup(account, activityGroup, webSession, msgs, dc);

            // Act
            var expectedList = WebSessionAction.LoadByAccountId(account.ID.Value,
                start: null,
                length: null,
                msgs: msgs, dc: dc);


            // Assert
            Assert.True(expectedList.Count > 0);

        }
    }
}
