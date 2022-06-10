using System;
using System.Collections.Generic;
using AfterSchool.Areas.Org.Controllers;
using AfterSchoolLibrary.Models;
using BasicLibrary;
using NUnit.Framework;

namespace after_school_hq.Tests
{
    public class EditSetupAssemblerTests
    {
        [Test]
        public void EditSetupAssemblerTest()
        {
            var assembler = new EditSetupAssembler();
            var activity = new Activity
            {
                AccountId = 1,
            };
            ActivityGroup activityGroup = null;
            var msgs = new SystemMessages();
            var currentAccount = new Account
            {
                ID = 1,
            };
            string activeMenu = null;
            var periods = new List<RegistrationPeriod>
            {
                new RegistrationPeriod
                {
                    StartDateTime = DateTime.UtcNow.AddDays(-1),
                    Name = "Yesterday",
                },
                new RegistrationPeriod
                {
                    StartDateTime = DateTime.UtcNow.AddDays(-2),
                    Name = "TwoDaysAgo",
                },
            };
            var activePeriod = new RegistrationPeriod
            {
                StartDateTime = DateTime.UtcNow.AddDays(0),
                Name = "Today",
            };
            ActivityForm activeForm = null;
            AccountPackage accountPackage = null;
            var accountUserType = UserType.NotSet;
            var currentUserIsActivityLeader = true;
            var forms = new List<Form>
            {
                new Form {Name = "Zebra"},
                new Form {Name = "Apple"},
                new Form {Name = "Green"},
            };
            var fullScreenExitUrl = string.Empty;

            var result = assembler.AssembleEditSetupResult(activity, activityGroup, msgs, currentAccount, activeMenu, periods, activePeriod, activeForm,
                accountPackage, accountUserType, currentUserIsActivityLeader, "1", forms, fullScreenExitUrl);

            Assert.That(result.Periods, Has.Count.EqualTo(3));
            Assert.That(result.Periods[0].Name, Is.EqualTo("TwoDaysAgo"));
            Assert.That(result.Periods[1].Name, Is.EqualTo("Yesterday"));
            Assert.That(result.Periods[2].Name, Is.EqualTo("Today"));
            Assert.That(result.Forms[0].Name, Is.EqualTo("Apple"));
            Assert.That(result.Forms[1].Name, Is.EqualTo("Green"));
            Assert.That(result.Forms[2].Name, Is.EqualTo("Zebra"));

            Assert.That(result.IsFullScreen, Is.True);
            Assert.That(result.IsActivityLeader, Is.True);

            Assert.That(result.Messages, Is.SameAs(msgs));
            Assert.That(result.Activity, Is.SameAs(activity));
        }
    }
}