using AfterSchoolLibrary.Models;
using BasicLibrary;

namespace AfterSchool.Areas.Org.Assemblers.Activity
{
    public class SummaryResult
    {
        public AfterSchoolLibrary.Models.Activity Activity { get; set; }
        public ParticipationObject ParticipationObject { get; set; }
        public ActivitySchedule Schedule { get; set; }
        public decimal TotalPaid { get; set; }
        public bool? IsActivityLeader { get; set; }
        public UserType AccountUserType { get; set; }
        public Account CurrentAccount { get; set; }
        public AccountPackage AccountPackage { get; set; }
        public WebUser CurrentUser { get; set; }
        public SystemMessages Messages { get; set; }
    }
    public class SummaryAssembler
    {
        public SummaryResult AssembleSummaryResult(AfterSchoolLibrary.Models.Activity activity, ParticipationObject participationObject,
            ActivitySchedule schedule, decimal totalPaid, bool? isActivityLeader, UserType accountUserType, Account currentAccount,
            AccountPackage accountPackage, WebUser currentUser, SystemMessages messages)
        {
            return new SummaryResult
            {
                Activity = activity,
                ParticipationObject = participationObject,
                Schedule = schedule,
                TotalPaid = totalPaid,
                IsActivityLeader = isActivityLeader,
                AccountUserType = accountUserType,
                CurrentAccount = currentAccount,
                AccountPackage = accountPackage,
                CurrentUser = currentUser,
                Messages = messages,
            };
        }
    }
}