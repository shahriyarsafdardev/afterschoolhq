using System.Collections.Generic;
using AfterSchool.Controllers;
using AfterSchool.Models;
using AfterSchoolLibrary.Models;
using BasicLibrary;
using NUnit.Framework;

namespace after_school_hq.Tests
{
    public class RegisterAssemblerTests
    {
        [Test]
        public void RegisterAssemblerTest()
        {
            var registerAssembler = new RegisterAssembler();
            var formQuestionViewModels = new List<FormQuestionViewModel>();
            var participationObject = new ParticipationObject(ActivityType.Activity);
            var registrationStartPageAccount = new Account();
            var registrant = new Registrant();
            var sessionActivities = new List<ParticipationObject>();
            var systemMessages = new SystemMessages();
            var result = registerAssembler.AssembleRegisterResult(formQuestionViewModels, participationObject, registrationStartPageAccount, registrant,
                sessionActivities, systemMessages);

            Assert.That(result.Questions, Is.SameAs(formQuestionViewModels));
            Assert.That(result.ParticipationObject, Is.SameAs(participationObject));
            Assert.That(result.RegistrationStartPageAccount, Is.SameAs(registrationStartPageAccount));
            Assert.That(result.Registrant, Is.SameAs(registrant));
            Assert.That(result.SessionActivities, Is.SameAs(sessionActivities));
            Assert.That(result.Messages, Is.SameAs(systemMessages));
        }
    }
}