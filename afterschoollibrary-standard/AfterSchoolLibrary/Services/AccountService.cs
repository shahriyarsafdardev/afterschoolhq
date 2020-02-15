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
            return account.GetActiveAccountPackage(msgs, dc).HasFeature(featureType, msgs, dc);
        }

        public static WebUser GetAccountOwner(this Account account, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (account.HasAccountOwner)
                return WebUser.Load(account.OwnerId.Value, msgs, dc);

            return null;
        }

        public static List<WebUser> GetAccountOwnersAndAdmins(this Account account, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return AccountWebUser.LoadByAccountIdByIsActiveByIsDeleted(account.ID.Value, true, false, msgs, dc)
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

        #region ActivityMessages

        public static List<ActivityMessage> GetActivityMessages(this Account account, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return ActivityMessage.LoadByAccountId(account.ID.Value, msgs, dc);
        }

        public static List<ActivityMessage> GetActivityMessages(this Account account, int? start, int? length, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return ActivityMessage.LoadByAccountId(account.ID.Value, start, length, msgs, dc);
        }

        public static int GetActivityMessageCount(this Account account, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return ActivityMessage.GetCountByAccountId(account.ID.Value, msgs, dc);
        }

        public static ActivityMessage AddMessage(this Account account, string subject, string body, List<int> registrantIds, bool sendToStaff, int loggedInUserId, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();
            ActivityMessage am = null;

            Database.ExecuteTransaction(
                action: (SystemMessages amsgs, DatabaseConnection adc) =>
                {
                    am = new ActivityMessage
                    {
                        AccountId = account.ID,
                        Subject = subject.MaxChars(150),
                        Body = body,
                        SendToStaff = sendToStaff,
                        CreateDate = DateTime.UtcNow,
                        CreatedBy = loggedInUserId
                    };
                    amsgs.AddRange(am.Save(adc));

                    registrantIds.ForEach(registrantId => am.AddRegistrant(registrantId, loggedInUserId, amsgs, adc));
                },
                onSuccess: (SystemMessages smsgs) => msgs.AddRange(smsgs),
                onFailure: (SystemMessages fmsgs) => msgs.AddRange(fmsgs),
                onException: (SystemMessages emsgs) => Helpers.Utilities.SystemErrorLog(emsgs),
                dc: dc
            );

            return am;
        }

        #endregion

        #region Activities

        public static List<ParticipationObject> GetPublishedParticipationObjects(this Account account, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var participationObjects = new List<ParticipationObject>();

            participationObjects.AddRange(
                account.GetPublishedActivities(msgs, dc)
                .Select(activity => activity.GetParticipationObject(msgs, dc))
            );

            var hostedLocationIds = account.GetHostedLocations(msgs, dc)
                                            .Select(location => location.ID.Value)
                                            .ToList();

            account.GetApprovedVisitorAccounts(msgs, dc).ForEach(visitor =>
            {
                participationObjects.AddRange(
                    visitor.GetPublishedActivitiesInLocations(hostedLocationIds, msgs, dc)
                        .Select(activity => activity.GetParticipationObject(msgs))
                        .DistinctBy(participationObject => new { participationObject.ObjectID, participationObject.ObjectType }));
            });

            participationObjects.AddRange(
                account.GetPublishedExternalLeagues(msgs, dc).Select(x => x.GetParticipationObject(msgs, dc))
            );

            participationObjects.AddRange(
                account.GetPublishedSessions(msgs, dc).Select(x => x.GetParticipationObject(msgs, dc))
            );

            return participationObjects;
        }

        public static List<Activity> GetPublishedActivities(this Account account, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var periods = account.GetOpenUpcomingRecentlyClosedRegistrationPeriods(msgs, dc);
            var activities = new List<Activity>();

            periods.ForEach(period => activities.AddRange(period.GetActivities(msgs, dc).Where(x => x.IsPubliclyViewable)));

            activities.AddRange(
                Activity.LoadByProviderAccountId(account.ID.Value, msgs, dc)
                    .Where(activity => activity.IsOpenUpcomingOrRecentlyClosed(msgs, dc))
            );

            return activities.DistinctBy(activity => activity.ID).ToList();
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

        public static List<Activity> GetActivities(this Account account, UserType userType, WebUser user, ActivityType? activityType, bool? isArchived, bool? isDeleted, bool? ungroupedOnly, int? start = null, int? length = null, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (user.IsEmpty() || userType.In(AccountService.AdminUserTypes))
                return Activity.LoadByAccountId(account.ID.Value, null, null, activityType, isArchived, isDeleted, ungroupedOnly, start, length, msgs, dc).ToList();

            return Activity.LoadByAccountId(account.ID.Value, null, null, activityType, isArchived, isDeleted, ungroupedOnly, start, length, msgs, dc)
                    .Where(activity => user.HasAccessToActivity(activity, msgs, dc))
                    .ToList();
        }

        public static int GetActivityCount(this Account account, UserType userType, WebUser user, ActivityType? activityType, bool? isArchived, bool? isDeleted, bool? ungroupedOnly, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (user.IsEmpty() || userType.In(AccountService.AdminUserTypes))
                return Activity.GetCountByAccountId(account.ID.Value, null, activityType, isArchived, isDeleted, ungroupedOnly, msgs, dc);

            return account.GetActivities(userType, user, activityType, isArchived, isDeleted, ungroupedOnly, null, null, msgs, dc).Count();
        }

        public static int GetActivityCount(this Account account, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return Activity.GetCount(new Hashtable() { { "AccountId", account.ID.Value } }, msgs, dc);
        }

        public static bool HasActivities(this Account account, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return account.GetActivityCount(msgs, dc) > 0;
        }

        public static UserType[] AdminUserTypes
        {
            get
            {
                return new UserType[] { UserType.Administrator, UserType.Owner, UserType.SuperAdmin, UserType.Treasurer };
            }
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
            return Registrant.LoadByAccountIdByIsDeleted(account.ID.Value, false, start, length, dc: dc);
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

        public static List<ActivityGroup> GetActivityGroups(this Account account, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return ActivityGroup.LoadByAccountId(account.ID.Value, msgs, dc);
        }

        public static List<ActivityGroup> GetSessions(this Account account, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return ActivityGroup.LoadByAccountIdByActivityGroupTypeId(account.ID.Value, ActivityGroupType.Session, msgs, dc);
        }

        public static List<ActivityGroup> GetPublishedSessions(this Account account, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var groups = ActivityGroup.LoadByAccountIdByIsPublishedByIsArchived(account.ID.Value, true, false, msgs, dc);

            if (!groups.Any())
                return new List<ActivityGroup>();

            var periods = RegistrationPeriod.LoadByActivityGroupIds(
                    groups.Select(x => x.ID.Value).ToList(),
                    msgs,
                    dc);

            periods.Where(period => 
                        period.IsOpen(account, msgs, dc) || 
                        period.WillOpen(account, msgs, dc) || 
                        period.IsRecentlyClosed(account, msgs, dc))
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
            var now = account.ConvertToAccountTimeZone(DateTime.UtcNow);
            return account.GetRegistrationPeriods(msgs, dc)
                .Where(x => x.StartDateTime.Value.IsAfter(now))
                .OrderBy(x => x.StartDateTime.Value)
                .ToList();
        }

        public static List<RegistrationPeriod> GetOpenRegistrationPeriods(this Account account, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var now = account.ConvertToAccountTimeZone(DateTime.UtcNow);
            return account.GetRegistrationPeriods(msgs, dc)
                .Where(x =>
                    x.StartDateTime.Value.IsOnOrBefore(now) &&
                    x.EndDateTime.Value.IsOnOrAfter(now)
                 )
                .ToList();
        }

        public static List<RegistrationPeriod> GetClosedRegistrationPeriods(this Account account, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var now = account.ConvertToAccountTimeZone(DateTime.UtcNow);
            return account.GetRegistrationPeriods(msgs, dc)
                .Where(x => x.EndDateTime.Value.IsBefore(now))
                .ToList();
        }

        public static List<RegistrationPeriod> GetRecentlyClosedRegistrationPeriods(this Account account, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var now = account.ConvertToAccountTimeZone(DateTime.UtcNow);
            return account.GetRegistrationPeriods(msgs, dc)
                .Where(period => period.EndDateTime.Value.AddWeeks(8).IsOnOrAfter(now))
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

        public static bool HasAccountPackage(this Account account, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return AccountPackage.GetCount(new Hashtable() { { "AccountId", account.ID.Value } }, msgs, dc) > 0;
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

        public static bool HasAccountPackageFeatures(this Account account, List<FeatureType> featureTypes, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var accountPackage = account.GetActiveAccountPackage(msgs, dc);

            if (accountPackage.IsNotEmpty())
                return AccountPackageFeature.HasAccountPackageIdByFeatureTypeIds(accountPackage.ID.Value, featureTypes, msgs, dc);

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

        public static List<AccountWebUser> GetMembers(this Account account, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return AccountWebUser.LoadByAccountIdByIsDeleted(account.ID.Value, false, msgs, dc);
        }

        public static List<AccountWebUser> GetActiveMembers(this Account account, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return AccountWebUser.LoadByAccountIdByIsActiveByIsDeleted(account.ID.Value, true, false, msgs, dc);
        }

        public static AccountWebUser AddUser(this Account account, WebUser user, UserType ut, WebUser loggedInUser, SystemMessages msgs, DatabaseConnection dc = null)
        {
            AccountWebUser awu = null;

            Database.ExecuteTransaction(
                action: (SystemMessages amsgs, DatabaseConnection adc) =>
                {
                    awu = AccountWebUser.LoadByAccountIdByWebUserId(account.ID.Value, user.ID.Value, amsgs, adc);

                    if (awu.IsEmpty())
                        awu = new AccountWebUser
                        {
                            CreateDate = DateTime.UtcNow,
                            CreatedBy = loggedInUser?.ID.IfEmpty(0),
                            WebUserId = user.ID.Value,
                            AccountId = account.ID.Value,
                            IsDefault = false,
                            IsActive = user.IsActive
                        };
                    else
                        awu.IsActive = true;

                    awu.LastModifiedDate = DateTime.UtcNow;
                    awu.LastModifiedBy = loggedInUser?.ID.IfEmpty(0);
                    awu.UserTypeId = ut;
                    amsgs.AddRange(awu.Save(adc));

                    if (ut == UserType.Owner && account.OwnerId != user.ID)
                    {
                        account.OwnerId = user.ID.Value;
                        amsgs.AddRange(account.Save(adc));
                    }
                },
                onSuccess: (SystemMessages smsgs) => msgs.AddRange(smsgs),
                onFailure: (SystemMessages fmsgs) => msgs.AddRange(fmsgs),
                onException: (SystemMessages emsgs) => Helpers.Utilities.SystemErrorLog(emsgs),
                dc: dc
            );

            return awu;
        }

        public static WebUser AddUser(this Account account, string firstName, string lastName, string email, UserType userType, WebUser loggedInUser, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return account.AddUser(firstName, lastName, email, null, null, false, true, false, userType, true, loggedInUser, msgs, dc: dc);
        }

        public static WebUser AddUser(this Account account, string firstName, string lastName, string email, string phone, UserType userType, WebUser loggedInUser, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return account.AddUser(firstName, lastName, email, phone, null, false, false, false, userType, true, loggedInUser, msgs, dc: dc);
        }

        public static WebUser AddUser(this Account account, string firstName, string lastName, string email, string phone, UserType userType, bool canUseExistingUserAccount, WebUser loggedInUser, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return account.AddUser(firstName, lastName, email, phone, null, false, false, false, userType, canUseExistingUserAccount, loggedInUser, msgs, dc: dc);
        }

        public static WebUser AddUser(this Account account, string firstName, string lastName, string email, string phone, string password, bool isActive, bool isRegistered, bool isVerified, UserType userType, bool canUseExistingUserAccount, WebUser loggedInUser, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();
            WebUser user = null;
            var userExists = false;
            var emailExists = false;
            var phoneExists = false;

            if (WebUser.ExistsByEmail(email.Trim(), msgs, dc))
                userExists = emailExists = true;
            else if (phone.IsNotEmpty() && WebUser.ExistsByPhone(phone.Trim(), msgs, dc))
                userExists = phoneExists = true;

            if (!canUseExistingUserAccount)
            {
                if (emailExists)
                    msgs.Add("Email is already in use", MessageType.UserError);
                
                if (phoneExists)
                    msgs.Add("Phone is already in use", MessageType.UserError);
            }

            if (msgs.HasNoErrors())
                Database.ExecuteTransaction(
                    action: (SystemMessages amsgs, DatabaseConnection adc) =>
                    {
                        if (!userExists)
                        {
                            user = new WebUser
                            {
                                FirstName = firstName.Trim(),
                                LastName = lastName.Trim(),
                                Email = email.Trim(),
                                Phone = phone.IfBlank("").Trim(),
                                CreateDate = DateTime.Now,
                                LastModifiedDate = DateTime.Now,
                                CreatedBy = loggedInUser?.ID.IfEmpty(0),
                                LastModifiedBy = loggedInUser?.ID.IfEmpty(0),
                                IsActive = isActive,
                                IsRegistered = isRegistered,
                                VerifiedAccount = isVerified,
                                Uid = Guid.NewGuid(),
                                Username = WebUserService.GenerateUsername(firstName, lastName, amsgs, adc)
                            };

                            if (!password.IsEmpty())
                                user.Password = WebUserService.EncryptPassword(password);

                            amsgs.AddRange(user.Save(adc));
                        }
                        else if (canUseExistingUserAccount)
                        {
                            user = WebUser.LoadByEmail(email.Trim(), amsgs, adc);
                        }
                    },
                    onSuccess: (SystemMessages smsgs) => msgs.AddRange(smsgs),
                    onFailure: (SystemMessages fmsgs) => msgs.AddRange(fmsgs),
                    onException: (SystemMessages emsgs) => Helpers.Utilities.SystemErrorLog(emsgs),
                    dc: dc
                );

            if (msgs.HasNoErrors())
                AddUser(account, user, userType, loggedInUser, msgs, dc);

            return user;

        }

        public static WebUser CreateAndInviteUser(this Account account, string firstName, string lastName, string email, string phone, string message, UserType userType, WebUser loggedInUser, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            var user = account.AddUser(
                firstName: firstName, 
                lastName: lastName, 
                email: email, 
                phone: phone, 
                userType: userType, 
                canUseExistingUserAccount: false, 
                loggedInUser: loggedInUser, 
                msgs: msgs, 
                dc: dc);

            if (msgs.HasNoErrors())
                msgs.AddRange(account.SendInvitedEmail(user, loggedInUser, message, dc));

            if (msgs.HasErrors())
                msgs.Add($"An invitation was not sent to {email.Trim()}, a system error has occurred", MessageType.UserError);

            return user;
        }

        public static WebUser InviteUser(this Account account, string firstName, string lastName, string email, string phone, string message, UserType userType, WebUser loggedInUser, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return InviteUser(account, firstName, lastName, email, phone, message, userType, true, loggedInUser, msgs, dc);
        }

        public static WebUser InviteUser(this Account account, string firstName, string lastName, string email, string phone, string message, UserType userType, bool canUseExistingUserAccount, WebUser loggedInUser, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            var user = account.AddUser(firstName, lastName, email, phone, userType, canUseExistingUserAccount, loggedInUser, msgs, dc);

            if (msgs.HasNoErrors())
                msgs.AddRange(account.SendInvitedEmail(user, loggedInUser, message, dc));

            if (msgs.HasErrors())
                msgs.Add($"An invitation was not sent to {email.Trim()}, a system error has occurred", MessageType.UserError);

            return user;
        }

        public static bool HasTreasurer(this Account account, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return account.HasUserType(UserType.Treasurer, msgs, dc);
        }

        public static bool HasUserType(this Account account, UserType userType, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return AccountWebUser.GetCount(new Hashtable() { { "AccountId", account.ID.Value }, { "UserTypeId", (int)userType } }, msgs, dc) > 0;
        }

        #endregion

        #region Emails

        public static SystemMessages SendWelcomeEmailTrial(this Account account, DatabaseConnection dc = null)
        {
            return Helpers.EmailHelper.SendWelcomeTrial(account.GetAccountOwner(dc: dc), account);
        }

        public static SystemMessages SendSubscriptionPaymentSucceeded(this Account account, RegistrantSubscriptionPlan rsp, SubscriptionPlan plan, DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();
            var accountOwner = account.GetAccountOwner(msgs, dc);
            msgs.AddRange(Helpers.EmailHelper.SendSubscriptionPaymentSucceeded(accountOwner, rsp, plan));
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

        public static SystemMessages SendWelcomeEmailPurchase(int accountId)
        {
            var msgs = new SystemMessages();
            var account = Account.Load(accountId, msgs);
            var owner = account.GetAccountOwner(msgs);

            return account.SendWelcomeEmailPurchase(owner);
        }

        public static SystemMessages SendWelcomeEmailPurchase(this Account account, WebUser accountOwner)
        {
            return Helpers.EmailHelper.SendWelcomePurchase(accountOwner, account);
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
                account.CreateStripeCustomer(msgs, dc);

            return msgs;
        }

        public static Stripe.Account UpdateStripeCustomAccount(this Account account, Stripe.AccountUpdateOptions options, SystemMessages msgs, DatabaseConnection dc = null)
        {
            var stripeAccount = StripeHelper.UpdateCustomAccount(account.StripeUserId, options, msgs);

            if (stripeAccount.IsNotEmpty())
            {
                account.StripeChargesEnabled = stripeAccount.ChargesEnabled;
                account.StripePayoutsEnabled = stripeAccount.PayoutsEnabled;
                msgs.AddRange(account.Save(dc));
            }

            return stripeAccount;
        }

        public static Stripe.Account CreateStripeCustomAccount(this Account account, Stripe.AccountCreateOptions options, SystemMessages msgs, DatabaseConnection dc = null)
        {
            var stripeAccount = StripeHelper.CreateCustomAccount(options, msgs);

            if (stripeAccount.IsNotEmpty())
            {
                account.StripeUserId = stripeAccount.Id;
                msgs.AddRange(account.Save(dc));

                account.UpdateStripeChargesAndPayouts(stripeAccount, msgs, dc);
            }

            return stripeAccount;
        }

        public static void UpdateStripeChargesAndPayouts(this Account account, Stripe.Account stripeAccount, SystemMessages msgs, DatabaseConnection dc = null)
        {
            account.StripeChargesEnabled = stripeAccount.ChargesEnabled;
            account.StripePayoutsEnabled = stripeAccount.PayoutsEnabled;
            msgs.AddRange(account.Save(dc));
        }

        public static Stripe.Account GetStripeCustomAccount(this Account account, SystemMessages msgs)
        {
            if (account.HasStripeConnectedAccount)
                return StripeHelper.GetCustomAccount(account.StripeUserId, msgs);

            return null;
        }

        private static void SetStripeCustomer(this Account account, Stripe.Customer customer, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            account.StripeCustomerId = customer.Id;
            msgs.AddRange(account.Save(dc: dc));
        }

        public static Stripe.Customer CreateStripeCustomer(this Account account, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            var customer = account.GetStripeCustomer(msgs);

            if (customer.IsNotEmpty())
                return StripeHelper.UpdateCustomer(customer.Id, account.Name, account.Email, null, msgs);

            customer = StripeHelper.CreateCustomer(account.Name, account.Email, null, msgs);
            account.SetStripeCustomer(customer, msgs, dc);

            return customer;
        }

        public static Stripe.Source GetStripeSource(this Account account, string sourceId, SystemMessages msgs = null)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            Stripe.Source source = null;
            var customer = account.GetStripeCustomer(msgs);

            if (customer.IsNotEmpty())
                source = Billing.StripeHelper.GetSource(sourceId, msgs);

            return source;
        }

        public static List<Stripe.Source> GetStripeSources(this Account account, SystemMessages msgs = null)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();            
            var customer = account.GetStripeCustomer(msgs);

            if (customer.IsNotEmpty())
                return StripeHelper.GetSources(customer.Id, msgs);

            return new List<Stripe.Source>();
        }

        public static List<Stripe.BankAccount> GetStripeBankAccounts(this Account account, SystemMessages msgs = null)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();
            var customer = account.GetStripeCustomer(msgs);

            if (customer.IsNotEmpty())
                return StripeHelper.GetBankAccounts(customer.Id, msgs);

            return new List<Stripe.BankAccount>();
        }

        public static List<Stripe.Charge> GetStripeBillingHistory(this Account account, SystemMessages msgs = null)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            var charges = new List<Stripe.Charge>();
            var customer = account.GetStripeCustomer(msgs);

            if (customer.IsNotEmpty())
                charges = Billing.StripeHelper.GetCharges(customer.Id, msgs);

            return charges;
        }

        public static List<Stripe.Refund> GetStripeRefundHistory(this Account account, SystemMessages msgs = null)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            var refunds = new List<Stripe.Refund>();
            var customer = account.GetStripeCustomer(msgs);

            if (!customer.IsEmpty())
            {
                var charges = account.GetStripeBillingHistory(msgs);
                charges.ForEach(charge => refunds.AddRange(Billing.StripeHelper.GetRefunds(charge.Id, msgs)));
            }

            return refunds;
        }

        public static Stripe.Customer GetStripeCustomer(this Account account, SystemMessages msgs)
        {
            if (!account.StripeCustomerId.IsEmpty())
                return StripeHelper.GetCustomer(account.StripeCustomerId, null, msgs);

            return null;
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

        public static List<Attendance> GetCompletedAttendance(this Account account, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return Attendance.LoadByAccountIdByIsCompleted(account.ID.Value, true, msgs, dc);
        }

        public static int GetAttendanceCompletedCount(this Account account, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return Attendance.GetCountByAccountIdByIsCompleted(account.ID.Value, true, msgs, dc);
        }

        public static int GetAttendancePresentCount(this Account account, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return AttendanceRegistrant.GetPresentCountByAccountId(account.ID.Value, msgs, dc);
        }

        public static int GetAttendancePossibleCount(this Account account, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return AttendanceRegistrant.GetPossibleCountByAccountId(account.ID.Value, msgs, dc);
        }

        public static List<Attendance> GetAttendanceByDate(this Account account, DateTime date, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return Attendance.LoadByAccountIdByDate(account.ID.Value, date, msgs, dc);
        }

        #endregion

        #region Registrations

        public static List<Registration> GetRegistrations(this Account account, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return Registration.LoadByAccountId(account.ID.Value, msgs, dc)
                .Where(registration => Registration.ValidCompletionStatus.Contains(registration.CompletionStatusId))
                .ToList();
        }

        public static List<Registration> GetRegistrationsByDate(this Account account, DateTime startDate, DateTime endDate, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return Registration.LoadByAccountIdByDate(account.ID.Value, startDate, endDate, msgs, dc);
        }

        public static int GetRegistrationCountByDate(this Account account, DateTime startDate, DateTime endDate, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return Registration.GetCountByAccountIdByDate(account.ID.Value, startDate, endDate, msgs, dc);
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

        public static List<Registration> SearchRegistrations(this Account account, string searchText, int? questionId, List<int> participationObjectIds, ParticipationObjectType participationObjectType, RegistrationStatus regStatus, List<RegistrationAssignmentType> rats, int? start, int? length, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return Registration.Search(searchText, account.ID.Value, questionId, participationObjectIds, participationObjectType, regStatus, rats, start, length, msgs: msgs, dc: dc);
        }

        public static int SearchRegistrationCount(this Account account, string searchText, int? questionId, List<int> participationObjectIds, ParticipationObjectType participationObjectType, RegistrationStatus regStatus, List<RegistrationAssignmentType> rats, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return Registration.SearchCount(searchText, account.ID.Value, questionId, participationObjectIds, participationObjectType, regStatus, rats, msgs: msgs, dc: dc);
        }

        public static void CreateNewRegistrantAndRegistration(this Account account, ParticipationObject participationObject, List<(FormQuestion question, string answer)> questionsAndAnswers, RegistrationPeriod period, WebUser loggedInUser, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            account.CreateNewRegistrantAndRegistration(
                participationObject,
                questionsAndAnswers,
                new List<(Activity activity, int preferenceNumber)>(),
                period,
                loggedInUser,
                msgs,
                dc);
        }

        public static void CreateNewRegistrantAndRegistration(this Account account, ParticipationObject participationObject, List<(FormQuestion question, string answer)> questionsAndAnswers, List<(Activity activity, int preferenceNumber)> activitiesAndPreferences, RegistrationPeriod period, WebUser loggedInUser, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            Registrant student = null;

            Database.ExecuteTransaction
            (
                action: (SystemMessages amsgs, DatabaseConnection adc) =>
                {
                    var location = account.GetDefaultLocation(amsgs, adc);
                    var groupId = participationObject.GetAnyRelatedActivityGroupId();
                    var aagId = participationObject.GetAnyRelatedAccountActivityGroupId();

                    // create student registrant record associated to this (provider) account
                    student = account.CreateRegistrant(
                            registrantOwner: null,
                            canBeAccessedBy: null,
                            registrantType: RegistrantType.Individual,
                            isMaster: false,
                            isParentOrGuardian: false,
                            allowTextMessages: false,
                            schoolLocationId: location?.ID,
                            profileStatus: RegistrantProfileStatus.Started,
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
                        student.AddOrUpdateAnswer(
                            answerText: questionAndAnswer.answer.IfEmpty("").Trim(),
                            question: questionAndAnswer.question,
                            registrationId: registration.ID,
                            loggedInUser: loggedInUser,
                            msgs: amsgs,
                            dc: adc);
                    });

                    // manages seats and create registrantactivity record
                    registration.ProcessRegistrationAssignment(
                        participationObject: participationObject,
                        masterRegistrant: null,
                        registrant: student,
                        seatsRequested: 1,
                        activityGroupId: groupId,
                        periodId: period?.ID,
                        amountCharged: participationObject.Cost,
                        loggedInUser: loggedInUser,
                        msgs: amsgs,
                        dc: adc);

                    activitiesAndPreferences.ForEach(activityAndPreference =>
                    {
                        var activityParticipationObject = activityAndPreference.activity.GetParticipationObject(amsgs, adc);

                        registration.ProcessRegistrationAssignment(
                            participationObject: activityParticipationObject,
                            masterRegistrant: null,
                            registrant: student,
                            seatsRequested: 1,
                            preferenceNumber: activityAndPreference.preferenceNumber,
                            activityGroupId: groupId,
                            periodId: period?.ID,
                            amountCharged: activityParticipationObject.Cost,
                            loggedInUser: loggedInUser,
                            msgs: amsgs,
                            dc: adc);
                    });

                },
                onSuccess: (SystemMessages smsgs) =>
                {
                    msgs.AddRange(smsgs);
                    msgs.Add("Your new registration has been added", MessageType.Success);
                },
                onFailure: (SystemMessages fmsgs) => msgs.AddRange(fmsgs),
                onException: (SystemMessages emsgs) => Helpers.Utilities.SystemErrorLog(emsgs),
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

        public static AccountPackage SetDefaultAccountPackage(this Account account, WebUser loggedInUser, SystemMessages msgs, DatabaseConnection dc = null)
        {
            var package = Package.Load(start: 0, length: 1, msgs: msgs, dc: dc).First();

            return account.AddPackage(package.ID.Value, BillingPeriodType.Month, loggedInUser, msgs, dc);
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
            var reservedUniqueNames = new string[] { "demo", "about", "copyright", "index", "parents", "privacy", "providers", "schools", "tos", "account", "afterschool", "db", "faq", "home", "legal", "login", "logout", "oauth", "r", "reg", "services", "signup" };

            while ((duplicate.IsNotEmpty() && duplicate.ID != accountId) || uniqueName.ToLower().In(reservedUniqueNames))
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
            //if (account.StripeDefaultCardId.IsNotEmpty())
            //{
            //    // paying as soon as they sign up
            //    if (activePackage.IsEmpty())
            //    {
            //        signupType = SignUpType.NewAccountSignUp;
            //    }
            //    // changing packages but have already paid, force those upgrading from trials to upgrade on current plan
            //    else if (activePackage.PackageId != p.ID)
            //    {
            //        signupType = SignUpType.ChangePackage;
            //    }
            //    // if same package but the billing period is changing
            //    else if (activePackage.PackageId == p.ID && activePackage.BillingPeriodTypeId != billingPeriodType)
            //    {
            //        signupType = SignUpType.ChangeBillingPeriodType;
            //    }
            //    // simply converting from the same package from trial
            //    else if (activePackage.TrialEndDate.HasValue)
            //    {
            //        signupType = SignUpType.ConvertingFromTrial;
            //    }
            //}
            if (p.IsFree(billingPeriodType) || (activePackage.IsNotEmpty() && activePackage.IsFree))
            {
                // new sign up
                if (activePackage.IsEmpty())
                    signupType = SignUpType.NewAccountSignUp;

                // could be downgrading to a free account
                else if (activePackage.PackageId != p.ID)
                    signupType = SignUpType.ChangePackage;
            }
            else
            {
                if (activePackage.IsEmpty())
                    trialDayCount = (p.HasFeature(FeatureType.TrialDays, msgs, dc) ? p.GetFeature(FeatureType.TrialDays, msgs, dc).GetMaxValue() : 0).ToInteger();
                else if (activePackage.HasFeature(FeatureType.TrialDays, msgs, dc))
                    trialDayCount = account.GetTrialDaysRemaining(activePackage, msgs, dc);

                if (trialDayCount > 0)
                {
                    // totally new
                    if (activePackage.IsEmpty())
                        signupType = SignUpType.TrialSignUp;
                    // still during the trial but changing packages with no new payment info
                    else
                        signupType = SignUpType.ChangePackageDuringTrial;
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
            var loggedInUserId = loggedInUser.IsNotEmpty() ? loggedInUser.ID.Value : 0;

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
                                newap.CreatedBy = loggedInUserId;
                                newap.LastModifiedDate = DateTime.UtcNow;
                                newap.LastModifiedBy = loggedInUserId;
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
                                currentPackage.LastModifiedBy = loggedInUserId;

                                amsgs.AddRange(currentPackage.SaveWithStripe(account.StripeCustomerId, true, null, adc));

                                newap = currentPackage;
                            }

                        }

                        if (amsgs.HasNoErrors() && newap.ID.HasValue)
                            package.CopyFeaturesToAccountPackage(newap, loggedInUserId, amsgs, adc);

                        // ensure package features are added
                        account.EnsureDefaultPackageFeatures(newap, loggedInUser, msgs, dc);

                        // make sure the account package is set to the real trial days passed in
                        if (amsgs.HasNoErrors())
                        {
                            var apf = newap.GetFeature(FeatureType.TrialDays, amsgs, adc);
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
                onSuccess: (SystemMessages smsgs) => msgs.AddRange(smsgs),
                onFailure: (SystemMessages fmsgs) => msgs.AddRange(fmsgs),
                onException: (SystemMessages emsgs) => Helpers.Utilities.SystemErrorLog(emsgs),
                dc: dc
            );

            return newap;
        }

        public static void EnsureDefaultPackageFeatures(this Account account, WebUser loggedInUser, SystemMessages msgs, DatabaseConnection dc = null)
        {
            account.EnsureDefaultPackageFeatures(null, loggedInUser, msgs, dc);
        }

        public static void EnsureDefaultPackageFeatures(this Account account, AccountPackage ap, WebUser loggedInUser, SystemMessages msgs, DatabaseConnection dc = null)
        {
            if (ap.IsEmpty())
                ap = account.GetActiveAccountPackage(msgs, dc);

            if (ap.IsEmpty())
                return;

            if (account.AccountTypeId == AccountType.School)
            {
                ap.AddOrUpdatePackageFeature(FeatureType.ManagePermissionSlips, 1, loggedInUser, msgs, dc);
                ap.AddOrUpdatePackageFeature(FeatureType.ManageFieldTrips, 1, loggedInUser, msgs, dc);
                ap.AddOrUpdatePackageFeature(FeatureType.ManageActivityGroups, 1, loggedInUser, msgs, dc);
            }

            ap.AddOrUpdatePackageFeature(FeatureType.AcceptACH, 1, loggedInUser, msgs, dc);
        }

        public static int GetTrialDaysRemaining(this Account account, AccountPackage currentPackage, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var daysRemaining = 0;
            var firstAccountPackage = account.GetAccountPackages(msgs, dc).OrderBy(x => x.StartDate.Value).First();
            var daysSinceSignUp = (DateTime.UtcNow - firstAccountPackage.StartDate.Value).Days;
            if (daysSinceSignUp < currentPackage.GetFeature(FeatureType.TrialDays, msgs, dc).GetMaxValue() + 1) // they get the full last day
                daysRemaining = (currentPackage.GetFeature(FeatureType.TrialDays, msgs, dc).GetMaxValue() - daysSinceSignUp).ToInteger();

            return daysRemaining;
        }

        public static Activity GetDefaultNewActivity(this Account account)
        {
            return new Activity
            {
                WaitingListSeats = 0,
                SeatsAvailable = Math.Min(20, account.GetActiveAccountPackage().GetFeature(FeatureType.Participants).Max.Value.ToInteger()),
                CostPerRegistrant = 0,
                AccountId = account.ID,
                RegistrationModeId = RegistrationMode.Register,
                AllowToPayLater = true,
            };
        }

        public static ActivityGroup GetDefaultNewLeague(this Account account)
        {
            return new ActivityGroup
            {
                WaitingListSeats = 0,
                SeatsAvailable = Math.Min(20, account.GetActiveAccountPackage().GetFeature(FeatureType.Participants).Max.Value.ToInteger()),
                Cost = 0,
                AccountId = account.ID,
                RegistrationModeId = RegistrationMode.Register,
                AllowToPayLater = true,
                ActivityGroupTypeId = ActivityGroupType.League,
                RequiresParentalConsent = true,
                CostUnitTypeId = CostUnitType.PerRegistrant,
            };
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
            return CreateNew(orgName, null, null, false, accountType, null, null, null, null, loggedInUser, isAfterSchool, false, msgs, dc);
        }

        public static Account CreateNew(string orgName, string email, string phone, bool isActive, AccountType accountType, WebUser accountOwner, int? packageId, BillingPeriodType? packageBillingPeriod, string timeZone, WebUser loggedInUser, bool isAfterSchool, bool createWithStripe, SystemMessages msgs, DatabaseConnection dc)
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
                UniqueName = uniqueName,
                ConvenienceFeesPaidBy = FeesPaidByType.Parent,
                CreditCardFeesPaidBy = FeesPaidByType.Parent,
                TimeZone = timeZone
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
                //if (accountType != AccountType.AfterSchool && accountType != AccountType.Registrant && createWithStripe)
                //    account.CreateStripeCustomAccount(msgs, dc);

                //create default form for account to use for registration
                if (accountType != AccountType.Registrant)
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
            var arls = AccountRelationshipLocation.LoadByVisitorAccountId(visitorAccount.ID.Value, msgs, dc);
            var locations = new List<Location>();

            arls.ForEach(arl => {
                var location = arl.GetLocation(msgs, dc);

                if (location.IsNotEmpty())
                    locations.Add(location);
            });

            return locations;
        }

        public static List<Location> GetHostedLocations(this Account hostAccount, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return Location.LoadByHostAccountId(hostAccount.ID.Value, msgs, dc);
        }

        public static List<Location> GetAllAvailableLocations(this Account account, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var locations = account.GetHostedLocations(msgs, dc);
            var visitorLocations = account.GetVisitorLocations(msgs, dc);

            if (visitorLocations.Any())
                locations.AddRange(visitorLocations);

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

        public static bool IsApprovedAsVisitorTo(this Account visitorAccount, Account hostAccount, SystemMessages msgs, DatabaseConnection dc = null)
        {
            var relationship = AccountRelationship.LoadByHostAccountIdByVisitorAccountId(hostAccount.ID.Value, visitorAccount.ID.Value, msgs, dc);
            return relationship.ApprovalStatusId == ApprovalStatus.Approved;
        }

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
                relationship = AccountRelationshipService.CreateNew(
                    hostAccount, 
                    visitorAccount, 
                    ApprovalStatus.Approved, 
                    hostPermissionType, 
                    loggedInUser, 
                    msgs, 
                    dc);

            return relationship;
        }

        public static void SendApprovalNotificationToVisitor(this Account hostAccount, Account visitorAccount, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            msgs.AddRange(Helpers.EmailHelper.SendApprovalNotificationToVisitor(visitorAccount, hostAccount));
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
                timeZone: null,
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
            var account = user.AddNewAccount("Sample Account", "317-899-9189", true, accountType, packageId, BillingPeriodType.Month, null, true, user, msgs, dc);

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
            return Registrant.LoadByAccountIdByIsDeleted(account.ID.Value, false, msgs, dc)
                .Where(registrant => registrant.IsParentOrGuardian)
                .OrderBy(registrant => registrant.ID)
                .FirstOrDefault();
        }

        public static Registrant GetSecondParentOrGuardianRegistrant(this Account account, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var registrants = Registrant.LoadByAccountIdByIsDeleted(account.ID.Value, false, msgs, dc)
                .Where(registrant => registrant.IsParentOrGuardian)
                .OrderBy(registrant => registrant.ID)
                .ToList();

            if (registrants.Count > 1)
                return registrants[1];

            return null;
        }

        public static List<Registrant> GetChildrenRegistrants(this Account account, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return Registrant.LoadByAccountIdByIsDeleted(account.ID.Value, false, msgs, dc)
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
        public static Registrant CreateRegistrant(this Account account, WebUser registrantOwner, List<WebUser> canBeAccessedBy, RegistrantType registrantType, bool isMaster, bool isParentOrGuardian, bool allowTextMessages, int? schoolLocationId, RegistrantProfileStatus profileStatus, WebUser loggedInUser, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            var registrant = new Registrant
            {
                RegistrantTypeId = registrantType,
                IsMaster = isMaster,
                IsContactOnly = isParentOrGuardian,
                IsParentOrGuardian = isParentOrGuardian,
                WebUserId = registrantOwner?.ID,
                AccountId = account.ID,
                AllowTextMessages = allowTextMessages,
                SchoolLocationId = schoolLocationId,
                ProfileStatusId = profileStatus,
                CreateDate = DateTime.UtcNow,
                LastModifiedDate = DateTime.UtcNow,
                CreatedBy = loggedInUser?.ID.IfEmpty(0),
                LastModifiedBy = loggedInUser?.ID.IfEmpty(0)
            };
            msgs.AddRange(registrant.Save(dc));

            if (canBeAccessedBy.IsNotEmpty() && canBeAccessedBy.Count > 0)
                registrant.AddUsersWithAccess(canBeAccessedBy, loggedInUser, msgs, dc);

            return registrant;
        }

        #endregion

        #region Finance

        public static Payout RequestNewPayout(this Account account, List<Transaction> transactions, WebUser loggedInUser, SystemMessages msgs, DatabaseConnection dc = null)
        {
            var paymentType = PaymentType.NotSet;

            if (account.CanReceiveACHDeposits(msgs, dc))
                paymentType = PaymentType.ACH;
            else
            {
                paymentType = PaymentType.Check;
                //var checkAccount = account.GetCheckPaymentAccount(msgs, dc);
                //if (checkAccount.IsNotEmpty())
                //{
                //    if (!checkAccount.IsVerified)
                //        msgs.Add("Your payment address is not verified. Please correct in Settings > Accept Payments", MessageType.UserError);
                //    else
                //        paymentType = PaymentType.Check;
                //}
            }

            if (paymentType != PaymentType.NotSet)
            {
                var payout = PayoutService.CreateNew(account, transactions, PayoutStatus.Requested, paymentType, loggedInUser, msgs, dc);

                if (payout.IsNotEmpty())
                    payout.SendPayoutRequestNotification(account, msgs);

                return payout;
            }
            
            msgs.Add("No payment account defined", MessageType.UserError);

            return null;
        }

        public static List<Payout> GetPayouts(this Account account, PayoutStatus? payoutStatus, DateTime? startDate, DateTime? endDate, int? start, int? length, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return Payout.LoadPayouts(account.ID.Value, payoutStatus, startDate, endDate, start, length, msgs, dc);
        }

        public static Payout GetLastPayout(this Account account, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return Payout.GetLastPayout(account.ID.Value, msgs, dc);
        }

        public static decimal GetMaxAmountAvailableForPayout(this Account account, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var totalPayoutAmount = account.GetTotalPayoutAmount(null, msgs: msgs, dc: dc);
            var totalAmountPaid = account.GetTotalAmountBilledOut(msgs: msgs, dc: dc);
            // var totalAmountRefunded = account.GetTotalAmountRefundedTo(startDate, endDate, msgs);
            var totalFeesPaid = account.GetTotalFeesPaidAsBiller(null, null, msgs);

            return totalAmountPaid - totalFeesPaid - totalPayoutAmount;
        }

        public static int GetPayoutCount(this Account account, PayoutStatus? payoutStatus, DateTime? startDate, DateTime? endDate, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return Payout.GetCountByAccountId(account.ID.Value, payoutStatus, startDate, endDate, msgs, dc);
        }

        public static List<Transaction> GetBilledTransactions(this Account account, DateTime? startDate, DateTime? endDate, int? start, int? length, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return Transaction.LoadByBillerAccountId(account.ID.Value, startDate, endDate, null, start, length, msgs, dc);
        }

        public static List<Transaction> GetBilledTransactions(this Account account, DateTime? startDate, DateTime? endDate, List<(int ID, ParticipationObjectType ObjectType)> participationObjects, int? start, int? length, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return Transaction.LoadByBillerAccountId(account.ID.Value, startDate, endDate, participationObjects, start, length, msgs, dc);
        }

        public static List<Transaction> GetPaidTransactions(this Account account, DateTime? startDate, DateTime? endDate, int? start, int? length, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return Transaction.LoadByPayerAccountId(account.ID.Value, startDate, endDate, null, start, length, msgs, dc);
        }

        public static List<Transaction> GetPaidTransactions(this Account account, DateTime? startDate, DateTime? endDate, List<(int ID, ParticipationObjectType ObjectType)> participationObjects, int? start, int? length, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return Transaction.LoadByPayerAccountId(account.ID.Value, startDate, endDate, participationObjects, start, length, msgs, dc);
        }

        public static int GetBilledTransactionCount(this Account account, DateTime? startDate, DateTime? endDate, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return Transaction.GetCountByBillerAccountId(account.ID.Value, startDate, endDate, null, msgs, dc);
        }

        public static int GetBilledTransactionCount(this Account account, DateTime? startDate, DateTime? endDate, List<(int ID, ParticipationObjectType ObjectType)> participationObjects, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return Transaction.GetCountByBillerAccountId(account.ID.Value, startDate, endDate, participationObjects, msgs, dc);
        }

        public static int GetPaidTransactionCount(this Account account, DateTime? startDate, DateTime? endDate, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return Transaction.GetCountByPayerAccountId(account.ID.Value, startDate, endDate, null, msgs, dc);
        }

        public static int GetPaidTransactionCount(this Account account, DateTime? startDate, DateTime? endDate, List<(int ID, ParticipationObjectType ObjectType)> participationObjects, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return Transaction.GetCountByPayerAccountId(account.ID.Value, startDate, endDate, participationObjects, msgs, dc);
        }

        public static decimal CalculatePlatformFeeAmount(this Account billingAccount, decimal totalAmountToCharge, SystemMessages msgs, DatabaseConnection dc = null)
        {
            if (billingAccount.HasFeature(FeatureType.UseStripeConnectedAccount, msgs, dc))
            {
                var percentageFee = billingAccount.GetAccountPackageFeature(FeatureType.PaymentsPercentageFee, msgs, dc).GetMaxValue();
                var flatFee = billingAccount.GetAccountPackageFeature(FeatureType.PaymentsPercentageFlatFee, msgs, dc).GetMaxValue();
                return (totalAmountToCharge * percentageFee) + flatFee;
            }

            return Platform.GetConvenienceFee(totalAmountToCharge, billingAccount);
        }

        public static decimal GetPlatformPercentFee(this Account billingAccount, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (billingAccount.HasFeature(FeatureType.UseStripeConnectedAccount, msgs, dc))
                return billingAccount.GetAccountPackageFeature(FeatureType.PaymentsPercentageFee, msgs, dc).GetMaxValue();

            return Platform.ConveniencePercentFee;
        }

        public static decimal GetPlatformFlatFee(this Account billingAccount, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (billingAccount.HasFeature(FeatureType.UseStripeConnectedAccount, msgs, dc))
                return billingAccount.GetAccountPackageFeature(FeatureType.PaymentsPercentageFlatFee, msgs, dc).GetMaxValue();

            return Platform.ConvenienceFlatFee;
        }

        internal static string GetStripeSourceFromToken(this Account fromAccount, string stripeToken, SystemMessages msgs = null)
        {
            var source = StripeHelper.CreateCreditCardSource(stripeToken, msgs);
            StripeHelper.AddNewCardToCustomer(fromAccount.StripeCustomerId, source, null, msgs);
            return source.Id;
        }

        internal static string GetStripeSourceFromPlaidToken(this Account fromAccount, BankAccountPurchaseObject bankAccountPurchaseObject, SystemMessages msgs = null)
        {
            var plaidToken = Helpers.PlaidHelper.GetPlaidToken(bankAccountPurchaseObject.PlaidPublicToken, msgs);

            if (msgs.HasNoErrors())
            {
                var bankAccount = Helpers.PlaidHelper.CreateStripeBankAccount(plaidToken.AccessToken, bankAccountPurchaseObject.PlaidAccountId, msgs);
                var source = StripeHelper.AttachSourceToCustomer(fromAccount.StripeCustomerId, bankAccount.StripeBankAccountToken, null, msgs);
                //return customer.DefaultSourceId;
                return source.Id;
            }

            return null;
        }

        internal static Stripe.Charge CreateStripeChargeDuringCheckout(CheckoutObject checkoutObject, SystemMessages msgs, DatabaseConnection dc = null)
        {
            // if stripe token then don't send customer id
            string customerId = null;

            if (checkoutObject.SavePaymentInfo || checkoutObject.HasStripeSourceId)
                customerId = checkoutObject.PayingAccount.StripeCustomerId;

            // if can charge fees we bill it directly from our AHQ account
            var useStripeConnectedAccount = checkoutObject.BillingAccount.HasFeature(FeatureType.UseStripeConnectedAccount, msgs, dc);
            var platformFeeAmount = checkoutObject.BillingAccount.CalculatePlatformFeeAmount(checkoutObject.TuitionCostNoFees, msgs, dc);

            // only charge tuition during checkout, because subscriptions are billed through stripe
            var charge = StripeHelper.CreateCharge(
                fromCustomerId: customerId,
                destinationAccountId: useStripeConnectedAccount ? checkoutObject.BillingAccount.StripeUserId : null,
                platformFee: useStripeConnectedAccount ? platformFeeAmount : new decimal?(),
                tokenOrSourceId: checkoutObject.StripeSourceId.IfEmpty(checkoutObject.CreditCardPurchaseObject?.StripeToken),
                totalAmount: checkoutObject.GetTuitionCostWithFees(msgs, dc),
                description: $"{checkoutObject.ParticipationObject.Name} Registration",
                statementDescriptor: $"{checkoutObject.BillingAccount.Name}",
                metaData: new Dictionary<string, string>()
                {
                    { "ActivityUid", checkoutObject.ParticipationObject.Uid.ToString() },
                    { "BillingAccountUid", checkoutObject.BillingAccount.Uid.ToString() },
                    { "PayingAccountUid", checkoutObject.PayingAccount.Uid.ToString() },
                    { "RegistrationUid", checkoutObject.Registration.Uid.ToString() },
                    { "CreditCardFeePayer", checkoutObject.CreditCardFeePayer.Uid.ToString() },
                    { "ConvenienceFeePayer", checkoutObject.ConvenienceFeePayer.Uid.ToString() },
                    { "PurchasedBy", checkoutObject.MasterRegistrant.DisplayName },
                    { "BilledBy", checkoutObject.BillingAccount.Name }
                },
                msgs: msgs
                );

            return charge;
        }

        public static Stripe.Refund ProcessRefund(string chargeId, decimal totalAmount, Account billingAccount, SystemMessages msgs, DatabaseConnection dc = null)
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
            return TransactionService.CreateNewRefund(
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

        /// <summary>
        /// returns stripe source id
        /// </summary>
        /// <param name="fromAccount"></param>
        /// <param name="toAccount"></param>
        /// <param name="stripeToken"></param>
        /// <param name="msgs"></param>
        /// <returns></returns>
        internal static string CreateStripeSourceFromToken(this Account fromAccount, string stripeToken, Stripe.Customer customer, SystemMessages msgs)
        {
            // stripe stuff
            // create payment source if not already existent on the user
            var sourceId = "";

            if (customer.IsEmpty())
                customer = StripeHelper.GetCustomer(fromAccount.StripeCustomerId, null, msgs);

            if (stripeToken.IsNotEmpty())
            {
                var source = StripeHelper.CreateCreditCardSource(stripeToken, msgs);
                StripeHelper.AttachSourceToCustomer(fromAccount.StripeCustomerId, source.Id, null, msgs);
                sourceId = source.Id;
            }
            else
                sourceId = customer.DefaultSourceId;

            return sourceId;
        }

        /// <summary>
        /// returns stripe source id
        /// </summary>
        /// <param name="fromAccount"></param>
        /// <param name="toAccount"></param>
        /// <param name="stripeToken"></param>
        /// <param name="msgs"></param>
        /// <returns></returns>
        internal static string CopyStripeSourceToAccount(this Account fromAccount, CheckoutObject checkoutObject, SystemMessages msgs)
        {
            // what happens if the same registrant gets another payment plan?
            // Create a new customer? For now YES, it doesn't really matter I don't think

            // get shared source
            var platformCustomer = StripeHelper.GetCustomer(fromAccount.StripeCustomerId, null, msgs);

            if (checkoutObject.StripeSourceId.IsNotEmpty() && checkoutObject.BillingAccount.HasFeature(FeatureType.UseStripeConnectedAccount))
            {
                var stripeUserId = checkoutObject.BillingAccount.StripeUserId;
                var customer = StripeHelper.GetCustomer(fromAccount.StripeCustomerId, stripeUserId, msgs);

                if (customer.IsEmpty())
                    customer = StripeHelper.CreateCustomer(
                        platformCustomer.Description,
                        platformCustomer.Email,
                        stripeUserId,
                        msgs);

                var tokenFromConnectedAccount = StripeHelper.CreateCustomerToken(
                    platformCustomer.Id,
                    stripeUserId,
                    checkoutObject.StripeSourceId,
                    msgs);
                
                var source = StripeHelper.CreateCreditCardSource(
                    tokenFromConnectedAccount.Id,
                    stripeUserId, 
                    msgs);

                StripeHelper.AddDefaultSourceToCustomer(
                    customer.Id, 
                    source.Id,
                    stripeUserId, 
                    msgs);

                return source.Id;
            }

            string sourceId = checkoutObject.StripeSourceId;

            if (checkoutObject.StripeToken.IsNotEmpty())
                sourceId = fromAccount.CreateStripeSourceFromToken(checkoutObject.StripeToken, platformCustomer, msgs);

            var copiedSource = StripeHelper.CreateSourceFromOriginal(
                platformCustomer.Id, 
                sourceId, 
                checkoutObject.BillingAccount.StripeUserId, 
                msgs);

            return copiedSource.Id;
        }

        private static void ProcessTuitionWithStripe(CheckoutObject checkoutObject, SystemMessages msgs, DatabaseConnection dc = null)
        {
            // if user wants to save payment info, do it before you charge the card
            // can't use a token twice
            if (msgs.HasNoErrors())
                checkoutObject.LoadStripeSource(msgs);

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
                onSuccess: (SystemMessages smsgs) => msgs.AddRange(smsgs),
                onFailure: (SystemMessages fmsgs) => msgs.AddRange(fmsgs),
                onException: (SystemMessages emsgs) => Helpers.Utilities.SystemErrorLog(emsgs),
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
            // var amountPaid = checkoutObject.TuitionCostNoFees;

            // -- should be fixed now
            var amountPaid = checkoutObject.TotalCostToPurchaseTodayNoFees;

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
                    ra.ChangeSeatAssignment(RegistrationAssignmentType.Reserved, loggedInUser, msgs, dc);
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
                    -amount,
                    TransactionType.Refund,
                    PaymentType.ACH,
                    PaymentSourceType.AfterSchoolPlatform,
                    loggedInUser,
                    msgs,
                    dc);

                var registrant = Registrant.Load(originalTransaction.GetPurchase().RegistrantId.Value);

                // need to track the refund
                originalTransaction.GetParticipationObjects().FirstOrDefault().AddPayment(
                    amount: -amount,
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
            if (checkoutObject.TuitionCostNoFees > 0)
                AccountService.ProcessTuitionWithStripe(checkoutObject, msgs, dc);

            // must wait until after charges happen to know what card id is charged for sure
            // add purchase
            if (msgs.HasNoErrors())
                AccountService.CreatePurchaseTransactionPayment(checkoutObject, loggedInUser, msgs, dc);
        }

        public static void ProcessStudentRegistration(CheckoutObject checkoutObject, bool sendConfirmationEmail, WebUser loggedInUser, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            Database.ExecuteTransaction
            (
                action: (SystemMessages amsgs, DatabaseConnection adc) =>
                {
                    checkoutObject.Registration.SetRegistrationStatus(
                        completionStatus: checkoutObject.ParticipationObject.RequiresParentalConsent ? 
                            RegistrationCompletionStatus.RequiresParentSignature : 
                            RegistrationCompletionStatus.Completed, 
                        loggedInUser: loggedInUser, 
                        msgs: amsgs, 
                        dc: adc);

                    // TODO: Allow user to register for multiple activities in one session at once MAYBE
                    RegistrationService.ProcessRegistrationAssignment(
                        checkoutObject: checkoutObject,
                        loggedInUser: loggedInUser,
                        msgs: amsgs,
                        dc: adc);
                },
                onSuccess: (SystemMessages smsgs) =>
                {
                    msgs.AddRange(smsgs);

                    if (sendConfirmationEmail)
                        checkoutObject.Registration.SendConfirmation(checkoutObject, null, loggedInUser, true, msgs, dc);

                },
                onFailure: (SystemMessages fmsgs) => msgs.AddRange(fmsgs),
                onException: (SystemMessages emsgs) => Helpers.Utilities.SystemErrorLog(emsgs),
                dc: dc
            );
        }

        public static void ProcessRegistrationWithoutPayment(CheckoutObject checkoutObject, bool sendConfirmationEmail, WebUser loggedInUser, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            Database.ExecuteTransaction
            (
                action: (SystemMessages amsgs, DatabaseConnection adc) =>
                {
                    // mark as completed
                    checkoutObject.Registration.SetRegistrationStatus(
                        completionStatus: RegistrationCompletionStatus.Completed,
                        loggedInUser: loggedInUser,
                        msgs: amsgs,
                        dc: adc);

                    // TODO: Allow user to register for multiple activities in one session at once MAYBE
                    RegistrationService.ProcessRegistrationAssignment(
                        checkoutObject: checkoutObject,
                        loggedInUser: loggedInUser,
                        msgs: amsgs,
                        dc: adc);
                },
                onSuccess: (SystemMessages smsgs) =>
                {
                    msgs.AddRange(smsgs);

                    if (sendConfirmationEmail)
                        checkoutObject.Registration.SendConfirmation(checkoutObject, null, loggedInUser, true, msgs, dc);

                },
                onFailure: (SystemMessages fmsgs) => msgs.AddRange(fmsgs),
                onException: (SystemMessages emsgs) => Helpers.Utilities.SystemErrorLog(emsgs),
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

                    // process payment by to account, includes registration fee
                    if (!checkoutObject.ParticipationObject.IsTuitionFree && checkoutObject.PaymentPlan.IsEmpty())
                        AccountService.ProcessTuitionFee(checkoutObject, loggedInUser, amsgs, adc);

                    // Process Subscriptions separately, don't charge all at once for the registration Fee

                    // TODO: Decide if master account registrant should be added to plans or the account copy
                    // get the registrant record that was created just for the account
                    var accountRegistrant = checkoutObject.Registration.GetRegistrant(amsgs, adc);

                    // if they have a subscription plan
                    // will also process registration fees
                    if (checkoutObject.SubscriptionPlan.IsNotEmpty())
                        accountRegistrant.StartSubscriptionPlan(
                            checkoutObject: checkoutObject,
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

                            coupon.CreateStripeCoupon(
                                checkoutObject.BillingAccount.StripeUserId,
                                StripeCouponDuration.Repeating,
                                checkoutObject.PaymentPlan.TotalPeriods,
                                discountValue.Value,
                                amsgs,
                                dc
                            );
                        }

                        accountRegistrant.StartPaymentPlan(
                            checkoutObject: checkoutObject,
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
                        checkoutObject.PayingAccount.MarkCouponAsUsed(checkoutObject.Coupon);

                    checkoutObject.Registration.SendConfirmation(checkoutObject, null, loggedInUser, true, msgs, dc);
                },
                onFailure: (SystemMessages amsgs) => msgs.AddRange(amsgs),
                onException: (SystemMessages emsgs) => Helpers.Utilities.SystemErrorLog(emsgs),
                dc: dc
            );

        }

        public static CheckPaymentAccount CreateOrUpdateCheckPaymentAccount(this Account account, LegalEntityType legalEntityType, string name, string address1, string address2, string city, string state, string postalCode, WebUser loggedInUser, SystemMessages msgs, DatabaseConnection dc = null)
        {
            CheckPaymentAccount checkAccount = null;

            Database.ExecuteTransaction
            (
                action: (SystemMessages amsgs, DatabaseConnection adc) =>
                {
                    checkAccount = account.GetCheckPaymentAccount(amsgs, adc);

                    if (checkAccount.IsEmpty())
                        checkAccount = new CheckPaymentAccount
                        {
                            CreateDate = DateTime.UtcNow,
                            CreatedBy = loggedInUser.ID,
                            AccountId = account.ID
                        };

                    checkAccount.LegalEntityTypeId = legalEntityType;
                    checkAccount.Name = name;
                    checkAccount.Address1 = address1;
                    checkAccount.Address2 = address2;
                    checkAccount.City = city;
                    checkAccount.State = state;
                    checkAccount.PostalCode = postalCode;
                    checkAccount.LastModifiedDate = DateTime.UtcNow;
                    checkAccount.LastModifiedBy = loggedInUser.ID;
                    checkAccount.Country = "US";
                    amsgs.AddRange(checkAccount.Save(adc));

                    // Check deliverability later
                    //if (msgs.HasNoErrors())
                    //{
                    //    if (!checkAccount.IsDeliverable(amsgs, adc))
                    //        msgs.Add("Your address is invalid and not deliverable. Please update your address to a valid address", MessageType.UserError);
                    //}
                },
                onSuccess: (SystemMessages smsgs) => msgs.AddRange(smsgs),
                onFailure: (SystemMessages fmsgs) => msgs.AddRange(fmsgs),
                onException: (SystemMessages emsgs) => Helpers.Utilities.SystemErrorLog(emsgs),
                dc: dc
            );

            return checkAccount;
        }

        public static CheckPaymentAccount GetCheckPaymentAccount(this Account account, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return CheckPaymentAccount.LoadByAccountId(account.ID.Value, msgs, dc);
        }

        public static BankPaymentAccount CreateOrUpdateBankPaymentAccount(this Account account, string bankName, BankAccountType bankAccountType, string routingNumber, string accountNumber, string accountHoldersName, string bankCountry, WebUser loggedInUser, SystemMessages msgs, DatabaseConnection dc = null)
        {
            BankPaymentAccount bankAccount = null;

            Database.ExecuteTransaction
            (
                action: (SystemMessages amsgs, DatabaseConnection adc) =>
                {
                    bankAccount = account.GetBankPaymentAccount(amsgs, adc);

                    if (bankAccount.IsEmpty())
                        bankAccount = new BankPaymentAccount
                        {
                            CreateDate = DateTime.UtcNow,
                            CreatedBy = loggedInUser.ID,
                            AccountId = account.ID
                        };

                    bankAccount.BankAccountTypeId = bankAccountType;
                    bankAccount.BankCountry = bankCountry;
                    bankAccount.RoutingNumber = routingNumber;
                    bankAccount.AccountNumber = accountNumber;
                    bankAccount.BankName = bankName;
                    bankAccount.AccountHoldersName = accountHoldersName;
                    amsgs.AddRange(bankAccount.Save(adc));

                    // TODO: need to validate bank info at some point
                },
                onSuccess: (SystemMessages smsgs) => msgs.AddRange(smsgs),
                onFailure: (SystemMessages fmsgs) => msgs.AddRange(fmsgs),
                onException: (SystemMessages emsgs) => Helpers.Utilities.SystemErrorLog(emsgs),
                dc: dc
            );

            return bankAccount;
        }

        public static BankPaymentAccount GetBankPaymentAccount(this Account account, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return BankPaymentAccount.LoadByAccountId(account.ID.Value, msgs, dc);
        }

        public static bool CanProcessPayments(this Account account, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var oldPaymentFeatures = new List<FeatureType>() { FeatureType.UseStripeConnectedAccount, FeatureType.AcceptPayments };

            if (account.HasAccountPackageFeatures(oldPaymentFeatures, msgs, dc))
                return account.StripeChargesEnabled;

            return true;
        }

        public static bool CanReceivePaymentDeposits(this Account account, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return account.CanReceiveACHDeposits(msgs, dc) || account.GetCheckPaymentAccount(msgs, dc).IsNotEmpty();
        }

        public static bool CanReceiveACHDeposits(this Account account, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return account.CanReceiveStripeDeposits(msgs, dc) || account.GetBankPaymentAccount(msgs, dc).IsNotEmpty();
        }

        public static bool CanReceiveStripeDeposits(this Account account, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return (account.HasStripeConnectedAccount && account.StripePayoutsEnabled && account.StripeChargesEnabled);
        }

        public static Stripe.Customer AddCustomerToConnectedAccount(this Account billingAccount, Account purchasingAccount, string stripeBillToSourceId, SystemMessages msgs)
        {
            // create the customer on the connected account
            return StripeHelper.CreateCustomer(
                 description: purchasingAccount.Name,
                 email: purchasingAccount.Email,
                 sourceId: stripeBillToSourceId,
                 stripeAccountId: billingAccount.StripeUserId,
                 msgs: msgs);
        }

        public static Stripe.Subscription CreateStripeSubscription(this Account billingAccount, string stripeCustomerId, SubscriptionPlan plan, string stripeBillToSourceId, string stripeCouponId, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return StripeHelper.CreateSubscription(
                customerId: stripeCustomerId,
                planId: plan.StripePlanId,
                sourceId: stripeBillToSourceId,
                platformFeePercent: billingAccount.GetSubscriptionPlatformFeePercent(plan, msgs, dc),
                startDate: DateTime.UtcNow,
                stripeAccountId: billingAccount.StripeUserId,
                stripeCouponId: stripeCouponId,
                metaData: null,
                msgs: msgs);
        }

        public static decimal GetTotalPayoutAmount(this Account account, PayoutStatus? payoutStatus, DateTime? startDate = null, DateTime? endDate = null, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return Payout.GetTotalAmountByAccountId(account.ID.Value, payoutStatus, startDate, endDate, msgs, dc);
        }

        public static decimal GetTotalAmountBilledOut(this Account account, DateTime? startDate = null, DateTime? endDate = null, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return Transaction.GetTotalAmountBilledByBillerAccount(account.ID.Value, startDate, endDate, msgs, dc);
        }

        public static decimal GetTotalFeesPaidAsBiller(this Account account, DateTime? startDate, DateTime? endDate, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return Transaction.GetTotalFeesPaidByBillerAccount(account.ID.Value, startDate, endDate, msgs, dc);
        }

        public static decimal GetTotalAmountRefundedTo(this Account account, DateTime? startDate = null, DateTime? endDate = null, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return Transaction.GetTotalAmountPaidByPayerAccount(account.ID.Value, startDate, endDate, msgs, dc);
        }

        public static decimal GetSubscriptionPlatformFeePercent(this Account billingAccount, SubscriptionPlan plan, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (plan.CostPerPeriod <= 0)
                return 0;

            decimal feeAmount = 0;
            decimal billedCostPerPeriod = plan.CostPerPeriod.Value;

            if (billingAccount.HasFeature(FeatureType.UseStripeConnectedAccount, msgs, dc))
            {
                var platformFeeAmount = billingAccount.CalculatePlatformFeeAmount(plan.CostPerPeriod.Value, msgs, dc);
                var stripeFeeAmount = plan.GetStripeFeeAmountWithConnectedAccount();

                if (platformFeeAmount > stripeFeeAmount)
                    feeAmount = platformFeeAmount - stripeFeeAmount;
                else
                    feeAmount = 0;
            }
            else
            {
                feeAmount = Platform.GetTotalFees(plan.CostPerPeriod.Value, billingAccount);

                if (!billingAccount.IsConvenienceFeeTakenOutOfCost)
                    billedCostPerPeriod += Platform.GetConvenienceFee(billedCostPerPeriod, billingAccount);

                if (!billingAccount.IsCreditCardFeeTakenOutOfCost)
                    billedCostPerPeriod += Platform.GetCreditCardFee(billedCostPerPeriod, billingAccount);

            }

            return Math.Round(feeAmount / billedCostPerPeriod, 4);
        }
        public static void SendStripePayout(this Account account, Payout payout, WebUser loggedInUser, SystemMessages msgs, DatabaseConnection dc = null)
        {
            var requestor = payout.GetRequestor(msgs, dc);

            if (account.StripeUserId.IsNotEmpty())
            {
                var metaData = new Dictionary<string, string>()
                {
                    { "AccountUid", account.Uid.ToString() },
                    { "RequestedBy", requestor.Uid.ToString() },
                    { "ApprovedBy", loggedInUser.Uid.ToString() },
                    { "DateRequested", payout.DateRequested.ToShortDateTimeString() }
                };

                StripeHelper.CreateTransfer(
                    amount: payout.Amount.Value,
                    description: $"Transfer from AHQ to {account.Name} requested by {requestor.DisplayName} and approved by {loggedInUser.DisplayName}",
                    transferIdentifier: payout.Uid.ToString(),
                    destinationId: account.StripeUserId,
                    metaData: metaData,
                    msgs: msgs);

                var stripeAccount = account.GetStripeCustomAccount(msgs);

                StripeHelper.CreatePayout(
                    amount: payout.Amount.Value,
                    externalAccountId: stripeAccount.ExternalAccounts.First().Id,
                    connectedAccountId: account.StripeUserId,
                    metaData: metaData,
                    msgs: msgs);
            }
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
                couponCode: CouponService.GenerateCouponCode(account.ID.Value, msgs, dc),
                label: $"Coupon - {account.NowInAccountTimeZone()}",
                description: "",
                loggedInUser: loggedInUser,
                msgs: msgs,
                dc: dc);

            if (coupon.DoesApplyToSubscriptionPlan())
                coupon.CreateStripeCoupon(
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

                    if (coupon.DoesApplyToSubscriptionPlan() && !participationObject.SubscriptionPlans.Any())
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
            return ActivityGroup.LoadByAccountId(account.ID.Value, ActivityGroupType.League, IsArchived, start, length, msgs, dc);
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

            if (aags.Any())
            {
                var aagIds = aags.Select(x => x.ID.Value).ToList();
                var periodAagIds = RegistrationPeriod.LoadByAccountActivityGroupIds(aagIds, msgs, dc)
                    .Where(period => 
                            period.IsOpen(account, msgs, dc) || 
                            period.WillOpen(account, msgs, dc) || 
                            period.IsRecentlyClosed(account, msgs, dc))
                    .Select(period => period.AccountActivityGroupId.Value)
                    .ToList();

                if (periodAagIds.Any())
                    return aags.Where(aag => periodAagIds.Contains(aag.ID.Value)).ToList();
            }

            return new List<AccountActivityGroup>();
        }

        public static int GetExternalLeagueCount(this Account account, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return AccountActivityGroup.GetCount(
                new Hashtable() {
                    { "AccountId", account.ID.Value },
                    { "ActivityGroupIsArchived", false }
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

        public static bool IsProviderForOtherAccounts(this Account account, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return Activity.GetCount(new Hashtable() { { "ProviderAccountId", account.ID.Value } }, msgs, dc) > 0;
        }

        public static bool IsNewProviderForOtherAccounts(this Account account, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return account.IsProviderForOtherAccounts(msgs, dc) && !account.CanReceivePaymentDeposits(msgs, dc);
        }

        #endregion

        #region Stats

        public static decimal GetTotalHoursEnrolled(this Account account, DateTime? startDateTime, DateTime? endDateTime, SystemMessages msgs, DatabaseConnection dc = null)
        {
            return Account.GetTotalMinutesEnrolled(account.ID.Value, startDateTime, endDateTime, msgs, dc) / 60;
        }

        public static int GetTotalSeatsOffered(this Account account, DateTime? startDateTime, DateTime? endDateTime, SystemMessages msgs, DatabaseConnection dc = null)
        {
            return Account.GetTotalSeatsOffered(account.ID.Value, startDateTime, endDateTime, msgs, dc) / 60;
        }

        public static decimal GetTotalHoursOffered(this Account account, DateTime? startDateTime, DateTime? endDateTime, SystemMessages msgs, DatabaseConnection dc = null)
        {
            return Account.GetTotalMinutesOffered(account.ID.Value, startDateTime, endDateTime, msgs, dc) / 60;
        }

        public static int GetTotalStudentsServed(this Account account, DateTime? startDateTime, DateTime? endDateTime, SystemMessages msgs, DatabaseConnection dc = null)
        {
            return Account.GetTotalStudentsServed(account.ID.Value, startDateTime, endDateTime, msgs, dc) / 60;
        }

        public static List<(string ActivityName, int RegistrationCount)> GetRegistrationCountByActivity(this Account account, DateTime? startDateTime, DateTime? endDateTime, SystemMessages msgs, DatabaseConnection dc = null)
        {
            return Account.GetRegistrationCountByActivity(account.ID.Value, startDateTime, endDateTime, msgs, dc);
        }

        public static List<(string Grade, int RegistrationCount)> GetRegistrationCountByGrade(this Account account, DateTime? startDateTime, DateTime? endDateTime, SystemMessages msgs, DatabaseConnection dc = null)
        {
            return Account.GetRegistrationCountByGrade(account.ID.Value, startDateTime, endDateTime, msgs, dc);
        }

        public static (decimal totalProcessed, decimal totalProfit) GetTotalProcessedAndProfit(this Account account, DateTime? startDateTime, DateTime? endDateTime, SystemMessages msgs, DatabaseConnection dc = null)
        {
            return Account.GetTotalProcessedAndProfit(account.ID.Value, startDateTime, endDateTime, msgs, dc);
        }

        #endregion

    }
}
