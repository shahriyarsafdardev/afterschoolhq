using System.Collections.Generic;
using AfterSchool.Controllers;
using AfterSchool.Models;
using AfterSchoolLibrary.Models;
using BasicLibrary;
using NUnit.Framework;

namespace after_school_hq.Tests
{
    public class AddHomeAddressAssemblerTests
    {
        [Test]
        public void AddHomeAddressAssemblerTest()
        {
            var addHomeAddressAssembler = new AddHomeAddressAssembler();
            var formQuestionViewModels = new List<FormQuestionViewModel>();
            var participationObject = new ParticipationObject(ActivityType.Activity);
            var registrationStartPageAccount = new Account();
            var systemMessages = new SystemMessages();
            var result = addHomeAddressAssembler.AssembleAddHomeAddressResult(formQuestionViewModels, participationObject, registrationStartPageAccount,
                systemMessages);

            Assert.That(result.Questions, Is.SameAs(formQuestionViewModels));
            Assert.That(result.ParticipationObject, Is.SameAs(participationObject));
            Assert.That(result.RegistrationStartPageAccount, Is.SameAs(registrationStartPageAccount));
            Assert.That(result.Messages, Is.SameAs(systemMessages));
        }
    }
}