using BasicLibrary;
using BasicLibrary.DataAccess;
using AfterSchoolLibrary.Models;

namespace AfterSchoolLibraryTests.Factories
{
    public class ActivityPaymentFactory
    {
        public static ActivityPayment Create(Account account, Activity activity, Registrant registrant, Transaction transaction, SystemMessages msgs, DatabaseConnection dc)
        {
            var obj = new ActivityPayment()
            {
                AccountId = account.ID,
                ParticipationObjectId = activity.ID,
                ParticipationObjectTypeId = ParticipationObjectType.Activity,
                Amount = 100,
                RegistrantId = registrant.ID,
                TransactionId = transaction.ID
            };
            msgs.AddRange(obj.Save(dc: dc));
            return obj;
        }

        public static ActivityPayment Create(Account account, Activity activity, Registrant registrant, RegistrantSubscriptionPlan registrantSubscriptionPlan, decimal amount, decimal amountPaidByParent, bool isProRated, SystemMessages msgs, DatabaseConnection dc)
        {
            var obj = new ActivityPayment()
            {
                AccountId = account.ID,
                ParticipationObjectId = activity.ID,
                ParticipationObjectTypeId = ParticipationObjectType.Activity,
                Amount = amount,
                AmountPaidByParent = amountPaidByParent,
                IsProRated = isProRated,
                RegistrantSubscriptionPlanId = registrantSubscriptionPlan.ID,
                RegistrantId = registrant.ID,
                TransactionId = null,
            };
            msgs.AddRange(obj.Save(dc: dc));
            return obj;
        }

        public static ActivityPayment Create(Account account, Activity activity, RegistrantSubscriptionPlan registrantSubscriptionPlan, Transaction transaction, SystemMessages msgs, DatabaseConnection dc)
        {
            var obj = new ActivityPayment()
            {
                AccountId = account.ID,
                ParticipationObjectId = activity.ID,
                ParticipationObjectTypeId = ParticipationObjectType.Activity,
                Amount = 100,
                RegistrantSubscriptionPlanId = registrantSubscriptionPlan.ID,
                RegistrantId = registrantSubscriptionPlan.RegistrantId,
                TransactionId = transaction.ID
            };
            msgs.AddRange(obj.Save(dc: dc));
            return obj;
        }
    }
}
