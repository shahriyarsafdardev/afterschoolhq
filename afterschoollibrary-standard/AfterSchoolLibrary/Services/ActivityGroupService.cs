using System;
using System.Collections.Generic;
using System.Text;
using BasicLibrary;
using BasicLibrary.DataAccess;
using AfterSchoolLibrary.Models;
using System.Linq;

namespace AfterSchoolLibrary.Services
{
    public static class ActivityGroupService
    {
        /// <summary>
        /// Invite an account to an external league or session
        /// </summary>
        /// <param name="activityGroup"></param>
        /// <param name="accountToInvite"></param>
        /// <param name="loggedInUser"></param>
        /// <param name="dc"></param>
        /// <returns></returns>
        public static SystemMessages InviteAccount(this ActivityGroup activityGroup, Account accountToInvite, WebUser loggedInUser, DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();
            var aag = AccountActivityGroupService.CreateNew(accountToInvite, activityGroup, InviteStatus.Invited, loggedInUser, msgs, dc);

            // send email to invited account
            if (msgs.HasNoErrors())
            {
                // TODO: Write this
                msgs.Add("No email sent", MessageType.Warning);
            }

            return msgs;
        }

        /// <summary>
        /// Create a payment plan plan to allow a registrant to subscribe to when signing up
        /// </summary>
        /// <param name="activityGroup"></param>
        /// <param name="costPerPeriod"></param>
        /// <param name="billingPeriodType"></param>
        /// <param name="totalPeriods"></param>
        /// <param name="loggedInUser"></param>
        /// <param name="msgs"></param>
        /// <param name="dc"></param>
        /// <returns></returns>
        public static SubscriptionPlan AddPaymentPlan(this ActivityGroup activityGroup, decimal costPerPeriod, BillingPeriodType billingPeriodType, int totalPeriods, WebUser loggedInUser, SystemMessages msgs, DatabaseConnection dc = null)
        {
            return SubscriptionPlanService.CreateNewPaymentPlan(
                activityGroup.GetAccount(msgs, dc),
                activityGroup,
                costPerPeriod,
                billingPeriodType,
                totalPeriods,
                loggedInUser,
                msgs,
                dc);
        }

        /// <summary>
        /// Create a subscription plan to allow a registrant to subscribe to when signing up
        /// </summary>
        /// <param name="activityGroup"></param>
        /// <param name="costPerPeriod"></param>
        /// <param name="billingPeriodType"></param>
        /// <param name="doesAutoRenew"></param>
        /// <param name="loggedInUser"></param>
        /// <param name="msgs"></param>
        /// <param name="dc"></param>
        /// <returns></returns>
        public static SubscriptionPlan AddSubscriptionPlan(this ActivityGroup activityGroup, decimal costPerPeriod, BillingPeriodType billingPeriodType, bool doesAutoRenew, WebUser loggedInUser, SystemMessages msgs, DatabaseConnection dc = null)
        {
            return SubscriptionPlanService.CreateNewSubscriptionPlan(
                activityGroup.GetAccount(msgs, dc),
                activityGroup,
                costPerPeriod,
                billingPeriodType,
                doesAutoRenew,
                loggedInUser,
                msgs,
                dc);
        }

        public static List<SubscriptionPlan> GetPaymentPlans(this ActivityGroup activityGroup, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return SubscriptionPlan.LoadByParticipationObject(activityGroup.ID.Value, ParticipationObjectType.ActivityGroup, SubscriptionPlanType.PaymentPlan, msgs, dc)
                .Where(plan => plan.IsActive)
                .ToList();
        }

        public static List<SubscriptionPlan> GetSubscriptionPlans(this ActivityGroup activityGroup, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return SubscriptionPlan.LoadByParticipationObject(activityGroup.ID.Value, ParticipationObjectType.ActivityGroup, SubscriptionPlanType.Subscription, msgs, dc);
        }

        public static bool HasAccountActivityGroup(this ActivityGroup activityGroup, int accountId, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (activityGroup.AccountId != accountId)
                return AccountActivityGroup.GetCount(
                    new System.Collections.Hashtable()
                    {
                        { "AccountId", accountId },
                        { "ActivityGroupId", activityGroup.ID.Value }
                    },
                    msgs,
                    dc
                    ) > 0;

            return false;
        }

        public static AccountActivityGroup GetAccountActivityGroup(this ActivityGroup activityGroup, Account currentAccount, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return activityGroup.AccountId != currentAccount.ID ? AccountActivityGroup.LoadByAccountIdByActivityGroupId(currentAccount.ID.Value, activityGroup.ID.Value, msgs) : null;
        }

        public static List<AccountActivityGroup> GetAccountActivityGroups(this ActivityGroup activityGroup, Account currentAccount, List<AccountType> accountTypes, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();
            var vistorAccounts = currentAccount.GetVisitorAccounts(accountTypes, msgs, dc);
            var accountActivityGroups = new List<AccountActivityGroup>();

            vistorAccounts.ForEach(visitorAccount =>
                accountActivityGroups.Add(AccountActivityGroup.LoadByAccountIdByActivityGroupId(visitorAccount.ID.Value, activityGroup.ID.Value, msgs, dc))
            );

            return accountActivityGroups;
        }

