using System.Collections.Generic;
using AfterSchool.Models;
using AfterSchoolLibrary.Models;
using BasicLibrary;

namespace AfterSchool.Assemblers
{
    public class AddEmergencyContactResult
    {
        public List<FormQuestionViewModel> Questions { get; set; }
        public ParticipationObject ParticipationObject { get; set; }
        public Account RegistrationStartPageAccount { get; set; }
        public SystemMessages Messages { get; set; }
    }

    public class AddEmergencyContactAssembler
    {
        public AddEmergencyContactResult AssembleAddEmergencyContactResult(List<FormQuestionViewModel> questions, ParticipationObject participationObject,
            Account registrationStartPageAccount, SystemMessages messages)
        {
            return new AddEmergencyContactResult
            {
                Questions = questions,
                ParticipationObject = participationObject,
                RegistrationStartPageAccount = registrationStartPageAccount,
                Messages = messages,
            };
        }
    }
}