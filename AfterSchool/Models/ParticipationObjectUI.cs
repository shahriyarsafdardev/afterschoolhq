using System;
using BasicLibrary;
using System.Linq;
using System.Collections.Generic;
using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using Twilio.AspNet.Common;

namespace AfterSchool.Models
{
    public static class ParticipationObjectUI
    {
        public static string PrintAgeGradeRestrictions(ParticipationObject participationObject) 
        {
            if (participationObject.RestrictByGrade())
                return PrintGradeLevels(participationObject);
            else
                return PrintAges(participationObject);
        }

        public static string PrintGradeLevels(ParticipationObject participationObject)
        {
            if (participationObject.MinGradeId.HasValue && participationObject.MaxGradeId.HasValue)
            {
                if (participationObject.MinGradeId.Value == participationObject.MaxGradeId.Value)
                    return $"Grade: {participationObject.GetMinGradeLevel().Name}";

                return $"Grades: {participationObject.GetMinGradeLevel().Name} to {participationObject.GetMaxGradeLevel().Name}";
            }

            return "All Grades and Ages";
        }

        public static string PrintAges(ParticipationObject participationObject)
        {
            if (participationObject.AgeStart.HasValue && participationObject.AgeEnd.HasValue)
            {
                if (participationObject.AgeStart.Value == participationObject.AgeEnd.Value)
                    return $"Age: {participationObject.AgeStart.Value}";

                return $"Ages: {participationObject.AgeStart.Value} to {participationObject.AgeEnd.Value}";
            }
                

            return "All Grades and Ages";
        }

        public static string PrintLocation(ParticipationObject participationObject)
        {
            if (participationObject.Location.IsNotEmpty())
                return participationObject.Location.FormatAddress();

            return "No Location Available";
        }

        public static string PrintLocation(Location location)
        {
            if (location.IsNotEmpty())
                return location.FormatAddress();

            return "No Location Available";
        }

        public static string PrintRegistrationFee(ParticipationObject participationObject, bool isCheckoutCost)
        {
            if (participationObject.Cost.HasValue)
            {
                if (isCheckoutCost)
                    return participationObject.GetCheckoutCostWithConvenienceFee().ToCurrency();

                return participationObject.Cost.ToCurrency();
            }

            return "Free";
        }

        public static string PrintPlanFee(SubscriptionPlan plan, Account ownerAccount, bool isCheckoutCost)
        {
            if (isCheckoutCost)
                return plan.GetTotalCostWithConvenienceFee(ownerAccount).ToCurrency();

            return plan.CostPerPeriod.ToCurrency();
        }

        public static string PrintMinPlanFee(SubscriptionPlan plan, Account ownerAccount, bool isCheckoutCost)
        {
            if (isCheckoutCost)
                return plan.GetMinCostWithConvenienceFee(ownerAccount).ToCurrency();

            return (plan.CostPerPeriod.Value / plan.GetParticipationObject().SeatsAvailable.Value).ToCurrency();
        }

        public static string PrintMaxPlanFee(SubscriptionPlan plan, Account ownerAccount, bool isCheckoutCost)
        {
            if (isCheckoutCost)
                return plan.GetMaxCostWithConvenienceFee(ownerAccount).ToCurrency();

            return (plan.CostPerPeriod.Value / (plan.GetParticipationObject().MinSeatsRequired ?? 1)).ToCurrency();
        }

        public static List<string> PrintSubscriptions(ParticipationObject participationObject)
        {
            if (participationObject.SubscriptionPlans.Any())
                return participationObject.SubscriptionPlans.Select(plan => $"{plan.CostPerPeriod}/{plan.BillingPeriodTypeId}").ToList();

            return new List<string>();
        }

        public static List<string> PrintPaymentPlans(ParticipationObject participationObject)
        {
            if (participationObject.PaymentPlans.Any())
                return participationObject.PaymentPlans.Select(plan => 
                        $"{plan.CostPerPeriod}/{plan.BillingPeriodTypeId} for {plan.TotalPeriods} {plan.BillingPeriodTypeId.ToString().ToLower()}"
                    ).ToList();

            return new List<string>();
        }

        public static string PrintDaysOfWeekOrStartDate(ParticipationObject participationObject, bool longFormat = false)
        {
            if (participationObject.Schedule.IsNotEmpty())
            {
                if (longFormat)
                    return participationObject.Schedule.GetDaysOfTheWeek().OrderBy(x => (int)x).Select(x => x.ToString()).ToStringList(",");

                return participationObject.Schedule.GetDaysOfTheWeek().OrderBy(x => (int)x).Select(x => x.ToString().MaxChars(2)).ToStringList(",");
            }

            if (participationObject.StartDate.HasValue)
            {
                if (longFormat)
                    return $"Starts: {participationObject.StartDate.Value.ToLongDateTimeString()}";

                return $"Starts: {participationObject.StartDate.Value.ToString("M/d")}";
            }

            return "No Schedule";
        }

        public static string PrintDaysOfWeekList(ParticipationObject participationObject)
        {
            if (participationObject.Schedule.IsNotEmpty())
                return participationObject.Schedule.GetDaysOfTheWeek().OrderBy(x => (int)x).Select(x => x.ToString().MaxChars(2)).ToStringList(",");

            return "";
        }

        public static string PrintGenderRestrictions(ParticipationObject participationObject)
        {
            switch(participationObject.GenderRestriction)
            {
                case GenderRestrictionType.FemaleOnly:
                    return "Girls Only";
                case GenderRestrictionType.MaleOnly:
                    return "Boys Only";
            }
            return "Co-Ed";
        }

        public static string PrintRegistrationPeriod(ParticipationObject participationObject)
        {
            if (participationObject.RegistrationPeriod.IsNotEmpty())
            {
                var account = participationObject.OwnerAccount;

                if (participationObject.RegistrationPeriod.WillOpen(account))
                    return $"<span style=\"display:inline-block;\">Registration will open</span> <span style=\"display:inline-block;\">{participationObject.RegistrationPeriod.StartDateTime.ToShortDateString()} {participationObject.RegistrationPeriod.StartDateTime.Value.ToShortTimeString()}</span>";
                else if (participationObject.RegistrationPeriod.IsOpen(account))
                {
                    if (participationObject.IsContinuousEnrollmentEnabled)
                        return "<span style=\"display:inline-block;\">Registration will remain open</span>";
                    else
                        return $"<span style=\"display:inline-block;\">Registration will close</span> <span style=\"display:inline-block;\">{participationObject.RegistrationPeriod.EndDateTime.ToShortDateString()} {participationObject.RegistrationPeriod.EndDateTime.Value.ToShortTimeString()}</span>";
                }

            }
            return "Registration is closed";
        }
    }
}