        public static List<Activity> GetLeagueTeams(this ActivityGroup activityGroup, Account currentAccount, List<AccountType> accountTypes, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var teams = new List<Activity>();
            var accountActivityGroups = activityGroup.GetAccountActivityGroups(currentAccount, accountTypes, msgs, dc);

            accountActivityGroups.ForEach(accountActivityGroup =>
            {
                if (accountActivityGroup.IsNotEmpty())
                    teams.AddRange(Activity.LoadByAccountActivityGroupId(accountActivityGroup.ID.Value, msgs, dc));
            });

            return teams;
        }

        public static int GetLeagueInviteCount(this ActivityGroup activityGroup, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return AccountActivityGroup.GetCount(
                new System.Collections.Hashtable()
                {
                    { "activitygroupid", activityGroup.ID.Value}
                },
                msgs);
        }

        internal static ActivityGroup SaveActivityGroup(WebUser loggedInUser, Account account, ActivityGroupType activityGroupType, string activityGroupUid, Dictionary<string, object> values, SystemMessages msgs, DatabaseConnection dc = null)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            var name = values.GetValueOrDefault("name");
            var regmode = values.GetValueOrDefault("regmode");
            var form = values.GetValueOrDefault("form");
            var description = values.GetValueOrDefault("description");
            var details = values.GetValueOrDefault("details");
            var isinviteonly = values.GetValueOrDefault("isinviteonly");
            var maxgradeid = values.GetValueOrDefault("maxgrade");
            var mingradeid = values.GetValueOrDefault("mingrade");
            var genderrestrictions = values.GetValueOrDefault("genderrestrictions");
            var daysoftheweek = values.GetValueOrDefault("daysoftheweek");
            var repeatsweekly = values.GetValueOrDefault("repeatsweekly");
            var startdate = values.GetValueOrDefault("startdate");
            var enddate = values.GetValueOrDefault("enddate");
            var dailystarttime = values.GetValueOrDefault("dailystarttime");
            var dailyendtime = values.GetValueOrDefault("dailyendtime");
            var scheduledetails = values.GetValueOrDefault("scheduledetails");
            var scheduledates = values.GetValueOrDefault("scheduledates");
            var scheduledatesClosed = values.GetValueOrDefault("scheduledatesclosed");
            var schedule = values.GetValueOrDefault("schedule");
            var isongoing = values.GetValueOrDefault("isongoing");
            var contactname = values.GetValueOrDefault("contactname");
            var contactemail = values.GetValueOrDefault("contactemail");
            var contactphone = values.GetValueOrDefault("contactphone");
            var numberofseats = values.GetValueOrDefault("seatsavailable");
            var numberofwaitinglistseats = values.GetValueOrDefault("waitinglistseats");
            var haspaymentplan = values.GetValueOrDefault("haspaymentplan");
            var paymentplancost = values.GetValueOrDefault("paymentplancost");
            var paymentplanperiod = values.GetValueOrDefault("paymentplanperiod");
            var paymentplanperiodnumber = values.GetValueOrDefault("paymentplanperiodnumber");
            var paymentplanpaymentstarttype = values.GetValueOrDefault("paymentplanpaymentstarttype");
            var cost = values.GetValueOrDefault("cost");
            var allowtopaylater = values.GetValueOrDefault("allowtopaylater");
            var requiresparentalconsent = values.GetValueOrDefault("requiresparentalconsent");

            var group = ActivityGroup.LoadByUidByAccountId(activityGroupUid.ToGuid(), account.ID.Value, msgs, dc);
            var isNew = false;

            if (group.IsEmpty())
            {
                if (name.IsEmpty())
                    msgs.Add("Name is required", MessageType.UserError);
                else if (ActivityGroup.ExistsByAccountIdByName(account.ID.Value, name.ToString(), activityGroupUid.ToGuid()))
                    msgs.Add("Name is already in use", MessageType.UserError);

                if (regmode.IsEmpty())
                    msgs.Add("Registration Mode is required", MessageType.UserError);

                if (activityGroupType == ActivityGroupType.Session && form.IsEmpty())
                    msgs.Add("Form is required", MessageType.UserError);
            }

            if (group.IsEmpty())
            {
                group = new ActivityGroup();
                group.CreateDate = DateTime.UtcNow;
                group.CreatedBy = loggedInUser.ID;
                group.Uid = activityGroupUid.ToGuid();
                group.ActivityGroupTypeId = activityGroupType;
                group.RequiresParentalConsent = true;
                isNew = true;
            }

            if (name.IsNotEmpty())
                group.Name = name.ToString().MaxChars(50);

            if (description.IsNotEmpty())
                group.Description = description.ToString().MaxChars(400);

            if (details.IsNotEmpty())
                group.Details = details.ToString().MaxChars(4000);

            if (startdate.IsNotEmpty() && startdate.ToString().IsDate())
                group.StartDate = startdate.ToString().ToDateTime();

            if (enddate.IsNotEmpty() && enddate.ToString().IsDate())
                group.EndDate = enddate.ToString().ToDateTime();

            if (regmode.IsNumber())
                group.RegistrationModeId = (RegistrationMode)regmode.ToString().ToInteger();

