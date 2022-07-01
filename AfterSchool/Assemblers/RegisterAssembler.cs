using System.Collections.Generic;
using AfterSchool.Models;
using AfterSchoolLibrary.Models;
using BasicLibrary;

namespace AfterSchool.Assemblers
{
    public class RegisterResult
    {
        public List<FormQuestionViewModel> Questions { get; set; }
        public ParticipationObject ParticipationObject { get; set; }
        public Account RegistrationStartPageAccount { get; set; }
        public Registrant Registrant { get; set; }
        public List<ParticipationObject> SessionActivities { get; set; }
        public SystemMessages Messages { get; set; }
    }

    public class RegisterAssembler
    {
        public RegisterResult AssembleRegisterResult(List<FormQuestionViewModel> questions, ParticipationObject participationObject,
            Account registrationStartPageAccount, Registrant registrant, List<ParticipationObject> sessionActivities, SystemMessages messages)
        {
            return new RegisterResult
            {
                Questions = questions,
                ParticipationObject = participationObject,
                RegistrationStartPageAccount = registrationStartPageAccount,
                Registrant = registrant,
                SessionActivities = sessionActivities,
                Messages = messages,
            };
        }
    }
}