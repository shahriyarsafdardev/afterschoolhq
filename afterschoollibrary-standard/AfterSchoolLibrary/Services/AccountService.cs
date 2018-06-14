using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BasicLibrary;
using BasicLibrary.DataAccess;

namespace AfterSchoolLibrary
{
    public static class AccountService
    {
        public static bool HasFeature(this Account account, FeatureType featureType, DatabaseConnection dc = null)
        {
            return account.GetActiveAccountPackage(dc).HasFeature(featureType);
        }

        public static WebUser GetAccountOwner(this Account account, DatabaseConnection dc = null)
        {
            return WebUser.Load(account.OwnerId.Value, dc);
        }

        public static List<WebUser> GetAccountOwnersAndAdmins(this Account account, DatabaseConnection dc = null)
        {
            return AccountWebUser.LoadByAccountIdByIsActive(account.ID.Value, true, dc)
                .Where(x => x.UserTypeId == UserType.Administrator || x.UserTypeId == UserType.Owner)
                .Select(x => x.ToWebUser())
                .ToList();
        }

        public static List<Activity> GetPublishedActivities(this Account account, DatabaseConnection dc = null)
        {
            var periods = account.GetOpenAndUpcomingRegistrationPeriods(dc);
            var activities = new List<Activity>();

            periods.ForEach(period => activities.AddRange(period.GetActivities(dc).Where(x => x.IsPubliclyViewable)));

            return activities.Where(activity => activity.GetActiveActivityForm(dc).IsNotEmpty()).ToList();
        }

        public static List<ActivityForm> GetActivityForms(this Account account, DatabaseConnection dc = null)
        {
            return ActivityForm.LoadByAccountId(account.ID.Value, dc);
        }

        public static List<Activity> GetActivities(this Account account, UserType userType, WebUser user, bool? isArchived, bool? isDeleted, int? start = null, int? length = null, DatabaseConnection dc = null)
        {
            if (user.IsEmpty() || new UserType[] { UserType.Administrator, UserType.Owner, UserType.SuperAdmin }.Contains(userType))
            {
                return Activity.LoadByAccountId(account.ID.Value, null, isArchived, isDeleted, start, length, dc).ToList();
            }
            else
            {
                return Activity.LoadByAccountId(account.ID.Value, null, isArchived, isDeleted, start, length, dc)
                    .Where(activity => user.HasAccessToActivity(activity, dc))
                    .ToList();
            }
        }

        public static int GetActivityCount(this Account account, UserType userType, WebUser user, bool? isArchived, bool? isDeleted, DatabaseConnection dc = null)
        {
            if (user.IsEmpty() || new UserType[] { UserType.Administrator, UserType.Owner, UserType.SuperAdmin }.Contains(userType))
            {
                return Activity.GetCountByAccountId(account.ID.Value, null, isArchived, isDeleted, dc);
            }
            else
            {
                return account.GetActivities(userType, user, isArchived, isDeleted, dc: dc).Count();
            }
        }

        public static int SearchContactCount(this Account account, string searchtext, int? contactListId, DatabaseConnection dc = null)
        {
            return Registrant.SearchRegistrantCount(searchtext, contactListId, account.ID.Value, dc);
        }

        public static List<Registrant> SearchContacts(this Account account, string searchtext, int? contactListId, int? start = null, int? length = null, DatabaseConnection dc = null)
        {
            return Registrant.SearchRegistrants(searchtext, contactListId, account.ID.Value, start, length, dc);
        }

        public static int GetContactCount(this Account account, DatabaseConnection dc = null)
        {
            return Registrant.GetCountByAccountId(account.ID.Value, dc);
        }

        public static List<Registrant> GetContacts(this Account account, int? start = null, int? length = null, DatabaseConnection dc = null)
        {
            return Registrant.LoadByAccountId(account.ID.Value, start, length, dc);
        }

        public static List<ContactList> GetContactLists(this Account account, int? start = null, int? length = null, DatabaseConnection dc = null)
        {
            return ContactList.LoadByAccountId(account.ID.Value, start, length, dc);
        }

        public static int GetContactListCount(this Account account, DatabaseConnection dc = null)
        {
            return ContactList.GetCount(criterias: new Hashtable() { { "AccountId", account.ID.Value } }, dc: dc);
        }

        public static List<Registrant> GetRegistrantsByActivityId(this Account account, int activityId, int? start = null, int? length = null, DatabaseConnection dc = null)
        {
            return account.GetRegistrantActivitiesByActivityId(activityId, start, length, dc)
                .Select(x => x.ToRegistrant())
                .ToList();
        }

