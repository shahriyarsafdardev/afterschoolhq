using System.Collections.Generic;
using AfterSchool.Models;
using AfterSchoolLibrary.Models;
using BasicLibrary;

namespace AfterSchool.Controllers
{
    public class AddHomeAddressResult
    {
        public SystemMessages Messages { get; set; }
        public ParticipationObject ParticipationObject { get; set; }
        public Account RegistrationStartPageAccount { get; set; }
        public List<FormQuestionViewModel> Questions { get; set; }
    }

    public class AddHomeAddressAssembler
    {
        public AddHomeAddressResult AssembleAddHomeAddressResult(List<FormQuestionViewModel> formQuestionViewModels, ParticipationObject participationObject,
            Account registrationStartPageAccount, SystemMessages messages)
        {
            return new AddHomeAddressResult
            {
                Questions = formQuestionViewModels,
                ParticipationObject = participationObject,
                RegistrationStartPageAccount = registrationStartPageAccount,
                Messages = messages,
            };
        }
    }
}