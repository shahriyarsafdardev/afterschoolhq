using System.Collections.Generic;
using AfterSchool.Assemblers;
using AfterSchool.Models;
using AfterSchoolLibrary.Models;
using BasicLibrary;
using NUnit.Framework;

namespace after_school_hq.Tests.Assemblers
{
    public class AddEmergencyContactAssemblerTests
    {
        [Test]
        public void AddEmergencyContactAssemblerTest()
        {
            var addEmergencyContactAssembler = new AddEmergencyContactAssembler();
            var formQuestionViewModels = new List<FormQuestionViewModel>();
            var participationObject = new ParticipationObject(ActivityType.Activity);
            var registrationStartPageAccount = new Account();
            var systemMessages = new SystemMessages();
            var result = addEmergencyContactAssembler.AssembleAddEmergencyContactResult(formQuestionViewModels, participationObject, registrationStartPageAccount, systemMessages);

            Assert.That(result.Questions, Is.SameAs(formQuestionViewModels));
            Assert.That(result.ParticipationObject, Is.SameAs(participationObject));
            Assert.That(result.RegistrationStartPageAccount, Is.SameAs(registrationStartPageAccount));
            Assert.That(result.Messages, Is.SameAs(systemMessages));
        }
    }
}