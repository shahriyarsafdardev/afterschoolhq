using AfterSchoolLibrary.Helpers;
using AfterSchoolLibrary.Models;
using BasicLibrary;
using BasicLibrary.DataAccess;
using System;
using System.Collections.Generic;
using System.Text;

namespace AfterSchoolLibrary.Services
{
    public static class CheckPaymentAccountService
    {
        private static string LobFromAddressId
        {
            get
            {
                return Helpers.Utilities.IsProduction ? "adr_9be39e9b290dbb3b" : "adr_7e68ac15e4ff4bf7";
            }
        }
        private static string LobBankAccountId
        {
            get
            {
                return Helpers.Utilities.IsProduction ? "bank_e90556aa067848d" : "bank_a9ca0ac762c4040";
            }
        }
        
        public static bool IsDeliverable(this CheckPaymentAccount paymentAccount, SystemMessages msgs, DatabaseConnection dc = null)
        {
            var isDeliverable = LobHelper.IsDeliverableAddress(
                address1: paymentAccount.Address1,
                city: paymentAccount.City,
                state: paymentAccount.State,
                postalCode: paymentAccount.PostalCode,
                msgs: msgs);

            if (isDeliverable)
            {
                paymentAccount.IsVerified = true;
                msgs.AddRange(paymentAccount.Save(dc));
            }

            return isDeliverable;
        }

        public static void SendPayment(this CheckPaymentAccount paymentAccount, Account account, Payout payout, WebUser loggedInUser, SystemMessages msgs, DatabaseConnection dc = null)
        {
            var requestor = payout.GetRequestor(msgs, dc);            

            var check = LobHelper.CreateCheck(
                description: $"Payout to {account.Name} requested {payout.Amount.ToCurrency()}",
                accountType: paymentAccount.LegalEntityTypeId.ToLobAccountType(),
                toName: paymentAccount.Name,
                toAddress1: paymentAccount.Address1,
                toAddress2: paymentAccount.Address2,
                toCity: paymentAccount.City,
                toState: paymentAccount.State,
                toPostalCode: paymentAccount.PostalCode,
                fromAddressId: LobFromAddressId,
                bankAccountId: LobBankAccountId,
                amount: payout.Amount.Value,
                memo: $"Requested {payout.DateRequested.ToShortDateString()} by {requestor.DisplayName}".MaxChars(40, "..."),
                checkBottom: $"{Helpers.Utilities.BaseUrl(true)}/lob/payout/attachment/{payout.Uid.ToString()}",
                attachment: null,
                metaData: new
                {
                    accountUid = account.Uid,
                    requestedBy = requestor.Uid,
                    approvedBy = loggedInUser.Uid
                },
                msgs: msgs);

            payout.SetExternalPayoutId(check.Id, loggedInUser, msgs, dc);

        }
    }
}
