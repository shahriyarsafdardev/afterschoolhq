using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using AfterSchoolLibrary;
using BasicLibrary;
using BasicLibrary.DataAccess;

namespace AfterSchoolLibraryTests
{
    public class ActivityGroupTest : BaseTest
    {
        public ActivityGroupTest(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public void GetSheduleTest()
        {
            var user = Factories.WebUserFactory.Create(msgs, dc);
            var account = Factories.AccountFactory.Create(user, msgs, dc);
            var schedule = Factories.ActivityScheduleFactory.Create(account, msgs, dc);
            var activity = Factories.ActivityFactory.Create(account, schedule, msgs, dc);
            var activityGroup = Factories.ActivityGroupFactory.Create(account, ActivityGroupType.Session, msgs, dc);
            activityGroup.ActivityScheduleId = schedule.ID.Value;
            var activityInstance = Factories.ActivityInstanceFactory.Create(activity, schedule, msgs, dc);

            Assert.NotNull(activityGroup.GetSchedule(false, msgs, dc));
        }
    }
}
