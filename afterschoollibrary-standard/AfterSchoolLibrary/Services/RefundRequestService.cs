using AfterSchoolLibrary.Models;
using BasicLibrary;
using BasicLibrary.DataAccess;
using System;
using System.Collections.Generic;
using System.Text;

namespace AfterSchoolLibrary.Services
{
    public static class RefundRequestService
    {
        public static RefundRequest CreateNewRefundRequest(Transaction transaction, decimal amount, WebUser loggedInUser, SystemMessages msgs, DatabaseConnection dc = null)
        {
            var refundRequest = new RefundRequest()
            {
                TransactionId = transaction.ID.Value,
                Amount = amount,
                CreateDate = DateTime.UtcNow,
                CreatedBy = loggedInUser.ID.Value,
                LastModifiedDate = DateTime.UtcNow,
                LastModifiedBy = loggedInUser.ID.Value,
                RequestFulfilled = false,
                AccountId = transaction.BillerAccountId,
            };

            msgs.AddRange(refundRequest.Save(dc));

            return refundRequest;
        }

        public static WebUser GetCreator(this RefundRequest request, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return WebUser.Load(request.CreatedBy.Value, msgs, dc);
        }

        public static Transaction GetTransaction(this RefundRequest request, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return Transaction.Load(request.TransactionId.Value, msgs, dc);
        }

    }
}