        public static List<RegistrantActivity> GetRegistrantActivitiesByActivityId(this Account account, int activityId, int? start = null, int? length = null, DatabaseConnection dc = null)
        {
            return RegistrantActivity.LoadByActivityId(activityId, start, length, dc)
                .Where(x => x.ActivityAccountId == account.ID)
                .ToList();
        }

        public static int GetRegistrantActivityCountByActivityId(this Account account, int activityId, DatabaseConnection dc = null)
        {
            return RegistrantActivity.GetCountByActivityIdByAccountId(activityId, account.ID.Value, dc);
        }

        public static List<Form> GetForms(this Account account, DatabaseConnection dc = null)
        {
            return Form.LoadByAccountId(account.ID.Value, dc);
        }

        public static List<ActivityGroup> GetActivityGroups(this Account account, DatabaseConnection dc = null)
        {
            return ActivityGroup.LoadByAccountId(account.ID.Value, dc);
        }

        public static List<ActivityGroup> GetPublishedActivityGroups(this Account account, DatabaseConnection dc = null)
        {
            return ActivityGroup.LoadByAccountIdByIsPublished(account.ID.Value, true, dc);
        }

        public static List<ActivityGroup> GetActivityGroups(this Account account, bool IsArchived, int? start, int? length, DatabaseConnection dc = null)
        {
            return ActivityGroup.LoadByAccountId(account.ID.Value, IsArchived, start, length, dc);
        }

        public static int GetActivityGroupCount(this Account account, bool IsArchived = false, DatabaseConnection dc = null)
        {
            return ActivityGroup.GetCount(new Hashtable() { { "AccountId", account.ID.Value }, { "IsArchived", IsArchived } }, dc);
        }

        public static List<ActivitySchedule> GetActivitySchedules(this Account account, DatabaseConnection dc = null)
        {
            return ActivitySchedule.LoadByAccountId(account.ID.Value, dc);
        }

        public static List<RegistrationPeriod> GetRegistrationPeriods(this Account account, DatabaseConnection dc = null)
        {
            return RegistrationPeriod.LoadByAccountId(account.ID.Value, dc);
        }

        public static List<RegistrationPeriod> GetRegistrationPeriods(this Account account, int? start, int? length, DatabaseConnection dc = null)
        {
            return RegistrationPeriod.LoadByAccountId(account.ID.Value, start, length, dc);
        }

        public static int GetRegistrationPeriodCount(this Account account, DatabaseConnection dc = null)
        {
            return RegistrationPeriod.GetCount(new Hashtable() { { "AccountId", account.ID.Value } }, dc);
        }

        public static List<RegistrationPeriod> GetUpcomingRegistrationPeriods(this Account account, DatabaseConnection dc = null)
        {
            return account.GetRegistrationPeriods()
                .Where(x => x.StartDateTime.Value > account.ConvertToAccountTimeZone(DateTime.UtcNow))
                .OrderBy(x => x.StartDateTime.Value)
                .ToList();
        }

        public static List<RegistrationPeriod> GetOpenRegistrationPeriods(this Account account, DatabaseConnection dc = null)
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

        public static List<RegistrationPeriod> GetOpenAndUpcomingRegistrationPeriods(this Account account, DatabaseConnection dc = null)
        {
            var periods = account.GetOpenRegistrationPeriods(dc);
            periods.AddRange(account.GetUpcomingRegistrationPeriods(dc));
            return periods;
        }

        public static Form GetDefaultForm(this Account account, DatabaseConnection dc = null)
        {
            if (account.DefaultFormId.HasValue)
                return Form.Load(account.DefaultFormId.Value, dc);

            return Form.Load(Form.GetLatestTemplateForm(1, dc).ID.Value, dc);
        }

        public static List<AccountWebUser> GetMembers(this Account account, DatabaseConnection dc = null)
        {
            return AccountWebUser.LoadByAccountId(account.ID.Value, dc);
        }

        public static List<AccountWebUser> GetActiveMembers(this Account account, DatabaseConnection dc = null)
        {
            return AccountWebUser.LoadByAccountIdByIsActive(account.ID.Value, true, dc);
        }

        public static AccountPackage GetActiveAccountPackage(this Account account, DatabaseConnection dc = null)
        {
            return AccountPackage.LoadByAccountIdByIsActive(account.ID.Value, true, dc);
        }

