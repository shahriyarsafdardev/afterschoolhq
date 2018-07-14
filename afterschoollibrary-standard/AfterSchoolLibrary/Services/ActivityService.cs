using System;
using System.Collections.Generic;
using System.Text;
using BasicLibrary;
using BasicLibrary.DataAccess;
using AfterSchoolLibrary.Services;
using System.Text.RegularExpressions;

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
    }
}
