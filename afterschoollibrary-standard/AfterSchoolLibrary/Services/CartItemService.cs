using AfterSchoolLibrary.Helpers;
using AfterSchoolLibrary.Models;
using BasicLibrary;
using BasicLibrary.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

namespace AfterSchoolLibrary.Services
{
    public static class CartItemService
    {
        public static ActivityGroup GetActivityGroup(this CartItem cartItem, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return ActivityGroup.Load(cartItem.ParticipationObjectId.Value, msgs, dc);
        }
        
        public static AccountActivityGroup GetAccountActivityGroup(this CartItem cartItem, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return AccountActivityGroup.Load(cartItem.ParticipationObjectId.Value, msgs, dc);
        }

        public static ParticipationObject GetParticipationObject(this CartItem cartItem, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (cartItem.ParticipationObjectTypeId == ParticipationObjectType.Activity)
                return cartItem.GetActivity(msgs, dc).GetParticipationObject(msgs, dc);
            else if (cartItem.ParticipationObjectTypeId == ParticipationObjectType.ActivityGroup)
                return cartItem.GetActivityGroup(msgs, dc).GetParticipationObject(msgs, dc);
            else if (cartItem.ParticipationObjectTypeId == ParticipationObjectType.AccountActivityGroup)
                return cartItem.GetAccountActivityGroup(msgs, dc).GetParticipationObject(msgs, dc);

            return null;
        }

        public static bool AreAllCartItemsValid(PurchaseableObjectType purchaseableObjectType, int participationObjectId, List<CartItem> cartItems , SystemMessages msgs, DatabaseConnection dc = null)
        {
            if (purchaseableObjectType is PurchaseableObjectType.Activity)
            {
                if (cartItems.Count == 0)
                    return true;
                var newActivity = Activity.Load(participationObjectId, msgs, dc);
                foreach (var item in cartItems)
                {
                    var cartActivity = Activity.Load(item.ParticipationObjectId ?? 0, msgs, dc);
                    if (!ValidateActivityRestriction(newActivity, cartActivity, msgs))
                        return false;
                    if (!ValidateActivityBySchedule(newActivity, cartActivity, msgs, dc))
                        return false;
                }
            }
            return true;
        }

        public static bool ValidateActivityRestriction(Activity activity1, Activity activity2, SystemMessages msgs)
        {
            var isRestrictedByGrade = activity1.IsRestrictedByGrade() || activity2.IsRestrictedByGrade();
            var isRestrictedByAge = activity1.IsRestrictedByAge() || activity2.IsRestrictedByAge();

            var isValidAgeRestriction = activity1.IsRestrictedByAge() && activity2.IsRestrictedByAge();
            var isValidGradeRestriction = activity1.IsRestrictedByGrade() && activity2.IsRestrictedByGrade();

            var gradeValidated = false;
            var ageValidated = false;

            if (isValidAgeRestriction || isValidGradeRestriction)
            {
                if (isRestrictedByGrade)
                    gradeValidated = OverlappingUtilities.HasOverlappingVectorRanges(
                        new Vector2 { X = activity1.GradeLevelStart ?? 0, Y = activity1.GradeLevelEnd ?? 0 },
                        new Vector2 { X = activity2.GradeLevelStart ?? 0, Y = activity2.GradeLevelEnd ?? 0 }
                        );
                else if (isRestrictedByAge)
                    ageValidated = OverlappingUtilities.HasOverlappingVectorRanges(
                        new Vector2 { X = activity1.AgeStart ?? 0, Y = activity1.AgeEnd ?? 0 },
                        new Vector2 { X = activity2.AgeStart ?? 0, Y = activity2.AgeEnd ?? 0 }
                        );
                else
                    ageValidated = gradeValidated = false;
            }

            bool genderValidated = ValidateGender(activity1, activity2);
            var errorMsg = "";
            if (!gradeValidated && isRestrictedByGrade)
                errorMsg += "Grade";
            if (!ageValidated && isRestrictedByAge)
                errorMsg += errorMsg.IsNotEmpty() ? ", Age" : "Age";
            if (!genderValidated)
                errorMsg += errorMsg.IsNotEmpty() ? " & Gender" : "Gender";
            if (errorMsg.IsNotEmpty())
                msgs.Add(new SystemMessage($"Conflicting Activity {errorMsg} with cart items!", MessageType.UserError));
            return genderValidated && (ageValidated || gradeValidated);
        }

