using System.Collections.Generic;
using AfterSchool.Models;
using AfterSchoolLibrary.FeatureFlag;
using AfterSchoolLibrary.Models;
using BasicLibrary;

namespace AfterSchool.Assemblers
{
    public class VolunteerRegisterResult
    {
        public List<FormQuestionViewModel> Questions { get; set; }
        public ParticipationObject ParticipationObject { get; set; }
        public Account RegistrationStartPageAccount { get; set; }
        public Registrant Registrant { get; set; }
        public SystemMessages Messages { get; set; }
        public bool RequiresBackgroundCheck { get; set; }
        public bool UserIsApproved { get; set; }
    }

    public class VolunteerRegisterAssembler
    {
        private readonly IFeatureFlagRepository featureFlagRepository;

        public VolunteerRegisterAssembler(IFeatureFlagRepository featureFlagRepository)
        {
            this.featureFlagRepository = featureFlagRepository;
        }

        public VolunteerRegisterResult AssembleRegisterResult(List<FormQuestionViewModel> questions, ParticipationObject participationObject,
            Account registrationStartPageAccount, Registrant registrant, SystemMessages messages)
        {
            return new VolunteerRegisterResult
            {
                Questions = questions,
                ParticipationObject = participationObject,
                RegistrationStartPageAccount = registrationStartPageAccount,
                Registrant = registrant,
                Messages = messages,
                RequiresBackgroundCheck = featureFlagRepository.IsEnabled("background-check"),
                UserIsApproved = false,
            };
        }
    }
}