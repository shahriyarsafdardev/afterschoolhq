using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BasicLibrary;
using BasicLibrary.DataAccess;
using AfterSchoolLibrary.Services;

namespace AfterSchoolLibrary
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

        public static List<Activity> GetPublishedActivities(this Account account, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var periods = account.GetOpenAndUpcomingRegistrationPeriods(msgs, dc);
            var activities = new List<Activity>();

            periods.ForEach(period => activities.AddRange(period.GetActivities(msgs, dc).Where(x => x.IsPubliclyViewable)));

            return activities;
        }

        public static List<Activity> GetPublishedActivitiesInLocations(this Account account, List<int> locationIds, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var periods = account.GetOpenAndUpcomingRegistrationPeriods(msgs, dc);
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

        public static List<Activity> GetActivities(this Account account, UserType userType, WebUser user, bool? isArchived, bool? isDeleted, int? start = null, int? length = null, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (user.IsEmpty() || new UserType[] { UserType.Administrator, UserType.Owner, UserType.SuperAdmin }.Contains(userType))
            {
                return Activity.LoadByAccountId(account.ID.Value, null, isArchived, isDeleted, start, length, msgs, dc).ToList();
            }
            else
            {
                return Activity.LoadByAccountId(account.ID.Value, null, isArchived, isDeleted, start, length, msgs, dc)
                    .Where(activity => user.HasAccessToActivity(activity, msgs, dc))
                    .ToList();
            }
        }

        public static int GetActivityCount(this Account account, UserType userType, WebUser user, bool? isArchived, bool? isDeleted, DatabaseConnection dc = null)
        {
            if (user.IsEmpty() || new UserType[] { UserType.Administrator, UserType.Owner, UserType.SuperAdmin }.Contains(userType))
            {
                return Activity.GetCountByAccountId(account.ID.Value, null, isArchived, isDeleted, dc: dc);
            }
            else
            {
                return account.GetActivities(userType, user, isArchived, isDeleted, dc: dc).Count();
            }
        }

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

        public static List<Registrant> GetRegistrantsByActivityId(this Account account, int activityId, int? start = null, int? length = null, DatabaseConnection dc = null)
        {
            return account.GetRegistrantActivitiesByActivityId(activityId, start, length, dc: dc)
                .Select(x => x.ToRegistrant())
                .ToList();
        }

        public static List<RegistrantActivity> GetRegistrantActivitiesByActivityId(this Account account, int activityId, int? start = null, int? length = null, DatabaseConnection dc = null)
        {
            return RegistrantActivity.LoadByActivityId(activityId, start, length, dc: dc)
                .Where(x => x.ActivityAccountId == account.ID)
                .ToList();
        }

        public static int GetRegistrantActivityCountByActivityId(this Account account, int activityId, DatabaseConnection dc = null)
        {
            return RegistrantActivity.GetCountByActivityIdByAccountId(activityId, account.ID.Value, dc: dc);
        }

        public static List<Form> GetForms(this Account account, DatabaseConnection dc = null)
        {
            return Form.LoadByAccountId(account.ID.Value, dc: dc);
        }

        public static List<ActivityGroup> GetLeagues(this Account account, DatabaseConnection dc = null)
        {
            return ActivityGroup.LoadByAccountIdByActivityGroupTypeId(account.ID.Value, ActivityGroupType.League, dc: dc);
        }

        public static List<ActivityGroup> GetLeagues(this Account account, bool IsArchived, int? start, int? length, DatabaseConnection dc = null)
        {
            return ActivityGroup.LoadByAccountId(account.ID.Value, ActivityGroupType.League, start, length, dc: dc);
        }

        public static int GetLeagueCount(this Account account, bool IsArchived, DatabaseConnection dc = null)
        {
            return ActivityGroup.GetCount(
                new Hashtable() {
                    { "AccountId", account.ID.Value },
                    { "ActivityGroupTypeId", (int)ActivityGroupType.League },
                    { "IsArchived", IsArchived }
                },
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
                dc)
                .Where(period => period.IsOpen() || period.WillOpen())
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
        
        public static List<ActivityGroup> GetSessions(this Account account, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return ActivityGroup.LoadByAccountIdByActivityGroupTypeId(account.ID.Value, ActivityGroupType.Session, msgs, dc);
        }

        public static List<ActivityGroup> GetPublishedSessions(this Account account, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return ActivityGroup.LoadByAccountIdByIsPublished(account.ID.Value, true, msgs, dc);
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

        public static List<ActivitySchedule> GetActivitySchedules(this Account account, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return ActivitySchedule.LoadByAccountId(account.ID.Value, msgs, dc);
        }

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

        public static List<RegistrationPeriod> GetClosedRegistrationPeriods(this Account account, DatabaseConnection dc = null)
        {
            return account.GetRegistrationPeriods()
                .Where(x => x.EndDateTime.Value < account.ConvertToAccountTimeZone(DateTime.UtcNow))
                .ToList();
        }

        public static List<RegistrationPeriod> GetOpenAndUpcomingRegistrationPeriods(this Account account, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var periods = account.GetOpenRegistrationPeriods(msgs, dc);
            periods.AddRange(account.GetUpcomingRegistrationPeriods(msgs, dc));
            return periods;
        }

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
            var form = Form.GetOrCreateAfterSchoolAccountTemplateForm(msgs, dc);

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

        public static Location GetDefaultLocation(this Account account, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return account.DefaultLocationId.HasValue ? Location.Load(account.DefaultLocationId.Value, msgs, dc) : null;
        }

        public static List<AccountWebUser> GetMembers(this Account account, DatabaseConnection dc = null)
        {
            return AccountWebUser.LoadByAccountId(account.ID.Value, dc: dc);
        }

        public static List<AccountWebUser> GetActiveMembers(this Account account, DatabaseConnection dc = null)
        {
            return AccountWebUser.LoadByAccountIdByIsActive(account.ID.Value, true, dc: dc);
        }

        public static AccountPackage GetActiveAccountPackage(this Account account, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return AccountPackage.LoadByAccountIdByIsActive(account.ID.Value, true, msgs, dc);
        }

        public static List<AccountPackage> GetAccountPackages(this Account account, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return AccountPackage.LoadByAccountId(account.ID.Value, msgs, dc);
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
                dc: dc
            );

            return msgs;
        }

        public static WebUser AddUser(this Account account, string firstName, string lastName, string email, UserType ut, int loggedInUserId, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return account.AddUser(firstName, lastName, email, null, false, true, false, ut, loggedInUserId, msgs, dc: dc);
        }

        public static WebUser AddUser(this Account account, string firstName, string lastName, string email, string password, bool isActive, bool isRegistered, bool isVerified, UserType ut, int loggedInUserId, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();
            WebUser user = null;

            Database.ExecuteTransaction(
                action: (SystemMessages amsgs, DatabaseConnection adc) =>
                {
                    if (!WebUser.ExistsByEmail(email.Trim(), amsgs, adc))
                    {
                        user = new WebUser();
                        user.FirstName = firstName;
                        user.LastName = lastName;
                        user.Email = email.Trim();
                        user.Username = WebUserService.GenerateUsername(firstName, lastName, dc: adc);
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
                        amsgs.Add("Email already exists", MessageType.UserError);
                },
                onSuccess: (SystemMessages smsgs) =>
                {
                    msgs.AddRange(smsgs);
                },
                onFailure: (SystemMessages fmsgs) =>
                {
                    msgs.AddRange(fmsgs);
                },
                dc: dc
            );

            if (msgs.HasNoErrors())
                msgs.AddRange(AddUser(account, user, ut, loggedInUserId, dc: dc));

            return user;

        }

        public static SystemMessages SendWelcomeEmailTrial(this Account account, DatabaseConnection dc = null)
        {
            return EmailHelper.SendWelcomeTrial(account.GetAccountOwner(dc: dc), account);
        }

        public static SystemMessages SendSubscriptionPaymentSucceeded(this Account account, SubscriptionPlan plan, DatabaseConnection dc)
        {
            var msgs = new SystemMessages();
            var accountOwner = account.GetAccountOwner(msgs, dc);            
            msgs.AddRange(EmailHelper.SendSubscriptionPaymentSucceeded(account, accountOwner, plan, account.IsAfterSchool));
            return msgs;
        }

        public static SystemMessages SendSubscriptionPaymentFailed(this Account account, SubscriptionPlan plan, DatabaseConnection dc)
        {
            var msgs = new SystemMessages();
            var accountOwner = account.GetAccountOwner(msgs, dc);
            msgs.AddRange(EmailHelper.SendSubscriptionPaymentFailed(account, accountOwner, plan, account.IsAfterSchool));
            return msgs;
        }

        public static SystemMessages SendStripeAccountVerified(this Account account, DatabaseConnection dc)
        {
            var msgs = new SystemMessages();
            var accountOwner = account.GetAccountOwner(msgs, dc);
            msgs.AddRange(EmailHelper.SendStripeAccountVerified(account, accountOwner, account.IsAfterSchool));
            return msgs;
        }

        public static SystemMessages SendWelcomeEmailPurchase(this Account account, DatabaseConnection dc = null)
        {
            return EmailHelper.SendWelcomePurchase(account.GetAccountOwner(dc: dc), account);
        }

        public static SystemMessages SendInvitedEmail(this Account invitedAccount, WebUser invitedUser, WebUser loggedInUser, string customMessage = null, DatabaseConnection dc = null)
        {
            return EmailHelper.SendAccountInvitedEmail(invitedUser, invitedAccount, loggedInUser, customMessage);
        }

        public static WebUser InviteUser(this Account account, string firstName, string lastName, string email, string message, UserType ut, WebUser loggedInUser, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if(msgs.IsEmpty()) msgs = new SystemMessages();

            var user = account.AddUser(firstName, lastName, email, ut, loggedInUser.ID.Value, msgs, dc);
            if (msgs.HasNoErrors())
                msgs.AddRange(user.SendInviteToAccount(account, message, loggedInUser, dc: dc));
            
            if (msgs.HasErrors())
                msgs.Add($"An invitation was not sent to ${email.Trim()}, a system error has occurred", MessageType.UserError);       

            return user;
        }

        public static SystemMessages SendLeagueInvite(int invitedAccountId, int activityGroupId, int sendingUserId, string customMessage = null)
        {
            var msgs = new SystemMessages();
            var invitedAccount = Account.Load(invitedAccountId, msgs);
            var activityGroup = ActivityGroup.Load(activityGroupId, msgs);
            var sendingUser = WebUser.Load(sendingUserId, msgs);

            return EmailHelper.SendLeagueAccountInvitedEmail(invitedAccount, activityGroup, sendingUser, customMessage);
        }

        public static string GenerateUniqueName(string name, int? accountId, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            //unique name
            var reg = new System.Text.RegularExpressions.Regex(BasicLibrary.Utilities.UrlFriendlyRegEx, System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.Singleline);

            var origUniqueName = name.GetMaxWords(25);
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

            if(account.IsNotEmpty())
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

        public static SystemMessages AddCreditCard(this Account account, string stripeToken, bool makePrimary, WebUser loggedInUser, DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();

            if (account.StripeCustomerId.IsEmpty())
            {
                msgs.AddRange(account.CreateStripeCustomer(dc));
            }

            if (msgs.HasNoErrors())
            {
                var card = Billing.StripeHelper.CreateCard(
                    customerId: account.StripeCustomerId,
                    token: stripeToken,
                    msgs: msgs
                    );

                if (!card.IsEmpty() && makePrimary)
                {
                    account.StripeDefaultCardId = card.Id;
                    account.LastModifiedDate = DateTime.UtcNow;
                    account.LastModifiedBy = loggedInUser?.ID.IfEmpty(0);
                    msgs.AddRange(account.Save(dc));
                    msgs.AddRange(account.SetStripeDefaultCard(card.Id));
                }

                if (msgs.HasNoErrors())
                {
                    msgs.Add("The payment card has been successfully added", MessageType.Success);
                }
            }

            return msgs;
        }

        [Obsolete]
        public static Stripe.StripeCard GetStripeCard(this Account account, string cardId, SystemMessages msgs = null)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            var card = new Stripe.StripeCard();
            var customer = account.GetStripeCustomer(msgs);

            if (customer.IsNotEmpty())
            {
                card = Billing.StripeHelper.GetCard(customer.Id, cardId, msgs);
            }

            return card;
        }

        [Obsolete]
        public static List<Stripe.StripeCard> GetStripeCards(this Account account, SystemMessages msgs = null)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            var cards = new List<Stripe.StripeCard>();
            var customer = account.GetStripeCustomer(msgs);

            if (customer.IsNotEmpty())
            {
                cards.AddRange(Billing.StripeHelper.GetCards(customer.Id, msgs));
            }

            return cards;
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

        [Obsolete]
        public static SystemMessages SetStripeDefaultCard(this Account account, string cardId)
        {
            var msgs = new SystemMessages();
            var customer = account.GetStripeCustomer(msgs);

            if (customer.IsNotEmpty())
            {
                msgs.AddRange(Billing.StripeHelper.UpdateCustomerDefaultCard(customer.Id, null, cardId));
            }
            else
            {
                msgs.Add("Invalid stripe customer", MessageType.Error);
            }
            
            return msgs;
        }

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

        public static DateTime NowInAccountTimeZone(this Account account)
        {
            return account.ConvertToAccountTimeZone(DateTime.UtcNow);
        }

        public static List<RegistrantActivity> GetRegistrantActivities(this Account account, int? start = null, int? length = null, DatabaseConnection dc = null)
        {
            return RegistrantActivity.LoadByAccountId(account.ID.Value, start, length, dc: dc).ToList();
        }

        public static int GetRegistrantActivityCount(this Account account, DatabaseConnection dc = null)
        {
            return RegistrantActivity.GetCount(new Hashtable() { { "ActivityAccountId", account.ID.Value } });
        }

        public static int GetRegistrantCount(this Account account, DatabaseConnection dc = null)
        {
            return RegistrantActivity.GetSeatCountByAccountId(account.ID.Value, dc: dc);
        }

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

        public static List<AccountPackageFeature> GetAccountPackageFeatures(this Account account, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return AccountPackageFeature.LoadByAccountPackageId(account.GetActiveAccountPackage(msgs, dc).ID.Value, msgs, dc);
        }

        public static AccountPackageFeature GetAccountPackageFeature(this Account account, FeatureType featureType, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return AccountPackageFeature.LoadByAccountPackageIdByFeatureTypeId(account.GetActiveAccountPackage(msgs, dc).ID.Value, featureType, msgs, dc);
        }

        public static AccountProperty GetAccountProperty(this Account account, AccountPropertyType propertyType, DatabaseConnection dc = null)
        {
            return AccountProperty.LoadByAccountIdByAccountPropertyTypeId(account.ID.Value, propertyType, dc: dc);
        }

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

        public static List<Registration> SearchRegistrations(this Account account, string searchText, int? questionId, List<int> activityGroupIds, List<int> activityIds, RegistrationStatus regStatus, RegistrationAssignmentType rat, int? start, int? length, DatabaseConnection dc = null)
        {
            return Registration.Search(searchText, account.ID.Value, questionId, activityGroupIds, activityIds, regStatus, rat, start, length, dc: dc);
        }

        public static int SearchRegistrationCount(this Account account, string searchText, int? questionId, List<int> activityGroupIds, List<int> activityIds, RegistrationStatus regStatus, RegistrationAssignmentType rat, DatabaseConnection dc = null)
        {
            return Registration.SearchCount(searchText, account.ID.Value, questionId, activityGroupIds, activityIds, regStatus, rat, dc: dc);
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
                AllowToPayLater = true
            };
            return activity;
        }

        public static RegistrationPeriod GetDefaultNewRegistrationPeriod(this Account account)
        {
            var activePeriod = new RegistrationPeriod();
            activePeriod.ID = 0;
            activePeriod.StartDateTime = string.Format("{0} {1}", account.ConvertToAccountTimeZone(DateTime.UtcNow).ToShortDateString(), "8:00 AM").ToDateTime();
            activePeriod.EndDateTime = string.Format("{0} {1}", account.ConvertToAccountTimeZone(DateTime.UtcNow).AddWeeks(1).ToShortDateString(), "12:00 AM").ToDateTime();
            return activePeriod;
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

        public static AccountRelationship GetRequestForAccount(this Account visitorAccount, Account hostAccount, SystemMessages msgs = null, DatabaseConnection dc= null)
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

        
        //public static List<Account> GetApprovedHostAccounts(this Account hostAccount, SystemMessages msgs = null, DatabaseConnection dc = null)
        //{
        //    return AccountRelationship.LoadByApproved(hostAccount.ID.Value, msgs, dc)
        //        .Select(accountRelationship => accountRelationship.GetHostAccount(dc: dc))
        //        .ToList();
        //}

        //public static List<Account> GetDeniedHostAccounts(this Account hostAccount, SystemMessages msgs = null, DatabaseConnection dc = null)
        //{
        //    return AccountRelationship.LoadByDenied(hostAccount.ID.Value, msgs, dc)
        //        .Select(accountRelationship => accountRelationship.GetHostAccount(dc: dc))
        //        .ToList();
        //}
        

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
                    msgs.AddRange(EmailHelper.SendAccessRequestToHost(visitorAccount, hostAccount));
            }

            msgs.Add("This account has already previously been denied", MessageType.UserError);
            
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
                msgs.AddRange(EmailHelper.SendApprovalNotificationToVisitor(visitorAccount, hostAccount));

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

        public static int GetUserCount(this Account account, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return AccountWebUser.GetCount(
                new Hashtable() { { "AccountId", account.ID } },
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
                loggedInAccount: account,
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

        public static Registrant GetFirstParentOrGuardianRegistrant(this Account account, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return Registrant.LoadByAccountId(account.ID.Value, msgs, dc)
                .Where(registrant => registrant.IsParentOrGuardian)
                .OrderBy(registrant => registrant.ID)
                .FirstOrDefault();
        }

        public static Registrant GetSecondParentOrGuardianRegistrant(this Account account, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return Registrant.LoadByAccountId(account.ID.Value, msgs, dc)
                .Where(registrant => registrant.IsParentOrGuardian)
                .OrderBy(registrant => registrant.ID)
                .Skip(1)
                .FirstOrDefault();
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

        public static decimal CalculatePlatformFeeAmount(this Account toAccount, decimal totalAmountToCharge, SystemMessages msgs, DatabaseConnection dc = null)
        {
            var percentageFee = toAccount.GetAccountPackageFeature(FeatureType.PaymentsPercentageFee, msgs, dc).GetMaxValue();
            var flatFee = toAccount.GetAccountPackageFeature(FeatureType.PaymentsPercentageFlatFee, msgs, dc).GetMaxValue();

            return (totalAmountToCharge * percentageFee.ToDecimal()) + flatFee.ToDecimal();
        }

        public static decimal GetPlatformFeePercent(this Account toAccount, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return toAccount.GetAccountPackageFeature(FeatureType.PaymentsPercentageFee, msgs, dc).GetMaxValue().ToDecimal();
        }

        internal static string GetStripeSourceFromToken(this Account fromAccount, string stripeToken, SystemMessages msgs = null)
        {
            var source = Billing.StripeHelper.CreateSource(stripeToken, msgs);
            msgs.AddRange(Billing.StripeHelper.AddSourceToCustomer(fromAccount.StripeCustomerId, source.Id, null));
            return source.Id;
        }

        internal static Stripe.StripeCharge CreateStripeChargeDuringCheckout(this Account fromAccount, Account toAccount, CheckoutObject checkoutObject, SystemMessages msgs, DatabaseConnection dc = null)
        {
            // if stripe token then don't send customer id
            string customerId = null;

            if (checkoutObject.SavePaymentInfo || checkoutObject.SourceId.IsNotEmpty())
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

            return charge;
        }

        internal static Purchase AddPurchase(this Account fromAccount, CheckoutObject checkoutObject, WebUser loggedInUser, SystemMessages msgs, DatabaseConnection dc = null)
        {
            return PurchaseService.CreateNew(
                    purchasingAccount: fromAccount,
                    checkoutObject: checkoutObject,
                    loggedInUser: loggedInUser,
                    msgs: msgs,
                    dc: dc);
        }

        internal static Transaction AddTransaction(this Account fromAccount, Account toAccount, Purchase purchase, CheckoutObject checkoutObject, WebUser loggedInUser, SystemMessages msgs, DatabaseConnection dc = null)
        {
            return TransactionService.CreateNew(
                    fromAccount,
                    toAccount,
                    checkoutObject,
                    purchase.ID,
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

        private static void ProcessRegistrationFeeAndProducts(this Account toAccount, Account fromAccount, CheckoutObject checkoutObject, WebUser loggedInUser, SystemMessages msgs, DatabaseConnection dc = null)
        {
            // TODO: Add Products
            // if they have products
            //foreach (var product in purchaseObject.Products)
            //{
            //    purchaseObject.Registrant.PurchaseProduct();
            //}

            // if user wants to save payment info, do it before you charge the card
            // can't use a token twice
            if (msgs.HasNoErrors() && checkoutObject.SavePaymentInfo)
                checkoutObject.SourceId = fromAccount.GetStripeSourceFromToken(checkoutObject.StripeToken, msgs);

            // if everything goes right then bill the person, 
            // if any errors it'll just rollback the transaction anyway :)
            // don't process charge if the user is using a payment plan
            if (msgs.HasNoErrors())
                fromAccount.CreateStripeChargeDuringCheckout(toAccount, checkoutObject, msgs, dc);

            // must wait until after charges happen to know what card id is charged for sure
            // add purchase
            if (msgs.HasNoErrors())
            {
                // tracks the whole purchase, could contain multiple activities/activity groups
                var purchase = fromAccount.AddPurchase(checkoutObject, loggedInUser, msgs, dc);

                // charge successful so I need to create a transaction
                var transaction = fromAccount.AddTransaction(toAccount, purchase, checkoutObject, loggedInUser, msgs, dc);

                // need to track a payment for each activity/activitygroup
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
        }

        public static void ProcessRegistrationWithoutPayment(this Account fromAccount, Account toAccount, CheckoutObject checkoutObject, WebUser loggedInUser, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            // mark as completed
            msgs.AddRange(checkoutObject.Registration.FinalizeRegistration(loggedInUser, dc));

            // TODO: Allow user to register for multiple activities in one session at once

            // add them to the activity
            if (checkoutObject.ParticipationObject.ObjectType == ParticipationObjectType.Activity)
            {
                var activity = ((Activity)checkoutObject.ParticipationObject.Object);
                activity.ProcessRegistration(
                    purchaseObject: checkoutObject,
                    loggedInUser: loggedInUser,
                    msgs: msgs,
                    dc: dc);
            }

            // TODO: Add to an activity group if they are registering for a session

        }

        public static void ProcessRegistrationWithPayment(this Account fromAccount, Account toAccount, CheckoutObject checkoutObject, WebUser loggedInUser, SystemMessages msgs = null, DatabaseConnection dc = null)
        {

            Database.ExecuteTransaction
            (
                action: (SystemMessages amsgs, DatabaseConnection adc) =>
                {

                    fromAccount.ProcessRegistrationWithoutPayment(toAccount, checkoutObject, loggedInUser, amsgs, adc);                    

                    // process payment by to account, includes registration fee and product purchases
                    // skip for payment plans and subscription plans
                    if (checkoutObject.PaymentPlan.IsEmpty() && checkoutObject.SubscriptionPlan.IsEmpty())
                        toAccount.ProcessRegistrationFeeAndProducts(fromAccount, checkoutObject, loggedInUser, amsgs, adc);

                    // Process Subscriptions separately, don't charge all at once for the Registration Fee

                    // TODO: Decide if master account registrant should be added to plans or the account copy
                    // get the registrant record that was created just for the account
                    var accountRegistrant = checkoutObject.Registration.GetRegistrant(amsgs, adc);

                    // if they have a subscription plan
                    // will also process registration fees
                    if (checkoutObject.SubscriptionPlan.IsNotEmpty())
                    {
                        var rpp = accountRegistrant.StartSubscriptionPlan(
                            fromAccount: fromAccount,
                            toAccount: toAccount,
                            checkoutObject: checkoutObject,
                            startDateTime: DateTime.UtcNow,
                            subscriptionPlanType: SubscriptionPlanType.Subscription,
                            loggedInUser: loggedInUser,
                            msgs: amsgs,
                            dc: adc);
                    }

                    // if they have a payment plan
                    if (checkoutObject.PaymentPlan.IsNotEmpty())
                    {
                        var rpp = accountRegistrant.StartSubscriptionPlan(
                            fromAccount: fromAccount,
                            toAccount: toAccount,
                            checkoutObject: checkoutObject,
                            startDateTime: DateTime.UtcNow,
                            subscriptionPlanType: SubscriptionPlanType.PaymentPlan,
                            loggedInUser: loggedInUser,
                            msgs: amsgs,
                            dc: adc);
                    }

                },
                onSuccess: (SystemMessages smsgs) =>
                {
                    msgs.AddRange(smsgs);
                },
                onFailure: (SystemMessages amsgs) =>
                {
                    msgs.AddRange(amsgs);
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
        
        public static AccountActivityGroup InviteSchoolToLeague(this Account account, ActivityGroup activityGroup, WebUser user, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
           return AccountActivityGroupService.CreateNew(account, activityGroup, InviteStatus.Invited, user);
        }

        public static List<AccountActivityGroup> InviteSchoolsToLeague(List<Account> schools, ActivityGroup activityGroup, string customMessage, WebUser loggedInuser, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var accountActivityGroups = new List<AccountActivityGroup>();

            Database.ExecuteTransaction(
                   action: (SystemMessages amsgs, DatabaseConnection adc) => {
                       schools.ForEach(school => accountActivityGroups.Add(school.InviteSchoolToLeague(activityGroup, loggedInuser, amsgs, adc)));
                   },
                   onSuccess: (SystemMessages smsgs) => {
                       msgs.AddRange(smsgs);
                   },
                   onFailure: (SystemMessages fmsgs) => {
                       msgs.AddRange(fmsgs);
                   }
               );

            return accountActivityGroups;
        }
    }
}
