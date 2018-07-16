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

            string origUniqueName = reg.Replace(name, "").MaxChars(25);
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

        public static int GetSeatsTaken(this Activity activity, DatabaseConnection dc = null)
        {
            return activity.GetRegistrantCountByApplicationType(RegistrationAssignmentType.Reserved, dc: dc);
        }

        public static int GetWaitingListSeatsTaken(this Activity activity, DatabaseConnection dc = null)
        {
            return activity.GetRegistrantCountByApplicationType(RegistrationAssignmentType.WaitingList, dc: dc);
        }

        public static int GetSeatsRemaining(this Activity activity, DatabaseConnection dc = null)
        {
            var rtn = 0;
            if (activity.SeatsAvailable.HasValue)
                rtn = activity.SeatsAvailable.Value - activity.GetRegistrantCountByApplicationType(RegistrationAssignmentType.Reserved, dc: dc);

            return Math.Max(0, rtn);
        }

        public static int GetWaitingListSeatsRemaining(this Activity activity, DatabaseConnection dc = null)
        {
            var rtn = 0;
            if (activity.WaitingListSeats.HasValue)
                rtn = activity.WaitingListSeats.Value - activity.GetRegistrantCountByApplicationType(RegistrationAssignmentType.WaitingList, dc: dc);

            return Math.Max(0, rtn);
        }

        public static PaymentPlan AddPaymentPlan(this Activity activity, decimal costPerPeriod, BillingPeriodType billingPeriodType, int totalPeriods, WebUser loggedInUser, SystemMessages msgs, DatabaseConnection dc = null)
        {
            return PaymentPlanService.CreateNew(
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
            return SubscriptionPlanService.CreateNew(
                activity.GetAccount(msgs, dc), 
                activity,
                costPerPeriod, 
                billingPeriodType,
                doesAutoRenew, 
                loggedInUser, 
                msgs, 
                dc);
        }

        public static List<PaymentPlan> GetPaymentPlans(this Activity activity, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return PaymentPlan.LoadByParticipationObject(activity.ID.Value, ParticipationObjectType.Activity, msgs, dc);
        }

        public static List<SubscriptionPlan> GetSubscriptionPlans(this Activity activity, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return SubscriptionPlan.LoadByParticipationObject(activity.ID.Value, ParticipationObjectType.Activity, msgs, dc);
        }

        public static void CreateSampleActivity(Account account, SystemMessages msgs, DatabaseConnection dc = null)
        {
            var gradeLevels = GradeLevel.Load(msgs: msgs, dc: dc);

            var schedule = ActivityScheduleService.CreateNew(
                  scheduleName: "Sample Schedule",
                  details: "",
                  repeatsWeekly: true,
                  startDate: DateTime.UtcNow.AddWeeks(1),
                  endDate: DateTime.UtcNow.AddWeeks(5),
                  dailyStartTime: DateTime.UtcNow.Date.AddHours(15),
                  dailyEndTime: DateTime.UtcNow.Date.AddHours(17),
                  scheduleDates: null,
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
                    accountId: account.ID.Value,
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
                AcceptCreditCard = acceptCreditCard,
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

        //public static SystemMessages UpdateValues(this Activity activity, string activityName, string activityDetails, int seatsAvailable, int? waitingListSeatsAvailable, string contactName, string contactPhone, string contactEmail, decimal? costPerRegistrant, string costDescription, bool acceptCreditCard, bool allowToPayLater, RegistrationMode registrationMode, int? accountId, int? scheduleId, int? locationId, int? groupId, int? minGradeLevelId, int? maxGradeLevelId, WebUser loggedInUser, DatabaseConnection dc = null)
        //{
        //    var msgs = new SystemMessages();

        //    activity.AcceptCreditCard = acceptCreditCard;
        //    activity.AllowToPayLater = allowToPayLater;
        //    activity.AccountId = accountId;
        //    activity.GenderRestriction = GenderRestrictionType.None;
        //    activity.IsPublished = false;
        //    activity.ActivityTypeId = ActivityType.Activity;
        //    activity.ActivityScheduleId = scheduleId;
        //    activity.ActivityGroupId = groupId;
        //    activity.LocationId = locationId;
        //    activity.GradeLevelStart = minGradeLevelId;
        //    activity.GradeLevelEnd = maxGradeLevelId;
        //    activity.ContactName = contactName;
        //    activity.ContactPhone = contactPhone;
        //    activity.ContactEmail = contactEmail;
        //    activity.CostDescription = costDescription;
        //    activity.CostPerRegistrant = costPerRegistrant;
        //    activity.LastModifiedBy = loggedInUser?.ID.IfEmpty(0);
        //    activity.LastModifiedDate = DateTime.UtcNow;
        //    activity.CreateDate = DateTime.UtcNow;
        //    activity.CreatedBy = loggedInUser?.ID.IfEmpty(0);
        //    activity.Name = activityName;
        //    activity.RegistrationModeId = registrationMode;
        //    activity.SeatsAvailable = seatsAvailable;
        //    activity.WaitingListSeats = waitingListSeatsAvailable;
        //    activity.UniqueName = ActivityService.GenerateUniqueName(activityName, null, accountId, msgs, dc);
        //    activity.Details = activityDetails;

        //    msgs.AddRange(activity.Save(dc));

        //    return msgs;
        //}
    }
}
