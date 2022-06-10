using System.Collections.Generic;
using AfterSchool.Areas.Org.Controllers;
using AfterSchoolLibrary.Models;
using BasicLibrary;
using NUnit.Framework;

namespace after_school_hq.Tests
{
    public class EditContactAssemblerTests
    {
        [Test]
        public void EditContactAssemblerTest()
        {
            var assembler = new EditContactAssembler();
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
            var instructors = new List<WebUser>();
            var accountUserType = UserType.NotSet;
            var result = assembler.AssembleEditContactResult(currentAccount, msgs, activity, "1", fullScreenExitUrl, instructors, accountUserType, true);

            Assert.That(result.ActiveMenu, Is.EqualTo("EditContact"));
            Assert.That(result.IsFullScreen, Is.True);
            Assert.That(result.Messages, Is.SameAs(msgs));
            Assert.That(result.Activity, Is.SameAs(activity));
        }
    }
}