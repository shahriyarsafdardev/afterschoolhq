using AfterSchool.Areas.Org.Controllers;
using AfterSchoolLibrary.Models;
using BasicLibrary;
using NUnit.Framework;

namespace after_school_hq.Tests
{
    public class EditConfirmationAssemblerTests
    {
        [Test]
        public void EditConfirmationAssemblerTest()
        {
            var editConfirmationAssembler = new EditConfirmationAssembler();
            var account = new Account();
            var activity = new Activity();
            var systemMessages = new SystemMessages();
            var result = editConfirmationAssembler.AssembleEditConfirmationResult(account, activity, UserType.NotSet, true, "1", "http://fullscreen.local",
                systemMessages);
            
            Assert.That(result.Account, Is.SameAs(account));
            Assert.That(result.Activity, Is.SameAs(activity));
            Assert.That(result.Messages, Is.SameAs(systemMessages));
            Assert.That(result.IsActivityLeader, Is.True);
            Assert.That(result.IsFullScreen, Is.True);
            Assert.That(result.FullScreenExitUrl, Is.EqualTo("http://fullscreen.local"));
        }
    }
}