        public static bool ValidateActivityBySchedule(Activity activity1, Activity activity2, SystemMessages msgs, DatabaseConnection dc = null)
        {
            var hasError = false;
            if (activity1.HasSchedule || activity2.HasSchedule)
            {
                if (activity1.HasSchedule && activity2.HasSchedule)
                {
                    var activity1Schedule = ActivitySchedule.Load(activity1.ActivityScheduleId.Value, msgs, dc);
                    var activity2Schedule = ActivitySchedule.Load(activity2.ActivityScheduleId.Value, msgs, dc);
                    if (activity1Schedule.RepeatsWeekly && activity2Schedule.RepeatsWeekly)
                    {
                        var schedule1Range = new DateRange()
                        {
                            StartDate = activity1Schedule.StartDate ?? DateTime.MinValue,
                            EndDate = activity1Schedule.EndDate ?? DateTime.MaxValue
                        };
                        var schedule2Range = new DateRange()
                        {
                            StartDate = activity2Schedule.StartDate ?? DateTime.MinValue,
                            EndDate = activity2Schedule.EndDate ?? DateTime.MaxValue
                        };
                        if (OverlappingUtilities.HasOverlappingDateRanges(schedule1Range, schedule2Range))
                        {
                            var activity1Days = activity1Schedule.GetDaysOfTheWeek(msgs, dc);
                            var activity2Days = activity2Schedule.GetDaysOfTheWeek(msgs, dc);
                            if (OverlappingUtilities.HasOverlappingLists(activity1Days, activity2Days))
                            {
                                var timeRange1 = new TimeRange()
                                {
                                    StartTime = (activity1Schedule.DailyStartTime ?? DateTime.MinValue).TimeOfDay,
                                    EndTime = (activity1Schedule.DailyEndTime ?? DateTime.MaxValue).TimeOfDay
                                };
                                var timeRange2 = new TimeRange()
                                {
                                    StartTime = (activity2Schedule.DailyStartTime ?? DateTime.MinValue).TimeOfDay,
                                    EndTime = (activity2Schedule.DailyEndTime ?? DateTime.MaxValue).TimeOfDay
                                };
                                if (OverlappingUtilities.HasOverlappingTimeRanges(timeRange1, timeRange2))
                                    hasError = true;
                            }
                        }
                    }
                    else if (!activity1Schedule.RepeatsWeekly && !activity2Schedule.RepeatsWeekly)
                    {
                        var sessionInstance1 = activity1Schedule.GetUpcomingInstance(msgs, dc);
                        var sessionInstance2 = activity2Schedule.GetUpcomingInstance(msgs, dc);
                        var overlappingInstance = OverlappingUtilities.GetOverlappingInstanceDates(sessionInstance1, sessionInstance2);
                        if (OverlappingUtilities.HasOverlapingInstanceTimes(overlappingInstance))
                            hasError = true;
                    }
                    else if (!activity1Schedule.RepeatsWeekly || !activity2Schedule.RepeatsWeekly)
                    {
                        var sessionInstance = (!activity1Schedule.RepeatsWeekly ? activity1Schedule : activity2Schedule).GetUpcomingInstance(msgs, dc);
                        var dateRangeActivity = activity1Schedule.RepeatsWeekly ? activity1Schedule : activity2Schedule;
                        var dateRange = new DateRange()
                        {
                            StartDate = dateRangeActivity.StartDate ?? DateTime.MinValue,
                            EndDate = dateRangeActivity.EndDate ?? DateTime.MaxValue
                        };
                        var matchingDates = sessionInstance.Where(ins => (ins.StartDateTime?.Date ?? DateTime.MinValue).IsInRange(dateRange));
                        hasError = matchingDates.Any(dates => OverlappingUtilities.InstanceScheduleTimeOverlaps(dates, dateRangeActivity));
                    }
                    else
                        hasError = true;
                }
            }
            if (hasError)
                msgs.Add(new SystemMessage("Conflicting Activity Schedule with cart items!", MessageType.UserError));
            return !hasError;
        }

        public static bool ValidateGender(Activity activity1, Activity activity2)
        {
            if (activity1.IsGenderEnforced && activity2.IsGenderEnforced)
            {
                var activity1AllowedGenders = activity1.GetAllowedGenders();
                return activity2.GetAllowedGenders().All(gender => activity1AllowedGenders.Contains(gender));
            }
            else if (activity1.IsGenderEnforced || activity2.IsGenderEnforced)
                return false;
            return true;
        }
    }
}
