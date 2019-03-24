using System;
using BasicLibrary;
using BasicLibrary.DataAccess;
using AfterSchoolLibrary.Models;
using System.Linq;
using System.Collections.Generic;

namespace AfterSchoolLibrary.Services
{
    public static class RegistrantService
    {
        public static WebUser MatchCreateOrUpdateUser(this Registrant registrant, string firstname, string lastname, string email, string password = null, bool isAfterSchool = false, bool forceMatch = false, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            // connecting with an existing account that has a password
            // connecting with an existing account that doesn't have a password
            //  and you aren't trying to create an official account
            //  and you are trying to create an official account
            // creating a new account

            var creatingNewAccount = password.IsNotEmpty();

            if (WebUser.ExistsByEmail(email, dc: dc))
            {
                var user = WebUser.LoadByEmail(email, dc: dc);
                if (!creatingNewAccount)
                {
                    // not creating an account, current account has no password
                    if (user.Password.IsEmpty() || forceMatch)
                        return user;

                    msgs.Add("You already have an account with this email. Login to register.", MessageType.UserError);
                    return null;
                }

                // if user already has a password and user is trying to create a new account.
                if (user.Password.IsNotEmpty())
                {
                    msgs.Add("You already have an account with this email. Login to register.", MessageType.UserError);
                    return null;
                }
            }

            return CreateOrUpdateUser(registrant, firstname, lastname, email, password, isAfterSchool, msgs, dc: dc);
        }

        /// <summary>
        /// TODO: Refactor and simplify
        /// </summary>
        /// <param name="registrant"></param>
        /// <param name="firstname"></param>
        /// <param name="lastname"></param>
        /// <param name="email"></param>
        /// <param name="password"></param>
        /// <param name="isAfterSchool"></param>
        /// <param name="msgs"></param>
        /// <param name="dc"></param>
        /// <returns></returns>
        private static WebUser CreateOrUpdateUser(Registrant registrant, string firstname, string lastname, string email, string password = null, bool isAfterSchool = false, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();
            WebUser user = null;

            Database.ExecuteTransaction
            (
                action: (SystemMessages amsgs, DatabaseConnection adc) =>
                {
                    if (WebUser.ExistsByEmail(email, amsgs, adc))
                    {
                        user = WebUser.LoadByEmail(email, amsgs, adc);

                        if (registrant.WebUserId.HasValue && user.ID != registrant.WebUserId)
                            amsgs.Add("This email is already registered to another account", MessageType.UserError);
                    }

                    if (amsgs.HasNoErrors())
                    {
                        if (user.IsEmpty())
                        {                            
                            user = new WebUser
                            {
                                CreateDate = DateTime.UtcNow,
                                CreatedBy = 0,
                                Email = email.MaxChars(150),
                                VerifiedAccount = false,
                                IsActive = false,
                                IsRegistered = false
                            };

                            user.Username = WebUserService.GenerateUsername(user.FirstName, user.LastName, amsgs, adc);
                        }

                        user.FirstName = firstname.MaxChars(50);
                        user.LastName = lastname.MaxChars(50);
                        user.LastModifiedDate = DateTime.UtcNow;
                        user.LastModifiedBy = registrant.WebUserId;

                        if (password.IsNotBlank())
                        {
                            user.IsActive = true;
                            user.IsRegistered = true;
                            user.Password = WebUserService.EncryptPassword(password);
                        }

                        msgs.AddRange(user.Save(adc));

                        registrant.WebUserId = user.ID;
                        msgs.AddRange(registrant.Save(adc));
                    }
                },
                onSuccess: (SystemMessages smsgs) =>
                {
                    msgs.Add("Your account has been created", MessageType.Success);

                    // verify if there is a password and the user hasn't changed their email
                    if (password.IsNotBlank() && user.Email.ToLower() != registrant.Email.ToLower())
                        user.SendVerificationEmail(isAfterSchool);
                },
                onFailure: (SystemMessages fmsgs) =>
                {
                    msgs.Add("Error creating account", MessageType.Error);
                },
                dc: dc
            );

            return user;
        }

        private static Tuple<Stripe.StripeCustomer, string> GetCustomerAndSourceForSubscription(CheckoutObject checkoutObject, bool useStripeConnectedAccount)
        {
            // have to copy the source from the platform to be able to move it to the stripe connected account
            // unless you're not using connected accounts for subscriptions
            var msgs = new SystemMessages();
            var billingAccount = checkoutObject.BillingAccount;
            var purchasingAccount = checkoutObject.PurchasingAccount;
            string sourceId = null;
            Stripe.StripeCustomer customer = null;

            if (!useStripeConnectedAccount)
            {
                customer = purchasingAccount.GetStripeCustomer(msgs);
                sourceId = purchasingAccount.CreateStripeSourceFromToken(checkoutObject.StripeToken, customer, msgs);
            }
            else
            {
                // if running the billing through stripe connected account
                sourceId = purchasingAccount.CopyStripeSourceToAccount(checkoutObject.BillingAccount, checkoutObject.StripeToken, msgs);

                // create the customer on the connected account
                customer = Billing.StripeHelper.CreateCustomer(
                    description: purchasingAccount.Name,
                    email: purchasingAccount.Email,
                    sourceId: sourceId,
                    stripeAccountId: billingAccount.StripeUserId,
                    msgs: msgs);
            }

            return new Tuple<Stripe.StripeCustomer, string>(customer, sourceId);
        }