        public static List<AccountPackage> GetAccountPackages(this Account account, DatabaseConnection dc = null)
        {
            return AccountPackage.LoadByAccountId(account.ID.Value, dc);
        }

        public static SystemMessages AddUser(this Account account, WebUser user, UserType ut, int loggedInUserId, DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();

            Database.ExecuteTransaction(
                action: (SystemMessages amsgs, DatabaseConnection adc) =>
                {
                    var gwu = AccountWebUser.LoadByAccountIdByWebUserId(account.ID.Value, user.ID.Value, adc);
                    if (gwu.IsEmpty())
                    {
                        gwu = new AccountWebUser();
                        gwu.CreateDate = DateTime.UtcNow;
                        gwu.CreatedBy = loggedInUserId;
                        gwu.WebUserId = user.ID.Value;
                        gwu.AccountId = account.ID.Value;
                        gwu.IsDefault = false;
                        gwu.IsActive = user.IsActive;
                    }
                    else
                        gwu.IsActive = true;

                    gwu.LastModifiedDate = DateTime.UtcNow;
                    gwu.LastModifiedBy = loggedInUserId;
                    gwu.UserTypeId = ut;

                    amsgs.AddRange(gwu.Save(adc));

                    if (ut == UserType.Owner)
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
            return account.AddUser(firstName, lastName, email, null, false, true, false, ut, loggedInUserId, msgs, dc);
        }

        public static WebUser AddUser(this Account account, string firstName, string lastName, string email, string password, bool isActive, bool isRegistered, bool isVerified, UserType ut, int loggedInUserId, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();
            WebUser user = null;

            Database.ExecuteTransaction(
                action: (SystemMessages amsgs, DatabaseConnection adc) =>
                {
                    if (!WebUser.ExistsByEmail(email.Trim(), adc))
                    {
                        user = new WebUser();
                        user.FirstName = firstName;
                        user.LastName = lastName;
                        user.Email = email.Trim();
                        user.Username = WebUser.GenerateUsername(firstName, lastName, adc);
                        if (!password.IsEmpty())
                            user.Password = WebUser.EncryptPassword(password);

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
                msgs.AddRange(AddUser(account, user, ut, loggedInUserId, dc));

            return user;

        }

        public static SystemMessages SendWelcomeEmailTrial(this Account account, DatabaseConnection dc = null)
        {
            return EmailHelper.SendWelcomeTrial(account.GetAccountOwner(dc), account);
        }

        public static SystemMessages SendWelcomeEmailPurchase(this Account account, DatabaseConnection dc = null)
        {
            return EmailHelper.SendWelcomePurchase(account.GetAccountOwner(dc), account);
        }

        public static SystemMessages InviteUser(this Account account, WebUser wu, UserType ut, WebUser loggedInUser, DatabaseConnection dc = null)
        {
            return account.InviteUser(wu.FirstName, wu.LastName, wu.Email, ut, loggedInUser, dc);
        }

        public static SystemMessages InviteUser(this Account account, string firstName, string lastName, string email, UserType ut, WebUser loggedInUser, DatabaseConnection dc = null)
        {
            return account.InviteUser(firstName, lastName, email, null, ut, loggedInUser, dc);
        }

        public static SystemMessages InviteUser(this Account account, string firstName, string lastName, string email, string message, UserType ut, WebUser loggedInUser, DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();
            WebUser wu = null;

            Database.ExecuteTransaction(
                action: (SystemMessages amsgs, DatabaseConnection adc) =>
                {
                    wu = WebUser.LoadByEmail(email.Trim(), adc);

                    if (wu.IsEmpty())
                    {
                        wu = new WebUser();
                        wu.FirstName = firstName;
                        wu.LastName = lastName;
                        wu.Email = email.Trim();
                        wu.CreateDate = wu.LastModifiedDate = DateTime.Now;
                        wu.CreatedBy = wu.LastModifiedBy = loggedInUser.ID.Value;
                        wu.IsActive = false;
                        wu.Uid = Guid.NewGuid();
                        amsgs.AddRange(wu.Save(adc));
                    }

                    var awu = AccountWebUser.LoadByAccountIdByWebUserId(account.ID.Value, wu.ID.Value, adc);
                    if (awu.IsEmpty())
                    {
                        awu = new AccountWebUser();
                        awu.CreateDate = DateTime.Now;
                        awu.CreatedBy = loggedInUser.ID.Value;
                        awu.WebUserId = wu.ID.Value;
                        awu.AccountId = account.ID.Value;
                        awu.IsDefault = true;
                    }
                    else
                    {
                        awu.IsActive = true;
                    }
                    awu.LastModifiedDate = DateTime.Now;
                    awu.LastModifiedBy = loggedInUser.ID.Value;
                    awu.UserTypeId = ut;

                    amsgs.AddRange(awu.Save(adc));

                    if (ut == UserType.Owner)
                    {
                        account.OwnerId = wu.ID.Value;
                        amsgs.AddRange(account.Save(adc));
                    }
                },
                onSuccess: (SystemMessages smsgs) =>
                {
                    msgs.AddRange(smsgs);
                    msgs.AddRange(wu.SendInvite(email.Trim(), message, loggedInUser, account, dc));
                },
                onFailure: (SystemMessages fmsgs) =>
                {
                    msgs.AddRange(fmsgs);
                    msgs.Add("An invitation was not sent to " + email.Trim() + ", a system error has occurred", MessageType.UserError);
                },
                dc: dc
            );

            return msgs;
        }

        public static string GenerateUniqueName(string name, int? accountId, DatabaseConnection dc = null)
        {
            //unique name
            var reg = new System.Text.RegularExpressions.Regex(BasicLibrary.Utilities.UrlFriendlyRegEx, System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.Singleline);

            string origUniqueName = reg.Replace(name, "").MaxChars(25);
            string uniqueName = origUniqueName;
            Account duplicate = Account.LoadByUniqueName(uniqueName, dc);
            int count = 1;
            while (duplicate.IsNotEmpty() && duplicate.ID != accountId)
            {
                uniqueName = origUniqueName + count.ToString();
                count++;
                duplicate = Account.LoadByUniqueName(uniqueName, dc);
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

        private static SignUpType GetSignUpType(this Account account, Package p, AccountPackage activePackage, BillingPeriodType billingPeriodType, ref int trialDayCount, DatabaseConnection dc = null)
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
                    trialDayCount = p.HasFeature(FeatureType.TrialDays) ? p.GetFeature(FeatureType.TrialDays, dc).GetMaxValue() : 0;
                }
                else if (activePackage.HasFeature(FeatureType.TrialDays))
                {
                    trialDayCount = account.GetTrialDaysRemaining(activePackage, dc);
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

        public static AccountPackage AddPackage(this Account account, int package, BillingPeriodType billingPeriodType, WebUser loggedInUser, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            Package p = null;
            AccountPackage currentPackage = null;
            var newap = new AccountPackage();

            Database.ExecuteTransaction(
                action: (SystemMessages amsgs, BasicLibrary.DataAccess.DatabaseConnection adc) =>
                {
                    p = Package.Load(package, adc);
                    currentPackage = account.GetActiveAccountPackage(adc);

                    var trialDayCount = 0;

                    // is user:
                    //      paying for their Trial account
                    //      upgrading to a different package
                    //      paying for a new package with no trial account

                    var signupType = account.GetSignUpType(p, currentPackage, billingPeriodType, ref trialDayCount, adc);

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
                                var newStripePlanId = p.GetStripePlanId(billingPeriodType);

                                //create new package
                                newap.AccountId = account.ID;
                                newap.PackageId = package;
                                newap.BillingPeriodTypeId = billingPeriodType;
                                newap.CreateDate = DateTime.UtcNow;
                                newap.CreatedBy = loggedInUser.ID;
                                newap.LastModifiedDate = DateTime.UtcNow;
                                newap.LastModifiedBy = loggedInUser.ID;
                                newap.IsActive = true;
                                newap.Price = billingPeriodType == BillingPeriodType.Month ? p.Price : p.PricePerYear;
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
                            amsgs.AddRange(p.CopyFeaturesToAccountPackage(newap, loggedInUser.ID.Value, adc));
                        }

                        // make sure the account package is set to the real trial days passed in
                        if (amsgs.HasNoErrors())
                        {
                            var apf = newap.GetFeature(FeatureType.TrialDays, adc);
                            if (apf.IsNotEmpty())
                            {
                                apf.Max = trialDayCount;
                                amsgs.AddRange(apf.Save(adc));
                            }
                        }
                    }
                    else
                    {
                        amsgs.Add(string.Format("Credit Card is required to sign up for {0} Package", p.Name), MessageType.UserError);
                    }
                },
                onSuccess: (SystemMessages smsgs) =>
                {
                    msgs.AddRange(smsgs);
                    msgs.Add(string.Format("{0} Package has been activated", p.Name), MessageType.Success);
                },
                onFailure: (SystemMessages fmsgs) =>
                {
                    msgs.AddRange(fmsgs);
                },
                dc: dc
            );

            return newap;
        }

        public static int GetTrialDaysRemaining(this Account account, AccountPackage currentPackage, DatabaseConnection dc = null)
        {
            var daysRemaining = 0;
            var firstAccountPackage = account.GetAccountPackages(dc).OrderBy(x => x.StartDate.Value).First();
            var daysSinceSignUp = (DateTime.UtcNow - firstAccountPackage.StartDate.Value).Days;
            if (daysSinceSignUp < currentPackage.GetFeature(FeatureType.TrialDays).GetMaxValue() + 1) // they get the full last day
            {
                daysRemaining = currentPackage.GetFeature(FeatureType.TrialDays).GetMaxValue() - daysSinceSignUp;
            }
            return daysRemaining;
        }

        public static SystemMessages SaveWithStripe(this Account account, DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();

            msgs.AddRange(account.Save(dc));

            if (msgs.HasNoErrors())
            {
                msgs.AddRange(account.CreateStripeCustomer(dc));
            }

            return msgs;
        }

        public static SystemMessages CreateStripeCustomer(this Account account, DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();

            Stripe.StripeRequestOptions requestOptions = new Stripe.StripeRequestOptions();
            requestOptions.ApiKey = StripeHelper.SecretKey;

            Stripe.StripeCustomer customer = account.GetStripeCustomer();

            if (!customer.IsEmpty())
            {
                Stripe.StripeCustomerUpdateOptions options = new Stripe.StripeCustomerUpdateOptions();
                options.Description = account.Name;
                options.Email = account.Email;

                Stripe.StripeCustomerService service = new Stripe.StripeCustomerService(StripeHelper.SecretKey);
                try
                {
                    customer = service.Update(account.StripeCustomerId, options, requestOptions);
                }
                catch (Stripe.StripeException ex)
                {
                    msgs.Add("Error connecting to billing service", MessageType.Warning, ex);
                }
            }
            else
            {
                Stripe.StripeCustomerCreateOptions options = new Stripe.StripeCustomerCreateOptions();
                options.Description = account.Name;
                options.Email = account.Email;

                Stripe.StripeCustomerService service = new Stripe.StripeCustomerService(StripeHelper.SecretKey);
                try
                {
                    customer = service.Create(options);
                }
                catch (Stripe.StripeException ex)
                {
                    msgs.Add("Error connecting to billing service", MessageType.Warning, ex);
                }

                if (!customer.IsEmpty())
                {
                    account.StripeCustomerId = customer.Id;
                    msgs.AddRange(account.Save(dc));
                }
            }
            return msgs;
        }

        public static SystemMessages AddCreditCard(this Account account, string name, string number, string expMonth, string expYear, string cvc, bool makePrimary, DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();

            if (account.StripeCustomerId.IsEmpty())
            {
                msgs.AddRange(account.CreateStripeCustomer(dc));
            }

            if (msgs.HasNoErrors())
            {
                var options = new Stripe.StripeCardCreateOptions();
                options.SourceCard = new Stripe.SourceCard
                {
                    Name = name,
                    Number = number,
                    ExpirationMonth = expMonth.ToInteger(),
                    ExpirationYear = expYear.ToInteger(),
                    Cvc = cvc
                };

                Stripe.StripeCard card = null;
                Stripe.StripeCardService service = new Stripe.StripeCardService(StripeHelper.SecretKey);
                try
                {
                    card = service.Create(account.StripeCustomerId, options);
                    if (!card.IsEmpty() && makePrimary)
                    {
                        account.StripeDefaultCardId = card.Id;
                        msgs.AddRange(account.Save(dc));
                    }
                }
                catch (Stripe.StripeException ex)
                {
                    //error storing card
                    msgs.Add(ex.Message, MessageType.Error, ex);
                }
                catch (Exception ex)
                {
                    msgs.Add("Error storing card", MessageType.Error, ex);
                }

                if (msgs.HasNoErrors() && makePrimary)
                {
                    msgs.AddRange(account.SetStripeDefaultCard(card.Id));
                }

                if (msgs.HasNoErrors())
                {
                    msgs.Add("The payment card has been successfully added", MessageType.Success);
                }
            }

            return msgs;
        }

        public static List<Stripe.StripeCard> GetStripeCards(this Account account, SystemMessages msgs = null)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            var cards = new List<Stripe.StripeCard>();
            var customer = account.GetStripeCustomer();

            if (!customer.IsEmpty())
            {
                try
                {
                    Stripe.StripeCardService service = new Stripe.StripeCardService(StripeHelper.SecretKey);
                    cards.AddRange(service.List(customer.Id).ToList());
                }
                catch (Stripe.StripeException ex)
                {
                    //error storing card
                    msgs.Add("Error retrieving cards", MessageType.Error, ex);
                }
            }

            return cards;
        }

        public static List<Stripe.StripeCharge> GetStripeBillingHistory(this Account account, SystemMessages msgs = null)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            var charges = new List<Stripe.StripeCharge>();
            var customer = account.GetStripeCustomer();

            if (!customer.IsEmpty())
            {
                try
                {
                    Stripe.StripeChargeService service = new Stripe.StripeChargeService(StripeHelper.SecretKey);
                    Stripe.StripeChargeListOptions options = new Stripe.StripeChargeListOptions();
                    options.CustomerId = customer.Id;
                    options.Limit = 10;
                    charges.AddRange(service.List(options).ToList());
                }
                catch (Stripe.StripeException ex)
                {
                    //error storing card
                    msgs.Add("Error retrieving billing history", MessageType.Error, ex);
                }
            }

            return charges;
        }

        public static List<Stripe.StripeRefund> GetStripeRefundHistory(this Account account, SystemMessages msgs = null)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            var refunds = new List<Stripe.StripeRefund>();
            var customer = account.GetStripeCustomer();

            if (!customer.IsEmpty())
            {
                try
                {
                    var service = new Stripe.StripeRefundService(StripeHelper.SecretKey);
                    refunds.AddRange(service.List(null).ToList());
                }
                catch (Stripe.StripeException ex)
                {
                    msgs.Add("Error retrieving refund history", MessageType.Error, ex);
                }
            }

            return refunds;
        }

        public static Stripe.StripeCustomer GetStripeCustomer(this Account account, SystemMessages msgs = null)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            Stripe.StripeCustomer customer = null;

            if (!account.StripeCustomerId.IsEmpty())
            {
                try
                {
                    Stripe.StripeCustomerService service = new Stripe.StripeCustomerService(StripeHelper.SecretKey);
                    customer = service.Get(account.StripeCustomerId);
                }
                catch (Stripe.StripeException ex)
                {
                    msgs.Add("Error retrieving customer information", MessageType.Error, ex);
                }
            }
            return customer;
        }

        public static SystemMessages SetStripeDefaultCard(this Account account, string cardId)
        {
            var msgs = new SystemMessages();
            var customer = account.GetStripeCustomer();

            var options = new Stripe.StripeCustomerUpdateOptions();
            options.DefaultSource = cardId;

            try
            {
                Stripe.StripeCustomerService service = new Stripe.StripeCustomerService(StripeHelper.SecretKey);
                service.Update(customer.Id, options);
            }
            catch (Stripe.StripeException ex)
            {
                msgs.Add("Error retrieving update default payment information", MessageType.Error, ex);
            }

            return msgs;
        }

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
            msgs.AddRange(registrant.Save(dc));

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
            return RegistrantActivity.LoadByAccountId(account.ID.Value, start, length, dc).ToList();
        }

