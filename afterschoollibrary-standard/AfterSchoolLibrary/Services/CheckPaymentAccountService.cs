using AfterSchoolLibrary.Helpers;
using AfterSchoolLibrary.Models;
using BasicLibrary;
using BasicLibrary.DataAccess;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
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
                return Helpers.Utilities.IsProduction ? "bank_83e14a9fab38e24" : "bank_a9ca0ac762c4040";
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

        private static string GetCheckBottomTemplate(Payout payout, SystemMessages msgs, DatabaseConnection dc = null)
        {
            var model = new NameValueCollection()
            {
                { "ProductName", Helpers.Utilities.AppName(true) },
                { "TotalPayout", payout.Amount.ToCurrency() },
                { "Body", GetCheckBottomBody(payout, msgs, dc) }
            };

            return EmailHelper.ProcessTemplate(
               LobTemplates.PayoutSummary,
               model,
               "payoutsummary-1.2",
               msgs);
        }

        private class CheckBottomTransactionModel
        {
            public string Uid { get; set; }
            public decimal Value { get; set; }
            public string Name { get; set; }
        }

        private static string GetCheckBottomBody(Payout payout, SystemMessages msgs, DatabaseConnection dc = null)
        {
            var transactions = payout.GetTransactions(msgs, dc);
            var groupedTransactions = new Dictionary<string, decimal>();
            var checkBottomTransactions = new List<CheckBottomTransactionModel>();
            var participationObjects = new List<ParticipationObject>();
            var body = "";

            transactions.ForEach(transaction => {

                var participationObject = transaction.GetParticipationObjects(msgs, dc).FirstOrDefault();

                if (participationObject.IsNotEmpty())
                {
                    if (checkBottomTransactions.Any(cbt => cbt.Uid == participationObject.Uid.ToString()))
                    {
                        var index = checkBottomTransactions.FindIndex(cbt => cbt.Uid == participationObject.Uid.ToString());
                        checkBottomTransactions[index].Value += transaction.GetBillerPayout();
                    }
                    else
                    {
                        checkBottomTransactions.Add(new CheckBottomTransactionModel()
                        {
                            Uid = participationObject.Uid.ToString(),
                            Value = transaction.GetBillerPayout(),
                            Name = participationObject.Name,
                        });
                    }
                }
                else
                {
                    var payerAccount = transaction.GetPayerAccount(msgs, dc);
                    var payerAccountOwner = payerAccount.GetAccountOwner(msgs, dc);

                    checkBottomTransactions.Add(new CheckBottomTransactionModel()
                    {
                        Value = transaction.GetBillerPayout(),
                        Name = $"{payerAccountOwner.DisplayName} - {transaction.Description}",
                    });
                }

            });

            checkBottomTransactions.ForEach(checkBottomTransaction =>
            {
                body += $@"<tr>
                            <td>
                                {checkBottomTransaction.Name}
                            </td>
                            <td class=""text-right"">
                                {checkBottomTransaction.Value.ToCurrency()}
                            </td>
                        </tr>";
            });

            return body;
        }

        public static void SendPayment(this CheckPaymentAccount paymentAccount, Account account, Payout payout, WebUser loggedInUser, SystemMessages msgs, DatabaseConnection dc = null)
        {
            var requestor = payout.GetRequestor(msgs, dc);            
            var checkBottomHtml = GetCheckBottomTemplate(payout, msgs, dc);

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
                checkBottom: checkBottomHtml,
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
