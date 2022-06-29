using AfterSchool.Areas.Org.Assemblers.Activity;
using AfterSchoolLibrary.Models;
using BasicLibrary;
using NUnit.Framework;

namespace after_school_hq.Tests.Assemblers.Activity
{
    public class SummaryAssemblerTests
    {
        [Test]
        public void SummaryAssemblerTest()
        {
            var assembler = new SummaryAssembler();
            var activity = new AfterSchoolLibrary.Models.Activity();
            var participationObject = new ParticipationObject(ActivityType.NotSet);
            var schedule = new ActivitySchedule();
            var totalPaid = 0m;
            var accountUserType = UserType.NotSet;
            var currentAccount =  new Account();
            var accountPackage = new AccountPackage();
            var currentUser =  new WebUser();
            var messages = new SystemMessages();
            var result = assembler.AssembleSummaryResult(activity, participationObject, schedule, totalPaid, false, accountUserType, currentAccount, 
                accountPackage, currentUser, messages);
            
            Assert.That(result.Activity, Is.EqualTo(activity));
            Assert.That(result.ParticipationObject, Is.EqualTo(participationObject));
            Assert.That(result.Schedule, Is.EqualTo(schedule));
            Assert.That(result.TotalPaid, Is.EqualTo(totalPaid));
            Assert.That(result.IsActivityLeader, Is.False);
            Assert.That(result.AccountUserType, Is.EqualTo(accountUserType));
            Assert.That(result.CurrentAccount, Is.EqualTo(currentAccount));
            Assert.That(result.CurrentUser, Is.EqualTo(currentUser));
            Assert.That(result.Messages, Is.EqualTo(messages));
        }
    }
}