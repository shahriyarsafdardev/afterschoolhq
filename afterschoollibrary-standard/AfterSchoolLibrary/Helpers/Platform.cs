using System;
using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;

namespace AfterSchoolLibrary.Billing
{
    public class Platform
    {
        public const decimal ConveniencePercentFee = .05M;
        public const decimal ConvenienceFlatFee = .99M;

        public static decimal GetCreditCardFee(decimal cost, Account billingAccount)
        {
            if (cost <= 0)
                return 0;

            // Because stripe will take their charge out of the total charge with the convenience fee
            var costWithConvenienceFee = cost + GetConvenienceFee(cost, billingAccount);

            // always round up to nearest dollar or .5
            // result should always be 3.50 or 4.00 or 2.50 or 3.00, etc.
            return (Math.Ceiling(StripeHelper.GetStripeFee(costWithConvenienceFee, billingAccount.IsCreditCardFeeTakenOutOfCost) * 2) / 2);
        }

        public static decimal GetConvenienceFee(decimal cost, Account billingAccount)
        {
            var fee = 0M;

            if (cost <= 0)
                return 0;

            if (billingAccount.IsConvenienceFeeTakenOutOfCost)
                fee = (cost * ConveniencePercentFee) + ConvenienceFlatFee;
            else
                fee = ((cost + ConvenienceFlatFee) / (1 - ConveniencePercentFee)) - cost;

            // always round up to nearest dollar or .5
            // result should always be 3.50 or 4.00 or 2.50 or 3.00, etc.
            return (Math.Ceiling(fee * 2) / 2);
        }

        public static decimal GetTotalFees(decimal cost, Account billingAccount)
        {
            return GetConvenienceFee(cost, billingAccount) + GetCreditCardFee(cost, billingAccount);
        }
    }
}