            if (form.IsNumber())
                group.FormId = form.ToString().ToInteger();
            
            if (maxgradeid.IsNumber())
                group.MaxGradeLevelId = maxgradeid.ToString().ToInteger();

            if (mingradeid.IsNumber())
                group.MinGradeLevelId = mingradeid.ToString().ToInteger();

            if (genderrestrictions.IsNumber())
                group.GenderRestriction = (GenderRestrictionType)genderrestrictions.ToString().ToInteger();

            group.IsInviteOnly = false;
            if (isinviteonly.IsNotEmpty())
                group.IsInviteOnly = isinviteonly.ToString().ToBoolean();

            if (contactname.IsNotEmpty())
                group.ContactName = contactname.ToString();

            if (contactemail.IsNotEmpty())
                group.ContactEmail = contactemail.ToString();

            if (contactphone.IsNotEmpty())
                group.ContactPhone = contactphone.ToString();

            if (numberofseats.IsNumber())
                group.SeatsAvailable = numberofseats.ToString().ToInteger();

            if (numberofwaitinglistseats.IsNumber())
                group.WaitingListSeats = numberofwaitinglistseats.ToString().ToInteger();

            if (cost.IsNumber())
                group.Cost = cost.ToString().ToDecimal();

            if (requiresparentalconsent.IsNotEmpty() && requiresparentalconsent.ToString().IsBoolean())
                group.RequiresParentalConsent = requiresparentalconsent.ToString().ToBoolean();

            if (allowtopaylater.IsNotEmpty() && allowtopaylater.ToString().IsBoolean())
            {
                if (account.CanProcessPayments(msgs, dc))
                    group.AllowToPayLater = allowtopaylater.ToString().ToBoolean();
                else
                    group.AllowToPayLater = true;
            }

            if (msgs.HasNoErrors())
            {
                Database.ExecuteTransaction
                (
                    action: (SystemMessages amsgs, DatabaseConnection adc) =>
                    {
                        if (form.IsNotEmpty())
                        {
                            if (form.ToString().ToInteger() == 0)
                            {
                                var defaultForm = account.GetOrCreateDefaultForm(loggedInUser, amsgs, adc);
                                var newForm = defaultForm.Copy(
                                    loggedInUser.ID.Value,
                                    account.ID.Value,
                                    FormType.Registration,
                                    (group.Name + " Form").MaxChars(100, "..."),                                    
                                    amsgs,
                                    adc);
                                group.FormId = newForm.ID;
                            }
                            else
                                group.FormId = form.ToString().ToInteger();
                        }

                        group.AccountId = account.ID;

                        if (isNew)
                            group.UniqueName = ActivityGroup.GenerateUniqueName(group.Name, group.ID, group.AccountId, amsgs, adc);

                        group.LastModifiedDate = DateTime.UtcNow;
                        group.LastModifiedBy = loggedInUser.ID;
                        amsgs.AddRange(group.Save(adc));

                        // keep activities up to date with groups
                        group.GetActivities(null, false, false, null, null, false, amsgs, adc)
                            .ForEach(activity => {

                                activity.RegistrationModeId = group.RegistrationModeId;

                                if (group.MinGradeLevelId.HasValue)
                                {
                                    if (group.GenderRestriction == GenderRestrictionType.None)
                                        activity.GenderRestriction = group.GenderRestriction;

                                    if (group.MinGradeLevelId.HasValue && activity.GradeLevelStart < group.MinGradeLevelId)
                                        activity.GradeLevelStart = group.MinGradeLevelId;

                                    if (group.MaxGradeLevelId.HasValue && activity.GradeLevelEnd > group.MaxGradeLevelId)
                                        activity.GradeLevelEnd = group.MaxGradeLevelId;
                                }

                                amsgs.AddRange(activity.Save(adc));

                                // update form
                                var activityForm = activity.GetActiveActivityForm(amsgs, adc);
                                if (activityForm.IsEmpty())
                                {
                                    activity.AddFormAndActivitate(group.GetForm(amsgs, adc), loggedInUser.ID.IfEmpty(0), msgs: amsgs, dc: adc);
                                }
                                else
                                {
                                    activityForm.FormId = group.FormId;
                                    amsgs.AddRange(activityForm.Save(adc));
                                }
                            });

                        if (isNew)
                        {
                            group.AddOrUpdateGroupProperty(AccountPropertyType.LandingPageText, "Please register below.", loggedInUser.ID.Value, amsgs, adc);
                            group.AddOrUpdateGroupProperty(AccountPropertyType.RegistrationThankYouEmail, "Thank you for registering.", loggedInUser.ID.Value, amsgs, adc);
                            group.AddOrUpdateGroupProperty(AccountPropertyType.ApplicationThankYouEmail, "Thank you for applying.", loggedInUser.ID.Value, amsgs, adc);
                            group.AddOrUpdateGroupProperty(AccountPropertyType.NoActivitiesText, "Registration is now closed.", loggedInUser.ID.Value, amsgs, adc);
                        }

                        // setup schedule
                        if (amsgs.HasNoErrors() && (schedule.IsNotEmpty() || scheduledates.IsNotEmpty()))
                        {
                            amsgs.AddRange(group.SetupSchedule(
                                account,
                                schedule.IfEmpty(""),
                                (List<DateRange>)scheduledates,
                                (List<DateTime>)scheduledatesClosed,
                                scheduledetails.IfEmpty(""),
                                repeatsweekly.IfEmpty(""),
                                startdate.IfEmpty(""),
                                enddate.ToString(),
                                dailystarttime.IfEmpty(""),
                                dailyendtime.IfEmpty(""),
                                isongoing.IfEmpty(""),
                                loggedInUser.ID.Value,
                                adc));
                        }

                        //setup PaymentPlan
                        if (amsgs.HasNoErrors() && haspaymentplan != null && haspaymentplan.ToString().IsBoolean())
                            amsgs.AddRange(group.SetupPaymentPlan(
                                account,
                                haspaymentplan.IfEmpty(""),
                                paymentplancost.IfEmpty(""),
                                paymentplanperiod.IfEmpty(""),
                                paymentplanperiodnumber.IfEmpty(""),
                                paymentplanpaymentstarttype.IfEmpty(""),
                                loggedInUser.ID.Value,
                                adc
                            ));
                    },
                    onSuccess: (SystemMessages smsgs) => msgs.AddRange(smsgs),
                    onFailure: (SystemMessages fmsgs) => msgs.AddRange(fmsgs),
                    dc: dc
                );
            }

