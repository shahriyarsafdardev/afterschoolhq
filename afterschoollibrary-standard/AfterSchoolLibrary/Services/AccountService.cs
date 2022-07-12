using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using AfterSchool.Models;
using AfterSchoolLibrary.Billing;
using AfterSchoolLibrary.Enums;
using AfterSchoolLibrary.Helpers;
using AfterSchoolLibrary.Models;
using BasicLibrary;
using BasicLibrary.DataAccess;
using Stripe;
using Account = AfterSchoolLibrary.Models.Account;
using Coupon = AfterSchoolLibrary.Models.Coupon;
using Payout = AfterSchoolLibrary.Models.Payout;
using Utilities = AfterSchoolLibrary.Helpers.Utilities;

namespace AfterSchoolLibrary.Services
{
    public static class AccountService
    {
        public static bool HasFeature(this Account account, FeatureType featureType, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            if (account.IsNotEmpty())
            {
                var ap = account.GetActiveAccountPackage(msgs, dc);

                if (ap.IsNotEmpty())
                    return (bool) ap.HasFeature(featureType, msgs, dc);
            }

            return false;
        }

        public static WebUser GetAccountOwner(this Account account, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            if (account.HasAccountOwner)
                return WebUser.Load(account.OwnerId.Value, msgs, dc);

            return null;
        }

        public static List<WebUser> GetAccountOwnersAndAdmins(this Account account, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            return AccountWebUser.LoadByAccountIdByIsActiveByIsDeleted(account.ID.Value, true, false, msgs, dc)
                .Where(x => x.UserTypeId == UserType.Administrator || x.UserTypeId == UserType.Owner)
                .Select(x => x.ToWebUser())
                .ToList();
        }

        public static List<Form> GetForms(this Account account, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            return Form.LoadByAccountId(account.ID.Value, msgs, dc);
        }

        public static List<Form> GetForms(this Account account, FormType formType, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            return Form.LoadByAccountIdByFormTypeId(account.ID.Value, formType, msgs, dc);
        }

        public static List<Form> GetForms(this Account account, FormType formType, string search, int? start, int? length, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            return Form.Search(account.ID.Value, formType, search, start, length, msgs, dc);
        }

        public static int GetFormCount(this Account account, FormType formType, string search, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            return Form.GetSearchCount(account.ID.Value, formType, search, msgs, dc);
        }

        public static List<Form> GetForms(this Account account, ActivityType activityType, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            var formType = activityType == ActivityType.VolunteerOpportunity ? FormType.VolunteerForm : FormType.StudentForm;
            return Form.LoadByAccountIdByFormTypeId(account.ID.Value, formType, msgs, dc);
        }

        public static List<Form> GetSurveyForms(this Account account, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            return Form.LoadByAccountIdByFormTypeId(account.ID.Value, FormType.Survey, msgs, dc);
        }

        public static Form GetContactForm(this Account account, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            if (account.ContactFormId.HasValue)
                return Form.Load(account.ContactFormId.Value, msgs, dc);

            return null;
        }

        public static Form GetForm(this Account account, int Id, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            if (account.IsNotEmpty())
                return Form.LoadByIdByAccountId(Id, account.ID.Value, msgs, dc);

            return null;
        }

        public static void MarkAsDeleted(this Account account, WebUser loggedInUser, SystemMessages msgs,
            DatabaseConnection dc = null)
        {
            account.IsDeleted = true;
            account.LastModifiedDate = DateTime.UtcNow;
            account.LastModifiedBy = loggedInUser?.ID.IfEmpty(0);
            msgs.AddRange(account.Save(dc));
        }

        public static void SetIsClaimed(this Account account, bool isClaimed, WebUser loggedInUser, SystemMessages msgs,
            DatabaseConnection dc = null)
        {
            account.IsClaimed = isClaimed;
            account.LastModifiedDate = DateTime.UtcNow;
            account.LastModifiedBy = loggedInUser?.ID.IfEmpty(0);
            msgs.AddRange(account.Save(dc));
        }

        public static void SetIsTest(this Account account, bool isTest, WebUser loggedInUser, SystemMessages msgs,
            DatabaseConnection dc = null)
        {
            account.IsTest = isTest;
            account.LastModifiedDate = DateTime.UtcNow;
            account.LastModifiedBy = loggedInUser?.ID.IfEmpty(0);
            msgs.AddRange(account.Save(dc));
        }

        public static void SetIsActive(this Account account, bool isActive, WebUser loggedInUser, SystemMessages msgs,
            DatabaseConnection dc = null)
        {
            account.IsActive = isActive;
            account.LastModifiedDate = DateTime.UtcNow;
            account.LastModifiedBy = loggedInUser?.ID.IfEmpty(0);
            msgs.AddRange(account.Save(dc));
        }

        #region SubscriptionPlans

        public static List<SubscriptionPlan> GetSubscriptionPlans(this Account account, SystemMessages msgs,
            DatabaseConnection dc = null)
        {
            return SubscriptionPlan.LoadByPlanOwnerAccountId(account.ID.Value, msgs);
        }

        #endregion

        #region Locations

        public static bool IsLocationOwner(this Account currentAccount, Activity activity, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            var location = activity?.GetLocation(msgs, dc);
            var hostAccount = location?.GetHostAccount(msgs, dc);

            // this will be used to check whether logged in account is location owner or not.
            if (hostAccount.IsNotEmpty() && hostAccount.HasAccountOwner && hostAccount.ID == currentAccount.ID)
                return true;

            return false;
        }

        public static bool CanPublishFreeActivity(this Account account, SystemMessages msgs, DatabaseConnection dc)
        {
            var freeActivityLimitPackageFeature =
                account.GetAccountPackageFeature(FeatureType.FreeActivityLimit, msgs, dc);
            var freeActivityLimit = freeActivityLimitPackageFeature?.Max.Value;
            var numberOfPublishedActivities = account.GetPublishedActivities(msgs, dc)?
                .Count(a => a.CostPerRegistrant == 0 &&
                            (!a.HasSchedule || a.GetSchedule(msgs, dc).EndDate >= DateTime.Today));

            return freeActivityLimit.IsNotEmpty() && (
                freeActivityLimit == int.MaxValue || freeActivityLimit > numberOfPublishedActivities);
        }

        #endregion


        #region ActivitySchedule

        public static ActivitySchedule GetActivitySchedule(this Account account, int Id, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            return ActivitySchedule.LoadByIdByAccountId(Id, account.ID.Value, msgs, dc);
        }

        public static List<ActivitySchedule> GetActivitySchedules(this Account account, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            return ActivitySchedule.LoadByAccountId(account.ID.Value, msgs, dc);
        }

        #endregion

        #region ActivityMessages

        public static ActivityMessage GetActivityMessage(this Account account, Guid Uid, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            if (account.IsNotEmpty())
                return ActivityMessage.LoadByUidByAccountId(Uid, account.ID.Value, msgs, dc);

            return null;
        }

        public static List<ActivityMessage> GetActivityMessages(this Account account, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            return ActivityMessage.LoadByAccountId(account.ID.Value, msgs, dc);
        }

        public static List<ActivityMessage> GetActivityMessages(this Account account, int? start, int? length,
            SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return ActivityMessage.LoadByAccountId(account.ID.Value, start, length, msgs, dc);
        }

        public static int GetActivityMessageCount(this Account account, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            return ActivityMessage.GetCountByAccountId(account.ID.Value, msgs, dc);
        }

        public static ActivityMessage AddMessage(this Account account, string subject, string body,
            List<int> registrantIds, bool sendToStaff, int loggedInUserId, SystemMessages msgs = null,
            DatabaseConnection dc = null)
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
                        CreatedBy = loggedInUserId,
                    };
                    amsgs.AddRange(am.Save(adc));

