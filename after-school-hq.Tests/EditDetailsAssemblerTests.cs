using AfterSchool.Areas.Org.Controllers;
using AfterSchoolLibrary.Models;
using BasicLibrary;
using NUnit.Framework;

namespace after_school_hq.Tests
{
    public class EditDetailsAssemblerTests
    {
        [Test]
        public void EditDetailsAssemblerTest()
        {
            var assembler = new EditDetailsAssembler();
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
            var accountPackage = new AccountPackage();
            var result = assembler.AssembleEditDetailsResult(currentAccount, msgs, activity, fullScreenExitUrl, false, accountPackage, true );

            Assert.That(result.ActiveMenu, Is.EqualTo("EditDetails"));
            Assert.That(result.IsFullScreen, Is.True);
            Assert.That(result.Messages, Is.SameAs(msgs));
        }
    }
}