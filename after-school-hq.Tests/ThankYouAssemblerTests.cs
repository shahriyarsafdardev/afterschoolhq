using AfterSchool.Controllers;
using AfterSchoolLibrary.Models;
using BasicLibrary;
using NUnit.Framework;

namespace after_school_hq.Tests
{
    public class ThankYouAssemblerTests
    {
        [Test]
        public void ThankYouAssemblerTest()
        {
            var addEmergencyContactAssembler = new ThankYouResultAssembler();
            var participationObject = new ParticipationObject(ActivityType.Activity);
            var account = new Account();
            var currentUser = new WebUser();
            var registrant = new Registrant();
            const RegistrationAssignmentType assignmentType = RegistrationAssignmentType.NotSet;
            var systemMessages = new SystemMessages();
            var result = addEmergencyContactAssembler.AssembleThankYouResult(participationObject, account, currentUser, registrant, false, assignmentType,
                systemMessages);

            Assert.That(result.ParticipationObject, Is.SameAs(participationObject));
            Assert.That(result.Account, Is.SameAs(account));
            Assert.That(result.CurrentUser, Is.SameAs(currentUser));
            Assert.That(result.IsSelfRegistered, Is.EqualTo(false));
            Assert.That(result.AssignmentType, Is.EqualTo(assignmentType));
            Assert.That(result.Messages, Is.SameAs(systemMessages));
        }
    }
}