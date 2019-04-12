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
        private static string LobFromAddressId = "adr_7e68ac15e4ff4bf7";
        private static string LobBankAccountId = "bank_a9ca0ac762c4040";

        public static void VerifyAddress(this CheckPaymentAccount paymentAccount, WebUser loggedInUser, SystemMessages msgs, DatabaseConnection dc = null)
        {
            // TODO: lob address verification


        }

        public static void SendPayment(this CheckPaymentAccount paymentAccount, Account account, Payout payout, WebUser loggedInUser, SystemMessages msgs, DatabaseConnection dc = null)
        {
            var requestor = payout.GetRequestor(msgs, dc);            

            LobHelper.CreateCheck(
                description: $"Payout to {account.Name} requested {payout}",
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
                metaData: new
                {
                    accountUid = account.Uid,
                    requestedBy = requestor.Uid,
                    approvedBy = loggedInUser.Uid
                },
                msgs: msgs);
        }
    }
}
