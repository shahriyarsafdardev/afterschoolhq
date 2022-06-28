using AfterSchool.Areas.Org.Controllers;
using AfterSchoolLibrary.Models;
using BasicLibrary;

namespace AfterSchool.Areas.Org.Assemblers.Activity
{
    public class EditCostResult
    {
        public AfterSchoolLibrary.Models.Activity Activity { get; set; }
        public SystemMessages Messages { get; set; }
        public bool IsFullScreen { get; set; }
        public bool IsActivityLeader { get; set; }
        public string FullScreenExitUrl { get; set; }
        public SubscriptionPlan SubscriptionPlan { get; set; }
        public SubscriptionPlan PaymentPlan { get; set; }
        public SubscriptionPlan PostPayPlan { get; set; }
        public AccountPackage AccountPackage { get; set; }
    }

    public class EditCostAssembler
    {
        public EditCostResult AssembleEditCostResult(AfterSchoolLibrary.Models.Activity activity, int? accountId, UserType accountUserType, bool currentUserIsActivityLeader,
            string isNew,
            string fullScreenExitUrl, SystemMessages msgs, SubscriptionPlan subscriptionPlan, SubscriptionPlan paymentPlan, SubscriptionPlan postPayPlan,
            AccountPackage accountPackage)
        {
            var isActivityLeader = EditHelpers.IsActivityLeader(accountId, activity.AccountId, accountUserType, currentUserIsActivityLeader);
            var isFullScreen = EditHelpers.GetIsFullScreen(activity, isNew == "1");
            return new EditCostResult
            {
                Activity = activity,
                Messages = msgs,
                IsFullScreen = isFullScreen,
                IsActivityLeader = isActivityLeader,
                FullScreenExitUrl = fullScreenExitUrl,
                SubscriptionPlan = subscriptionPlan,
                PaymentPlan = paymentPlan,
                PostPayPlan = postPayPlan,
                AccountPackage = accountPackage,
            };
        }
    }
}