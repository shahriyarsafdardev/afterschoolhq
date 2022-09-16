using System.Collections.Generic;
using AfterSchool.Assemblers;
using AfterSchool.Models;
using AfterSchoolLibrary.Models;
using BasicLibrary;
using NUnit.Framework;

namespace after_school_hq.Tests.Assemblers
{
    public class VolunteerRegisterAssemblerTests
    {
        [Test]
        public void VolunteerRegisterAssemblerTest()
        {
            var assembler = new VolunteerRegisterAssembler();
            var formQuestionViewModels = new List<FormQuestionViewModel>();
            var participationObject = new ParticipationObject(ActivityType.Activity);
            var registrationStartPageAccount = new Account();
            var registrant = new Registrant();
            var systemMessages = new SystemMessages();
            var result = assembler.AssembleRegisterResult(formQuestionViewModels, participationObject, registrationStartPageAccount, registrant,
                systemMessages);

            Assert.That(result.Questions, Is.SameAs(formQuestionViewModels));
            Assert.That(result.ParticipationObject, Is.SameAs(participationObject));
            Assert.That(result.RegistrationStartPageAccount, Is.SameAs(registrationStartPageAccount));
            Assert.That(result.Registrant, Is.SameAs(registrant));
            Assert.That(result.Messages, Is.SameAs(systemMessages));
        }
    }
}