        public static void StartSubscriptionPlan(this Registrant registrant, CheckoutObject checkoutObject, SubscriptionPlanType subscriptionPlanType, DateTime? startDateTime, WebUser loggedInUser, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            var billingAccount = checkoutObject.BillingAccount;
            var purchasingAccount = checkoutObject.PurchasingAccount;            
            var useStripeConnectedAccount = billingAccount.HasAccountPackageFeature(FeatureType.UseStripeConnectedAccount, msgs, dc);
            var (customer, sourceId) = GetCustomerAndSourceForSubscription(checkoutObject, useStripeConnectedAccount);
            var plan = subscriptionPlanType == SubscriptionPlanType.PaymentPlan ? checkoutObject.PaymentPlan : checkoutObject.SubscriptionPlan;
            var stripeCouponId = plan.DoesCouponApply(checkoutObject.Coupon) ? checkoutObject.Coupon.StripeCouponId : null;

            // create the subscription on the stripe account (could be added to main account or connected account)
            var subscription = Billing.StripeHelper.CreateSubscription(
                customerId: customer.Id,
                planId: plan.StripePlanId,
                sourceId: sourceId,
                platformFeePercent: useStripeConnectedAccount ? billingAccount.GetSubscriptionPlatformFeePercent(plan, msgs, dc) : new decimal?(),
                stripeAccountId: useStripeConnectedAccount ? billingAccount.StripeUserId: null,
                trialEndDate: null,
                stripeCouponId: stripeCouponId,
                msgs: msgs);

            if (msgs.HasErrors())
                return;

            var purchase = checkoutObject.CreateOrUpdatePurchase(loggedInUser, msgs, dc);

            checkoutObject.RegistrantSubscriptionPlan = plan.AddRegistrant(
                registrant,
                startDateTime,
                purchase,
                subscription.Id,
                loggedInUser,
                msgs,
                dc);
        }
     
        public static string GetParentOrGuardianEmail(this Registrant registrant, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (registrant.ParentRegistrantId.HasValue)
                return registrant.GetMasterRegistrant(msgs, dc).GetParentOrGuardian(msgs, dc).Email;

            if (registrant.IsMaster)
                return registrant.GetParentOrGuardian(msgs, dc).Email;

            return null;
        }

        internal static SystemMessages SendText(this Registrant registrant, string subject, string body)
        {
            var msgs = new SystemMessages();

            if (registrant.Phone.IsEmpty())
                msgs.Add($"{registrant.DisplayName} has no phone number available", MessageType.Notice);

            if (!registrant.AllowTextMessages)
                msgs.Add($"{registrant.DisplayName} does not allow text messaging", MessageType.Notice);

            if (registrant.Phone.IsNotEmpty() && registrant.AllowTextMessages)
                return TwilioHelper.SendSMS(
                    registrant.Phone,
                    $"{registrant.Phone}\n{body}"
                    );

            return msgs;
        }

        internal static SystemMessages SendEmail(this Registrant registrant, bool isAfterSchool, string fromName, string replyToEmail, string replyToName, string subject, string body, string utmCampaign = "")
        {
            var msgs = new SystemMessages();

            if (registrant.Email.IsEmpty())
                msgs.Add("No email address available", MessageType.Notice);

            if (registrant.Email.IsNotEmpty())
                msgs.AddRange(SendGridHelper.SendMail(
                    fromEmail: new SendGrid.Helpers.Mail.EmailAddress(
                        Helpers.Email.AppEmailAddress,
                        $"{fromName} via {Helpers.Utilities.AppName(isAfterSchool)}"),
                    toEmail: new SendGrid.Helpers.Mail.EmailAddress(registrant.Email, registrant.DisplayName),
                    subject: subject,
                    body: $"{body}{(Helpers.Email.GetAppSignature(isAfterSchool: isAfterSchool, utm_campaign: utmCampaign))}",
                    replyTo: new SendGrid.Helpers.Mail.EmailAddress(replyToEmail, replyToName),
                    emailTemplateId: SendGridHelper.GetGenericTemplateId(isAfterSchool)
                ));

            return msgs;
        }

        public static List<ParticipationObject> GetParticipationObjects(this Registrant registrant, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return RegistrantActivity.LoadByRegistrantId(registrant.ID.Value, msgs, dc)
                .Select(x => x.GetParticipationObject(msgs, dc))
                .ToList();
        }

    }
}
