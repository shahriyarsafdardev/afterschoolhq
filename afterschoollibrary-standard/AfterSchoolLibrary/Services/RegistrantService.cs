using System;
using System.Collections.Generic;
using System.Text;
using BasicLibrary;
using BasicLibrary.DataAccess;

namespace AfterSchoolLibrary.Services
{
    public static class RegistrantService
    {
        //public static Registrant CreateNew(Account registrantOwner, RegistrantType registrantType, WebUser loggedInUser, SystemMessages msgs, DatabaseConnection dc = null)
        //{
        //    var registrant = new Registrant
        //    {
        //        AccountId = registrantOwner.ID,
        //        RegistrantTypeId = registrantType,
        //        IsMaster = false,
        //        CreateDate = DateTime.UtcNow,
        //        LastModifiedDate = DateTime.UtcNow,
        //        CreatedBy = loggedInUser?.ID.IfEmpty(0),
        //        LastModifiedBy = loggedInUser?.ID.IfEmpty(0),
                 
        //    };
        //    msgs.AddRange(registrant.Save(dc: dc));
        //}

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


        public static RegistrantSubscriptionPlan StartSubscriptionPlan(this Registrant registrant, Account fromAccount, Account toAccount, CheckoutObject checkoutObject, SubscriptionPlanType subscriptionPlanType, DateTime? startDateTime, WebUser loggedInUser, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            Stripe.StripeCharge stripeCharge = null;

            // process tuition fee if one exists and it has a subscription
            if (!checkoutObject.ParticipationObject.IsFree)
            {
                // create source for new card to charge             
                if (checkoutObject.StripeToken.IsNotBlank())
                    checkoutObject.SourceId = fromAccount.GetStripeSourceFromToken(checkoutObject.StripeToken, msgs);

                // create charge for card
                if (msgs.HasNoErrors())
                    stripeCharge = fromAccount.CreateStripeChargeDuringCheckout(toAccount, checkoutObject, msgs, dc);
            }

            if (msgs.HasErrors())
                return null;

            // need to create a purchase record first
            var purchase = fromAccount.AddPurchase(checkoutObject, loggedInUser, msgs, dc);

            // need to create a transaction if there's a tuition fee
            // need to create an activity payment if there's a tuition fee
            if (!checkoutObject.ParticipationObject.IsFree)
            {
                fromAccount.AddTransaction(toAccount, purchase, checkoutObject, TransactionType.Charge, stripeCharge?.Id, loggedInUser, msgs, dc);

                if (checkoutObject.ParticipationObject.ObjectType == ParticipationObjectType.Activity)
                {
                    // TODO: How much does a user pay for an activity if a coupon is used to purchase multiple activities at once?
                    var activity = (Activity)checkoutObject.ParticipationObject.Object;
                    var payment = activity.AddPayment(
                        amount: activity.CostPerRegistrant.Value * checkoutObject.NumberOfSeats,
                        registrant: checkoutObject.Registration.GetRegistrant(msgs, dc),
                        loggedInUser: loggedInUser,
                        msgs: msgs,
                        dc: dc);
                }
            }

            if (msgs.HasErrors())
                return null;

            // have to copy the source from the platform to be able to move it to the stripe connected account
            var copiedSourceId = fromAccount.CopyStripeSourceToAccount(toAccount, checkoutObject.StripeToken, msgs);

            // create the customer on the connected account
            var accountCustomer = Billing.StripeHelper.CreateCustomer(
                description: fromAccount.Name,
                email: fromAccount.Email,         
                sourceId: copiedSourceId,
                stripeAccountId: toAccount.StripeUserId,
                msgs: msgs);

            var plan = subscriptionPlanType == SubscriptionPlanType.PaymentPlan ? checkoutObject.PaymentPlan : checkoutObject.SubscriptionPlan;

            // create the subscription on the connected account
            var subscription = Billing.StripeHelper.CreateSubscription(
                customerId: accountCustomer.Id,
                planId: plan.StripePlanId,
                sourceId: copiedSourceId,
                platformFeePercent: toAccount.GetPlatformPercentFee(msgs, dc),
                trialEndDate: null,
                stripeAccountId: toAccount.StripeUserId,
                msgs: msgs);

            if (msgs.HasErrors())
                return null;
 
            var rpp = RegistrantSubscriptionPlanService.CreateNew(
                registrant,
                plan,
                startDateTime.IfEmpty(DateTime.UtcNow),
                purchase.ID,
                subscription.Id,
                loggedInUser,
                msgs,
                dc);

            return rpp;
        }
        
    }
}
