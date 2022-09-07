using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using AfterSchoolLibrary.Helpers;
using AfterSchoolLibrary.Models;
using BasicLibrary;
using BasicLibrary.DataAccess;
using SendGrid.Helpers.Mail;
using Email = AfterSchoolLibrary.Helpers.Email;
using Match = AfterSchoolLibrary.Models.Match;
using Utilities = AfterSchoolLibrary.Helpers.Utilities;

namespace AfterSchoolLibrary.Services
{
    public static class ActivityService
    {
        public static string GetHomePageUrl(this Activity activity, bool isAfterSchool = true,
            SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return BuildHomePageUrl(activity.GetAccount(msgs, dc).UniqueName, activity.ActivityTypeId,
                activity.UniqueName);
        }

        public static string GetShortHomePageUrl(this Activity activity, bool isAfterSchool = true,
            SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return BuildShortHomePageUrl(activity.GetAccount(msgs, dc).UniqueName, activity.ActivityTypeId,
                activity.UniqueName);
        }

        public static string BuildHomePageUrl(string accountUniqueName, ActivityType activityType,
            string activityUniqueName)
        {
            var activityTypeSlug = activityType != ActivityType.VolunteerOpportunity ? activityType.ToString().ToLower() : "opportunity";
            return $"{Utilities.BaseUrl()}/{accountUniqueName}/{activityTypeSlug}/{activityUniqueName?.ToLower()}";
        }

        public static string BuildShortHomePageUrl(string accountUniqueName, ActivityType activityType,
            string activityUniqueName)
        {
            return
                $"{Utilities.ShortBaseUrl()}/{accountUniqueName}/{activityType.ToString().ToLower()}/{activityUniqueName?.ToLower()}";
        }

        public static bool IsFull(this Activity activity, DatabaseConnection dc = null)
        {
            return activity.GetSeatsRemaining(dc: dc) == 0;
        }

        public static Location GetLocation(this Activity activity, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            if (activity.LocationId.HasValue)
                return Location.Load(activity.LocationId.Value, msgs, dc);

            return null;
        }

        public static void SetLocation(this Activity activity, Location location, WebUser loggedInUser,
            SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (activity.IsNotEmpty() && location.IsNotEmpty() && location.ID.HasValue)
            {
                activity.LocationId = location.ID;
                activity.LastModifiedDate = DateTime.UtcNow;
                activity.LastModifiedBy = loggedInUser.ID;
                msgs.AddRange(activity.Save(dc));
            }
        }

        public static ParticipationObject GetParticipationObject(this Activity activity, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            return new ParticipationObject(activity, msgs, dc);
        }

        public static string GenerateUniqueName(string name, int? activityId, int? accountId, SystemMessages msgs,
            DatabaseConnection dc = null)
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

        public static int GetSeatsTaken(this Activity activity, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            return activity.GetRegistrantCountByAssignmentTypes(
                Registration.ReservedStatuses,
                msgs,
                dc);
        }

        public static int GetWaitingListSeatsTaken(this Activity activity, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            return activity.GetRegistrantCountByAssignmentType(RegistrationAssignmentType.WaitingList, msgs, dc);
        }

        public static int GetSeatsRemaining(this Activity activity, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            var rtn = 0;
            if (activity.SeatsAvailable.HasValue)
                rtn = activity.SeatsAvailable.Value - activity.GetRegistrantCountByAssignmentTypes(
                    Registration.ReservedStatuses,
                    msgs,
                    dc);

            return Math.Max(0, rtn);
        }

        public static int GetSeatsRemainingForNextDate(this Activity activity, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            var schedule = activity.GetSchedule(msgs, dc);
            if (schedule.IsNotEmpty())
            {
                var dates = schedule.GetNextInstances(1, activity, msgs, dc);

                if (dates.Any())
                    return activity.GetSeatsRemainingByDate(dates.First(), msgs, dc);
            }

            return 0;
        }

        public static int GetSeatsRemainingByDate(this Activity activity, DateTime activityDate,
            SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (activity.CostFrequencyTypeId == ActivityCostFrequencyType.PerActivity)
                return activity.GetSeatsRemaining(msgs, dc);

            if (activity.HasSchedule)
            {
                var schedule = activity.GetSchedule(msgs, dc);
                var instance = schedule?.GetInSessionInstanceByDate(activityDate, msgs, dc);

                if (instance.IsNotEmpty())
                    return instance.GetSeatsRemaining(activity, msgs, dc);
            }

            return activity.SeatsAvailable.Value;
        }

        public static SubscriptionPlan GetSubscriptionPlan(this Activity activity, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            return SubscriptionPlan
                .LoadActiveByParticipationObject(activity.ID.Value, ParticipationObjectType.Activity, msgs, dc)
                .Where(plan => plan.PlanTypeId == SubscriptionPlanType.Subscription && plan.IsActive)
                .FirstOrDefault();
        }

        public static SubscriptionPlan GetPostPayPlan(this Activity activity, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            return SubscriptionPlan
                .LoadActiveByParticipationObject(activity.ID.Value, ParticipationObjectType.Activity, msgs, dc)
                .Where(plan => plan.PlanTypeId == SubscriptionPlanType.PostPayPlan && plan.IsActive)
                .FirstOrDefault();
        }

        public static SubscriptionPlan GetPaymentPlan(this Activity activity, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            return SubscriptionPlan
                .LoadActiveByParticipationObject(activity.ID.Value, ParticipationObjectType.Activity, msgs, dc)
                .Where(plan => plan.PlanTypeId == SubscriptionPlanType.PaymentPlan && plan.IsActive)
                .FirstOrDefault();
        }

        public static List<SubscriptionPlan> GetActivePaymentPlans(this Activity activity, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            return SubscriptionPlan.LoadByParticipationObject(activity.ID.Value, ParticipationObjectType.Activity,
                    SubscriptionPlanType.PaymentPlan, msgs, dc)
                .Where(plan => plan.IsActive)
                .ToList();
        }

        public static List<SubscriptionPlan> GetActiveSubscriptionPlans(this Activity activity,
            SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return SubscriptionPlan.LoadByParticipationObject(activity.ID.Value, ParticipationObjectType.Activity,
                    SubscriptionPlanType.Subscription, msgs, dc)
                .Where(plan => plan.IsActive)
                .ToList();
        }

        public static List<SubscriptionPlan> GetActivePostPayPlans(this Activity activity, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            return SubscriptionPlan.LoadByParticipationObject(activity.ID.Value, ParticipationObjectType.Activity,
                    SubscriptionPlanType.PostPayPlan, msgs, dc)
                .Where(plan => plan.IsActive)
                .ToList();
        }

        public static Activity CreateNew(string activityName, ActivityType activityType, int seatsAvailable, GenderRestrictionType genderRestriction,
            string contactName, string contactPhone, string contactEmail, decimal? costPerRegistrant, string costDescription, bool allowToPayLater,
            RegistrationMode registrationMode, int? accountId, int? scheduleId, int? locationId, string locationDetails, int? activityGroupId,
            int? minGradeLevelId, int? maxGradeLevelId, WebUser loggedInUser, SystemMessages msgs, DatabaseConnection dc = null)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            var activity = new Activity
            {
                Name = activityName,
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
                UniqueName = GenerateUniqueName(activityName, null, accountId, msgs, dc),
                LocationDetails = locationDetails,
            };

            msgs.AddRange(activity.Save(dc));

            return activity;
        }

        public static decimal GetTotalPaid(this Activity activity, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            return ActivityPayment
                .LoadByParticipationObject(activity.ID.Value, ParticipationObjectType.Activity, false, msgs, dc)
                .Sum(payment => payment.Amount.Value);
        }

        public static GradeLevel GetStartingGradeLevel(this Activity activity, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            return GradeLevel.Load(activity.GradeLevelStart.Value, msgs, dc);
        }

        public static GradeLevel GetEndingGradeLevel(this Activity activity, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            return GradeLevel.Load(activity.GradeLevelEnd.Value, msgs, dc);
        }

        public static RegistrantActivity InviteContact(this Activity activity, Registrant contact, int? periodId,
            int invitationSeats, int loggedInUserId, SystemMessages msgs, DatabaseConnection dc = null)
        {
            return contact.AddInviteToActivity(activity.ID.Value, loggedInUserId, activity.ActivityGroupId, periodId,
                msgs, dc);
        }

        public static SystemMessages InviteContacts(this Activity activity, List<Registrant> contacts,
            RegistrationPeriod period, int loggedInUserId, DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();

            contacts.ForEach(contact =>
            {
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

        public static SystemMessages EmailInvitedContacts(int activityId, int periodId, List<int> contactIds,
            string emailmessage, string inviteUrl, int loggedInUserId, DatabaseConnection dc = null)
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

                var acceptInviteText =
                    "To accept this invite and register, please click the following link: <a href=\"{0}\">{0}</a>";
                var declineInviteText =
                    "<p>To decline this invite, please click the following link: <a href=\"{0}\">{0}</a></p>";
                var userSignature = $"<p>Regards,<br />{loggedInUser.DisplayName}</p>";
                var declineUrl = "";

                body = "{0}{1}<p>{2} {3}</p>{4}{5}{6}";

                contacts.ForEach(contact =>
                {
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
                        fromEmail: new EmailAddress(
                            Email.AppNoReplyEmailAddress,
                            $"{account.Name} via {Utilities.AppName(account.IsAfterSchool)}"),
                        toEmail: new EmailAddress(contact.Email, contact.Name),
                        subject: subject,
                        body: string.Format(
                            body,
                            salutation,
                            emailmessage,
                            regOpenText,
                            acceptInviteTextWithLink,
                            declineInviteTextWithLink,
                            userSignature,
                            Email.GetAppSignature(account.IsAfterSchool, "invite_contacts")),
                        replyTo: new EmailAddress(account.Email, account.Name),
                        emailTemplateId: SendGridHelper.GetGenericTemplateId(account.IsAfterSchool)
                    );
                });
            }

            return msgs;
        }

        public static decimal GetTotalAmountPaid(this Activity activity, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            return ActivityPayment.GetTotalPaidByParticipationObject(activity.ID.Value,
                ParticipationObjectType.Activity, false, msgs, dc);
        }

        public static Activity CopyWithRegistrants(this Activity activity, string newName, bool useSameForm,
            int loggedInUserId, SystemMessages msgs, DatabaseConnection dc = null)
        {
            return activity.Copy(newName, useSameForm, true, true, true, loggedInUserId, msgs, dc);
        }

        public static Activity Copy(this Activity activity, string newName, bool useSameForm, int loggedInUserId,
            SystemMessages msgs, DatabaseConnection dc = null)
        {
            return activity.Copy(newName, useSameForm, false, true, true, loggedInUserId, msgs, dc);
        }

        public static Activity CopyWithoutSchedule(this Activity activity, string newName, bool useSameForm,
            int loggedInUserId, SystemMessages msgs, DatabaseConnection dc = null)
        {
            return activity.Copy(newName, useSameForm, false, true, false, loggedInUserId, msgs, dc);
        }

        public static Activity Copy(this Activity activity, string newName, bool useSameForm, bool includeSignUpPeriods,
            bool includeSchedule, int loggedInUserId, SystemMessages msgs, DatabaseConnection dc = null)
        {
            return activity.Copy(newName, useSameForm, false, includeSignUpPeriods, includeSchedule, loggedInUserId,
                msgs, dc);
        }

        internal static Activity Copy(this Activity activity, string newName, bool useSameForm, bool includeRegistrants,
            bool includeSignUpPeriods, bool includeSchedule, int loggedInUserId, SystemMessages msgs,
            DatabaseConnection dc = null)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            Activity copy = null;

