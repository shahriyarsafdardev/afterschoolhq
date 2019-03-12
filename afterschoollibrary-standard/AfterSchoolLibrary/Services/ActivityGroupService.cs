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
            return SubscriptionPlanService.CreateNewSubscription(
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
            return SubscriptionPlan.LoadByParticipationObject(activityGroup.ID.Value, ParticipationObjectType.ActivityGroup, SubscriptionPlanType.PaymentPlan, msgs, dc);
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

        public static List<AccountActivityGroup> GetAccountActivityGroups(this ActivityGroup activityGroup, Account currentAccount, AccountType accountType, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();
            var vistorAccounts = currentAccount.GetVisitorAccounts(accountType);
            List<AccountActivityGroup> accountActivityGroups = new List<AccountActivityGroup>();
            
            foreach(var visitorAccount in vistorAccounts)
            {
                accountActivityGroups.Add(AccountActivityGroup.LoadByAccountIdByActivityGroupId(visitorAccount.ID.Value, activityGroup.ID.Value));
            }

            return accountActivityGroups;
        }

        public static List<Activity> GetLeagueTeams(this ActivityGroup activityGroup, Account currentAccount, AccountType accountType, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            List<Activity> teams = new List<Activity>();
            var accountActivityGroups = activityGroup.GetAccountActivityGroups(currentAccount, accountType);

            foreach(var accountActivityGroup in accountActivityGroups)
            {
                if (accountActivityGroup.IsNotEmpty())
                {
                    teams.AddRange(Activity.LoadByAccountActivityGroupId(accountActivityGroup.ID.Value));
                }
                
            }

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

            if (name.IsEmpty())
                msgs.Add("Label is required", MessageType.UserError);
            else if (ActivityGroup.ExistsByAccountIdByName(account.ID.Value, name.ToString(), activityGroupUid.ToGuid()))
                msgs.Add("Label is already in use", MessageType.UserError);

            if (regmode.IsEmpty())
                msgs.Add("Registration Mode is required", MessageType.UserError);

            if (activityGroupType == ActivityGroupType.Session && form.IsEmpty())
                msgs.Add("Form is required", MessageType.UserError);

            var group = ActivityGroup.LoadByUid(activityGroupUid.ToGuid(), msgs, dc);
            var isNew = false;
            if (group.IsEmpty())
            {
                group = new ActivityGroup();
                group.CreateDate = DateTime.UtcNow;
                group.CreatedBy = loggedInUser.ID;
                group.Uid = activityGroupUid.ToGuid();
                group.ActivityGroupTypeId = activityGroupType;
                isNew = true;
            }

            if (name.IsNotEmpty())
                group.Name = name.ToString().MaxChars(50);

            if (description.IsNotEmpty())
                group.Description = description.ToString().MaxChars(400);

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
                                    (group.Name + " Form").MaxChars(100, "..."),
                                    amsgs,
                                    adc);
                                group.FormId = newForm.ID;
                            }
                            else
                            {
                                group.FormId = form.ToString().ToInteger();
                            }
                        }

                        group.AccountId = account.ID;

                        if (isNew)
                            group.UniqueName = ActivityGroup.GenerateUniqueName(group.Name, group.ID, group.AccountId, amsgs, adc);

                        group.LastModifiedDate = DateTime.UtcNow;
                        group.LastModifiedBy = loggedInUser.ID;
                        amsgs.AddRange(group.Save(adc));

                        group.GetActivities(null, false, false, null, null, false, amsgs, adc)
                        .ForEach(activity => {
                            activity.RegistrationModeId = group.RegistrationModeId;
                            activity.GenderRestriction = group.GenderRestriction;
                            activity.GradeLevelStart = group.MinGradeLevelId;
                            activity.GradeLevelEnd = group.MaxGradeLevelId;
                            amsgs.AddRange(activity.Save(adc));
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
            }

            return group;
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

        public static ActivityGroup Copy(this ActivityGroup activityGroup, string newName, bool useSameForm, int loggedInUserId, SystemMessages msgs, DatabaseConnection dc = null)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();
            ActivityGroup group = null;

            Database.ExecuteTransaction(
                action: (SystemMessages amsgs, DatabaseConnection adc) =>
                {
                    // copy group
                    group = new ActivityGroup
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
                        ActivityScheduleId = activityGroup.ActivityScheduleId,
                        GenderRestriction = activityGroup.GenderRestriction,
                        MaxGradeLevelId = activityGroup.MaxGradeLevelId,
                        MinGradeLevelId = activityGroup.MinGradeLevelId
                    };
                    msgs.AddRange(group.Save(adc));

                    // copy AccountProperties / Content
                    activityGroup.GetGroupProperties(msgs, adc).ForEach(item => {
                        var newprop = new AccountProperty();
                        newprop.AccountPropertyTypeId = item.AccountPropertyTypeId;
                        newprop.ActivityGroupId = group.ID;
                        newprop.CreateDate = newprop.LastModifiedDate = DateTime.UtcNow;
                        newprop.CreatedBy = newprop.LastModifiedBy = loggedInUserId;
                        newprop.Value = item.Value;
                        msgs.AddRange(newprop.Save(adc));
                    });

                    // activities
                    activityGroup.GetActivities(amsgs, adc).ForEach(activity => {
                        var newact = activity.Copy(activity.Name, true, loggedInUserId, amsgs, dc: adc);
                        newact.ActivityGroupId = group.ID;
                        newact.ActivityScheduleId = null;
                        newact.IsPublished = true;
                        msgs.AddRange(newact.Save(adc));
                    });
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

            return group;
        }

        public static ActivityGroup SaveSession(WebUser loggedInUser, Account account, string activityGroupUid, Dictionary<string, object> values, SystemMessages msgs, DatabaseConnection dc = null)
        {
            return SaveActivityGroup(loggedInUser, account, ActivityGroupType.Session, activityGroupUid, values, msgs, dc);
        }

        public static ActivityGroup SaveLeague(WebUser loggedInUser, Account account, string activityGroupUid, Dictionary<string, object> values, SystemMessages msgs, DatabaseConnection dc = null)
        {
            return SaveActivityGroup(loggedInUser, account, ActivityGroupType.League, activityGroupUid, values, msgs, dc);
        }

        public static SystemMessages EmailInvitedContacts(int id, int periodId, List<int> contactIds, string emailmessage, string inviteUrl, int loggedInUserId, DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();
            var group = ActivityGroup.Load(id, dc: dc);
            var account = group.GetAccount(dc: dc);
            var period = RegistrationPeriod.Load(periodId, dc: dc);
            var loggedInUser = WebUser.Load(loggedInUserId, dc: dc);
            var contacts = Registrant.LoadList(contactIds, dc: dc);

            if (contacts.IsNotEmpty() && contacts.Any())
            {
                var subject = "";
                var body = "";

                subject = string.Format("You have been invited to register for {0}", group.Name);

                var regOpenText = "";
                if (period.IsNotEmpty())
                {
                    if (period.IsOpen(dc: dc))
                        regOpenText = "Registration is open now.";
                    else if (group.WillOpen(dc: dc))
                        regOpenText = "Registration will open " + period.StartDateTime.ToShortDateString() + ".";
                }

                var acceptInviteText = "To accept this invite and register, please click the following link: <a href=\"{0}\">{0}</a>";
                var declineInviteText = "<p>To decline this invite, please click the following link: <a href=\"{0}\">{0}</a></p>";
                var userSignature = string.Format("<p>Regards,<br />{0}</p>", loggedInUser.DisplayName);
                var declineUrl = "";

                body = "{0}{1}<p>{2} {3}</p>{4}{5}{6}";

                contacts.ForEach(contact => {
                    var finalInviteUrl = inviteUrl;
                    var rag = RegistrantActivityGroup.LoadByActivityGroupIdByRegistrantId(group.ID.Value, contact.ID.Value, dc: dc);

                    var salutation = "<p>Hello,</p>";
                    if (contact.FirstName.IsNotBlank())
                        salutation = string.Format("<p>Hello {0},</p>", contact.FirstName);

                    finalInviteUrl += (finalInviteUrl.Contains("?") ? "&" : "?") + "invite=" + rag.Uid.ToString();
                    declineUrl = finalInviteUrl + "&declined=1";

                    var acceptInviteTextWithLink = string.Format(acceptInviteText, finalInviteUrl);
                    var declineInviteTextWithLink = string.Format(declineInviteText, declineUrl);

                    SendGridHelper.SendMail(
                        fromEmail: new SendGrid.Helpers.Mail.EmailAddress(
                            Helpers.Email.AppEmailAddress,
                            string.Format("{0} via {1}", account.Name,
                            Helpers.Utilities.AppName(account.IsAfterSchool))),
                        toEmail: new SendGrid.Helpers.Mail.EmailAddress(contact.Email, contact.Name),
                        subject: subject,
                        body: string.Format(
                            body,
                            salutation,
                            emailmessage,
                            regOpenText,
                            acceptInviteTextWithLink,
                            declineInviteTextWithLink,
                            userSignature,
                            Helpers.Email.GetAppSignature(account.IsAfterSchool, "invite_contacts")),
                        replyTo: new SendGrid.Helpers.Mail.EmailAddress(account.Email, account.Name),
                        emailTemplateId: SendGridHelper.GetGenericTemplateId(account.IsAfterSchool)
                    );
                });
            }

            return msgs;
        }

        public static int GetSeatsRemaining(this ActivityGroup group, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var rtn = 0;
            if (group.SeatsAvailable.HasValue)
                rtn = group.SeatsAvailable.Value - group.GetRegistrantCountByAssignmentType(RegistrationAssignmentType.Reserved, msgs, dc);

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
    }
}
