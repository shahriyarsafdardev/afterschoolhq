using AfterSchoolLibrary.Billing;
using BasicLibrary;
using BasicLibrary.DataAccess;
using System;
using System.Collections.Generic;
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

        public static void SaveAndSendLegalEntityToStripe(this StripeAccount stripeAccount, string fullSSN, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if(msgs.IsEmpty()) msgs = new SystemMessages();

            msgs.AddRange(stripeAccount.Save(dc));

            var account = stripeAccount.GetAccount(msgs, dc);

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
                tosAcceptanceDate: stripeAccount.TOSAcceptanceDate.Value,
                tosIPAddress: stripeAccount.TOSAcceptanceIP,
                tosUserAgent: stripeAccount.TOSAcceptanceUserAgent,
                msgs: msgs);

            if (msgs.HasNoErrors())
            {
                var results = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(stripeStripeAccount.StripeResponse.ObjectJson);
                var fieldsRequired = results.verification.fields_needed;
                var chargesEnabled = results.charges_enabled;
                var verificationStatus = results.legal_entity.verification.status;

                stripeAccount.RequiresFullSSN = false;

                foreach (var field in fieldsRequired)
                {
                    if ((string)field == "legal_entity.personal_id_number")
                    {
                        stripeAccount.RequiresFullSSN = true;
                        msgs.Add("It has been determined your Full SSN required in order to process payments. Please update your information.", MessageType.Warning);
                    }
                }
                
                stripeAccount.ChargesEnabled = ((string)chargesEnabled).ToBoolean();

                if ((string)verificationStatus == StripeVerificationStatus.Pending.ToString().ToLower())
                    stripeAccount.VerificationStatusId = StripeVerificationStatus.Pending;

                else if ((string)verificationStatus == StripeVerificationStatus.Unverified.ToString().ToLower())
                    stripeAccount.VerificationStatusId = StripeVerificationStatus.Unverified;

                else if ((string)verificationStatus == StripeVerificationStatus.Verified.ToString().ToLower())
                    stripeAccount.VerificationStatusId = StripeVerificationStatus.Verified;
                
                if (stripeAccount.VerificationStatusId == StripeVerificationStatus.Verified)
                    msgs.Add("Your account is now verified", MessageType.Notice);

                msgs.AddRange(stripeAccount.Save(dc));                    
            }
        }

        public static Account GetAccount(this StripeAccount stripeAccount, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return Account.Load(stripeAccount.AccountId.Value, msgs, dc);
        }
    }
}