        public static int GetRegistrantActivityCount(this Account account, DatabaseConnection dc = null)
        {
            return RegistrantActivity.GetCount(new Hashtable() { { "ActivityAccountId", account.ID.Value } });
        }

        public static int GetRegistrantCount(this Account account, DatabaseConnection dc = null)
        {
            return RegistrantActivity.GetSeatCountByAccountId(account.ID.Value, dc);
        }

        public static List<Attendance> GetCompletedAttendance(this Account account, DatabaseConnection dc = null)
        {
            return Attendance.LoadByAccountIdByIsCompleted(account.ID.Value, true, dc);
        }

        public static int GetAttendancePresentCount(this Account account, DatabaseConnection dc = null)
        {
            return AttendanceRegistrant.GetPresentCountByAccountId(account.ID.Value, dc);
        }

        public static int GetAttendancePossibleCount(this Account account, DatabaseConnection dc = null)
        {
            return AttendanceRegistrant.GetPossibleCountByAccountId(account.ID.Value, dc);
        }

        public static List<Attendance> GetAttendanceByDate(this Account account, DateTime date, DatabaseConnection dc = null)
        {
            return Attendance.LoadByAccountIdByDate(account.ID.Value, date, dc);
        }

        public static List<AccountPackageFeature> GetAccountPackageFeatures(this Account account, DatabaseConnection dc = null)
        {
            return AccountPackageFeature.LoadByAccountPackageId(account.GetActiveAccountPackage(dc).ID.Value, dc);
        }

