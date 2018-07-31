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


        public static RegistrantSubscriptionPlan StartSubscriptionPlan(this Registrant registrant, Account fromAccount, Account toAccount, CheckoutObject checkoutObject, DateTime? startDateTime, WebUser loggedInUser, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            // process registration fee if one exists and it has a subscription
            if (!checkoutObject.ParticipationObject.IsFree && checkoutObject.SubscriptionPlan.IsNotEmpty())
            {
                // create a charge for the registration fee and save the card                
                checkoutObject.SourceId = fromAccount.GetStripeSourceFromToken(checkoutObject.SourceId, msgs);

                if (msgs.HasNoErrors())
                {
                    // if stripe token then don't send customer id
                    string customerId = null;

                    if (checkoutObject.SourceId.IsNotEmpty())
                        customerId = fromAccount.StripeCustomerId;

                    var charge = Billing.StripeHelper.CreateCharge(
                        fromCustomerId: customerId,
                        destinationAccountId: toAccount.StripeUserId,
                        tokenOrSourceId: checkoutObject.SourceId.IfEmpty(checkoutObject.StripeToken),
                        totalAmount: checkoutObject.TotalCostToPurchase,
                        platformFee: toAccount.CalculatePlatformFeeAmount(checkoutObject.TotalCostToPurchase, msgs, dc),
                        description: $"{checkoutObject.ParticipationObject.Name} registration",
                        statementDescriptor: $"AFTRSCHL/{toAccount.Name}",
                        msgs: msgs
                        );
                }
            }

            if (msgs.HasErrors())
                return null;

            // need to create a purchase record first
            var purchase = PurchaseService.CreateNew(
                purchasingAccount: fromAccount,
                checkoutObject: checkoutObject,
                loggedInUser: loggedInUser,
                msgs: msgs,
                dc: dc);

            // need to create a transaction if there's a registration fee

            // need to create an activity payment if there's a registration fee

            if (msgs.HasErrors())
                return null;

            // stripe stuff
            // create payment source if not already existent on the user
            var customer = Billing.StripeHelper.GetCustomer(fromAccount.StripeCustomerId, null, msgs);
            if (customer.DefaultSourceId.IsEmpty())
            {
                var source = Billing.StripeHelper.CreateSource(checkoutObject.StripeToken, msgs);
                msgs.AddRange(Billing.StripeHelper.AddSourceToCustomer(fromAccount.StripeCustomerId, source.Id, null));
                checkoutObject.SourceId = source.Id;
            }
            else
            {
                checkoutObject.SourceId = customer.DefaultSourceId;
            }

            // what happens if the same registrant gets another payment plan?
            // Create a new customer? For now YES, it doesn't really matter I don't think

            // get shared source
            var copiedSource = Billing.StripeHelper.CreateSourceFromOriginal(customer.Id, checkoutObject.SourceId, toAccount.StripeUserId, msgs);

            // get customer from platform, shared customer
            //var token = Billing.StripeHelper.CreateCustomerToken(fromAccount.StripeCustomerId, toAccount.StripeUserId, msgs);

            // create the customer on the connected account
            var accountCustomer = Billing.StripeHelper.CreateCustomer(
                description: fromAccount.Name,
                email: fromAccount.Email,         
                sourceId: copiedSource.Id,
                stripeAccountId: toAccount.StripeUserId,
                msgs: msgs);

            // create the subscription on the connected account
            var subscription = Billing.StripeHelper.CreateSubscription(
                customerId: accountCustomer.Id,
                planId: checkoutObject.PaymentPlan.StripePlanId,
                sourceId: copiedSource.Id,
                platformFeePercent: toAccount.GetPlatformFeePercent(msgs, dc),
                trialEndDate: null,
                stripeAccountId: toAccount.StripeUserId,
                msgs: msgs);

            if (msgs.HasErrors())
                return null;

            var rpp = RegistrantSubscriptionPlanService.CreateNew(
                registrant,
                checkoutObject.PaymentPlan,
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
