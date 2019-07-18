using System;
using System.Collections.Generic;
using System.Text;
using BasicLibrary;
using BasicLibrary.DataAccess;
using System.Text.RegularExpressions;
using System.Linq;
using AfterSchoolLibrary.Models;
using System.Collections;

namespace AfterSchoolLibrary.Services
{
    public static class ActivityService
    {
        public static double GetPercentPublishable(this Activity activity, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            int totalFields = 0;
            int totalErrors = 0;

            totalFields++;
            if (activity.Name.IsEmpty())
                totalErrors++;

            totalFields++;
            if (!activity.SeatsAvailable.HasValue)
                totalErrors++;

            if (!activity.ActivityGroupId.HasValue)
            {
                totalFields++;
                if (activity.GetActiveRegistrationPeriod(msgs, dc).IsEmpty())
                    totalErrors++;

                totalFields++;

                if (activity.GetActiveActivityForm(msgs, dc).IsEmpty())
                    totalErrors++;
            }

            totalFields++;
            if (activity.ContactName.IsBlank())
                totalErrors++;

            totalFields++;
            if (activity.ContactEmail.IsBlank())
                totalErrors++;

            totalFields++;
            if (activity.ContactPhone.IsBlank())
                totalErrors++;

            totalFields++;
            if (Utilities.RemoveHTMLAndEntities(activity.Details).IsBlank())
                totalErrors++;

            totalFields++;
            if (!activity.HasSchedule)
                totalErrors++;

            totalFields++;
            if (!activity.LocationId.HasValue)
                totalErrors++;

            //totalFields++;
            //if (CostPerRegistrant.HasValue && CostPerRegistrant.Value > 0 && !AllowToPayLater && !AcceptCreditCard)
            //{
            //    totalErrors++;
            //}

            if (!activity.ActivityGroupId.HasValue)
            {
                totalFields++;
                if (activity.GetActivityProperty(AccountPropertyType.ApplicationThankYouEmail, msgs, dc).IsEmpty() ||
                    activity.GetActivityProperty(AccountPropertyType.ApplicationThankYouEmail, msgs, dc).Value.IsEmpty())
                    totalErrors++;

                totalFields++;
                if (activity.GetActivityProperty(AccountPropertyType.RegistrationThankYouEmail, msgs, dc).IsEmpty() ||
                    activity.GetActivityProperty(AccountPropertyType.RegistrationThankYouEmail, msgs, dc).Value.IsEmpty())
                    totalErrors++;
            }

            return ((totalFields - totalErrors) / totalFields) * 100;

        }

        public static string GetHomePageUrl(this Activity activity, bool isAfterSchool = true, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return BuildHomePageUrl(activity.GetAccount(msgs, dc).UniqueName, activity.ActivityTypeId, activity.UniqueName);
        }

        public static string BuildHomePageUrl(string accountUniqueName, ActivityType activityType, string activityUniqueName)
        {
            return $"{Helpers.Utilities.BaseUrl(true)}/{accountUniqueName}/{activityType.ToString().ToLower()}/{activityUniqueName.ToLower()}";
        }

        public static bool IsFull(this Activity activity, DatabaseConnection dc = null)
        {
            return activity.GetSeatsRemaining(dc: dc) == 0;
        }

        public static bool IsWaitingListFull(this Activity activity, DatabaseConnection dc = null)
        {
            return activity.GetWaitingListSeatsRemaining(dc: dc) == 0;
        }

        public static Location GetLocation(this Activity activity, SystemMessages msgs = null, DatabaseConnection dc = null)
        {            
            if (activity.LocationId.HasValue)
                return Location.Load(activity.LocationId.Value, msgs, dc);

            return null;
        }

        public static ParticipationObject GetParticipationObject(this Activity activity, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return new ParticipationObject(activity, msgs, dc);
        }

        public static string GenerateUniqueName(string name, int? activityId, int? accountId, SystemMessages msgs, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            //unique name
            var reg = new Regex(BasicLibrary.Utilities.UrlFriendlyRegEx, RegexOptions.IgnoreCase | RegexOptions.Singleline);

            var origUniqueName = reg.Replace(name.GetMaxWords(25), "");
            string uniqueName = origUniqueName;
            var duplicate = Activity.LoadByUniqueNameByAccountId(uniqueName, accountId.Value, msgs, dc);
            int count = 1;
            while (duplicate.IsNotEmpty() && duplicate.ID != activityId)
            {
                uniqueName = origUniqueName + count.ToString();
                count++;
                duplicate = Activity.LoadByUniqueNameByAccountId(uniqueName, accountId.Value, msgs, dc);
            }
            return uniqueName;
        }

        public static int GetSeatsTaken(this Activity activity, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return activity.GetRegistrantCountByAssignmentTypes(
                Registration.ReservedStatuses, 
                msgs, 
                dc);
        }

        public static int GetWaitingListSeatsTaken(this Activity activity, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return activity.GetRegistrantCountByAssignmentType(RegistrationAssignmentType.WaitingList, msgs, dc);
        }

        public static int GetSeatsRemaining(this Activity activity, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var rtn = 0;
            if (activity.SeatsAvailable.HasValue)
                rtn = activity.SeatsAvailable.Value - activity.GetRegistrantCountByAssignmentTypes(
                    Registration.ReservedStatuses, 
                    msgs, 
                    dc);

            return Math.Max(0, rtn);
        }

        public static int GetWaitingListSeatsRemaining(this Activity activity, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var rtn = 0;
            if (activity.WaitingListSeats.HasValue)
                rtn = activity.WaitingListSeats.Value - activity.GetRegistrantCountByAssignmentType(RegistrationAssignmentType.WaitingList, msgs, dc);

            return Math.Max(0, rtn);
        }

        public static SubscriptionPlan GetSubscriptionPlan(this Activity activity, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return SubscriptionPlan.LoadActiveByParticipationObject(activity.ID.Value, ParticipationObjectType.Activity, msgs, dc)
                .Where(plan => plan.PlanTypeId == SubscriptionPlanType.Subscription && plan.IsActive)
                .FirstOrDefault();
        }

        public static SubscriptionPlan GetPaymentPlan(this Activity activity, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return SubscriptionPlan.LoadActiveByParticipationObject(activity.ID.Value, ParticipationObjectType.Activity, msgs, dc)
                .Where(plan => plan.PlanTypeId == SubscriptionPlanType.PaymentPlan && plan.IsActive)
                .FirstOrDefault();
        }

        public static SubscriptionPlan AddPaymentPlan(this Activity activity, decimal costPerPeriod, BillingPeriodType billingPeriodType, int totalPeriods, WebUser loggedInUser, SystemMessages msgs, DatabaseConnection dc = null)
        {
            return SubscriptionPlanService.CreateNewPaymentPlan(
                activity.GetAccount(msgs, dc),
                activity,
                costPerPeriod, 
                billingPeriodType, 
                totalPeriods, 
                loggedInUser, 
                msgs, 
                dc);
        }

        public static SubscriptionPlan AddSubscriptionPlan(this Activity activity, decimal costPerPeriod, BillingPeriodType billingPeriodType, bool doesAutoRenew, WebUser loggedInUser, SystemMessages msgs, DatabaseConnection dc = null)
        {
            return SubscriptionPlanService.CreateNewSubscriptionPlan(
                activity.GetAccount(msgs, dc), 
                activity,
                costPerPeriod, 
                billingPeriodType,
                doesAutoRenew, 
                loggedInUser, 
                msgs, 
                dc);
        }

        public static List<SubscriptionPlan> GetActivePaymentPlans(this Activity activity, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return SubscriptionPlan.LoadByParticipationObject(activity.ID.Value, ParticipationObjectType.Activity, SubscriptionPlanType.PaymentPlan, msgs, dc)
                .Where(plan => plan.IsActive)
                .ToList();
        }

        public static List<SubscriptionPlan> GetActiveSubscriptionPlans(this Activity activity, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return SubscriptionPlan.LoadByParticipationObject(activity.ID.Value, ParticipationObjectType.Activity, SubscriptionPlanType.Subscription, msgs, dc)
                .Where(plan => plan.IsActive)
                .ToList();
        }

        public static void CreateSampleActivity(Account account, SystemMessages msgs, DatabaseConnection dc = null)
        {
            var gradeLevels = GradeLevel.Load(msgs: msgs, dc: dc);

            var schedule = ActivityScheduleService.CreateNew(
                  scheduleName: "Sample Schedule",
                  details: "",
                  repeatsWeekly: true,
                  isongoing: false,
                  startDate: DateTime.UtcNow.AddWeeks(1),
                  endDate: DateTime.UtcNow.AddWeeks(5),
                  dailyStartTime: DateTime.UtcNow.Date.AddHours(15),
                  dailyEndTime: DateTime.UtcNow.Date.AddHours(17),
                  scheduleDates: null,
                  scheduleDatesClosed: null,
                  accountId: account.ID.Value,
                  activityGroupId: null,
                  loggedInUserId: 0,
                  msgs: msgs,
                  dc: dc
                );

            var locationName = "Sample Location";
            var address1 = "1234 N AfterSchool Avenue";
            var address2 = "Suite 1";
            var city = "Fishers";
            var state = "IN";
            var postalCode = "46038";

            Location location = null;
            var locations = LocationService.LoadByAddress(
                locationName,
                address1,
                address2,
                city,
                state,
                postalCode,
                msgs,
                dc);

            if (!locations.Any())
            {
                location = LocationService.CreateNew(
                    locationName: locationName,
                    address1: address1,
                    address2: address2,
                    city: city,
                    state: state,
                    postalCode: postalCode,
                    country: "US",
                    hostAccountId: account.ID.Value,
                    locationType: LocationType.School,
                    loggedInUser: null,
                    msgs: msgs,
                    dc: dc
                    );
            }

            ActivityService.CreateNew(
                activityName: "Sample Chess Activity",
                activityType: ActivityType.Activity,
                activityDetails: "<p>This activity starts with building leadership in each child that participates.</p><p>We build upon the confidence of your child to help him or her create strategies and </p>",
                seatsAvailable: 100,
                waitingListSeatsAvailable: 10,
                genderRestriction: GenderRestrictionType.None,
                contactName: "Test User",
                contactPhone: "123-123-1234",
                contactEmail: "dave@dave.com",
                costPerRegistrant: 12,
                costDescription: "Here is the main reason for the cost and what you get",
                acceptCreditCard: false,
                allowToPayLater: true,
                registrationMode: RegistrationMode.Register,
                accountId: account.ID,
                scheduleId: schedule.ID,
                locationId: location.ID,
                locationDetails: "<p>Please enter through the rear entrance of the school</p>",
                activityGroupId: null,
                minGradeLevelId: gradeLevels.First().ID,
                maxGradeLevelId: gradeLevels.Last().ID,
                loggedInUser: null,
                msgs: msgs,
                dc: dc
                );
        }

