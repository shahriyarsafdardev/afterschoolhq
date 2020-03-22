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

        public static bool CanRefundAmountRequested(this RefundRequest request, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return request.GetTransaction(msgs, dc).IsAmountRefundable(request.Amount.Value, true, msgs, dc);
        }

        public static bool ApproveAndProcessRequest(this RefundRequest request, bool allowFullRefundAmount, bool refundCCFees, bool refundConvenienceFees, WebUser loggedInuser, SystemMessages msgs, DatabaseConnection dc = null)
        {
            var transaction = request.GetTransaction(msgs, dc);

            if (transaction.IsNotEmpty() && !transaction.IsDeleted)
            {
                Database.ExecuteTransaction
                (
                    action: (SystemMessages amsgs, DatabaseConnection adc) =>
                    {
                        var refundAmount = request.Amount.Value;

                        if (allowFullRefundAmount && refundAmount <= transaction.GetBillerAmount(amsgs, adc))
                        {
                            if (refundCCFees)
                                refundAmount += transaction.PaymentTypeFeeCharged.Value;

                            if (refundConvenienceFees)
                                refundAmount += transaction.ConvenienceFeeCharged.Value;
                        }

                        transaction.Refund(refundAmount, allowFullRefundAmount, transaction.PaymentSourceTypeId, loggedInuser, amsgs, adc);

                        if (msgs.HasNoErrors())
                        {
                            request.RequestFulfilled = true;
                            amsgs.AddRange(request.Save(adc));
                        }
                    },
                    onSuccess: (SystemMessages smsgs) => msgs.AddRange(smsgs),
                    onFailure: (SystemMessages fmsgs) => msgs.AddRange(fmsgs),
                    onException: (SystemMessages emsgs) => msgs.AddRange(emsgs)
                );
            }
            else
                msgs.Add("Invalid transaction", MessageType.UserError);

            return msgs.HasNoErrors();
        }

    }
}