        public static AccountProperty GetAccountProperty(this Account account, AccountPropertyType propertyType, DatabaseConnection dc = null)
        {
            return AccountProperty.LoadByAccountIdByAccountPropertyTypeId(account.ID.Value, propertyType, dc);
        }

        public static List<Registration> GetRegistrations(this Account account, DatabaseConnection dc = null)
        {
            return Registration.LoadByAccountId(account.ID.Value, dc);
        }

        public static List<Registration> GetCurrentRegistrations(this Account account, DatabaseConnection dc = null)
        {
            return Registration.LoadCurrentByAccountId(account.ID.Value, dc);
        }

        public static List<Registration> GetRegistrations(this Account account, int start, int length, DatabaseConnection dc = null)
        {
            return Registration.LoadByAccountId(account.ID.Value, start, length, dc);
        }

        public static int GetRegistrationCount(this Account account, DatabaseConnection dc = null)
        {
            return Registration.GetCount(new Hashtable() { { "AccountId", account.ID.Value } }, dc);
        }

        public static List<Registration> SearchRegistrations(this Account account, string searchText, int? questionId, List<int> activityGroupIds, List<int> activityIds, RegistrationStatus regStatus, RegistrationAssignmentType rat, int? start, int? length, DatabaseConnection dc = null)
        {
            return Registration.Search(searchText, account.ID.Value, questionId, activityGroupIds, activityIds, regStatus, rat, start, length, dc);
        }