        public static Activity CreateNew(string activityName, ActivityType activityType, string activityDetails, int seatsAvailable, int? waitingListSeatsAvailable, GenderRestrictionType genderRestriction, string contactName, string contactPhone, string contactEmail, decimal? costPerRegistrant, string costDescription, bool acceptCreditCard, bool allowToPayLater, RegistrationMode registrationMode,  int? accountId, int? scheduleId, int? locationId, string locationDetails, int? activityGroupId, int? minGradeLevelId, int? maxGradeLevelId, WebUser loggedInUser, SystemMessages msgs, DatabaseConnection dc = null)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            var activity = new Activity()
            {
                Name = activityName,
                // AcceptCreditCard = acceptCreditCard,
                AllowToPayLater = allowToPayLater,
                AccountId = accountId,
                GenderRestriction = genderRestriction,
                IsPublished = false,
                ActivityTypeId = activityType,
                ActivityScheduleId = scheduleId,
                ActivityGroupId = activityGroupId,
                LocationId = locationId,
                GradeLevelStart = minGradeLevelId,
                GradeLevelEnd = maxGradeLevelId,
                ContactName = contactName,
                ContactPhone = contactPhone,
                ContactEmail = contactEmail,
                CostDescription = costDescription,
                CostPerRegistrant = costPerRegistrant,
                LastModifiedBy = loggedInUser?.ID.IfEmpty(0),
                LastModifiedDate = DateTime.UtcNow,
                CreateDate = DateTime.UtcNow,
                CreatedBy = loggedInUser?.ID.IfEmpty(0),
                RegistrationModeId = registrationMode,
                SeatsAvailable = seatsAvailable,
                WaitingListSeats = waitingListSeatsAvailable,
                UniqueName = ActivityService.GenerateUniqueName(activityName, null, accountId, msgs, dc),
                Details = activityDetails,
                LocationDetails = locationDetails
            };

            msgs.AddRange(activity.Save(dc));

            return activity;
        }        
        
        public static ActivityPayment AddPayment(this Activity activity, decimal amount, Registrant registrant, WebUser loggedInUser, SystemMessages msgs, DatabaseConnection dc = null)
        {
            var payment = new ActivityPayment()
            {
                ParticipationObjectId = activity.ID,
                ParticipationObjectTypeId = ParticipationObjectType.Activity,
                Amount = amount,
                RegistrantId = registrant.ID,
                AccountId = activity.AccountId,
                CreateDate = DateTime.UtcNow,
                CreatedBy = loggedInUser?.ID.IfEmpty(0),
                LastModifiedDate = DateTime.UtcNow,
                LastModifiedBy = loggedInUser?.ID.IfEmpty(0)
            };
            msgs.AddRange(payment.Save(dc));
            return payment;
        }

        public static decimal GetTotalPaid(this Activity activity, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return ActivityPayment.LoadByParticipationObject(activity.ID.Value, ParticipationObjectType.Activity, msgs, dc).Sum(payment => payment.Amount.Value);
        }

        public static GradeLevel GetStartingGradeLevel(this Activity activity, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return GradeLevel.Load(activity.GradeLevelStart.Value, msgs, dc);
        }

        public static GradeLevel GetEndingGradeLevel(this Activity activity, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return GradeLevel.Load(activity.GradeLevelEnd.Value, msgs, dc);
        }
        
        public static RegistrantActivity InviteContact(this Activity activity, Registrant contact, int? periodId, int invitationSeats, int loggedInUserId, SystemMessages msgs, DatabaseConnection dc = null)
        {
            return contact.AddInviteToActivity(activity.ID.Value, loggedInUserId, activity.ActivityGroupId, periodId, msgs, dc);            
        }

        public static SystemMessages InviteContacts(this Activity activity, List<Registrant> contacts, RegistrationPeriod period, int loggedInUserId, DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();

            contacts.ForEach(contact => {
                activity.InviteContact(
                    contact: contact, 
                    periodId: period?.ID, 
                    invitationSeats: 1, 
                    loggedInUserId: loggedInUserId, 
                    msgs: msgs, 
                    dc: dc);                
            });

            return msgs;
        }

