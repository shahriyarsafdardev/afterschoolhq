using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BasicLibrary;
using BasicLibrary.DataAccess;

namespace AfterSchoolLibrary.Services
{
    public class TransactionService
    {
        public static Transaction CreateNew(Account fromAccount, Account toAccount, CheckoutObject checkoutObject, int? purchaseId, WebUser loggedInUser, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            var transaction = new Transaction()
            {
                Amount = checkoutObject.RegistrationFee,
                BillerAccountId = toAccount.ID.Value,
                Description = $"{checkoutObject.ParticipationObject.Name} registration",
                PayerAccountId = fromAccount.ID,
                PaymentTypeId = PaymentType.CreditCard,
                PurchaseId = purchaseId,
                CreateDate = DateTime.UtcNow,
                CreatedBy = loggedInUser?.ID.IfEmpty(0),
                LastModifiedDate = DateTime.UtcNow,
                LastModifiedBy = loggedInUser?.ID.IfEmpty(0),
            };
            msgs.AddRange(transaction.Save(dc));

            return transaction;
        }
    }
}
