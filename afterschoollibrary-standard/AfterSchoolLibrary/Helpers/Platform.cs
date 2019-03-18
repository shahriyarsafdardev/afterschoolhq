using System;

namespace AfterSchoolLibrary.Billing
{
    public class Platform
    {
        public static decimal GetPlatformCreditCardFee(decimal cost)
        {
            return Math.Ceiling(StripeHelper.GetStripeFee(cost)) - .01M;
        }

        public static decimal GetConvenienceFee(decimal cost)
        {
            return Math.Ceiling(cost * .05M) - .01M;
        }
    }
}