        public static SystemMessages EmailInvitedContacts(int activityId, int periodId, List<int> contactIds, string emailmessage, string inviteUrl, int loggedInUserId, DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();
            var activity = Activity.Load(activityId, msgs, dc);
            var account = activity.GetAccount(dc: dc);
            var period = RegistrationPeriod.Load(periodId, dc: dc);
            var loggedInUser = WebUser.Load(loggedInUserId, dc: dc);
            var contacts = Registrant.LoadList(contactIds, dc: dc);

            if (contacts.IsNotEmpty() && contacts.Any())
            {
                var subject = "";
                var body = "";

                subject = $"You have been invited to register for {activity.Name}";

                var regOpenText = "";
                if (period.IsNotEmpty())
                {
                    if (period.IsOpen(account, msgs, dc))
                        regOpenText = "Registration is open now.";
                    else if (period.WillOpen(account, msgs, dc))
                        regOpenText = "Registration will open " + period.StartDateTime.ToShortDateString() + ".";
                }

                var acceptInviteText = "To accept this invite and register, please click the following link: <a href=\"{0}\">{0}</a>";
                var declineInviteText = "<p>To decline this invite, please click the following link: <a href=\"{0}\">{0}</a></p>";
                var userSignature = $"<p>Regards,<br />{loggedInUser.DisplayName}</p>";
                var declineUrl = "";

                body = "{0}{1}<p>{2} {3}</p>{4}{5}{6}";

                contacts.ForEach(contact => {
                    var finalInviteUrl = inviteUrl;
                    var rag = RegistrantActivity.LoadByParticipationObjectByRegistrant(
                        activity.ID.Value, 
                        ParticipationObjectType.Activity, 
                        contact.ID.Value, 
                        msgs, 
                        dc);

                    var salutation = "<p>Hello,</p>";
                    if (contact.FirstName.IsNotBlank())
                        salutation = $"<p>Hello {contact.FirstName},</p>";

                    finalInviteUrl += (finalInviteUrl.Contains("?") ? "&" : "?") + "invite=" + rag.Uid.ToString();
                    declineUrl = finalInviteUrl + "&declined=1";

                    var acceptInviteTextWithLink = string.Format(acceptInviteText, finalInviteUrl);
                    var declineInviteTextWithLink = string.Format(declineInviteText, declineUrl);

                    SendGridHelper.SendMail(
                        fromEmail: new SendGrid.Helpers.Mail.EmailAddress(
                            Helpers.Email.AppNoReplyEmailAddress,
                            $"{account.Name} via {Helpers.Utilities.AppName(account.IsAfterSchool)}"),
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

        public static decimal GetTotalAmountPaid(this Activity activity, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return ActivityPayment.GetTotalPaidByParticipationObject(activity.ID.Value, ParticipationObjectType.Activity, msgs, dc);
        }

        public static Activity CopyWithRegistrants(this Activity activity, string newName, bool useSameForm, int loggedInUserId, SystemMessages msgs, DatabaseConnection dc = null)
        {
            return activity.Copy(newName, useSameForm, true, loggedInUserId, msgs, dc);
        }

        public static Activity Copy(this Activity activity, string newName, bool useSameForm, int loggedInUserId, SystemMessages msgs, DatabaseConnection dc = null)
        {
            return activity.Copy(newName, useSameForm, false, loggedInUserId, msgs, dc);
        }

        internal static Activity Copy(this Activity activity, string newName, bool useSameForm, bool includeRegistrants, int loggedInUserId, SystemMessages msgs, DatabaseConnection dc = null)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            Activity copy = null;

            Database.ExecuteTransaction(
                action: (SystemMessages amsgs, DatabaseConnection adc) =>
                {
                    var account = activity.GetAccount(amsgs, adc);

                    copy = activity.CopyFields();
                    copy.Name = newName;
                    copy.CreateDate = DateTime.UtcNow;
                    copy.CreatedBy = loggedInUserId;
                    copy.LastModifiedDate = DateTime.UtcNow;
                    copy.LastModifiedBy = loggedInUserId;
                    copy.UniqueName = GenerateUniqueName(newName, null, account.ID, amsgs, adc);
                    amsgs.AddRange(copy.Save(adc));

                    //don't copy the whole form is the account can manage forms
                    if (account.HasFeature(FeatureType.ManageForms, amsgs, adc))
                    {
                        //get form and use the same form
                        var formToUse = activity.GetActiveForm(amsgs, adc);
                        if (formToUse.IsEmpty())
                            formToUse = account.GetDefaultForm(msgs, adc);

                        copy.AddFormAndActivitate(formToUse, loggedInUserId, false, amsgs, dc: adc);
                    }
                    //copy form completely
                    else
                    {
                        Form formToCopy = null;
                        if (useSameForm)
                            formToCopy = activity.GetActiveForm(amsgs, adc);
                        else
                            formToCopy = account.GetDefaultForm(amsgs, adc);

                        var newForm = formToCopy.Copy(loggedInUserId, copy.AccountId.Value, null, amsgs, dc: adc);
                        copy.AddFormAndActivitate(newForm, loggedInUserId, false, amsgs, dc: adc);
                    }

                    // copy team and permissions
                    activity.GetTeamMembers(amsgs, adc).ForEach(member => member.Copy(copy.ID.Value, loggedInUserId, amsgs, adc));

                    var activePeriod = activity.GetActiveRegistrationPeriod(amsgs, adc);
                    if (activePeriod.IsNotEmpty())
                    {
                        //copy registration period if doesn't have activity groups
                        if (!account.HasFeature(FeatureType.ManageActivityGroups, amsgs, adc))
                        {
                            var periodName = RegistrationPeriod.GenerateUniqueName(account.ID.Value, $"{copy.Name} - Reg Window", amsgs, adc);
                            var newPeriod = activePeriod.Copy(periodName, loggedInUserId, amsgs, dc: adc);
                            amsgs.AddRange(newPeriod.AddActivity(copy, true, loggedInUserId, adc));
                        }
                        else
                            amsgs.AddRange(activePeriod.AddActivity(copy, true, loggedInUserId, adc));
                    }

                    // copy Activity Properties
                    activity.GetActivityProperties(msgs, adc).ForEach(property => {
                        var newProperty = new AccountProperty
                        {
                            ActivityId = copy.ID,
                            Value = property.Value,
                            AccountPropertyTypeId = property.AccountPropertyTypeId,
                            LastModifiedDate = DateTime.UtcNow,
                            CreateDate = DateTime.UtcNow,
                            LastModifiedBy = loggedInUserId,
                            CreatedBy = loggedInUserId
                        };
                        amsgs.AddRange(newProperty.Save(adc));
                    });

                    // NOTE: copy schedule?
                    if (activity.ActivityScheduleId.HasValue)
                    {
                        copy.ActivityScheduleId = activity.GetSchedule(amsgs, adc)
                            .CopyAndSave(loggedInUserId, amsgs, adc)
                            .ID;
                        amsgs.AddRange(copy.Save(adc));
                    }

                    var oldSubPlan = activity.GetSubscriptionPlan(amsgs, adc);
                    if (oldSubPlan.IsNotEmpty())
                        oldSubPlan.Copy(copy.ID.Value, loggedInUserId, amsgs, adc);

                    var oldPaymentPlan = activity.GetPaymentPlan(amsgs, adc);
                    if (oldPaymentPlan.IsNotEmpty())
                        oldPaymentPlan.Copy(copy.ID.Value, loggedInUserId, amsgs, adc);

                    if (includeRegistrants)
                    {
                        // need a new registration
                        var registrantActivities = activity.GetRegistrantActivities(amsgs, adc);

                        registrantActivities.ForEach(registrantActivity => {

                            var registrant = registrantActivity.ToRegistrant();

                            // copies with no esignature because the parent isn't really signing. 
                            // TODO: Need to get it, request esignature from parent?
                            var newRegistration = registrant.AddRegistration(
                                account.ID.Value,
                                copy.ActivityGroupId,
                                copy.AccountActivityGroupId,
                                null,
                                RegistrationCompletionStatus.RequiresParentSignature,
                                loggedInUserId,
                                amsgs,
                                adc);

                            var newRegistrantActivity = new RegistrantActivity()
                            {
                                ActivityGroupId = activity.ActivityGroupId,
                                //ActivityId = copy.ID,
                                CreateDate = DateTime.UtcNow,
                                CreatedBy = loggedInUserId,
                                LastModifiedBy = loggedInUserId,
                                LastModifiedDate = DateTime.UtcNow,
                                PreferenceNumber = registrantActivity.PreferenceNumber,
                                RegistrantId = registrantActivity.RegistrantId,
                                SeatIndex = registrantActivity.SeatIndex,
                                RegistrationId = newRegistration.ID
                            };
                            amsgs.AddRange(newRegistrantActivity.Save(adc));

                            // TODO: probably should notify parents that they've been added
                        });
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

            return copy;
        }

        private static Activity CopyFields(this Activity activity)
        {
            return new Activity()
            {
                AccountId = activity.AccountId,
                ProviderAccountId = activity.ProviderAccountId,
                LeaderTypeId = activity.LeaderTypeId,
                ContactEmail = activity.ContactEmail,
                ContactPhone = activity.ContactPhone,
                ContactName = activity.ContactName,
                Description = activity.Description,
                Details = activity.Details,
                LocationDetails = activity.LocationDetails,
                SeatsAvailable = activity.SeatsAvailable,
                Uid = Guid.NewGuid(),
                WaitingListSeats = activity.WaitingListSeats,
                //AcceptPayPal = this.AcceptPayPal,
                //AcceptCreditCard = this.AcceptCreditCard,
                CostPerRegistrant = activity.CostPerRegistrant,
                CostDescription = activity.CostDescription,
                GenderRestriction = activity.GenderRestriction,
                GradeLevelStart = activity.GradeLevelStart,
                GradeLevelEnd = activity.GradeLevelEnd,
                AgeStart = activity.AgeStart,
                AgeEnd = activity.AgeEnd,
                //AllowOrganizationRegistration = this.AllowOrganizationRegistration,
                AllowToPayLater = activity.AllowToPayLater,
                RegistrationModeId = activity.RegistrationModeId,
                ActivityGroupId = activity.ActivityGroupId,
                ActivityTypeId = activity.ActivityTypeId,
                LocationId = activity.LocationId,

            };
        }

        public static SystemMessages CopyDefaultForm(this Activity activity, int loggedInUserId, DatabaseConnection dc = null)
        {
            SystemMessages msgs = new SystemMessages();
            if (activity.GetActiveForm(dc: dc).IsEmpty())
            {
                var account = activity.GetAccount(dc: dc);
                var formToCopy = account.GetDefaultForm(dc: dc);
                var newForm = formToCopy.Copy(loggedInUserId, activity.AccountId.Value, null, msgs, dc: dc);
                var af = activity.AddFormAndActivitate(newForm, loggedInUserId, false, msgs, dc: dc);

                account.DefaultFormId = af.FormId;
                msgs.AddRange(account.Save(dc: dc));
            }

            return msgs;
        }

        public static ActivityForm AddFormAndActivitate(this Activity activity, Form form, int loggedInUserId, bool isPublished = false, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            //deactivate all other ones
            activity.GetActivityForms(msgs, dc).ForEach(item => {
                item.IsActive = false;
                msgs.AddRange(item.Save(dc));
            });

            var af = ActivityForm.LoadByActivityIdByFormId(activity.ID.Value, form.ID.Value, dc: dc);

            if (af.IsEmpty())
            {
                af = new ActivityForm();
                af.ActivityId = activity.ID;
                af.AccountId = activity.AccountId;
                af.Name = $"{activity.Name} Registration Form".MaxChars(50);
                af.Description = "Please fill in the registration form below";
                af.FormId = form.ID;
                af.IsPublished = isPublished;
                af.CreateDate = DateTime.UtcNow;
                af.CreatedBy = loggedInUserId;
            }

            af.LastModifiedDate = DateTime.UtcNow;
            af.LastModifiedBy = loggedInUserId;
            af.IsActive = true;
            msgs.AddRange(af.Save(dc));

            return af;
        }

        public static Activity EditExisting(int loggedInUserId, Account account, string activityId, Dictionary<string, object> values, SystemMessages msgs = null)
        {
            return SaveActivity(loggedInUserId, account, activityId, values, msgs);
        }

        public static Activity CreateNew(int loggedInUserId, Account account, Dictionary<string, object> values, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return SaveActivity(loggedInUserId, account, null, values, msgs, dc: dc);
        }

        private static Activity SaveActivity(int loggedInUserId, Account account, string activityId, Dictionary<string, object> values, SystemMessages msgs, DatabaseConnection dc = null)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            var name = values.GetValueOrDefault("name");
            var seatsavailable = values.GetValueOrDefault("seatsavailable");
            var waitinglistseats = values.GetValueOrDefault("waitinglistseats");
            var registrationperiod = values.GetValueOrDefault("registrationperiod");
            var registrationopendate = values.GetValueOrDefault("registrationopendate");
            var registrationopentime = values.GetValueOrDefault("registrationopentime");
            var registrationclosedate = values.GetValueOrDefault("registrationclosedate");
            var registrationclosetime = values.GetValueOrDefault("registrationclosetime");
            var leadertype = values.GetValueOrDefault("leadertype");
            var timezone = values.GetValueOrDefault("timezone");
            var contactname = values.GetValueOrDefault("contactname");
            var contactphone = values.GetValueOrDefault("contactphone");
            var contactemail = values.GetValueOrDefault("contactemail");
            var instructor = values.GetValueOrDefault("instructor");
            var instructorfirstname = values.GetValueOrDefault("instructorfirstname");
            var instructorlastname = values.GetValueOrDefault("instructorlastname");
            var instructoremail = values.GetValueOrDefault("instructoremail");
            var instructorphone = values.GetValueOrDefault("instructorphone");
            var details = values.GetValueOrDefault("details");
            var location = values.GetValueOrDefault("location");
            var locationname = values.GetValueOrDefault("locationname");
            var locationdetails = values.GetValueOrDefault("locationdetails");
            var scheduledetails = values.GetValueOrDefault("scheduledetails");
            var address1 = values.GetValueOrDefault("address1");
            var address2 = values.GetValueOrDefault("address2");
            var city = values.GetValueOrDefault("city");
            var state = values.GetValueOrDefault("state");
            var postalcode = values.GetValueOrDefault("postalcode");
            var ismylocation = values.GetValueOrDefault("mylocation");
            var locationtype = values.GetValueOrDefault("locationtype");
            var cost = values.GetValueOrDefault("cost");
            var costdescription = values.GetValueOrDefault("costdescription");
            var acceptpaypal = values.GetValueOrDefault("acceptpaypal");
            var acceptcreditcard = values.GetValueOrDefault("acceptcreditcard");
            var allowtopaylater = values.GetValueOrDefault("allowtopaylater");
            var form = values.GetValueOrDefault("form");
            var alloworganization = values.GetValueOrDefault("alloworganization");
            var requiresparentalconsent = values.GetValueOrDefault("requiresparentalconsent");
            var agestart = values.GetValueOrDefault("agestart");
            var ageend = values.GetValueOrDefault("ageend");
            var gradestart = values.GetValueOrDefault("gradestart");
            var gradeend = values.GetValueOrDefault("gradeend");
            var genderrestrictions = values.GetValueOrDefault("genderrestrictions");
            var regmode = values.GetValueOrDefault("regmode");
            var days = values.GetValueOrDefault("days");
            var gradeIds = values.GetValueOrDefault("gradeIds");
            var group = values.GetValueOrDefault("group");
            var registrationthankyouemail = values.GetValueOrDefault("registrationthankyouemail");
            var applicationthankyouemail = values.GetValueOrDefault("applicationthankyouemail");
            var daysoftheweek = values.GetValueOrDefault("daysoftheweek");
            var repeatsweekly = values.GetValueOrDefault("repeatsweekly");
            var isongoing = values.GetValueOrDefault("isongoing");
            var startdate = values.GetValueOrDefault("startdate");
            var enddate = values.GetValueOrDefault("enddate");
            var dailystarttime = values.GetValueOrDefault("dailystarttime");
            var dailyendtime = values.GetValueOrDefault("dailyendtime");
            var scheduledates = values.GetValueOrDefault("scheduledates");
            var scheduledatesClosed = values.GetValueOrDefault("scheduledatesclosed");
            var schedule = values.GetValueOrDefault("schedule");
            var description = values.GetValueOrDefault("description");
            var hassubscriptionplan = values.GetValueOrDefault("hassubscriptionplan");
            var subscriptionplancost = values.GetValueOrDefault("subscriptionplancost");
            var subscriptionplanperiod = values.GetValueOrDefault("subscriptionplanperiod");
            var haspaymentplan = values.GetValueOrDefault("haspaymentplan");
            var paymentplancost = values.GetValueOrDefault("paymentplancost");
            var paymentplanperiod = values.GetValueOrDefault("paymentplanperiod");
            var paymentplanperiodnumber = values.GetValueOrDefault("paymentplanperiodnumber");
            var accountactivitygroup = values.GetValueOrDefault("accountactivitygroup");
            var provider = values.GetValueOrDefault("provider");
            var providername = values.GetValueOrDefault("providername");
            var providerfirstname = values.GetValueOrDefault("providerfirstname");
            var providerlastname = values.GetValueOrDefault("providerlastname");
            var provideremail = values.GetValueOrDefault("provideremail");
            var providerphone = values.GetValueOrDefault("providerphone");

            if (!activityId.IsNumber() || activityId.ToInteger() == 0)
            {
                if (name.IsEmpty())
                    msgs.Add("Activity Name is required", MessageType.UserError);

                if (!seatsavailable.IsNumber())
                {
                    msgs.Add("Total Available Seats is required and must be greater than 0", MessageType.UserError);
                }
                else
                {
                    var ap = account.GetActiveAccountPackage(msgs, dc);
                    var maxParticipants = ap.GetFeature(FeatureType.Participants, msgs, dc).Max.Value;
                    if (maxParticipants < seatsavailable.ToString().ToInteger())
                        msgs.Add(string.Format("Total Available Seats is required and must be less than or equal to {0}", maxParticipants), MessageType.UserError);

                }
                if (group.IsEmpty() && registrationperiod.ToString().ToInteger() > 0)
                {
                    if (!$"{registrationopendate} {registrationopentime}".IsDateTime())
                        msgs.Add("Registration Open Date & Time is required", MessageType.UserError);

                    if (!$"{registrationclosedate} {registrationclosetime}".IsDateTime())
                        msgs.Add("Registration Close Date & Time is required", MessageType.UserError);
                }

                if (!waitinglistseats.IsNumber())
                    waitinglistseats = "0";

                //pre-load account info
                if (contactphone.IsBlank())
                    contactphone = account.Phone;

                if (cost.IsBlank())
                    cost = "0";
            }

            Activity activity = null;

            if (activityId.IsNotEmpty())
            {
                activity = Activity.Load(activityId.ToInteger(), msgs, dc);
                if (seatsavailable.IsNumber() &&
                    activity.GetRegistrantCountByAssignmentTypes(Registration.ReservedStatuses, msgs, dc) <= seatsavailable.ToString().ToInteger())
                {
                    activity.SeatsAvailable = seatsavailable.ToString().ToInteger();
                }
                if (waitinglistseats.IsNumber() &&
                    activity.GetRegistrantCountByAssignmentType(RegistrationAssignmentType.WaitingList, msgs, dc) <= waitinglistseats.ToString().ToInteger())
                {
                    activity.WaitingListSeats = waitinglistseats.ToString().ToInteger();
                }
            }
            else
            {
                activity = new Activity
                {
                    ActivityTypeId = ActivityType.Activity,
                    CreateDate = DateTime.UtcNow,
                    CreatedBy = loggedInUserId,
                    SeatsAvailable = seatsavailable.ToString().ToInteger(),
                    WaitingListSeats = waitinglistseats.ToString().ToInteger(),
                    AccountId = account.ID,
                    LocationId = account.DefaultLocationId,
                    RequiresParentalConsent = true
                };

                if (group.IsEmpty() && !form.IsNumber())
                    msgs.Add("Registration Form is required", MessageType.UserError);
            }

            activity.LastModifiedDate = DateTime.UtcNow;
            activity.LastModifiedBy = loggedInUserId;

            if (name.IsNotEmpty())
            {
                activity.Name = name.ToString();
                activity.UniqueName = ActivityService.GenerateUniqueName(name.ToString(), activity.ID, activity.AccountId, msgs, dc);
            }

            if (agestart.IsNumber())
            {
                if (agestart.ToString().ToInteger() >= 0)
                    activity.AgeStart = agestart.ToString().ToInteger();
                else
                    activity.AgeStart = null;
            }
            if (ageend.IsNumber())
            {
                if (ageend.ToString().ToInteger() >= 0)
                    activity.AgeEnd = ageend.ToString().ToInteger();
                else
                    activity.AgeEnd = null;
            }

            if (gradestart.IsNumber() && gradeend.IsNumber())
            {
                if (gradestart.ToString().ToInteger() > -1)
                    activity.GradeLevelStart = gradestart.ToString().ToInteger();
                else
                    activity.GradeLevelStart = null;

                if (gradeend.ToString().ToInteger() > -1)
                    activity.GradeLevelEnd = gradeend.ToString().ToInteger();
                else
                    activity.GradeLevelEnd = null;
            }

            if (genderrestrictions.IsNumber())
                activity.GenderRestriction = (GenderRestrictionType)genderrestrictions.ToString().ToInteger();

            if (requiresparentalconsent.IsNumber())
                activity.RequiresParentalConsent = (bool)requiresparentalconsent.ToString().ToBoolean();

            if (contactname.IsNotEmpty())
                activity.ContactName = contactname.ToString().IfBlank("").MaxChars(50);

            if (contactphone.IsNotEmpty())
            {
                if (contactphone.ToString().IsRegExMatch(BasicLibrary.Utilities.PhoneRegEx))
                    activity.ContactPhone = contactphone.ToString().IfBlank("").MaxChars(50);
                else
                    msgs.Add("Contact Phone must be a valid phone number", MessageType.UserError);
            }

            if (contactemail.IsNotEmpty())
                activity.ContactEmail = contactemail.ToString().IfBlank("").MaxChars(100);

            if (details.IsNotEmpty())
                activity.Details = details.ToString().IfBlank("").MaxChars(4000);

            if (description.IsNotEmpty())
                activity.Description = description.ToString().IfBlank("").MaxChars(1000);

            if (cost.IsNumber())
                activity.CostPerRegistrant = cost.ToString().ToDecimal();

            if (costdescription != null)
                activity.CostDescription = costdescription.ToString().IfBlank("").MaxChars(1000);

            if (allowtopaylater.IsNotEmpty() && allowtopaylater.ToString().IsBoolean())
            {
                if (account.CanProcessPayments(msgs, dc))
                    activity.AllowToPayLater = allowtopaylater.ToString().ToBoolean();
                else
                    activity.AllowToPayLater = true;
            }

            if (regmode.IsNumber())
                activity.RegistrationModeId = (RegistrationMode)regmode.ToString().ToInteger();

            if (leadertype.IsNumber())
            {
                var leaderType = (ActivityLeaderType)leadertype.ToString().ToInteger();

                if (leaderType == ActivityLeaderType.NotSet)
                    msgs.Add("Led By is required", MessageType.UserError);
                else
                    activity.LeaderTypeId = leaderType;
            }

            if (accountactivitygroup.IsNotEmpty())
                activity.AccountActivityGroupId = accountactivitygroup.ToString().ToInteger();

            if (msgs.HasNoErrors())
            {
                Database.ExecuteTransaction(
                    action: (SystemMessages amsgs, BasicLibrary.DataAccess.DatabaseConnection adc) =>
                    {
                        if (!activity.ID.HasValue)
                        {
                            if (contactname.IsEmpty())
                                activity.ContactName = account.GetAccountOwner(amsgs, adc).DisplayName;

                            if (contactemail.IsEmpty())
                                activity.ContactEmail = account.Email;

                            if (contactphone.IsEmpty())
                                activity.ContactPhone = account.Phone;
                        }

                        ActivityGroup activityGroup = null;

                        if (group.IsNotEmpty() && group.ToString().IsGuid())
                            activityGroup = ActivityGroup.LoadByUid(group.ToString().ToGuid(), amsgs, adc);

                        if (activityGroup.IsNotEmpty())
                        {
                            activity.ActivityGroupId = activityGroup.ID.Value;
                            activity.GradeLevelStart = activityGroup.MinGradeLevelId;
                            activity.GradeLevelEnd = activityGroup.MaxGradeLevelId;
                            activity.GenderRestriction = activityGroup.GenderRestriction;
                        }

                        amsgs.AddRange(activity.Save(adc));
                        
                        // create / associate form
                        if (amsgs.HasNoErrors() && form.IsNumber())
                            amsgs.AddRange(activity.SetupNewForm(form.ToString().ToInteger(), account, loggedInUserId, adc));

                        // skip if it is attached to a activity group
                        if (!activity.ActivityGroupId.HasValue)
                        {
                            //create registration period if not created yet and there is no activitygroup (activity groups have their own registration period)
                            if (amsgs.HasNoErrors() && registrationperiod.IsNumber())
                            {
                                amsgs.AddRange(activity.SetupRegistrationPeriods(
                                    registrationperiod.IfEmpty(""),
                                    registrationopendate.IfEmpty(""),
                                    registrationopentime.IfEmpty(""),
                                    registrationclosedate.IfEmpty(""),
                                    registrationclosetime.IfEmpty(""),
                                    account,
                                    loggedInUserId,
                                    adc));
                            }
                        }

                        //add creator to activity team if new activity
                        if (amsgs.HasNoErrors() && activityId.IsEmpty())
                            amsgs.AddRange(activity.SetupTeam(account, loggedInUserId, adc));

                        // setup schedule
                        if (amsgs.HasNoErrors() && (schedule.IsNotEmpty() || scheduledates.IsNotEmpty()))
                        {
                            amsgs.AddRange(activity.SetupSchedule(
                                account,
                                schedule.IfEmpty(""),
                                (List<DateRange>)scheduledates,
                                (List<DateTime>)scheduledatesClosed,
                                scheduledetails.IfEmpty(""),
                                repeatsweekly.IfEmpty(""),
                                startdate.IfEmpty(""),
                                enddate.IfEmpty(""),
                                dailystarttime.IfEmpty(""),
                                dailyendtime.IfEmpty(""),
                                isongoing.IfEmpty(""),
                                loggedInUserId,
                                adc));
                        }

                        //setup initial activityProperties
                        if (amsgs.HasNoErrors())
                        {
                            amsgs.AddRange(activity.SetupActivityProperties(
                                registrationthankyouemail.IfEmpty(null),
                                applicationthankyouemail.IfEmpty(null),
                                loggedInUserId,
                                adc));
                        }

                        if (amsgs.HasNoErrors() && instructor.IsNotEmpty())
                        {
                            amsgs.AddRange(activity.SetupInstructor(
                                account,
                                instructor.IfEmpty(""),
                                instructorfirstname.IfEmpty(""),
                                instructorlastname.IfEmpty(""),
                                instructoremail.IfEmpty(""),
                                instructorphone.IfEmpty(""),
                                loggedInUserId,
                                adc
                                ));
                        }

                        //setup location
                        if (amsgs.HasNoErrors() && (location.IsNotEmpty() || locationname.IsNotEmpty()))
                        {
                            var hostAccountId = ismylocation.IfEmpty("").ToBoolean() ? account.ID : new int?();

                            amsgs.AddRange(activity.SetupLocation(
                                location.IfEmpty(""),
                                locationname.IfEmpty(""),
                                address1.IfEmpty(""),
                                address2.IfEmpty(""),
                                city.IfEmpty(""),
                                state.IfEmpty(""),
                                postalcode.IfEmpty(""),
                                locationdetails.IfEmpty(""),
                                ismylocation.IfEmpty(""),
                                locationtype.IfEmpty(""),
                                loggedInUserId,
                                adc
                                ));

                            if (!account.DefaultLocationId.HasValue)
                                msgs.AddRange(account.SetDefaultLocation(activity.LocationId.Value, loggedInUserId, dc));
                        }
                       
                        //setup provider
                        if (amsgs.HasNoErrors() && (provider.IsNotEmpty() || providername.IsNotEmpty()))
                        {
                            amsgs.AddRange(activity.SetupProvider(
                                account,
                                provider.IfEmpty(""),
                                providername.IfEmpty(""),
                                providerfirstname.IfEmpty(""),
                                providerlastname.IfEmpty(""),
                                provideremail.IfEmpty(""),
                                providerphone.IfEmpty(""),
                                loggedInUserId,
                                adc
                                ));
                        }

                        //setup SubscriptionPlan
                        if (amsgs.HasNoErrors() && hassubscriptionplan != null && hassubscriptionplan.ToString().IsBoolean())
                        {
                            amsgs.AddRange(activity.SetupSubscriptionPlan(
                                account,
                                hassubscriptionplan.IfEmpty(""),
                                subscriptionplancost.IfEmpty(""),
                                subscriptionplanperiod.IfEmpty(""),
                                loggedInUserId,
                                adc
                            ));
                        }

                        //setup PaymentPlan
                        if (amsgs.HasNoErrors() && haspaymentplan != null && haspaymentplan.ToString().IsBoolean())
                        {
                            amsgs.AddRange(activity.SetupPaymentPlan(
                                account,
                                haspaymentplan.IfEmpty(""),
                                paymentplancost.IfEmpty(""),
                                paymentplanperiod.IfEmpty(""),
                                paymentplanperiodnumber.IfEmpty(""),
                                loggedInUserId,
                                adc
                            ));
                        }
                    },
                    onSuccess: (SystemMessages smsgs) => msgs.AddRange(smsgs),
                    onFailure: (SystemMessages fmsgs) => msgs.AddRange(fmsgs),
                    onException: (SystemMessages emsgs) => Helpers.Utilities.SystemErrorLog(emsgs),
                    dc: dc
                );
            }

            return activity;
        }

        private static SystemMessages SetupInstructor(this Activity activity, Account account, string instructor, string instructorfirstname, string instructorlastname, string instructoremail, string instructorphone, int loggedInUserId, DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();

            if (instructor.IsNumber())
            {
                //new instructor
                if (instructor.ToInteger() == 0)
                {
                    if (instructorfirstname.IsEmpty())
                        msgs.Add("Instructor First Name is required", MessageType.UserError);

                    if (instructorlastname.IsEmpty())
                        msgs.Add("Instructor Last Name is required", MessageType.UserError);

                    if (instructoremail.IsEmpty())
                        msgs.Add("Instructor Email is required", MessageType.UserError);
                    else if (!instructoremail.IsRegExMatch(Utilities.EmailRegEx))
                        msgs.Add("Instructor Email must be a valid email", MessageType.UserError);

                    if (instructorphone.IsNotEmpty() && !instructorphone.IsRegExMatch(Utilities.PhoneRegEx))
                        msgs.Add("Instructor Phone must be a valid phone number", MessageType.UserError);

                    if (msgs.HasNoErrors())
                    {
                        // add user
                        var user = account.InviteUser(
                            firstName: instructorfirstname,
                            lastName: instructorlastname,
                            email: instructoremail,
                            phone: instructorphone,
                            message: "",
                            userType: UserType.Instructor,
                            loggedInUser: WebUser.Load(loggedInUserId, msgs, dc),
                            msgs: msgs,
                            dc: dc);

                        // add to team
                        // should they be a leader by default? For right now, no.
                        activity.AddTeamMember(user.ID.Value, false, loggedInUserId, msgs, dc);

                        activity.InstructorId = user.ID;
                    }
                }
                else
                {
                    activity.InstructorId = instructor.ToInteger();
                }

                msgs.AddRange(activity.Save(dc));
            }

            return msgs;
        }

        private static SystemMessages SetupSchedule(this Activity activity, Account account, string schedule, List<DateRange> scheduledates, List<DateTime> scheduledatesclosed, string scheduledetails, string repeatsweekly, string startdate, string enddate, string dailystarttime, string dailyendtime, string isongoing, int loggedInUserId, DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();

            // this.ScheduleRequiresAllDays = true;

            var newSchedule = ActivityScheduleService.SetupSchedule(
                activity.AccountId.Value,
                schedule.IfEmpty("0").ToInteger(),
                $"{activity.Name} - Schedule",
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
                activity.ActivityGroupId,
                loggedInUserId,
                msgs,
                dc);            

            activity.ActivityScheduleId = newSchedule.ID;

            msgs.AddRange(activity.Save(dc));

            return msgs;
        }

        private static SystemMessages SetupSubscriptionPlan(this Activity activity, Account account, string hassubscriptionplan, string subscriptionplancost, string subscriptionplanperiod, int loggedInUserId, DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();

            // if the subscription plan option(UI) is activated
            if (hassubscriptionplan.ToBoolean())
            {
                // if there is no subscription plan active in the DB
                // let's create a new one
                if (!activity.HasSubscriptionPlan(msgs, dc))
                {
                    var subscriptionPlan = Services.SubscriptionPlanService.CreateNewSubscriptionPlan(
                            account,
                            ParticipationObjectType.Activity,
                            activity.ID.Value,
                            subscriptionplancost.ToDecimal(),
                            (BillingPeriodType)subscriptionplanperiod.ToInteger(),
                            true,
                            loggedInUserId,
                            msgs,
                            dc
                        );
                }
                else
                {
                    var subscriptionPlan = activity.GetSubscriptionPlan(msgs, dc);

                    // if the plan is purchased and begin edited
                    if (subscriptionPlan.IsPurchased(msgs, dc) && (subscriptionPlan.CostPerPeriod != subscriptionplancost.ToDecimal()))
                    {
                        // first: desactivate the current plan
                        msgs.AddRange(subscriptionPlan.Deactivate(loggedInUserId, dc));

                        // second: create a new plan
                        var newSubscriptionPlan = Services.SubscriptionPlanService.CreateNewSubscriptionPlan(
                                account,
                                ParticipationObjectType.Activity,
                                activity.ID.Value,
                                subscriptionplancost.ToDecimal(),
                                (BillingPeriodType)subscriptionplanperiod.ToInteger(),
                                true,
                                loggedInUserId,
                                msgs,
                                dc
                            );
                    }
                    else
                    {
                        // update the current plan
                        subscriptionPlan.LastModifiedBy = loggedInUserId;
                        subscriptionPlan.LastModifiedDate = DateTime.UtcNow;
                        subscriptionPlan.CostPerPeriod = Convert.ToDecimal(subscriptionplancost);
                        subscriptionPlan.BillingPeriodTypeId = (BillingPeriodType)Convert.ToInt32(subscriptionplanperiod);
                        msgs.AddRange(subscriptionPlan.SaveWithStripe(dc));
                    }
                }

            }
            else if (activity.HasSubscriptionPlan())
            {
                var subscriptionPlan = activity.GetSubscriptionPlan(msgs, dc);
                msgs.AddRange(subscriptionPlan.Deactivate(loggedInUserId, dc));
            }

            return msgs;
        }

        private static SystemMessages SetupPaymentPlan(this Activity activity, Account account, string haspaymentplan, string paymentplancost, string paymentplanperiod, string paymentplanperiodnumber, int loggedInUserId, DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();

            // if the payment plan option(UI) is activated
            if (haspaymentplan.ToBoolean())
            {
                // if there is no payment plan active in the DB
                // let's create a new one
                if (!activity.HasPaymentPlan(msgs, dc))
                {
                    var paymentPlan = SubscriptionPlanService.CreateNewPaymentPlan(
                        account,
                        ParticipationObjectType.Activity,
                        activity.ID.Value,
                        paymentplancost.ToDecimal(),
                        (BillingPeriodType)paymentplanperiod.ToInteger(),
                        paymentplanperiodnumber.ToInteger(),
                        loggedInUserId,
                        msgs,
                        dc);
                }
                else
                {
                    var paymentPlan = activity.GetPaymentPlan(msgs, dc);

                    // if the plan is purchased and begin edited
                    if (paymentPlan.IsPurchased(msgs, dc) && (paymentPlan.CostPerPeriod != paymentplancost.ToDecimal()))
                    {
                        // first: desactivate the current plan
                        msgs.AddRange(paymentPlan.Deactivate(loggedInUserId, dc));

                        // second: create a new plan
                        var newPaymentPlan = Services.SubscriptionPlanService.CreateNewPaymentPlan(
                                account,
                                ParticipationObjectType.Activity,
                                activity.ID.Value,
                                paymentplancost.ToDecimal(),
                                (BillingPeriodType)paymentplanperiod.ToInteger(),
                                paymentplanperiodnumber.ToInteger(),
                                loggedInUserId,
                                msgs,
                                dc
                            );
                    }
                    else
                    {
                        // update the current plan
                        paymentPlan.LastModifiedBy = loggedInUserId;
                        paymentPlan.LastModifiedDate = DateTime.UtcNow;
                        paymentPlan.CostPerPeriod = paymentplancost.ToDecimal();
                        paymentPlan.BillingPeriodTypeId = (BillingPeriodType)paymentplanperiod.ToInteger();
                        paymentPlan.TotalPeriods = paymentplanperiodnumber.ToInteger();
                        msgs.AddRange(paymentPlan.SaveWithStripe(dc));
                    }
                }

            }
            else if (activity.HasPaymentPlan(msgs, dc))
            {
                var paymentPlan = activity.GetPaymentPlan(msgs, dc);
                msgs.AddRange(paymentPlan.Deactivate(loggedInUserId, dc));
            }

            return msgs;
        }

        private static SystemMessages SetupProvider(this Activity activity, Account activityAccount, string provider, string providername, string providerfirstname, string providerlastname, string provideremail, string providerphone, int loggedInUserId, DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();
            var loggedInUser = new WebUser { ID = loggedInUserId };

            Account providerAccount = null;

            // provider already exists
            if (provider.IsNumber() && provider.ToInteger() > 0)
                providerAccount = Account.Load(provider.ToInteger(), msgs, dc);
            else
            {
                if (providername.IsEmpty())
                    msgs.Add("Provider Organization or Program is required", MessageType.UserError);

                if (providerfirstname.IsEmpty())
                    msgs.Add("Provider First Name is required", MessageType.UserError);

                if (providerlastname.IsEmpty())
                    msgs.Add("Provider Last Name is required", MessageType.UserError);

                if (!provideremail.IsEmail())
                    msgs.Add("Provider Email is required", MessageType.UserError);

                if (providerphone.IsNotEmpty() && !providerphone.IsPhone())
                    msgs.Add("Please enter a valid phone number", MessageType.UserError);

                if (msgs.HasNoErrors())
                {
                    providerAccount = AccountService.CreateNew(
                         orgName: providername,
                         email: provideremail,
                         phone: providerphone,
                         isActive: false,
                         accountType: AccountType.ActivityProvider,
                         accountOwner: null,
                         packageId: null,
                         packageBillingPeriod: null,
                         timeZone: null,
                         loggedInUser: loggedInUser,
                         isAfterSchool: true,
                         createWithStripe: false,
                         msgs: msgs,
                         dc: dc);

                    providerAccount.AddUser(
                        firstName: providerfirstname,
                        lastName: providerlastname,
                        email: provideremail,
                        phone: providerphone,
                        userType: UserType.Owner,
                        loggedInUser: loggedInUser,
                        msgs: msgs,
                        dc: dc);

                    providerAccount.SetDefaultAccountPackage(loggedInUser, msgs, dc);
                }
            }

            if (msgs.HasNoErrors())
            {
                // connect the accounts
                msgs.AddRange(activityAccount.AddVisitorAccount(
                        visitorAccount: providerAccount,
                        hostPermissionType: HostPermissionType.ApproveOnly,
                        approvalStatus: ApprovalStatus.Approved,
                        loggedInUser: loggedInUser,
                        dc: dc));

                activity.ProviderAccountId = providerAccount.ID;
                msgs.AddRange(activity.Save(dc));
            }

            return msgs;
        }

        private static SystemMessages SetupLocation(this Activity activity, string location, string locationname, string address1, string address2, string city, string state, string postalcode, string locationdetails, string ismylocation, string locationtype, int loggedInUserId, DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();

            // just switching locations
            if (location.IsNumber())
            {
                // make sure we setup a relationship
                var actualLocation = Location.Load(location.ToInteger(), msgs, dc);

                // don't need to ask permission for your own location
                if (actualLocation.HostAccountId.HasValue && actualLocation.HostAccountId != activity.AccountId)
                {
                    var locationOwner = Account.Load(actualLocation.HostAccountId.Value, msgs, dc);
                    var visitorAccount = activity.GetAccount(msgs, dc);

                    var relationship = Services.AccountRelationshipService.GetOrCreateNew(
                       hostAccount: locationOwner,
                       visitorAccount: visitorAccount,
                       approvalStatus: ApprovalStatus.Requested,
                       hostPermissionType: HostPermissionType.ApproveOnly,
                       loggedInUser: new WebUser { ID = loggedInUserId },
                       msgs: msgs,
                       dc: dc);

                    var relationshipLocation = Services.AccountRelationshipLocationService.GetOrCreateNew(
                        accountRelationship: relationship,
                        hostAccount: locationOwner,
                        visitorAccount: visitorAccount,
                        location: actualLocation,
                        loggedInUser: new WebUser { ID = loggedInUserId },
                        msgs: msgs,
                        dc: dc);
                }

                activity.LocationId = location.ToString().ToInteger();
            }
            // they entered a new location possibly? 
            // otherwise somehow the UI allowed them to enter a location that already exists, with the exact same location name
            else
            {
                if (address1.IsEmpty())
                    msgs.Add("Address 1 is required", MessageType.UserError);
                if (city.IsEmpty())
                    msgs.Add("City is required", MessageType.UserError);
                if (state.IsEmpty())
                    msgs.Add("State is required", MessageType.UserError);
                if (postalcode.IsEmpty())
                    msgs.Add("Postal Code is required", MessageType.UserError);

                if (msgs.HasErrors())
                    return msgs;

                var locationType = LocationType.NotSet;
                var hostAccountId = new int?();

                if (locationtype.IsNumber())
                    locationType = (LocationType)locationtype.ToInteger();

                if (ismylocation.IsBoolean() && ismylocation.ToBoolean())
                    hostAccountId = activity.AccountId;

                var activityLocation = Services.LocationService.CreateNewWithValidation(
                        locationname,
                        address1,
                        address2,
                        city,
                        state,
                        postalcode,
                        "US",
                        locationType,
                        hostAccountId,
                        new Account { ID = activity.AccountId },
                        new WebUser { ID = loggedInUserId },
                        msgs,
                        dc);

                activity.LocationId = activityLocation.ID;
            }

            if (locationdetails.IsNotEmpty())
                activity.LocationDetails = locationdetails.IfBlank("").MaxChars(4000);

            msgs.AddRange(activity.Save(dc));

            return msgs;
        }

        private static SystemMessages SetupActivityProperties(this Activity activity, string registrationthankyouemail, string applicationthankyouemail, int loggedInUserId, DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();

            if (applicationthankyouemail != null)
                activity.AddOrUpdateActivityProperty(
                    AccountPropertyType.ApplicationThankYouEmail,
                    applicationthankyouemail,
                    loggedInUserId,
                    msgs,
                    dc: dc);

            if (registrationthankyouemail != null)
                activity.AddOrUpdateActivityProperty(
                    AccountPropertyType.RegistrationThankYouEmail,
                    registrationthankyouemail,
                    loggedInUserId,
                    msgs,
                    dc: dc);

            return msgs;
        }

        private static SystemMessages SetupRegistrationPeriods(this Activity activity, string registrationperiod, string registrationopendate, string registrationopentime, string registrationclosedate, string registrationclosetime, Account account, int loggedInUserId, DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();

            if (!registrationopendate.IsDate())
                msgs.Add("Open Date must be a valid date", MessageType.UserError);

            if (!registrationclosedate.IsDate())
                msgs.Add("Close Date must be a valid date", MessageType.UserError);

            if (registrationopendate.ToDateTime() > registrationclosedate.ToDateTime())
                msgs.Add("Open date must be before the Close date", MessageType.UserError);

            if (msgs.HasErrors())
                return msgs;

            if (registrationperiod.ToInteger() == 0)
            {
                var periodname = RegistrationPeriod.GenerateUniqueName(account.ID.Value, $"{activity.Name} - Reg Window", dc: dc).MaxChars(50);

                activity.AddNewRegistrationPeriod(
                    periodname,
                    $"{registrationopendate} {registrationopentime}".ToDateTime(),
                    $"{registrationclosedate} {registrationclosetime}".ToDateTime(),
                    account.ID.Value,
                    loggedInUserId,
                    msgs,
                    dc);
            }
            else
            {
                var period = RegistrationPeriod.Load(registrationperiod.ToInteger(), dc: dc);

                //if user doesn't have feature then we're updaing the period in the background
                if (!account.HasFeature(FeatureType.ManageActivityGroups, dc: dc))
                {
                    period.StartDateTime = $"{registrationopendate} {registrationopentime}".ToDateTime();
                    period.EndDateTime = $"{registrationclosedate} {registrationclosetime}".ToDateTime();
                    period.LastModifiedDate = DateTime.UtcNow;
                    period.LastModifiedBy = loggedInUserId;
                    msgs.AddRange(period.Save(dc: dc));
                }
                msgs.AddRange(period.AddActivity(activity, true, loggedInUserId, dc: dc));
            }

            return msgs;
        }

        public static SystemMessages SetupTeam(this Activity activity, Account account, int loggedInUserId, DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();
            //validate the user is part of this account (not a global admin)
            var awu = AccountWebUser.LoadByAccountIdByWebUserId(activity.AccountId.Value, loggedInUserId, dc: dc);

            if (awu.IsNotEmpty())
                activity.AddTeamMember(loggedInUserId, true, loggedInUserId, msgs, dc: dc);

            //add account admins to activity
            account.GetAccountOwnersAndAdmins(dc: dc)
                .ForEach(user => activity.AddTeamMember(user.ID.Value, true, loggedInUserId, msgs, dc: dc));

            return msgs;
        }

        private static SystemMessages SetupNewForm(this Activity activity, int formId, Account account, int loggedInUserId, DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();
            var activeForm = activity.GetActiveActivityForm(dc: dc);

            //already has created a form, no need to copy it again
            if (activeForm.IsEmpty() || activeForm.FormId != formId)
            {
                var isPublished = false;

                // deactivating existing form
                if (activeForm.IsNotEmpty())
                {
                    //deactivate old form
                    isPublished = activeForm.IsPublished;
                    activeForm.IsActive = false;
                    msgs.AddRange(activeForm.Save(dc: dc));
                }

                Form newForm = null;
                if (formId == 0)
                {
                    //if creating a new form, copy from the default form
                    var defaultForm = account.GetDefaultForm(dc: dc);
                    newForm = defaultForm.Copy(loggedInUserId, account.ID.Value, null, msgs, dc: dc);
                    newForm.Name = (activity.Name + " Form").MaxChars(100, "...");
                    msgs.AddRange(newForm.Save(dc: dc));
                }
                else
                {
                    //get the already existing form
                    newForm = Form.Load(formId, dc: dc);
                }

                if (newForm.IsNotEmpty())
                {
                    //link form to activity
                    activity.AddFormAndActivitate(newForm, loggedInUserId, isPublished, msgs, dc: dc);
                }
            }

            return msgs;
        }

        public static bool IsInSession(this Activity activity, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var account = activity.GetAccount(msgs, dc);
            var now = account.ConvertToAccountTimeZone(DateTime.UtcNow);
            var instances = activity.GetSchedule(msgs, dc).GetInstances(msgs: msgs, dc: dc);
            return instances.First().StartDateTime <= now && now <= instances.Last().EndDateTime;
        }

        public static bool WillBeInSession(this Activity activity, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var account = activity.GetAccount(msgs, dc);
            var now = account.ConvertToAccountTimeZone(DateTime.UtcNow);
            var instances = activity.GetSchedule(msgs, dc).GetInstances(msgs: msgs, dc: dc);
            return now < instances.First().StartDateTime;
        }

        public static bool IsOutOfSession(this Activity activity, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var now = activity.GetAccount(msgs, dc).ConvertToAccountTimeZone(DateTime.UtcNow);
            var instances = activity.GetSchedule(msgs, dc).GetInstances(msgs: msgs, dc: dc);
            return now > instances.Last().StartDateTime;
        }

        public static bool CanAcceptPayments(this Activity activity, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return activity.GetAccount(msgs, dc)
                .GetActiveAccountPackage(msgs, dc)
                .HasFeature(FeatureType.AcceptPayments, msgs, dc);
        }

        public static PurchaseableObject GetPurchaseableObject(this Activity activity)
        {
            return PurchaseableObject.FromActivity(activity);
        }

        public static bool HasTeamMatches(this Activity activity, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return Models.Match.LoadByTeamId(activity.ID.Value, msgs, dc).IsNotEmpty();
        }

        public static bool HasNoTeamMatches(this Activity activity, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return Models.Match.LoadByTeamId(activity.ID.Value, msgs, dc).IsEmpty();
        }

        public static Attendance GetOrCreateNewAttendance(this Activity activity, int? attendanceId, WebUser loggedInUser, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (!attendanceId.HasValue)
                return activity.CreateNewAttendance(loggedInUser, msgs, dc);

            return Attendance.Load(attendanceId.Value, msgs, dc);
        }

        public static Attendance CreateNewAttendance(this Activity activity, WebUser loggedInUser, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            //create attendance records for this date and mark as absent by default (even if temporary)
            var attendance = new Attendance();

            Database.ExecuteTransaction
            (
                action: (SystemMessages amsgs, DatabaseConnection adc) =>
                {
                    activity.ClearEmptyAttendances(amsgs, adc);
                    attendance.InitAttendance(activity, loggedInUser, amsgs, adc);
                },
                onSuccess: (SystemMessages smsgs) =>
                {
                    msgs.AddRange(smsgs);
                    attendance = Attendance.Load(attendance.ID.Value);
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

            return attendance;
        }

        private static void ClearEmptyAttendances(this Activity activity, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            //check for existing incomplete attendance and delete so that we're not just keeping a bunch of blank records
            var attendanceList = Attendance.LoadByActivityIdByIsCompleted(activity.ID.Value, false, msgs, dc);
            if (attendanceList.Count > 0)
            {
                attendanceList.ForEach(attendanceItem =>
                {
                    attendanceItem
                        .GetAttendanceRegistrants(msgs, dc)
                        .ForEach(attendaceRegistrant => msgs.AddRange(attendaceRegistrant.Delete(dc)));

                    msgs.AddRange(attendanceItem.Delete(dc));
                });
            }
        }

        public static ActivityMessage AddMessage(this Activity activity, string subject, string body, List<int> registrantIds, int loggedInUserId, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();
            ActivityMessage am = null;

            Database.ExecuteTransaction(
                action: (SystemMessages amsgs, BasicLibrary.DataAccess.DatabaseConnection adc) =>
                {
                    am = new ActivityMessage();
                    am.ActivityId = activity.ID;
                    am.AccountId = activity.AccountId;
                    am.Subject = subject.MaxChars(150);
                    am.Body = body;
                    am.CreateDate = DateTime.UtcNow;
                    am.CreatedBy = loggedInUserId;
                    amsgs.AddRange(am.Save(adc));

                    registrantIds.ForEach(registrantId => am.AddRegistrant(registrantId, loggedInUserId, amsgs, adc));
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

            return am;
        }

        public static List<DayOfWeekASL> GetDaysOfTheWeek(this Activity activity, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (activity.GetSchedule(msgs: msgs, dc: dc)?.GetDaysOfTheWeek(msgs, dc).Any() ?? false)
                return activity.GetSchedule(msgs: msgs, dc: dc)?.GetDaysOfTheWeek();

            return new List<DayOfWeekASL>();
        }

        public static int GetRegistrantCount(this Activity activity, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (activity.ID.HasValue)
                return RegistrantActivity.GetSeatCountByParticipationObject(
                    activity.ID.Value,
                    ParticipationObjectType.Activity,
                    msgs,
                    dc);

            return 0;
        }

        public static int GetRegistrantCountByAssignmentType(this Activity activity, RegistrationAssignmentType assignmentType, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (activity.ID.HasValue)
                return RegistrantActivity.GetSeatCountByAssignmentType(
                    assignmentType: assignmentType,
                    participationObjectId: activity.ID,
                    participationObjectType: ParticipationObjectType.Activity,
                    msgs: msgs,
                    dc: dc);

            return 0;
        }

        public static int GetRegistrantCountByAssignmentTypes(this Activity activity, List<RegistrationAssignmentType> assignmentTypes, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (activity.ID.HasValue)
                return RegistrantActivity.GetSeatCountByAssignmentTypes(
                    assignmentTypes: assignmentTypes,
                    participationObjectId: activity.ID,
                    participationObjectType: ParticipationObjectType.Activity,
                    msgs: msgs,
                    dc: dc);

            return 0;
        }

        public static List<Registrant> GetRegistrants(this Activity activity, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return activity.GetRegistrantActivities(msgs, dc).Select(x => x.ToRegistrant()).ToList();
        }

        public static List<RegistrantActivity> GetRegistrantActivities(this Activity activity, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return activity.GetRegistrantActivities(false, msgs, dc);
        }

        public static List<RegistrantActivity> GetRegistrantActivities(this Activity activity, bool includeInvited, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var ras = new List<RegistrantActivity>();

            if (includeInvited)
                ras = RegistrantActivity.LoadByParticipationObject(activity.ID.Value, ParticipationObjectType.Activity, msgs, dc).ToList();
            else
                ras = RegistrantActivity.LoadByParticipationObject(activity.ID.Value, ParticipationObjectType.Activity, msgs, dc)
                    .Where(x =>
                        x.AssignmentTypeId != RegistrationAssignmentType.Invited &&
                        x.AssignmentTypeId != RegistrationAssignmentType.Declined
                    ).ToList();

            return ras;
        }

        public static RegistrationPeriod AddNewRegistrationPeriod(this Activity activity, string name, DateTime startDateTime, DateTime endDateTime, int accountId, int loggedInUserId, SystemMessages msgs, DatabaseConnection dc = null)
        {
            var period = new RegistrationPeriod();
            period.StartDateTime = startDateTime;
            period.EndDateTime = endDateTime;
            period.Name = name;
            period.AccountId = accountId;
            period.CreateDate = period.LastModifiedDate = DateTime.UtcNow;
            period.CreatedBy = period.LastModifiedBy = loggedInUserId;
            msgs.AddRange(period.Save(dc: dc));

            msgs.AddRange(period.AddActivity(activity, true, loggedInUserId, dc: dc));

            return period;
        }

        public static ActivityWebUser AddTeamMember(this Activity activity, int webUserId, bool isLeader, int loggedInUserId, SystemMessages msgs, DatabaseConnection dc = null)
        {
            var teammember = new ActivityWebUser
            {
                WebUserId = webUserId,
                ActivityId = activity.ID,
                IsLeader = isLeader,
                CreateDate = DateTime.UtcNow,
                CreatedBy = loggedInUserId,
                LastModifiedDate = DateTime.UtcNow,
                LastModifiedBy = loggedInUserId
            };
            msgs.AddRange(teammember.Save(dc));

            return teammember;
        }

        public static List<ActivityWebUser> GetTeamMembers(this Activity activity, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return ActivityWebUser.LoadByActivityId(activity.ID.Value, msgs, dc);
        }

        public static List<Form> GetForms(this Activity activity, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return ActivityForm.LoadByActivityId(activity.ID.Value, dc: dc).Select(x => x.GetForm()).ToList();
        }

        public static Form GetActiveForm(this Activity activity, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var af = activity.GetActiveActivityForm(msgs, dc);

            if (af.IsEmpty())
                return null;

            return af.GetForm(msgs, dc);
        }

        public static ActivityForm GetActiveActivityForm(this Activity activity, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (activity.ID.HasValue)
                return ActivityForm.LoadByActivityIdByIsActive(activity.ID.Value, true, msgs, dc);

            return null;
        }

        public static List<ActivityForm> GetActivityForms(this Activity activity, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (activity.ID.HasValue)
                return ActivityForm.LoadByActivityId(activity.ID.Value, msgs: msgs, dc: dc);

            return new List<ActivityForm>();
        }

        public static List<Attendance> GetAttendance(this Activity activity, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return Attendance.LoadByActivityId(activity.ID.Value, msgs: msgs, dc: dc);
        }

        public static List<Attendance> GetCompletedAttendance(this Activity activity, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return Attendance.LoadByActivityIdByIsCompleted(activity.ID.Value, true, msgs: msgs, dc: dc);
        }

        public static int GetAttendancePresentCount(this Activity activity, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return AttendanceRegistrant.GetPresentCountByActivityId(activity.ID.Value, msgs: msgs, dc: dc);
        }

        public static int GetAttendancePossibleCount(this Activity activity, SystemMessages msgs = null,  DatabaseConnection dc = null)
        {
            return AttendanceRegistrant.GetPossibleCountByActivityId(activity.ID.Value, msgs: msgs, dc: dc);
        }

        public static List<Attendance> GetAttendanceByDate(this Activity activity, DateTime date, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return Attendance.LoadByActivityIdByDate(activity.ID.Value, date, msgs: msgs, dc: dc);
        }

        public static List<ActivityRegistrationPeriod> GetActivityRegistrationPeriods(this Activity activity, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return ActivityRegistrationPeriod.LoadByActivityId(activity.ID.Value, msgs, dc);
        }

        public static List<RegistrationPeriod> GetRegistrationPeriods(this Activity activity, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return activity.GetActivityRegistrationPeriods(msgs, dc)
                .Select(x => x.ToRegistrationPeriod())
                .ToList();
        }

        public static List<RegistrationPeriod> GetUpcomingRegistrationPeriods(this Activity activity, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return activity.GetRegistrationPeriods(msgs, dc)
                .Where(x => x.StartDateTime.Value > DateTime.UtcNow)
                .OrderBy(x => x.StartDateTime.Value)
                .ToList();
        }

        public static RegistrationPeriod GetCurrentRegistrationPeriod(this Activity activity, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return activity.GetRegistrationPeriods(msgs, dc)
                .Where(x => x.StartDateTime.Value <= DateTime.UtcNow && x.EndDateTime.Value >= DateTime.UtcNow)
                .FirstOrDefault();
        }

        public static RegistrationPeriod GetLatestRegistrationPeriod(this Activity activity, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var period = activity.GetCurrentOrNextRegistrationPeriod(msgs, dc: dc);
            if (period.IsEmpty())
                period = activity.GetRegistrationPeriods(msgs, dc: dc).OrderByDescending(p => p.EndDateTime).FirstOrDefault();

            return period;
        }

        public static RegistrationPeriod GetCurrentOrNextRegistrationPeriod(this Activity activity, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var period = activity.GetCurrentRegistrationPeriod(msgs, dc);
            if (period.IsEmpty())
                return activity.GetUpcomingRegistrationPeriods(msgs, dc).FirstOrDefault();

            return period;
        }

        public static RegistrationPeriod GetActiveRegistrationPeriod(this Activity activity, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (activity.ID.HasValue)
            {
                var item = ActivityRegistrationPeriod.LoadByActivityIdByIsActive(activity.ID.Value, true, msgs, dc);
                if (item.IsNotEmpty())
                    return item.ToRegistrationPeriod(true, msgs, dc);
            }
            return null;
        }

        public static Account GetAccount(this Activity activity, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return Account.Load(activity.AccountId.Value, msgs, dc);
        }

        public static SystemMessages MarkAsDeleted(this Activity activity, int loggedInUserId, DatabaseConnection dc = null)
        {
            activity.IsDeleted = true;
            activity.LastModifiedDate = DateTime.UtcNow;
            activity.LastModifiedBy = loggedInUserId;
            return activity.Save(dc);
        }

        public static SystemMessages MoveToArchive(this Activity activity, int loggedInUserId, DatabaseConnection dc = null)
        {
            activity.IsArchived = true;
            activity.LastModifiedDate = DateTime.UtcNow;
            activity.LastModifiedBy = loggedInUserId;
            return activity.Save(dc);
        }

        public static SystemMessages MoveFromArchive(this Activity activity, int loggedInUserId, DatabaseConnection dc = null)
        {
            activity.IsArchived = false;
            activity.LastModifiedDate = DateTime.UtcNow;
            activity.LastModifiedBy = loggedInUserId;
            return activity.Save(dc);
        }

        public static AccountProperty AddOrUpdateActivityProperty(this Activity activity, AccountPropertyType type, string value, int loggedInUserId, SystemMessages msgs, DatabaseConnection dc = null)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            var ap = activity.GetActivityProperty(type, msgs, dc);

            if (ap.IsEmpty())
            {
                ap = new AccountProperty();
                ap.ActivityId = activity.ID.Value;
                ap.AccountPropertyTypeId = type;
                ap.CreateDate = DateTime.UtcNow;
                ap.CreatedBy = loggedInUserId;
            }

            ap.Value = value;
            ap.LastModifiedDate = DateTime.UtcNow;
            ap.LastModifiedBy = loggedInUserId;
            msgs.AddRange(ap.Save(dc: dc));

            return ap;
        }

        public static AccountProperty GetActivityProperty(this Activity activity, AccountPropertyType propertyType, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return activity.GetActivityProperties(msgs, dc)?.Find(x => x.AccountPropertyTypeId == propertyType);
        }

        public static List<GenderType> GetAllowedGenders(this Activity activity)
        {
            switch (activity.GenderRestriction)
            {
                case GenderRestrictionType.None:
                    return new List<GenderType>() { GenderType.Female, GenderType.Male };
                case GenderRestrictionType.MaleOnly:
                    return new List<GenderType>() { GenderType.Male };
                case GenderRestrictionType.FemaleOnly:
                    return new List<GenderType>() { GenderType.Female };
            }
            return new List<GenderType>();
        }


        /// <summary>
        /// gets the first scheduled date for the activity, also gets the start time
        /// </summary>
        /// <param name="dc"></param>
        /// <returns></returns>
        public static DateTime? GetFirstDate(this Activity activity, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return activity.GetSchedule(msgs: msgs, dc: dc).GetInstances(msgs: msgs, dc: dc).FirstOrDefault()?.StartDateTime;
        }

        /// <summary>
        /// gets the last scheduled date for the activity, also gets the start time
        /// </summary>
        /// <param name="dc"></param>
        /// <returns></returns>
        public static DateTime? GetLastDate(this Activity activity, DatabaseConnection dc = null)
        {
            return activity.GetSchedule(dc: dc).GetInstances(dc: dc).LastOrDefault()?.StartDateTime;
        }

        public static SystemMessages SendRegistrationSummaryReport(int activityId, string uiActivityLabel, DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();
            var activity = Activity.Load(activityId, msgs, dc);
            var registrants = activity.GetRegistrants(msgs, dc);

            string subject = $"{activity.Name} Registration Summary Report";
            string body = $"<p>{activity.ContactName},</p><p>Here's your registration summary report for <strong>{activity.Name}</strong>. You have <strong>{registrants.Count.ToString()}</strong> registrations.</p>";

            msgs.AddRange(Services.RegistrationService.EmailCSVReport(
                activity.GetAccount(msgs, dc).ID.Value,
                null,
                null,
                activityId.AsList(),
                ParticipationObjectType.Activity,
                "registrations",
                RegistrationReportType.SingleActivity,
                null,
                RegistrationStatus.NotSet,
                uiActivityLabel,
                activity.ContactEmail,
                subject,
                body));

            return msgs;
        }

        public static Account GetProvider(this Activity activity, SystemMessages msgs, DatabaseConnection dc = null)
        {
            if (activity.ProviderAccountId.HasValue)
                return Account.Load(activity.ProviderAccountId.Value, msgs, dc);

            return null;
        }

        public static void NotifyProviderOfNewActivity(int activityId, int loggedInUserId)
        {
            var msgs = new SystemMessages();
            var activity = Activity.Load(activityId, msgs);

            if (!activity.ProviderAccountId.HasValue)
                return;

            var provider = activity.GetProvider(msgs);
            var loggedInUser = WebUser.Load(loggedInUserId, msgs);            
            var hostAccount = activity.GetAccount(msgs);
            var providerOwner = provider.GetAccountOwner(msgs);

            if (provider.IsNotEmpty())
                msgs.AddRange(Helpers.EmailHelper.SendProviderNewActivityNotification(hostAccount, provider, providerOwner, activity, loggedInUser));
        }

        public static void Publish(this Activity activity, WebUser loggedInUser, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            msgs.AddRange(activity.SetIsPublished(true, loggedInUser.ID.Value, dc));
        }

        public static void Unpublish(this Activity activity, WebUser loggedInUser, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            msgs.AddRange(activity.SetIsPublished(false, loggedInUser.ID.Value, dc));
        }

        public static SystemMessages SetIsPublished(this Activity activity, bool isPublished, int loggedInUserId, DatabaseConnection dc = null)
        {
            activity.IsPublished = isPublished;
            activity.LastModifiedBy = loggedInUserId;
            activity.LastModifiedDate = DateTime.UtcNow;
            return activity.Save(dc);
        }

        public static ActivityGroup GetActivityGroup(this Activity activity, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (activity.ActivityGroupId.HasValue)
                return ActivityGroup.Load(activity.ActivityGroupId.Value, msgs, dc: dc);

            return null;
        }

        public static ActivitySchedule GetSchedule(this Activity activity, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (activity.ActivityScheduleId.HasValue)
                return ActivitySchedule.LoadWithInstances(activity.ActivityScheduleId.Value, msgs, dc);

            return null;
        }

        public static List<AccountProperty> GetActivityProperties(this Activity activity, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (activity.ID.HasValue)
                return AccountProperty.LoadByActivityId(activity.ID.Value, msgs, dc: dc);

            return new List<AccountProperty>();
        }

        public static List<ActivityMessage> GetMessages(this Activity activity, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return ActivityMessage.LoadByActivityId(activity.ID.Value, msgs, dc);
        }

        public static List<ActivityMessage> GetMessages(this Activity activity, int? start, int? length, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return ActivityMessage.LoadByActivityId(activity.ID.Value, start, length, msgs, dc);
        }

        public static int GetMessagesCount(this Activity activity, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return ActivityMessage.GetCount(
                criterias: new Hashtable() { { "ActivityId", activity.ID.Value } },
                msgs: msgs,
                dc: dc);
        }

        public static bool IsOpenUpcomingOrRecentlyClosed(this Activity activity, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var regPeriod = activity.GetActiveRegistrationPeriod(msgs, dc);

            if (regPeriod.IsEmpty())
                return false;

            var account = activity.GetAccount(msgs, dc);
            var now = DateTime.UtcNow;

            // currently open or upcoming
            if (regPeriod.EndDateTime.Value >= account.ConvertFromAccountTimeZone(now))
                return true;

            // recently closed
            if (regPeriod.EndDateTime.Value.AddWeeks(2) >= account.ConvertFromAccountTimeZone(now))
                return true;

            return false;
        }

        public static void RequestToPublish(this Activity activity, WebUser loggedInUser, SystemMessages msgs, DatabaseConnection dc = null)
        {
            var location = activity.GetLocation(msgs, dc);
            var locationOwner = location.GetHostAcount(msgs, dc);
            var accountOwner = locationOwner.GetAccountOwner(msgs, dc);
            msgs.AddRange(Helpers.EmailHelper.SendPublishRequestNotification(activity, locationOwner, accountOwner, loggedInUser));
        }
    }
}
