using AfterSchoolLibrary.Billing;
using BasicLibrary;
using BasicLibrary.DataAccess;
using System;
using AfterSchoolLibrary.Models;

namespace AfterSchoolLibrary.Services
{
    public static class StripeAccountService
    {
        public static bool CanReceivePaymentDeposits(this StripeAccount stripeAccount)
        {
            return stripeAccount.ChargesEnabled && stripeAccount.PayoutsEnabled;
        }

        public static void SaveAndSendBankInfoToStripe(this StripeAccount ahqStripeAccount, string routingNumber, string accountNumber, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            var account = ahqStripeAccount.GetAccount(msgs, dc);
            var bankAccount = StripeHelper.CreateCustomBankAccount(
                    stripeAccountId: account.StripeUserId,
                    routingNumber: routingNumber,
                    accountNumber: accountNumber,
                    msgs: msgs
                    );

            if (msgs.HasNoErrors())
            {
                ahqStripeAccount.BankAccountId = bankAccount.Id;
                ahqStripeAccount.BankLast4 = bankAccount.Last4;
                ahqStripeAccount.BankName = bankAccount.BankName;
                msgs.AddRange(ahqStripeAccount.Save(dc));

                var stripeAccount = StripeHelper.GetCustomAccount(ahqStripeAccount.GetAccount(msgs, dc).StripeUserId, msgs);
                ParseAndUpdateStripeCustomAccount(ahqStripeAccount, stripeAccount, null, msgs, dc);
            }

            if (msgs.HasNoErrors())
                msgs.Add("Your bank account info has been saved", MessageType.Success);
        }

        public static void GetLatestStripeAccountDetails(this StripeAccount stripeAccount, Account account = null, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (account.IsEmpty())
                account = stripeAccount.GetAccount(msgs, dc);

            var stripeStripeAccount = StripeHelper.GetCustomAccount(account.StripeUserId, msgs);

            stripeAccount.ParseAndUpdateStripeCustomAccount(stripeStripeAccount, null, msgs, dc);
        }

        public static void SaveAndSendLegalEntityToStripe(this StripeAccount ahqStripeAccount, string fullSSN, StripeUploadFile document, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if(msgs.IsEmpty()) msgs = new SystemMessages();

            var account = ahqStripeAccount.GetAccount(msgs, dc);

            Stripe.File stripeFileUpload = null;

            if (ahqStripeAccount.RequiresDocument)
                stripeFileUpload = StripeHelper.UploadFile(document, msgs);

            if (msgs.HasErrors())
                return;

            if (account.StripeUserId.IsEmpty())
                account.CreateStripeCustomAccount(msgs, dc);

            var stripeAccount = StripeHelper.UpdateCustomAccountLegalEntity(
                stripeAccountId: account.StripeUserId,
                entityType: ahqStripeAccount.LegalEntityTypeId,
                firstName: ahqStripeAccount.LegalEntityFirstName,
                lastName: ahqStripeAccount.LegalEntityLastName,
                ssnLast4: ahqStripeAccount.LegalEntitySSNLast4,
                fullSSN: fullSSN,
                dateOfBirth: ahqStripeAccount.LegalEntityDOB,
                address1: ahqStripeAccount.LegalEntityAddress1,
                address2: ahqStripeAccount.LegalEntityAddress2,
                city: ahqStripeAccount.LegalEntityCity,
                state: ahqStripeAccount.LegalEntityState,
                postalCode: ahqStripeAccount.LegalEntityPostalCode,
                supportPhone: ahqStripeAccount.SupportPhone,
                supportEmail: ahqStripeAccount.SupportEmail,
                businessName: ahqStripeAccount.LegalEntityBusinessName,
                ein: ahqStripeAccount.LegalEntityEIN,
                businessUrl: ahqStripeAccount.BusinessUrl,
                businessPhone: ahqStripeAccount.BusinessPhone,
                documentFileId: stripeFileUpload?.Id,
                tosAcceptanceDate: ahqStripeAccount.TOSAcceptanceDate.Value,
                tosIPAddress: ahqStripeAccount.TOSAcceptanceIP,
                tosUserAgent: ahqStripeAccount.TOSAcceptanceUserAgent,
                msgs: msgs);

            if (msgs.HasNoErrors())
                ahqStripeAccount.ParseAndUpdateStripeCustomAccount(stripeAccount, stripeFileUpload, msgs, dc);
        }

        public static void ParseAndUpdateStripeCustomAccount(this StripeAccount ahqStripeAccount, Stripe.Account stripeAccount, Stripe.File stripeFileUpload, SystemMessages msgs, DatabaseConnection dc = null)
        {
            var json = stripeAccount.StripeResponse.ObjectJson;

            ahqStripeAccount.RequiresFullSSN = false;
            ahqStripeAccount.RequiresDocument = false;

            foreach (var field in stripeAccount.Requirements.CurrentlyDue)
            {
                switch ((string)field)
                {
                    case "legal_entity.personal_id_number":
                        ahqStripeAccount.RequiresFullSSN = true;
                        msgs.Add("It has been determined your Full SSN required in order to process payments. Please update your information.", MessageType.Warning);
                        break;
                    case "legal_entity.verification.document":
                        ahqStripeAccount.RequiresDocument = true;
                        msgs.Add("It has been determined that a scan of a Valid Driver's license or Passport is required to be uploaded in order to process payments. Please update your information.", MessageType.Warning);
                        break;
                }
                
            }
            
            ahqStripeAccount.VerificationDueBy = stripeAccount.Requirements.CurrentDeadline;
            ahqStripeAccount.PayoutsEnabled = stripeAccount.PayoutsEnabled;
            ahqStripeAccount.ChargesEnabled = stripeAccount.ChargesEnabled;

            if (stripeAccount.Requirements.DisabledReason.IsNotEmpty())
                ahqStripeAccount.ChargesDisabledReason = stripeAccount.Requirements.DisabledReason;

            if (stripeAccount.Individual.IsNotEmpty())
            {
                if (stripeAccount.Individual.Verification.DetailsCode != null)
                {
                    ahqStripeAccount.FailedDetailsCode = stripeAccount.Individual.Verification.DetailsCode.Replace("_", " ")
                                                        .ToProperCase()
                                                        .Replace(" ", "")
                                                        .ParseEnum<StripeVerificationFailedCode>();

                    msgs.Add(ahqStripeAccount.FailedDetailsCode.GetDescription(), MessageType.Warning);
                }

                if (stripeAccount.Individual.Verification.Status != null)
                    ahqStripeAccount.VerificationStatusId = stripeAccount.Individual.Verification.Status.ParseEnum<StripeVerificationStatus>();
            }
            

            if (ahqStripeAccount.VerificationStatusId == StripeVerificationStatus.Verified)
                msgs.Add("Your account is now verified", MessageType.Notice);

            if (stripeFileUpload.IsNotEmpty())
                ahqStripeAccount.DocumentFileId = stripeFileUpload.Id;

            msgs.AddRange(ahqStripeAccount.Save(dc));
        }

        public static Account GetAccount(this StripeAccount stripeAccount, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return Account.Load(stripeAccount.AccountId.Value, msgs, dc);
        }
    }
}
