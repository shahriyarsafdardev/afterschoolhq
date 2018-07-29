using AfterSchoolLibrary.Billing;
using BasicLibrary;
using BasicLibrary.DataAccess;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AfterSchoolLibrary.Services
{
    public static class StripeAccountService
    {
        public static void SaveAndSendBankInfoToStripe(this StripeAccount stripeAccount, string routingNumber, string accountNumber, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            var account = stripeAccount.GetAccount(msgs, dc);

            var stripeStripeAccount = StripeHelper.UpdateCustomAccountBankInfo(
                stripeAccountId: account.StripeUserId,
                routingNumber: routingNumber,
                accountNumber: accountNumber,
                msgs: msgs
                );


            if (msgs.HasNoErrors())
            {
                var bankAccount = stripeStripeAccount.ExternalAccounts.Data[0].BankAccount;
                stripeAccount.BankAccountId = bankAccount.Id;
                stripeAccount.BankLast4 = bankAccount.Last4;
                stripeAccount.BankName = bankAccount.BankName;
                msgs.AddRange(stripeAccount.Save(dc));
            }

            if (msgs.HasNoErrors())
                msgs.Add("Your bank account info has been saved", MessageType.Success);

        }

        public static void GetLatestStripeAccountDetails(this StripeAccount stripeAccount, Account account = null, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (account.IsEmpty())
                account = stripeAccount.GetAccount(msgs, dc);

            var stripeStripeAccount = StripeHelper.GetCustomAccount(account.StripeUserId, msgs);

            stripeAccount.ParseAndUpdateStripeCustomAccount(stripeStripeAccount.StripeResponse.ObjectJson, null, msgs, dc);
        }

        public static void SaveAndSendLegalEntityToStripe(this StripeAccount stripeAccount, string fullSSN, StripeUploadFile document, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if(msgs.IsEmpty()) msgs = new SystemMessages();

            var account = stripeAccount.GetAccount(msgs, dc);

            Stripe.StripeFileUpload stripeFileUpload = null;

            if (stripeAccount.RequiresDocument)
                stripeFileUpload = StripeHelper.UploadFile(document, msgs);

            if (msgs.HasErrors())
                return;

            var stripeStripeAccount = StripeHelper.UpdateCustomAccountLegalEntity(
                stripeAccountId: account.StripeUserId,
                entityType: stripeAccount.LegalEntityTypeId,
                firstName: stripeAccount.LegalEntityFirstName,
                lastName: stripeAccount.LegalEntityLastName,
                ssnLast4: stripeAccount.LegalEntitySSNLast4,
                fullSSN: fullSSN,
                dateOfBirth: stripeAccount.LegalEntityDOB.Value,
                address1: stripeAccount.LegalEntityAddress1,
                address2: stripeAccount.LegalEntityAddress2,
                city: stripeAccount.LegalEntityCity,
                state: stripeAccount.LegalEntityState,
                postalCode: stripeAccount.LegalEntityPostalCode,
                supportPhone: stripeAccount.SupportPhone,
                supportEmail: stripeAccount.SupportEmail,
                businessName: stripeAccount.LegalEntityBusinessName,
                ein: stripeAccount.LegalEntityEIN,
                businessUrl: stripeAccount.BusinessUrl,
                businessPhone: stripeAccount.BusinessPhone,
                documentFileId: stripeFileUpload?.Id,
                tosAcceptanceDate: stripeAccount.TOSAcceptanceDate.Value,
                tosIPAddress: stripeAccount.TOSAcceptanceIP,
                tosUserAgent: stripeAccount.TOSAcceptanceUserAgent,
                msgs: msgs);

            if (msgs.HasNoErrors())
            {
                stripeAccount.ParseAndUpdateStripeCustomAccount(stripeStripeAccount.StripeResponse.ObjectJson, stripeFileUpload, msgs, dc);
            }
        }

        public static void ParseAndUpdateStripeCustomAccount(this StripeAccount stripeAccount, string json, Stripe.StripeFileUpload stripeFileUpload, SystemMessages msgs, DatabaseConnection dc = null)
        {
            var results = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(json);
            var fieldsRequired = results.verification.fields_needed;
            var verificationDueBy = results.verification.due_by;
            var verificationStatus = results.legal_entity.verification.status;
            var disabledReason = results.verification.disabled_reason;
            var detailsCode = results.legal_entity.verification.details_code;
            var payoutsEnabled = results.payouts_enabled;
            var chargesEnabled = results.charges_enabled;            

            stripeAccount.RequiresFullSSN = false;
            stripeAccount.RequiresDocument = false;

            foreach (var field in fieldsRequired)
            {
                switch ((string)field)
                {
                    case "legal_entity.personal_id_number":
                        stripeAccount.RequiresFullSSN = true;
                        msgs.Add("It has been determined your Full SSN required in order to process payments. Please update your information.", MessageType.Warning);
                        break;
                    case "legal_entity.verification.document":
                        stripeAccount.RequiresDocument = true;
                        msgs.Add("It has been determined that a scan of a Valid Driver's license or Passport is required to be uploaded in order to process payments. Please update your information.", MessageType.Warning);
                        break;
                }
                
            }

            if (verificationDueBy != null && ((string)verificationDueBy).IsDateTime())
                stripeAccount.VerificationDueBy = ((DateTime)verificationDueBy);

            if (disabledReason != null)
                stripeAccount.ChargesDisabledReason = (string)disabledReason;

            if (payoutsEnabled != null && ((string)payoutsEnabled).IsBoolean())
                stripeAccount.PayoutsEnabled = ((string)payoutsEnabled).ToBoolean();

            if (chargesEnabled != null && ((string)chargesEnabled).IsBoolean())
                stripeAccount.ChargesEnabled = ((string)chargesEnabled).ToBoolean();
                


            if (detailsCode != null)
            {
                msgs.Add(stripeAccount.FailedDetailsCode.GetDescription(), MessageType.Warning);
                stripeAccount.FailedDetailsCode = ((string)detailsCode).Replace("_", " ")
                                                    .ToProperCase()
                                                    .Replace(" ", "")
                                                    .ParseEnum<StripeVerificationFailedCode>();
            }

            if (verificationStatus != null)
                stripeAccount.VerificationStatusId = ((string)verificationStatus).ParseEnum<StripeVerificationStatus>();

            if (stripeAccount.VerificationStatusId == StripeVerificationStatus.Verified)
                msgs.Add("Your account is now verified", MessageType.Notice);

            if (stripeFileUpload.IsNotEmpty())
                stripeAccount.DocumentFileId = stripeFileUpload.Id;

            msgs.AddRange(stripeAccount.Save(dc));
        }

        public static Account GetAccount(this StripeAccount stripeAccount, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return Account.Load(stripeAccount.AccountId.Value, msgs, dc);
        }
    }
}
