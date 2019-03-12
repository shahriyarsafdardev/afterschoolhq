using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BasicLibrary;
using BasicLibrary.DataAccess;
using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Billing;

namespace AfterSchoolLibrary.Services
{
    public static class AccountService
    {
        public static bool HasFeature(this Account account, FeatureType featureType, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return account.GetActiveAccountPackage(msgs, dc).HasFeature(featureType);
        }

        public static WebUser GetAccountOwner(this Account account, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return WebUser.Load(account.OwnerId.Value, msgs, dc);
        }

        public static List<WebUser> GetAccountOwnersAndAdmins(this Account account, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return AccountWebUser.LoadByAccountIdByIsActive(account.ID.Value, true, msgs, dc)
                .Where(x => x.UserTypeId == UserType.Administrator || x.UserTypeId == UserType.Owner)
                .Select(x => x.ToWebUser())
                .ToList();
        }

        public static List<Form> GetForms(this Account account, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return Form.LoadByAccountId(account.ID.Value, msgs, dc);
        }

        public static List<ActivitySchedule> GetActivitySchedules(this Account account, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return ActivitySchedule.LoadByAccountId(account.ID.Value, msgs, dc);
        }

        #region Activities

        public static List<Activity> GetPublishedActivities(this Account account, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var periods = account.GetOpenUpcomingRecentlyClosedRegistrationPeriods(msgs, dc);
            var activities = new List<Activity>();

            periods.ForEach(period => activities.AddRange(period.GetActivities(msgs, dc).Where(x => x.IsPubliclyViewable)));

            return activities;
        }

        public static List<Activity> GetPublishedActivitiesInLocations(this Account account, List<int> locationIds, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var periods = account.GetOpenUpcomingRecentlyClosedRegistrationPeriods(msgs, dc);
            var activities = new List<Activity>();

            periods.ForEach(period => 
                activities.AddRange(period.GetActivitiesInLocations(locationIds, msgs, dc).Where(x => x.IsPubliclyViewable))
            );

            return activities;
        }

        public static List<ActivityForm> GetActivityForms(this Account account, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return ActivityForm.LoadByAccountId(account.ID.Value, msgs, dc);
        }

        public static List<Activity> GetActivities(this Account account, UserType userType, WebUser user, ActivityType? activityType, bool? isArchived, bool? isDeleted, int? start = null, int? length = null, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (user.IsEmpty() || userType.In(new UserType[] { UserType.Administrator, UserType.Owner, UserType.SuperAdmin }))
                return Activity.LoadByAccountId(account.ID.Value, null, null, activityType, isArchived, isDeleted, start, length, msgs, dc).ToList();

            return Activity.LoadByAccountId(account.ID.Value, null, null, activityType, isArchived, isDeleted, start, length, msgs, dc)
                    .Where(activity => user.HasAccessToActivity(activity, msgs, dc))
                    .ToList();
        }

        public static int GetActivityCount(this Account account, UserType userType, WebUser user, ActivityType? activityType, bool? isArchived, bool? isDeleted, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (user.IsEmpty() || userType.In(new UserType[] { UserType.Administrator, UserType.Owner, UserType.SuperAdmin }))
                return Activity.GetCountByAccountId(account.ID.Value, null, activityType, isArchived, isDeleted, msgs, dc);

            return account.GetActivities(userType, user, activityType, isArchived, isDeleted, null, null, msgs, dc).Count();
        }

        #endregion

        #region Contacts

        public static int SearchContactCount(this Account account, string searchtext, int? contactListId, DatabaseConnection dc = null)
        {
            return Registrant.SearchRegistrantCount(searchtext, contactListId, account.ID.Value, dc: dc);
        }

        public static List<Registrant> SearchContacts(this Account account, string searchtext, int? contactListId, int? start = null, int? length = null, DatabaseConnection dc = null)
        {
            return Registrant.SearchRegistrants(searchtext, contactListId, account.ID.Value, start, length, dc: dc);
        }

        public static int GetContactCount(this Account account, DatabaseConnection dc = null)
        {
            return Registrant.GetCountByAccountId(account.ID.Value, dc: dc);
        }

        public static List<Registrant> GetContacts(this Account account, int? start = null, int? length = null, DatabaseConnection dc = null)
        {
            return Registrant.LoadByAccountId(account.ID.Value, start, length, dc: dc);
        }

        public static List<ContactList> GetContactLists(this Account account, int? start = null, int? length = null, DatabaseConnection dc = null)
        {
            return ContactList.LoadByAccountId(account.ID.Value, start, length, dc: dc);
        }

        public static int GetContactListCount(this Account account, DatabaseConnection dc = null)
        {
            return ContactList.GetCount(criterias: new Hashtable() { { "AccountId", account.ID.Value } }, dc: dc);
        }

        #endregion

        #region Sessions

        public static List<ActivityGroup> GetSessions(this Account account, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return ActivityGroup.LoadByAccountIdByActivityGroupTypeId(account.ID.Value, ActivityGroupType.Session, msgs, dc);
        }

        public static List<ActivityGroup> GetPublishedSessions(this Account account, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var groups = ActivityGroup.LoadByAccountIdByIsPublished(account.ID.Value, true, msgs, dc);
            var periods = RegistrationPeriod.LoadByActivityGroupIds(
                    groups.Select(x => x.ID.Value).ToList(),
                    msgs,
                    dc
                )
                .Where(period => period.IsOpen() || period.WillOpen() || period.WillCloseIn().Days > -14)
                .ToList();

            return groups.Where(group => periods.Select(period => period.ActivityGroupId.Value).Contains(group.ID.Value)).ToList();
        }

        public static List<ActivityGroup> GetSessions(this Account account, bool IsArchived, int? start, int? length, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return ActivityGroup.LoadByAccountId(account.ID.Value, ActivityGroupType.Session, IsArchived, start, length, msgs, dc);
        }

        public static int GetSessionCount(this Account account, bool IsArchived = false, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return ActivityGroup.GetCount(
                new Hashtable() {
                    { "AccountId", account.ID.Value },
                    { "ActivityGroupTypeId", (int)ActivityGroupType.Session },
                    { "IsArchived", IsArchived }
                }, 
                msgs: msgs,
                dc: dc);
        }

        #endregion

        #region RegistrationPeriods

        public static List<RegistrationPeriod> GetRegistrationPeriods(this Account account, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return RegistrationPeriod.LoadByAccountId(account.ID.Value, msgs, dc);
        }

        public static List<RegistrationPeriod> GetRegistrationPeriods(this Account account, int? start, int? length, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return RegistrationPeriod.LoadByAccountId(account.ID.Value, start, length, msgs, dc);
        }

        public static int GetRegistrationPeriodCount(this Account account, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return RegistrationPeriod.GetCount(new Hashtable() { { "AccountId", account.ID.Value } }, msgs, dc);
        }

        public static List<RegistrationPeriod> GetUpcomingRegistrationPeriods(this Account account, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return account.GetRegistrationPeriods(msgs, dc)
                .Where(x => x.StartDateTime.Value > account.ConvertToAccountTimeZone(DateTime.UtcNow))
                .OrderBy(x => x.StartDateTime.Value)
                .ToList();
        }

        public static List<RegistrationPeriod> GetOpenRegistrationPeriods(this Account account, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return account.GetRegistrationPeriods()
                .Where(x => 
                    x.StartDateTime.Value <= account.ConvertToAccountTimeZone(DateTime.UtcNow) && 
                    x.EndDateTime.Value >= account.ConvertToAccountTimeZone(DateTime.UtcNow)
                 )
                .ToList();
        }

        public static List<RegistrationPeriod> GetClosedRegistrationPeriods(this Account account, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return account.GetRegistrationPeriods()
                .Where(x => x.EndDateTime.Value < account.ConvertToAccountTimeZone(DateTime.UtcNow))
                .ToList();
        }

        public static List<RegistrationPeriod> GetRecentlyClosedRegistrationPeriods(this Account account, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return account.GetRegistrationPeriods()
                .Where(x => x.EndDateTime.Value.AddWeeks(2) < account.ConvertToAccountTimeZone(DateTime.UtcNow))
                .ToList();
        }

        public static List<RegistrationPeriod> GetOpenAndUpcomingRegistrationPeriods(this Account account, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var periods = account.GetOpenRegistrationPeriods(msgs, dc);
            periods.AddRange(account.GetUpcomingRegistrationPeriods(msgs, dc));
            return periods;
        }

        public static List<RegistrationPeriod> GetOpenUpcomingRecentlyClosedRegistrationPeriods(this Account account, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var periods = account.GetOpenRegistrationPeriods(msgs, dc);
            periods.AddRange(account.GetUpcomingRegistrationPeriods(msgs, dc));
            periods.AddRange(account.GetRecentlyClosedRegistrationPeriods(msgs, dc));
            return periods;
        }

        #endregion

        #region AccountPackage

        public static AccountPackage GetActiveAccountPackage(this Account account, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return AccountPackage.LoadByAccountIdByIsActive(account.ID.Value, true, msgs, dc);
        }

        public static List<AccountPackage> GetAccountPackages(this Account account, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return AccountPackage.LoadByAccountId(account.ID.Value, msgs, dc);
        }

        public static List<AccountPackageFeature> GetAccountPackageFeatures(this Account account, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var accountPackage = account.GetActiveAccountPackage(msgs, dc);

            if (accountPackage.IsNotEmpty())
                return AccountPackageFeature.LoadByAccountPackageId(accountPackage.ID.Value, msgs, dc);

            return new List<AccountPackageFeature>();
        }

        public static AccountPackageFeature GetAccountPackageFeature(this Account account, FeatureType featureType, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var accountPackage = account.GetActiveAccountPackage(msgs, dc);

            if (accountPackage.IsNotEmpty())
                return AccountPackageFeature.LoadByAccountPackageIdByFeatureTypeId(accountPackage.ID.Value, featureType, msgs, dc);

            return null;
        }

        public static bool HasAccountPackageFeature(this Account account, FeatureType featureType, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var accountPackage = account.GetActiveAccountPackage(msgs, dc);

            if (accountPackage.IsNotEmpty())
                return AccountPackageFeature.HasAccountPackageIdByFeatureTypeId(accountPackage.ID.Value, featureType, msgs, dc);

            return false;
        }

        #endregion

        #region Users

        public static int GetUserCount(this Account account, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return AccountWebUser.GetCount(
                new Hashtable() { { "AccountId", account.ID } },
                msgs,
                dc
                );
        }

        public static int GetActiveUserCount(this Account account, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return AccountWebUser.GetCount(
                new Hashtable() {
                    { "AccountId", account.ID },
                    { "IsActive", true }
                },
                msgs,
                dc
                );
        }

        public static List<WebUser> GetUsers(this Account account, int? start = null, int? length = null, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return AccountWebUser.Load(
                new Hashtable() { { "AccountId", account.ID } },
                start,
                length,
                msgs,
                dc
                )
                .Select(x => x.ToWebUser())
                .ToList();
        }