            return group;
        }
        private static SystemMessages SetupPaymentPlan(this ActivityGroup group, Account account, string haspaymentplan, string paymentplancost, string paymentplanperiod, string paymentplanperiodnumber, string paymentplanpaymentstarttype, int loggedInUserId, DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();
            var hasChanges = true;
            var planStartType = paymentplanpaymentstarttype.ToInteger().ToEnum<PlanPaymentStartType>();

            // first: desactivate the current plan if there are any changes
            if (group.HasPaymentPlan(msgs, dc))
            {
                var plan = group.GetPaymentPlan(msgs, dc);

                hasChanges = plan.CostPerPeriod.Value != paymentplancost.ToDecimal() ||
                            plan.BillingPeriodTypeId != (BillingPeriodType)paymentplanperiod.ToInteger() ||
                            plan.TotalPeriods.Value != paymentplanperiodnumber.ToInteger() ||
                            plan.PaymentStartTypeId != planStartType; ;

                if (hasChanges || !haspaymentplan.ToBoolean())
                    msgs.AddRange(plan.Deactivate(loggedInUserId, dc));
            }

            // if the subscription plan option(UI) is activated
            if (haspaymentplan.ToBoolean() && hasChanges)
                SubscriptionPlanService.CreateNewPaymentPlan(
                    account,
                    ParticipationObjectType.ActivityGroup,
                    group.ID.Value,
                    paymentplancost.ToDecimal(),
                    (BillingPeriodType)paymentplanperiod.ToInteger(),
                    paymentplanperiodnumber.ToInteger(),
                    planStartType,
                    false,
                    loggedInUserId,
                    msgs,
                    dc);

            return msgs;
        }

        private static SystemMessages SetupSchedule(this ActivityGroup activityGroup, Account account, string schedule, List<DateRange> scheduledates, List<DateTime> scheduledatesclosed, string scheduledetails, string repeatsweekly, string startdate, string enddate, string dailystarttime, string dailyendtime, string isongoing, int loggedInUserId, DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();

            // activityGroup.ScheduleRequiresAllDays = true;

            var newSchedule = Services.ActivityScheduleService.SetupSchedule(
                 activityGroup.AccountId.Value,
                 schedule.IfEmpty("0").ToInteger(),
                 $"{activityGroup.Name} - Schedule",
                 scheduledetails.ToString(),
                 repeatsweekly.ToString(),
                 isongoing.ToString(),
                 startdate.ToString(),
                 enddate.ToString(),
                 dailystarttime.ToString(),
                 dailyendtime.ToString(),
                 scheduledates,
                 scheduledatesclosed,
                 account,
                 activityGroup.ID,
                 loggedInUserId,
                 msgs,
                 dc);

            activityGroup.ActivityScheduleId = newSchedule.ID;

            msgs.AddRange(activityGroup.Save(dc));

            return msgs;
        }

        public static SystemMessages SetIsPublished(this ActivityGroup group, bool isPublished, int loggedInUserId, DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();

            group.IsPublished = isPublished;
            group.LastModifiedBy = loggedInUserId;
            group.LastModifiedDate = DateTime.UtcNow;
            msgs.AddRange(group.Save(dc: dc));

            return msgs;
        }

        public static ActivityGroup Copy(this ActivityGroup activityGroup, string newName, bool useSameForm, int loggedInUserId, SystemMessages msgs, DatabaseConnection dc = null)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();
            ActivityGroup copy = null;

