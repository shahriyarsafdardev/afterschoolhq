using AfterSchool.Areas.Org.Assemblers.Activity;
using AfterSchoolLibrary.Models;
using BasicLibrary;
using NUnit.Framework;

namespace after_school_hq.Tests.Assemblers.Activity
{
    public class EditDetailsAssemblerTests
    {
        [Test]
        public void EditDetailsAssemblerTest()
        {
            var assembler = new EditDetailsAssembler();
            var activity = new AfterSchoolLibrary.Models.Activity
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
            var result = assembler.AssembleEditDetailsResult(currentAccount, activity, fullScreenExitUrl, false, accountPackage, true, "Edit Details", msgs);

            Assert.That(result.Details, Is.EqualTo("Edit Details"));
            Assert.That(result.IsFullScreen, Is.True);
            Assert.That(result.Messages, Is.SameAs(msgs));
            Assert.That(result.Activity, Is.SameAs(activity));
        }
    }
}