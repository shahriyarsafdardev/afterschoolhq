using BasicLibrary;
using BasicLibrary.DataAccess;
using AfterSchoolLibrary.Models;

namespace AfterSchoolLibraryTests.Factories
{
    public class RefundRequestFactory
    {
        public static RefundRequest Create(decimal amount, Transaction transaction, Account billingAccount, string reason, WebUser creator, SystemMessages msgs, DatabaseConnection dc)
        {
            var obj = new RefundRequest()
            {
                Amount = amount,
                TransactionId = transaction.ID.Value,
                AccountId = billingAccount.ID.Value,
                RequestReason = reason,
                CreatedBy = creator.ID.Value,
                RefundRequestStatusId = RefundRequestStatus.RequestedByProvider,
                RequestedRefundTypeId = RefundType.Credit,
                ApprovedRefundTypeId = RefundType.Credit
            };
            msgs.AddRange(obj.Save(dc: dc));
            return obj;
        }
    }
}
