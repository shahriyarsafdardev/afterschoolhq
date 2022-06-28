using AfterSchool.Areas.Org.Assemblers.Activity;
using AfterSchoolLibrary.Models;
using BasicLibrary;
using NUnit.Framework;

namespace after_school_hq.Tests.Assemblers.Activity
{
    public class EditLocationAssemblerTests
    {
        [Test]
        public void EditLocationAssemblerTest()
        {
            var assembler = new EditLocationAssembler();
            var account = new Account();
            var activity = new AfterSchoolLibrary.Models.Activity();
            var messages = new SystemMessages();
            const string fullScreenExitUrl = "";
            var result = assembler.AssembleEditLocationResult(account, activity, messages, "1", UserType.NotSet, true, fullScreenExitUrl,
                null);

            Assert.That(result.Account, Is.SameAs(account));
            Assert.That(result.Activity, Is.SameAs(activity));
            Assert.That(result.Messages, Is.SameAs(messages));
            Assert.That(result.IsActivityLeader, Is.True);
            Assert.That(result.IsFullScreen, Is.True);
            Assert.That(result.FullScreenExitUrl, Is.EqualTo(fullScreenExitUrl));
            Assert.That(result.Location.LocationTypeId, Is.EqualTo(LocationType.NotSet));
        }
    }
}