        public static List<AccountWebUser> GetMembers(this Account account, DatabaseConnection dc = null)
        {
            return AccountWebUser.LoadByAccountId(account.ID.Value, dc: dc);
        }

        public static List<AccountWebUser> GetActiveMembers(this Account account, DatabaseConnection dc = null)
        {
            return AccountWebUser.LoadByAccountIdByIsActive(account.ID.Value, true, dc: dc);
        }

        public static SystemMessages AddUser(this Account account, WebUser user, UserType ut, int loggedInUserId, DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();

            Database.ExecuteTransaction(
                action: (SystemMessages amsgs, DatabaseConnection adc) =>
                {
                    var awu = AccountWebUser.LoadByAccountIdByWebUserId(account.ID.Value, user.ID.Value, dc: adc);
                    if (awu.IsEmpty())
                        awu = new AccountWebUser
                        {
                            CreateDate = DateTime.UtcNow,
                            CreatedBy = loggedInUserId,
                            WebUserId = user.ID.Value,
                            AccountId = account.ID.Value,
                            IsDefault = false,
                            IsActive = user.IsActive
                        };
                    else
                        awu.IsActive = true;

                    awu.LastModifiedDate = DateTime.UtcNow;
                    awu.LastModifiedBy = loggedInUserId;
                    awu.UserTypeId = ut;
                    amsgs.AddRange(awu.Save(adc));

                    if (ut == UserType.Owner && account.OwnerId != user.ID)
                    {
                        account.OwnerId = user.ID.Value;
                        amsgs.AddRange(account.Save(adc));
                    }
                },
                onSuccess: (SystemMessages smsgs) =>
                {
                    msgs.AddRange(smsgs);
                },
                onFailure: (SystemMessages fmsgs) =>
                {
                    msgs.AddRange(fmsgs);
                },
                onException: (SystemMessages emsgs) =>
                {
                    Helpers.Utilities.SystemErrorLog(emsgs);
                },
                dc: dc
            );

            return msgs;
        }

        public static WebUser AddUser(this Account account, string firstName, string lastName, string email, UserType userType, int loggedInUserId, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return account.AddUser(firstName, lastName, email, null, null, false, true, false, userType, loggedInUserId, msgs, dc: dc);
        }

        public static WebUser AddUser(this Account account, string firstName, string lastName, string email, string phone, UserType userType, int loggedInUserId, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return account.AddUser(firstName, lastName, email, phone, null, false, true, false, userType, loggedInUserId, msgs, dc: dc);
        }

        public static WebUser AddUser(this Account account, string firstName, string lastName, string email, string phone, string password, bool isActive, bool isRegistered, bool isVerified, UserType userType, int loggedInUserId, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();
            WebUser user = null;

            Database.ExecuteTransaction(
                action: (SystemMessages amsgs, DatabaseConnection adc) =>
                {
                    if (!WebUser.ExistsByEmail(email.Trim(), amsgs, adc))
                    {
                        user = new WebUser();
                        user.FirstName = firstName.Trim();
                        user.LastName = lastName.Trim();
                        user.Email = email.Trim();
                        user.Phone = phone.Trim();
                        user.Username = WebUserService.GenerateUsername(firstName, lastName, amsgs, adc);

                        if (!password.IsEmpty())
                            user.Password = WebUserService.EncryptPassword(password);

                        user.CreateDate = user.LastModifiedDate = DateTime.Now;
                        user.CreatedBy = user.LastModifiedBy = loggedInUserId;
                        user.IsActive = isActive;
                        user.IsRegistered = isRegistered;
                        user.VerifiedAccount = isVerified;
                        user.Uid = Guid.NewGuid();
                        amsgs.AddRange(user.Save(adc));
                    }
                    else
                        user = WebUser.LoadByEmail(email.Trim(), amsgs, adc);
                },
                onSuccess: (SystemMessages smsgs) =>
                {
                    msgs.AddRange(smsgs);
                },
                onFailure: (SystemMessages fmsgs) =>
                {
                    msgs.AddRange(fmsgs);
                },
                onException: (SystemMessages emsgs) =>
                {
                    Helpers.Utilities.SystemErrorLog(emsgs);
                },
                dc: dc
            );

            if (msgs.HasNoErrors())
                msgs.AddRange(AddUser(account, user, userType, loggedInUserId, dc: dc));

            return user;

        }

        public static WebUser InviteUser(this Account account, string firstName, string lastName, string email, string phone, string message, UserType userType, WebUser loggedInUser, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            var user = account.AddUser(firstName, lastName, email, phone, userType, loggedInUser.ID.Value, msgs, dc);
            if (msgs.HasNoErrors())
                msgs.AddRange(account.SendInvitedEmail(user, loggedInUser, message, dc));

            if (msgs.HasErrors())
                msgs.Add($"An invitation was not sent to {email.Trim()}, a system error has occurred", MessageType.UserError);

            return user;
        }

        #endregion

        #region Emails

        public static SystemMessages SendWelcomeEmailTrial(this Account account, DatabaseConnection dc = null)
        {
            return Helpers.EmailHelper.SendWelcomeTrial(account.GetAccountOwner(dc: dc), account);
        }

        public static SystemMessages SendSubscriptionPaymentSucceeded(this Account account, SubscriptionPlan plan, DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();
            var accountOwner = account.GetAccountOwner(msgs, dc);            
            msgs.AddRange(Helpers.EmailHelper.SendSubscriptionPaymentSucceeded(accountOwner, plan));
            return msgs;
        }

        public static SystemMessages SendSubscriptionPaymentFailed(this Account account, SubscriptionPlan plan, DatabaseConnection dc)
        {
            var msgs = new SystemMessages();
            var accountOwner = account.GetAccountOwner(msgs, dc);
            msgs.AddRange(Helpers.EmailHelper.SendSubscriptionPaymentFailed(account, accountOwner, plan, account.IsAfterSchool));
            return msgs;
        }

        public static SystemMessages SendStripeAccountVerified(this Account account, DatabaseConnection dc)
        {
            var msgs = new SystemMessages();
            var accountOwner = account.GetAccountOwner(msgs, dc);
            msgs.AddRange(Helpers.EmailHelper.SendStripeAccountVerified(account, accountOwner, account.IsAfterSchool));
            return msgs;
        }

        public static SystemMessages SendWelcomeEmailPurchase(this Account account, DatabaseConnection dc = null)
        {
            return Helpers.EmailHelper.SendWelcomePurchase(account.GetAccountOwner(dc: dc), account);
        }

        public static SystemMessages SendInvitedEmail(this Account invitedAccount, WebUser invitedUser, WebUser loggedInUser, string customMessage = null, DatabaseConnection dc = null)
        {
            return Helpers.EmailHelper.SendAccountInvitedEmail(invitedUser, invitedAccount, loggedInUser, customMessage);
        }

        #endregion

        #region Stripe

        public static SystemMessages SaveWithStripe(this Account account, DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();

            msgs.AddRange(account.Save(dc: dc));

            if (msgs.HasNoErrors())
            {
                msgs.AddRange(account.CreateStripeCustomer(dc));
            }

            return msgs;
        }

        public static void CreateStripeCustomAccount(this Account account, SystemMessages msgs, DatabaseConnection dc = null)
        {
            var stripeAccount = Billing.StripeHelper.CreateCustomAccount(account.Name, account.Email, msgs);

            if(stripeAccount.IsNotEmpty())
            {
                account.StripePublishableKey = stripeAccount.CustomAccountKeys.Publishable;
                account.StripeSecretKey = stripeAccount.CustomAccountKeys.Secret;
                account.StripeUserId = stripeAccount.Id;
                msgs.AddRange(account.Save(dc));
            }
        }

        public static SystemMessages CreateStripeCustomer(this Account account, DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();

            var customer = account.GetStripeCustomer(msgs);

            if (customer.IsNotEmpty())
            {
                customer = Billing.StripeHelper.UpdateCustomer(customer.Id, account.Name, account.Email, null, msgs);
            }
            else
            {
                customer = Billing.StripeHelper.CreateCustomer(account.Name, account.Email, null, msgs);

                if (customer.IsNotEmpty())
                {
                    account.StripeCustomerId = customer.Id;
                    msgs.AddRange(account.Save(dc: dc));
                }
            }
            return msgs;
        }

        public static Stripe.StripeSource GetStripeSource(this Account account, string sourceId, SystemMessages msgs = null)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            Stripe.StripeSource source = null;
            var customer = account.GetStripeCustomer(msgs);

            if (customer.IsNotEmpty())
            {
                source = Billing.StripeHelper.GetSource(sourceId, msgs);
            }

            return source;
        }

        public static List<Stripe.StripeSource> GetStripeSources(this Account account, SystemMessages msgs = null)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            var sources = new List<Stripe.StripeSource>();
            var customer = account.GetStripeCustomer(msgs);

            if (customer.IsNotEmpty())
            {
                sources.AddRange(Billing.StripeHelper.GetSources(customer.Id, msgs));
            }

