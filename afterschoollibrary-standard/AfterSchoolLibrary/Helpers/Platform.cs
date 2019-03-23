using System;

namespace AfterSchoolLibrary.Billing
{
    public class Platform
    {
        public const decimal ConveniencePercentFee = .05M;
        public const decimal ConvenienceFlatFee = .99M;

        public static decimal GetCreditCardFee(decimal cost, bool feeTakenOutOfCost)
        {
            if (cost <= 0)
                return 0;

            return Math.Ceiling(StripeHelper.GetStripeFee(cost, feeTakenOutOfCost)) - .01M;
        }

        public static decimal GetConvenienceFee(decimal cost, bool feeTakenOutOfCost)
        {
            var fee = 0M;

            if (cost <= 0)
                return 0;

            if (feeTakenOutOfCost)
                fee = (cost * ConveniencePercentFee) + ConvenienceFlatFee;
            else
                fee = ((cost + ConvenienceFlatFee) / (1 - ConveniencePercentFee)) - cost;

            // always round up to nearest dollar, then subtract a penny, 
            // result should always be 3.99 or 2.99, etc.
            return Math.Ceiling(fee) - .01M;
        }

        public static decimal GetTotalFees(decimal cost, bool feeTakenOutOfCost)
        {
            return GetConvenienceFee(cost, feeTakenOutOfCost) + GetCreditCardFee(cost, feeTakenOutOfCost);
        }
    }
}
