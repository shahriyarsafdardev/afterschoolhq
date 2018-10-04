using System;
using BasicLibrary;
using System.Linq;
using System.Collections.Generic;
using AfterSchoolLibrary.Models;

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

        public static string PrintTuitionFee(ParticipationObject participationObject)
        {
            if (participationObject.Cost.HasValue)
                return participationObject.Cost.ToCurrency();

            return "Free";
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
                if (participationObject.RegistrationPeriod.WillOpen())
                    return $"Registration will open {participationObject.RegistrationPeriod.StartDateTime.ToLongDateTimeString()}";
                else if (participationObject.RegistrationPeriod.IsOpen())
                    return $"Registration will close {participationObject.RegistrationPeriod.EndDateTime.ToLongDateTimeString()}";
            }
            return "Registration is closed";
        }
    }
}