        public static int SearchRegistrationCount(this Account account, string searchText, int? questionId, List<int> activityGroupIds, List<int> activityIds, RegistrationStatus regStatus, RegistrationAssignmentType rat, DatabaseConnection dc = null)
        {
            return Registration.SearchCount(searchText, account.ID.Value, questionId, activityGroupIds, activityIds, regStatus, rat, dc);
        }

        public static Activity GetDefaultNewActivity(this Account account)
        {
            var activity = new Activity
            {
                ID = 0,
                WaitingListSeats = 0,
                SeatsAvailable = Math.Min(20, account.GetActiveAccountPackage().GetFeature(FeatureType.Participants).Max.Value),
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

        public static Account NewAccount(string company, string email, string phone, bool isActive, int loggedInUserId, bool isAfterSchool, SystemMessages msgs, DatabaseConnection dc)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();
            var account = new Account();
            account.Name = company;
            account.IsAdminAccount = false;
            account.IsActive = isActive;
            account.Email = email;
            account.Phone = phone;
            account.UniqueName = AccountService.GenerateUniqueName(account.Name, account.ID, dc);
            account.CreateDate = DateTime.UtcNow;
            account.LastModifiedDate = DateTime.UtcNow;
            account.CreatedBy = loggedInUserId;
            account.LastModifiedBy = loggedInUserId;
            account.IsAfterSchool = isAfterSchool;

            msgs.AddRange(account.SaveWithStripe(dc));

            return account;
        }
        public static List<Account> GetVisitorAccounts(this Account account, DatabaseConnection dc = null)
        {
            return AccountRelationship.LoadByHostAccountId(account.ID.Value, dc)
                .Select(accountRelationship => accountRelationship.GetVisitorAccount(dc))
                .ToList();            
        }

        public static List<Account> GetHostAccounts(this Account account, DatabaseConnection dc = null)
        {
            return AccountRelationship.LoadByVisitorAccountId(account.ID.Value, dc)
                .Select(x => x.GetHostAccount(dc))
                .ToList();
        }

        //Gets the accounts that are requesting the host's approval
        public static List<Account> GetRequestedVisitorAccounts(this Account Hostaccount, DatabaseConnection dc = null)
        {
            return AccountRelationship.LoadByRequested(Hostaccount.ID.Value,dc)
                .Select(accountRelationship => accountRelationship.GetVisitorAccount(dc))
                .ToList();
        }

        //Gets the accounts that the host has approved
        public static List<Account> GetApprovedVisitorAccounts(this Account Hostaccount, DatabaseConnection dc = null)
        {
            return AccountRelationship.LoadByApproved(Hostaccount.ID.Value,dc)
                .Select(accountRelationship => accountRelationship.GetVisitorAccount(dc))
                .ToList();
        }
        //Gets the accounts that the host has Denied
        public static List<Account> GetDeniedVisitorAccounts(this Account Hostaccount, DatabaseConnection dc = null)
        {
            return AccountRelationship.LoadByDenied(Hostaccount.ID.Value, dc)
                .Select(accountRelationship => accountRelationship.GetVisitorAccount(dc))
                .ToList();
        }

        
        public static List<Account> GetApprovedHostAccounts(this Account Hostaccount, DatabaseConnection dc = null)
        {
            return AccountRelationship.LoadByApproved(Hostaccount.ID.Value, dc)
                .Select(accountRelationship => accountRelationship.GetHostAccount(dc))
                .ToList();
        }

        public static List<Account> GetDeniedHostAccounts(this Account Hostaccount, DatabaseConnection dc = null)
        {
            return AccountRelationship.LoadByDenied(Hostaccount.ID.Value, dc)
                .Select(accountRelationship => accountRelationship.GetHostAccount(dc))
                .ToList();
        }
        

        public static List<Location> GetVisitorLocations(this Account account, DatabaseConnection dc = null)
        {
            return AccountRelationshipLocation.LoadByVisitorAccountId(account.ID.Value, dc)
                .Select(x => x.GetLocation(dc))
                .ToList();
        }

        public static List<Location> GetHostedLocations(this Account account, DatabaseConnection dc = null)
        {
            return Location.LoadByHostAccountId(account.ID.Value, dc);
        }



        public static SystemMessages RequestAccessToAccount(this Account visitorAccount,Account hostAccount, DatabaseConnection dc = null)
        {
            //creating an account relationship in which the visitor is requesting approval saving it to database
            //and returning the system messages
            var msgs = new SystemMessages();
            // Makes sure that the visitor has not been previously denied by the host
            if (!GetDeniedVisitorAccounts(hostAccount, dc).Contains(visitorAccount))
            {
                var accountRelationship = new AccountRelationship();
                accountRelationship.VisitorAccountId = visitorAccount.ID;
                accountRelationship.HostAccountId = hostAccount.ID;
                accountRelationship.HostPermissionTypeId = HostPermissionType.Requested;
                accountRelationship.CreatedBy = visitorAccount.ID;
                accountRelationship.CreateDate = DateTime.UtcNow;
                accountRelationship.LastModifiedDate = DateTime.UtcNow;
                msgs.AddRange(accountRelationship.Save(dc));
                //TODO Notify Host account of request
                if (msgs.HasNoErrors())
                {
                    msgs.AddRange(EmailHelper.SendAccessRequestToHost(visitorAccount, hostAccount));
                }
            }
            return msgs;
        }

        public static SystemMessages ApproveAccessToAccount(this Account hostAccount,Account visitorAccount ,DatabaseConnection dc = null)
        {
            //Approves access to the host account 
            SystemMessages msgs = new SystemMessages();
            var accountRelationship = AccountRelationship.LoadByHostAccountIdByVisitorAccountId(hostAccount.ID.Value,visitorAccount.ID.Value);
            accountRelationship.HostPermissionTypeId = HostPermissionType.Approve;
            accountRelationship.LastModifiedDate = DateTime.UtcNow;
            accountRelationship.LastModifiedBy = hostAccount.ID;
            msgs.AddRange(accountRelationship.Save(dc));

            //Notifies Host account of acceptance
            if (msgs.HasNoErrors())
            {
                msgs.AddRange(EmailHelper.SendApprovalNotificationToVisitor(visitorAccount, hostAccount));
            }

            return msgs;
        }

        public static SystemMessages DenyAccessToAccount(this Account hostAccount, Account visitorAccount, DatabaseConnection dc = null)
        {
            //Denies access to the host account 
            var msgs = new SystemMessages();
            var accountRelationship = AccountRelationship.LoadByHostAccountIdByVisitorAccountId(hostAccount.ID.Value, visitorAccount.ID.Value);
            accountRelationship.HostPermissionTypeId = HostPermissionType.Deny;
            accountRelationship.LastModifiedDate = DateTime.UtcNow;
            accountRelationship.LastModifiedBy = hostAccount.ID;
            msgs.AddRange(accountRelationship.Save(dc));

            //Notifies Host account of Denial
            if (msgs.HasNoErrors())
            {
                msgs.AddRange(EmailHelper.SendDenialNotificationToVisitor(visitorAccount, hostAccount));
            }

            return msgs;
        }


    }
}
