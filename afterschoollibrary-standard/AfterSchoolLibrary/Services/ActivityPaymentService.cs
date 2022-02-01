using AfterSchoolLibrary.Models;
using BasicLibrary;
using BasicLibrary.DataAccess;
using System;
using System.Collections.Generic;
using System.Text;

namespace AfterSchoolLibrary.Services
{
    public static class ActivityPaymentService
    {
        public static void MarkAsDeleted(this ActivityPayment payment, WebUser loggedInUser, SystemMessages msgs, DatabaseConnection dc = null)
        {
            payment.IsDeleted = true;
            payment.LastModifiedDate = DateTime.UtcNow;
            payment.LastModifiedBy = loggedInUser.ID;
            msgs.AddRange(payment.Save(dc));
        }

        public static Transaction CheckTransactionForPayout(this ActivityPayment payment, string activityUniqueName, SystemMessages msgs, DatabaseConnection dc = null)
        {
            msgs.Add($"Activity payment found for activity: ({activityUniqueName}). reference-({payment.Uid})", MessageType.Success);

            var transaction = payment.GetTransaction(msgs: msgs, dc: dc);
            if (transaction.IsNotEmpty() && transaction.CanRequestPayout(msgs, dc))
            {
                msgs.Add($"Transaction worth {transaction.Amount} found. reference-({transaction.Uid})", MessageType.Success);
                return transaction;
            }
                
            msgs.Add($"Either transaction not found or transaction does not match crietria for activity: ({activityUniqueName}) and payment: ({payment.Uid}).", MessageType.Success);
            return null;
        }
    }
}
