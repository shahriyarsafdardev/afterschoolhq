using System;
using BasicLibrary;
using System.Linq;
using System.Collections.Generic;
using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;

namespace AfterSchool.Models
{
    public class ParticipationObjectUI
    {
        public static string PrintGradeLevels(ParticipationObject participationObject)
        {
            if (participationObject.MinGradeId.HasValue && participationObject.MaxGradeId.HasValue)
                return $"{participationObject.GetMinGradeLevel().Name} - {participationObject.GetMaxGradeLevel().Name}";

            return "All Grades";
        }

        public static string PrintLocation(ParticipationObject participationObject)
        {
            if (participationObject.Location.IsNotEmpty())
                return participationObject.Location.FormatAddress();

            return "No Location Available";
        }

        public static string PrintTuitionFee(ParticipationObject participationObject, bool displayCheckoutPrice)
        {
            if (participationObject.Cost.HasValue)
            {
                if (displayCheckoutPrice)
                    return participationObject.GetCheckoutTotalCost().ToCurrency();

                return participationObject.Cost.ToCurrency();
            }

            return "Free";
        }

        public static string PrintPlanFee(SubscriptionPlan plan, Account ownerAccount, bool displayCheckoutPrice)
        {
            if (displayCheckoutPrice)
                return AfterSchoolLibrary.Billing.Platform.GetFinalTotalCost(plan.CostPerPeriod.Value, ownerAccount).ToCurrency();

            return plan.CostPerPeriod.ToCurrency();
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

        public static string PrintDaysOfWeek(ParticipationObject participationObject)
        {
            if (participationObject.Schedule.IsNotEmpty())
                return participationObject.Schedule.GetDaysOfTheWeek().OrderBy(x => (int)x).Select(x => x.ToString().MaxChars(2)).ToStringList(",");

            return "No Schedule Available";
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
                    return $"<span style=\"display:inline-block;\">Registration will open</span> <span style=\"display:inline-block;\">{participationObject.RegistrationPeriod.StartDateTime.Value.ToLongDateString()} {participationObject.RegistrationPeriod.StartDateTime.Value.ToShortTimeString()}</span>";
                else if (participationObject.RegistrationPeriod.IsOpen(account))
                    return $"<span style=\"display:inline-block;\">Registration will close</span> <span style=\"display:inline-block;\">{participationObject.RegistrationPeriod.EndDateTime.Value.ToLongDateString()} {participationObject.RegistrationPeriod.EndDateTime.Value.ToShortTimeString()}</span>";
            }
            return "Registration is closed";
        }
    }
}