            return sources;
        }

        public static List<Stripe.StripeCharge> GetStripeBillingHistory(this Account account, SystemMessages msgs = null)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            var charges = new List<Stripe.StripeCharge>();
            var customer = account.GetStripeCustomer(msgs);

            if (customer.IsNotEmpty())
            {
                charges = Billing.StripeHelper.GetCharges(customer.Id, msgs);

                //try
                //{
                //    Stripe.StripeChargeService service = new Stripe.StripeChargeService(StripeHelper.SecretKey);
                //    Stripe.StripeChargeListOptions options = new Stripe.StripeChargeListOptions();
                //    options.CustomerId = customer.Id;
                //    options.Limit = 10;
                //    charges.AddRange(service.List(options).ToList());
                //}
                //catch (Stripe.StripeException ex)
                //{
                //    //error storing card
                //    msgs.Add("Error retrieving billing history", MessageType.Error, ex);
                //}
            }

            return charges;
        }

        public static List<Stripe.StripeRefund> GetStripeRefundHistory(this Account account, SystemMessages msgs = null)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            var refunds = new List<Stripe.StripeRefund>();
            var customer = account.GetStripeCustomer(msgs);

            if (!customer.IsEmpty())
            {
                var charges = account.GetStripeBillingHistory(msgs);
                foreach(var item in charges)
                {
                    refunds.AddRange(Billing.StripeHelper.GetRefunds(item.Id, msgs));
                }

                //try
                //{
                //    var service = new Stripe.StripeRefundService(StripeHelper.SecretKey);
                //    refunds.AddRange(service.List(null).ToList());
                //}
                //catch (Stripe.StripeException ex)
                //{
                //    msgs.Add("Error retrieving refund history", MessageType.Error, ex);
                //}
            }

            return refunds;
        }

        public static Stripe.StripeCustomer GetStripeCustomer(this Account account, SystemMessages msgs)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            Stripe.StripeCustomer customer = null;

            if (!account.StripeCustomerId.IsEmpty())
            {
                customer = Billing.StripeHelper.GetCustomer(account.StripeCustomerId, null, msgs);
            }
            return customer;
        }

        public static void CreateStripeCoupon(Coupon coupon, string stripeAccountId, StripeCouponDuration couponDuration, int? durationInMonths, decimal discountValue, SystemMessages msgs,  DatabaseConnection dc = null)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            var StripeCoupon = Billing.StripeHelper.CreateCoupon(discountValue, stripeAccountId, coupon.AmountTypeId, couponDuration, durationInMonths, msgs);

            if (StripeCoupon.IsNotEmpty())
            {
                coupon.StripeCouponId = StripeCoupon.Id;
                msgs.AddRange(coupon.Save(dc: dc));
            }
        }

        #endregion

        #region Registrants

        /// <summary>
        /// creates the initial contact/student record for the current account, not the global registrant account, details are added later
        /// </summary>
        /// <param name="account"></param>
        /// <param name="loggedInUserId"></param>
        /// <param name="registrantType"></param>
        /// <param name="isMaster"></param>
        /// <param name="msgs"></param>
        /// <param name="dc"></param>
        /// <returns></returns>
        public static Registrant AddRegistrant(this Account account, int loggedInUserId, RegistrantType registrantType, bool isMaster, SystemMessages msgs, DatabaseConnection dc = null)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            var registrant = new Registrant
            {
                AccountId = account.ID,
                RegistrantTypeId = registrantType,
                IsMaster = isMaster,
                CreateDate = DateTime.UtcNow,
                LastModifiedDate = DateTime.UtcNow,
                CreatedBy = loggedInUserId,
                LastModifiedBy = loggedInUserId
            };
            msgs.AddRange(registrant.Save(dc: dc));

            return registrant;
        }

        public static List<RegistrantActivity> GetRegistrantActivities(this Account account, int? start = null, int? length = null, DatabaseConnection dc = null)
        {
            return RegistrantActivity.LoadByAccountId(account.ID.Value, start, length, dc: dc).ToList();
        }

        public static int GetRegistrantActivityCount(this Account account, DatabaseConnection dc = null)
        {
            return RegistrantActivity.GetCount(new Hashtable() { { "ActivityAccountId", account.ID.Value } });
        }

        #endregion

        #region AccountSettingsAndProperties

        public static DateTime ConvertToAccountTimeZone(this Account account, DateTime dt)
        {
            switch (account.TimeZoneAbbr)
            {
                case "EST":
                    return TimeZoneInfo.ConvertTimeFromUtc(dt, TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time"));
                case "CST":
                    return TimeZoneInfo.ConvertTimeFromUtc(dt, TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time"));
                case "MST":
                    return TimeZoneInfo.ConvertTimeFromUtc(dt, TimeZoneInfo.FindSystemTimeZoneById("Mountain Standard Time"));
                case "PST":
                    return TimeZoneInfo.ConvertTimeFromUtc(dt, TimeZoneInfo.FindSystemTimeZoneById("Mountain Standard Time"));
                case "AKST":
                    return TimeZoneInfo.ConvertTimeFromUtc(dt, TimeZoneInfo.FindSystemTimeZoneById("Alaskan Standard Time"));
                case "HAST":
                    return TimeZoneInfo.ConvertTimeFromUtc(dt, TimeZoneInfo.FindSystemTimeZoneById("Hawaiian Standard Time"));
                default:
                    return dt;
            }
        }

        public static DateTime ConvertFromAccountTimeZone(this Account account, DateTime dt)
        {
            switch (account.TimeZoneAbbr)
            {
                case "EST":
                    return TimeZoneInfo.ConvertTimeToUtc(dt, TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time"));
                case "CST":
                    return TimeZoneInfo.ConvertTimeToUtc(dt, TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time"));
                case "MST":
                    return TimeZoneInfo.ConvertTimeToUtc(dt, TimeZoneInfo.FindSystemTimeZoneById("Mountain Standard Time"));
                case "PST":
                    return TimeZoneInfo.ConvertTimeToUtc(dt, TimeZoneInfo.FindSystemTimeZoneById("Mountain Standard Time"));
                case "AKST":
                    return TimeZoneInfo.ConvertTimeToUtc(dt, TimeZoneInfo.FindSystemTimeZoneById("Alaskan Standard Time"));
                case "HAST":
                    return TimeZoneInfo.ConvertTimeToUtc(dt, TimeZoneInfo.FindSystemTimeZoneById("Hawaiian Standard Time"));
                default:
                    return dt;
            }
        }

        public static DateTime NowInAccountTimeZone(this Account account)
        {
            return account.ConvertToAccountTimeZone(DateTime.UtcNow);
        }

        public static AccountProperty GetAccountProperty(this Account account, AccountPropertyType propertyType, DatabaseConnection dc = null)
        {
            return AccountProperty.LoadByAccountIdByAccountPropertyTypeId(account.ID.Value, propertyType, dc: dc);
        }

        #endregion

        #region Attendance

        public static List<Attendance> GetCompletedAttendance(this Account account, DatabaseConnection dc = null)
        {
            return Attendance.LoadByAccountIdByIsCompleted(account.ID.Value, true, dc: dc);
        }

        public static int GetAttendancePresentCount(this Account account, DatabaseConnection dc = null)
        {
            return AttendanceRegistrant.GetPresentCountByAccountId(account.ID.Value, dc: dc);
        }

        public static int GetAttendancePossibleCount(this Account account, DatabaseConnection dc = null)
        {
            return AttendanceRegistrant.GetPossibleCountByAccountId(account.ID.Value, dc: dc);
        }

        public static List<Attendance> GetAttendanceByDate(this Account account, DateTime date, DatabaseConnection dc = null)
        {
            return Attendance.LoadByAccountIdByDate(account.ID.Value, date, dc: dc);
        }

        #endregion

        #region Registrations

        public static List<Registration> GetRegistrations(this Account account, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return Registration.LoadByAccountId(account.ID.Value, msgs, dc)
                .Where(registration => Registration.ValidCompletionStatus.Contains((int)registration.CompletionStatusId))
                .ToList();
        }

        public static List<Registration> GetCurrentRegistrations(this Account account, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return Registration.LoadCurrentByAccountId(account.ID.Value, msgs, dc)
                .Where(registration => Registration.ValidCompletionStatus.Contains((int)registration.CompletionStatusId))
                .ToList();
        }

        public static List<Registration> GetRegistrations(this Account account, int start, int length, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return Registration.Load(
                new Hashtable() {
                    { "AccountId", account.ID.Value },
                    { "CompletionStatusId", Registration.ValidCompletionStatus }
                }, 
                start, 
                length, 
                msgs, 
                dc);
        }

        public static int GetRegistrationCount(this Account account, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return Registration.GetCount(
                new Hashtable() {
                    { "AccountId", account.ID.Value },
                    { "CompletionStatusId", RegistrationCompletionStatus.Completed }
                }, 
                msgs: msgs,
                dc: dc);
        }

        public static List<Registration> SearchRegistrations(this Account account, string searchText, int? questionId, List<int> participationObjectIds, ParticipationObjectType participationObjectType, RegistrationStatus regStatus, RegistrationAssignmentType rat, int? start, int? length, DatabaseConnection dc = null)
        {
            return Registration.Search(searchText, account.ID.Value, questionId, participationObjectIds, participationObjectType, regStatus, rat, start, length, dc: dc);
        }

        public static int SearchRegistrationCount(this Account account, string searchText, int? questionId, List<int> participationObjectIds, ParticipationObjectType participationObjectType, RegistrationStatus regStatus, RegistrationAssignmentType rat, DatabaseConnection dc = null)
        {
            return Registration.SearchCount(searchText, account.ID.Value, questionId, participationObjectIds, participationObjectType, regStatus, rat, dc: dc);
        }

        public static void CreateNewRegistrantAndRegistration(this Account account, ParticipationObject participationObject, List<(FormQuestion Question, string Answer)> questionsAndAnswers, RegistrationPeriod period, WebUser loggedInUser, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            Registrant student = null;

            Database.ExecuteTransaction
            (
                action: (SystemMessages amsgs, DatabaseConnection adc) =>
                {
                    var location = account.GetDefaultLocation(amsgs, adc);
                    var groupId = new int?();
                    var aagId = new int?();

                    if (participationObject.IsAccountActivityGroup())
                    {
                        groupId = participationObject.GetAccountActivityGroup().ActivityGroupId;
                        aagId = participationObject.GetAccountActivityGroupId();
                    }
                    else if (participationObject.IsActivityGroup())
                    {
                        groupId = participationObject.GetActivityGroupId();
                    }
                    else if (participationObject.IsActivity())
                    {
                        groupId = participationObject.GetActivity().ActivityGroupId;
                        aagId = participationObject.GetActivity().AccountActivityGroupId;
                    }


                    // create student registrant record associated to this account
                    student = account.CreateRegistrant(
                        associatedUser: null,
                        registrantType: RegistrantType.Individual,
                        isMaster: false,
                        isParentOrGuardian: false,
                        allowTextMessages: false,
                        schoolLocationId: location?.ID,
                        loggedInUser: loggedInUser,
                        msgs: amsgs,
                        dc: adc);

                    // create a registration
                    var registration = student.AddRegistration(
                        ownerAccountId: account.ID.Value,
                        activityGroupId: groupId,
                        accountActivityGroupId: aagId,
                        periodId: period?.ID,
                        completionStatus: RegistrationCompletionStatus.RequiresParentSignature,
                        loggedInUserId: loggedInUser.ID.Value,
                        msgs: amsgs,
                        dc: adc);

                    // add answers
                    questionsAndAnswers.ForEach(questionAndAnswer =>
                    {
                        student.AddAnswer(
                            answerText: questionAndAnswer.Answer.IfEmpty("").Trim(),
                            question: questionAndAnswer.Question,
                            registrationId: registration.ID,
                            loggedInUserId: loggedInUser.ID.Value,
                            msgs: amsgs,
                            dc: adc);
                    });

                    // manages seats and create registrantactivity record
                    Registration.ProcessRegistrationAssignment(
                        registration: registration,
                        participationObject: participationObject,
                        registrant: student,
                        seatsRequested: 1,
                        activityGroupId: groupId,
                        periodId: period?.ID,
                        loggedInUser: loggedInUser,
                        msgs: amsgs,
                        dc: adc);

                },
                onSuccess: (SystemMessages smsgs) =>
                {
                    msgs.AddRange(smsgs);
                    msgs.Add("Your new registration has been added", MessageType.Success);
                },
                onFailure: (SystemMessages fmsgs) =>
                {
                    msgs.AddRange(fmsgs);
                },
                onException: (SystemMessages emsgs) =>
                {
                    Helpers.Utilities.SystemErrorLog(emsgs);
                },
                dc: dc
            );
        }

        #endregion

        #region NewAccounSetup

        /// <summary>
        /// Copy default form from main After School HQ Account
        /// </summary>
        /// <param name="account"></param>
        /// <param name="loggedInUser"></param>
        /// <param name="msgs"></param>
        /// <param name="dc"></param>
        /// <returns></returns>
        public static Form CreateDefaultForm(this Account formOwner, WebUser loggedInUser, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var form = FormService.GetOrCreateAfterSchoolAccountTemplateForm(msgs, dc);

            if (form.AccountId != formOwner.ID)
                form = form.Copy(loggedInUser.ID.Value, formOwner.ID.Value, "Standard Reg Form", msgs, dc);

            msgs.AddRange(formOwner.SetDefaultForm(form, loggedInUser, dc));

            return form;
        }

        public static SystemMessages SetDefaultForm(this Account account, Form form, WebUser loggedInUser, DatabaseConnection dc = null)
        {
            account.DefaultFormId = form.ID;
            account.LastModifiedBy = loggedInUser?.ID.IfEmpty(0);
            account.LastModifiedDate = DateTime.UtcNow;
            return account.Save(dc);
        }

        public static Form GetOrCreateDefaultForm(this Account account, WebUser loggedInUser, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return account.GetDefaultForm(msgs, dc) ?? CreateDefaultForm(account, loggedInUser, msgs, dc);
        }

        public static Form GetDefaultForm(this Account account, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return account.DefaultFormId.HasValue ? Form.Load(account.DefaultFormId.Value, msgs, dc) : null;
        }

        public static ActivityForm GetDefaultActivityForm(this Account account, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return new ActivityForm()
            {
                FormId = account.GetDefaultForm(msgs, dc).ID,
                IsActive = true,
                AccountId = account.ID
            };
        }

        public static Location GetDefaultLocation(this Account account, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return account.DefaultLocationId.HasValue ? Location.Load(account.DefaultLocationId.Value, msgs, dc) : null;
        }

        public static string GenerateUniqueName(string name, int? accountId, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            //unique name
            var reg = new System.Text.RegularExpressions.Regex(BasicLibrary.Utilities.UrlFriendlyRegEx, System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.Singleline);

            var origUniqueName = reg.Replace(name.GetMaxWords(25), "");
            var uniqueName = origUniqueName;
            var duplicate = Account.LoadByUniqueName(uniqueName, msgs, dc);
            var count = 1;

            while (duplicate.IsNotEmpty() && duplicate.ID != accountId)
            {
                uniqueName = origUniqueName + count.ToString();
                count++;
                duplicate = Account.LoadByUniqueName(uniqueName, msgs, dc);
            }
            return uniqueName;
        }

        private enum SignUpType
        {
            NotSet = 0,
            NewAccountSignUp,
            TrialSignUp,
            ConvertingFromTrial,
            ChangePackage,
            ChangePackageDuringTrial,
            ChangeBillingPeriodType
        }

        private static SignUpType GetSignUpType(this Account account, Package p, AccountPackage activePackage, BillingPeriodType billingPeriodType, ref int trialDayCount, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var signupType = SignUpType.NotSet;

            // has payment info
            if (account.StripeDefaultCardId.IsNotEmpty())
            {
                // paying as soon as they sign up
                if (activePackage.IsEmpty())
                {
                    signupType = SignUpType.NewAccountSignUp;
                }
                // changing packages but have already paid, force those upgrading from trials to upgrade on current plan
                else if (activePackage.PackageId != p.ID)
                {
                    signupType = SignUpType.ChangePackage;
                }
                // if same package but the billing period is changing
                else if (activePackage.PackageId == p.ID && activePackage.BillingPeriodTypeId != billingPeriodType)
                {
                    signupType = SignUpType.ChangeBillingPeriodType;
                }
                // simply converting from the same package from trial
                else if (activePackage.TrialEndDate.HasValue)
                {
                    signupType = SignUpType.ConvertingFromTrial;
                }
            }
            else if (p.IsFree(billingPeriodType) || (activePackage.IsNotEmpty() && activePackage.IsFree))
            {
                // new sign up
                if (activePackage.IsEmpty())
                {
                    signupType = SignUpType.NewAccountSignUp;
                }
                // could be downgrading to a free account
                else if (activePackage.PackageId != p.ID)
                {
                    signupType = SignUpType.ChangePackage;
                }
            }
            else
            {
                if (activePackage.IsEmpty())
                {
                    trialDayCount = (p.HasFeature(FeatureType.TrialDays, msgs, dc) ? p.GetFeature(FeatureType.TrialDays, msgs, dc).GetMaxValue() : 0).ToInteger();
                }
                else if (activePackage.HasFeature(FeatureType.TrialDays, msgs, dc))
                {
                    trialDayCount = account.GetTrialDaysRemaining(activePackage, msgs, dc);
                }

                if (trialDayCount > 0)
                {
                    // totally new
                    if (activePackage.IsEmpty())
                    {
                        signupType = SignUpType.TrialSignUp;
                    }
                    // still during the trial but changing packages with no new payment info
                    else
                    {
                        signupType = SignUpType.ChangePackageDuringTrial;
                    }
                }
            }

            return signupType;
        }

        public static AccountPackage AddPackage(this Account account, int packageId, BillingPeriodType billingPeriodType, WebUser loggedInUser, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            Package package = null;
            AccountPackage currentPackage = null;
            var newap = new AccountPackage();

            Database.ExecuteTransaction(
                action: (SystemMessages amsgs, BasicLibrary.DataAccess.DatabaseConnection adc) =>
                {
                    package = Package.Load(packageId, amsgs, adc);
                    currentPackage = account.GetActiveAccountPackage(amsgs, adc);

                    var trialDayCount = 0;

                    // is user:
                    //      paying for their Trial account
                    //      upgrading to a different package
                    //      paying for a new package with no trial account

                    var signupType = account.GetSignUpType(package, currentPackage, billingPeriodType, ref trialDayCount, amsgs, adc);

                    if (signupType != SignUpType.NotSet)
                    {
                        if (signupType == SignUpType.ChangePackage ||
                            signupType == SignUpType.ChangeBillingPeriodType ||
                            signupType == SignUpType.ChangePackageDuringTrial)
                        {
                            //cancel existing package
                            amsgs.AddRange(currentPackage.CancelPackage(adc));
                        }

                        if (msgs.HasNoErrors())
                        {
                            if (signupType != SignUpType.ConvertingFromTrial)
                            {
                                var newStripePlanId = package.GetStripePlanId(billingPeriodType);

                                //create new package
                                newap.AccountId = account.ID;
                                newap.PackageId = packageId;
                                newap.BillingPeriodTypeId = billingPeriodType;
                                newap.CreateDate = DateTime.UtcNow;
                                newap.CreatedBy = loggedInUser.ID;
                                newap.LastModifiedDate = DateTime.UtcNow;
                                newap.LastModifiedBy = loggedInUser.ID;
                                newap.IsActive = true;
                                newap.Price = billingPeriodType == BillingPeriodType.Month ? package.Price : package.PricePerYear;
                                newap.StripeSubscriptionId = currentPackage?.StripeSubscriptionId ?? null;

                                // set trial end date if there are still trial days
                                if (trialDayCount > 0)
                                {
                                    newap.TrialStartDate = DateTime.UtcNow;
                                    newap.TrialEndDate = DateTime.UtcNow.AddDays(trialDayCount);
                                }
                                else
                                {
                                    newap.StartDate = DateTime.UtcNow;
                                }

                                amsgs.AddRange(newap.SaveWithStripe(account.StripeCustomerId, false, newStripePlanId, adc));
                            }
                            // if it has a trial date end in the future, then end it now if they are converting from trial
                            else if (currentPackage.TrialEndDate.HasValue && currentPackage.TrialEndDate.Value >= DateTime.Now)
                            {
                                currentPackage.TrialEndDate = DateTime.UtcNow;
                                currentPackage.LastModifiedDate = DateTime.UtcNow;
                                currentPackage.LastModifiedBy = loggedInUser.ID;

                                amsgs.AddRange(currentPackage.SaveWithStripe(account.StripeCustomerId, true, null, adc));

                                newap = currentPackage;
                            }

                        }

                        if (amsgs.HasNoErrors() && newap.ID.HasValue)
                        {
                            amsgs.AddRange(package.CopyFeaturesToAccountPackage(newap, loggedInUser.ID.Value, adc));
                        }

                        // make sure the account package is set to the real trial days passed in
                        if (amsgs.HasNoErrors())
                        {
                            var apf = newap.GetFeature(FeatureType.TrialDays, dc: adc);
                            if (apf.IsNotEmpty())
                            {
                                apf.Max = trialDayCount;
                                amsgs.AddRange(apf.Save(adc));
                            }
                        }
                    }
                    else
                    {
                        amsgs.Add($"Credit Card is required to sign up for {package.Name} Package", MessageType.UserError);
                    }
                },
                onSuccess: (SystemMessages smsgs) =>
                {
                    msgs.AddRange(smsgs);
                    // msgs.Add($"{package.Name} Package has been activated", MessageType.Success);
                },
                onFailure: (SystemMessages fmsgs) =>
                {
                    msgs.AddRange(fmsgs);
                },
                onException: (SystemMessages emsgs) =>
                {
                    Helpers.Utilities.SystemErrorLog(emsgs);
                },
                dc: dc
            );

            return newap;
        }

        public static int GetTrialDaysRemaining(this Account account, AccountPackage currentPackage, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var daysRemaining = 0;
            var firstAccountPackage = account.GetAccountPackages(msgs, dc).OrderBy(x => x.StartDate.Value).First();
            var daysSinceSignUp = (DateTime.UtcNow - firstAccountPackage.StartDate.Value).Days;
            if (daysSinceSignUp < currentPackage.GetFeature(FeatureType.TrialDays, msgs, dc).GetMaxValue() + 1) // they get the full last day
            {
                daysRemaining = (currentPackage.GetFeature(FeatureType.TrialDays, msgs, dc).GetMaxValue() - daysSinceSignUp).ToInteger();
            }
            return daysRemaining;
        }

        public static Activity GetDefaultNewActivity(this Account account)
        {
            var activity = new Activity
            {
                ID = 0,
                WaitingListSeats = 0,
                SeatsAvailable = Math.Min(20, account.GetActiveAccountPackage().GetFeature(FeatureType.Participants).Max.Value.ToInteger()),
                CostPerRegistrant = 0,
                AccountId = account.ID,
                RegistrationModeId = RegistrationMode.Register,
                AllowToPayLater = true,               
            };
            return activity;
        }

        public static RegistrationPeriod GetDefaultNewRegistrationPeriod(this Account account)
        {
            return new RegistrationPeriod()
            {
                ID = 0,
                StartDateTime = $"{account.ConvertToAccountTimeZone(DateTime.UtcNow).ToShortDateString()} {"8:00 AM"}".ToDateTime(),
                EndDateTime = $"{account.ConvertToAccountTimeZone(DateTime.UtcNow).AddWeeks(1).ToShortDateString()} {"12:00 AM"}".ToDateTime()
            };
        }

        public static Account CreateNewGhostAccount(string orgName, AccountType accountType, WebUser loggedInUser, bool isAfterSchool, SystemMessages msgs, DatabaseConnection dc)
        {
            return CreateNew(orgName, null, null, false, accountType, null, null, null, loggedInUser, isAfterSchool, false, msgs, dc);
        }

        public static Account CreateNew(string orgName, string email, string phone, bool isActive, AccountType accountType, WebUser accountOwner, int? packageId, BillingPeriodType? packageBillingPeriod, WebUser loggedInUser, bool isAfterSchool, bool createWithStripe, SystemMessages msgs, DatabaseConnection dc)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();
            var uniqueName = AccountService.GenerateUniqueName(orgName, null, dc: dc);

            var account = new Account
            {
                Name = orgName,
                IsAdminAccount = false,
                IsActive = isActive,
                Email = email,
                Phone = phone,
                OwnerId = accountOwner?.ID,
                CreateDate = DateTime.UtcNow,
                LastModifiedDate = DateTime.UtcNow,
                CreatedBy = loggedInUser.ID,
                LastModifiedBy = loggedInUser.ID,
                IsAfterSchool = isAfterSchool,
                AccountTypeId = accountType,
                UniqueName = uniqueName
            };

            if (createWithStripe)
                msgs.AddRange(account.SaveWithStripe(dc));
            else
                msgs.AddRange(account.Save(dc));

            if (msgs.HasNoErrors())
            {
                // need to add a package
                if (packageId.HasValue && packageBillingPeriod.HasValue)
                    account.AddPackage(packageId.Value, packageBillingPeriod.Value, loggedInUser, msgs, dc);

                // create stripe custom account to allow account to receive payments
                if (accountType != AccountType.AfterSchool && accountType != AccountType.Registrant && createWithStripe)
                    account.CreateStripeCustomAccount(msgs, dc);

                //create default form for account to use for registration
                account.CreateDefaultForm(loggedInUser, msgs, dc);
            }

            return account;
        }

        #endregion

        #region AccountRelationships

        public static List<Account> GetVisitorAccounts(this Account hostAccount, AccountType accountType, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return AccountRelationship.LoadByHostAccountIdByAccountType(hostAccount.ID.Value, accountType, msgs, dc)
                .Select(accountRelationship => accountRelationship.GetVisitorAccount(msgs, dc))
                .ToList();
        }

        public static List<Account> GetVisitorAccounts(this Account hostAccount, AccountType accountType, int? start, int? length, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return AccountRelationship.LoadByHostAccountIdByAccountType(hostAccount.ID.Value, accountType, msgs, dc)
                .Select(accountRelationship => accountRelationship.GetVisitorAccount(msgs, dc))
                .ToList();
        }

        public static int GetVisitorAccountsCount(this Account hostAccount, AccountType accountType, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return AccountRelationship.GetCount(new Hashtable() { { "HostAccountId", hostAccount.ID.Value }, { "AccountAccountTypeId", (int)accountType } }, msgs, dc);
        }

        public static List<Account> GetHostAccounts(this Account account, DatabaseConnection dc = null)
        {
            return AccountRelationship.LoadByVisitorAccountId(account.ID.Value, dc: dc)
                .Select(x => x.GetHostAccount(dc: dc))
                .ToList();
        }

        //Gets the accounts that are requesting the host's approval
        public static List<Account> GetRequestedVisitorAccounts(this Account hostAccount, AccountType? accountType, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return AccountRelationship.LoadByRequested(hostAccount.ID.Value, accountType, msgs, dc)
                .Select(accountRelationship => accountRelationship.GetVisitorAccount(msgs, dc))
                .ToList();
        }

        public static AccountRelationship GetRequestForAccount(this Account visitorAccount, Account hostAccount, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return AccountRelationship.LoadByHostAccountIdByVisitorAccountId(hostAccount.ID.Value, visitorAccount.ID.Value, msgs, dc);
        }

        //Gets the accounts that are requesting the host's approval
        public static int GetRequestedVisitorAccountsCount(this Account hostAccount, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return AccountRelationship.GetCount(
                new Hashtable()
                {
                    { "HostAccountId", hostAccount.ID.Value },
                    { "ApprovalStatusId", (int)ApprovalStatus.Requested }
                },
                msgs,
                dc);
        }

        //Gets the accounts that the host has approved
        public static List<Account> GetApprovedVisitorAccounts(this Account hostAccount, AccountType accountType, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return AccountRelationship.LoadByApproved(hostAccount.ID.Value, accountType, msgs, dc)
                .Select(accountRelationship => accountRelationship.GetVisitorAccount(msgs, dc))
                .ToList();
        }

        public static List<Account> GetApprovedVisitorAccounts(this Account hostAccount, List<AccountType> accountTypes, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return AccountRelationship.LoadByApproved(hostAccount.ID.Value, accountTypes, msgs, dc)
                .Select(accountRelationship => accountRelationship.GetVisitorAccount(msgs, dc))
                .ToList();
        }

        public static List<Account> GetApprovedVisitorAccounts(this Account hostAccount, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return AccountRelationship.LoadByApproved(hostAccount.ID.Value, msgs, dc)
                .Select(accountRelationship => accountRelationship.GetVisitorAccount(msgs, dc))
                .ToList();
        }

        //Gets the accounts that the host has Denied
        public static List<Account> GetDeniedVisitorAccounts(this Account hostAccount, AccountType accountType, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return AccountRelationship.LoadByDenied(hostAccount.ID.Value, accountType, msgs, dc)
                .Select(accountRelationship => accountRelationship.GetVisitorAccount(msgs, dc))
                .ToList();
        }

        public static List<Location> GetVisitorLocations(this Account visitorAccount, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return AccountRelationshipLocation.LoadByVisitorAccountId(visitorAccount.ID.Value, msgs, dc)
                .Select(x => x.GetLocation(msgs, dc))
                .ToList();
        }

        public static List<Location> GetHostedLocations(this Account hostAccount, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return Location.LoadByHostAccountId(hostAccount.ID.Value, msgs, dc);
        }

        public static List<Location> GetAllAvailableLocations(this Account account, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var locations = account.GetHostedLocations(msgs, dc);
            locations.AddRange(account.GetVisitorLocations(msgs, dc));
            return locations;
        }

        /// <summary>
        /// creating an account relationship in which the visitor is requesting approval saving it to database
        /// and returning the system messages
        /// </summary>
        /// <param name="visitorAccount"></param>
        /// <param name="hostAccount"></param>
        /// <param name="hostPermissionType"></param>
        /// <param name="loggedInUser"></param>
        /// <param name="dc"></param>
        /// <returns></returns>
        public static AccountRelationship RequestAccessToAccount(this Account visitorAccount, Account hostAccount, HostPermissionType hostPermissionType, WebUser loggedInUser, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();
            AccountRelationship relationship = null;

            // Makes sure that the visitor has not been previously denied by the host
            if (!GetDeniedVisitorAccounts(hostAccount, visitorAccount.AccountTypeId, dc: dc).Contains(visitorAccount))
            {
                relationship = visitorAccount.AddHostAccount(
                    hostAccount,
                    hostPermissionType,
                    ApprovalStatus.Requested,
                    loggedInUser,
                    msgs,
                    dc);

                //TODO Notify Host account of request
                if (msgs.HasNoErrors() && hostAccount.IsActive)
                    msgs.AddRange(Helpers.EmailHelper.SendAccessRequestToHost(visitorAccount, hostAccount));
            }
            else
            {
                msgs.Add("This account has already previously been denied", MessageType.UserError);
            }

            return relationship;
        }

        /// <summary>
        /// Approves access to the host account 
        /// </summary>
        /// <param name="hostAccount"></param>
        /// <param name="visitorAccount"></param>
        /// <param name="loggedInUser"></param>
        /// <param name="dc"></param>
        /// <returns></returns>
        public static AccountRelationship ApproveAccessToAccount(this Account hostAccount, Account visitorAccount, WebUser loggedInUser, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return ApproveAccessToAccount(hostAccount, visitorAccount, HostPermissionType.ApproveOnly, loggedInUser, msgs, dc);
        }

        public static AccountRelationship ApproveAccessToAccount(this Account hostAccount, Account visitorAccount, HostPermissionType hostPermissionType, WebUser loggedInUser, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var relationship = AccountRelationship.LoadByHostAccountIdByVisitorAccountId(hostAccount.ID.Value, visitorAccount.ID.Value, msgs, dc);

            if (relationship.IsNotEmpty())
                msgs.AddRange(AccountRelationshipService.UpdateApprovalStatus(hostAccount, visitorAccount, ApprovalStatus.Approved, loggedInUser, dc));
            else
                relationship = AccountRelationshipService.CreateNew(hostAccount, visitorAccount, ApprovalStatus.Approved, hostPermissionType, loggedInUser, msgs, dc);

            if (msgs.HasNoErrors())
                msgs.AddRange(Helpers.EmailHelper.SendApprovalNotificationToVisitor(visitorAccount, hostAccount));

            return relationship;
        }

        /// <summary>
        /// Denies access to the host account 
        /// </summary>
        /// <param name="hostAccount"></param>
        /// <param name="visitorAccount"></param>
        /// <param name="loggedInUser"></param>
        /// <param name="dc"></param>
        /// <returns></returns>
        public static SystemMessages DenyAccessToAccount(this Account hostAccount, Account visitorAccount, WebUser loggedInUser, DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();

            msgs.AddRange(AccountRelationshipService.UpdateApprovalStatus(hostAccount, visitorAccount, ApprovalStatus.Denied, loggedInUser, dc));

            // if (msgs.HasNoErrors())
            // msgs.AddRange(EmailHelper.SendDenialNotificationToVisitor(visitorAccount, hostAccount));

            return msgs;
        }

        public static SystemMessages AddVisitorAccount(this Account hostAccount, Account visitorAccount, HostPermissionType hostPermissionType, ApprovalStatus approvalStatus, WebUser loggedInUser, DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();

            AccountRelationshipService.CreateNew(
                    hostAccount,
                    visitorAccount,
                    approvalStatus,
                    hostPermissionType,
                    loggedInUser,
                    msgs,
                    dc);

            return msgs;
        }

        /// <summary>
        /// Should only be used in situations where the visiting account doesn't need approval status. Otherwise use RequestAccessToAccount
        /// </summary>
        /// <param name="visitorAccount"></param>
        /// <param name="hostAccount"></param>
        /// <param name="hostPermissionType"></param>
        /// <param name="approvalStatus"></param>
        /// <param name="loggedInUser"></param>
        /// <param name="dc"></param>
        /// <returns></returns>
        private static AccountRelationship AddHostAccount(this Account visitorAccount, Account hostAccount, HostPermissionType hostPermissionType, ApprovalStatus approvalStatus, WebUser loggedInUser, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return AccountRelationshipService.CreateNew(
                    hostAccount,
                    visitorAccount,
                    approvalStatus,
                    hostPermissionType,
                    loggedInUser,
                    msgs,
                    dc);
        }

        #endregion

        #region ManageAfterSchool

        public static SystemMessages CreateAfterSchoolAccount(string adminEmail, string adminPassword, string adminPhone, string accountName, DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();

            adminEmail = adminEmail.IfBlank("darye@reborncode.com");
            adminPassword = adminPassword.IfBlank("aDm1n1906");            
            accountName = accountName.IfBlank("AfterSchool Admin");
            adminPhone = adminPhone.IfBlank("317-316-0596");

            if (WebUser.LoadByEmail(adminEmail, msgs, dc) != null)
                throw new Exception("User with the same email address already exists.");

            var user = WebUserService.CreateNew(
                firstName: "Admin", 
                lastName: "User", 
                email: adminEmail, 
                phone: adminPhone, 
                password: adminPassword, 
                isActive: true, 
                isRegistered: true, 
                isVerified: true, 
                allowTextMessages: true,
                loggedInUser: null, 
                msgs: msgs, 
                dc: dc);

            var account = user.AddNewAccount(
                orgName: accountName,
                phone: null,
                isActive: true,
                accountType: AccountType.AfterSchool,
                packageId: null,
                packageBillingPeriod: null,
                isAfterSchool: true,
                loggedInUser: user, 
                msgs: msgs, 
                dc: dc);

            return msgs;
        }

        public static SystemMessages CreateSampleAccount(AccountType accountType, int packageId, DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();

            var email = "sample@afterschoolhq.com";
            var phone = "(317) 316-0596";
            var firstName = "Sample";
            var lastName = "User";
            var password = "the best sample account ever";

            if (WebUser.LoadByEmail(email, msgs, dc) != null)
                throw new Exception("User with the same email address already exists.");

            var user = WebUserService.CreateNew(firstName, lastName, email, phone, password, true, true, true, true, null, msgs, dc);
            var account = user.AddNewAccount("Sample Account", "317-899-9189", true, accountType, packageId, BillingPeriodType.Month, true, user, msgs, dc);

            var defaultLocation = LocationService.CreateNewWithValidation(
                locationName: "The Sample Account Building",
                address1: "5254 Winthrop Avenue",
                address2: null,
                city: "Indianapolis",
                state: "IN",
                postalCode: "46220",
                country: "US",
                locationType: LocationType.School,
                hostAccountId: account.ID,
                visitorAccount: account,
                loggedInUser: null,
                msgs: msgs,
                dc: dc);

            msgs.AddRange(account.SetDefaultLocation(defaultLocation.ID.Value, 0, dc));

            return msgs;
        }

        public static SystemMessages SetDefaultLocation(this Account account, int locationId, int loggedInUserId, DatabaseConnection dc = null)
        {
            account.DefaultLocationId = locationId;
            account.LastModifiedBy = loggedInUserId;
            account.LastModifiedDate = DateTime.UtcNow;
            return account.Save(dc);
        }

        public static Account GetAfterSchoolAccount(SystemMessages msgs = null, DatabaseConnection dc = null)
        {            
            return Account.LoadByAccountTypeId(AccountType.AfterSchool, msgs, dc).FirstOrDefault();
        }

        #endregion

        #region ParentAccounts

        public static Registrant GetFirstParentOrGuardianRegistrant(this Account account, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return Registrant.LoadByAccountId(account.ID.Value, msgs, dc)
                .Where(registrant => registrant.IsParentOrGuardian)
                .OrderBy(registrant => registrant.ID)
                .FirstOrDefault();
        }

        public static Registrant GetSecondParentOrGuardianRegistrant(this Account account, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var registrants = Registrant.LoadByAccountId(account.ID.Value, msgs, dc)
                .Where(registrant => registrant.IsParentOrGuardian)
                .OrderBy(registrant => registrant.ID)
                .ToList();

            if (registrants.Count > 1)
                return registrants[1];

            return null;
        }

        public static List<Registrant> GetChildrenRegistrants(this Account account, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return Registrant.LoadByAccountId(account.ID.Value, msgs, dc)
                .Where(registrant => !registrant.IsParentOrGuardian)
                .OrderBy(registrant => registrant.DisplayName)
                .ToList();
        }

        /// <summary>
        /// Create basic registrant/student record for an account user, not attached to an account, so it's attached to the user record, data can be added later
        /// </summary>
        /// <param name="account"></param>
        /// <param name="associatedUser">Can be null, should only be linked if the logged in user is the same as the registrant</param>
        /// <param name="registrantType"></param>
        /// <param name="isMaster"></param>
        /// <param name="isParentOrGuardian"></param>
        /// <param name="loggedInUser"></param>
        /// <param name="msgs"></param>
        /// <param name="dc"></param>
        /// <returns></returns>
        public static Registrant CreateRegistrant(this Account account, WebUser associatedUser, RegistrantType registrantType, bool isMaster, bool isParentOrGuardian, bool allowTextMessages, int? schoolLocationId, WebUser loggedInUser, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            var registrant = new Registrant
            {
                RegistrantTypeId = registrantType,
                IsMaster = isMaster,
                IsContactOnly = isParentOrGuardian,
                IsParentOrGuardian = isParentOrGuardian,
                WebUserId = associatedUser?.ID,
                AccountId = account.ID,
                AllowTextMessages = allowTextMessages,
                SchoolLocationId = schoolLocationId,
                CreateDate = DateTime.UtcNow,
                LastModifiedDate = DateTime.UtcNow,
                CreatedBy = loggedInUser?.ID.IfEmpty(0),
                LastModifiedBy = loggedInUser?.ID.IfEmpty(0)
            };
            msgs.AddRange(registrant.Save(dc));

            return registrant;
        }

        #endregion

        #region Finance

        public static List<Transaction> GetBilledTransactions(this Account account, DateTime? startDate, DateTime? endDate, int? start, int? length, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return Transaction.LoadByBillerAccountId(account.ID.Value, startDate, endDate, start, length, msgs, dc);
        }

        public static List<Transaction> GetPaidTransactions(this Account account, DateTime? startDate, DateTime? endDate, int? start, int? length, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return Transaction.LoadByPayerAccountId(account.ID.Value, startDate, endDate, start, length, msgs, dc);
        }

        public static int GetBilledTransactionCount(this Account account, DateTime? startDate, DateTime? endDate, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return Transaction.GetCountByBillerAccountId(account.ID.Value, startDate, endDate, msgs, dc);
        }

        public static int GetPaidTransactionCount(this Account account, DateTime? startDate, DateTime? endDate, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return Transaction.GetCountByPayerAccountId(account.ID.Value, startDate, endDate, msgs, dc);
        }

        public static decimal CalculatePlatformFeeAmount(this Account toAccount, decimal totalAmountToCharge, SystemMessages msgs, DatabaseConnection dc = null)
        {
            var percentageFee = toAccount.GetAccountPackageFeature(FeatureType.PaymentsPercentageFee, msgs, dc).GetMaxValue();
            var flatFee = toAccount.GetAccountPackageFeature(FeatureType.PaymentsPercentageFlatFee, msgs, dc).GetMaxValue();

            return (totalAmountToCharge * percentageFee) + flatFee;
        }

        public static decimal GetPlatformPercentFee(this Account toAccount, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return toAccount.GetAccountPackageFeature(FeatureType.PaymentsPercentageFee, msgs, dc).GetMaxValue();
        }

        public static decimal GetPlatformFlatFee(this Account toAccount, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return toAccount.GetAccountPackageFeature(FeatureType.PaymentsPercentageFlatFee, msgs, dc).GetMaxValue();
        }

        internal static string GetStripeSourceFromToken(this Account fromAccount, string stripeToken, SystemMessages msgs = null)
        {
            var source = Billing.StripeHelper.CreateSource(stripeToken, msgs);
            msgs.AddRange(Billing.StripeHelper.AddSourceToCustomer(fromAccount.StripeCustomerId, source.Id, null));
            return source.Id;
        }

        internal static Stripe.StripeCharge CreateStripeChargeDuringCheckout(CheckoutObject checkoutObject, SystemMessages msgs, DatabaseConnection dc = null)
        {
            // if stripe token then don't send customer id
            string customerId = null;

            if (checkoutObject.SavePaymentInfo || checkoutObject.SourceId.IsNotEmpty())
                customerId = checkoutObject.PurchasingAccount.StripeCustomerId;

            var charge = StripeHelper.CreateCharge(
                fromCustomerId: customerId,
                destinationAccountId: checkoutObject.BillingAccount.StripeUserId,
                tokenOrSourceId: checkoutObject.SourceId.IfEmpty(checkoutObject.StripeToken),
                totalAmount: checkoutObject.TuitionFee,
                platformFee: checkoutObject.BillingAccount.CalculatePlatformFeeAmount(checkoutObject.TuitionFee, msgs, dc),
                description: $"{checkoutObject.ParticipationObject.Name} registration",
                statementDescriptor: $"AFTRSCHL/{checkoutObject.BillingAccount.Name}",
                msgs: msgs
                );

            return charge;
        }

        public static Stripe.StripeRefund ProcessRefund(string chargeId, decimal totalAmount, Account billingAccount, SystemMessages msgs, DatabaseConnection dc = null)
        {
            var refund = Billing.StripeHelper.CreateRefund(
                chargeId: chargeId,
                totalAmount: totalAmount,
                reason: Billing.StripeRefundReason.RequestedByCustomer.ToString().SplitByUpperCase().Replace(" ", "_").ToLower(),
                msgs: msgs
                );

            return refund;
        }

        internal static Purchase AddPurchase(CheckoutObject checkoutObject, WebUser loggedInUser, SystemMessages msgs, DatabaseConnection dc = null)
        {
            return PurchaseService.CreateNew(
                    checkoutObject: checkoutObject,
                    loggedInUser: loggedInUser,
                    msgs: msgs,
                    dc: dc);
        }

        internal static Transaction AddTransaction(CheckoutObject checkoutObject, TransactionType transactionType, WebUser loggedInUser, SystemMessages msgs, DatabaseConnection dc = null)
        {
            return TransactionService.CreateNew(
                    checkoutObject,
                    transactionType,
                    loggedInUser,
                    msgs,
                    dc);
        }

        internal static Transaction AddTransaction(Transaction orginalTransaction, string stripeRefundId, decimal amount, TransactionType transactionType, PaymentType paymentType, PaymentSourceType paymentSourceType, WebUser loggedInUser, SystemMessages msgs, DatabaseConnection dc = null)
        {
            return TransactionService.CreateNew(
                    orginalTransaction,
                    amount,
                    stripeRefundId,
                    transactionType,
                    paymentType,
                    paymentSourceType,
                    loggedInUser,
                    msgs,
                    dc);
        }

        internal static string CopyStripeSourceToAccount(this Account fromAccount, Account toAccount, string stripeToken, SystemMessages msgs)
        {
            // stripe stuff
            // create payment source if not already existent on the user
            var sourceId = "";
            var customer = Billing.StripeHelper.GetCustomer(fromAccount.StripeCustomerId, null, msgs);

            if (customer.DefaultSourceId.IsEmpty())
            {
                var source = Billing.StripeHelper.CreateSource(stripeToken, msgs);
                msgs.AddRange(Billing.StripeHelper.AddSourceToCustomer(fromAccount.StripeCustomerId, source.Id, null));
                sourceId = source.Id;
            }
            else
            {
                sourceId = customer.DefaultSourceId;
            }

            // what happens if the same registrant gets another payment plan?
            // Create a new customer? For now YES, it doesn't really matter I don't think

            // get shared source
            var copiedSource = Billing.StripeHelper.CreateSourceFromOriginal(customer.Id, sourceId, toAccount.StripeUserId, msgs);

            return copiedSource.Id;
        }

        private static void ProcessTuitionWithStripe(CheckoutObject checkoutObject, SystemMessages msgs, DatabaseConnection dc = null)
        {
            // if user wants to save payment info, do it before you charge the card
            // can't use a token twice
            if (msgs.HasNoErrors() && checkoutObject.SavePaymentInfo)
                checkoutObject.SourceId = checkoutObject.PurchasingAccount.GetStripeSourceFromToken(checkoutObject.StripeToken, msgs);

            // if everything goes right then bill the person, 
            // if any errors it'll just rollback the transaction anyway :)
            // don't process charge if the user is using a payment plan
            if (msgs.HasNoErrors())
            {
                var charge = AccountService.CreateStripeChargeDuringCheckout(checkoutObject, msgs, dc);
                checkoutObject.StripeChargeId = charge.Id;
            }
        }

        public static void ProcessSubscriptionPlanPayment(this Account billingAccount, CheckoutObject checkoutObject, SubscriptionPlan plan, RegistrantSubscriptionPlan registrantSubscriptionPlan, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            Database.ExecuteTransaction
            (
                action: (SystemMessages amsgs, DatabaseConnection adc) =>
                {                           
                    // manage transaction
                    AccountService.CreatePurchaseTransactionPayment(
                        checkoutObject,
                        null,
                        amsgs,
                        adc
                    );

                    // cancel stripe subscription at end of term
                    plan.CheckAndCancelPaymentPlan(registrantSubscriptionPlan, billingAccount, amsgs, adc);
                },
                onSuccess: (SystemMessages smsgs) =>
                {
                    msgs.AddRange(smsgs);
                },
                onFailure: (SystemMessages fmsgs) =>
                {
                    msgs.AddRange(fmsgs);
                },
                onException: (SystemMessages emsgs) =>
                {
                    AfterSchoolLibrary.Helpers.Utilities.SystemErrorLog(emsgs);
                },
                dc: dc
            );
        }

        public static void CreatePurchaseTransactionPayment(CheckoutObject checkoutObject, WebUser loggedInUser, SystemMessages msgs, DatabaseConnection dc = null)
        {
            // not the master registrant record
            var registrant = checkoutObject.Registration.GetRegistrant(msgs, dc);
            
            // need to check for existing purchase or create a new one
            checkoutObject.CreateOrUpdatePurchase(loggedInUser, msgs, dc);

            // charge successful so I need to create a transaction
            var transaction = AccountService.AddTransaction(
                checkoutObject,
                TransactionType.Charge,
                loggedInUser,
                msgs,
                dc);

            // TODO: this is problematic, but a little better now
            // you really don't know this is the amount paid, but it works for now
            var amountPaid = checkoutObject.TuitionFee;

            if (checkoutObject.OverrideAmount.HasValue)
                amountPaid = checkoutObject.OverrideAmount.Value;

            // need to track a payment for each activity/activitygroup
            checkoutObject.ParticipationObject.AddPayment(
                amount: amountPaid,
                registrant: registrant,
                transaction: transaction,
                registrantSubscriptionPlan: checkoutObject.RegistrantSubscriptionPlan,
                loggedInUser: loggedInUser,
                msgs: msgs,
                dc: dc);

            // if successful payment and reserved pending payment then it should be reserved
            // TODO: Needs review!!
            if (msgs.HasNoErrors())
            {
                var ra = RegistrantActivity.LoadByParticipationObjectByRegistrant(
                    checkoutObject.ParticipationObject.ObjectID.Value,
                    checkoutObject.ParticipationObject.ObjectType,
                    registrant.ID.Value,
                    msgs,
                    dc);

                if (ra.IsNotEmpty() && ra.AssignmentTypeId != RegistrationAssignmentType.Reserved)
                    msgs.AddRange(ra.ChangeSeatAssignment(RegistrationAssignmentType.Reserved, loggedInUser?.ID.IfEmpty(0), dc));
            }
        }

        public static Transaction CreateRefundTransactionPayment(Transaction originalTransaction, decimal amount, WebUser loggedInUser, SystemMessages msgs, DatabaseConnection dc = null)
        {
            var refund = AccountService.ProcessRefund(
                originalTransaction.StripeChargeOrRefundId,
                amount,
                originalTransaction.GetPayerAccount(),
                msgs
            );

            if (refund != null && refund.Status == Billing.StripeRefundStatus.Succeeded.ToString().ToLower())
            {
                var transaction = AccountService.AddTransaction(
                    originalTransaction,
                    refund.Id,
                    - amount,
                    TransactionType.Refund,
                    PaymentType.ACH,
                    PaymentSourceType.AfterSchoolPlatform,
                    loggedInUser,
                    msgs,
                    dc);
                var registrant = Registrant.Load(originalTransaction.GetPurchase().RegistrantId.Value);

                // need to track the refund
                originalTransaction.GetParticipationObjects().FirstOrDefault().AddPayment(
                    amount: - amount,
                    registrant: registrant,
                    transaction: transaction,
                    registrantSubscriptionPlan: null,
                    loggedInUser: loggedInUser,
                    msgs: msgs,
                    dc: dc);
                return transaction;
            }

            return null;
        }

        public static void ProcessTuitionFee(CheckoutObject checkoutObject, WebUser loggedInUser, SystemMessages msgs, DatabaseConnection dc = null)
        {
            if (checkoutObject.TuitionFee > 0)
              AccountService.ProcessTuitionWithStripe(checkoutObject, msgs, dc);

            // must wait until after charges happen to know what card id is charged for sure
            // add purchase
            if (msgs.HasNoErrors())
                AccountService.CreatePurchaseTransactionPayment(checkoutObject, loggedInUser, msgs, dc);
        }

        public static void ProcessRegistrationWithoutPayment(CheckoutObject checkoutObject, bool sendConfirmationEmail, WebUser loggedInUser, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            Database.ExecuteTransaction
            (
                action: (SystemMessages amsgs, DatabaseConnection adc) =>
                {
                    // mark as completed
                    amsgs.AddRange(checkoutObject.Registration.SetRegistrationToCompleted(loggedInUser, adc));

                    // TODO: Allow user to register for multiple activities in one session at once MAYBE
                    Registration.ProcessRegistrationAssignment(
                        checkoutObject: checkoutObject,
                        loggedInUser: loggedInUser,
                        msgs: amsgs,
                        dc: adc);                    
                },
                onSuccess: (SystemMessages smsgs) =>
                {
                    msgs.AddRange(smsgs);

                    if (sendConfirmationEmail)
                        Registration.SendConfirmation(checkoutObject, true);

                },
                onFailure: (SystemMessages amsgs) =>
                {
                    msgs.AddRange(amsgs);
                },
                onException: (SystemMessages emsgs) =>
                {
                    Helpers.Utilities.SystemErrorLog(emsgs);
                },
                dc: dc
            );
        }

        public static void ProcessRegistrationWithPayment(CheckoutObject checkoutObject, WebUser loggedInUser, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            Database.ExecuteTransaction
            (
                action: (SystemMessages amsgs, DatabaseConnection adc) =>
                {
                    AccountService.ProcessRegistrationWithoutPayment(
                        checkoutObject: checkoutObject,
                        sendConfirmationEmail: false,
                        loggedInUser: loggedInUser,
                        msgs: amsgs,
                        dc: adc);

                    // process payment by to account, includes tuition fee
                    if (!checkoutObject.ParticipationObject.IsTuitionFree && checkoutObject.PaymentPlan.IsEmpty())
                        AccountService.ProcessTuitionFee(checkoutObject, loggedInUser, amsgs, adc);

                    // Process Subscriptions separately, don't charge all at once for the tuition Fee

                    // TODO: Decide if master account registrant should be added to plans or the account copy
                    // get the registrant record that was created just for the account
                    var accountRegistrant = checkoutObject.Registration.GetRegistrant(amsgs, adc);

                    // if they have a subscription plan
                    // will also process tuition fees
                    if (checkoutObject.SubscriptionPlan.IsNotEmpty())
                        accountRegistrant.StartSubscriptionPlan(
                            checkoutObject: checkoutObject,
                            startDateTime: DateTime.UtcNow,
                            subscriptionPlanType: SubscriptionPlanType.Subscription,
                            loggedInUser: loggedInUser,
                            msgs: amsgs,
                            dc: adc);

                    // if they have a payment plan
                    if (checkoutObject.PaymentPlan.IsNotEmpty())
                    {
                        // create a stripe coupon if needed
                        var coupon = checkoutObject.Coupon;
                        if (coupon.IsNotEmpty() && coupon.DoesApplyToTuitionFee())
                        {
                            var discountValue = coupon.Amount;
                            if (coupon.AmountTypeId == CouponAmountType.FlatAmount) // calculate the new discount amount for the coupon
                                discountValue = (checkoutObject.ParticipationObject.Cost - coupon.Amount) / checkoutObject.PaymentPlan.TotalPeriods;

                            AccountService.CreateStripeCoupon(
                                coupon,
                                checkoutObject.BillingAccount.StripeUserId,
                                StripeCouponDuration.Repeating,
                                checkoutObject.PaymentPlan.TotalPeriods,
                                discountValue.Value,
                                amsgs,
                                dc
                            );
                        }

                        accountRegistrant.StartSubscriptionPlan(
                            checkoutObject: checkoutObject,
                            startDateTime: DateTime.UtcNow,
                            subscriptionPlanType: SubscriptionPlanType.PaymentPlan,
                            loggedInUser: loggedInUser,
                            msgs: amsgs,
                            dc: adc
                        );
                    }  
                },
                onSuccess: (SystemMessages smsgs) =>
                {
                    msgs.AddRange(smsgs);

                    // invalidate coupon
                    if (checkoutObject.Coupon.IsNotEmpty() && checkoutObject.Coupon.ID.IsNotEmpty())
                        checkoutObject.PurchasingAccount.MarkCouponAsUsed(checkoutObject.Coupon);

                    Registration.SendConfirmation(checkoutObject, true);
                },
                onFailure: (SystemMessages amsgs) =>
                {
                    msgs.AddRange(amsgs);
                },
                onException: (SystemMessages emsgs) =>
                {
                    Helpers.Utilities.SystemErrorLog(emsgs);
                },
                dc: dc
            );
            
        }

        public static StripeAccount GetOrCreateStripeAccount(this Account account, WebUser loggedInUser, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();
            var stripeAccount = account.GetStripeAccount(msgs, dc);

            if (stripeAccount.IsEmpty())
            {
                var location = account.GetDefaultLocation(msgs, dc);

                stripeAccount = new StripeAccount()
                {
                    AccountId = account.ID,
                    CreateDate = DateTime.UtcNow,
                    CreatedBy = loggedInUser?.ID.IfEmpty(0),
                    LastModifiedDate = DateTime.UtcNow,
                    LastModifiedBy = loggedInUser?.ID.IfEmpty(0),
                    LegalEntityAddress1 = location?.Address1,
                    LegalEntityAddress2 = location?.Address2,
                    LegalEntityCity = location?.City,
                    LegalEntityState = location?.State,
                    LegalEntityPostalCode = location?.PostalCode,
                    LegalEntityFirstName = loggedInUser.FirstName,
                    LegalEntityLastName = loggedInUser.LastName,
                    LegalEntityBusinessName = account.Name,
                    SupportEmail = account.Email,
                    SupportPhone = account.Phone
                };
                msgs.AddRange(stripeAccount.Save(dc));
            }

            return stripeAccount;
        }

        public static StripeAccount GetStripeAccount(this Account account, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return StripeAccount.LoadByAccountId(account.ID.Value, msgs, dc);
        }

        public static bool CanProcessPayments(this Account account, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (account.HasAccountPackageFeature(FeatureType.AcceptPayments, msgs, dc))
                return StripeAccount.CanProcessPayments(account.ID.Value, msgs, dc);

            return false;
        }

        #endregion

        #region Coupons

        public static AttachedCoupon AttachCoupon(this Account account, Coupon coupon, WebUser loggedInUser, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (account.AccountTypeId != AccountType.Registrant)
            {
                msgs.Add("Coupon can only be applied to Registrant accounts", MessageType.Error);
                return null;
            }

            return AttachedCouponService.CreateNew(account, coupon, loggedInUser, msgs, dc);
        }

        public static List<AttachedCoupon> GetUnusedAttachedCoupons(this Account account, DatabaseActionHelper actionHelper)
        {
            return AttachedCoupon.LoadByAccountIdByIsUsed(account.ID.Value, false, actionHelper?.Messages, actionHelper?.Connection);
        }

        public static List<AttachedCoupon> GetUsedAttachedCoupons(this Account account, DatabaseActionHelper actionHelper)
        {
            return AttachedCoupon.LoadByAccountIdByIsUsed(account.ID.Value, false, actionHelper?.Messages, actionHelper?.Connection);
        }

        public static List<AttachedCoupon> GetAttachedCoupons(this Account account, DatabaseActionHelper actionHelper = null)
        {
            return AttachedCoupon.LoadByAccountId(account.ID.Value, actionHelper?.Messages, actionHelper?.Connection);
        }

        public static Coupon CreateCoupon(this Account account, decimal amount, DateTime? expirationDate, CouponAmountType amountType, CouponApplyType applyType, StripeCouponDuration couponDuration, int? durationInMonths, WebUser loggedInUser, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var coupon = CouponService.CreateNew(
                            accountId: account.ID.Value,
                            amount: amount,
                            expirationDate: expirationDate,
                            couponAmountType: amountType,
                            couponApplyType: applyType,
                            maxUsesAll: 1,
                            maxUsesPerPerson: 1,
                            couponCode: CouponService.GenerateCouponCode(account.ID.Value, null),
                            label: $"Coupon - {account.NowInAccountTimeZone()}",
                            description: "",
                            loggedInUser: loggedInUser,
                            msgs: msgs,
                            dc: dc);
            if (coupon.DoesApplyToSubscriptionPlan())
                AccountService.CreateStripeCoupon(
                    coupon,
                    account.StripeUserId,
                    couponDuration,
                    durationInMonths,
                    coupon.Amount.Value,
                    msgs,
                    dc
                );

            return coupon;
        }

        public static Coupon ValidateCoupon(this Account account, ParticipationObject participationObject, string couponCode, SystemMessages msgs = null)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            var coupon = Coupon.LoadByAccountIdByCouponCode(
               participationObject.OwnerAccount.ID.Value,
               couponCode,
               msgs
           );

            if (coupon.IsEmpty())
                msgs.Add("Invalid coupon", MessageType.UserError);
            else
            {
                var attachedCoupon = AttachedCoupon.LoadByCouponIdByAccountIdIsUsed(
                    coupon.ID.Value,
                    account.ID.Value,
                    false
                );
                var purchaseableObjectCoupon = PurchaseableObjectCoupon.LoadByCouponByPurchaseableObject(
                    coupon.ID.Value,
                    participationObject.ObjectID.Value,
                    (PurchaseableObjectType)participationObject.ObjectType
                );

                if (attachedCoupon.IsEmpty() || purchaseableObjectCoupon.IsEmpty())
                    msgs.Add("You are not allowed to use this coupon", MessageType.UserError);
                else
                {
                    if (DateTime.UtcNow > coupon.ExpirationDate)
                        msgs.Add("Coupon is Expired", MessageType.UserError);

                    if (coupon.AppliesTo == CouponApplyType.SubscriptionPlan && participationObject.SubscriptionPlans.FirstOrDefault() == null)
                        msgs.Add("Coupon can only be applied to a subscription", MessageType.UserError);
                }
            }

            return coupon;
        }

        public static void MarkCouponAsUsed(this Account account, Coupon coupon, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var attachedCoupon = AttachedCoupon.LoadByCouponIdByAccountIdIsUsed(
                coupon.ID.Value,
                account.ID.Value,
                false
            );

            attachedCoupon.IsUsed = true;

            attachedCoupon.Save(dc);
        }

        #endregion

        #region Leagues

        public static List<ActivityGroup> GetLeagues(this Account account, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return ActivityGroup.LoadByAccountIdByActivityGroupTypeId(account.ID.Value, ActivityGroupType.League, msgs, dc);
        }

        public static List<ActivityGroup> GetLeagues(this Account account, bool IsArchived, int? start, int? length, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return ActivityGroup.LoadByAccountId(account.ID.Value, ActivityGroupType.League, start, length, msgs, dc);
        }

        public static int GetLeagueCount(this Account account, bool IsArchived, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return ActivityGroup.GetCount(
                new Hashtable() {
                    { "AccountId", account.ID.Value },
                    { "ActivityGroupTypeId", (int)ActivityGroupType.League },
                    { "IsArchived", IsArchived }
                },
                msgs: msgs,
                dc: dc);
        }

        public static List<AccountActivityGroup> GetExternalLeagues(this Account account, int? start, int? length, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return AccountActivityGroup.LoadByAccountId(account.ID.Value, start, length, msgs, dc);
        }

        /// <summary>
        /// return AccountActivityGroups where it is published 
        /// and is in a currently open registration period or upcoming registration period
        /// </summary>
        /// <param name="account"></param>
        /// <param name="msgs"></param>
        /// <param name="dc"></param>
        /// <returns></returns>
        public static List<AccountActivityGroup> GetPublishedExternalLeagues(this Account account, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var aags = AccountActivityGroup.LoadByAccountIdByIsPublished(account.ID.Value, true, msgs, dc);
            var periods = RegistrationPeriod.LoadByAccountActivityGroupIds(
                    aags.Select(x => x.ID.Value).ToList(),
                    msgs,
                    dc
                )
                .Where(period => period.IsOpen() || period.WillOpen() || period.WillCloseIn().Days > -14)
                .ToList();

            return aags.Where(aag => periods.Select(period => period.AccountActivityGroupId.Value).Contains(aag.ID.Value)).ToList();
        }

        public static int GetExternalLeagueCount(this Account account, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return AccountActivityGroup.GetCount(
                new Hashtable() {
                    { "AccountId", account.ID.Value }
                },
                msgs,
                dc);
        }

        public static int GetExternalLeagueInviteCount(this Account account, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return AccountActivityGroup.GetCount(
                new Hashtable() {
                    { "AccountId", account.ID.Value },
                    { "InviteStatusId", (int)InviteStatus.Invited }
                },
                msgs,
                dc);
        }

        public static SystemMessages SendSubmitTeamtoDistrictEmail(this Account vistorAccount, Account hostAccount, Activity activity, ActivityGroup activityGroup, bool isSubmitting, DatabaseConnection dc = null)
        {
            return new SystemMessages();
            // return EmailHelper.SendTeamSubmmittedToDistrictNotifitication(vistorAccount, hostAccount, activity, activityGroup, isSubmitting);
        }

        public static SystemMessages SendLeagueInvite(int invitedAccountId, int activityGroupId, int sendingUserId, string customMessage = null)
        {
            var msgs = new SystemMessages();
            var invitedAccount = Account.Load(invitedAccountId, msgs);
            var activityGroup = ActivityGroup.Load(activityGroupId, msgs);
            var sendingUser = WebUser.Load(sendingUserId, msgs);

            return Helpers.EmailHelper.SendLeagueAccountInvitedEmail(invitedAccount, activityGroup, sendingUser, customMessage);
        }

        public static AccountActivityGroup InviteSchoolToLeague(this Account account, ActivityGroup activityGroup, WebUser user, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
           return AccountActivityGroupService.CreateNew(account, activityGroup, InviteStatus.Invited, user);
        }

        public static List<AccountActivityGroup> InviteSchoolsToLeague(List<Account> schools, ActivityGroup activityGroup, string customMessage, WebUser loggedInuser, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var accountActivityGroups = new List<AccountActivityGroup>();

            Database.ExecuteTransaction
            (
                action: (SystemMessages amsgs, DatabaseConnection adc) =>
                {
                    schools.ForEach(school => accountActivityGroups.Add(school.InviteSchoolToLeague(activityGroup, loggedInuser, amsgs, adc)));
                },
                onSuccess: (SystemMessages smsgs) =>
                {
                    msgs.AddRange(smsgs);
                },
                onFailure: (SystemMessages fmsgs) =>
                {
                    msgs.AddRange(fmsgs);
                },
                onException: (SystemMessages emsgs) =>
                {
                    Helpers.Utilities.SystemErrorLog(emsgs);
                }
            );

            return accountActivityGroups;
        }

        public static decimal GetTotalAmountPaidTo(this Account account, DateTime? startDate = null, DateTime? endDate = null, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return Transaction.GetTotalAmountPaidToBillerAccount(account.ID.Value, startDate, endDate, msgs, dc);
        }

        public static decimal GetTotalFeesPaidBy(this Account account, DateTime? startDate, DateTime? endDate, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return Transaction.GetTotalFeesPaidByBillerAccount(account.ID.Value, startDate, endDate, msgs, dc);
        }

        public static decimal GetTotalAmountRefundedTo(this Account account, DateTime? startDate = null, DateTime? endDate = null, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return Transaction.GetTotalAmountPaidByPayerAccount(account.ID.Value, startDate, endDate, msgs, dc);
        }

        public static decimal GetSubscriptionPlatformFeePercent(this Account account, SubscriptionPlan plan, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var platformFeeAmount = account.CalculatePlatformFeeAmount(plan.CostPerPeriod.Value, msgs, dc);
            var stripeFeeAmount = plan.GetStripeFeeAmount();
            
            if (platformFeeAmount > stripeFeeAmount)
                return Math.Round((platformFeeAmount - stripeFeeAmount) / plan.CostPerPeriod.Value, 4);

            return 0;
        }
        #endregion
    }
}