            Database.ExecuteTransaction
            (
                action: (SystemMessages amsgs, DatabaseConnection adc) =>
                {
                    var account = activity.GetAccount(amsgs, adc);

                    newName = newName.MaxChars(75);

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
                            formToUse = account.GetDefaultStudentForm(msgs, adc);

                        copy.AddFormAndActivate(formToUse, loggedInUserId, false, amsgs, dc: adc);
                    }
                    //copy form completely
                    else
                    {
                        Form formToCopy = null;
                        if (useSameForm)
                            formToCopy = activity.GetActiveForm(amsgs, adc);
                        else
                            formToCopy = account.GetDefaultStudentForm(amsgs, adc);

                        var newForm = formToCopy.Copy(loggedInUserId, copy.AccountId.Value, FormType.NotSet, null,
                            amsgs, dc: adc);
                        copy.AddFormAndActivate(newForm, loggedInUserId, false, amsgs, dc: adc);
                    }

                    // copy team and permissions
                    var newActivityId = copy.ID!.Value;
                    activity.GetTeamMembers(amsgs, adc)
                        .ForEach(member => member.Copy(newActivityId, loggedInUserId, amsgs, adc));

                    if (!WebUser.Load(loggedInUserId, amsgs, adc).HasAccessToActivity(copy, amsgs, adc))
                    {
                        copy.AddTeamMember(loggedInUserId, true, loggedInUserId, amsgs, adc);
                    }

                    if (includeSignUpPeriods)
                    {
                        var activePeriod = activity.GetActiveRegistrationPeriod(amsgs, adc);
                        if (activePeriod.IsNotEmpty())
                        {
                            //copy registration period if doesn't have activity groups
                            if (!account.HasFeature(FeatureType.ManageActivityGroups, amsgs, adc))
                            {
                                var periodName = RegistrationPeriod.GenerateUniqueName(account.ID.Value,
                                    $"{copy.Name} - Reg Window".MaxChars(50), amsgs, adc);
                                var newPeriod = activePeriod.Copy(periodName, loggedInUserId, amsgs, dc: adc);
                                amsgs.AddRange(newPeriod.AddActivity(copy, true, loggedInUserId, adc));
                            }
                            else
                                amsgs.AddRange(activePeriod.AddActivity(copy, true, loggedInUserId, adc));
                        }
                    }

                    activity.GetActivityProperties(msgs, adc).ForEach(property =>
                    {
                        var newProperty = new AccountProperty
                        {
                            ActivityId = copy.ID,
                            Value = property.Value,
                            AccountPropertyTypeId = property.AccountPropertyTypeId,
                            LastModifiedDate = DateTime.UtcNow,
                            CreateDate = DateTime.UtcNow,
                            LastModifiedBy = loggedInUserId,
                            CreatedBy = loggedInUserId,
                        };
                        amsgs.AddRange(newProperty.Save(adc));
                    });

                    var details = activity.GetActivityDetails(msgs, adc);
                    if (details.IsNotEmpty())
                    {
                        var detailsCopy = new ActivityDetails
                        {
                            ActivityId = newActivityId,
                            Value = details.Value,
                            CreateDate = DateTime.UtcNow,
                            CreatedBy = loggedInUserId,
                            LastModifiedDate = DateTime.UtcNow,
                            LastModifiedBy = loggedInUserId,
                        };
                        amsgs.AddRange(detailsCopy.Save(adc));
                    }

                    // NOTE: copy schedule?
                    if (includeSchedule && activity.ActivityScheduleId.HasValue)
                    {
                        copy.ActivityScheduleId = activity.GetSchedule(amsgs, adc)
                            .CopyAndSave(loggedInUserId, amsgs, adc)
                            .ID;
                        amsgs.AddRange(copy.Save(adc));
                    }

                    var oldSubPlan = activity.GetSubscriptionPlan(amsgs, adc);
                    if (oldSubPlan.IsNotEmpty())
                        oldSubPlan.Copy(newActivityId, loggedInUserId, amsgs, adc);

                    var oldPaymentPlan = activity.GetPaymentPlan(amsgs, adc);
                    if (oldPaymentPlan.IsNotEmpty())
                        oldPaymentPlan.Copy(newActivityId, loggedInUserId, amsgs, adc);

                    var oldPostPayPlan = activity.GetPostPayPlan(amsgs, adc);
                    if (oldPostPayPlan.IsNotEmpty())
                        oldPostPayPlan.Copy(newActivityId, loggedInUserId, amsgs, adc);
                },
                onSuccess: smsgs => msgs.AddRange(smsgs),
                onFailure: fmsgs => msgs.AddRange(fmsgs),
                onException: emsgs => Utilities.SystemErrorLog(emsgs),
                dc: dc
            );

            return copy;
        }

        private static Activity CopyFields(this Activity activity)
        {
            return new Activity
            {
                AccountId = activity.AccountId,
                ProviderAccountId = activity.ProviderAccountId,
                LeaderTypeId = activity.LeaderTypeId,
                ContactEmail = activity.ContactEmail,
                ContactPhone = activity.ContactPhone,
                ContactName = activity.ContactName,
                Description = activity.Description,
                LocationDetails = activity.LocationDetails,
                SeatsAvailable = activity.SeatsAvailable,
                Uid = Guid.NewGuid(),
                CostPerRegistrant = activity.CostPerRegistrant,
                CostDescription = activity.CostDescription,
                GenderRestriction = activity.GenderRestriction,
                GradeLevelStart = activity.GradeLevelStart,
                GradeLevelEnd = activity.GradeLevelEnd,
                AgeStart = activity.AgeStart,
                AgeEnd = activity.AgeEnd,
                AllowToPayLater = activity.AllowToPayLater,
                RegistrationModeId = activity.RegistrationModeId,
                ActivityGroupId = activity.ActivityGroupId,
                ActivityTypeId = activity.ActivityTypeId,
                LocationId = activity.LocationId,
                RequiresParentalConsent = activity.RequiresParentalConsent,
                IsStudentAllowedToRegister = activity.IsStudentAllowedToRegister,
                InstructorId = activity.InstructorId,
                IsInviteOnly = activity.IsInviteOnly,
                ActivityLocationTypeId = activity.ActivityLocationTypeId,
                MinSeatsRequired = activity.MinSeatsRequired,
                HidePreciseLocation = activity.HidePreciseLocation,
                InterestFormId = activity.InterestFormId,
                IsConvenienceFeeTakenOutOfCost = activity.IsConvenienceFeeTakenOutOfCost,
                CostFrequencyTypeId = activity.CostFrequencyTypeId,
                RegistrationCutoffHours = activity.RegistrationCutoffHours,
            };
        }

        public static ActivityForm AddFormAndActivate(this Activity activity, Form form, int loggedInUserId,
            bool isPublished = false, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            //deactivate all other ones
            activity.GetActivityForms(msgs, dc).ForEach(item =>
            {
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

        public static Activity EditExisting(int loggedInUserId, Account account, string activityId,
            Dictionary<string, object> values, SystemMessages msgs = null)
        {
            return SaveActivity(loggedInUserId, account, activityId, values, msgs);
        }

        public static Activity CreateNew(int loggedInUserId, Account account, Dictionary<string, object> values,
            SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return SaveActivity(loggedInUserId, account, null, values, msgs, dc: dc);
        }

        private static Activity SaveActivity(int loggedInUserId, Account account, string activityId,
            Dictionary<string, object> values, SystemMessages msgs, DatabaseConnection dc = null)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            var name = values.GetValueOrDefault("name");
            var seatsAvailable = values.GetValueOrDefault("seatsavailable");
            var registrationPeriod = values.GetValueOrDefault("registrationperiod");
            var registrationOpenDate = values.GetValueOrDefault("registrationopendate");
            var registrationOpenTime = values.GetValueOrDefault("registrationopentime");
            var registrationCloseDate = values.GetValueOrDefault("registrationclosedate");
            var registrationCloseTime = values.GetValueOrDefault("registrationclosetime");
            var leaderType = values.GetValueOrDefault("leadertype");
            var contactName = values.GetValueOrDefault("contactname");
            var contactPhone = values.GetValueOrDefault("contactphone");
            var contactEmail = values.GetValueOrDefault("contactemail");
            var instructor = values.GetValueOrDefault("instructor");
            var instructorFirstName = values.GetValueOrDefault("instructorfirstname");
            var instructorLastName = values.GetValueOrDefault("instructorlastname");
            var instructorEmail = values.GetValueOrDefault("instructoremail");
            var instructorPhone = values.GetValueOrDefault("instructorphone");
            var details = values.GetValueOrDefault("details");
            var location = values.GetValueOrDefault("location");
            var locationName = values.GetValueOrDefault("locationname");
            var locationDetails = values.GetValueOrDefault("locationdetails");
            var scheduleDetails = values.GetValueOrDefault("scheduledetails");
            var address1 = values.GetValueOrDefault("address1");
            var streetNumber = values.GetValueOrDefault("streetnumber");
            var streetName = values.GetValueOrDefault("streetname");
            var address2 = values.GetValueOrDefault("address2");
            var neighborhood = values.GetValueOrDefault("neighborhood");
            var city = values.GetValueOrDefault("city");
            var county = values.GetValueOrDefault("county");
            var state = values.GetValueOrDefault("state");
            var postalCode = values.GetValueOrDefault("postalcode");
            var country = values.GetValueOrDefault("country");
            var placeId = values.GetValueOrDefault("placeid");
            var latitude = values.GetValueOrDefault("latitude");
            var longitude = values.GetValueOrDefault("longitude");
            var isMyLocation = values.GetValueOrDefault("mylocation");
            var locationType = values.GetValueOrDefault("locationtype");
            var activityLocationType = values.GetValueOrDefault("activitylocationtype");
            var cost = values.GetValueOrDefault("cost");
            var costDescription = values.GetValueOrDefault("costdescription");
            var allowToPayLater = values.GetValueOrDefault("allowtopaylater");
            var form = values.GetValueOrDefault("form");
            var requiresParentalConsent = values.GetValueOrDefault("requiresparentalconsent");
            var isStudentAllowed = values.GetValueOrDefault("isStudentAllowed");
            var ageStart = values.GetValueOrDefault("agestart");
            var ageEnd = values.GetValueOrDefault("ageend");
            var gradeStart = values.GetValueOrDefault("gradestart");
            var gradeEnd = values.GetValueOrDefault("gradeend");
            var genderRestrictions = values.GetValueOrDefault("genderrestrictions");
            var registrationMode = values.GetValueOrDefault("regmode");
            var days = values.GetValueOrDefault("days");
            var group = values.GetValueOrDefault("group");
            var registrationThankYouEmail = values.GetValueOrDefault("registrationthankyouemail");
            var applicationThankYouEmail = values.GetValueOrDefault("applicationthankyouemail");
            var repeatsWeekly = values.GetValueOrDefault("repeatsweekly");
            var isOngoing = values.GetValueOrDefault("isongoing");
            var startDate = values.GetValueOrDefault("startdate");
            var endDate = values.GetValueOrDefault("enddate");
            var dailyStartTime = values.GetValueOrDefault("dailystarttime");
            var dailyEndTime = values.GetValueOrDefault("dailyendtime");
            var scheduleDates = values.GetValueOrDefault("scheduledates");
            var scheduleDatesClosed = values.GetValueOrDefault("scheduledatesclosed");
            var schedule = values.GetValueOrDefault("schedule");
            var description = values.GetValueOrDefault("description");
            var hasSubscriptionPlan = values.GetValueOrDefault("hassubscriptionplan");
            var subscriptionPlanCost = values.GetValueOrDefault("subscriptionplancost");
            var subscriptionPlanPeriod = values.GetValueOrDefault("subscriptionplanperiod");
            var subscriptionPlanPeriodNumber = values.GetValueOrDefault("subscriptionPlanPeriodNumber");
            var subscriptionPaymentStartType = values.GetValueOrDefault("subscriptionPaymentStartType");
            var subscriptionStartDate = values.GetValueOrDefault("subscriptionStartDate");
            var subscriptionEndDate = values.GetValueOrDefault("subscriptionEndDate");
            var isOngoingSubscription = values.GetValueOrDefault("isongoingsubscription");
            var hasPaymentPlan = values.GetValueOrDefault("haspaymentplan");
            var paymentPlanCost = values.GetValueOrDefault("paymentplancost");
            var paymentPlanPeriod = values.GetValueOrDefault("paymentplanperiod");
            var paymentPlanPeriodNumber = values.GetValueOrDefault("paymentplanperiodnumber");
            var paymentPlanPaymentStartType = values.GetValueOrDefault("paymentplanpaymentstarttype");
            var hasPostPayPlan = values.GetValueOrDefault("haspostpayplan");
            var postPayCost = values.GetValueOrDefault("postpaycost");
            var accountActivityGroup = values.GetValueOrDefault("accountactivitygroup");
            var provider = values.GetValueOrDefault("provider");
            var providerName = values.GetValueOrDefault("providername");
            var providerFirstName = values.GetValueOrDefault("providerfirstname");
            var providerLastName = values.GetValueOrDefault("providerlastname");
            var providerEmail = values.GetValueOrDefault("provideremail");
            var providerPhone = values.GetValueOrDefault("providerphone");
            var type = values.GetValueOrDefault("type");
            var virtualLocation = values.GetValueOrDefault("virtuallocation");
            var inviteOnly = values.GetValueOrDefault("inviteonly");
            var minimumSeatsRequired = values.GetValueOrDefault("minseatsrequired");
            var splitCostEvenly = values.GetValueOrDefault("splitcostevenly");
            var registrationFeeDownPayment = values.GetValueOrDefault("regfeedownpayment");
            var hidePreciseLocation = values.GetValueOrDefault("hidepreciselocation");
            var isParentPriceCost = values.GetValueOrDefault("isparentpricecost");
            var isParentPricePostPay = values.GetValueOrDefault("isparentpricepostpay");
            var isParentPricePaymentPlan = values.GetValueOrDefault("isparentpricepaymentplan");
            var isParentPriceSubscription = values.GetValueOrDefault("isparentpricesubscription");
            var prepaidPeriodsPostPay = values.GetValueOrDefault("prepaidperiodspostpay");
            var postPayPlanPeriod = values.GetValueOrDefault("postpayplanperiod");
            var costFrequencyType = values.GetValueOrDefault("costfrequencytype");
            var cutoffHours = values.GetValueOrDefault("cutoffhours");
            var isContinuousEnrollmentEnabled = values.GetValueOrDefault("isContinuousEnrollmentEnabled");

            var activityType = ActivityType.NotSet;

            if (type.IsNotEmpty() && type.IsNumber())
                activityType = type.ToString().ToInteger().ToEnum<ActivityType>();

            if (!activityId.IsNumber() || activityId.ToInteger() == 0)
            {
                if (name.IsEmpty())
                    msgs.Add("Activity Name is required", MessageType.UserError);

                if (activityType.NotIn(new List<ActivityType> {ActivityType.FieldTrip, ActivityType.PermissionSlip}))
                {
                    if (!seatsAvailable.IsNumber())
                    {
                        msgs.Add("Total Available Seats is required and must be greater than 0", MessageType.UserError);
                    }
                    else
                    {
                        var ap = account.GetActiveAccountPackage(msgs, dc);
                        var maxParticipants = ap.GetFeature(FeatureType.Participants, msgs, dc).Max.Value;
                        if (maxParticipants < seatsAvailable.ToString().ToInteger())
                            msgs.Add(
                                string.Format("Total Available Seats is required and must be less than or equal to {0}",
                                    maxParticipants), MessageType.UserError);
                    }
                }

                if (group.IsEmpty() && registrationPeriod.ToString().ToInteger() > 0)
                {
                    if (!$"{registrationOpenDate} {registrationOpenTime}".IsDateTime())
                        msgs.Add("Registration Open Date & Time is required", MessageType.UserError);

                    if (!$"{registrationCloseDate} {registrationCloseTime}".IsDateTime())
                        msgs.Add("Registration Close Date & Time is required", MessageType.UserError);
                }

                //pre-load account info
                if (contactPhone.IsBlank())
                    contactPhone = account.Phone;

                if (cost.IsBlank())
                    cost = "0";
            }

            Activity activity;

            if (activityId.IsNotEmpty())
            {
                activity = Activity.Load(activityId.ToInteger(), msgs, dc);
                if (seatsAvailable.IsNumber() &&
                    activity.GetRegistrantCountByAssignmentTypes(Registration.ReservedStatuses, msgs, dc) <=
                    seatsAvailable.ToString().ToInteger())
                {
                    activity.SeatsAvailable = seatsAvailable.ToString().ToInteger();
                }
            }
            else
            {
                activity = new Activity
                {
                    ActivityTypeId = activityType,
                    CreateDate = DateTime.UtcNow,
                    CreatedBy = loggedInUserId,
                    SeatsAvailable = seatsAvailable.ToString().ToInteger(),
                    AccountId = account.ID,
                    RequiresParentalConsent = activityType != ActivityType.VolunteerOpportunity,
                    CostFrequencyTypeId = ActivityCostFrequencyType.PerActivity,
                };

                if (activityType.In(new[] {ActivityType.FieldTrip, ActivityType.PermissionSlip}))
                    activity.IsInviteOnly = true;

                if (group.IsEmpty() && !form.IsNumber())
                    msgs.Add("Registration Form is required", MessageType.UserError);
            }

            activity.LastModifiedDate = DateTime.UtcNow;
            activity.LastModifiedBy = loggedInUserId;

            if (name.IsNotEmpty())
            {
                activity.Name = name.ToString();

                // would be better to know if the activity has been published at least once
                if (activity.UniqueName.IsEmpty())
                    activity.UniqueName = GenerateUniqueName(name.ToString(), activity.ID, activity.AccountId, msgs, dc);
            }

            if (ageStart.IsNumber())
            {
                if (ageStart.ToString().ToInteger() < 0)
                    activity.AgeStart = null;
                else
                {
                    activity.AgeStart = ageStart.ToString().ToInteger();
                    activity.GradeLevelStart = null;
                }
            }

            if (isContinuousEnrollmentEnabled.IsNotEmpty())
                activity.IsContinuousEnrollment = isContinuousEnrollmentEnabled.ToString().ToBoolean();

            if (ageEnd.IsNumber())
            {
                if (ageEnd.ToString().ToInteger() < 0)
                    activity.AgeEnd = null;
                else
                {
                    activity.AgeEnd = ageEnd.ToString().ToInteger();
                    activity.GradeLevelEnd = null;
                }
            }

            var gradeLevels = GradeLevel.Load(msgs: msgs, dc: dc).Select(grade => grade.ID).ToList();

            if (gradeStart.IsNumber())
            {
                if (gradeLevels.Contains(gradeStart.ToString().ToInteger()))
                {
                    activity.GradeLevelStart = gradeStart.ToString().ToInteger();
                    activity.AgeStart = null;
                }
                else
                    activity.GradeLevelStart = null;
            }

            if (gradeEnd.IsNumber())
            {
                if (gradeLevels.Contains(gradeEnd.ToString().ToInteger()))
                {
                    activity.GradeLevelEnd = gradeEnd.ToString().ToInteger();
                    activity.AgeEnd = null;
                }
                else
                    activity.GradeLevelEnd = null;
            }

            if (minimumSeatsRequired.IsNumber())
                activity.MinSeatsRequired = minimumSeatsRequired.ToString().ToInteger();

            if (genderRestrictions.IsNumber())
                activity.GenderRestriction = (GenderRestrictionType) genderRestrictions.ToString().ToInteger();

            if (requiresParentalConsent.IsNumber())
                activity.RequiresParentalConsent = requiresParentalConsent.ToString().ToBoolean();

            if (isStudentAllowed.IsNumber())
                activity.IsStudentAllowedToRegister = isStudentAllowed.ToString().ToBoolean();

            if (contactName.IsNotEmpty())
                activity.ContactName = contactName.ToString().IfBlank("").MaxChars(50);

            if (contactPhone.IsNotEmpty())
            {
                if (contactPhone.ToString().IsPhone())
                    activity.ContactPhone = contactPhone.ToString().IfBlank("").MaxChars(50);
                else
                    msgs.Add("Contact Phone must be a valid phone number", MessageType.UserError);
            }

            if (contactEmail.IsNotEmpty())
                activity.ContactEmail = contactEmail.ToString().IfBlank("").MaxChars(100);

            if (description.IsNotEmpty())
                activity.Description = description.ToString().IfBlank("").MaxChars(1000);

            if (cost.IsNumber())
                activity.CostPerRegistrant = cost.ToString().ToDecimal();

            if (costDescription != null)
                activity.CostDescription = costDescription.ToString().IfBlank("").MaxChars(1000);

            if (allowToPayLater.IsNotEmpty() && allowToPayLater.ToString().IsBoolean())
                activity.AllowToPayLater = allowToPayLater.ToString().ToBoolean();

            if (registrationMode.IsNumber())
                activity.RegistrationModeId = (RegistrationMode) registrationMode.ToString().ToInteger();

            if (costFrequencyType.IsNumber())
                activity.CostFrequencyTypeId = (ActivityCostFrequencyType) costFrequencyType.ToString().ToInteger();

            if (leaderType.IsNumber())
            {
                var leaderTypeId = (ActivityLeaderType) leaderType.ToString().ToInteger();

                if (leaderTypeId == ActivityLeaderType.NotSet)
                    msgs.Add("Led By is required", MessageType.UserError);
                else
                    activity.LeaderTypeId = leaderTypeId;
            }

            if (accountActivityGroup.IsNotEmpty())
                activity.AccountActivityGroupId = accountActivityGroup.ToString().ToInteger();

            if (inviteOnly != null)
                activity.IsInviteOnly = inviteOnly.ToString().ToBoolean();

            if (hidePreciseLocation.IsNotEmpty() && hidePreciseLocation.ToString().IsBoolean())
                activity.HidePreciseLocation = hidePreciseLocation.ToString().ToBoolean();

            if (isParentPriceCost.IsNotEmpty() && isParentPriceCost.ToString().IsBoolean())
                activity.IsConvenienceFeeTakenOutOfCost = isParentPriceCost.ToString().ToBoolean();

            if (cutoffHours.IsNotEmpty() && cutoffHours.IsNumber())
                activity.RegistrationCutoffHours = cutoffHours.ToString().ToInteger();

            if (msgs.HasNoErrors())
            {
                Database.ExecuteTransaction(
                    action: (amsgs, adc) =>
                    {
                        if (!activity.ID.HasValue)
                        {
                            if (contactName.IsEmpty())
                                activity.ContactName = account.GetAccountOwner(amsgs, adc).DisplayName;

                            if (contactEmail.IsEmpty())
                                activity.ContactEmail = account.Email;

                            if (contactPhone.IsEmpty())
                                activity.ContactPhone = account.Phone;
                        }

                        ActivityGroup activityGroup = null;

                        if (group.IsNotEmpty() && group.ToString().IsGuid())
                            activityGroup = ActivityGroup.LoadByUid(group.ToString().ToGuid(), amsgs, adc);

                        if (activityGroup.IsNotEmpty())
                        {
                            activity.ActivityGroupId = activityGroup.ID.Value;
                            activity.GradeLevelStart = activity.GradeLevelStart.IsNotEmpty()
                                ? activity.GradeLevelStart
                                : activityGroup.MinGradeLevelId;
                            activity.GradeLevelEnd = activity.GradeLevelEnd.IsNotEmpty()
                                ? activity.GradeLevelEnd
                                : activityGroup.MaxGradeLevelId;
                            activity.GenderRestriction = activityGroup.GenderRestriction;
                        }

                        amsgs.AddRange(activity.Save(adc));

                        // create / associate form
                        if (amsgs.HasNoErrors() && form.IsNumber())
                            amsgs.AddRange(activity.SetupNewForm(form.ToString().ToInteger(), account, loggedInUserId,
                                adc));

                        // skip if it is attached to a activity group
                        // create registration period if not created yet and there is no activitygroup (activity groups have their own registration period)
                        if (!activity.ActivityGroupId.HasValue && amsgs.HasNoErrors() && registrationPeriod.IsNumber())
                            amsgs.AddRange(activity.SetupRegistrationPeriods(
                                registrationPeriod.IfEmpty(""),
                                registrationOpenDate.IfEmpty(""),
                                registrationOpenTime.IfEmpty(""),
                                registrationCloseDate.IfEmpty(""),
                                registrationCloseTime.IfEmpty(""),
                                account,
                                loggedInUserId,
                                adc));

                        //add creator to activity team if new activity
                        if (amsgs.HasNoErrors() && activityId.IsEmpty())
                            amsgs.AddRange(activity.SetupTeam(account, loggedInUserId, adc));

                        // setup schedule
                        if (amsgs.HasNoErrors() && (schedule.IsNotEmpty() || scheduleDates.IsNotEmpty()))
                            amsgs.AddRange(activity.SetupSchedule(
                                account,
                                schedule.IfEmpty(""),
                                (List<ActivityInstance>) scheduleDates,
                                (List<DateTime>) scheduleDatesClosed,
                                scheduleDetails.IfEmpty(""),
                                repeatsWeekly.IfEmpty(""),
                                startDate.IfEmpty(""),
                                endDate.IfEmpty(""),
                                dailyStartTime.IfEmpty(""),
                                dailyEndTime.IfEmpty(""),
                                isOngoing.IfEmpty(""),
                                days.IfEmpty(""),
                                loggedInUserId,
                                adc));

                        //setup initial activityProperties
                        if (amsgs.HasNoErrors())
                            amsgs.AddRange(activity.SetupActivityProperties(
                                registrationThankYouEmail.IfEmpty(null),
                                applicationThankYouEmail.IfEmpty(null),
                                loggedInUserId,
                                adc));

                        if (amsgs.HasNoErrors() && instructor.IsNotEmpty())
                            amsgs.AddRange(activity.SetupInstructor(
                                account,
                                instructor.IfEmpty(""),
                                instructorFirstName.IfEmpty(""),
                                instructorLastName.IfEmpty(""),
                                instructorEmail.IfEmpty(""),
                                instructorPhone.IfEmpty(""),
                                loggedInUserId,
                                adc
                            ));

                        //setup location
                        if (activityLocationType.IsNotEmpty() && activityLocationType.IsNumber())
                        {
                            activity.ActivityLocationTypeId = activityLocationType.ToString().ToInteger()
                                .ToEnum<ActivityLocationType>();

                            if (activity.ActivityLocationTypeId == ActivityLocationType.Virtual && virtualLocation.IsNotEmpty())
                                activity.VirtualLocationInfo = virtualLocation.ToString();

                            if (amsgs.HasNoErrors())
                            {
                                amsgs.AddRange(activity.Save(adc));

                                if (activity.ActivityLocationTypeId == ActivityLocationType.Physical &&
                                    amsgs.HasNoErrors() && (location.IsNotEmpty() || locationName.IsNotEmpty()))
                                {
                                    var hostAccountId = isMyLocation.IfEmpty("").ToBoolean()
                                        ? account.ID
                                        : new int?();

                                    amsgs.AddRange(activity.SetupLocation(
                                        location.IfEmpty(""),
                                        locationName.IfEmpty(""),
                                        address1.IfEmpty(""),
                                        streetNumber.IfEmpty(""),
                                        streetName.IfEmpty(""),
                                        address2.IfEmpty(""),
                                        neighborhood.IfEmpty(""),
                                        city.IfEmpty(""),
                                        county.IfEmpty(""),
                                        state.IfEmpty(""),
                                        postalCode.IfEmpty(""),
                                        country.IfEmpty(""),
                                        placeId.IfEmpty(""),
                                        latitude.IsNotEmpty() ? latitude.ToString().ToDecimal() : 0,
                                        longitude.IsNotEmpty() ? longitude.ToString().ToDecimal() : 0,
                                        locationDetails.IfEmpty(""),
                                        isMyLocation.IfEmpty(""),
                                        locationType.IfEmpty(""),
                                        loggedInUserId,
                                        adc));

                                    if (!account.DefaultLocationId.HasValue)
                                        account.SetDefaultLocation(activity.LocationId.Value,
                                            new WebUser {ID = loggedInUserId}, amsgs, dc);
                                }
                            }
                        }

                        //setup provider
                        if (amsgs.HasNoErrors() && activity.LeaderTypeId == ActivityLeaderType.ExternalProvider &&
                            (provider.IsNumber() || providerName.IsNotEmpty()))
                            amsgs.AddRange(activity.SetupProvider(
                                account,
                                provider.IfEmpty(""),
                                providerName.IfEmpty(""),
                                providerFirstName.IfEmpty(""),
                                providerLastName.IfEmpty(""),
                                providerEmail.IfEmpty(""),
                                providerPhone.IfEmpty(""),
                                loggedInUserId,
                                adc));

                        //setup SubscriptionPlan
                        if (amsgs.HasNoErrors() && hasSubscriptionPlan != null &&
                            hasSubscriptionPlan.ToString().IsBoolean())
                        {
                            amsgs.AddRange(activity.SetupSubscriptionPlan(
                                account,
                                hasSubscriptionPlan.IfEmpty(""),
                                subscriptionPlanCost.IfEmpty(""),
                                subscriptionPlanPeriod.IfEmpty(""),
                                subscriptionPlanPeriodNumber.IfEmpty(""),
                                subscriptionPaymentStartType.IfEmpty(""),
                                subscriptionStartDate.IsNotEmpty()
                                    ? Convert.ToDateTime(subscriptionStartDate)
                                    : (DateTime?) null,
                                subscriptionEndDate.IsNotEmpty()
                                    ? Convert.ToDateTime(subscriptionEndDate)
                                    : (DateTime?) null,
                                isOngoingSubscription.IfEmpty(""),
                                splitCostEvenly.IfEmpty(""),
                                registrationFeeDownPayment.IfEmpty(""),
                                isParentPriceSubscription.IfEmpty("0"),
                                loggedInUserId,
                                adc
                            ));
                        }

                        //setup PaymentPlan
                        if (amsgs.HasNoErrors() && hasPaymentPlan != null && hasPaymentPlan.ToString().IsBoolean())
                            amsgs.AddRange(activity.SetupPaymentPlan(
                                account,
                                hasPaymentPlan.IfEmpty(""),
                                paymentPlanCost.IfEmpty(""),
                                paymentPlanPeriod.IfEmpty(""),
                                paymentPlanPeriodNumber.IfEmpty(""),
                                paymentPlanPaymentStartType.IfEmpty(""),
                                isParentPricePaymentPlan.IfEmpty("0"),
                                loggedInUserId,
                                adc
                            ));

                        //setup PaymentPlan
                        if (amsgs.HasNoErrors() && hasPostPayPlan != null && hasPostPayPlan.ToString().IsBoolean())
                            amsgs.AddRange(activity.SetupPostPayPlan(
                                account,
                                hasPostPayPlan.IfEmpty(""),
                                postPayCost.IfEmpty(""),
                                isParentPricePostPay.IfEmpty("0"),
                                prepaidPeriodsPostPay.IfEmpty(""),
                                postPayPlanPeriod.IfEmpty(""),
                                loggedInUserId,
                                adc
                            ));

                        if (amsgs.HasNoErrors() && details.IsNotEmpty())
                            amsgs.AddRange(activity.SetActivityDetails(details.ToString().IfBlank(""), adc));
                    },
                    onSuccess: smsgs => msgs.AddRange(smsgs),
                    onFailure: fmsgs => msgs.AddRange(fmsgs),
                    onException: emsgs => Utilities.SystemErrorLog(emsgs),
                    dc: dc
                );
            }

            return activity;
        }

        private static SystemMessages SetupInstructor(this Activity activity, Account account, string instructor,
            string instructorfirstname, string instructorlastname, string instructoremail, string instructorphone,
            int loggedInUserId, DatabaseConnection dc = null)
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
                    else if (!instructoremail.IsRegExMatch(BasicLibrary.Utilities.EmailRegEx))
                        msgs.Add("Instructor Email must be a valid email", MessageType.UserError);

                    if (instructorphone.IsNotEmpty() && !instructorphone.IsRegExMatch(BasicLibrary.Utilities.PhoneRegEx))
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

        private static SystemMessages SetupSchedule(this Activity activity, Account account, string schedule,
            List<ActivityInstance> scheduledates, List<DateTime> scheduledatesclosed, string scheduledetails,
            string repeatsweekly, string startdate, string enddate, string dailystarttime, string dailyendtime,
            string isongoing, string daysOfTheWeek, int loggedInUserId, DatabaseConnection dc = null)
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
                daysOfTheWeek,
                account,
                activity.ActivityGroupId,
                loggedInUserId,
                msgs,
                dc);

            activity.ActivityScheduleId = newSchedule.ID;

            msgs.AddRange(activity.Save(dc));

            return msgs;
        }

        private static SystemMessages SetupSubscriptionPlan(this Activity activity, Account account,
            string hassubscriptionplan, string subscriptionplancost, string subscriptionplanperiod,
            string subscriptionPlanPeriodNumber,
            string subscriptionPaymentStartType, DateTime? subscriptionStartDate, DateTime? subscriptionEndDate,
            string isongoingsubscription, string splitcostevenly, string regfeedownpayment,
            string isparentpricesubscription,
            int loggedInUserId, DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();
            var hasStripeChanges = true;
            var paymentStartType = subscriptionPaymentStartType.ToInteger().ToEnum<PlanPaymentStartType>();
            var isParentPrice = isparentpricesubscription.ToBoolean();

            // first: desactivate the current plan if there are any changes
            if (activity.HasSubscriptionPlan(msgs, dc))
            {
                var plan = activity.GetSubscriptionPlan(msgs, dc);

                hasStripeChanges = plan.CostPerPeriod.Value != subscriptionplancost.ToDecimal() ||
                                   plan.BillingPeriodTypeId != (BillingPeriodType) subscriptionplanperiod.ToInteger() ||
                                   plan.PaymentStartTypeId != paymentStartType ||
                                   (subscriptionStartDate.HasValue && subscriptionStartDate.Value != plan.StartDate) ||
                                   (subscriptionEndDate.HasValue && subscriptionEndDate.Value != plan.EndDate) ||
                                   plan.IsConvenienceFeeTakenOutOfCost != isParentPrice;

                // if has stripe changes or subscription option has been turned off
                if (hasStripeChanges || !hassubscriptionplan.ToBoolean())
                    msgs.AddRange(plan.Deactivate(loggedInUserId, dc));
                else
                {
                    plan.RegFeeAsDownPayment = regfeedownpayment.ToBoolean();
                    plan.SplitCostEvenly = splitcostevenly.ToBoolean();
                    msgs.AddRange(plan.Save(dc));
                }
            }

            // if the subscription plan option(UI) is activated
            if (hassubscriptionplan.ToBoolean() && hasStripeChanges)
                SubscriptionPlanService.CreateNewSubscriptionPlan(
                    planOwner: account,
                    participationObjectType: ParticipationObjectType.Activity,
                    participationObjectId: activity.ID.Value,
                    costPerPeriod: subscriptionplancost.ToDecimal(),
                    billingPeriodType: (BillingPeriodType) subscriptionplanperiod.ToInteger(),
                    paymentStartType: paymentStartType,
                    subscriptionStartDate: subscriptionStartDate,
                    subscriptionEndDate: subscriptionEndDate,
                    isongoingsubscription: isongoingsubscription.ToBoolean(),
                    doesAutoRenew: true,
                    splitCostEvenly: splitcostevenly.ToBoolean(),
                    regFeeAsDownPayment: regfeedownpayment.ToBoolean(),
                    isConvenienceFeeTakenOutOfCost: isParentPrice,
                    loggedInUserId: loggedInUserId,
                    msgs: msgs,
                    dc: dc);

            return msgs;
        }

        private static SystemMessages SetupPaymentPlan(this Activity activity, Account account, string haspaymentplan,
            string paymentplancost, string paymentplanperiod, string paymentplanperiodnumber,
            string paymentplanpaymentstarttype, string isparentpricepaymentplan, int loggedInUserId,
            DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();
            var hasChanges = true;
            var planStartType = paymentplanpaymentstarttype.ToInteger().ToEnum<PlanPaymentStartType>();
            var isParentPrice = isparentpricepaymentplan.ToBoolean();

            // first: desactivate the current plan if there are any changes
            if (activity.HasPaymentPlan(msgs, dc))
            {
                var plan = activity.GetPaymentPlan(msgs, dc);

                hasChanges = plan.CostPerPeriod.Value != paymentplancost.ToDecimal() ||
                             plan.BillingPeriodTypeId != (BillingPeriodType) paymentplanperiod.ToInteger() ||
                             plan.TotalPeriods.Value != paymentplanperiodnumber.ToInteger() ||
                             plan.PaymentStartTypeId != planStartType ||
                             isParentPrice != plan.IsConvenienceFeeTakenOutOfCost;

                if (hasChanges || !haspaymentplan.ToBoolean())
                    msgs.AddRange(plan.Deactivate(loggedInUserId, dc));
            }

            // if the subscription plan option(UI) is activated
            if (haspaymentplan.ToBoolean() && hasChanges)
                SubscriptionPlanService.CreateNewPaymentPlan(
                    account,
                    ParticipationObjectType.Activity,
                    activity.ID.Value,
                    paymentplancost.ToDecimal(),
                    (BillingPeriodType) paymentplanperiod.ToInteger(),
                    paymentplanperiodnumber.ToInteger(),
                    planStartType,
                    isParentPrice,
                    loggedInUserId,
                    msgs,
                    dc);

            return msgs;
        }

        private static SystemMessages SetupPostPayPlan(this Activity activity, Account account,
            string haspostpayplan, string postpaycost, string isparentpricepostpay, string prepaidperiodspostpay,
            string postpayplanperiod,
            int loggedInUserId, DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();
            var hasChanges = true;
            var isParentPrice = isparentpricepostpay.ToBoolean();
            var prepaidPeriods = prepaidperiodspostpay.IsNumber() ? prepaidperiodspostpay.ToInteger() : new int?();

            // first: desactivate the current plan if there are any changes
            if (activity.HasPostPayPlan(msgs, dc))
            {
                var plan = activity.GetPostPayPlan(msgs, dc);

                hasChanges = plan.CostPerPeriod.Value != postpaycost.ToDecimal() ||
                             isParentPrice != plan.IsConvenienceFeeTakenOutOfCost ||
                             prepaidPeriods != plan.NumberOfPeriodsPrePaid ||
                             plan.BillingPeriodTypeId != (BillingPeriodType) postpayplanperiod.ToInteger();

                // if has stripe changes or subscription option has been turned off
                if (hasChanges || !haspostpayplan.ToBoolean())
                    msgs.AddRange(plan.Deactivate(loggedInUserId, dc));
            }

            // if the subscription plan option(UI) is activated
            if (haspostpayplan.ToBoolean() && hasChanges)
                SubscriptionPlanService.CreateNewPostPayPlan(
                    planOwner: account,
                    participationObjectType: ParticipationObjectType.Activity,
                    participationObjectId: activity.ID.Value,
                    costPerPeriod: postpaycost.ToDecimal(),
                    billingPeriodType: (BillingPeriodType) postpayplanperiod.ToInteger(),
                    isConvenienceFeeTakenOutOfCost: isParentPrice,
                    prepaidPeriods: prepaidPeriods,
                    loggedInUserId: loggedInUserId,
                    msgs: msgs,
                    dc: dc);

            return msgs;
        }

        private static SystemMessages SetupProvider(this Activity activity, Account activityAccount, string provider,
            string providername, string providerfirstname, string providerlastname, string provideremail,
            string providerphone, int loggedInUserId, DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();
            var loggedInUser = new WebUser {ID = loggedInUserId};

            Account providerAccount = null;

            // provider already exists
            if (provider.IsNumber() && provider.ToInteger() > 0)
                providerAccount = Account.Load(provider.ToInteger(), msgs, dc);
            else
            {
                var providerMsgs = new SystemMessages();

                if (providername.IsEmpty())
                    providerMsgs.Add("Provider Organization or Program is required", MessageType.UserError);

                if (providerfirstname.IsEmpty())
                    providerMsgs.Add("Provider First Name is required", MessageType.UserError);

                if (providerlastname.IsEmpty())
                    providerMsgs.Add("Provider Last Name is required", MessageType.UserError);

                if (!provideremail.IsEmail())
                    providerMsgs.Add("Provider Email is required", MessageType.UserError);

                if (providerphone.IsNotEmpty() && !providerphone.IsPhone())
                    providerMsgs.Add("Please enter a valid phone number", MessageType.UserError);

                if (providerMsgs.Count >= 4)
                    msgs.Add("Provider Information is required", MessageType.UserError);
                else
                    msgs.AddRange(providerMsgs);

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
                        timeZone: activityAccount.TimeZone,
                        loggedInUser: loggedInUser,
                        isAfterSchool: true,
                        createWithStripe: false,
                        msgs: msgs,
                        dc: dc);

                    providerAccount.AddUser(
                        firstName: providerfirstname,
                        lastName: providerlastname,
                        email: provideremail,
                        phone: null,
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
                    requiresPublishPermission: true,
                    showAllPrograms: false,
                    loggedInUser: loggedInUser,
                    dc: dc));

                activity.ProviderAccountId = providerAccount.ID;
                msgs.AddRange(activity.Save(dc));
            }

            return msgs;
        }

        private static SystemMessages SetupLocation(this Activity activity, string location, string locationName,
            string address1, string streetNumber, string streetName, string address2, string neighborhood, string city,
            string county, string state, string postalCode, string country, string googlePlaceId,
            decimal latitude, decimal longitude, string locationdetails, string ismylocation, string locationtype,
            int loggedInUserId, DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();

            var activityLocation = LocationService.CreateOrUpdateFromGoogle(
                locationName,
                streetNumber,
                streetName,
                address2,
                neighborhood,
                city,
                county,
                state,
                postalCode,
                country,
                googlePlaceId,
                latitude,
                longitude,
                new WebUser {ID = loggedInUserId},
                msgs,
                dc);

            activity.LocationId = activityLocation.ID;

            if (locationdetails.IsNotEmpty())
                activity.LocationDetails = locationdetails.IfBlank("").MaxChars(4000);

            msgs.AddRange(activity.Save(dc));

            return msgs;

            //// just switching locations
            //if (location.IsNumber())
            //{
            //    // make sure we setup a relationship
            //    var actualLocation = Location.Load(location.ToInteger(), msgs, dc);

            //    // don't need to ask permission for your own location
            //    if (actualLocation.HostAccountId.HasValue && actualLocation.HostAccountId != activity.AccountId)
            //    {
            //        var locationOwner = Account.Load(actualLocation.HostAccountId.Value, msgs, dc);
            //        var visitorAccount = activity.GetAccount(msgs, dc);

            //        var relationship = Services.AccountRelationshipService.GetOrCreateNew(
            //           hostAccount: locationOwner,
            //           visitorAccount: visitorAccount,
            //           approvalStatus: ApprovalStatus.Requested,
            //           hostPermissionType: HostPermissionType.ApproveOnly,
            //           loggedInUser: new WebUser { ID = loggedInUserId },
            //           msgs: msgs,
            //           dc: dc);

            //        var relationshipLocation = Services.AccountRelationshipLocationService.GetOrCreateNew(
            //            accountRelationship: relationship,
            //            hostAccount: locationOwner,
            //            visitorAccount: visitorAccount,
            //            location: actualLocation,
            //            loggedInUser: new WebUser { ID = loggedInUserId },
            //            msgs: msgs,
            //            dc: dc);
            //    }

            //    activity.LocationId = location.ToString().ToInteger();
            //}
            //// they entered a new location possibly? 
            //// otherwise somehow the UI allowed them to enter a location that already exists, with the exact same location name
            //else
            //{
            //    if (address1.IsEmpty())
            //        msgs.Add("Address 1 is required", MessageType.UserError);
            //    if (city.IsEmpty())
            //        msgs.Add("City is required", MessageType.UserError);
            //    if (state.IsEmpty())
            //        msgs.Add("State is required", MessageType.UserError);
            //    if (postalcode.IsEmpty())
            //        msgs.Add("Postal Code is required", MessageType.UserError);

            //    if (msgs.HasErrors())
            //        return msgs;

            //    var locationType = LocationType.NotSet;
            //    var hostAccountId = new int?();

            //    if (locationtype.IsNumber())
            //        locationType = (LocationType)locationtype.ToInteger();

            //    if (ismylocation.IsBoolean() && ismylocation.ToBoolean())
            //        hostAccountId = activity.AccountId;

            //    var activityLocation = Services.LocationService.CreateNewWithValidation(
            //            locationname,
            //            address1,
            //            address2,
            //            city,
            //            state,
            //            postalcode,
            //            "US",
            //            locationType,
            //            hostAccountId,
            //            new Account { ID = activity.AccountId },
            //            new WebUser { ID = loggedInUserId },
            //            msgs,
            //            dc);

            //    activity.LocationId = activityLocation.ID;
            //}
        }

        private static SystemMessages SetupActivityProperties(this Activity activity, string registrationthankyouemail,
            string applicationthankyouemail, int loggedInUserId, DatabaseConnection dc = null)
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

        private static SystemMessages SetupRegistrationPeriods(this Activity activity, string registrationperiod,
            string registrationopendate, string registrationopentime, string registrationclosedate,
            string registrationclosetime, Account account, int loggedInUserId, DatabaseConnection dc = null)
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
                var periodname = RegistrationPeriod
                    .GenerateUniqueName(account.ID.Value, $"{activity.Name} - Reg Window", dc: dc).MaxChars(50);
                var openDate = $"{registrationopendate} {registrationopentime}".ToDateTime();
                var closeDate = $"{registrationclosedate} {registrationclosetime}".ToDateTime();

                if (activity.CostFrequencyTypeId == ActivityCostFrequencyType.PerClass)
                    closeDate = openDate.AddYears(10);

                activity.AddNewRegistrationPeriod(
                    periodname,
                    openDate,
                    closeDate,
                    account.ID.Value,
                    loggedInUserId,
                    msgs,
                    dc);
            }
            else
            {
                var period = RegistrationPeriod.Load(registrationperiod.ToInteger(), dc: dc);

                period.StartDateTime = $"{registrationopendate} {registrationopentime}".ToDateTime();
                period.EndDateTime = $"{registrationclosedate} {registrationclosetime}".ToDateTime();
                period.LastModifiedDate = DateTime.UtcNow;
                period.LastModifiedBy = loggedInUserId;
                msgs.AddRange(period.Save(dc: dc));

                msgs.AddRange(period.AddActivity(activity, true, loggedInUserId, dc: dc));
            }

            return msgs;
        }

        public static SystemMessages SetupTeam(this Activity activity, Account account, int loggedInUserId,
            DatabaseConnection dc = null)
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

        private static SystemMessages SetupNewForm(this Activity activity, int formId, Account account,
            int loggedInUserId, DatabaseConnection dc = null)
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
                    var defaultForm = account.GetDefaultForm(activity, msgs, dc);
                    newForm = defaultForm.Copy(loggedInUserId, account.ID.Value, defaultForm.FormTypeId, null, msgs,
                        dc: dc);
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
                    activity.AddFormAndActivate(newForm, loggedInUserId, isPublished, msgs, dc: dc);
                }
            }

            return msgs;
        }

        public static bool IsInSession(this Activity activity, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var account = activity.GetAccount(msgs, dc);
            var now = account.ConvertToAccountTimeZone(DateTime.UtcNow);
            var schedule = activity.GetSchedule(msgs, dc);
            var instances = schedule?.GetInstances(msgs: msgs, dc: dc);

            if (instances.IsNotEmpty() && instances.Any())
                return instances.First().StartDateTime <= now && now <= instances.Last().EndDateTime;

            if (schedule.IsNotEmpty())
                return schedule.IsOngoing;

            return false;
        }

        public static bool WillBeInSession(this Activity activity, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            var account = activity.GetAccount(msgs, dc);
            var now = account.ConvertToAccountTimeZone(DateTime.UtcNow);
            var instances = activity.GetSchedule(msgs, dc)?.GetInstances(msgs: msgs, dc: dc);

            if (instances.IsNotEmpty() && instances.Any())
                return now < instances.First().StartDateTime;

            return false;
        }

        public static bool IsOutOfSession(this Activity activity, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            var account = activity.GetAccount(msgs, dc);
            var now = account.ConvertToAccountTimeZone(DateTime.UtcNow);
            var schedule = activity.GetSchedule(msgs, dc);
            var instances = schedule?.GetInstances(msgs: msgs, dc: dc);

            if (instances.IsNotEmpty() && instances.Any())
                return now > instances.Last().StartDateTime;

            if (schedule.IsNotEmpty())
                return !schedule.IsOngoing;

            return true;
        }

        public static bool CanAcceptPayments(this Activity activity, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            return activity.GetAccount(msgs, dc).CanProcessPayments(msgs, dc);
        }

        public static PurchaseableObject GetPurchaseableObject(this Activity activity)
        {
            return PurchaseableObject.FromActivity(activity);
        }

        public static bool HasTeamMatches(this Activity activity, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            return Match.LoadByTeamId(activity.ID.Value, msgs, dc).IsNotEmpty();
        }

        public static bool IsValidActivityInstance(this Activity activity, ActivityInstance activityInstance,
            SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (activityInstance?.ActivityScheduleId == activity.ActivityScheduleId)
                return true;

            msgs.Add("Invalid record error", MessageType.Error);
            return false;
        }

        public static bool HasNoTeamMatches(this Activity activity, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            return Match.LoadByTeamId(activity.ID.Value, msgs, dc).IsEmpty();
        }

        public static Attendance GetOrCreateNewAttendance(this Activity activity, int? attendanceId,
            int? activityInstanceId, WebUser loggedInUser, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            // get attendance by activityInstanceId if it exists
            if (activityInstanceId.HasValue)
            {
                var activityInstance = ActivityInstance.Load(activityInstanceId.Value, msgs, dc);
                if (!activity.IsValidActivityInstance(activityInstance, msgs, dc))
                    return null;

                var attendance = activityInstance.GetAttendance(msgs, dc);
                if (attendance.IsNotEmpty())
                    return attendance;
            }

            // check if most recent activityinstance has an attendance
            var closestActivityInstance = activity.GetClosestPastActivityInstance(msgs, dc);
            if (closestActivityInstance.IsEmpty())
                return null;

            var closestActivityInstanceAttendance = Attendance
                .LoadByActivityInstanceId(closestActivityInstance.ID.Value, msgs, dc).FirstOrDefault();

            if (closestActivityInstanceAttendance.IsNotEmpty())
            {
                var registrantActivities = activity.GetRegistrantActivities(msgs, dc);
                var attendanceRegistrants =
                    AttendanceRegistrant.LoadByAttendanceId(closestActivityInstanceAttendance.ID.Value, msgs, dc);

                //.. if student registers in between class, it should be listed in attendance
                //.. for that we need entry of that user in AttendanceRegistrant table
                if (registrantActivities.Count() > attendanceRegistrants.Count)
                {
                    // if a new student has registered then we do not need to create new Attendance. Instead we just need to add AttendanceRegistrant 
                    // to existing Attendance only   
                    activity.CreateNewAttendanceRegistrantByRegistrantActivities(closestActivityInstanceAttendance,
                        attendanceRegistrants, registrantActivities, loggedInUser, msgs, dc);
                }

                if (!activityInstanceId.HasValue)
                    return closestActivityInstanceAttendance;
            }

            // otherwise create a new attendance if there is no attendance id
            if (!attendanceId.HasValue)
                return activity.CreateNewAttendance(
                    activityInstanceId.IsEmpty() ? closestActivityInstance.ID.Value : activityInstanceId, loggedInUser,
                    msgs, dc);

            // otherwise return the attendance with given id
            return Attendance.Load(attendanceId.Value, msgs, dc);
        }

        public static List<AttendanceRegistrant> CreateNewAttendanceRegistrantByRegistrantActivities(
            this Activity activity, Attendance attendance, List<AttendanceRegistrant> attendanceRegistrants,
            List<RegistrantActivity> registrantActivities, WebUser loggedInUser, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            List<AttendanceRegistrant> newAttendanceRegistrants = new List<AttendanceRegistrant>();
            var newStudents = registrantActivities.Where(registrant => !attendanceRegistrants.Any(
                attendanceRegistrant =>
                    attendanceRegistrant.RegistrantId == registrant.RegistrantId)).ToList();

            newStudents
                .ForEach(registrantActivity =>
                {
                    newAttendanceRegistrants.Add(AttendanceRegistrantService.CreateNew(attendance.ID.Value,
                        registrantActivity.RegistrantId.Value,
                        loggedInUser, msgs, dc));
                });
            return newAttendanceRegistrants;
        }

        private static ActivityInstance GetClosestPastActivityInstance(this Activity activity, SystemMessages msgs, DatabaseConnection dc)
        {
            var instances = activity
                .GetSchedule(msgs: msgs, dc: dc)
                .GetInstances(msgs: msgs, dc: dc);

            var account = activity.GetAccount(msgs, dc);
            return GetClosestPastActivityInstance(instances, account);
        }

        internal static ActivityInstance GetClosestPastActivityInstance(IEnumerable<ActivityInstance> instances, Account account)
        {
            var nowInAccountTimeZone = account.ConvertToAccountTimeZone(DateTime.UtcNow);

            var closestInstancesToNow = instances
                .Where(x => x.StartDateTime.HasValue)
                .OrderBy(x => Math.Abs((nowInAccountTimeZone - x.StartDateTime.Value).Ticks)).ToArray();
            if (closestInstancesToNow.Length == 0)
                return null;

            var closestPastActivity = closestInstancesToNow.FirstOrDefault(x => x.StartDateTime!.Value.IsBefore(nowInAccountTimeZone));

            return closestPastActivity.IsNotEmpty() ? closestPastActivity : closestInstancesToNow.First();
        }

        public static ActivityInstance GetClosestActivityInstance(this Activity activity, SystemMessages msgs, DatabaseConnection dc)
        {
            var instances = activity
                .GetSchedule(msgs: msgs, dc: dc)
                .GetInstances(msgs: msgs, dc: dc);

            var account = activity.GetAccount(msgs, dc);
            return GetClosestActivityInstance(instances, account);
        }

        internal static ActivityInstance GetClosestActivityInstance(IEnumerable<ActivityInstance> instances, Account account)
        {
            var dateInAccountTimeZone = account.ConvertToAccountTimeZone(DateTime.UtcNow);

            return instances
                .Where(x => x.StartDateTime.HasValue)
                .OrderBy(x => Math.Abs((dateInAccountTimeZone - x.StartDateTime.Value).Ticks))
                .First();
        }

        public static Attendance CreateNewAttendance(this Activity activity, int? activityInstanceId,
            WebUser loggedInUser, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            //create attendance records for this date and mark as absent by default (even if temporary)
            var attendance = new Attendance();

            Database.ExecuteTransaction
            (
                action: (SystemMessages amsgs, DatabaseConnection adc) =>
                {
                    ActivityInstance activityInstance = null;
                    if (activityInstanceId.HasValue)
                        activityInstance = ActivityInstance.Load(activityInstanceId.Value, amsgs, adc);
                    activity.ClearEmptyAttendances(amsgs, adc);
                    attendance.InitAttendance(activity, loggedInUser, activityInstance, amsgs, adc);
                },
                onSuccess: (SystemMessages smsgs) =>
                {
                    msgs.AddRange(smsgs);
                    attendance = Attendance.Load(attendance.ID.Value);
                },
                onFailure: (SystemMessages fmsgs) => { msgs.AddRange(fmsgs); },
                onException: (SystemMessages emsgs) => { Utilities.SystemErrorLog(emsgs); },
                dc: dc
            );

            return attendance;
        }

        private static void ClearEmptyAttendances(this Activity activity, SystemMessages msgs = null,
            DatabaseConnection dc = null)
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

        public static ActivityMessage AddMessage(this Activity activity, string subject, string body,
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
                        ActivityId = activity.ID,
                        AccountId = activity.AccountId,
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

        public static List<DayOfWeekASL> GetDaysOfTheWeek(this Activity activity, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            var daysOfTheWeek = activity.GetSchedule(msgs: msgs, dc: dc)?.GetDaysOfTheWeek(msgs, dc);
            if (daysOfTheWeek?.Any() ?? false)
                return daysOfTheWeek;

            return new List<DayOfWeekASL>();
        }

        public static int GetRegistrantCount(this Activity activity, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            if (activity.ID.HasValue)
                return RegistrantActivity.GetSeatCountByParticipationObject(
                    activity.ID.Value,
                    ParticipationObjectType.Activity,
                    msgs,
                    dc);

            return 0;
        }

        public static int GetRegistrantCountByAssignmentType(this Activity activity,
            RegistrationAssignmentType assignmentType, SystemMessages msgs = null, DatabaseConnection dc = null)
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

        public static int GetRegistrantCountByAssignmentTypes(this Activity activity,
            List<RegistrationAssignmentType> assignmentTypes, SystemMessages msgs = null, DatabaseConnection dc = null)
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

        public static List<Registrant> GetRegistrants(this Activity activity, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            return activity.GetRegistrantActivities(msgs, dc).Select(x => x.ToRegistrant()).ToList();
        }

        public static List<Registrant> GetRegistrants(this Activity activity, bool includeInvited,
            SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return activity.GetRegistrantActivities(includeInvited, msgs, dc).Select(x => x.ToRegistrant()).ToList();
        }

        public static List<RegistrantActivity> GetRegistrantActivities(this Activity activity,
            SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return activity.GetRegistrantActivities(false, msgs, dc);
        }

        public static List<RegistrantActivity> GetRegistrantActivities(this Activity activity, bool includeInvited,
            SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var ras = new List<RegistrantActivity>();

            var registrantAssigmentTypesToInclude = new List<RegistrationAssignmentType>
            {
                RegistrationAssignmentType.Applied,
                RegistrationAssignmentType.Reserved,
                RegistrationAssignmentType.WaitingList,
                RegistrationAssignmentType.ReservedPendingPayment,
                RegistrationAssignmentType.ReservedPendingParentalConsent,
            };

            if (includeInvited)
                ras = RegistrantActivity
                    .LoadByParticipationObject(activity.ID.Value, ParticipationObjectType.Activity, msgs, dc).ToList();
            else
                ras = RegistrantActivity
                    .LoadByParticipationObject(activity.ID.Value, ParticipationObjectType.Activity, msgs, dc)
                    .Where(x => registrantAssigmentTypesToInclude.Contains(x.AssignmentTypeId))
                    .ToList();

            return ras;
        }

        public static List<RegistrantActivity> GetRegistrantActivitiesForSurvey(this Activity activity,
            SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return RegistrantActivity
                .LoadByParticipationObject(activity.ID.Value, ParticipationObjectType.Activity, msgs, dc)
                .Where(a => a.HasParentRegistrantId(msgs, dc) && a.AssignmentTypeId.In(
                    new List<RegistrationAssignmentType>
                    {
                        RegistrationAssignmentType.Reserved,
                        RegistrationAssignmentType.ReservedPendingParentalConsent,
                        RegistrationAssignmentType.ReservedPendingPayment,
                    })).ToList();
        }

        public static RegistrantActivity GetRegistrantActivity(this Activity activity, Registrant registrant,
            SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return RegistrantActivity.LoadByParticipationObjectByRegistrant(activity.ID.Value,
                ParticipationObjectType.Activity, registrant.ID.Value, msgs, dc);
        }

        public static RegistrantActivity GetRegistrantActivity(this Activity activity, int registrantId,
            SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return RegistrantActivity.LoadByParticipationObjectByRegistrant(activity.ID.Value,
                ParticipationObjectType.Activity, registrantId, msgs, dc);
        }

        public static RegistrationPeriod AddNewRegistrationPeriod(this Activity activity, string name,
            DateTime startDateTime, DateTime endDateTime, int accountId, int loggedInUserId, SystemMessages msgs,
            DatabaseConnection dc = null)
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

        public static ActivityWebUser AddTeamMember(this Activity activity, int webUserId, bool isLeader,
            int loggedInUserId, SystemMessages msgs, DatabaseConnection dc = null)
        {
            var teammember = new ActivityWebUser
            {
                WebUserId = webUserId,
                ActivityId = activity.ID,
                IsLeader = isLeader,
                CreateDate = DateTime.UtcNow,
                CreatedBy = loggedInUserId,
                LastModifiedDate = DateTime.UtcNow,
                LastModifiedBy = loggedInUserId,
            };
            msgs.AddRange(teammember.Save(dc));

            return teammember;
        }

        public static List<ActivityWebUser> GetTeamMembers(this Activity activity, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            return ActivityWebUser.LoadByActivityId(activity.ID.Value, msgs, dc);
        }

        public static List<Form> GetForms(this Activity activity, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            return ActivityForm.LoadByActivityId(activity.ID.Value, dc: dc).Select(x => x.GetForm()).ToList();
        }

        public static Form GetActiveForm(this Activity activity, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            var af = activity.GetActiveActivityForm(msgs, dc);

            if (af.IsEmpty())
                return null;

            return af.GetForm(msgs, dc);
        }

        public static ActivityForm GetActiveActivityForm(this Activity activity, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            if (activity.ID.HasValue)
                return ActivityForm.LoadByActivityIdByIsActive(activity.ID.Value, true, msgs, dc);

            return null;
        }

        public static List<ActivityForm> GetActivityForms(this Activity activity, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            if (activity.ID.HasValue)
                return ActivityForm.LoadByActivityId(activity.ID.Value, msgs: msgs, dc: dc);

            return new List<ActivityForm>();
        }

        public static List<Attendance> GetAttendance(this Activity activity, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            return Attendance.LoadByActivityId(activity.ID.Value, msgs: msgs, dc: dc);
        }

        public static List<Attendance> GetAttendance(this Activity activity, string orderByParam,
            OrderByDirectionEnum direction, int? start = null, int? length = null, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            return Attendance.LoadByActivityId(activity.ID.Value, orderByParam, direction, start, length, msgs, dc);
        }

        public static List<Attendance> GetCompletedAttendance(this Activity activity, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            return Attendance.LoadByActivityIdByIsCompleted(activity.ID.Value, true, msgs: msgs, dc: dc);
        }

        public static List<Attendance> GetCompletedAttendanceWithPaging(this Activity activity, string orderByParam,
            OrderByDirectionEnum direction, int? start = null, int? length = null, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            return Attendance.LoadByActivityIdByIsCompletedWithPaging(activity.ID.Value, true, orderByParam, direction,
                start, length, msgs: msgs, dc: dc);
        }

        public static int GetAttendancePresentCount(this Activity activity, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            return AttendanceRegistrant.GetPresentCountByActivityId(activity.ID.Value, msgs: msgs, dc: dc);
        }

        public static int GetAttendancePossibleCount(this Activity activity, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            return AttendanceRegistrant.GetPossibleCountByActivityId(activity.ID.Value, msgs: msgs, dc: dc);
        }

        public static List<Attendance> GetAttendanceByDate(this Activity activity, DateTime date,
            SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return Attendance.LoadByActivityIdByDate(activity.ID.Value, date, msgs: msgs, dc: dc);
        }

        public static int GetTotalAttendanceCount(this Activity activity, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            return Attendance.GetCount(new Hashtable {{"ActivityId", activity.ID.Value}}, msgs, dc);
        }

        public static List<ActivityRegistrationPeriod> GetActivityRegistrationPeriods(this Activity activity,
            SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return ActivityRegistrationPeriod.LoadByActivityId(activity.ID.Value, true, msgs, dc);
        }

        public static List<RegistrationPeriod> GetRegistrationPeriods(this Activity activity,
            SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return activity.GetActivityRegistrationPeriods(msgs, dc)
                .Select(x => x.ToRegistrationPeriod())
                .ToList();
        }

        public static List<RegistrationPeriod> GetUpcomingRegistrationPeriods(this Activity activity,
            SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return activity.GetRegistrationPeriods(msgs, dc)
                .Where(x => x.StartDateTime.Value > DateTime.UtcNow)
                .OrderBy(x => x.StartDateTime.Value)
                .ToList();
        }

        public static RegistrationPeriod GetCurrentRegistrationPeriod(this Activity activity,
            SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return activity.GetRegistrationPeriods(msgs, dc)
                .Where(x => x.StartDateTime.Value <= DateTime.UtcNow && x.EndDateTime.Value >= DateTime.UtcNow)
                .FirstOrDefault();
        }

        public static RegistrationPeriod GetLatestRegistrationPeriod(this Activity activity, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            var period = activity.GetCurrentOrNextRegistrationPeriod(msgs, dc: dc);
            if (period.IsEmpty())
                period = activity.GetRegistrationPeriods(msgs, dc: dc).OrderByDescending(p => p.EndDateTime)
                    .FirstOrDefault();

            return period;
        }

        public static RegistrationPeriod GetCurrentOrNextRegistrationPeriod(this Activity activity,
            SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var period = activity.GetCurrentRegistrationPeriod(msgs, dc);
            if (period.IsEmpty())
                return activity.GetUpcomingRegistrationPeriods(msgs, dc).FirstOrDefault();

            return period;
        }

        public static RegistrationPeriod GetActiveRegistrationPeriod(this Activity activity, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            if (activity.ID.HasValue)
            {
                if (activity.ActivityGroupId.HasValue)
                {
                    return activity.GetActivityGroup(msgs, dc).GetCurrentOrNextRegistrationPeriod(msgs, dc);
                }
                else
                {
                    var item = ActivityRegistrationPeriod.LoadByActivityIdByIsActive(activity.ID.Value, true, msgs, dc);
                    if (item.IsNotEmpty())
                        return item.ToRegistrationPeriod(true, msgs, dc);
                }
            }

            return null;
        }

        public static Account GetAccount(this Activity activity, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            return Account.Load(activity.AccountId.Value, msgs, dc);
        }

        public static bool BelongsTo(this Activity activity, Account account, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            return activity.AccountId == account.ID;
        }

        public static SystemMessages MarkAsDeleted(this Activity activity, int loggedInUserId,
            DatabaseConnection dc = null)
        {
            activity.IsDeleted = true;
            activity.LastModifiedDate = DateTime.UtcNow;
            activity.LastModifiedBy = loggedInUserId;
            return activity.Save(dc);
        }

        public static SystemMessages MoveToArchive(this Activity activity, int loggedInUserId,
            DatabaseConnection dc = null)
        {
            activity.IsArchived = true;
            activity.LastModifiedDate = DateTime.UtcNow;
            activity.LastModifiedBy = loggedInUserId;
            return activity.Save(dc);
        }

        public static SystemMessages MoveFromArchive(this Activity activity, int loggedInUserId,
            DatabaseConnection dc = null)
        {
            activity.IsArchived = false;
            activity.LastModifiedDate = DateTime.UtcNow;
            activity.LastModifiedBy = loggedInUserId;
            return activity.Save(dc);
        }

        public static SystemMessages UpdateActivitySchedule(this Activity activity, int? ScheduleId, int loggedInUserId,
            SystemMessages msgs, DatabaseConnection dc = null)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            var wasSetToNull = activity.ActivityScheduleId.HasValue && !ScheduleId.HasValue;

            activity.ActivityScheduleId = ScheduleId;
            activity.LastModifiedDate = DateTime.UtcNow;
            activity.LastModifiedBy = loggedInUserId;

            msgs.AddRange(activity.Save(dc));

            if (wasSetToNull)
            {
                var logs = new SystemMessages();
                logs.Add(
                    $"Schedule ID was set to null for: Account ({activity.AccountId}), Activity ({activity.ID}), Logged In User ({loggedInUserId})",
                    MessageType.Notice);
                Utilities.SystemLog(logs);
            }

            return msgs;
        }

        public static AccountProperty AddOrUpdateActivityProperty(this Activity activity, AccountPropertyType type,
            string value, int loggedInUserId, SystemMessages msgs, DatabaseConnection dc = null)
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

        public static AccountProperty GetActivityProperty(this Activity activity, AccountPropertyType propertyType,
            SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return activity.GetActivityProperties(msgs, dc)?.Find(x => x.AccountPropertyTypeId == propertyType);
        }

        public static List<GenderType> GetAllowedGenders(this Activity activity)
        {
            switch (activity.GenderRestriction)
            {
                case GenderRestrictionType.None:
                    return new List<GenderType> {GenderType.Female, GenderType.Male};
                case GenderRestrictionType.MaleOnly:
                    return new List<GenderType> {GenderType.Male};
                case GenderRestrictionType.FemaleOnly:
                    return new List<GenderType> {GenderType.Female};
            }

            return new List<GenderType>();
        }


        /// <summary>
        ///     gets the first scheduled date for the activity, also gets the start time
        /// </summary>
        /// <param name="dc"></param>
        /// <returns></returns>
        public static DateTime? GetFirstDate(this Activity activity, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            var schedule = activity.GetSchedule(msgs, dc);

            if (schedule.IsNotEmpty())
            {
                if (schedule.IsOngoing)
                    return schedule.GetNextInstances(1, activity, msgs, dc).FirstOrDefault();

                return schedule.GetInstances(msgs: msgs, dc: dc).FirstOrDefault()?.StartDateTime;
            }

            return null;
        }

        /// <summary>
        ///     gets the last scheduled date for the activity, also gets the start time
        /// </summary>
        /// <param name="dc"></param>
        /// <returns></returns>
        public static DateTime? GetLastDate(this Activity activity, SystemMessages msgs, DatabaseConnection dc = null)
        {
            return activity.GetSchedule(msgs, dc)?.GetInstances(msgs: msgs, dc: dc).LastOrDefault()?.StartDateTime;
        }

        public static SystemMessages SendRegistrationSummaryReport(int activityId, string uiActivityLabel,
            bool getVolunteers, DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();
            var activity = Activity.Load(activityId, msgs, dc);
            var registrants = activity.GetRegistrants(msgs, dc);

            var subject = $"{activity.Name} Registration Summary Report";
            var body =
                $"<p>{activity.ContactName},</p><p>Here's your registration summary report for <strong>{activity.Name}</strong>. You have <strong>{registrants.Count.ToString()}</strong> registrations.</p>";

            msgs.AddRange(RegistrationService.EmailCsvReport(
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
                body, getVolunteers,
                msgs,
                dc));

            return msgs;
        }

        public static Account GetProvider(this Activity activity, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (activity.ProviderAccountId.HasValue)
                return Account.Load(activity.ProviderAccountId.Value, msgs, dc);

            return null;
        }

        public static void SetProvider(this Activity activity, Account providerAccount, WebUser loggedInUser, SystemMessages msgs, DatabaseConnection dc = null)
        {
            activity.ProviderAccountId = providerAccount.ID;
            activity.LastModifiedDate = DateTime.UtcNow;
            activity.LastModifiedBy = loggedInUser.ID;
            msgs.AddRange(activity.Save(dc));
        }

        public static void NotifyProviderOfNewActivity(this Activity activity, WebUser loggedInUser, SystemMessages msgs, DatabaseConnection dc = null)
        {
            if (!activity.ProviderAccountId.HasValue)
                return;

            var provider = activity.GetProvider(msgs, dc);
            var hostAccount = activity.GetAccount(msgs, dc);
            var providerOwner = provider.GetAccountOwner(msgs, dc);

            if (provider.IsNotEmpty() && providerOwner.IsNotEmpty())
                msgs.AddRange(EmailHelper.SendProviderNewActivityNotification(
                    hostAccount,
                    provider,
                    providerOwner,
                    activity,
                    loggedInUser));
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

        public static ActivityDetails GetActivityDetails(this Activity activity, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return ActivityDetails.LoadByActivityId(activity.ID!.Value, msgs, dc: dc);
        }

        public static SystemMessages SetActivityDetails(this Activity activity, string details, DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();
            var activityId = activity.ID!.Value;
            var existingDetails = ActivityDetails.LoadByActivityId(activityId, msgs, dc);
            if (existingDetails != null && existingDetails.Value != details)
            {
                existingDetails.Value = details;
                msgs.AddRange(existingDetails.Save(dc));
            }
            else if (existingDetails is null)
            {
                var newDetails = new ActivityDetails {ActivityId = activityId, Value = details};
                msgs.AddRange(newDetails.Save(dc));
            }

            return msgs;
        }

        public static ActivityGroup GetActivityGroup(this Activity activity, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (activity.ActivityGroupId.HasValue)
                return ActivityGroup.Load(activity.ActivityGroupId.Value, msgs, dc: dc);

            return null;
        }

        public static ActivityGroup GetActivityGroup(this Activity activity, int AccountId, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            if (activity.IsNotEmpty() && activity.ActivityGroupId.HasValue)
                return ActivityGroup.LoadByIdByAccountId(activity.ActivityGroupId.Value, AccountId, msgs, dc: dc);

            return null;
        }

        public static ActivitySchedule GetSchedule(this Activity activity, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            msgs = msgs ?? new SystemMessages();

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

        public static List<ActivityMessage> GetMessages(this Activity activity, int? start, int? length, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            return ActivityMessage.LoadByActivityId(activity.ID.Value, start, length, msgs, dc);
        }

        public static int GetMessagesCount(this Activity activity, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return ActivityMessage.GetCount(
                criterias: new Hashtable {{"ActivityId", activity.ID.Value}},
                msgs: msgs,
                dc: dc);
        }

        public static bool IsOpenUpcomingOrRecentlyClosed(this Activity activity, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var regPeriod = activity.GetActiveRegistrationPeriod(msgs, dc);

            if (regPeriod.IsEmpty())
                return false;

            var account = activity.GetAccount(msgs, dc);

            if (regPeriod.IsOpen(account, msgs, dc) || regPeriod.WillOpen(account, msgs, dc))
                return true;

            if (regPeriod.IsRecentlyClosed(account, msgs, dc))
                return true;

            return false;
        }

        public static bool IsOpen(this Activity activity, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var regPeriod = activity.GetActiveRegistrationPeriod(msgs, dc);

            if (regPeriod.IsEmpty())
                return false;

            var account = activity.GetAccount(msgs, dc);
            if (regPeriod.IsOpen(account, msgs, dc))
                return true;

            return false;
        }

        public static bool IsOpenOrUpcoming(this Activity activity, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            var regPeriod = activity.GetActiveRegistrationPeriod(msgs, dc);

            if (regPeriod.IsEmpty())
                return false;

            var account = activity.GetAccount(msgs, dc);
            if (regPeriod.IsOpen(account, msgs, dc) || regPeriod.IsFuture(msgs, dc))
                return true;

            return false;
        }

        public static Account GetLocationOwner(this Activity activity, SystemMessages msgs,
            DatabaseConnection dc = null)
        {
            if (activity.IsNotEmpty())
            {
                var location = activity.GetLocation(msgs, dc);
                return location?.GetHostAccount(msgs, dc);
            }

            return null;
        }

        public static void RequestToPublish(this Activity activity, Account locationOwner, WebUser loggedInUser,
            SystemMessages msgs, DatabaseConnection dc = null)
        {
            if (locationOwner.IsNotEmpty() && locationOwner.HasAccountOwner)
            {
                var accountOwner = locationOwner.GetAccountOwner(msgs, dc);
                msgs.AddRange(
                    EmailHelper.SendPublishRequestNotification(activity, locationOwner, accountOwner, loggedInUser));
            }
        }

        public static void EmailInvitedContacts(int id, int periodId, List<int> contactIds, string emailmessage,
            string inviteUrl, int loggedInUserId, SystemMessages msgs, DatabaseConnection dc = null)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();
            var activity = Activity.Load(id, msgs, dc);
            var account = activity.GetAccount(msgs, dc);
            var loggedInUser = WebUser.Load(loggedInUserId, msgs, dc);
            var contacts = Registrant.LoadList(contactIds, msgs, dc);
            var period = RegistrationPeriod.Load(periodId, msgs, dc);

            if (contacts.IsNotEmpty() && contacts.Any())
                contacts.ForEach(contact =>
                {
                    EmailHelper.SendActivityInviteEmail(
                        contact,
                        account,
                        activity,
                        period,
                        loggedInUser,
                        emailmessage,
                        msgs);
                });
        }

        public static void FinalizeRegistrations(this Activity activity, WebUser loggedInUser, SystemMessages msgs,
            DatabaseConnection dc = null)
        {
            activity.RegistrationFinalized = true;
            activity.LastModifiedBy = loggedInUser?.ID;
            activity.LastModifiedDate = DateTime.UtcNow;
            msgs.AddRange(activity.Save(dc));

            var ras = activity.GetRegistrantActivities(msgs, dc);

            ras.ForEach(ra =>
            {
                if (ra.AssignmentTypeId == RegistrationAssignmentType.ReservedPendingPayment)
                    msgs.AddRange(ra.SendPaymentReminder());
            });
        }

        public static bool RestrictByGrade(this Activity activity)
        {
            return activity.AgeStart.IsEmpty();
        }

        public static (DateTime? paymentStart, DateTime? paymentEnd) FinalizeStartAndEndDateForSubscription(
            this Activity activity, BillingPeriodType billingPeriodType, DayOfWeek dayOfWeek, DateTime dayOfMonth,
            SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (!activity.HasSchedule)
                return (null, null);

            var schedule = activity.GetSchedule(msgs, dc);
            var startDate = schedule.GetUpcomingInstance(msgs, dc).FirstOrDefault().StartDateTime.Value;
            var endDate = schedule.EndDate.Value;

            if (billingPeriodType == BillingPeriodType.Week)
            {
                //.. if activity schedule starts on November 1st, 2021(Monday) ends on December 1st, 2021(Thursday) and provide choose weekly billing every sunday, 
                //.. that would mean the parent would be first billed on Sunday Oct 31st and last billed on Sunday Nov 27th.
                //.. but if start date is of today or past, we will find next sunday or user's choosen day                
                var startingDay = startDate.AddDays(-1).IsOnOrBefore(DateTime.UtcNow)
                    ? startDate.AddDays(((int) dayOfWeek - (int) startDate.DayOfWeek + 7) % 7)
                    : startDate.AddDays(((int) dayOfWeek - (int) startDate.DayOfWeek - 7) % 7);

                //int daysUntilLastPayment = ((int)dayOfWeek - (int)endDate.DayOfWeek - 7) % 7;
                //var endingDay = endDate.AddDays(daysUntilLastPayment);
                return (paymentStart: startingDay, paymentEnd: endDate);
            }
            else if (billingPeriodType == BillingPeriodType.Month)
            {
                //.. if provider chooses monthly payment cycle in this case with date November 5th,
                //.. that would mean the parent would be first billed on November 5th.                

                //.. if activity is in future
                //var endingMonth = endDate.Month;
                //if (schedule.StartDate.Value.IsAfter(DateTime.Now))
                //    endingMonth += 1;

                var startingDay = new DateTime(endDate.Year, startDate.Month, dayOfMonth.Day);
                //var endingDay = new DateTime(endDate.Year, endingMonth, dayOfMonth.Day);

                return (paymentStart: startingDay.IsAfter(DateTime.UtcNow) ? startingDay : DateTime.UtcNow,
                    paymentEnd: endDate);
            }

            return (null, null);
        }

        public static void RemoveActivitySchedule(this Activity activity, WebUser currentUser, Account currentAccount,
            SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            msgs = msgs ?? new SystemMessages();

            if (currentAccount.HasActivityPermissions(activity, msgs))
            {
                var schedule = activity.GetSchedule(msgs, dc);

                if (schedule.IsNotEmpty())
                {
                    activity.ActivityScheduleId = null;
                    activity.LastModifiedBy = currentUser.ID.Value;
                    activity.LastModifiedDate = DateTime.UtcNow;
                    msgs.AddRange(activity.Save(dc));

                    return;
                }
            }

            msgs.Add("Error Removing Schedule", MessageType.UserError);
        }

        public static List<RegistrationAssignmentType> GetRegistrationAssignmentTypes(this Activity activity)
        {
            var dropdownOptions = new List<RegistrationAssignmentType>
            {
                RegistrationAssignmentType.Reserved,
                RegistrationAssignmentType.WaitingList,
                RegistrationAssignmentType.Applied,
                RegistrationAssignmentType.Invited,
                RegistrationAssignmentType.Declined,
                RegistrationAssignmentType.ReservedPendingPayment,
                RegistrationAssignmentType.ReservedPendingParentalConsent,
            };

            if (activity.ActivityTypeId == ActivityType.VolunteerOpportunity)
            {
                dropdownOptions = new List<RegistrationAssignmentType>
                {
                    RegistrationAssignmentType.Reserved,
                    RegistrationAssignmentType.WaitingList,
                    RegistrationAssignmentType.ReservedPendingBackgroundCheck,
                };
            }
            
            else if (activity.ActivityTypeId == ActivityType.PermissionSlip)
            {
                dropdownOptions.Remove(RegistrationAssignmentType.WaitingList);
                dropdownOptions.Remove(RegistrationAssignmentType.Applied);
            }

            return dropdownOptions;
        }

        public static void ProcessActivityPayouts(this Activity activity, WebUser currentUser, Account account,
            SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            msgs.Add($"Checking payments for activity: ({activity.UniqueName}).", MessageType.Success);

            var transactions = new List<Transaction>();
            var activityPayments = ActivityPayment.LoadByParticipationObject(ParticipationObjectId: activity.ID.Value,
                ParticipationObjectTypeId: ParticipationObjectType.Activity,
                IsDeleted: false,
                msgs: msgs,
                dc: dc);

            // load only those transactions which aren't payed out yet
            foreach (var activityPayment in activityPayments)
            {
                var transaction =
                    activityPayment.CheckTransactionForPayout(activityUniqueName: activity.UniqueName, msgs: msgs,
                        dc: dc);
                if (transaction.IsNotEmpty())
                    transactions.Add(transaction);
            }

            if (transactions.Any())
                PayoutService.ProcessManualPayouts(transactions: transactions, activityUniqueName: activity.UniqueName,
                    account: account, currentUser: currentUser, msgs: msgs, dc: dc);
        }
    }
}