using System;
using System.Collections.Generic;
using System.Text;
using BasicLibrary;
using BasicLibrary.DataAccess;
using AfterSchoolLibrary.Services;
using System.Text.RegularExpressions;
using System.Linq;

namespace AfterSchoolLibrary
{
    public static class ActivityService
    {
        public static Location GetLocation(this Activity activity, SystemMessages msgs = null, DatabaseConnection dc = null)
        {            
            if (activity.LocationId.HasValue)
                return Location.Load(activity.LocationId.Value, msgs, dc);

            return null;
        }

        public static ParticipationObject ToParticipationObject(this Activity activity, SystemMessages msgs = null, DatabaseConnection dc = null)
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
            return activity.GetRegistrantCountByAssignmentType(RegistrationAssignmentType.Reserved, msgs, dc);
        }

        public static int GetWaitingListSeatsTaken(this Activity activity, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return activity.GetRegistrantCountByAssignmentType(RegistrationAssignmentType.WaitingList, msgs, dc);
        }

        public static int GetSeatsRemaining(this Activity activity, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var rtn = 0;
            if (activity.SeatsAvailable.HasValue)
                rtn = activity.SeatsAvailable.Value - activity.GetRegistrantCountByAssignmentType(RegistrationAssignmentType.Reserved, msgs, dc);

            return Math.Max(0, rtn);
        }

        public static int GetWaitingListSeatsRemaining(this Activity activity, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var rtn = 0;
            if (activity.WaitingListSeats.HasValue)
                rtn = activity.WaitingListSeats.Value - activity.GetRegistrantCountByAssignmentType(RegistrationAssignmentType.WaitingList, msgs, dc);

            return Math.Max(0, rtn);
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
            return SubscriptionPlanService.CreateNewSubscription(
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
                    if (period.IsOpen(msgs, dc))
                        regOpenText = "Registration is open now.";
                    else if (period.WillOpen(msgs, dc))
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
                            Email.AppEmailAddress,
                            $"{account.Name} via {Utilities.AppName(account.IsAfterSchool)}"),
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
                            Email.GetAppSignature(account.IsAfterSchool, "invite_contacts")),
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

        public static bool HasTeamMatches(this Activity actitivy, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return Match.LoadByTeamId(actitivy.ID.Value).IsNotEmpty();
        }

        public static bool HasNoTeamMatches(this Activity actitivy, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return Match.LoadByTeamId(actitivy.ID.Value).IsEmpty();
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
                    Utilities.SystemErrorLog(emsgs);
                },
                dc: dc
            );

            return attendance;
        }

        private static void ClearEmptyAttendances(this Activity activity, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            //check for existing incomplete attendance and delete so that we're not just keeping a bunch of blank records
            var attendanceList = AfterSchoolLibrary.Attendance.LoadByActivityIdByIsCompleted(activity.ID.Value, false, msgs, dc);
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
    }
}
