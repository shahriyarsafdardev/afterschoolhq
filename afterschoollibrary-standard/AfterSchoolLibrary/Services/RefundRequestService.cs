using AfterSchoolLibrary.Models;
using BasicLibrary;
using BasicLibrary.DataAccess;
using System;


namespace AfterSchoolLibrary.Services
{
    public static class RefundRequestService
    {
        public static RefundRequest CreateNew(Transaction transaction, decimal amount, RefundRequestStatus refundRequestStatus, string reason, WebUser loggedInUser, SystemMessages msgs, DatabaseConnection dc = null)
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
                RefundRequestStatusId = refundRequestStatus,
                RequestReason = reason,
                ApprovedRefundTypeId = RefundType.Refund,
                RequestedRefundTypeId = RefundType.Refund,
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

        public static Transaction ApproveAndProcessRequest(this RefundRequest request, bool allowFullRefundAmount, bool refundCCFees, bool refundConvenienceFees, PaymentSourceType paymentSourceType, WebUser loggedInUser, SystemMessages msgs, DatabaseConnection dc = null)
        {
            var transaction = request.GetTransaction(msgs, dc);
            Transaction refund = new Transaction();
            
            if (transaction.IsNotEmpty() && !transaction.IsDeleted)
                Database.ExecuteTransaction
                (
                    action: (SystemMessages amsgs, DatabaseConnection adc) =>
                    {
                        var refundAmount = request.Amount.Value;
                        var convenienceFee = new decimal?();
                        var paymentTypeFee = new decimal?();

                        if (allowFullRefundAmount && refundAmount <= transaction.GetBillerAmount(amsgs, adc))
                        {
                            if (refundCCFees)
                            {
                                paymentTypeFee = transaction.PaymentTypeFeeCharged;
                                refundAmount += transaction.GetPaymentTypeFeesChargedToPayer();
                            }

                            if (refundConvenienceFees)
                            {
                                convenienceFee = transaction.ConvenienceFeeCharged;
                                refundAmount += transaction.GetConvenienceFeesChargedToPayer();
                            }
                        }

                        refund = transaction.Refund(
                                        refundAmount, 
                                        allowFullRefundAmount, 
                                        convenienceFee,
                                        paymentTypeFee,
                                        paymentSourceType, 
                                        loggedInUser, 
                                        amsgs, 
                                        adc);

                        if (msgs.HasNoErrors())
                            request.MarkAsCompleted(refund, loggedInUser, amsgs, adc);

                    },
                    onSuccess: (SystemMessages smsgs) => msgs.AddRange(smsgs),
                    onFailure: (SystemMessages fmsgs) => msgs.AddRange(fmsgs),
                    onException: (SystemMessages emsgs) => msgs.AddRange(emsgs)
                );
            else
                msgs.Add("Invalid transaction", MessageType.UserError);

            return refund;
        }

        public static void DenyRequestAsProvider(this RefundRequest request, string reason, WebUser loggedInUser, SystemMessages msgs, DatabaseConnection dc = null)
        {
            request.UpdateStatus(RefundRequestStatus.DeniedByProvider, reason, loggedInUser, msgs, dc);

            if (msgs.HasNoErrors())
                Helpers.EmailHelper.SendRefundRequestDeniedByProvider(request, loggedInUser, msgs, dc);
        }

        public static void DenyRequestAsAHQ(this RefundRequest request, string reason, WebUser loggedInuser, SystemMessages msgs, DatabaseConnection dc = null)
        {
            request.UpdateStatus(RefundRequestStatus.DeniedByProvider, reason, loggedInuser, msgs, dc);
        }

        public static void ApproveRequestAsProvider(this RefundRequest request, string reason, WebUser loggedInuser, SystemMessages msgs, DatabaseConnection dc = null)
        {
            request.UpdateStatus(RefundRequestStatus.ApprovedByProvider, reason, loggedInuser, msgs, dc);

            if (msgs.HasNoErrors())
                Helpers.EmailHelper.SendRefundRequestToSupport(request, loggedInuser, msgs, dc);
        }

        public static void ApproveRequestAsAHQ(this RefundRequest request, string reason, WebUser loggedInuser, SystemMessages msgs, DatabaseConnection dc = null)
        {
            request.UpdateStatus(RefundRequestStatus.ApprovedByAfterSchoolHQ, reason, loggedInuser, msgs, dc);
        }

        public static void MarkAsCompleted(this RefundRequest request, Transaction refund, WebUser loggedInUser, SystemMessages msgs, DatabaseConnection dc = null)
        {
            request.UpdateStatus(RefundRequestStatus.Completed, loggedInUser, msgs, dc);

            if (msgs.HasNoErrors())
                Helpers.EmailHelper.SendRefundRequestProcessed(request, refund, loggedInUser, msgs, dc);
        }

        public static void UpdateStatus(this RefundRequest request, RefundRequestStatus status, WebUser loggedInuser, SystemMessages msgs, DatabaseConnection dc = null)
        {
            request.UpdateStatus(status, null, loggedInuser, msgs, dc);
        }

        public static void UpdateStatus(this RefundRequest request, RefundRequestStatus status, string reason, WebUser loggedInuser, SystemMessages msgs, DatabaseConnection dc = null)
        {
            request.RefundRequestStatusId = status;
            request.LastModifiedBy = loggedInuser.ID;
            request.LastModifiedDate = DateTime.UtcNow;

            if (status == RefundRequestStatus.ApprovedByAfterSchoolHQ || status == RefundRequestStatus.ApprovedManualRefund)
                request.ApprovedDate = DateTime.UtcNow;
            else if (status == RefundRequestStatus.DeniedByAfterSchoolHQ || status == RefundRequestStatus.DeniedByProvider)
            {
                request.DeniedDate = DateTime.UtcNow;
                request.DeniedReason = reason;
            }

            msgs.AddRange(request.Save(dc));
        }

    }
}
