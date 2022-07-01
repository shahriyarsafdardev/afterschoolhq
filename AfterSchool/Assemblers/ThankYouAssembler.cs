using AfterSchoolLibrary.Models;
using BasicLibrary;

namespace AfterSchool.Assemblers
{
    public class ThankYouResult
    {
        public ParticipationObject ParticipationObject { get; set; }
        public Account Account { get; set; }
        public WebUser CurrentUser { get; set; }
        public Registrant Registrant { get; set; }
        public bool IsSelfRegistered { get; set; }
        public RegistrationAssignmentType AssignmentType { get; set; }
        public SystemMessages Messages { get; set; }
    }

    public class ThankYouResultAssembler
    {
        public ThankYouResult AssembleThankYouResult(ParticipationObject participationObject, Account account, WebUser currentUser, Registrant registrant,
            bool isSelfRegistered, RegistrationAssignmentType assignmentType, SystemMessages messages)
        {
            return new ThankYouResult
            {
                ParticipationObject = participationObject,
                Account = account,
                CurrentUser = currentUser,
                Registrant = registrant,
                IsSelfRegistered = isSelfRegistered,
                AssignmentType = assignmentType,
                Messages = messages,
            };
        }
    }
}