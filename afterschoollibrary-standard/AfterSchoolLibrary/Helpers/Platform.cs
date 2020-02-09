using System;
using AfterSchoolLibrary.Models;
using BasicLibrary;

namespace AfterSchoolLibrary.Billing
{
    public class Platform
    {
        public const decimal ConveniencePercentFee = .05M;
        public const decimal ConvenienceFlatFee = 0M;

        public static decimal GetCreditCardFee(decimal cost, Account billingAccount)
        {
            if (cost <= 0)
                return 0;

            // Because stripe will take their charge out of the total charge with the convenience fee
            var costWithConvenienceFee = cost + GetConvenienceFee(cost, billingAccount);
            var stripeFee = StripeHelper.GetStripeFee(costWithConvenienceFee, billingAccount.IsCreditCardFeeTakenOutOfCost);

            // always round up to nearest dollar or .5
            // result should always be 3.50 or 4.00 or 2.50 or 3.00, etc.
            if (cost <= 50)
                return (Math.Ceiling(stripeFee * 2) / 2) - .01M;

            return Math.Ceiling(stripeFee) - .01M;
        }

        public static decimal GetConvenienceFee(decimal cost, Account billingAccount)
        {
            var fee = 0M;

            if (cost <= 0)
                return 0;

            fee = (cost * ConveniencePercentFee) + ConvenienceFlatFee;
            //if (billingAccount.IsConvenienceFeeTakenOutOfCost)
            //    fee = (cost * ConveniencePercentFee) + ConvenienceFlatFee;
            //else
            //    fee = ((cost + ConvenienceFlatFee) / (1 - ConveniencePercentFee)) - cost;

            // always round up to nearest dollar or .5
            // result should always be 3.50 or 4.00 or 2.50 or 3.00, etc. Max of $10
            // return Math.Min(Math.Ceiling(fee * 2) / 2, 10M);

            var finalFee = Math.Ceiling(fee * 2) / 2;

            if (cost > 50)
                finalFee = Math.Ceiling(fee);

            return (finalFee - .01M).KeepInRange(0.49M, 9.99M) + (Math.Ceiling(cost) - cost);
        }

        public static decimal GetTotalFees(decimal cost, Account billingAccount)
        {
            return GetConvenienceFee(cost, billingAccount) + GetCreditCardFee(cost, billingAccount);
        }

        public static decimal GetCostWithConvenienceFee(decimal cost, Account billingAccount)
        {
            return cost + GetConvenienceFee(cost, billingAccount);
        }

        public static decimal GetFinalTotalCost(decimal cost, Account billingAccount)
        {
            return cost + GetTotalFees(cost, billingAccount);
        }

        public static DateRange GetCurrentSchoolYear()
        {
            var startYear = DateTime.UtcNow.Year;

            if (DateTime.UtcNow.Month <= 6)
                startYear = DateTime.UtcNow.Year - 1;

            var startDate = $"07/01/{startYear}".ToDateTime();
            var endDate = startDate.AddYears(1).AddSeconds(-1);

            return new DateRange(startDate, endDate);
        }
    }
}