                    registrantIds.ForEach(registrantId => am.AddRegistrant(registrantId, loggedInUserId, amsgs, adc));
                },
                onSuccess: (SystemMessages smsgs) => msgs.AddRange(smsgs),
                onFailure: (SystemMessages fmsgs) => msgs.AddRange(fmsgs),
                onException: (SystemMessages emsgs) => Utilities.SystemErrorLog(emsgs),
                dc: dc
            );

            return am;
        }

        #endregion

        #region Activities

        public static Activity GetActivity(this Account account, int activityId, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            Activity activity = null;

            if (account.IsNotEmpty())
            {
                activity = Activity.LoadByIdByAccountId(activityId, account.ID.Value, msgs, dc);
                // If schools need to publish an activity for a provider who is external to the school page
                // I can see this activity if it is hosted at my location
                if (activity.IsEmpty())
                    activity = account.GetHostedActivity(activityId, msgs, dc);

                //if current account is external provider
                if (activity.IsEmpty())
                    activity = account.GetExternallyProvidedActivity(activityId, msgs, dc);
            }

            return activity;
        }

        public static Activity GetActivity(this Account account, Guid activityUid, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            Activity activity = null;

            if (account.IsNotEmpty())
            {
                activity = Activity.LoadByUidByAccountId(activityUid, account.ID.Value, msgs, dc);
                // If schools need to publish an activity for a provider who is external to the school page
                // I can see this activity if it is hosted at my location
                if (activity.IsEmpty())
                    activity = account.GetHostedActivity(activityUid, msgs, dc);

                //if current account is external provider
                if (activity.IsEmpty())
                    activity = account.GetExternallyProvidedActivity(activityUid, msgs, dc);
            }

            return activity;
        }

        public static Activity GetExternallyProvidedActivity(this Account account, Guid activityUid,
            SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return Activity.LoadByUidByProviderAccountId(activityUid, account.ID.Value, msgs, dc);
        }

        public static Activity GetExternallyProvidedActivity(this Account account, int activityId,
            SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var activity = Activity.Load(activityId, msgs, dc);

            if (activity.IsNotEmpty())
                return Activity.LoadByUidByProviderAccountId(activity.Uid, account.ID.Value, msgs, dc);

            return null;
        }

        public static Activity GetHostedActivity(this Account account, Guid activityUid, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            var activity = Activity.LoadByUid(activityUid, msgs, dc);

            if (activity.IsNotEmpty())
                return account.GetHostedActivity(activity, msgs, dc);

            return null;
        }

        public static Activity GetHostedActivity(this Account account, int activityId, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            var activity = Activity.Load(activityId, msgs, dc);

            if (activity.IsNotEmpty())
                return account.GetHostedActivity(activity, msgs, dc);

            return null;
        }

        public static Activity GetHostedActivity(this Account account, Activity tempActivity,
            SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            Activity hostedActivity = null;

            if (tempActivity.IsNotEmpty())
            {
                var locations = account.GetHostedLocations(msgs, dc);

                if (locations.IsNotEmpty())
                    locations.ForEach(location =>
                    {
                        if (tempActivity.LocationId == location.ID)
                            hostedActivity = tempActivity;
                    });
            }

            return hostedActivity;
        }

        public static List<Activity> GetActivities(this Account account, List<int> ActivityIds,
            SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (account.IsNotEmpty())
                return Activity.LoadListByIdsByAccountId(ActivityIds, account.ID.Value, msgs, dc);

            return null;
        }

        public static List<Activity> LoadByActivitiesbyActivityGroupByAccountId(this Account account,
            int activityGroupId, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (account.IsNotEmpty())
                return Activity.LoadByActivityGroupIdByAccountId(activityGroupId, account.ID.Value, msgs, dc);

            return null;
        }

        public static List<ParticipationObject> GetPublishedParticipationObjects(this Account account,
            SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var participationObjects = new List<ParticipationObject>();
            var topParticipationObjects = new List<ParticipationObject>();
            var bottomParticipationObjects = new List<ParticipationObject>();

            topParticipationObjects.AddRange(
                account.GetPublishedActivities(msgs, dc)
                    .Select(activity => activity.GetParticipationObject(msgs, dc))
            );

            topParticipationObjects.AddRange(
                account.GetPublishedExternalLeagues(msgs, dc).Select(x => x.GetParticipationObject(msgs, dc))
            );

            topParticipationObjects.AddRange(
                account.GetOpenPublishedSessions(msgs, dc).Select(x => x.GetParticipationObject(msgs, dc))
            );

            var hostedLocationIds = account.GetHostedLocations(msgs, dc)
                .Select(location => location.ID.Value)
                .ToList();

            account.GetApprovedVisitorAccounts(msgs, dc).ForEach(visitor =>
            {
                topParticipationObjects.AddRange(
                    visitor.GetPublishedActivitiesInLocations(hostedLocationIds, msgs, dc)
                        .Select(activity => activity.GetParticipationObject(msgs)));

                if (visitor.CanShowAllActivitiesOn(account))
                    bottomParticipationObjects.AddRange(visitor.GetPublishedOpenedActivities(false, msgs, dc)
                        .Select(activity => activity.GetParticipationObject(msgs, dc)));
            });

            participationObjects.AddRange(topParticipationObjects.Shuffle());
            participationObjects.AddRange(bottomParticipationObjects.Shuffle());

            return participationObjects
                .DistinctBy(participationObject => new {participationObject.ObjectID, participationObject.ObjectType})
                .ToList();
        }

        public static List<Activity> GetPublishedActivities(this Account account, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            var periods = account.GetOpenUpcomingRecentlyClosedRegistrationPeriods(msgs, dc);
            var activities = new List<Activity>();

            periods.ForEach(period =>
                activities.AddRange(period.GetActivities(msgs, dc).Where(x => x.IsPubliclyViewable)));

            activities.AddRange(
                Activity.LoadByProviderAccountId(account.ID.Value, msgs, dc)
                    .Where(activity => activity.IsOpenUpcomingOrRecentlyClosed(msgs, dc) && activity.IsPubliclyViewable)
            );

            return activities.DistinctBy(activity => activity.ID).ToList();
        }

        public static List<Activity> GetPublishedOpenedActivities(this Account account,
            bool getExternallyProvidedActivities = true, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var periods = account.GetOpenRegistrationPeriods(msgs, dc);
            var activities = new List<Activity>();

            periods.ForEach(period =>
                activities.AddRange(period.GetActivities(msgs, dc).Where(x => x.IsPubliclyViewable)));

            if (getExternallyProvidedActivities)
                activities.AddRange(Activity.LoadByProviderAccountId(account.ID.Value, msgs, dc)
                    .Where(activity => activity.IsOpen(msgs, dc) && activity.IsPubliclyViewable));

            return activities.DistinctBy(activity => activity.ID).ToList();
        }

        public static bool CanShowAllActivitiesOn(this Account visitorAccount, Account hostAccount,
            SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var ar = visitorAccount.GetRequestForAccount(hostAccount, msgs, dc);

            return ar.IsNotEmpty() &&
                   ar.ApprovalStatusId != ApprovalStatus.NotSet &&
                   ar.ApprovalStatusId != ApprovalStatus.Denied &&
                   !ar.RequiresPublishPermission &&
                   ar.ShowAllPrograms;
        }

        public static List<Activity> GetPublishedActivitiesInLocations(this Account account, List<int> locationIds,
            SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var activities = new List<Activity>();

            if (locationIds.Any())
            {
                var periods = account.GetOpenUpcomingRecentlyClosedRegistrationPeriods(msgs, dc);

                periods.ForEach(period =>
                    activities.AddRange(period.GetActivitiesInLocations(locationIds, msgs, dc)
                        .Where(x => x.IsPubliclyViewable))
                );
            }

            return activities.DistinctBy(x => x.ID.Value).ToList();
        }

        public static List<ActivityForm> GetActivityForms(this Account account, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            return ActivityForm.LoadByAccountId(account.ID.Value, msgs, dc);
        }

        public static List<Activity> GetActivities(this Account account, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            if (account.IsNotEmpty())
                return Activity.LoadByAccountId(account.ID.Value, msgs, dc);

            return null;
        }

        public static List<Activity> GetActivities(this Account account, UserType userType, WebUser user, string q,
            ActivityType? activityType, bool? isArchived, bool? isDeleted, bool? ungroupedOnly, int? start = null,
            int? length = null, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (user.IsEmpty() || userType.In(FormConstants.AdminUserTypes))
                return Activity.LoadByAccountId(account.ID.Value, null, null, activityType, isArchived, isDeleted,
                    ungroupedOnly, start, length, q, msgs, dc).ToList();


            var activityIdsByWebUser = ActivityWebUser.LoadByWebUserId(user.ID.Value, msgs, dc)
                .Select(a => a.ActivityId.Value).ToList();
            return Activity.LoadListByIdsByGroupByIsArchivedIsDeleted(activityIdsByWebUser, null, null, activityType,
                isArchived, isDeleted, ungroupedOnly, start, length, msgs, dc);
        }

        public static List<Activity> GetAllActivitiesForAccount(this Account account, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            return Activity.LoadByAccountId(account.ID.Value, msgs, dc);
        }

        public static int GetActivityCount(this Account account, UserType userType, WebUser user, string q,
            ActivityType? activityType, bool? isArchived, bool? isDeleted, bool? ungroupedOnly,
            SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (user.IsEmpty() || userType.In(FormConstants.AdminUserTypes))
                return Activity.GetCountByAccountId(account.ID.Value, q, null, activityType, isArchived, isDeleted,
                    ungroupedOnly, msgs, dc);

            return account.GetActivities(userType, user, q, activityType, isArchived, isDeleted, ungroupedOnly, null,
                null, msgs, dc).Count();
        }

        public static int GetActivityCount(this Account account, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            return Activity.GetCount(new Hashtable {{"AccountId", account.ID.Value}}, msgs, dc);
        }

        public static int GetExternallyProvidedActivityCount(this Account account, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            return Activity.GetCount(
                new Hashtable
                {
                    {"ProviderAccountId", account.ID.Value},
                    {"IsPublished", true},
                    {"IsDeleted", false},
                    {"isArchived", false},
                },
                msgs,
                dc);
        }

        public static List<Activity> GetExternallyProvidedActivities(this Account account, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            return Activity.Load(
                new Hashtable
                {
                    {"ProviderAccountId", account.ID.Value},
                    {"IsPublished", true},
                    {"IsDeleted", false},
                    {"isArchived", false},
                },
                null,
                null,
                msgs,
                dc);
        }

        public static int GetActiveAndPublishedActivityCount(this Account account, bool includeExternallyPublished,
            SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var count = Activity.GetCount(new Hashtable
            {
                {"AccountId", account.ID.Value},
                {"IsPublished", true},
                {"IsDeleted", false},
                {"isArchived", false},
            }, msgs, dc);

            if (includeExternallyPublished)
                count += Activity.GetCountByProviderAccountId(account.ID.Value, msgs, dc);

            return count;
        }

        public static int GetActiveAndNonPublishedActivityCount(this Account account, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            return Activity.GetCount(new Hashtable
            {
                {"AccountId", account.ID.Value},
                {"IsPublished", false},
                {"IsDeleted", false},
                {"isArchived", false},
            }, msgs, dc) + Activity.GetCountByProviderAccountId(account.ID.Value, msgs, dc);
        }

        public static int GetActiveActivityCount(this Account account, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            return Activity.GetCount(new Hashtable
            {
                {"AccountId", account.ID.Value},
                {"IsDeleted", false},
                {"isArchived", false},
            }, msgs, dc) + Activity.GetCountByProviderAccountId(account.ID.Value, msgs, dc);
        }

        public static List<Activity> GetAllActiveAndNonPublishedActivities(this Account account,
            SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return Activity.Load(new Hashtable
            {
                {"AccountId", account.ID.Value},
                {"IsPublished", false},
                {"IsDeleted", false},
                {"isArchived", false},
            }, null, null, msgs, dc);
        }

        public static Activity GetUnpublishActivity(this Account account, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            return Activity.LoadByAccountIdByIsPublished(account.ID.Value, false, msgs, dc).FirstOrDefault();
        }

        public static bool HasActivities(this Account account, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return account.GetActivityCount(msgs, dc) > 0 || account.GetExternalLeagueCount(msgs, dc) > 0 ||
                   account.GetExternallyProvidedActivityCount(msgs, dc) > 0;
        }

        public static bool HasActiveActivities(this Account account, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            return account.GetActiveActivityCount(msgs, dc) > 0;
        }

        public static bool HasPaidActivities(this Account account, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            return account.HasActivities(msgs, dc) &&
                   (account.GetActivities(msgs, dc).Any(activity => !activity.GetIsFree(msgs, dc)) ||
                    account.GetExternallyProvidedActivities(msgs, dc).Any(activity => !activity.GetIsFree(msgs, dc)));
            // return account.GetAllActiveAndNonPublishedActivities(msgs, dc).Any(activity => !activity.IsFree);
        }

        public static bool IsOnboardingCompleted(this Account account, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            if (account.IsEmpty())
                return false;

            var accountOwner = account.GetAccountOwner(msgs);
            return account.HasAccountProfile(msgs) && account.HasActiveActivities(msgs) && accountOwner.IsNotEmpty() &&
                   accountOwner.VerifiedAccount && account.CanProcessPayments();
        }

        public static GettingStartedOptions OnboardingCompletionStatus(this Account account, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            if (account.IsEmpty())
                return GettingStartedOptions.NotSet;

            var accountOwner = account.GetAccountOwner(msgs);
            var HasAccountProfile = account.HasAccountProfile(msgs);
            var HasActivities = account.HasActiveActivities(msgs);
            var activitycount = account.GetActiveActivityCount(msgs);
            var VerifiedAccount = accountOwner.IsNotEmpty() && accountOwner.VerifiedAccount;
            var HasPaymentConfigured = account.CanProcessPayments(msgs);

            if (!HasAccountProfile)
                return GettingStartedOptions.CreateProfile;

            else if (!HasActivities || activitycount < 1)
                return GettingStartedOptions.CreateFirstActivity;

            else if (!VerifiedAccount)
                return GettingStartedOptions.ValidateEmail;

            else if (activitycount >= 1 && !HasActivities)
                return GettingStartedOptions.PublishActivity;

            else if (!HasPaymentConfigured)
                return GettingStartedOptions.AcceptPayments;

            else if (HasPaymentConfigured)
                return GettingStartedOptions.ProcessCompleted;

            else
                return GettingStartedOptions.NotSet;
        }

        public static List<GettingStartedOptions> GetCurrentOnboardingProcess(this Account account,
            SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var startingOptions = new List<GettingStartedOptions>();

            if (account.IsEmpty())
                return startingOptions;

            var accountOwner = account.GetAccountOwner(msgs);

            if (account.HasAccountProfile(msgs))
                startingOptions.Add(GettingStartedOptions.CreateProfile);

            if (account.HasActiveActivities(msgs))
                startingOptions.Add(GettingStartedOptions.CreateFirstActivity);

            if (accountOwner.IsNotEmpty() && accountOwner.VerifiedAccount)
                startingOptions.Add(GettingStartedOptions.ValidateEmail);

            if (account.CanProcessPayments(msgs))
                startingOptions.Add(GettingStartedOptions.AcceptPayments);

            return startingOptions;
        }

        public static bool HasActivityPermissions(this Account account, Activity activity, SystemMessages msgs,
            DatabaseConnection dc = null)
        {
            if (account.IsNotEmpty() && activity.IsNotEmpty())
            {
                if (account.ID == activity.AccountId || account.ID == activity.GetLocationOwner(msgs, dc)?.ID)
                    return true;
            }

            msgs.Add("Account does not have permission to edit activity", MessageType.UserError);
            return false;
        }

        #endregion

        #region Contacts

        public static int SearchContactCount(this Account account, string searchtext, int? contactListId,
            DatabaseConnection dc = null)
        {
            return Registrant.SearchRegistrantCount(searchtext, contactListId, account.ID.Value, dc: dc);
        }

        public static List<Registrant> SearchContacts(this Account account, string searchtext, int? contactListId,
            int? start = null, int? length = null, DatabaseConnection dc = null)
        {
            return Registrant.SearchRegistrants(searchtext, contactListId, account.ID.Value, start, length, dc: dc);
        }

        public static int GetContactCount(this Account account, DatabaseConnection dc = null)
        {
            return Registrant.GetCountByAccountId(account.ID.Value, dc: dc);
        }

        public static List<Registrant> GetContacts(this Account account, int? start = null, int? length = null,
            DatabaseConnection dc = null)
        {
            return Registrant.LoadByAccountIdByIsDeleted(account.ID.Value, false, start, length, dc: dc);
        }

        public static List<ContactList> GetContactLists(this Account account, int? start = null, int? length = null,
            DatabaseConnection dc = null)
        {
            return ContactList.LoadByAccountId(account.ID.Value, start, length, dc: dc);
        }

        public static int GetContactListCount(this Account account, DatabaseConnection dc = null)
        {
            return ContactList.GetCount(criterias: new Hashtable {{"AccountId", account.ID.Value}}, dc: dc);
        }

        public static bool DoesContactExist(this Account account, string firstName, string lastName, string email,
            SystemMessages msgs, DatabaseConnection dc = null)
        {
            return Registrant.FindRelatedRegistrants(
                account.ID.Value,
                firstName.IfEmpty("").Trim(),
                lastName.IfEmpty("").Trim(),
                email.IfEmpty("").Trim(),
                msgs,
                dc).Any();
        }

        public static Registrant AddContact(this Account account, string firstName, string lastName, string email,
            string phone, WebUser loggedInUser, SystemMessages msgs, DatabaseConnection dc = null)
        {
            return account.AddContact(firstName, lastName, email, phone, false, loggedInUser, msgs, dc);
        }

        public static Registrant AddContact(this Account account, string firstName, string lastName, string email,
            string phone, bool allowDuplicates, WebUser loggedInUser, SystemMessages msgs, DatabaseConnection dc = null)
        {
            return RegistrantService.CreateNewContact(
                account,
                firstName,
                lastName,
                email,
                phone,
                allowDuplicates,
                loggedInUser,
                msgs,
                dc);
        }

        #endregion

        #region Sessions

        public static ActivityGroup GetActivityGroup(this Account account, Guid Uid, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            if (account.IsNotEmpty())
                return ActivityGroup.LoadByUidByAccountId(Uid, account.ID.Value, msgs, dc);

            return null;
        }

        public static List<ActivityGroup> GetActivityGroups(this Account account, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            return ActivityGroup.LoadByAccountId(account.ID.Value, msgs, dc);
        }

        public static List<ActivityGroup> GetActivityGroups(this Account account, List<int> activityGroupIds,
            SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (account.IsNotEmpty())
                return ActivityGroup.LoadListByIdsByAccountId(activityGroupIds, account.ID.Value, msgs, dc);

            return null;
        }

        public static List<ActivityGroup> GetSessions(this Account account, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            return ActivityGroup.LoadByAccountIdByActivityGroupTypeId(account.ID.Value, ActivityGroupType.Session, msgs,
                dc);
        }

        public static List<ActivityGroup> GetOpenPublishedSessions(this Account account, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            var groups =
                ActivityGroup.LoadByAccountIdByIsPublishedByIsArchivedByIsDeleted(account.ID.Value, true, false, false,
                    msgs, dc);

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

            return groups
                .Where(group => periods.Select(period => period.ActivityGroupId.Value).Contains(group.ID.Value))
                .ToList();
        }

        public static List<ActivityGroup> GetSessions(this Account account, bool IsArchived, int? start, int? length,
            bool IsDeleted = false, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return ActivityGroup.LoadByAccountId(account.ID.Value, ActivityGroupType.Session, IsArchived, start, length,
                IsDeleted, msgs, dc);
        }

        public static int GetSessionCount(this Account account, bool IsArchived = false, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            return ActivityGroup.GetCount(
                new Hashtable
                {
                    {"AccountId", account.ID.Value},
                    {"ActivityGroupTypeId", (int) ActivityGroupType.Session},
                    {"IsArchived", IsArchived},
                    {"IsDeleted", false},
                },
                msgs: msgs,
                dc: dc);
        }

        #endregion

        #region RegistrationPeriods

        public static List<RegistrationPeriod> GetRegistrationPeriods(this Account account, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            return RegistrationPeriod.LoadByAccountId(account.ID.Value, msgs, dc);
        }

        public static RegistrationPeriod GetRegistrationPeriod(this Account account, int id, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            return RegistrationPeriod.LoadByIdByAccountId(id, account.ID.Value, msgs, dc);
        }

        public static bool IsNowEnrolling(this Account account, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            return account.GetPublishedParticipationObjects(msgs, dc)
                .Any(participationObject => participationObject.GetCurrentRegistrationPeriod(msgs, dc).IsNotEmpty());
        }

        public static List<RegistrationPeriod> GetRegistrationPeriods(this Account account, int? start, int? length,
            SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return RegistrationPeriod.LoadByAccountId(account.ID.Value, start, length, msgs, dc);
        }

        public static int GetRegistrationPeriodCount(this Account account, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            return RegistrationPeriod.GetCount(new Hashtable {{"AccountId", account.ID.Value}}, msgs, dc);
        }

        public static List<RegistrationPeriod> GetRegistrationPeriodsByGroupId(this Account account, int id,
            SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (account.IsNotEmpty())
                return RegistrationPeriod.LoadByActivityGroupIdByAccountId(id, account.ID.Value, msgs, dc);

            return null;
        }

        public static List<RegistrationPeriod> GetUpcomingRegistrationPeriods(this Account account,
            SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var now = account.ConvertToAccountTimeZone(DateTime.UtcNow);
            return account.GetRegistrationPeriods(msgs, dc)
                .Where(x => x.StartDateTime.Value.IsAfter(now))
                .OrderBy(x => x.StartDateTime.Value)
                .ToList();
        }

        public static List<RegistrationPeriod> GetOpenRegistrationPeriods(this Account account,
            SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var now = account.ConvertToAccountTimeZone(DateTime.UtcNow);
            return account.GetRegistrationPeriods(msgs, dc)
                .Where(x =>
                    x.StartDateTime.Value.IsOnOrBefore(now) &&
                    x.EndDateTime.Value.IsOnOrAfter(now)
                )
                .ToList();
        }

        public static List<RegistrationPeriod> GetClosedRegistrationPeriods(this Account account,
            SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var now = account.ConvertToAccountTimeZone(DateTime.UtcNow);
            return account.GetRegistrationPeriods(msgs, dc)
                .Where(x => x.EndDateTime.Value.IsBefore(now))
                .ToList();
        }

        public static List<RegistrationPeriod> GetRecentlyClosedRegistrationPeriods(this Account account,
            SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var now = account.ConvertToAccountTimeZone(DateTime.UtcNow);
            return account.GetRegistrationPeriods(msgs, dc)
                .Where(period => period.EndDateTime.Value.AddWeeks(8).IsOnOrAfter(now))
                .ToList();
        }

        public static List<RegistrationPeriod> GetOpenAndUpcomingRegistrationPeriods(this Account account,
            SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var periods = account.GetOpenRegistrationPeriods(msgs, dc);
            periods.AddRange(account.GetUpcomingRegistrationPeriods(msgs, dc));
            return periods;
        }

        public static List<RegistrationPeriod> GetOpenUpcomingRecentlyClosedRegistrationPeriods(this Account account,
            SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var periods = account.GetOpenRegistrationPeriods(msgs, dc);
            periods.AddRange(account.GetUpcomingRegistrationPeriods(msgs, dc));
            periods.AddRange(account.GetRecentlyClosedRegistrationPeriods(msgs, dc));
            return periods.DistinctBy(x => x.ID).ToList();
        }

        #endregion

        #region AccountPackage

        public static AccountPackage GetAccountPackage(this Account account, Guid Uid, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            if (account.IsNotEmpty())
                return AccountPackage.LoadByUidByAccountId(Uid, account.ID.Value, msgs, dc);

            return null;
        }

        public static AccountPackage GetActiveAccountPackage(this Account account, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            if (account.IsNotEmpty())
                return AccountPackage.LoadFirstFromListByAccountIdByIsActiveByOrderProperty(account.ID.Value, true,
                    "CreateDate", OrderByDirectionEnum.Descending, msgs, dc);

            return null;
        }

        public static bool IsActiveAccountPackage(this Account account, Package package, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            return account.IsActiveAccountPackage(package.ID, msgs, dc);
        }

        public static bool IsActiveAccountPackage(this Account account, int? packageId, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            if (account.IsNotEmpty())
                return account.GetActiveAccountPackage(msgs, dc)?.PackageId == packageId;

            return false;
        }

        public static List<AccountPackage> GetAccountPackages(this Account account, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            return AccountPackage.LoadByAccountId(account.ID.Value, msgs, dc);
        }

        public static bool HasAccountPackage(this Account account, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            return AccountPackage.GetCount(new Hashtable {{"AccountId", account.ID.Value}}, msgs, dc) > 0;
        }

        public static List<AccountPackageFeature> GetAccountPackageFeatures(this Account account,
            SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var accountPackage = account.GetActiveAccountPackage(msgs, dc);

            if (accountPackage.IsNotEmpty())
                return AccountPackageFeature.LoadByAccountPackageId(accountPackage.ID.Value, msgs, dc);

            return new List<AccountPackageFeature>();
        }

        public static AccountPackageFeature GetAccountPackageFeature(this Account account, FeatureType featureType,
            SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var accountPackage = account.GetActiveAccountPackage(msgs, dc);

            if (accountPackage.IsNotEmpty())
                return AccountPackageFeature.LoadByAccountPackageIdByFeatureTypeId(accountPackage.ID.Value, featureType,
                    msgs, dc);

            return null;
        }

        public static bool HasAccountPackageFeature(this Account account, FeatureType featureType,
            SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var accountPackage = account.GetActiveAccountPackage(msgs, dc);

            if (accountPackage.IsNotEmpty())
                return AccountPackageFeature.HasAccountPackageByFeatureTypeId(accountPackage.ID.Value, featureType,
                    msgs, dc);

            return false;
        }

        public static bool HasAccountPackageFeatures(this Account account, List<FeatureType> featureTypes,
            SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var accountPackage = account.GetActiveAccountPackage(msgs, dc);

            if (accountPackage.IsNotEmpty())
                return AccountPackageFeature.HasAccountPackageByFeatureTypeIds(accountPackage.ID.Value, featureTypes,
                    msgs, dc);

            return false;
        }

        public static AccountPackageFeature AddOrUpdatePackageFeature(this Account account, FeatureType featureTypeId,
            decimal maxValue, WebUser loggedInUser, SystemMessages msgs, DatabaseConnection dc = null)
        {
            return account.GetActiveAccountPackage(msgs, dc)
                .AddOrUpdatePackageFeature(featureTypeId, maxValue, loggedInUser, msgs, dc);
        }

        /// <summary>
        ///     Load all accounts with ProviderAccountTypes which has feature
        /// </summary>
        /// <param name="featureTypeId"></param>
        /// <param name="msgs"></param>
        /// <param name="dc"></param>
        /// <returns></returns>
        public static List<Account> GetAccountsWithSpecificFeatures(FeatureType featureTypeId, SystemMessages msgs,
            DatabaseConnection dc = null)
        {
            //.. load all AccountPackageFeature where provided FeatureTypeId has value >= 1 and then grab the AccountPackageId
            var accountPackageIds = AccountPackageFeature
                .LoadByFeatureTypeIdByGreaterOrEqualMaxValue(FeatureTypeId: featureTypeId, Max: 1, msgs: msgs, dc: dc)
                .Select(m => m.AccountPackageId.Value).ToList();

            if (accountPackageIds.Any())
            {
                //.. load all AccountPackage and get AccountId
                var accountIds = AccountPackage.LoadByIds(ids: accountPackageIds, msgs: msgs, dc: dc)
                    .Select(m => m.AccountId.Value).ToList();

                if (accountIds.Any())
                {
                    // return all accounts which are filtered by feature type
                    return Account.LoadByIdsByAccountTypes(ids: accountIds.Distinct().ToList(),
                        accountTypes: FormConstants.ProviderAccountTypes.ToList(), msgs: msgs, dc: dc).ToList();
                }
            }

            return null;
        }

        #endregion

        #region Users

        public static int GetMemberCount(this Account account, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return AccountWebUser.GetCount(
                new Hashtable
                {
                    {"AccountId", account.ID},
                    {"IsDeleted", false},
                    {"IsGhostAccount", false},
                },
                msgs,
                dc);
        }

        public static int GetActiveMemberCount(this Account account, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            return AccountWebUser.GetCount(
                new Hashtable
                {
                    {"AccountId", account.ID},
                    {"IsActive", true},
                    {"IsDeleted", false},
                    {"IsGhostAccount", false},
                },
                msgs,
                dc);
        }

        public static List<WebUser> GetUsers(this Account account, int? start = null, int? length = null,
            SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return account.GetMembers(start, length, msgs, dc)
                .Select(x => x.ToWebUser())
                .ToList();
        }

        public static AccountWebUser GetAccountWebUser(this Account account, int webUserId, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            if (account.IsNotEmpty())
                return AccountWebUser.LoadByAccountIdByWebUserId(account.ID.Value, webUserId, msgs, dc);

            return null;
        }

        public static List<AccountWebUser> GetMembers(this Account account, int? start = null, int? length = null,
            SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return AccountWebUser.LoadByAccountId(account.ID.Value, false, false, start, length, msgs, dc);
        }

        public static List<AccountWebUser> GetActiveMembers(this Account account, int? start = null, int? length = null,
            SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return AccountWebUser.LoadByAccountId(account.ID.Value, true, false, false, start, length, msgs, dc);
        }

        public static AccountWebUser AddUser(this Account account, WebUser user, UserType ut, WebUser loggedInUser,
            SystemMessages msgs, DatabaseConnection dc = null)
        {
            return AddUser(account, user, ut, false, loggedInUser, msgs, dc);
        }

        public static AccountWebUser AddGhostUser(this Account account, WebUser user, UserType ut, WebUser loggedInUser,
            SystemMessages msgs, DatabaseConnection dc = null)
        {
            return AddUser(account, user, ut, true, loggedInUser, msgs, dc);
        }

        public static AccountWebUser AddUser(this Account account, WebUser user, UserType ut, bool isGhostAccount,
            WebUser loggedInUser, SystemMessages msgs, DatabaseConnection dc = null)
        {
            AccountWebUser awu = null;

            Database.ExecuteTransaction
            (
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
                            IsActive = user.IsActive,
                            IsGhostAccount = isGhostAccount,
                        };
                    else
                    {
                        awu.IsActive = true;
                        awu.IsDeleted = false;
                    }

                    awu.LastModifiedDate = DateTime.UtcNow;
                    awu.LastModifiedBy = loggedInUser?.ID.IfEmpty(0);
                    awu.UserTypeId = ut;
                    amsgs.AddRange(awu.Save(adc));

                    if (ut == UserType.Owner && account.OwnerId != user.ID && user.IsActive)
                    {
                        account.OwnerId = user.ID.Value;
                        amsgs.AddRange(account.Save(adc));
                    }
                },
                onSuccess: (SystemMessages smsgs) => msgs.AddRange(smsgs),
                onFailure: (SystemMessages fmsgs) => msgs.AddRange(fmsgs),
                onException: (SystemMessages emsgs) => Utilities.SystemErrorLog(emsgs),
                dc: dc
            );

            return awu;
        }

        public static WebUser AddUser(this Account account, string firstName, string lastName, string email,
            UserType userType, WebUser loggedInUser, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return account.AddUser(firstName, lastName, email, null, null, false, true, false, userType, true,
                loggedInUser, msgs, dc: dc);
        }

        public static WebUser AddUser(this Account account, string firstName, string lastName, string email,
            string phone, UserType userType, WebUser loggedInUser, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            return account.AddUser(firstName, lastName, email, phone, null, false, false, false, userType, true,
                loggedInUser, msgs, dc: dc);
        }

        public static WebUser AddUser(this Account account, string firstName, string lastName, string email,
            string phone, UserType userType, bool canUseExistingUserAccount, WebUser loggedInUser,
            SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return account.AddUser(firstName, lastName, email, phone, null, false, false, false, userType,
                canUseExistingUserAccount, loggedInUser, msgs, dc: dc);
        }

        public static WebUser AddUser(this Account account, string firstName, string lastName, string email,
            string phone, string password, bool isActive, bool isRegistered, bool isVerified, UserType userType,
            bool canUseExistingUserAccount, WebUser loggedInUser, SystemMessages msgs = null,
            DatabaseConnection dc = null)
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
                                Username = WebUserService.GenerateUsername(firstName, lastName, amsgs, adc),
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
                    onException: (SystemMessages emsgs) => Utilities.SystemErrorLog(emsgs),
                    dc: dc
                );

            if (msgs.HasNoErrors())
                AddUser(account, user, userType, loggedInUser, msgs, dc);

            return user;
        }

        public static WebUser CreateAndInviteUser(this Account account, string firstName, string lastName, string email,
            string phone, string message, UserType userType, WebUser loggedInUser, SystemMessages msgs = null,
            DatabaseConnection dc = null)
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
                msgs.Add($"An invitation was not sent to {email.Trim()}, a system error has occurred",
                    MessageType.UserError);

            return user;
        }

        public static WebUser InviteUser(this Account account, string firstName, string lastName, string email,
            string phone, string message, UserType userType, WebUser loggedInUser, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            return InviteUser(account, firstName, lastName, email, phone, message, userType, true, loggedInUser, msgs,
                dc);
        }

        public static WebUser InviteUser(this Account account, string firstName, string lastName, string email,
            string phone, string message, UserType userType, bool canUseExistingUserAccount, WebUser loggedInUser,
            SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            var user = account.AddUser(firstName, lastName, email, phone, userType, canUseExistingUserAccount,
                loggedInUser, msgs, dc);

            if (msgs.HasNoErrors())
                msgs.AddRange(account.SendInvitedEmail(user, loggedInUser, message, dc));

            if (msgs.HasErrors())
                msgs.Add($"An invitation was not sent to {email.Trim()}, a system error has occurred",
                    MessageType.UserError);

            return user;
        }

        public static WebUser InviteParentUser(this Account account, string firstName, string lastName, string email,
            string phone, string message, UserType userType, bool canUseExistingUserAccount, WebUser loggedInUser,
            SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            var user = account.AddUser(firstName, lastName, email, phone, userType, canUseExistingUserAccount,
                loggedInUser, msgs, dc);

            if (msgs.HasNoErrors())
                msgs.AddRange(EmailHelper.SendParentAccountInvitedEmail(user.FirstName, loggedInUser.FirstName,
                    user.GetInviteUrl(account), message, email));

            if (msgs.HasErrors())
                msgs.Add($"An invitation was not sent to {email.Trim()}, a system error has occurred",
                    MessageType.UserError);
            return user;
        }

        public static bool HasTreasurer(this Account account, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return account.HasUserType(UserType.Treasurer, msgs, dc);
        }

        public static bool HasUserType(this Account account, UserType userType, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            return AccountWebUser.GetCount(
                new Hashtable {{"AccountId", account.ID.Value}, {"UserTypeId", (int) userType}}, msgs, dc) > 0;
        }

        #endregion

        #region Emails

        public static SystemMessages SendWelcomeEmailTrial(this Account account, DatabaseConnection dc = null)
        {
            return EmailHelper.SendWelcomeTrial(account.GetAccountOwner(dc: dc), account);
        }

        public static SystemMessages SendSubscriptionPaymentSucceeded(this Account account,
            RegistrantSubscriptionPlan rsp, SubscriptionPlan plan, DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();
            var accountOwner = account.GetAccountOwner(msgs, dc);
            msgs.AddRange(EmailHelper.SendSubscriptionPaymentSucceeded(accountOwner, rsp, plan));
            return msgs;
        }

        public static SystemMessages SendSubscriptionPaymentFailed(this Account account, SubscriptionPlan plan,
            DatabaseConnection dc)
        {
            var msgs = new SystemMessages();
            var accountOwner = account.GetAccountOwner(msgs, dc);
            msgs.AddRange(
                EmailHelper.SendSubscriptionPaymentFailed(account, accountOwner, plan, account.IsAfterSchool));
            return msgs;
        }

        public static SystemMessages SendStripeAccountVerified(this Account account, DatabaseConnection dc)
        {
            var msgs = new SystemMessages();
            var accountOwner = account.GetAccountOwner(msgs, dc);
            msgs.AddRange(EmailHelper.SendStripeAccountVerified(account, accountOwner, account.IsAfterSchool));
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
            return EmailHelper.SendWelcomePurchase(accountOwner, account);
        }

        public static SystemMessages SendInvitedEmail(this Account invitedAccount, WebUser invitedUser,
            WebUser loggedInUser, string customMessage = null, DatabaseConnection dc = null)
        {
            return EmailHelper.SendAccountInvitedEmail(invitedUser, invitedAccount, loggedInUser, customMessage);
        }

        #endregion

        #region Stripe

        public static void CompleteStripeConnect(this Account account, string code, WebUser loggedInUser,
            SystemMessages msgs, DatabaseConnection dc = null)
        {
            var stripeHelper = StripeHelper.GetStripeHelperConnectAccount();
            var token = stripeHelper.CompleteOAuthConnection(code, msgs);

            account.StripePublishableKey = null;
            account.StripeAccessToken = null;
            account.StripeRefreshToken = null;
            account.StripeUserId = token.StripeUserId;
            account.StripeChargesEnabled = true;
            account.StripePayoutsEnabled = true;
            account.LastModifiedBy = loggedInUser.ID;
            account.LastModifiedDate = DateTime.UtcNow;
            msgs.AddRange(account.Save(dc));

            account.AddOrUpdatePackageFeature(FeatureType.UseStripeConnectedAccount, 1, loggedInUser, msgs, dc);

            if (msgs.HasNoErrors())
            {
                msgs.Add("Your account has been connected to Stripe", MessageType.Success);

                // set payout type to manual for this account if it has package with ManualStripePayouts
                var isManualStripeEnabled =
                    account.GetAccountPackageFeature(featureType: FeatureType.ManualStripePayouts, msgs: msgs, dc: dc);

                if (isManualStripeEnabled.IsNotEmpty() && isManualStripeEnabled.Max > 0 &&
                    account.StripeUserId.IsNotEmpty())
                    stripeHelper.ToggleStripePayoutForConnectedAccount(customAccountsStripeUserId: account.StripeUserId,
                        accountName: account.Name, payoutIntervalType: PayoutIntervalType.Manual, msgs: msgs);
            }
            else
                account.DisconnectStripeConnect(loggedInUser, msgs, dc);
        }

        public static void DisconnectStripeConnect(this Account account, WebUser loggedInUser, SystemMessages msgs,
            DatabaseConnection dc = null)
        {
            account.StripeUserId = null;
            account.StripePublishableKey = null;
            account.StripeAccessToken = null;
            account.StripeRefreshToken = null;
            account.StripeSecretKey = null;
            account.StripeChargesEnabled = false;
            account.StripePayoutsEnabled = false;
            account.LastModifiedBy = loggedInUser.ID;
            account.LastModifiedDate = DateTime.UtcNow;
            msgs.AddRange(account.Save(dc));

            if (account.StripeUserId.IsNotEmpty())
                StripeHelper.GetStripeHelperConnectAccount().DeauthorizeOAuthConnection(account.StripeUserId, msgs);
        }

        public static SystemMessages SaveWithStripe(this Account account, StripeAccountType stripeAccountType, DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();

            msgs.AddRange(account.Save(dc: dc));

            if (msgs.HasNoErrors())
                account.CreateStripeCustomer(stripeAccountType, msgs, dc);

            return msgs;
        }

        public static void UpdateStripeChargesAndPayouts(this Account account, Stripe.Account stripeAccount,
            SystemMessages msgs, DatabaseConnection dc = null)
        {
            account.StripeChargesEnabled = stripeAccount.ChargesEnabled;
            account.StripePayoutsEnabled = stripeAccount.PayoutsEnabled;
            msgs.AddRange(account.Save(dc));
        }

        public static Stripe.Account GetStripeConnectedAccount(this Account account, SystemMessages msgs)
        {
            if (account.HasStripeConnectedAccount)
                return StripeHelper.GetStripeHelperConnectAccount().GetConnectedAccount(account.StripeUserId, msgs);

            return null;
        }

        public static Customer GetStripeCustomer(this Account account, StripeAccountType stripeAccountType, SystemMessages msgs)
        {
            if (account.StripeCustomerId.IsNotEmpty())
                return StripeHelper.GetStripeHelper(stripeAccountType).GetCustomer(account.StripeCustomerId, null, msgs);

            return null;
        }

        private static void SetStripeCustomer(this Account account, Customer customer, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            account.StripeCustomerId = customer.Id;
            msgs.AddRange(account.Save(dc: dc));
        }

        public static Customer CreateStripeCustomer(this Account account, StripeAccountType stripeAccountType, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            var customer = account.GetStripeCustomer(stripeAccountType, msgs);

            if (customer.IsNotEmpty())
                return StripeHelper.GetStripeHelper(stripeAccountType).UpdateCustomer(customer.Id, account.Name, account.Email, null, msgs);

            customer = StripeHelper.GetStripeHelper(stripeAccountType).CreateCustomer(account.Name, account.Email, null, msgs);
            account.SetStripeCustomer(customer, msgs, dc);

            return customer;
        }

        public static List<Source> GetStripeSources(this Account account, StripeAccountType stripeAccountType, SystemMessages msgs = null)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();
            var customer = account.GetStripeCustomer(stripeAccountType, msgs);

            if (customer.IsNotEmpty())
                return StripeHelper.GetStripeHelper(stripeAccountType).GetSources(customer.Id, msgs);

            return new List<Source>();
        }

        public static List<Card> GetStripeCards(this Account account, StripeAccountType stripeAccountType, SystemMessages msgs = null)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            var customer = account.GetStripeCustomer(stripeAccountType, msgs);

            if (customer.IsNotEmpty())
                return StripeHelper.GetStripeHelper(stripeAccountType).GetCards(customer.Id, msgs);

            return new List<Card>();
        }

        public static List<BankAccount> GetStripeBankAccounts(this Account account, StripeAccountType stripeAccountType, SystemMessages msgs = null)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            var customer = account.GetStripeCustomer(stripeAccountType, msgs);

            if (customer.IsNotEmpty())
                return StripeHelper.GetStripeHelper(stripeAccountType).GetBankAccounts(customer.Id, msgs);

            return new List<BankAccount>();
        }

        public static List<Charge> GetStripeBillingHistory(this Account account, StripeAccountType stripeAccountType, SystemMessages msgs = null)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            var charges = new List<Charge>();
            var customer = account.GetStripeCustomer(stripeAccountType, msgs);

            if (customer.IsNotEmpty())
                charges = StripeHelper.GetStripeHelper(stripeAccountType).GetCharges(customer.Id, msgs);

            return charges;
        }

        public static List<Refund> GetStripeRefundHistory(this Account account, StripeAccountType stripeAccountType, SystemMessages msgs = null)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            var refunds = new List<Refund>();
            var customer = account.GetStripeCustomer(stripeAccountType, msgs);

            if (!customer.IsEmpty())
            {
                var charges = account.GetStripeBillingHistory(stripeAccountType, msgs);

                charges.ForEach(charge => refunds.AddRange(StripeHelper.GetStripeHelper(stripeAccountType).GetRefunds(charge.Id, msgs)));
            }

            return refunds;
        }

        #endregion

        #region Registrants

        /// <summary>
        ///     creates the initial contact/student record for the current account, not the global registrant account, details are
        ///     added later
        /// </summary>
        /// <param name="account"></param>
        /// <param name="loggedInUserId"></param>
        /// <param name="registrantType"></param>
        /// <param name="isMaster"></param>
        /// <param name="msgs"></param>
        /// <param name="dc"></param>
        /// <returns></returns>
        public static Registrant AddRegistrant(this Account account, int loggedInUserId, RegistrantType registrantType,
            bool isMaster, SystemMessages msgs, DatabaseConnection dc = null)
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
                LastModifiedBy = loggedInUserId,
            };
            msgs.AddRange(registrant.Save(dc: dc));

            return registrant;
        }

        public static List<RegistrantActivity> GetRegistrantActivities(this Account account, int? start = null,
            int? length = null, DatabaseConnection dc = null)
        {
            return RegistrantActivity.LoadByAccountId(account.ID.Value, start, length, dc: dc).ToList();
        }

        public static int GetRegistrantActivityCount(this Account account, DatabaseConnection dc = null)
        {
            return RegistrantActivity.GetCount(new Hashtable {{"ActivityAccountId", account.ID.Value}});
        }

        public static Registrant GetRegistrant(this Account account, Guid Uid, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            if (account.IsNotEmpty())
                return Registrant.LoadByUidByAccountId(Uid, account.ID.Value, msgs, dc);

            return null;
        }

        #endregion

        #region AccountSettingsAndProperties

        public static DateTime ConvertToAccountTimeZone(this Account account, DateTime dt)
        {
            switch (account.TimeZoneAbbr)
            {
                case "EST":
                    return TimeZoneInfo.ConvertTimeFromUtc(dt,
                        TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time"));
                case "CST":
                    return TimeZoneInfo.ConvertTimeFromUtc(dt,
                        TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time"));
                case "MST":
                    return TimeZoneInfo.ConvertTimeFromUtc(dt,
                        TimeZoneInfo.FindSystemTimeZoneById("Mountain Standard Time"));
                case "PST":
                    return TimeZoneInfo.ConvertTimeFromUtc(dt,
                        TimeZoneInfo.FindSystemTimeZoneById("Mountain Standard Time"));
                case "AKST":
                    return TimeZoneInfo.ConvertTimeFromUtc(dt,
                        TimeZoneInfo.FindSystemTimeZoneById("Alaskan Standard Time"));
                case "HAST":
                    return TimeZoneInfo.ConvertTimeFromUtc(dt,
                        TimeZoneInfo.FindSystemTimeZoneById("Hawaiian Standard Time"));
                case "AST":
                    return TimeZoneInfo.ConvertTimeFromUtc(dt,
                        TimeZoneInfo.FindSystemTimeZoneById("Arab Standard Time"));
                default:
                    return dt;
            }
        }

        public static DateTime ConvertFromAccountTimeZone(this Account account, DateTime dt)
        {
            switch (account.TimeZoneAbbr)
            {
                case "EST":
                    return TimeZoneInfo.ConvertTimeToUtc(dt,
                        TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time"));
                case "CST":
                    return TimeZoneInfo.ConvertTimeToUtc(dt,
                        TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time"));
                case "MST":
                    return TimeZoneInfo.ConvertTimeToUtc(dt,
                        TimeZoneInfo.FindSystemTimeZoneById("Mountain Standard Time"));
                case "PST":
                    return TimeZoneInfo.ConvertTimeToUtc(dt,
                        TimeZoneInfo.FindSystemTimeZoneById("Mountain Standard Time"));
                case "AKST":
                    return TimeZoneInfo.ConvertTimeToUtc(dt,
                        TimeZoneInfo.FindSystemTimeZoneById("Alaskan Standard Time"));
                case "HAST":
                    return TimeZoneInfo.ConvertTimeToUtc(dt,
                        TimeZoneInfo.FindSystemTimeZoneById("Hawaiian Standard Time"));
                case "AST":
                    return TimeZoneInfo.ConvertTimeToUtc(dt,
                        TimeZoneInfo.FindSystemTimeZoneById("Arab Standard Time"));
                default:
                    return dt;
            }
        }

        public static DateTime NowInAccountTimeZone(this Account account)
        {
            return account.ConvertToAccountTimeZone(DateTime.UtcNow);
        }

        public static List<AccountProperty> GetAccountProperties(this Account account, AccountPropertyType propertyType,
            SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return AccountProperty.LoadByAccountIdByAccountPropertyTypeId(account.ID.Value, propertyType, msgs, dc);
        }

        public static List<AccountProperty> GetAccountProperties(this Account account,
            List<AccountPropertyType> propertyTypes, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return AccountProperty.LoadByAccountIdByAccountPropertyTypes(account.ID.Value, propertyTypes, msgs, dc);
        }

        #endregion

        #region Attendance

        public static List<Attendance> GetCompletedAttendance(this Account account, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            return Attendance.LoadByAccountIdByIsCompleted(account.ID.Value, true, msgs, dc);
        }

        public static int GetAttendanceCompletedCount(this Account account, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            return Attendance.GetCountByAccountIdByIsCompleted(account.ID.Value, true, msgs, dc);
        }

        public static int GetAttendancePresentCount(this Account account, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            return AttendanceRegistrant.GetPresentCountByAccountId(account.ID.Value, msgs, dc);
        }

        public static int GetAttendancePossibleCount(this Account account, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            return AttendanceRegistrant.GetPossibleCountByAccountId(account.ID.Value, msgs, dc);
        }

        public static List<Attendance> GetAttendanceByDate(this Account account, DateTime date,
            SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return Attendance.LoadByAccountIdByDate(account.ID.Value, date, msgs, dc);
        }

        public static AttendanceRegistrant GetAttendanceRegistrant(this Account account, Activity activity,
            int attendanceId, int registrantId, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            msgs = msgs.IsNotEmpty() ? msgs : new SystemMessages();

            if (activity.AccountId.Value != account.ID.Value)
            {
                msgs.Add("Invalid account attempting to get AttendanceRegistrant record", MessageType.ApiUsageError);
                return null;
            }

            return AttendanceRegistrant.LoadByAttendanceIdByRegistrantId(
                    attendanceId,
                    registrantId,
                    msgs,
                    dc)
                .FirstOrDefault();
        }

        public static List<AttendanceRegistrant> GetAttendanceRegistrantByDateRange(this Account account,
            Activity activity, int registrantId, DateRange dateRange, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            msgs = msgs.IsNotEmpty() ? msgs : new SystemMessages();

            if (activity.AccountId.Value != account.ID.Value)
            {
                msgs.Add("Invalid account attempting to get AttendanceRegistrant records", MessageType.ApiUsageError);
                return null;
            }

            return AttendanceRegistrant.LoadByActivityIdRegistrantIdByDateRange(activity.ID.Value, registrantId,
                dateRange, msgs, dc);
        }

        #endregion

        #region Registrations

        public static List<Registration> GetRegistrations(this Account account, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            return Registration.LoadByAccountId(account.ID.Value, msgs, dc)
                .Where(registration => Registration.ValidCompletionStatus.Contains(registration.CompletionStatusId))
                .ToList();
        }

        public static Registration GetRegistration(this Account account, Guid Uid, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            if (account.IsNotEmpty())
                return Registration.LoadByUidByAccountId(Uid, account.ID.Value, msgs, dc);

            return null;
        }

        public static List<Registration> GetRegistrationsByDate(this Account account, DateTime startDate,
            DateTime endDate, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return Registration.LoadByAccountIdByDate(account.ID.Value, startDate, endDate, msgs, dc);
        }

        public static int GetRegistrationCountByDate(this Account account, DateTime startDate, DateTime endDate,
            SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return Registration.GetCountByAccountIdByDate(account.ID.Value, startDate, endDate, msgs, dc);
        }

        public static List<Registration> GetRegistrations(this Account account, int start, int length,
            SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return Registration.Load(
                new Hashtable
                {
                    {"AccountId", account.ID.Value},
                    {"CompletionStatusId", Registration.ValidCompletionStatus},
                },
                start,
                length,
                msgs,
                dc);
        }

        public static int GetRegistrationCount(this Account account, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            return Registration.GetCount(
                new Hashtable
                {
                    {"AccountId", account.ID.Value},
                    {"CompletionStatusId", RegistrationCompletionStatus.Completed},
                },
                msgs: msgs,
                dc: dc);
        }

        public static List<(int ID, ParticipationObjectType ParticipationObjectTypeId, string Name)>
            GetParticipationObjectsByBillerAccountId(this Account billerAccount,
                DateTime? startDate, DateTime? endDate, bool isDeleted, SystemMessages msgs = null,
                DatabaseConnection dc = null)
        {
            return Transaction.GetParticipationObjectsByBillerOrPayerAccountId(billerAccount.ID, null, startDate,
                endDate, isDeleted, msgs, dc);
        }

        public static List<(int ID, ParticipationObjectType ParticipationObjectTypeId, string Name)>
            GetParticipationObjectsByPayerAccountId(this Account payerAccount,
                DateTime? startDate, DateTime? endDate, bool isDeleted, SystemMessages msgs = null,
                DatabaseConnection dc = null)
        {
            return Transaction.GetParticipationObjectsByBillerOrPayerAccountId(null, payerAccount.ID, startDate,
                endDate, isDeleted, msgs, dc);
        }

        public static List<Registration> SearchRegistrations(this Account account, string searchText, int? questionId,
            List<int> participationObjectIds, ParticipationObjectType participationObjectType,
            RegistrationStatus regStatus, List<RegistrationAssignmentType> rats, int? start, int? length,
            SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return Registration.Search(searchText, account.ID.Value, questionId, participationObjectIds,
                participationObjectType, regStatus, rats, start, length, msgs: msgs, dc: dc);
        }

        public static int SearchRegistrationCount(this Account account, string searchText, int? questionId,
            List<int> participationObjectIds, ParticipationObjectType participationObjectType,
            RegistrationStatus regStatus, List<RegistrationAssignmentType> rats, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            return Registration.SearchCount(searchText, account.ID.Value, questionId, participationObjectIds,
                participationObjectType, regStatus, rats, msgs: msgs, dc: dc);
        }

        public static void CreateNewRegistrantAndRegistration(this Account account,
            ParticipationObject participationObject, List<(FormQuestion question, string answer)> questionsAndAnswers,
            RegistrationPeriod period, WebUser loggedInUser, SystemMessages msgs = null, DatabaseConnection dc = null)
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

        public static void CreateNewRegistrantAndRegistration(this Account account,
            ParticipationObject participationObject, List<(FormQuestion question, string answer)> questionsAndAnswers,
            List<(Activity activity, int preferenceNumber)> activitiesAndPreferences, RegistrationPeriod period,
            WebUser loggedInUser, SystemMessages msgs = null, DatabaseConnection dc = null)
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
                        var activityParticipationObject =
                            activityAndPreference.activity.GetParticipationObject(amsgs, adc);

                        registration.ProcessRegistrationAssignment(
                            participationObject: activityParticipationObject,
                            masterRegistrant: null,
                            registrant: student,
                            seatsRequested: 1,
                            preferenceNumber: activityAndPreference.preferenceNumber,
                            activityGroupId: groupId,
                            periodId: period?.ID,
                            amountCharged: activityParticipationObject.Cost,
                            registeredDates: null,
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
                onException: (SystemMessages emsgs) => Utilities.SystemErrorLog(emsgs),
                dc: dc
            );
        }

        #endregion

        #region NewAccounSetup

        public static void EnsureDefaultStudentForm(this Account formOwner, WebUser loggedInUser, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            formOwner.GetOrCreateDefaultStudentForm(loggedInUser, msgs, dc);
        }

        /// <summary>
        ///     Copy default form from main After School HQ Account
        /// </summary>
        /// <param name="account"></param>
        /// <param name="loggedInUser"></param>
        /// <param name="msgs"></param>
        /// <param name="dc"></param>
        /// <returns></returns>
        public static Form CreateDefaultStudentForm(this Account formOwner, WebUser loggedInUser, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            var form = FormService.GetOrCreateAfterSchoolAccountTemplateForm(FormType.StudentForm, msgs, dc);

            if (form.AccountId != formOwner.ID)
                form = form.Copy(loggedInUser.ID.Value, formOwner.ID.Value, FormType.StudentForm, "Standard Reg Form",
                    msgs, dc);

            msgs.AddRange(formOwner.SetDefaultStudentForm(form, loggedInUser, dc));

            return form;
        }

        public static void EnsureDefaultForm(this Account formOwner, Activity activity, WebUser loggedInUser, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            if (activity.ActivityTypeId == ActivityType.VolunteerOpportunity)
                formOwner.GetOrCreateDefaultVolunteerForm(loggedInUser, msgs, dc);
            else
                formOwner.GetOrCreateDefaultStudentForm(loggedInUser, msgs, dc);
        }

        public static void EnsureDefaultVolunteerForm(this Account formOwner, WebUser loggedInUser, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            formOwner.GetOrCreateDefaultVolunteerForm(loggedInUser, msgs, dc);
        }

        public static Form CreateDefaultVolunteerForm(this Account formOwner, WebUser loggedInUser, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            var form = FormService.GetOrCreateAfterSchoolAccountTemplateForm(FormType.VolunteerForm, msgs, dc);

            if (form.AccountId != formOwner.ID)
                form = form.Copy(loggedInUser.ID.Value, formOwner.ID.Value, FormType.VolunteerForm, "Standard Volunteer Reg Form",
                    msgs, dc);

            msgs.AddRange(formOwner.SetDefaultVolunteerForm(form, loggedInUser, dc));

            return form;
        }

        public static SystemMessages SetDefaultStudentForm(this Account account, Form form, WebUser loggedInUser,
            DatabaseConnection dc = null)
        {
            account.DefaultFormId = form.ID;
            account.LastModifiedBy = loggedInUser?.ID.IfEmpty(0);
            account.LastModifiedDate = DateTime.UtcNow;
            return account.Save(dc);
        }

        public static SystemMessages SetDefaultVolunteerForm(this Account account, Form form, WebUser loggedInUser,
            DatabaseConnection dc = null)
        {
            account.DefaultVolunteerFormId = form.ID;
            account.LastModifiedBy = loggedInUser?.ID.IfEmpty(0);
            account.LastModifiedDate = DateTime.UtcNow;
            return account.Save(dc);
        }

        public static Form GetOrCreateDefaultForm(this Account account, FormType formType, WebUser loggedInUser,
            SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return formType switch
            {
                FormType.StudentForm => account.GetOrCreateDefaultStudentForm(loggedInUser, msgs, dc),
                FormType.VolunteerForm => account.GetOrCreateDefaultVolunteerForm(loggedInUser, msgs, dc),
                FormType.Survey => FormService.GetOrCreateAfterSchoolAccountTemplateForm(FormType.Survey, msgs, dc),
                _ => null,
            };
        }

        public static Form GetOrCreateDefaultStudentForm(this Account account, WebUser loggedInUser,
            SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return account.GetDefaultStudentForm(msgs, dc) ?? CreateDefaultStudentForm(account, loggedInUser, msgs, dc);
        }

        public static Form GetDefaultStudentForm(this Account account, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            return account.DefaultFormId.HasValue ? Form.Load(account.DefaultFormId.Value, msgs, dc) : null;
        }

        public static Form GetDefaultForm(this Account account, Activity activity, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return activity.ActivityTypeId == ActivityType.VolunteerOpportunity
                ? account.GetDefaultVolunteerForm(msgs, dc)
                : account.GetDefaultStudentForm(msgs, dc);
        }

        public static Form GetOrCreateDefaultVolunteerForm(this Account account, WebUser loggedInUser,
            SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return account.GetDefaultVolunteerForm(msgs, dc) ?? CreateDefaultVolunteerForm(account, loggedInUser, msgs, dc);
        }

        public static Form GetDefaultVolunteerForm(this Account account, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            return account.DefaultVolunteerFormId.HasValue ? Form.Load(account.DefaultVolunteerFormId.Value, msgs, dc) : null;
        }

        public static SystemMessages SetInterestForm(this Account account, Form form, WebUser loggedInUser,
            DatabaseConnection dc = null)
        {
            account.InterestFormId = form.ID;
            account.LastModifiedBy = loggedInUser?.ID.IfEmpty(0);
            account.LastModifiedDate = DateTime.UtcNow;
            return account.Save(dc);
        }

        public static Form CreateInterestForm(this Account formOwner, WebUser loggedInUser, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            var form = FormService.GetOrCreateAfterSchoolAccountInterestForm(msgs, dc);

            if (form.AccountId != formOwner.ID)
                form = form.Copy(loggedInUser.ID.Value, formOwner.ID.Value, FormType.StudentForm, "Interest Form",
                    msgs, dc);

            msgs.AddRange(formOwner.SetInterestForm(form, loggedInUser, dc));

            return form;
        }

        public static Form GetOrCreateInterestForm(this Account account, WebUser loggedInUser,
            SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return account.GetInterestForm(msgs, dc) ?? CreateInterestForm(account, loggedInUser, msgs, dc);
        }

        public static Form GetInterestForm(this Account account, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            return account.InterestFormId.HasValue ? Form.Load(account.InterestFormId.Value, msgs, dc) : null;
        }

        public static ActivityForm GetDefaultActivityForm(this Account account, Activity activity, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            var formId = activity.ActivityTypeId == ActivityType.VolunteerOpportunity
                ? account.GetDefaultVolunteerForm(msgs, dc).ID
                : account.GetDefaultStudentForm(msgs, dc).ID;

            return new ActivityForm
            {
                FormId = formId,
                IsActive = true,
                AccountId = account.ID,
            };
        }

        public static AccountPackage SetDefaultAccountPackage(this Account account, WebUser loggedInUser,
            SystemMessages msgs, DatabaseConnection dc = null)
        {
            var package = Package.Load(start: 0, length: 1, msgs: msgs, dc: dc).First();

            return account.AddPackage(package.ID.Value, BillingPeriodType.Month, loggedInUser, msgs, dc);
        }

        public static Location GetDefaultLocation(this Account account, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            return account.DefaultLocationId.HasValue ? Location.Load(account.DefaultLocationId.Value, msgs, dc) : null;
        }

        public static string GenerateUniqueName(string name, int? accountId, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            //unique name
            var reg = new Regex(BasicLibrary.Utilities.UrlFriendlyRegEx,
                RegexOptions.IgnoreCase | RegexOptions.Singleline);

            var origUniqueName = reg.Replace(name.GetMaxWords(25), "");
            var uniqueName = origUniqueName;
            var duplicate = Account.LoadByUniqueName(uniqueName, msgs, dc);
            var count = 1;
            var reservedUniqueNames = new[]
            {
                "demo", "about", "copyright", "index", "parents", "privacy", "providers", "schools", "tos", "account",
                "afterschool", "db", "faq", "home", "legal", "login", "logout", "oauth", "r", "reg", "services",
                "signup",
            };

            while ((duplicate.IsNotEmpty() && duplicate.ID != accountId) ||
                   uniqueName.ToLower().In(reservedUniqueNames))
            {
                uniqueName = origUniqueName + count.ToString();
                count++;
                duplicate = Account.LoadByUniqueName(uniqueName, msgs, dc);
            }

            return uniqueName;
        }

        public static SignUpType GetSignUpType(this Account account, Package p, AccountPackage activePackage,
            BillingPeriodType billingPeriodType, ref int trialDayCount, SystemMessages msgs = null,
            DatabaseConnection dc = null)
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

            if ((p.IsNotEmpty() && p.IsFree(billingPeriodType)) || (activePackage.IsNotEmpty() && activePackage.IsFree))
            {
                // new sign up
                if (activePackage.IsEmpty() || !account.IsClaimed)
                    signupType = SignUpType.NewAccountSignUp;

                // could be downgrading to a free account
                else if (activePackage.PackageId != p.ID)
                    signupType = SignUpType.ChangePackage;
            }
            else
            {
                if (activePackage.IsEmpty())
                    trialDayCount =
                        (p.HasFeature(FeatureType.TrialDays, msgs, dc)
                            ? p.GetFeature(FeatureType.TrialDays, msgs, dc).GetMaxValue()
                            : 0).ToInteger();
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

                // could be changing same package with other plan, like monthly or yearly
                if (activePackage.IsNotEmpty() && activePackage.PackageId == p.ID &&
                    activePackage.BillingPeriodTypeId != billingPeriodType)
                    signupType = SignUpType.ChangeBillingPeriodType;

                //package changed
                else if (activePackage.IsNotEmpty() && activePackage.PackageId != p.ID)
                    signupType = SignUpType.ChangePackage;
            }

            return signupType;
        }

        public static AccountPackage AddPackage(this Account account, int packageId,
            BillingPeriodType billingPeriodType, WebUser loggedInUser, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            Package package = null;
            AccountPackage currentPackage = null;
            var newap = new AccountPackage();
            var loggedInUserId = loggedInUser.IsNotEmpty() ? loggedInUser.ID.Value : 0;

            Database.ExecuteTransaction(
                action: (SystemMessages amsgs, DatabaseConnection adc) =>
                {
                    package = Package.Load(packageId, amsgs, adc);
                    currentPackage = account.GetActiveAccountPackage(amsgs, adc);

                    var trialDayCount = 0;

                    // is user:
                    //      paying for their Trial account
                    //      upgrading to a different package
                    //      paying for a new package with no trial account

                    var signupType = account.GetSignUpType(package, currentPackage, billingPeriodType,
                        ref trialDayCount, amsgs, adc);

                    if (signupType != SignUpType.NotSet)
                    {
                        if (signupType == SignUpType.ChangePackage ||
                            signupType == SignUpType.ChangePackageDuringTrial)
                        {
                            //cancel existing package                            
                            amsgs.AddRange(
                                currentPackage.CancelPackageWithStripe(account.StripeCustomerId, false, adc));
                        }

                        if (package.StripePricePerMonthId.IsEmpty() || package.StripePricePerYearId.IsEmpty())
                        {
                            package.SaveWithStripe(adc);
                        }

                        var priceId = billingPeriodType == BillingPeriodType.Month
                            ? package.StripePricePerMonthId
                            : package.StripePricePerYearId;


                        if (msgs.HasNoErrors())
                        {
                            if (signupType != SignUpType.ConvertingFromTrial)
                            {
                                //create new package
                                newap.AccountId = account.ID;
                                newap.PackageId = packageId;
                                newap.BillingPeriodTypeId = billingPeriodType;
                                newap.CreateDate = DateTime.UtcNow;
                                newap.CreatedBy = loggedInUserId;
                                newap.LastModifiedDate = DateTime.UtcNow;
                                newap.LastModifiedBy = loggedInUserId;
                                newap.IsActive = true;
                                newap.Price = billingPeriodType == BillingPeriodType.Month
                                    ? package.Price
                                    : package.PricePerYear;

                                newap.StripeSubscriptionId = null;

                                if (currentPackage.IsNotEmpty() &&
                                    currentPackage.StripeSubscriptionId.IsNotEmpty() &&
                                    signupType != SignUpType.ChangePackage &&
                                    signupType != SignUpType.ChangePackageDuringTrial)
                                {
                                    var subscription = StripeHelper.GetStripeHelperSubscriberAccount()
                                        .GetSubscription(currentPackage?.StripeSubscriptionId, "", msgs);
                                    if (subscription.IsNotEmpty() && subscription.Status == "active")
                                        newap.StripeSubscriptionId = subscription.Id;
                                }

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


                                amsgs.AddRange(newap.SaveWithStripe(account.StripeCustomerId, false, priceId, adc));

                                if (amsgs.HasNoErrors() && currentPackage.IsNotEmpty())
                                    amsgs.AddRange(currentPackage.CancelPackage(adc));
                            }
                            // if it has a trial date end in the future, then end it now if they are converting from trial
                            else if (currentPackage.TrialEndDate.HasValue &&
                                     currentPackage.TrialEndDate.Value >= DateTime.Now)
                            {
                                currentPackage.TrialEndDate = DateTime.UtcNow;
                                currentPackage.LastModifiedDate = DateTime.UtcNow;
                                currentPackage.LastModifiedBy = loggedInUserId;

                                amsgs.AddRange(currentPackage.SaveWithStripe(account.StripeCustomerId, true, priceId,
                                    adc));

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
                        amsgs.Add($"Credit Card is required to sign up for {package.Name} Package",
                            MessageType.UserError);
                    }
                },
                onSuccess: (SystemMessages smsgs) => msgs.AddRange(smsgs),
                onFailure: (SystemMessages fmsgs) => msgs.AddRange(fmsgs),
                onException: (SystemMessages emsgs) => Utilities.SystemErrorLog(emsgs),
                dc: dc
            );

            return newap;
        }

        public static void EnsureDefaultPackageFeatures(this Account account, WebUser loggedInUser, SystemMessages msgs,
            DatabaseConnection dc = null)
        {
            account.EnsureDefaultPackageFeatures(null, loggedInUser, msgs, dc);
        }

        public static void EnsureDefaultPackageFeatures(this Account account, AccountPackage ap, WebUser loggedInUser,
            SystemMessages msgs, DatabaseConnection dc = null)
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

        public static int GetTrialDaysRemaining(this Account account, AccountPackage currentPackage,
            SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var daysRemaining = 0;
            var package = account.GetActiveAccountPackage(msgs, dc);
            var daysSinceSignUp = (DateTime.UtcNow -
                                   (package.StartDate.HasValue
                                       ? package.StartDate.Value
                                       : package.TrialStartDate.Value)).Days;
            if (daysSinceSignUp <
                currentPackage.GetFeature(FeatureType.TrialDays, msgs, dc).GetMaxValue() +
                1) // they get the full last day
                daysRemaining =
                    (currentPackage.GetFeature(FeatureType.TrialDays, msgs, dc).GetMaxValue() - daysSinceSignUp)
                    .ToInteger();

            return daysRemaining;
        }

        public static Activity GetDefaultNewActivity(this Account account)
        {
            var profile = account.GetAccountProfile();

            return new Activity
            {
                SeatsAvailable = Math.Min(20, account.GetActiveAccountPackage().GetFeature(FeatureType.Participants).Max.Value.ToInteger()),
                CostPerRegistrant = 0,
                AccountId = account.ID,
                RegistrationModeId = RegistrationMode.Register,
                AllowToPayLater = true,
                MinSeatsRequired = 1,
                GradeLevelStart = profile?.GradeLevelStart,
                GradeLevelEnd = profile?.GradeLevelEnd,
                GenderRestriction = profile.IsNotEmpty() ? profile.GenderRestriction : GenderRestrictionType.None,
                AgeStart = profile?.AgeStart,
                AgeEnd = profile?.AgeEnd,
                IsContinuousEnrollment = false,
                ActivityLocationTypeId = ActivityLocationType.Physical,
            };
        }

        public static ActivityGroup GetDefaultNewLeague(this Account account)
        {
            return new ActivityGroup
            {
                SeatsAvailable = Math.Min(20,
                    account.GetActiveAccountPackage().GetFeature(FeatureType.Participants).Max.Value.ToInteger()),
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
            return new RegistrationPeriod
            {
                ID = 0,
                StartDateTime = $"{account.ConvertToAccountTimeZone(DateTime.UtcNow).ToShortDateString()} {"8:00 AM"}"
                    .ToDateTime(),
                EndDateTime =
                    $"{account.ConvertToAccountTimeZone(DateTime.UtcNow).AddWeeks(1).ToShortDateString()} {"12:00 AM"}"
                        .ToDateTime(),
            };
        }

        public static Account CreateNewGhostAccount(string orgName, AccountType accountType, WebUser loggedInUser,
            bool isAfterSchool, SystemMessages msgs, DatabaseConnection dc)
        {
            return CreateNew(orgName, null, null, false, accountType, null, null, null, null, loggedInUser,
                isAfterSchool, false, msgs, dc);
        }

        public static Account CreateNewUnclaimedAccount(string orgName, string email, string claimemaildomain,
            string phone, bool isActive, AccountType accountType, WebUser accountOwner, int? packageId,
            BillingPeriodType? packageBillingPeriod, string timeZone, WebUser loggedInUser, bool isAfterSchool,
            bool createWithStripe, SystemMessages msgs, DatabaseConnection dc)
        {
            return CreateNew(orgName, email, claimemaildomain, phone, isActive, accountType, accountOwner, packageId,
                packageBillingPeriod, timeZone, loggedInUser, isAfterSchool, createWithStripe, false, msgs, dc);
        }

        public static Account CreateNew(string orgName, string email, string phone, bool isActive,
            AccountType accountType, WebUser accountOwner, int? packageId, BillingPeriodType? packageBillingPeriod,
            string timeZone, WebUser loggedInUser, bool isAfterSchool, bool createWithStripe, SystemMessages msgs,
            DatabaseConnection dc)
        {
            return CreateNew(orgName, email, phone, isActive, accountType, accountOwner, packageId,
                packageBillingPeriod, timeZone, loggedInUser, isAfterSchool, createWithStripe, StripeAccountType.SubscriberAccount, true, msgs, dc);
        }

        public static Account CreateNew(string orgName, string email, string phone, bool isActive,
            AccountType accountType, WebUser accountOwner, int? packageId, BillingPeriodType? packageBillingPeriod,
            string timeZone, WebUser loggedInUser, bool isAfterSchool, bool createWithStripe, StripeAccountType stripeAccountType, SystemMessages msgs,
            DatabaseConnection dc)
        {
            return CreateNew(orgName, email, phone, isActive, accountType, accountOwner, packageId,
                packageBillingPeriod, timeZone, loggedInUser, isAfterSchool, createWithStripe, stripeAccountType, true, msgs, dc);
        }

        public static Account CreateNew(string orgName, string email, string phone, bool isActive,
            AccountType accountType, WebUser accountOwner, int? packageId, BillingPeriodType? packageBillingPeriod,
            string timeZone, WebUser loggedInUser, bool isAfterSchool, bool createWithStripe, StripeAccountType stripeAccountType, bool isClaimed,
            SystemMessages msgs, DatabaseConnection dc)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();
            var uniqueName = GenerateUniqueName(orgName, null, msgs, dc);

            var onboardingStepDone = OnboardingStepType.NotSet;

            if (accountType != AccountType.NotSet)
                onboardingStepDone = OnboardingStepType.AccountType;

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
                CreatedBy = loggedInUser?.ID,
                LastModifiedBy = loggedInUser?.ID,
                IsAfterSchool = isAfterSchool,
                AccountTypeId = accountType,
                UniqueName = uniqueName,
                ConvenienceFeesPaidBy = FeesPaidByType.Parent,
                CreditCardFeesPaidBy = FeesPaidByType.Parent,
                TimeZone = timeZone,
                IsClaimed = isClaimed,
                OnboardingStepDone = onboardingStepDone,
            };

            if (createWithStripe)
                msgs.AddRange(account.SaveWithStripe(stripeAccountType, dc));
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
                {
                    account.CreateDefaultStudentForm(loggedInUser, msgs, dc);

                    if (account.HasFeature(FeatureType.ManageVolunteerOpportunities))
                        account.CreateDefaultVolunteerForm(loggedInUser, msgs, dc);
                }
            }

            return account;
        }

        public static Account CreateNew(string orgName, string email, string claimemaildomain, string phone,
            bool isActive, AccountType accountType, WebUser accountOwner, int? packageId,
            BillingPeriodType? packageBillingPeriod, string timeZone, WebUser loggedInUser, bool isAfterSchool,
            bool createWithStripe, SystemMessages msgs, DatabaseConnection dc)
        {
            return CreateNew(orgName, email, claimemaildomain, phone, isActive, accountType, accountOwner, packageId,
                packageBillingPeriod, timeZone, loggedInUser, isAfterSchool, createWithStripe, true, msgs, dc);
        }

        public static Account CreateNew(string orgName, string email, string claimemaildomain, string phone,
            bool isActive, AccountType accountType, WebUser accountOwner, int? packageId,
            BillingPeriodType? packageBillingPeriod, string timeZone, WebUser loggedInUser, bool isAfterSchool,
            bool createWithStripe, bool isClaimed, SystemMessages msgs, DatabaseConnection dc)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();
            var uniqueName = GenerateUniqueName(orgName, null, msgs, dc);

            var onboardingStepDone = OnboardingStepType.NotSet;

            if (accountType != AccountType.NotSet)
                onboardingStepDone = OnboardingStepType.AccountType;

            var account = new Account
            {
                Name = orgName,
                IsAdminAccount = false,
                IsActive = isActive,
                Email = email,
                ClaimEmailDomain = claimemaildomain,
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
                TimeZone = timeZone,
                IsClaimed = isClaimed,
                OnboardingStepDone = onboardingStepDone,
            };

            if (createWithStripe)
                msgs.AddRange(account.SaveWithStripe(StripeAccountType.SubscriberAccount, dc));
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
                    account.CreateDefaultStudentForm(loggedInUser, msgs, dc);
            }

            return account;
        }

        public static Account CreateNewEmptyAccount(WebUser accountOwner, WebUser loggedInUser, SystemMessages msgs,
            DatabaseConnection dc)
        {
            var account = new Account
            {
                Name = $"{loggedInUser.FirstName}'s Enrichment Club",
                IsAfterSchool = true,
                CreatedBy = loggedInUser.ID,
                LastModifiedBy = loggedInUser.ID,
                CreateDate = DateTime.UtcNow,
                LastModifiedDate = DateTime.UtcNow,
                OwnerId = accountOwner?.ID,
                IsAdminAccount = false,
                IsActive = true,
                Email = accountOwner.Email,
                Phone = accountOwner.Phone,
                ConvenienceFeesPaidBy = FeesPaidByType.Parent,
                CreditCardFeesPaidBy = FeesPaidByType.Parent,
                IsClaimed = true,
            };

            msgs.AddRange(account.Save(dc));

            //create default form for account to use for registration
            if (msgs.HasNoErrors())
                account.CreateDefaultStudentForm(loggedInUser, msgs, dc);

            return account;
        }

        #endregion

        #region AccountRelationships

        public static AccountRelationship GetAccountRelationship(this Account account, int accountRelationshipId,
            SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (account.IsNotEmpty())
                return AccountRelationship.LoadByIdByHostAccountId(accountRelationshipId, account.ID.Value, msgs, dc);

            return null;
        }

        public static List<Account> GetVisitorAccounts(this Account hostAccount, List<AccountType> accountTypes,
            SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return AccountRelationship.LoadByHostAccountIdByAccountTypes(hostAccount.ID.Value, accountTypes, msgs, dc)
                .Select(accountRelationship => accountRelationship.GetVisitorAccount(msgs, dc))
                .ToList();
        }

        public static bool CheckIfIsypAccount(Account account, SystemMessages msgs)
        {
            if (account.ID == null)
            {
                return false;
            }

            var accountRelationships = AccountRelationship.LoadByVisitorAccountId(account.ID.Value, msgs);
            if (!accountRelationships.IsNotEmpty() || accountRelationships.Count == 0)
            {
                return false;
            }

            return accountRelationships
                .Where(ar => ar.HostAccountId != null)
                .Select(ar => Account.Load(ar.HostAccountId.Value, msgs))
                .Any(hostAccount => hostAccount.IsNotEmpty()
                                    && hostAccount.Name.Equals("ISYP", StringComparison.OrdinalIgnoreCase)
                                    && hostAccount.AccountTypeId == AccountType.Directory);
        }

        public static List<Account> GetVisitorAccounts(this Account hostAccount, List<AccountType> accountTypes,
            int? start, int? length, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return AccountRelationship.LoadByHostAccountIdByAccountTypes(hostAccount.ID.Value, accountTypes, msgs, dc)
                .Select(accountRelationship => accountRelationship.GetVisitorAccount(msgs, dc))
                .ToList();
        }

        public static int GetVisitorAccountsCount(this Account hostAccount, List<AccountType> accountTypes,
            SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return AccountRelationship.GetCount(hostAccount.ID.Value, accountTypes, msgs, dc);
        }

        public static List<Account> GetHostAccounts(this Account account, DatabaseConnection dc = null)
        {
            return AccountRelationship.LoadByVisitorAccountId(account.ID.Value, dc: dc)
                .Select(x => x.GetHostAccount(dc: dc))
                .ToList();
        }

        //Gets the accounts that are requesting the host's approval
        public static List<Account> GetRequestedVisitorAccounts(this Account hostAccount, AccountType? accountType,
            SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return AccountRelationship.LoadByRequested(hostAccount.ID.Value, accountType, msgs, dc)
                .Select(accountRelationship => accountRelationship.GetVisitorAccount(msgs, dc))
                .ToList();
        }

        public static AccountRelationship GetRequestForAccount(this Account visitorAccount, Account hostAccount,
            SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return AccountRelationship.LoadByHostAccountIdByVisitorAccountId(hostAccount.ID.Value,
                visitorAccount.ID.Value, msgs, dc);
        }

        //Gets the accounts that are requesting the host's approval
        public static int GetRequestedVisitorAccountsCount(this Account hostAccount, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            return AccountRelationship.GetCount(
                new Hashtable
                {
                    {"HostAccountId", hostAccount.ID.Value},
                    {"ApprovalStatusId", (int) ApprovalStatus.Requested},
                },
                msgs,
                dc);
        }

        //Gets the accounts that the host has approved
        public static List<Account> GetApprovedVisitorAccounts(this Account hostAccount, AccountType accountType,
            SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return AccountRelationship.LoadByApproved(hostAccount.ID.Value, accountType, msgs, dc)
                .Select(accountRelationship => accountRelationship.GetVisitorAccount(msgs, dc))
                .ToList();
        }

        public static List<Account> GetApprovedVisitorAccounts(this Account hostAccount, List<AccountType> accountTypes,
            SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return AccountRelationship.LoadByApproved(hostAccount.ID.Value, accountTypes, msgs, dc)
                .Select(accountRelationship => accountRelationship.GetVisitorAccount(msgs, dc))
                .ToList();
        }

        public static List<Account> GetApprovedVisitorAccounts(this Account hostAccount, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            return AccountRelationship.LoadByApproved(hostAccount.ID.Value, msgs, dc)
                .Select(accountRelationship => accountRelationship.GetVisitorAccount(msgs, dc))
                .Where(account => !account.IsDeleted && account.IsActive)
                .OrderBy(account => account.GetActiveAndPublishedActivityCount(false, msgs, dc) > 0)
                .ToList();
        }

        //Gets the accounts that the host has Denied
        public static List<Account> GetDeniedVisitorAccounts(this Account hostAccount, AccountType accountType,
            SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return AccountRelationship.LoadByDenied(hostAccount.ID.Value, accountType, msgs, dc)
                .Select(accountRelationship => accountRelationship.GetVisitorAccount(msgs, dc))
                .ToList();
        }

        public static List<Location> GetVisitorLocations(this Account visitorAccount, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            var arls = AccountRelationshipLocation.LoadByVisitorAccountId(visitorAccount.ID.Value, msgs, dc);
            var locations = new List<Location>();

            arls.ForEach(arl =>
            {
                var location = arl.GetLocation(msgs, dc);

                if (location.IsNotEmpty())
                    locations.Add(location);
            });

            return locations;
        }

        public static List<Location> GetHostedLocations(this Account hostAccount, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            return Location.LoadByHostAccountId(hostAccount.ID.Value, msgs, dc);
        }

        public static List<Location> GetAllAvailableLocations(this Account account, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            var locations = account.GetHostedLocations(msgs, dc);
            var visitorLocations = account.GetVisitorLocations(msgs, dc);

            if (visitorLocations.Any())
                locations.AddRange(visitorLocations);

            return locations;
        }

        /// <summary>
        ///     creating an account relationship in which the visitor is requesting approval saving it to database
        ///     and returning the system messages
        /// </summary>
        /// <param name="visitorAccount"></param>
        /// <param name="hostAccount"></param>
        /// <param name="hostPermissionType"></param>
        /// <param name="loggedInUser"></param>
        /// <param name="dc"></param>
        /// <returns></returns>
        public static AccountRelationship RequestAccessToAccount(this Account visitorAccount, Account hostAccount,
            HostPermissionType hostPermissionType, WebUser loggedInUser, SystemMessages msgs = null,
            DatabaseConnection dc = null)
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
                    true,
                    false,
                    loggedInUser,
                    msgs,
                    dc);

                //TODO Notify Host account of request
                if (msgs.HasNoErrors() && hostAccount.IsActive)
                    msgs.AddRange(EmailHelper.SendAccessRequestToHost(visitorAccount, hostAccount, loggedInUser));
            }
            else
            {
                msgs.Add("This account has already previously been denied", MessageType.UserError);
            }

            return relationship;
        }

        public static bool IsApprovedAsVisitorTo(this Account visitorAccount, Account hostAccount, SystemMessages msgs,
            DatabaseConnection dc = null)
        {
            var relationship =
                AccountRelationship.LoadByHostAccountIdByVisitorAccountId(hostAccount.ID.Value, visitorAccount.ID.Value,
                    msgs, dc);
            return relationship.IsNotEmpty() && relationship.ApprovalStatusId == ApprovalStatus.Approved;
        }

        public static bool RequiresPublishPermissionFor(this Account visitorAccount, Account hostAccount,
            SystemMessages msgs, DatabaseConnection dc = null)
        {
            var relationship =
                AccountRelationship.LoadByHostAccountIdByVisitorAccountId(hostAccount.ID.Value, visitorAccount.ID.Value,
                    msgs, dc);

            return relationship.IsEmpty() ||
                   relationship.ApprovalStatusId == ApprovalStatus.Denied ||
                   relationship.RequiresPublishPermission;
        }

        public static AccountRelationship ApproveAccessToAccount(this Account hostAccount, Account visitorAccount,
            WebUser loggedInUser, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return ApproveAccessToAccount(hostAccount, visitorAccount, HostPermissionType.ApproveOnly, loggedInUser,
                msgs, dc);
        }

        public static AccountRelationship ApproveAccessToAccount(this Account hostAccount, Account visitorAccount,
            HostPermissionType hostPermissionType, WebUser loggedInUser, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            var relationship =
                AccountRelationship.LoadByHostAccountIdByVisitorAccountId(hostAccount.ID.Value, visitorAccount.ID.Value,
                    msgs, dc);

            if (relationship.IsNotEmpty())
                msgs.AddRange(AccountRelationshipService.UpdateApprovalStatus(hostAccount, visitorAccount,
                    ApprovalStatus.Approved, loggedInUser, dc));
            else
                relationship = AccountRelationshipService.CreateNew(
                    hostAccount,
                    visitorAccount,
                    ApprovalStatus.Approved,
                    hostPermissionType,
                    true,
                    false,
                    loggedInUser,
                    msgs,
                    dc);

            return relationship;
        }

        public static void SendApprovalNotificationToVisitor(this Account hostAccount, Account visitorAccount,
            SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            msgs.AddRange(EmailHelper.SendApprovalNotificationToVisitor(visitorAccount, hostAccount));
        }

        /// <summary>
        ///     Denies access to the host account
        /// </summary>
        /// <param name="hostAccount"></param>
        /// <param name="visitorAccount"></param>
        /// <param name="loggedInUser"></param>
        /// <param name="dc"></param>
        /// <returns></returns>
        public static SystemMessages DenyAccessToAccount(this Account hostAccount, Account visitorAccount,
            WebUser loggedInUser, DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();

            msgs.AddRange(AccountRelationshipService.UpdateApprovalStatus(hostAccount, visitorAccount,
                ApprovalStatus.Denied, loggedInUser, dc));

            // if (msgs.HasNoErrors())
            // msgs.AddRange(EmailHelper.SendDenialNotificationToVisitor(visitorAccount, hostAccount));

            return msgs;
        }

        public static SystemMessages AddVisitorAccount(this Account hostAccount, Account visitorAccount,
            HostPermissionType hostPermissionType, ApprovalStatus approvalStatus, bool requiresPublishPermission,
            bool showAllPrograms, WebUser loggedInUser, DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();

            AccountRelationshipService.CreateNew(
                hostAccount,
                visitorAccount,
                approvalStatus,
                hostPermissionType,
                requiresPublishPermission,
                showAllPrograms,
                loggedInUser,
                msgs,
                dc);

            return msgs;
        }

        /// <summary>
        ///     Should only be used in situations where the visiting account doesn't need approval status. Otherwise use
        ///     RequestAccessToAccount
        /// </summary>
        /// <param name="visitorAccount"></param>
        /// <param name="hostAccount"></param>
        /// <param name="hostPermissionType"></param>
        /// <param name="approvalStatus"></param>
        /// <param name="loggedInUser"></param>
        /// <param name="dc"></param>
        /// <returns></returns>
        private static AccountRelationship AddHostAccount(this Account visitorAccount, Account hostAccount,
            HostPermissionType hostPermissionType, ApprovalStatus approvalStatus, bool requiresPublishPermission,
            bool showAllPrograms, WebUser loggedInUser, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return AccountRelationshipService.CreateNew(
                hostAccount,
                visitorAccount,
                approvalStatus,
                hostPermissionType,
                requiresPublishPermission,
                showAllPrograms,
                loggedInUser,
                msgs,
                dc);
        }

        public static List<SearchDirectoryResponse> SearchDirectoryAccounts(int hostAccountId, string search,
            string enrollToday, string grades, string ages, string gender, string services, string timing,
            string pricing, string cost, string starting, string other, string specialNeeds, decimal? latitude,
            decimal? longitude, int PageNum, int PageSize, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            try
            {
                var accountDataTable = Account.SearchVisitorAccountsByHostAccount(hostAccountId, search, enrollToday,
                    grades, ages, gender, services, timing, pricing, cost, starting, other, specialNeeds, latitude,
                    longitude, PageNum, PageSize, msgs: msgs);

                if (!accountDataTable.IsNotEmpty() || !accountDataTable.Rows.Any())
                {
                    return new List<SearchDirectoryResponse>();
                }

                var listResponse = new List<SearchDirectoryResponse>();
                foreach (DataRow row in accountDataTable.Rows)
                {
                    if (!row.IsNotEmpty())
                    {
                        continue;
                    }

                    var costLow = row["CostLow"].IsNotEmpty()
                        ? (decimal?) Convert.ToDecimal(row["CostLow"])
                        : null;
                    var costHigh = row["CostHigh"].IsNotEmpty()
                        ? (decimal?) Convert.ToDecimal(row["CostHigh"])
                        : null;

                    var response = new SearchDirectoryResponse
                    {
                        // ..account
                        AccountName = row["AccountName"].IsNotEmpty() ? row["AccountName"].ToString() : "",
                        AccountUid = new Guid(row["AccountUid"].ToString()),
                        HomePageUrl = $"{Utilities.BaseUrl()}/{row["AccountUniqueName"]}",
                        //.. account profile  
                        IsOpenDuringCovid = row["IsOpenDuringCovid"].IsNotEmpty() &&
                                            Convert.ToBoolean(row["IsOpenDuringCovid"]),
                        IsELearningCenter = row["IsELearningCenter"].IsNotEmpty() &&
                                            Convert.ToBoolean(row["IsELearningCenter"]),
                        IsAccountNowEnrolling = row["IsAccountNowEnrolling"].IsNotEmpty() &&
                                                Convert.ToBoolean(row["IsAccountNowEnrolling"]),
                        IsVirtualProgram = row["IsVirtualProgram"].IsNotEmpty() &&
                                           Convert.ToBoolean(row["IsVirtualProgram"]),
                        ProfileHeadline = row["Headline"].IsNotEmpty()
                            ? row["Headline"].ToString().MaxChars(70, "...")
                            : "",
                        IsRestrictByGrade = row["AgeStart"].IsEmpty(),
                        AgeStart = row["AgeStart"].IsNotEmpty() ? row["AgeStart"].ToString() : null,
                        AgeEnd = row["AgeEnd"].IsNotEmpty() ? row["AgeEnd"].ToString() : null,
                        GradeStart = row["GradeLevelStart"].IsNotEmpty()
                            ? row["GradeLevelStart"].ToString()
                            : null,
                        GradeEnd = row["GradeLevelEnd"].IsNotEmpty()
                            ? row["GradeLevelEnd"].ToString()
                            : null,
                        CostRange = GetCostRangeString(costLow, costHigh),
                        Priority = row["Priority"].IsNotEmpty() ? Convert.ToInt32(row["Priority"]) : 0,
                        TotalDifference = accountDataTable.Columns.Contains("TotalDifference") &&
                                          row["TotalDifference"].IsNotEmpty()
                            ? Convert.ToDecimal(row["TotalDifference"])
                            : 0,
                    };

                    var headerImageUrl = row["HeaderImage"].IsNotEmpty() ? row["HeaderImage"].ToString() : "";
                    var logo = row["Logo"].IsNotEmpty() ? row["Logo"].ToString() : "";
                    //.. reverted photos logic as we will use logo instead. part of AC for https://app.clickup.com/t/nv068a
                    //..var photos = AccountProperty.LoadByAccountIdByAccountPropertyTypeId(accountId, AccountPropertyType.Photo, msgs, dc);
                    var ah = Utilities.GetNewAmazonHelper();

                    if (headerImageUrl.IsNotBlank())
                        response.ProfileHeaderImageUrl = ah.GetFileLink(headerImageUrl);
                    else if (logo.IsNotBlank())
                        response.ProfileHeaderImageUrl = ah.GetFileLink(logo);

                    if (row["LocationId"].IsNotEmpty())
                    {
                        //..Location, ProfileHeaderImageUrl
                        var location = Location.Load(Convert.ToInt32(row["LocationId"]));
                        response.Latitude = location.Latitude.IsNotEmpty()
                            ? location.Latitude.Value.ToString(CultureInfo.InvariantCulture)
                            : "";
                        response.Longitude = location.Longitude.IsNotEmpty()
                            ? location.Longitude.Value.ToString(CultureInfo.InvariantCulture)
                            : "";
                        response.Location = location.FormatCityState(false);

                        if (response.ProfileHeaderImageUrl.IsEmpty())
                            response.ProfileHeaderImageUrl = location.GetGoogleMapsImageUrl(1500, 600);
                    }

                    listResponse.Add(response);
                }

                if (latitude.IsNotEmpty() && latitude != 0 && longitude.IsNotEmpty() && longitude != 0)
                    return listResponse.DistinctBy(m => m.AccountName).ToList();

                return listResponse.OrderBy(m => m.Priority).ThenBy(m => Guid.NewGuid()).ToList();
            }
            catch (Exception ex)
            {
                msgs.Add(new SystemMessage {Exception = ex, Text = ex.Message, MessageType = MessageType.Error});
                Utilities.SystemErrorLog(ex);
            }

            return null;
        }

        public static string GetCostRangeString(decimal? costLow, decimal? costHigh)
        {
            if (!costLow.HasValue || !costHigh.HasValue)
            {
                return string.Empty;
            }

            if (costLow <= 0 && costHigh <= 0)
            {
                return "Free";
            }

            string ConvertToStringWithAppropriateDecimalPrecision(decimal cost)
            {
                var format = cost % 1 == 0 ? "C0" : "C2";
                return cost.ToString(format, new CultureInfo("en-US"));
            }

            var costLowString = costLow > 0 ? ConvertToStringWithAppropriateDecimalPrecision(costLow.Value) : "Free";
            var costHighString = ConvertToStringWithAppropriateDecimalPrecision(costHigh.Value);
            return $"{costLowString} – {costHighString}";
        }

        #endregion

        #region ManageAfterSchool

        public static SystemMessages CreateAfterSchoolAccount(string adminEmail, string adminPassword,
            string adminPhone, string accountName, DatabaseConnection dc = null)
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

        public static SystemMessages CreateSampleAccount(AccountType accountType, int packageId,
            DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();

            var email = "darye+sample@afterschoolhq.com";
            var phone = "(317) 316-0596";
            var firstName = "Sample";
            var lastName = "User";
            var password = "the best sample account ever";

            if (WebUser.LoadByEmail(email, msgs, dc) != null)
                throw new Exception("User with the same email address already exists.");

            var user = WebUserService.CreateNew(firstName, lastName, email, phone, password, true, true, true, true,
                null, msgs, dc);
            var account = user.AddNewAccount("Sample Account", "317-899-9189", true, accountType, packageId,
                BillingPeriodType.Month, null, true, user, msgs, dc);

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

            account.SetDefaultLocation(defaultLocation, new WebUser {ID = 0}, msgs, dc);

            return msgs;
        }

        public static void SetDefaultLocation(this Account account, Location location, WebUser loggedInUser,
            SystemMessages msgs, DatabaseConnection dc = null)
        {
            account.SetDefaultLocation(location.ID.Value, loggedInUser, msgs, dc);
        }

        public static void SetDefaultLocation(this Account account, int locationId, WebUser loggedInUser,
            SystemMessages msgs, DatabaseConnection dc = null)
        {
            account.DefaultLocationId = locationId;
            account.LastModifiedBy = loggedInUser?.ID;
            account.LastModifiedDate = DateTime.UtcNow;
            msgs.AddRange(account.Save(dc));
        }

        public static Account GetAfterSchoolAccount(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return Account.LoadByAccountTypeId(AccountType.AfterSchool, msgs, dc).FirstOrDefault();
        }

        #endregion

        #region ParentAccounts

        public static Registrant GetVolunteerRegistrant(this Account account, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            return Registrant.LoadByAccountIdByIsDeleted(account.ID.Value, false, msgs, dc)
                .Where(registrant => registrant.PersonTypeId == RegistrantPersonType.Volunteer)
                .OrderBy(registrant => registrant.ID)
                .FirstOrDefault();
        }
        
        public static Registrant GetFirstParentOrGuardianRegistrant(this Account account, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            return Registrant.LoadByAccountIdByIsDeleted(account.ID.Value, false, msgs, dc)
                .Where(registrant => registrant.IsParentOrGuardian)
                .OrderBy(registrant => registrant.ID)
                .FirstOrDefault();
        }

        public static Registrant GetSecondParentOrGuardianRegistrant(this Account account, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            var registrants = Registrant.LoadByAccountIdByIsDeleted(account.ID.Value, false, msgs, dc)
                .Where(registrant => registrant.IsParentOrGuardian)
                .OrderBy(registrant => registrant.ID)
                .ToList();

            if (registrants.Count > 1)
                return registrants[1];

            return null;
        }

        public static List<Registrant> GetChildrenRegistrants(this Account account, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            return Registrant.LoadByAccountIdByIsDeleted(account.ID.Value, false, msgs, dc)
                .Where(registrant => !registrant.IsParentOrGuardian)
                .OrderBy(registrant => registrant.DisplayName)
                .ToList();
        }

        /// <summary>
        ///     Create basic registrant/student record for an account user, not attached to an account, so it's attached to the
        ///     user record, data can be added later
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
        public static Registrant CreateRegistrant(this Account account, WebUser registrantOwner,
            List<WebUser> canBeAccessedBy, RegistrantType registrantType, bool isMaster, bool isParentOrGuardian,
            bool allowTextMessages, int? schoolLocationId, RegistrantProfileStatus profileStatus, WebUser loggedInUser,
            SystemMessages msgs = null, DatabaseConnection dc = null)
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
                LastModifiedBy = loggedInUser?.ID.IfEmpty(0),
                IsDeleted = false,
            };
            msgs.AddRange(registrant.Save(dc));

            if (canBeAccessedBy.IsNotEmpty() && canBeAccessedBy.Count > 0)
                registrant.AddUsersWithAccess(canBeAccessedBy, loggedInUser, msgs, dc);

            return registrant;
        }

        /// <summary>
        ///     Create basic registrant/student record for an account user, not attached to an account, so it's attached to the
        ///     user record, data can be added later
        /// </summary>
        /// <returns></returns>
        public static Registrant CreateRegistrant(this Account account, string firstName, string lastName, string email,
            WebUser registrantOwner, List<WebUser> canBeAccessedBy, RegistrantType registrantType, bool isMaster,
            bool isParentOrGuardian, bool allowTextMessages, int? schoolLocationId,
            RegistrantProfileStatus profileStatus, WebUser loggedInUser, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            Registrant registrant = null;

            if (msgs.IsEmpty()) msgs = new SystemMessages();

            Database.ExecuteTransaction
            (
                action: (SystemMessages amsgs, DatabaseConnection adc) =>
                {
                    registrant = new Registrant
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
                        LastModifiedBy = loggedInUser?.ID.IfEmpty(0),
                        FirstName = firstName,
                        LastName = lastName,
                        Email = email,
                        IsDeleted = false,
                    };
                    amsgs.AddRange(registrant.Save(adc));

                    if (canBeAccessedBy.IsNotEmpty() && canBeAccessedBy.Count > 0)
                        registrant.AddUsersWithAccess(canBeAccessedBy, loggedInUser, amsgs, adc);
                },
                onSuccess: (SystemMessages smsgs) => msgs.AddRange(smsgs),
                onFailure: (SystemMessages fmsgs) => msgs.AddRange(fmsgs),
                onException: (SystemMessages emsgs) => Utilities.SystemErrorLog(emsgs),
                dc: dc
            );

            return registrant;
        }

        /// <summary>
        ///     Create basic registrant/student record for an account user, not attached to an account, so it's attached to the
        ///     user record, data can be added later
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
        public static Registrant CreateVolunteerRegistrant(this Account account, WebUser registrantOwner,
            List<WebUser> canBeAccessedBy, bool isMaster,
            bool allowTextMessages, RegistrantProfileStatus profileStatus, WebUser loggedInUser,
            SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            var registrant = new Registrant
            {
                RegistrantTypeId = RegistrantType.Individual,
                IsMaster = isMaster,
                IsContactOnly = false,
                IsParentOrGuardian = false,
                WebUserId = registrantOwner?.ID,
                AccountId = account.ID,
                AllowTextMessages = allowTextMessages,
                SchoolLocationId = null,
                ProfileStatusId = profileStatus,
                CreateDate = DateTime.UtcNow,
                LastModifiedDate = DateTime.UtcNow,
                CreatedBy = loggedInUser?.ID.IfEmpty(0),
                LastModifiedBy = loggedInUser?.ID.IfEmpty(0),
                IsDeleted = false,
                PersonTypeId = RegistrantPersonType.Volunteer
            };
            msgs.AddRange(registrant.Save(dc));

            if (canBeAccessedBy.IsNotEmpty() && canBeAccessedBy.Count > 0)
                registrant.AddUsersWithAccess(canBeAccessedBy, loggedInUser, msgs, dc);

            return registrant;
        }

        /// <summary>
        ///     Create basic registrant/student record for an account user, not attached to an account, so it's attached to the
        ///     user record, data can be added later
        /// </summary>
        /// <returns></returns>
        public static Registrant CreateVolunteerRegistrant(this Account account, string firstName, string lastName, string email,
            WebUser registrantOwner, List<WebUser> canBeAccessedBy, bool isMaster, bool allowTextMessages,
            RegistrantProfileStatus profileStatus, WebUser loggedInUser, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            Registrant registrant = null;

            if (msgs.IsEmpty()) msgs = new SystemMessages();

            Database.ExecuteTransaction
            (
                action: (SystemMessages amsgs, DatabaseConnection adc) =>
                {
                    registrant = new Registrant
                    {
                        RegistrantTypeId = RegistrantType.Individual,
                        IsMaster = isMaster,
                        IsContactOnly = false,
                        IsParentOrGuardian = false,
                        WebUserId = registrantOwner?.ID,
                        AccountId = account.ID,
                        AllowTextMessages = allowTextMessages,
                        SchoolLocationId = null,
                        ProfileStatusId = profileStatus,
                        CreateDate = DateTime.UtcNow,
                        LastModifiedDate = DateTime.UtcNow,
                        CreatedBy = loggedInUser?.ID.IfEmpty(0),
                        LastModifiedBy = loggedInUser?.ID.IfEmpty(0),
                        FirstName = firstName,
                        LastName = lastName,
                        Email = email,
                        IsDeleted = false,
                        PersonTypeId = RegistrantPersonType.Volunteer
                    };
                    amsgs.AddRange(registrant.Save(adc));

                    if (canBeAccessedBy.IsNotEmpty() && canBeAccessedBy.Count > 0)
                        registrant.AddUsersWithAccess(canBeAccessedBy, loggedInUser, amsgs, adc);
                },
                onSuccess: (SystemMessages smsgs) => msgs.AddRange(smsgs),
                onFailure: (SystemMessages fmsgs) => msgs.AddRange(fmsgs),
                onException: (SystemMessages emsgs) => Utilities.SystemErrorLog(emsgs),
                dc: dc
            );

            return registrant;
        }

        public static bool HasRegistrationsWith(this Account familyAccount, Account providerAccount,
            SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return Account.GetConnectedEducatorAccountCount(familyAccount.ID.Value, providerAccount.ID.Value, msgs,
                dc) > 0;
        }

        #endregion

        #region Finance

        public static bool UseStripeConnectDuringCheckout(this Account account, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            if (account.IsNotEmpty())
                return account.HasFeature(FeatureType.UseStripeConnectedAccount, msgs, dc) &&
                       account.StripeChargesEnabled;

            return false;
        }

        public static Payout RequestNewPayout(this Account account, string statementDescriptor,
            List<Transaction> transactions, WebUser loggedInUser, SystemMessages msgs, DatabaseConnection dc = null)
        {
            var paymentType = PaymentType.NotSet;

            if (account.CanReceiveACHDeposits(msgs, dc))
                paymentType = PaymentType.ACH;
            else
                paymentType = PaymentType.Check;

            if (paymentType != PaymentType.NotSet && msgs.HasNoErrors())
            {
                var payout = PayoutService.CreateNew(account, statementDescriptor, transactions, PayoutStatus.Requested,
                    paymentType, loggedInUser, msgs, dc);

                if (payout.IsNotEmpty())
                    payout.SendPayoutRequestNotification(account, msgs);

                return payout;
            }

            msgs.Add("No payment account defined", MessageType.UserError);

            return null;
        }

        public static Payout GetPayout(this Account account, Guid Uid, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            if (account.IsNotEmpty())
                return Payout.LoadByUidByAccountId(Uid, account.ID.Value, msgs, dc);

            return null;
        }

        public static List<Payout> GetPayouts(this Account account, PayoutStatus? payoutStatus, DateTime? startDate,
            DateTime? endDate, int? start, int? length, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return Payout.LoadPayouts(account.ID.Value, payoutStatus, startDate, endDate, start, length, msgs, dc);
        }

        public static Payout GetLastPayout(this Account account, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            return Payout.GetLastPayout(account.ID.Value, msgs, dc);
        }

        public static decimal GetMaxAmountAvailableForPayout(this Account account, string search,
            DateTime? startDate = null, DateTime? endDate = null, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            var totalPayoutAmount = account.GetTotalPayoutAmount(search, null, startDate, endDate, msgs, dc);

            var totalAmountBilledACH = account.GetTotalAmountBilledOut(
                search,
                PaymentSourceType.AfterSchoolPlatform,
                PaymentType.ACH,
                startDate,
                endDate,
                msgs,
                dc);

            var totalAmountBilledCredit = account.GetTotalAmountBilledOut(
                search,
                PaymentSourceType.AfterSchoolPlatform,
                PaymentType.CreditCard,
                startDate,
                endDate,
                msgs,
                dc);

            var totalFeesPaidACH =
                account.GetTotalFeesPaidAsBiller(search, PaymentType.ACH, startDate, endDate, msgs, dc);
            var totalFeesPaidCredit =
                account.GetTotalFeesPaidAsBiller(search, PaymentType.CreditCard, startDate, endDate, msgs, dc);
            var totalFeesRefunded = account.GetTotalFeesRefunded(search, startDate, endDate, msgs, dc);

            var totalRefunds = account.GetTotalAmountRefundedAHQ(search, true, startDate, endDate, msgs, dc);
            var totalRefundManual = account.GetTotalAmountRefundedManually(search, startDate, endDate, msgs, dc);
            var totalDebits = account.GetTotalAmountDebited(search, startDate, endDate, msgs, dc);
            var totalCredits = account.GetTotalAmountCredited(search, startDate, endDate, msgs, dc);

            return totalAmountBilledACH +
                   totalAmountBilledCredit +
                   totalFeesRefunded +
                   totalCredits -
                   totalFeesPaidACH -
                   totalFeesPaidCredit -
                   totalPayoutAmount -
                   totalRefunds -
                   totalRefundManual -
                   totalDebits;
        }

        public static decimal GetMaxAmountAvailableForRefund(this Account account, string search,
            DateTime? startDate = null, DateTime? endDate = null, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            var totalPayoutAmount = account.GetTotalPayoutAmount(search, null, startDate, endDate, msgs, dc);

            var totalAmountBilledACH = account.GetTotalAmountBilledOut(
                search,
                PaymentSourceType.AfterSchoolPlatform,
                PaymentType.ACH,
                startDate,
                endDate,
                msgs,
                dc);

            var totalAmountBilledCredit = account.GetTotalAmountBilledOut(
                search,
                PaymentSourceType.AfterSchoolPlatform,
                PaymentType.CreditCard,
                startDate,
                endDate,
                msgs,
                dc);

            var totalConvenienceFeesReceived =
                account.GetTotalConvenienceFeesReceivedAsBiller(null, startDate, endDate, msgs, dc);
            var totalRefunds = account.GetTotalAmountRefundedAHQ(search, true, startDate, endDate, msgs, dc);
            var totalRefundManual = account.GetTotalAmountRefundedManually(search, startDate, endDate, msgs, dc);

            return totalAmountBilledACH +
                   totalAmountBilledCredit +
                   totalConvenienceFeesReceived -
                   totalPayoutAmount -
                   totalRefunds -
                   totalRefundManual;
        }

        public static int GetPayoutCount(this Account account, PayoutStatus? payoutStatus, DateTime? startDate,
            DateTime? endDate, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return Payout.GetCountByAccountId(account.ID.Value, payoutStatus, startDate, endDate, msgs, dc);
        }

        public static List<Transaction> GetBilledTransactions(this Account account, string search, DateTime? startDate,
            DateTime? endDate, bool isDeleted, int? start, int? length, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            return Transaction.LoadByBillerAccountId(account.ID.Value, search, startDate, endDate, null, isDeleted,
                start, length, msgs, dc);
        }

        public static List<Transaction> GetBilledTransactions(this Account account, string search, DateTime? startDate,
            DateTime? endDate, List<(int ID, ParticipationObjectType ObjectType)> participationObjects, bool isDeleted,
            int? start, int? length, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return Transaction.LoadByBillerAccountId(account.ID.Value, search, startDate, endDate, participationObjects,
                isDeleted, start, length, msgs, dc);
        }

        public static List<Transaction> GetPaidTransactions(this Account account, string search, DateTime? startDate,
            DateTime? endDate, bool isDeleted, int? start, int? length, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            return Transaction.LoadByPayerAccountId(account.ID.Value, search, startDate, endDate, null, isDeleted,
                start, length, msgs, dc);
        }

        public static List<Transaction> GetPaidTransactions(this Account account, string search, DateTime? startDate,
            DateTime? endDate, List<(int ID, ParticipationObjectType ObjectType)> participationObjects, bool isDeleted,
            int? start, int? length, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return Transaction.LoadByPayerAccountId(account.ID.Value, search, startDate, endDate, participationObjects,
                isDeleted, start, length, msgs, dc);
        }

        public static List<Transaction> GetTransactions(this Account account, string search, DateTime? startDate,
            DateTime? endDate, bool isDeleted, int? start, int? length, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            return Transaction.LoadByAccountId(account.ID.Value, search, startDate, endDate, null, isDeleted, start,
                length, msgs, dc);
        }

        public static List<Transaction> GetTransactions(this Account account, string search, DateTime? startDate,
            DateTime? endDate, List<(int ID, ParticipationObjectType ObjectType)> participationObjects, bool isDeleted,
            int? start, int? length, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return Transaction.LoadByAccountId(account.ID.Value, search, startDate, endDate, participationObjects,
                isDeleted, start, length, msgs, dc);
        }

        public static Transaction GetBillerTransaction(this Account account, Guid transactionUid,
            SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return Transaction.LoadByUidByBillerAccountId(transactionUid, account.ID.Value, msgs, dc);
        }

        public static Transaction GetPayerTransaction(this Account account, Guid transactionUid,
            SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return Transaction.LoadByUidByPayerAccountId(transactionUid, account.ID.Value, msgs, dc);
        }

        public static List<Transaction> GetBillerAccountTransactions(this Account account, List<int> ids,
            SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return Transaction.LoadByIdsByBillerAccountId(ids, account.ID.Value, msgs, dc);
        }

        public static List<Transaction> GetPayerAccountTransactions(this Account account, List<int> ids,
            SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return Transaction.LoadByIdsByPayerAccountId(ids, account.ID.Value, msgs, dc);
        }

        public static int GetBilledTransactionCount(this Account account, string search, DateTime? startDate,
            DateTime? endDate, bool isDeleted, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return Transaction.GetCountByBillerAccountId(account.ID.Value, search, startDate, endDate, null, isDeleted,
                msgs, dc);
        }

        public static int GetBilledTransactionCount(this Account account, string search, DateTime? startDate,
            DateTime? endDate, List<(int ID, ParticipationObjectType ObjectType)> participationObjects, bool isDeleted,
            SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return Transaction.GetCountByBillerAccountId(account.ID.Value, search, startDate, endDate,
                participationObjects, isDeleted, msgs, dc);
        }

        public static int GetPaidTransactionCount(this Account account, string search, DateTime? startDate,
            DateTime? endDate, bool isDeleted, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return Transaction.GetCountByPayerAccountId(account.ID.Value, search, startDate, endDate, null, isDeleted,
                msgs, dc);
        }

        public static int GetPaidTransactionCount(this Account account, string search, DateTime? startDate,
            DateTime? endDate, List<(int ID, ParticipationObjectType ObjectType)> participationObjects, bool isDeleted,
            SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return Transaction.GetCountByPayerAccountId(account.ID.Value, search, startDate, endDate,
                participationObjects, isDeleted, msgs, dc);
        }

        public static int GetTransactionCount(this Account account, string search, DateTime? startDate,
            DateTime? endDate, bool isDeleted, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return Transaction.GetCountByAccountId(account.ID.Value, search, startDate, endDate, null, isDeleted, msgs,
                dc);
        }

        public static int GetTransactionCount(this Account account, string search, DateTime? startDate,
            DateTime? endDate, List<(int ID, ParticipationObjectType ObjectType)> participationObjects, bool isDeleted,
            SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return Transaction.GetCountByAccountId(account.ID.Value, search, startDate, endDate, participationObjects,
                isDeleted, msgs, dc);
        }

        public static decimal CalculatePlatformConvenienceFeeAmount(this Account billingAccount,
            decimal totalAmountToCharge, SystemMessages msgs, DatabaseConnection dc = null)
        {
            if (billingAccount.HasFeature(FeatureType.UseStripeConnectedAccount, msgs, dc))
            {
                var percentageFee = billingAccount.GetAccountPackageFeature(FeatureType.PaymentsPercentageFee, msgs, dc)
                    .GetMaxValue();
                var flatFee = billingAccount.GetAccountPackageFeature(FeatureType.PaymentsPercentageFlatFee, msgs, dc)
                    .GetMaxValue();

                if (percentageFee != 0 || flatFee != 0)
                    return (totalAmountToCharge * percentageFee) + flatFee;
            }

            return new FeeCalculationObject(
                    totalAmountToCharge,
                    billingAccount,
                    msgs,
                    dc)
                .GetConvenienceFee();
        }

        public static decimal GetPlatformConveniencePercentFee(this Account billingAccount, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            return billingAccount.GetAccountPackageFeature(FeatureType.PaymentsPercentageFee, msgs, dc).GetMaxValue();
        }

        public static decimal GetPlatformConvenienceFlatFee(this Account billingAccount, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            return billingAccount.GetAccountPackageFeature(FeatureType.PaymentsPercentageFlatFee, msgs, dc)
                .GetMaxValue();
        }

        internal static string GetStripeSourceFromToken(this Account payingAccount,
            string stripeToken, StripeAccountType stripeAccountType, SystemMessages msgs = null)
        {
            var stripeHelper = StripeHelper.GetStripeHelper(stripeAccountType);
            var source = stripeHelper.CreateCreditCardSource(stripeToken, msgs);

            stripeHelper.AddNewCardToCustomer(payingAccount.StripeCustomerId, source, null, msgs);

            return source.Id;
        }

        internal static string GetStripeSourceFromPlaidToken(this Account fromAccount,
            BankAccountPurchaseObject bankAccountPurchaseObject, StripeAccountType stripeAccountType, SystemMessages msgs = null)
        {
            var plaidToken = PlaidHelper.GetPlaidToken(bankAccountPurchaseObject.PlaidPublicToken, msgs);

            if (msgs.HasNoErrors())
            {
                var bankAccount = PlaidHelper.CreateStripeBankAccount(plaidToken.AccessToken,
                    bankAccountPurchaseObject.PlaidAccountId, msgs);
                var source = StripeHelper.GetStripeHelper(stripeAccountType).AttachSourceToCustomer(fromAccount.StripeCustomerId,
                    bankAccount.StripeBankAccountToken, null, msgs);

                //return customer.DefaultSourceId;
                return source.Id;
            }

            return null;
        }

        internal static Charge CreateStripeChargeDuringCheckout(CheckoutObject checkoutObject, SystemMessages msgs,
            DatabaseConnection dc = null)
        {
            // if stripe token then don't send customer id
            string customerId = null;
            var billingAccount = checkoutObject.BillingAccount;

            if (billingAccount.IsEmpty())
            {
                msgs.Add("Stripe charge can not be processed.", MessageType.UserError);
                return null;
            }

            if (checkoutObject.SavePaymentInfo ||
                checkoutObject.HasStripeSourceId ||
                (checkoutObject.PayingAccount.IsNotEmpty() &&
                 checkoutObject.PayingAccount.StripeCustomerId.IsNotEmpty()))
            {
                customerId = checkoutObject.PayingAccount.StripeCustomerId;
            }

            // if can charge fees we bill it directly from our AHQ account
            var useStripeConnectedAccount = billingAccount.UseStripeConnectDuringCheckout(msgs, dc);
            var platformFeeAmount = checkoutObject.CalculatePlatformFeeAmount(msgs, dc);
            var amount = checkoutObject.GetStripeAmount(msgs, dc);

            // only charge tuition during checkout, because subscriptions are billed through stripe
            var charge = StripeHelper.GetStripeHelperConnectAccount().CreateCharge(
                fromCustomerId: customerId,
                destinationAccountId: useStripeConnectedAccount ? billingAccount.StripeUserId : null,
                platformFee: useStripeConnectedAccount ? platformFeeAmount : new decimal?(),
                tokenOrSourceId: checkoutObject.StripeSourceId,
                totalAmount: amount,
                description: checkoutObject.Description,
                statementDescriptor: $"{billingAccount.Name}",
                metaData: new Dictionary<string, string>
                {
                    {"ActivityUid", checkoutObject.ParticipationObject?.Uid.ToString()},
                    {"BillingAccountUid", billingAccount.Uid.ToString()},
                    {"PayingAccountUid", checkoutObject.PayingAccount.Uid.ToString()},
                    {"RegistrationUid", checkoutObject.Registration?.Uid.ToString()},
                    {"CreditCardFeePayer", checkoutObject.CreditCardFeePayer?.Uid.ToString()},
                    {"ConvenienceFeePayer", checkoutObject.ConvenienceFeePayer?.Uid.ToString()},
                    {"PurchasedBy", checkoutObject.MasterRegistrant?.DisplayName},
                    {"BilledBy", billingAccount.Name},
                },
                msgs: msgs
            );

            return charge;
        }

        internal static Purchase AddPurchase(CheckoutObject checkoutObject, WebUser loggedInUser, SystemMessages msgs,
            DatabaseConnection dc = null)
        {
            return PurchaseService.CreateNew(
                checkoutObject: checkoutObject,
                loggedInUser: loggedInUser,
                msgs: msgs,
                dc: dc);
        }

        internal static Transaction AddTransaction(CheckoutObject checkoutObject, TransactionType transactionType,
            WebUser loggedInUser, SystemMessages msgs, DatabaseConnection dc = null)
        {
            return TransactionService.CreateNew(
                checkoutObject,
                transactionType,
                loggedInUser,
                msgs,
                dc);
        }

        public static string CreateStripeCardFromToken(this Account fromAccount, string stripeToken, Customer customer, StripeAccountType stripeAccountType,
            SystemMessages msgs, DatabaseConnection dc = null)
        {
            if (stripeToken.IsEmpty())
                msgs.Add("stripe token is not valid.", MessageType.Error);

            if (customer.IsEmpty())
                msgs.Add("Stripe customer is not valid.", MessageType.Error);

            if (msgs.HasNoErrors())
            {
                var card = StripeHelper.GetStripeHelper(stripeAccountType).AddNewCardToCustomer(customer.Id, stripeToken, null, msgs);
                if (card.IsNotEmpty())
                {
                    //set this card as default           
                    UpdateStripeCard(fromAccount, card.Id, customer, stripeAccountType, msgs, dc);
                    return card.Id;
                }
            }

            return customer.DefaultSourceId;
        }

        public static void UpdateStripeCard(this Account fromAccount, string cardId, Customer customer, StripeAccountType stripeAccountType,
            SystemMessages msgs, DatabaseConnection dc = null)
        {
            if (cardId.IsEmpty())
                msgs.Add("stripe card is not valid.", MessageType.Error);

            if (customer.IsEmpty())
                msgs.Add("Stripe customer is not valid.", MessageType.Error);

            if (msgs.HasNoErrors())
            {
                StripeHelper.GetStripeHelper(stripeAccountType).UpdateCustomerDefaultCard(customer.Id, null, cardId);

                //set this card id to account, which will work as a default card                
                fromAccount.StripeDefaultCardId = cardId;
                msgs.AddRange(fromAccount.Save(dc));
            }
        }

        private static void ProcessPaymentWithStripe(CheckoutObject checkoutObject, SystemMessages msgs,
            DatabaseConnection dc = null)
        {
            // if user wants to save payment info, do it before you charge the card
            // can't use a token twice
            if (msgs.HasNoErrors())
                checkoutObject.LoadStripeSource(StripeAccountType.ConnectAccount, msgs);

            // if everything goes right then bill the person, 
            // if any errors it'll just rollback the transaction anyway :)
            // don't process charge if the user is using a payment plan
            if (msgs.HasNoErrors())
            {
                var charge = CreateStripeChargeDuringCheckout(checkoutObject, msgs, dc);
                if (charge.IsNotEmpty() && charge.Id.IsNotEmpty())
                    checkoutObject.StripeChargeId = charge.Id;
            }
        }

        public static void ProcessSubscriptionPlanPayment(this Account billingAccount, CheckoutObject checkoutObject,
            SubscriptionPlan plan, RegistrantSubscriptionPlan registrantSubscriptionPlan, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            Database.ExecuteTransaction
            (
                action: (SystemMessages amsgs, DatabaseConnection adc) =>
                {
                    // manage transaction
                    CreatePurchaseTransactionPayment(
                        checkoutObject,
                        null,
                        amsgs,
                        adc
                    );

                    // cancel stripe subscription at end of term
                    // this logic is based on total periods, we don't need it anymore since we used subscription cancellation via CancelAt property
                    // plan.CheckAndCancelPlan(registrantSubscriptionPlan, billingAccount, amsgs, adc);
                },
                onSuccess: (SystemMessages smsgs) => msgs.AddRange(smsgs),
                onFailure: (SystemMessages fmsgs) => msgs.AddRange(fmsgs),
                onException: (SystemMessages emsgs) => Utilities.SystemErrorLog(emsgs),
                dc: dc
            );
        }

        public static Registrant CreatePurchase(CheckoutObject checkoutObject, WebUser loggedInUser,
            SystemMessages msgs, DatabaseConnection dc = null)
        {
            Registrant registrant = null;

            // not the master registrant record
            if (checkoutObject.Registration.IsNotEmpty())
            {
                registrant = checkoutObject.Registration.GetRegistrant(msgs, dc);

                // need to check for existing purchase or create a new one
                checkoutObject.CreateOrUpdatePurchase(loggedInUser, msgs, dc);
            }

            return registrant;
        }

        public static void CreatePurchaseTransactionPayment(CheckoutObject checkoutObject, WebUser loggedInUser,
            SystemMessages msgs, DatabaseConnection dc = null)
        {
            var registrant = CreatePurchase(checkoutObject, loggedInUser, msgs, dc);

            // charge successful so I need to create a transaction
            var transaction = AddTransaction(
                checkoutObject,
                TransactionType.Charge,
                loggedInUser,
                msgs,
                dc);

            checkoutObject.Transaction = transaction;

            if (registrant.IsNotEmpty())
            {
                registrant.ProcessActivityPayment(checkoutObject, transaction, loggedInUser, msgs, dc);

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

                    if (ra.IsNotEmpty() && !ra.IsAssigned(RegistrationAssignmentType.Reserved))
                        ra.ChangeSeatAssignment(RegistrationAssignmentType.Reserved, loggedInUser, msgs, dc);
                }
            }
        }

        public static void ProcessPrePayFee(CheckoutObject checkoutObject, WebUser loggedInUser, SystemMessages msgs,
            DatabaseConnection dc = null)
        {
            if (checkoutObject.GetPostPayPlanCostTodayWithFees() > 0)
                ProcessPaymentWithStripe(checkoutObject, msgs, dc);

            // must wait until after charges happen to know what card id is charged for sure
            // add purchase
            if (msgs.HasNoErrors())
                CreatePurchaseTransactionPayment(checkoutObject, loggedInUser, msgs, dc);

            // add prepayment records to ensure this payment is associated with attendances later
            if (msgs.HasNoErrors())
                checkoutObject.PostPayPlan.CreatePrePayments(
                    checkoutObject.PayingAccount,
                    checkoutObject.ParticipationObject,
                    checkoutObject.Transaction,
                    checkoutObject.Registration,
                    loggedInUser,
                    msgs,
                    dc);
        }

        public static void ProcessRegistrationFee(CheckoutObject checkoutObject, WebUser loggedInUser,
            SystemMessages msgs, DatabaseConnection dc = null)
        {
            if (checkoutObject.RegistrationCost(msgs, dc) > 0)
                ProcessPaymentWithStripe(checkoutObject, msgs, dc);

            // must wait until after charges happen to know what card id is charged for sure
            // add purchase
            if (msgs.HasNoErrors())
                CreatePurchaseTransactionPayment(checkoutObject, loggedInUser, msgs, dc);
        }

        public static void ProcessSubscriptionPlan(CheckoutObject checkoutObject, Registrant submittedRecord,
            WebUser loggedInUser, SystemMessages msgs, DatabaseConnection dc = null)
        {
            submittedRecord.StartSubscriptionPlan(checkoutObject: checkoutObject, loggedInUser: loggedInUser,
                msgs: msgs, dc: dc);

            if (msgs.HasNoErrors())
                CreatePurchase(checkoutObject, loggedInUser, msgs, dc);
        }

        public static void ProcessPaymentPlan(CheckoutObject checkoutObject, Registrant submittedRecord,
            WebUser loggedInUser, SystemMessages msgs, DatabaseConnection dc = null)
        {
            // create a stripe coupon if needed
            var coupon = checkoutObject.Coupon;
            if (coupon.IsNotEmpty() && coupon.DoesApplyToRegistrationFee())
            {
                var discountValue = coupon.Amount;
                if (coupon.AmountTypeId ==
                    CouponAmountType.FlatAmount) // calculate the new discount amount for the coupon
                    discountValue = (checkoutObject.ParticipationObject.Cost - coupon.Amount) /
                                    checkoutObject.PaymentPlan.TotalPeriods;

                coupon.CreateStripeCoupon(
                    checkoutObject.BillingAccount.StripeUserId,
                    StripeCouponDuration.Repeating,
                    checkoutObject.PaymentPlan.TotalPeriods,
                    discountValue.Value,
                    msgs,
                    dc
                );
            }

            submittedRecord.StartPaymentPlan(checkoutObject: checkoutObject, loggedInUser: loggedInUser, msgs: msgs,
                dc: dc);

            if (msgs.HasNoErrors())
                CreatePurchase(checkoutObject, loggedInUser, msgs, dc);
        }

        public static void ProcessStudentRegistration(CheckoutObject checkoutObject, bool sendConfirmationEmail,
            WebUser loggedInUser, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            Database.ExecuteTransaction
            (
                action: (SystemMessages amsgs, DatabaseConnection adc) =>
                {
                    checkoutObject.Registration.SetRegistrationStatus(
                        completionStatus: checkoutObject.ParticipationObject.RequiresParentalConsent
                            ? RegistrationCompletionStatus.RequiresParentSignature
                            : RegistrationCompletionStatus.Completed,
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
                        checkoutObject.Registration.SendConfirmation(checkoutObject, null, loggedInUser, true, msgs,
                            dc);
                },
                onFailure: (SystemMessages fmsgs) => msgs.AddRange(fmsgs),
                onException: (SystemMessages emsgs) => Utilities.SystemErrorLog(emsgs),
                dc: dc
            );
        }

        public static void ProcessRegistrationWithoutPayment(CheckoutObject checkoutObject, bool sendConfirmationEmail,
            WebUser loggedInUser, SystemMessages msgs = null, DatabaseConnection dc = null)
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

                    // subscribe to the account
                    checkoutObject.BillingAccount.AddOrUpdateSubscriber(loggedInUser, SubscriberStatus.Subscribed,
                        loggedInUser, amsgs, adc);
                },
                onSuccess: (SystemMessages smsgs) =>
                {
                    msgs.AddRange(smsgs);

                    if (sendConfirmationEmail)
                        checkoutObject.Registration.SendConfirmation(checkoutObject, null, loggedInUser, true, msgs,
                            dc);
                },
                onFailure: (SystemMessages fmsgs) => msgs.AddRange(fmsgs),
                onException: (SystemMessages emsgs) => Utilities.SystemErrorLog(emsgs),
                dc: dc
            );
        }

        public static CheckoutObject ProcessRegistrationWithPayment(CheckoutObject checkoutObject, WebUser loggedInUser,
            SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            Database.ExecuteTransaction
            (
                action: (SystemMessages amsgs, DatabaseConnection adc) =>
                {
                    // TODO: Decide if master account registrant should be added to plans or the account copy
                    // get the registrant record that was created just for the account
                    var submittedRecord = checkoutObject.Registration.GetRegistrant(amsgs, adc);

                    ProcessRegistrationWithoutPayment(
                        checkoutObject: checkoutObject,
                        sendConfirmationEmail: false,
                        loggedInUser: loggedInUser,
                        msgs: amsgs,
                        dc: adc);

                    // process payment by to account, includes registration fee
                    var isRegFree = checkoutObject.ParticipationObject.IsRegistrationFree;
                    var hasPaymentPlan = checkoutObject.PaymentPlan.IsNotEmpty();
                    var hasSubscription = checkoutObject.SubscriptionPlan.IsNotEmpty();
                    var hasPostPayPlan = checkoutObject.PostPayPlan.IsNotEmpty();
                    var alreadyPaidRegFee = submittedRecord.GetAmountPaid(msgs, dc) > 0;

                    if (!isRegFree && !hasPaymentPlan && !alreadyPaidRegFee && !hasPostPayPlan)
                        ProcessRegistrationFee(checkoutObject, loggedInUser, amsgs, adc);

                    if (hasSubscription)
                        ProcessSubscriptionPlan(checkoutObject, submittedRecord, loggedInUser, amsgs, adc);

                    if (hasPostPayPlan)
                        ProcessPostPayPlan(checkoutObject, loggedInUser, amsgs, adc);

                    if (hasPaymentPlan)
                        ProcessPaymentPlan(checkoutObject, submittedRecord, loggedInUser, amsgs, adc);
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
                onException: (SystemMessages emsgs) => Utilities.SystemErrorLog(emsgs),
                dc: dc
            );

            return checkoutObject;
        }

        public static void ProcessPostPayPlan(CheckoutObject checkoutObject, WebUser loggedInUser, SystemMessages msgs,
            DatabaseConnection dc = null)
        {
            StorePaymentInfo(checkoutObject, loggedInUser, msgs, dc);

            if (msgs.HasNoErrors() && checkoutObject.PostPayPlan.NumberOfPeriodsPrePaid.IsPositive())
                ProcessPrePayFee(checkoutObject, loggedInUser, msgs, dc);
        }

        public static void StorePaymentInfo(CheckoutObject checkoutObject, WebUser loggedInUser, SystemMessages msgs,
            DatabaseConnection dc = null)
        {
            if (checkoutObject.StripeSourceId.IsEmpty())
                checkoutObject.LoadStripeSource(StripeAccountType.ConnectAccount, msgs);

            var costPerClass = checkoutObject.ParticipationObject.GetPostPayCostWithConvenienceFee();
            var convenienceFeePaid = checkoutObject.ParticipationObject.GetPostPayConvenienceFee();
            var paymentTypeFeePaid = 0M;

            if (checkoutObject.PaymentType == PaymentType.CreditCard)
            {
                costPerClass += checkoutObject.ParticipationObject.GetPostPayCreditCardFee();
                paymentTypeFeePaid = checkoutObject.ParticipationObject.GetPostPayCreditCardFee();
            }

            checkoutObject.Registration.SavePaymentSource(
                checkoutObject.StripeSourceId,
                checkoutObject.PaymentType,
                costPerClass,
                convenienceFeePaid,
                paymentTypeFeePaid,
                checkoutObject.ConvenienceFeePayer,
                checkoutObject.CreditCardFeePayer,
                checkoutObject.Coupon,
                loggedInUser,
                msgs,
                dc);
        }

        public static CheckoutObject ProcessPayment(CheckoutObject checkoutObject, WebUser loggedInUser,
            bool isAttendanceCharge = false, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            Database.ExecuteTransaction
            (
                action: (SystemMessages amsgs, DatabaseConnection adc) =>
                {
                    ProcessPaymentWithStripe(checkoutObject, amsgs, adc);

                    // must wait until after charges happen to know what card id is charged for sure
                    if (amsgs.HasNoErrors())
                        CreatePurchaseTransactionPayment(checkoutObject, loggedInUser, amsgs, adc);
                },
                onSuccess: (SystemMessages smsgs) =>
                {
                    msgs.AddRange(smsgs);
                    EmailHelper.SendPaymentConfirmation(checkoutObject, null, loggedInUser, true, msgs,
                        isAttendanceCharge, dc);
                },
                onFailure: (SystemMessages amsgs) => msgs.AddRange(amsgs),
                onException: (SystemMessages emsgs) => Utilities.SystemErrorLog(emsgs),
                dc: dc
            );

            return checkoutObject;
        }

        public static CheckPaymentAccount CreateOrUpdateCheckPaymentAccount(this Account account,
            LegalEntityType legalEntityType, string name, string address1, string address2, string city, string state,
            string postalCode, WebUser loggedInUser, SystemMessages msgs, DatabaseConnection dc = null)
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
                            AccountId = account.ID,
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
                onException: (SystemMessages emsgs) => Utilities.SystemErrorLog(emsgs),
                dc: dc
            );

            return checkAccount;
        }

        public static CheckPaymentAccount GetCheckPaymentAccount(this Account account, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            if (account.IsNotEmpty())
                return CheckPaymentAccount.LoadByAccountId(account.ID.Value, msgs, dc);

            return null;
        }

        public static BankPaymentAccount CreateOrUpdateBankPaymentAccount(this Account account, string bankName,
            BankAccountType bankAccountType, string routingNumber, string accountNumber, string accountHoldersName,
            string bankCountry, WebUser loggedInUser, SystemMessages msgs, DatabaseConnection dc = null)
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
                            AccountId = account.ID,
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
                onException: (SystemMessages emsgs) => Utilities.SystemErrorLog(emsgs),
                dc: dc
            );

            return bankAccount;
        }

        public static BankPaymentAccount GetBankPaymentAccount(this Account account, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            return BankPaymentAccount.LoadByAccountId(account.ID.Value, msgs, dc);
        }

        public static bool CanProcessPayments(this Account account, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            if (!account.HasAccountOwner || !account.GetAccountOwner(msgs, dc).VerifiedAccount)
                return false;

            var paymentFeatures = new List<FeatureType>
                {FeatureType.UseStripeConnectedAccount, FeatureType.AcceptPayments};

            if (account.HasAccountPackageFeatures(paymentFeatures, msgs, dc))
                return account.CanReceiveStripeDeposits(msgs, dc) || account.CanReceiveCheckPayouts(msgs, dc);

            return true;
        }

        public static bool CanReceivePaymentDeposits(this Account account, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            return account.CanReceiveACHDeposits(msgs, dc) || account.CanReceiveCheckPayouts(msgs, dc);
        }

        public static bool CanReceiveCheckPayouts(this Account account, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            return account.GetCheckPaymentAccount(msgs, dc).IsNotEmpty();
        }

        public static bool CanReceiveACHDeposits(this Account account, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            return account.CanReceiveStripeDeposits(msgs, dc) || account.GetBankPaymentAccount(msgs, dc).IsNotEmpty();
        }

        public static bool CanReceiveStripeDeposits(this Account account, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            return (account.HasStripeConnectedAccount && account.StripePayoutsEnabled && account.StripeChargesEnabled);
        }

        public static decimal GetTotalPayoutAmount(this Account account, string search, PayoutStatus? payoutStatus,
            DateTime? startDate = null, DateTime? endDate = null, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            return Payout.GetTotalAmountByAccountId(account.ID.Value, search, payoutStatus, startDate, endDate, msgs,
                       dc) +
                   Transaction.GetPayoutAmountForBillerAccount(account.ID.Value, msgs, dc);
        }

        public static decimal GetTotalAmountBilledOut(this Account account, string search,
            PaymentSourceType? paymentSourceType = null, PaymentType? paymentType = null, DateTime? startDate = null,
            DateTime? endDate = null, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return Transaction.GetTotalAmountBilledByBillerAccount(account.ID.Value, search, paymentSourceType,
                paymentType, startDate, endDate, false, msgs, dc);
        }

        public static decimal GetTotalFeesPaidAsBiller(this Account account, string search, PaymentType? paymentType,
            DateTime? startDate, DateTime? endDate, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return Transaction.GetTotalFeesPaidByBillerAccount(account.ID.Value, search, paymentType, startDate,
                endDate, false, msgs, dc);
        }

        public static decimal GetTotalConvenienceFeesReceivedAsBiller(this Account account, PaymentType? paymentType,
            DateTime? startDate, DateTime? endDate, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return Transaction.GetTotalConvenienceFeesReceivedByBillerAccount(account.ID.Value, paymentType, startDate,
                endDate, false, msgs, dc);
        }

        public static decimal GetTotalAmountRefunded(this Account account, string search, DateTime? startDate = null,
            DateTime? endDate = null, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return Transaction.GetTotalAmountPaidByPayerAccount(account.ID.Value, search, null, startDate, endDate,
                       false, msgs, dc) -
                   Transaction.GetTotalConvenienceFeesNotPaidByPayerAccount(account.ID.Value, search, startDate,
                       endDate, false, msgs, dc) -
                   Transaction.GetTotalPaymentTypeFeesNotPaidByPayerAccount(account.ID.Value, search, startDate,
                       endDate, false, msgs, dc);
        }

        public static decimal GetTotalAmountRefundedManually(this Account account, string search, DateTime? startDate,
            DateTime? endDate, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return Transaction.GetTotalAmountPaidByPayerAccount(account.ID.Value, search,
                PaymentSourceType.ManualProvider, startDate, endDate, false, msgs, dc);
        }

        public static decimal GetTotalFeesRefunded(this Account account, string search, DateTime? startDate,
            DateTime? endDate, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return Transaction.GetTotalConvenienceFeesPaidByPayerAccount(account.ID.Value, search, null, startDate,
                       endDate, false, msgs, dc) +
                   Transaction.GetTotalPaymentTypeFeesPaidByPayerAccount(account.ID.Value, search, null, startDate,
                       endDate, false, msgs, dc);
        }

        public static decimal GetTotalAmountRefundedAHQ(this Account account, string search, bool getProviderAmountOnly,
            DateTime? startDate, DateTime? endDate, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var totalFeesPaidByPayer = 0M;
            var amountRefunded = Transaction.GetTotalAmountPaidByPayerAccount(account.ID.Value, search,
                PaymentSourceType.AfterSchoolPlatform, startDate, endDate, false, msgs, dc);

            if (getProviderAmountOnly)
            {
                totalFeesPaidByPayer += Transaction.GetTotalConvenienceFeesNotPaidByPayerAccount(
                    account.ID.Value,
                    search,
                    startDate,
                    endDate,
                    false,
                    msgs,
                    dc);

                totalFeesPaidByPayer += Transaction.GetTotalPaymentTypeFeesNotPaidByPayerAccount(
                    account.ID.Value,
                    search,
                    startDate,
                    endDate,
                    false,
                    msgs,
                    dc);

                amountRefunded -= totalFeesPaidByPayer;
            }

            return amountRefunded;
        }

        public static decimal GetTotalAmountCredited(this Account account, string search, DateTime? startDate = null,
            DateTime? endDate = null, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return Transaction.GetTotalAmountCreditedToAccount(account.ID.Value, search, startDate, endDate, false,
                msgs, dc);
        }

        public static decimal GetTotalAmountDebited(this Account account, string search, DateTime? startDate = null,
            DateTime? endDate = null, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return Transaction.GetTotalAmountDebitedFromAccount(account.ID.Value, search, startDate, endDate, false,
                msgs, dc);
        }

        // TODO: Needs review
        public static decimal GetSubscriptionPlatformFeePercent(this Account billingAccount, SubscriptionPlan plan,
            PaymentType paymentType, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (plan.CostPerPeriod <= 0)
                return 0;

            var costPerPeriod = plan.GetCostPerPeriodPerPerson(msgs, dc);
            var calcObj = new FeeCalculationObject(
                costPerPeriod,
                billingAccount,
                plan.IsConvenienceFeeTakenOutOfCost,
                msgs,
                dc);

            var feeAmount = calcObj.GetTotalFees(paymentType);
            var totalCost = calcObj.GetFinalTotalCost(paymentType);

            return Math.Round(feeAmount / totalCost, 4);
        }

        /// <summary>
        ///     FOR AHQ Platform Payouts Only
        /// </summary>
        /// <param name="account"></param>
        /// <param name="payout"></param>
        /// <param name="loggedInUser"></param>
        /// <param name="msgs"></param>
        /// <param name="dc"></param>
        public static void SendStripePayout(this Account account, Payout payout, WebUser loggedInUser,
            SystemMessages msgs, DatabaseConnection dc = null)
        {
            var requestor = payout.GetRequestor(msgs, dc);

            if (account.StripeUserId.IsNotEmpty())
            {
                var metaData = new Dictionary<string, string>
                {
                    {"AccountUid", account.Uid.ToString()},
                    {"RequestedBy", requestor.Uid.ToString()},
                    {"ApprovedBy", loggedInUser.Uid.ToString()},
                    {"DateRequested", payout.DateRequested.ToShortDateTimeString()},
                };

                var stripeHelper = StripeHelper.GetStripeHelperConnectAccount();

                stripeHelper.CreateTransfer(
                    amount: payout.Amount.Value,
                    description:
                    $"Transfer from AHQ to {account.Name} requested by {requestor.DisplayName} and approved by {loggedInUser.DisplayName}",
                    transferIdentifier: payout.Uid.ToString(),
                    destinationId: account.StripeUserId,
                    chargeId: null,
                    metaData: metaData,
                    msgs: msgs);

                var stripeAccount = account.GetStripeConnectedAccount(msgs);

                var stripePayout = stripeHelper.CreatePayout(
                    amount: payout.Amount.Value,
                    externalAccountId: stripeAccount.ExternalAccounts.First().Id,
                    statementDescriptor: payout.StatementDescriptor.IsNotEmpty()
                        ? payout.StatementDescriptor
                        : "AHQ Payout",
                    connectedAccountId: account.StripeUserId,
                    metaData: metaData,
                    msgs: msgs);

                if (msgs.HasNoErrors() && stripePayout.IsNotEmpty())
                    payout.SetExternalPayoutId(externalPayoutId: stripePayout.Id, loggedInUser: loggedInUser,
                        msgs: msgs, dc: dc);
            }
        }

        #endregion

        #region Coupons

        public static Coupon GetCoupon(this Account account, Guid Uid, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            if (account.IsNotEmpty())
                return Coupon.LoadByUidByAccountId(Uid, account.ID.Value, msgs, dc);

            return null;
        }

        public static List<Coupon> GetCoupons(this Account account, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            if (account.IsNotEmpty())
                return Coupon.LoadByAccountId(account.ID.Value, msgs, dc);

            return null;
        }

        public static List<Coupon> GetCoupons(this Account account, string search, DateTime? startDate,
            DateTime? endDate, List<(int ID, ParticipationObjectType ObjectType)> participationObjects, bool isDeleted,
            int? start, int? length, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return Coupon.LoadByAccountId(account.ID.Value, search, startDate, endDate, participationObjects, isDeleted,
                start, length, msgs, dc);
        }

        public static AttachedCoupon AttachCoupon(this Account account, Coupon coupon, WebUser loggedInUser,
            SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (account.AccountTypeId != AccountType.Registrant)
            {
                msgs.Add("Coupon can only be applied to Registrant accounts", MessageType.Error);
                return null;
            }

            return AttachedCouponService.CreateNew(account, coupon, loggedInUser, msgs, dc);
        }

        public static List<AttachedCoupon> GetUnusedAttachedCoupons(this Account account,
            DatabaseActionHelper actionHelper)
        {
            return AttachedCoupon.LoadByAccountIdByIsUsed(account.ID.Value, false, actionHelper?.Messages,
                actionHelper?.Connection);
        }

        public static List<AttachedCoupon> GetUsedAttachedCoupons(this Account account,
            DatabaseActionHelper actionHelper)
        {
            return AttachedCoupon.LoadByAccountIdByIsUsed(account.ID.Value, false, actionHelper?.Messages,
                actionHelper?.Connection);
        }

        public static List<AttachedCoupon> GetAttachedCoupons(this Account account,
            DatabaseActionHelper actionHelper = null)
        {
            return AttachedCoupon.LoadByAccountId(account.ID.Value, actionHelper?.Messages, actionHelper?.Connection);
        }

        public static Coupon CreateorUpdateCoupon(this Account account, Coupon coupon, decimal amount,
            DateTime? expirationDate, CouponAmountType amountType, CouponApplyType applyType, int? maxUsesAll,
            int? maxUsesPerPerson, string couponCode, string name, string description,
            StripeCouponDuration couponDuration, int? durationInMonths, WebUser loggedInUser,
            SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            coupon.CreateOrUpdate(
                account: account,
                amount: amount,
                expirationDate: expirationDate,
                couponAmountType: amountType,
                couponApplyType: applyType,
                maxUsesAll: maxUsesAll,
                maxUsesPerPerson: maxUsesPerPerson,
                couponCode: couponCode,
                name: name,
                description: description,
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
                    dc);

            return coupon;
        }

        public static Coupon GetCoupon(this Account ownerAccount, string couponCode, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            return Coupon.LoadByAccountIdByCouponCode(
                ownerAccount.ID.Value,
                couponCode,
                msgs);
        }

        public static Coupon ValidateCoupon(this Account payerAccount, ParticipationObject participationObject,
            string couponCode, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            var coupon = participationObject.OwnerAccount.GetCoupon(couponCode, msgs, dc);

            if (coupon.IsEmpty())
                msgs.Add("Invalid coupon", MessageType.UserError);
            else
            {
                //var attachedCoupon = AttachedCoupon.LoadByCouponIdByAccountIdIsUsed(
                //                        coupon.ID.Value,
                //                        payerAccount.ID.Value,
                //                        false);

                var purchaseableObjectCoupon = PurchaseableObjectCoupon.LoadByCouponByPurchaseableObject(
                    coupon.ID.Value,
                    participationObject.ObjectID.Value,
                    (PurchaseableObjectType) participationObject.ObjectType);

                // if coupon is not assigned to any purchaseable object that means it can be applied to any purchaseable object
                if (coupon.HasPurchaseableObjects(msgs) && purchaseableObjectCoupon.IsEmpty())
                    msgs.Add("This coupon code is invalid", MessageType.UserError);
                else
                {
                    if (coupon.ExpirationDate.HasValue)
                    {
                        var today = participationObject.OwnerAccount.ConvertToAccountTimeZone(DateTime.UtcNow).Date;
                        if (coupon.ExpirationDate.Value.IsBefore(today))
                            msgs.Add($"Coupon is Expired on {coupon.ExpirationDate.ToShortDateString()}",
                                MessageType.UserError);
                    }

                    if (coupon.DoesApplyToSubscriptionPlan() && !participationObject.SubscriptionPlans.Any())
                        msgs.Add("Coupon can only be applied to a subscription", MessageType.UserError);

                    if (coupon.HasReachedLimit(payerAccount.ID.Value, participationObject.PostPayPlans.IsNotEmpty(),
                            msgs, dc))
                        msgs.Add("Coupon has reached the limit of uses", MessageType.UserError);
                }
            }

            if (msgs.HasErrors())
                return null;

            return coupon;
        }

        public static void MarkCouponAsUsed(this Account account, Coupon coupon, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            var attachedCoupon = AttachedCoupon.LoadByCouponIdByAccountIdIsUsed(
                coupon.ID.Value,
                account.ID.Value,
                false);

            if (attachedCoupon.IsNotEmpty())
            {
                attachedCoupon.IsUsed = true;
                attachedCoupon.Save(dc);
            }
        }

        #endregion

        #region Leagues

        public static List<ActivityGroup> GetLeagues(this Account account, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            return ActivityGroup.LoadByAccountIdByActivityGroupTypeId(account.ID.Value, ActivityGroupType.League, msgs,
                dc);
        }

        public static List<ActivityGroup> GetLeagues(this Account account, bool IsArchived, int? start, int? length,
            SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return ActivityGroup.LoadByAccountId(account.ID.Value, ActivityGroupType.League, IsArchived, start, length,
                false, msgs, dc);
        }

        public static int GetLeagueCount(this Account account, bool IsArchived, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            return ActivityGroup.GetCount(
                new Hashtable
                {
                    {"AccountId", account.ID.Value},
                    {"ActivityGroupTypeId", (int) ActivityGroupType.League},
                    {"IsArchived", IsArchived},
                },
                msgs: msgs,
                dc: dc);
        }

        public static List<AccountActivityGroup> GetExternalLeagues(this Account account, int? start, int? length,
            SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return AccountActivityGroup.LoadByAccountId(account.ID.Value, start, length, msgs, dc);
        }

        /// <summary>
        ///     return AccountActivityGroups where it is published
        ///     and is in a currently open registration period or upcoming registration period
        /// </summary>
        /// <param name="account"></param>
        /// <param name="msgs"></param>
        /// <param name="dc"></param>
        /// <returns></returns>
        public static List<AccountActivityGroup> GetPublishedExternalLeagues(this Account account,
            SystemMessages msgs = null, DatabaseConnection dc = null)
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

        public static int GetExternalLeagueCount(this Account account, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            return AccountActivityGroup.GetCount(
                new Hashtable
                {
                    {"AccountId", account.ID.Value},
                    {"ActivityGroupIsArchived", false},
                },
                msgs,
                dc);
        }

        public static int GetExternalLeagueInviteCount(this Account account, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            return AccountActivityGroup.GetCount(
                new Hashtable
                {
                    {"AccountId", account.ID.Value},
                    {"InviteStatusId", (int) InviteStatus.Invited},
                },
                msgs,
                dc);
        }

        public static SystemMessages SendSubmitTeamtoDistrictEmail(this Account vistorAccount, Account hostAccount,
            Activity activity, ActivityGroup activityGroup, bool isSubmitting, DatabaseConnection dc = null)
        {
            return new SystemMessages();
            // return EmailHelper.SendTeamSubmmittedToDistrictNotifitication(vistorAccount, hostAccount, activity, activityGroup, isSubmitting);
        }

        public static SystemMessages SendLeagueInvite(int invitedAccountId, int activityGroupId, int sendingUserId,
            string customMessage = null)
        {
            var msgs = new SystemMessages();
            var invitedAccount = Account.Load(invitedAccountId, msgs);
            var activityGroup = ActivityGroup.Load(activityGroupId, msgs);
            var sendingUser = WebUser.Load(sendingUserId, msgs);

            return EmailHelper.SendLeagueAccountInvitedEmail(invitedAccount, activityGroup, sendingUser, customMessage);
        }

        public static AccountActivityGroup InviteSchoolToLeague(this Account account, ActivityGroup activityGroup,
            WebUser user, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return AccountActivityGroupService.CreateNew(account, activityGroup, InviteStatus.Invited, user);
        }

        public static List<AccountActivityGroup> InviteSchoolsToLeague(List<Account> schools,
            ActivityGroup activityGroup, string customMessage, WebUser loggedInuser, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            var accountActivityGroups = new List<AccountActivityGroup>();

            Database.ExecuteTransaction
            (
                action: (SystemMessages amsgs, DatabaseConnection adc) =>
                {
                    schools.ForEach(school =>
                        accountActivityGroups.Add(school.InviteSchoolToLeague(activityGroup, loggedInuser, amsgs,
                            adc)));
                },
                onSuccess: (SystemMessages smsgs) => { msgs.AddRange(smsgs); },
                onFailure: (SystemMessages fmsgs) => { msgs.AddRange(fmsgs); },
                onException: (SystemMessages emsgs) => { Utilities.SystemErrorLog(emsgs); }
            );

            return accountActivityGroups;
        }

        public static bool IsProviderForOtherAccounts(this Account account, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            return Activity.GetCount(new Hashtable {{"ProviderAccountId", account.ID.Value}}, msgs, dc) > 0;
        }

        public static bool IsNewProviderForOtherAccounts(this Account account, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            return account.IsProviderForOtherAccounts(msgs, dc) && !account.CanReceivePaymentDeposits(msgs, dc);
        }

        #endregion

        #region Stats

        public static decimal GetTotalHoursEnrolled(this Account account, DateTime? startDateTime = null,
            DateTime? endDateTime = null, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return Account.LoadTotalMinutesEnrolled(account.ID.Value, startDateTime, endDateTime, msgs, dc) / 60;
        }

        public static int GetTotalSeatsOffered(this Account account, DateTime? startDateTime = null,
            DateTime? endDateTime = null, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return Account.LoadTotalSeatsOffered(account.ID.Value, startDateTime, endDateTime, msgs, dc);
        }

        public static decimal GetTotalHoursOffered(this Account account, DateTime? startDateTime = null,
            DateTime? endDateTime = null, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return Account.LoadTotalMinutesOffered(account.ID.Value, startDateTime, endDateTime, msgs, dc) / 60;
        }

        public static int GetTotalStudentsServed(this Account account, DateTime? startDateTime = null,
            DateTime? endDateTime = null, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return Account.LoadTotalStudentsServed(account.ID.Value, startDateTime, endDateTime, msgs, dc);
        }

        public static List<(string ActivityName, int RegistrationCount)> GetRegistrationCountByActivity(
            this Account account, DateTime? startDateTime, DateTime? endDateTime, SystemMessages msgs,
            DatabaseConnection dc = null)
        {
            return Account.LoadRegistrationCountByActivity(account.ID.Value, startDateTime, endDateTime, msgs, dc);
        }

        public static List<(string Grade, int RegistrationCount)> GetRegistrationCountByGrade(this Account account,
            DateTime? startDateTime, DateTime? endDateTime, SystemMessages msgs, DatabaseConnection dc = null)
        {
            return Account.LoadRegistrationCountByGrade(account.ID.Value, startDateTime, endDateTime, msgs, dc);
        }

        public static (decimal totalProcessed, decimal totalProfit) GetTotalProcessedAndProfit(this Account account,
            DateTime? startDateTime, DateTime? endDateTime, SystemMessages msgs, DatabaseConnection dc = null)
        {
            return Account.LoadTotalProcessedAndProfit(account.ID.Value, startDateTime, endDateTime, msgs, dc);
        }

        #endregion

        #region "Profile"

        public static AccountProfile GetOrCreateAccountProfile(this Account account, WebUser loggedInUser,
            SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var profile = account.GetAccountProfile(msgs, dc);

            if (profile.IsEmpty())
                profile = AccountProfileService.CreateNewAccountProfile(account, loggedInUser, msgs, dc);

            return profile;
        }

        public static AccountProfile GetAccountProfile(this Account account, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            return AccountProfile.LoadByAccountId(account.ID.Value, msgs, dc);
        }

        public static bool HasAccountProfile(this Account account, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            return AccountProfile.GetCountByAccountId(account.ID.Value, msgs, dc) > 0;
        }

        public static Subscriber AddOrUpdateSubscriber(this Account account, WebUser user, SubscriberStatus status,
            WebUser loggedInUser, SystemMessages msgs, DatabaseConnection dc = null)
        {
            var subscriber = Subscriber.LoadByObjectIdByObjectTypeIdByWebUserId(
                account.ID.Value,
                AHQObjectType.Account,
                user.ID.Value,
                msgs,
                dc);

            if (subscriber.IsEmpty())
                return SubscriberService.CreateNew(account, user, status, loggedInUser, msgs, dc);

            subscriber.UpdateSubscriber(status, loggedInUser, msgs, dc);

            return subscriber;
        }

        public static int GetSubscriberCount(this Account account, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            return Subscriber.GetCountByObjectByStatus(account.ID.Value, AHQObjectType.Account,
                SubscriberStatus.Subscribed, msgs, dc);
        }

        #endregion

        #region MessageCount

        public static List<ActivityMessageRegistrant> GetActivityMessageRegistrantsForCurrentMessagePeriod(
            this Account account, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var activityMessages = account.GetActivityMessages(msgs, dc);
            var activityMessageRegistrants = new List<ActivityMessageRegistrant>();
            var currentMessagePeriodActivivityRegistrants = new List<ActivityMessageRegistrant>();
            var packagePurchaseDate = account.GetActiveAccountPackage(msgs, dc).CreateDate.Value;

            var purchaseDayOfMonth = packagePurchaseDate.Day;

            if (DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month) < purchaseDayOfMonth)
                purchaseDayOfMonth = DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month);

            var thisMonthsMessageCountReset = new DateTime(DateTime.Now.Year, DateTime.Now.Month, purchaseDayOfMonth);
            var mostRecentMessageCountReset = DateTime.Now.IsBefore(thisMonthsMessageCountReset)
                ? thisMonthsMessageCountReset.AddMonths(-1)
                : thisMonthsMessageCountReset;

            activityMessages.ForEach(am =>
                activityMessageRegistrants.AddRange(am.GetActivityMessageRegistrants(msgs, dc)));

            return activityMessageRegistrants.Where(amr =>
                amr.CreateDate.Value.IsInRange(mostRecentMessageCountReset, DateTime.Now.ToUniversalTime())).ToList();
        }

        public static int GetTextMessageCountForCurrentMessagePeriod(this Account account, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            return account.GetActivityMessageRegistrantsForCurrentMessagePeriod(msgs, dc).Where(amr => amr.SentText)
                .ToList().Count();
        }

        public static int GetEmailCountForCurrentMessagePeriod(this Account account, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            return account.GetActivityMessageRegistrantsForCurrentMessagePeriod(msgs, dc).Where(amr => amr.SentEmail)
                .ToList().Count();
        }

        public static bool WillExceedEmailQuota(this Account account, AccountPackage accountPackage, int messageCount,
            WebUser loggedInUser, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            // get current limit
            var maxEmailCount = accountPackage.HasFeature(FeatureType.EmailMessaging, msgs, dc)
                ? (int) accountPackage.GetFeature(FeatureType.EmailMessaging).Max.Value
                : 0;

            // get current quota
            var amr = ActivityMessageRegistrant.LoadByCreatedBy(loggedInUser.ID.Value.ToString(), msgs, dc)
                .Where(m => m.CreateDate > DateTime.Now.AddMonths(-1).Date);
            var usedEmailQuota = account.GetEmailCountForCurrentMessagePeriod(msgs, dc);

            //add additional registransts
            usedEmailQuota += messageCount;

            // if exceeds, return error
            if (usedEmailQuota > maxEmailCount)
            {
                msgs.Add("Email quota exceeded for current user.", MessageType.UserError);
                return true;
            }

            return false;
        }

        #endregion

        #region REFUND-REQUEST

        public static RefundRequest GetRefundRequest(this Account account, Guid Uid, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            if (account.IsNotEmpty())
                return RefundRequest.LoadByUidByAccountId(Uid, account.ID.Value, msgs, dc);

            return null;
        }

        public static List<RefundRequest> GetRefundRequests(this Account account, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            return RefundRequest.LoadByAccountId(account.ID.Value, msgs, dc);
        }

        #endregion

        #region Stripe Manual Payout

        public static SystemMessages PreparePayouts(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (msgs.IsEmpty())
                msgs = new SystemMessages();

            msgs.Add($"Payout job started on at {DateTime.UtcNow.ToShortDateString()}.", MessageType.Success);

            //get accounts with ManualStripePayouts enabled
            var accountsToGeneratePayout =
                GetAccountsWithSpecificFeatures(featureTypeId: FeatureType.ManualStripePayouts, msgs: msgs, dc: dc);
            var webUser = WebUserService.GetAdminUserForLogs(msgs, dc);

            if (accountsToGeneratePayout.IsNotEmpty())
                accountsToGeneratePayout.PrepareManualPayouts(loggedInUser: webUser, msgs: msgs, dc: dc);
            else
                msgs.Add($"No accounts found for payout.", MessageType.Success);

            if (msgs.HasNoErrors())
                msgs.Add($"Payout job completed on at {DateTime.UtcNow.ToShortDateString()}.", MessageType.Success);

            WebSessionActionService.SaveSummary(webUser, msgs, dc);
            return msgs;
        }

        public static void PrepareManualPayouts(this List<Account> accounts, WebUser loggedInUser,
            SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            foreach (var account in accounts)
            {
                msgs.Add($"Payout process started for account: ({account.UniqueName})", MessageType.Success);

                //check for account whether it's getting paid via check or not and based on that we can use them in manual payouts
                if (account.CanReceiveACHDeposits(msgs: msgs, dc: dc) &&
                    !account.CanReceiveCheckPayouts(msgs: msgs, dc: dc))
                {
                    msgs.Add($"({account.UniqueName}) CanReceiveACHDeposits and not opted for check payouts.",
                        MessageType.Success);

                    var activities = account.GetActivities(msgs: msgs, dc: dc);

                    foreach (var activity in activities)
                        activity.ProcessActivityPayouts(currentUser: loggedInUser, account: account, msgs: msgs,
                            dc: dc);
                }
            }
        }

        #endregion

        #region Student-Registration

        public static (Account, WebUser, Registrant) ProcessStudentRegistration(string firstName, string lastName,
            string email, string phone, string password, int? schoolLocationId, bool allowTextMessages,
            bool isAfterSchool,
            List<KeyValuePair<FormQuestion, string>> questionAnswerModel, Account parentAccount = null,
            SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            Account studentAccount = null;
            WebUser studentUser = null;
            Registrant studentRegistrant = null;

            Database.ExecuteTransaction(
                action: (SystemMessages amsgs, DatabaseConnection adc) =>
                {
                    studentUser = WebUserService.CreateNewWithOptionalPhone(
                        firstName: firstName,
                        lastName: lastName,
                        email: email,
                        phone: phone,
                        password: password,
                        isActive: true,
                        isRegistered: true,
                        isVerified: false,
                        allowTextMessages: allowTextMessages,
                        loggedInUser: null,
                        msgs: amsgs,
                        dc: adc);

                    if (amsgs.HasNoErrors() && studentUser.IsNotEmpty())
                    {
                        if (parentAccount.IsNotEmpty())
                        {
                            parentAccount.AddUser(studentUser, UserType.Student, studentUser, amsgs, adc);

                            // create student registrant record
                            studentRegistrant = parentAccount.CreateRegistrant(
                                registrantOwner: studentUser,
                                canBeAccessedBy: studentUser.AsList(),
                                registrantType: RegistrantType.Individual,
                                isMaster: true,
                                isParentOrGuardian: false,
                                allowTextMessages: false,
                                schoolLocationId: schoolLocationId,
                                profileStatus: RegistrantProfileStatus.Completed,
                                loggedInUser: studentUser,
                                msgs: amsgs,
                                dc: adc);
                        }
                        else
                        {
                            // create account
                            studentAccount = studentUser.AddNewStudentAccount(
                                userType: UserType.Owner,
                                isActive: true,
                                isAfterSchool: isAfterSchool,
                                loggedInUser: studentUser,
                                msgs: amsgs,
                                dc: adc);

                            if (amsgs.HasNoErrors() && studentAccount.IsNotEmpty())
                            {
                                // create student registrant record
                                studentRegistrant = studentAccount.CreateRegistrant(
                                    registrantOwner: studentUser,
                                    canBeAccessedBy: studentUser.AsList(),
                                    registrantType: RegistrantType.Individual,
                                    isMaster: true,
                                    isParentOrGuardian: false,
                                    allowTextMessages: false,
                                    schoolLocationId: schoolLocationId,
                                    profileStatus: RegistrantProfileStatus.Completed,
                                    loggedInUser: studentUser,
                                    msgs: amsgs,
                                    dc: adc);
                            }
                        }

                        if (amsgs.HasNoErrors() && studentRegistrant.IsNotEmpty())
                        {
                            // add or update answer for registratnt
                            questionAnswerModel.ForEach(qvm =>
                            {
                                studentRegistrant.AddOrUpdateAnswer(
                                    answerText: qvm.Value,
                                    question: qvm.Key,
                                    loggedInUser: studentUser,
                                    msgs: amsgs,
                                    dc: adc);
                            });
                        }
                    }
                },
                onSuccess: (SystemMessages smsgs) => msgs.AddRange(smsgs),
                onFailure: (SystemMessages fmsgs) => msgs.AddRange(fmsgs),
                onException: (SystemMessages emsgs) => Utilities.SystemErrorLog(emsgs),
                dc: dc);

            return (studentAccount, studentUser, studentRegistrant);
        }

        public static (WebUser, Registrant) ProcessParentRegistration(this Account studentAccount, string firstName,
            string lastName, string email, string browserUrl, bool allowTextMessages,
            List<KeyValuePair<FormQuestion, string>> questionAnswerModel, WebUser studentUser,
            SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            WebUser parentUser = null;
            Registrant parentRegistrant = null;

            Database.ExecuteTransaction(
                action: (SystemMessages amsgs, DatabaseConnection adc) =>
                {
                    // invite parent: 
                    parentUser = studentAccount.InviteParentUser(
                        firstName: firstName,
                        lastName: lastName,
                        email: email,
                        phone: string.Empty,
                        message:
                        $"{studentUser.FirstName} has created an account on AfterSchool HQ to register for {browserUrl}.",
                        userType: UserType.Owner,
                        canUseExistingUserAccount: true,
                        loggedInUser: studentUser,
                        msgs: amsgs,
                        dc: adc);

                    if (amsgs.HasNoErrors() && parentUser.IsNotEmpty())
                    {
                        // Create parent registrant
                        parentRegistrant = studentAccount.CreateRegistrant(
                            firstName: parentUser.FirstName,
                            lastName: parentUser.LastName,
                            email: parentUser.Email,
                            registrantOwner: parentUser,
                            canBeAccessedBy: parentUser.AsList(),
                            registrantType: RegistrantType.Individual,
                            isMaster: true,
                            isParentOrGuardian: true,
                            allowTextMessages: allowTextMessages,
                            schoolLocationId: null,
                            profileStatus: RegistrantProfileStatus.Completed,
                            loggedInUser: studentUser,
                            msgs: amsgs,
                            dc: adc);

                        if (amsgs.HasNoErrors() && parentRegistrant.IsNotEmpty())
                        {
                            // add answers to student registrant record
                            questionAnswerModel.ForEach(qvm =>
                            {
                                parentRegistrant.AddOrUpdateAnswer(
                                    answerText: qvm.Value,
                                    question: qvm.Key,
                                    loggedInUser: studentUser,
                                    msgs: amsgs,
                                    dc: adc);
                            });
                        }
                    }
                },
                onSuccess: (SystemMessages smsgs) => msgs.AddRange(smsgs),
                onFailure: (SystemMessages fmsgs) => msgs.AddRange(fmsgs),
                onException: (SystemMessages emsgs) => Utilities.SystemErrorLog(emsgs),
                dc: dc);

            return (parentUser, parentRegistrant);
        }

        public static (Registration, Registrant) ProcessRegisterStudent(Account ownerAccount, Registrant registrant,
            List<KeyValuePair<FormQuestion, string>> questionAnswerModel, int? activityGroupId,
            int? accountActivityGroupId, int? periodId, List<FilesCollection> files,
            WebUser loggedInUser, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            Registration registration = null;
            Registrant submittedRecord = null;

            Database.ExecuteTransaction(
                action: (SystemMessages amsgs, DatabaseConnection adc) =>
                {
                    // account gets copy / snapshot of master registrant record
                    // required in order to make sure registration data doesn't change as registrant changes information in master record.
                    submittedRecord = registrant.SubmitRecordToAccount(
                        ownerAccount: ownerAccount,
                        loggedInUser: loggedInUser,
                        msgs: amsgs,
                        dc: adc);

                    // Create Registration
                    registration = submittedRecord.AddRegistration(
                        ownerAccountId: ownerAccount.ID.Value,
                        activityGroupId: activityGroupId,
                        accountActivityGroupId: accountActivityGroupId,
                        periodId: periodId,
                        completionStatus: RegistrationCompletionStatus.RequiresParentSignature,
                        loggedInUserId: loggedInUser.ID.Value,
                        msgs: amsgs,
                        dc: adc);

                    if (questionAnswerModel.IsEmpty() || questionAnswerModel.Count == 0) return;

                    questionAnswerModel.Where(qvm => qvm.Key.QuestionDataTypeId != QuestionDataType.NotSet).ToList()
                        .ForEach(qvm =>
                        {
                            registrant.AddOrUpdateAnswer(
                                answerText: qvm.Value,
                                question: qvm.Key,
                                loggedInUser: loggedInUser,
                                msgs: amsgs,
                                dc: adc);
                        });

                    AddCustomQuestions(questionAnswerModel: questionAnswerModel, submittedRecord: submittedRecord,
                        registration: registration, files: files, currentUser: loggedInUser, msgs: amsgs, dc: adc);

                    registrant.CopyAnswersTo(
                        submittedRecord,
                        registration,
                        loggedInUser,
                        amsgs,
                        adc);
                },
                onSuccess: (SystemMessages smsgs) => msgs.AddRange(smsgs),
                onFailure: (SystemMessages fmsgs) => msgs.AddRange(fmsgs),
                onException: (SystemMessages emsgs) => Utilities.SystemErrorLog(emsgs),
                dc: dc);

            return (registration, submittedRecord);
        }

        private static void AddCustomQuestions(List<KeyValuePair<FormQuestion, string>> questionAnswerModel,
            Registrant submittedRecord, Registration registration, List<FilesCollection> files,
            WebUser currentUser, SystemMessages msgs, DatabaseConnection dc = null)
        {
            // add custom answers to questions directly to this registration
            questionAnswerModel.Where(qvm => qvm.Key.QuestionDataTypeId == QuestionDataType.NotSet)
                .ToList()
                .ForEach(qvm =>
                {
                    if (qvm.Key.QuestionTypeId == QuestionType.FileUpload)
                    {
                        var name = $"question-{(int) qvm.Key.ID}";
                        if (files.IsNotEmpty() && files.Any(m => m.FileName == name) &&
                            files.FirstOrDefault(m => m.FileName == name).ContentLength > 0)
                        {
                            var file = files.FirstOrDefault(m => m.FileName == name);
                            //5 MB = 5242880 Bytes
                            //File uploaded here should be less than of 5MB
                            if (file.ContentLength <= 5242880)
                                submittedRecord.AddOrUpdateAnswer(
                                    file: file.InputStream,
                                    filename: file.FileName,
                                    question: qvm.Key,
                                    registrationId: registration.ID,
                                    loggedInUser: currentUser,
                                    msgs: msgs,
                                    dc: dc);
                            else
                                msgs.Add("Upload a file less than or equal to 5 megabytes", MessageType.UserError);
                        }
                    }
                    else
                        submittedRecord.AddOrUpdateAnswer(
                            answerText: qvm.Value,
                            question: qvm.Key,
                            registrationId: registration?.ID,
                            loggedInUser: currentUser,
                            msgs: msgs,
                            dc: dc);
                });
        }

        #endregion
    }
}