            Database.ExecuteTransaction
            (
                action: (SystemMessages amsgs, DatabaseConnection adc) =>
                {
                    // copy group
                    copy = new ActivityGroup
                    {
                        AccountId = activityGroup.AccountId,
                        CreateDate = DateTime.UtcNow,
                        LastModifiedDate = DateTime.UtcNow,
                        CreatedBy = loggedInUserId,
                        LastModifiedBy = loggedInUserId,
                        Description = activityGroup.Description,
                        FormId = activityGroup.FormId,
                        IsArchived = activityGroup.IsArchived,
                        IsInviteOnly = activityGroup.IsInviteOnly,
                        IsPublished = false,
                        Name = newName,
                        RegistrationModeId = activityGroup.RegistrationModeId,
                        UniqueName = ActivityGroup.GenerateUniqueName(newName, null, activityGroup.AccountId, amsgs, adc),
                        ActivityGroupTypeId = activityGroup.ActivityGroupTypeId,
                        ActivityScheduleId = null,
                        GenderRestriction = activityGroup.GenderRestriction,
                        MaxGradeLevelId = activityGroup.MaxGradeLevelId,
                        MinGradeLevelId = activityGroup.MinGradeLevelId,
                        ContactEmail = activityGroup.ContactEmail,
                        ContactName = activityGroup.ContactName,
                        ContactPhone = activityGroup.ContactPhone,
                        SeatsAvailable = activityGroup.SeatsAvailable,
                        WaitingListSeats = activityGroup.WaitingListSeats,
                        Cost = activityGroup.Cost,
                        CostUnitTypeId = activityGroup.CostUnitTypeId,
                        AllowToPayLater = activityGroup.AllowToPayLater,
                        Details = activityGroup.Details,
                        RequiresParentalConsent = activityGroup.RequiresParentalConsent,
                    };
                    amsgs.AddRange(copy.Save(adc));

                    // copy AccountProperties / Content
                    activityGroup.GetGroupProperties(amsgs, adc).ForEach(item => {
                        var newprop = new AccountProperty
                        {
                            AccountPropertyTypeId = item.AccountPropertyTypeId,
                            ActivityGroupId = copy.ID,
                            CreateDate = DateTime.UtcNow,
                            LastModifiedDate = DateTime.UtcNow,
                            CreatedBy = loggedInUserId,
                            LastModifiedBy = loggedInUserId,
                            Value = item.Value,
                        };
                        amsgs.AddRange(newprop.Save(adc));
                    });

                    // activities
                    activityGroup.GetActivities(amsgs, adc).ForEach(activity => {
                        var newact = activity.Copy(
                                        newName: activity.Name, 
                                        useSameForm: true,
                                        includeSignUpPeriods: false,
                                        includeSchedule: false,
                                        loggedInUserId: loggedInUserId, 
                                        msgs: amsgs, 
                                        dc: adc);

                        newact.ActivityGroupId = copy.ID;
                        newact.ActivityScheduleId = null;
                        newact.IsPublished = true;
                        amsgs.AddRange(newact.Save(adc));
                    });

                    var oldPaymentPlan = activityGroup.GetPaymentPlan(amsgs, adc);
                    if (oldPaymentPlan.IsNotEmpty())
                        oldPaymentPlan.Copy(copy.ID.Value, loggedInUserId, amsgs, adc);

                },
                onSuccess: (SystemMessages smsgs) => msgs.AddRange(smsgs),
                onFailure: (SystemMessages fmsgs) => msgs.AddRange(fmsgs),
                onException: (SystemMessages emsgs) => Helpers.Utilities.SystemErrorLog(emsgs),
                dc: dc
            );

