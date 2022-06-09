using AfterSchool.Areas.Org.Controllers;
using AfterSchoolLibrary.Models;
using BasicLibrary;
using NUnit.Framework;

namespace after_school_hq.Tests
{
    public class EditRestrictionsAssemblerTests
    {
        [Test]
        public void EditRestrictionsAssemblerTest()
        {
            var assembler = new EditRestrictionsAssembler();
            var activity = new Activity
            {
                AccountId = 1,
            };
            var msgs = new SystemMessages();
            var currentAccount = new Account
            {
                ID = 1,
            };
            var fullScreenExitUrl = string.Empty;

            var result = assembler.AssembleEditRestrictionsResult(activity, msgs, currentAccount, true, fullScreenExitUrl);

            Assert.That(result.ActiveMenu, Is.EqualTo("EditRestrictions"));
            Assert.That(result.IsFullScreen, Is.True);
            Assert.That(result.Messages, Is.SameAs(msgs));
        }
    }
}