            return copy;
        }

        public static ActivityGroup SaveSession(WebUser loggedInUser, Account account, string activityGroupUid, Dictionary<string, object> values, SystemMessages msgs, DatabaseConnection dc = null)
        {
            return SaveActivityGroup(loggedInUser, account, ActivityGroupType.Session, activityGroupUid, values, msgs, dc);
        }

        public static ActivityGroup SaveLeague(WebUser loggedInUser, Account account, string activityGroupUid, Dictionary<string, object> values, SystemMessages msgs, DatabaseConnection dc = null)
        {
            return SaveActivityGroup(loggedInUser, account, ActivityGroupType.League, activityGroupUid, values, msgs, dc);
        }

        public static SubscriptionPlan GetPaymentPlan(this ActivityGroup group, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return SubscriptionPlan.LoadActiveByParticipationObject(group.ID.Value, ParticipationObjectType.ActivityGroup, msgs, dc)
                .Where(plan => plan.PlanTypeId == SubscriptionPlanType.PaymentPlan && plan.IsActive)
                .FirstOrDefault();
        }

        public static RegistrationPeriod AddNewRegistrationPeriod(this ActivityGroup group, DateTime startDateTime, DateTime endDateTime, WebUser loggedInUser, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var regperiod = new RegistrationPeriod
            {
                CreatedBy = loggedInUser.ID,
                CreateDate = DateTime.UtcNow,
                ActivityGroupId = group.ID,
                AccountId = group.AccountId,
                StartDateTime = startDateTime,
                EndDateTime = endDateTime,
                LastModifiedBy = loggedInUser.ID,
                LastModifiedDate = DateTime.UtcNow
            };
            msgs.AddRange(regperiod.Save());

            return regperiod;
        }

        public static bool IsInvited(this ActivityGroup group, string invite, SystemMessages msgs, DatabaseConnection dc = null)
        {
            return invite.IsGuid() && group.IsInvited(invite.ToGuid(), msgs, dc);
        }

        public static bool IsInvited(this ActivityGroup group, Guid invite, SystemMessages msgs, DatabaseConnection dc = null)
        {
            return RegistrantActivityGroup.ExistsByUid(invite, msgs, dc);
        }

        public static void DeclineInvite(this ActivityGroup group, Guid invite, SystemMessages msgs, DatabaseConnection dc = null)
        {
            RegistrantActivityGroup.LoadByUid(invite, msgs, dc)?.DeclineInvite(0, msgs, dc);
        }

        public static void EmailInvitedContacts(int id, int periodId, List<int> contactIds, string emailmessage, string inviteUrl, int loggedInUserId, SystemMessages msgs, DatabaseConnection dc = null)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();
            var group = ActivityGroup.Load(id, msgs, dc);
            var account = group.GetAccount(msgs, dc);
            var loggedInUser = WebUser.Load(loggedInUserId, msgs, dc);
            var contacts = Registrant.LoadList(contactIds, msgs, dc);
            var period = RegistrationPeriod.Load(periodId, msgs, dc);

            if (contacts.IsNotEmpty() && contacts.Any())
                contacts.ForEach(contact => {

                    var rag = RegistrantActivityGroup.LoadByActivityGroupIdByRegistrantId(group.ID.Value, contact.ID.Value, msgs, dc);

                    Helpers.EmailHelper.SendSessionInviteEmail(
                        contact,
                        account,
                        group,
                        rag,
                        period,
                        loggedInUser,
                        emailmessage,
                        msgs);
                });
        }

        public static int GetSeatsRemaining(this ActivityGroup group, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var rtn = 0;
            if (group.SeatsAvailable.HasValue)
                rtn = group.SeatsAvailable.Value - group.GetRegistrantCountByAssignmentTypes(Registration.ReservedStatuses, msgs, dc);

            return Math.Max(0, rtn);
        }

        public static int GetWaitingListSeatsRemaining(this ActivityGroup group, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var rtn = 0;
            if (group.WaitingListSeats.HasValue)
                rtn = group.WaitingListSeats.Value - group.GetRegistrantCountByAssignmentType(RegistrationAssignmentType.WaitingList, msgs, dc);

            return Math.Max(0, rtn);
        }

        public static int GetRegistrantCountByAssignmentType(this ActivityGroup group, RegistrationAssignmentType assignmentType, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return RegistrantActivity.GetSeatCountByAssignmentType(
                assignmentType: assignmentType,
                participationObjectId: group.ID,
                participationObjectType: ParticipationObjectType.ActivityGroup,
                msgs: msgs,
                dc: dc);
        }

        public static int GetRegistrantCountByAssignmentTypes(this ActivityGroup group, List<RegistrationAssignmentType> assignmentTypes, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return RegistrantActivity.GetSeatCountByAssignmentTypes(
                assignmentTypes: assignmentTypes,
                participationObjectId: group.ID,
                participationObjectType: ParticipationObjectType.ActivityGroup,
                msgs: msgs,
                dc: dc);
        }

        public static List<RegistrantAssignment> RandomizePlacementAttempt(this ActivityGroup group, WebUser loggedInUser, SystemMessages msgs, DatabaseConnection dc = null)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();
            var rr = new RegistrationRandomization(group);
            rr.LoggedInUser = loggedInUser;
            return rr.AttemptLottery(msgs, dc);
        }

        public static void RandomizePlacementApply(this ActivityGroup group, List<RegistrantAssignment> assignments, WebUser loggedInUser, SystemMessages msgs, DatabaseConnection dc = null)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();
            var rr = new RegistrationRandomization(group);
            rr.LoggedInUser = loggedInUser;
            rr.ApplyLottery(assignments, msgs, dc);
        }

        public static SystemMessages RandomizePlacementReset(this ActivityGroup group, WebUser loggedInUser, SystemMessages msgs, DatabaseConnection dc = null)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            Database.ExecuteTransaction(
                action: (SystemMessages amsgs, BasicLibrary.DataAccess.DatabaseConnection adc) =>
                {
                    group.GetRegistrantActivities(msgs: amsgs, dc: adc).ForEach(
                        ra => ra.ChangeSeatAssignment(RegistrationAssignmentType.Applied, loggedInUser, amsgs, adc)
                    );
                },
                onSuccess: (SystemMessages smsgs) => msgs.AddRange(smsgs),
                onFailure: (SystemMessages fmsgs) => msgs.AddRange(fmsgs),
                onException: (SystemMessages emsgs) => Helpers.Utilities.SystemErrorLog(emsgs),
                dc: dc
            );

            return msgs;
        }

        public static ActivityForm GetDefaultActivityForm(this ActivityGroup group, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var form = group.GetForm(msgs, dc);

            if (form.IsNotEmpty())
                return new ActivityForm()
                {
                    FormId = form.ID,
                    IsActive = true,
                    AccountId = group.AccountId
                };

            return null;
        }

        public static Form GetForm(this ActivityGroup group, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (group.FormId.HasValue)
                return Form.Load(group.FormId.Value, msgs, dc);

            return null;
        }

        public static void InviteContacts(this ActivityGroup group, List<Registrant> contacts, RegistrationPeriod period, List<int> activityIds, int loggedInUserId, SystemMessages msgs, DatabaseConnection dc = null)
        {
            contacts.ForEach(contact => {
                group.InviteContact(contact, period?.ID, 1, loggedInUserId, msgs, dc);

                if (activityIds.Any())
                    contact.AddInviteToActivities(activityIds, loggedInUserId, group.ID, period?.ID, msgs, dc);
            });
        }

        public static RegistrantActivityGroup InviteContact(this ActivityGroup group, Registrant contact, int? periodId, int invitationSeats, int loggedInUserId, SystemMessages msgs, DatabaseConnection dc = null)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();
            var rag = RegistrantActivityGroup.LoadByActivityGroupIdByRegistrantId(group.ID.Value, contact.ID.Value, msgs, dc);

            if (rag.IsEmpty())
            {
                rag = new RegistrantActivityGroup();
                rag.CreateDate = DateTime.UtcNow;
                rag.CreatedBy = loggedInUserId;
                rag.ActivityGroupId = group.ID;
                rag.RegistrantId = contact.ID;
            }
            rag.NumberOfInvitationSeats = invitationSeats;
            rag.NumberOfSeatsDeclined = 0;
            rag.RegistrationPeriodId = periodId;
            rag.LastModifiedDate = DateTime.UtcNow;
            rag.LastModifiedBy = loggedInUserId;
            msgs.AddRange(rag.Save(dc));
            return rag;
        }

        public static bool IsInSession(this ActivityGroup activityGroup, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var account = activityGroup.GetAccount(msgs, dc);
            var now = account.ConvertToAccountTimeZone(DateTime.UtcNow);
            var schedule = activityGroup.GetSchedule(msgs, dc);
            var instances = schedule?.GetInstances(msgs: msgs, dc: dc);

            if (instances.IsNotEmpty() && instances.Any())
                return instances.First().StartDateTime <= now && now <= instances.Last().EndDateTime;

            if (schedule.IsNotEmpty())
                return schedule.IsOngoing;

            return false;
        }

        public static bool WillBeInSession(this ActivityGroup activityGroup, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var account = activityGroup.GetAccount(msgs, dc);
            var now = account.ConvertToAccountTimeZone(DateTime.UtcNow);
            var instances = activityGroup.GetSchedule(msgs, dc)?.GetInstances(msgs: msgs, dc: dc);

            if (instances.IsNotEmpty() && instances.Any())
                return now < instances.First().StartDateTime;

            return false;
        }

        public static bool IsOutOfSession(this ActivityGroup activityGroup, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var account = activityGroup.GetAccount(msgs, dc);
            var now = account.ConvertToAccountTimeZone(DateTime.UtcNow);
            var schedule = activityGroup.GetSchedule(msgs, dc);
            var instances = schedule?.GetInstances(msgs: msgs, dc: dc);

            if (instances.IsNotEmpty() && instances.Any())
                return now > instances.Last().StartDateTime;

            if (schedule.IsNotEmpty())
                return !schedule.IsOngoing;

            return true;
        }

        public static ActivityMessage AddMessage(this ActivityGroup group, string subject, string body, List<int> registrantIds, bool sendToStaff, int loggedInUserId, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();
            ActivityMessage am = null;

            Database.ExecuteTransaction
            (
                action: (SystemMessages amsgs, DatabaseConnection adc) =>
                {
                    am = new ActivityMessage
                    {
                        ActivityGroupId = group.ID,
                        AccountId = group.AccountId,
                        Subject = subject.MaxChars(150),
                        Body = body,
                        SendToStaff = sendToStaff,
                        CreateDate = DateTime.UtcNow,
                        CreatedBy = loggedInUserId
                    };
                    amsgs.AddRange(am.Save(dc));

                    if (amsgs.HasNoErrors())
                        registrantIds.ForEach(registrantId => am.AddRegistrant(registrantId, loggedInUserId, amsgs, adc));
                },
                onSuccess: (SystemMessages smsgs) => msgs.AddRange(smsgs),
                onFailure: (SystemMessages fmsgs) => msgs.AddRange(fmsgs),
                onException: (SystemMessages emsgs) => Helpers.Utilities.SystemErrorLog(emsgs),
                dc: dc
            );

            return am;
        }

        public static SystemMessages MoveToArchive(this ActivityGroup group, int loggedInUserId, DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();

            group.IsArchived = true;
            group.LastModifiedDate = DateTime.UtcNow;
            group.LastModifiedBy = loggedInUserId;
            msgs.AddRange(group.Save(dc));

            return msgs;
        }

        public static List<ActivitySchedule> GetSchedules(this ActivityGroup group, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return ActivitySchedule.LoadByActivityGroupId(group.ID.Value, msgs, dc);
        }

        public static void MoveFromArchive(this ActivityGroup group, int loggedInUserId, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            group.IsArchived = false;
            group.LastModifiedDate = DateTime.UtcNow;
            group.LastModifiedBy = loggedInUserId;
            msgs.AddRange(group.Save(dc));
        }

        public static void SetIsDeleted(this ActivityGroup group, int loggedInUserId,bool isDeleted, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            group.LastModifiedDate = DateTime.UtcNow;
            group.LastModifiedBy = loggedInUserId;
            group.IsDeleted = isDeleted;
            msgs.AddRange(group.Save(dc));
        }

        public static List<RegistrantActivityGroup> GetInvitedContacts(this ActivityGroup group, int? start, int? length, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return RegistrantActivityGroup.LoadByActivityGroupId(group.ID.Value, start, length, msgs, dc);
        }

        public static RegistrationPeriod GetLatestRegistrationPeriod(this ActivityGroup group, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var period = group.GetCurrentOrNextRegistrationPeriod(msgs, dc);

            if (period.IsEmpty())
                period = group.GetRegistrationPeriods(msgs, dc)
                            .OrderByDescending(p => p.EndDateTime)
                            .FirstOrDefault();

            return period;
        }

        public static RegistrationPeriod GetCurrentOrNextRegistrationPeriod(this ActivityGroup group, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var period = group.GetCurrentRegistrationPeriod(msgs, dc);

            if (period.IsEmpty())
                period = group.GetNextRegistrationPeriod(msgs, dc);

            return period;
        }

        public static RegistrationPeriod GetNextRegistrationPeriod(this ActivityGroup group, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return group.GetRegistrationPeriods(msgs, dc)
                    .Where(period => period.WillOpen(group.GetAccount(msgs, dc), msgs, dc))
                    .OrderBy(period => period.StartDateTime.Value)
                    .FirstOrDefault();
        }

        public static RegistrationPeriod GetCurrentRegistrationPeriod(this ActivityGroup group, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return group.GetRegistrationPeriods(msgs, dc)
                    .Where(period => period.IsOpen(group.GetAccount(msgs, dc), msgs, dc))
                    .FirstOrDefault();
        }

        public static bool IsOpenNow(this ActivityGroup group, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return group.GetRegistrationPeriods(msgs, dc)
                    .Where(period => period.IsOpen(group.GetAccount(msgs, dc), msgs, dc))
                    .Any();
        }

        public static bool IsClosedNow(this ActivityGroup group, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return !group.IsOpenNow(msgs, dc);
        }

        public static bool WillOpen(this ActivityGroup group, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return group.GetRegistrationPeriods(msgs, dc)
                    .Where(period => period.WillOpen(group.GetAccount(msgs, dc), msgs, dc))
                    .Any();
        }

        public static TimeSpan WillOpenIn(this ActivityGroup group, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (group.WillOpen(msgs, dc))
            {
                var period = group.GetNextRegistrationPeriod(msgs, dc);

                if (period.IsNotEmpty())
                    return period.WillOpenIn(group.GetAccount(msgs, dc), msgs, dc);
            }

            return new TimeSpan();
        }

        public static TimeSpan WillCloseIn(this ActivityGroup group, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (group.IsOpenNow(msgs, dc))
            {
                var period = group.GetCurrentRegistrationPeriod(msgs, dc);

                if (period.IsNotEmpty())
                    return period.WillCloseIn(group.GetAccount(msgs, dc), msgs, dc);
            }

            return new TimeSpan();
        }

        public static void SendLotteryResults(int groupId, int loggedInUserId, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            var group = ActivityGroup.Load(groupId, msgs, dc);
            var registrations = group.GetRegistrations(msgs: msgs, dc: dc);
            var loggedInUser = WebUser.Load(loggedInUserId, msgs, dc);
            var sendBcc = false;

            registrations.ForEach(registration => {
                var masterRegistrant = registration.GetMasterRegistrant(msgs, dc);
                var registrantActivities = registration.GetRegistrantActivities(msgs, dc);
                var hasActivities = registrantActivities.Any(ra => ra.ParticipationObjectTypeId == ParticipationObjectType.Activity);
                var hasThisGroup = registrantActivities.Any(ra => ra.ParticipationObjectTypeId == ParticipationObjectType.ActivityGroup && ra.ParticipationObjectId == group.ID);

                if (hasActivities && hasThisGroup)
                    registration.SendLotteryConfirmation(
                        billingAccount: group.GetAccount(msgs, dc),
                        group: group,
                        loggedInUser: loggedInUser,
                        sendBcc: sendBcc,
                        msgs: msgs,
                        dc: dc);

                sendBcc = false;
            });
        }


        /// <summary>
        /// gets the first scheduled date for the activity, also gets the start time
        /// </summary>
        /// <param name="dc"></param>
        /// <returns></returns>
        public static DateTime? GetFirstDate(this ActivityGroup group, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (group.StartDate.HasValue)
                return group.StartDate;

            return group.GetSchedule(msgs, dc)?.GetInstances(msgs: msgs, dc: dc).FirstOrDefault()?.StartDateTime;
        }

        /// <summary>
        /// gets the last scheduled date for the activity, also gets the start time
        /// </summary>
        /// <param name="dc"></param>
        /// <returns></returns>
        public static DateTime? GetLastDate(this ActivityGroup group, SystemMessages msgs, DatabaseConnection dc = null)
        {
            if (group.EndDate.HasValue)
                return group.EndDate;

            return group.GetSchedule(msgs, dc)?.GetInstances(msgs: msgs, dc: dc).LastOrDefault()?.StartDateTime;
        }

        public static bool RestrictByGrade(this ActivityGroup group)
        {
            return group.AgeStart.IsEmpty();
        }
    }
}
