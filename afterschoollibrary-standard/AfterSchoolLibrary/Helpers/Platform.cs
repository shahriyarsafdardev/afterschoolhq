using System;

namespace AfterSchoolLibrary.Billing
{
    public class Platform
    {
        public static decimal GetPlatformCreditCardFee(decimal cost)
        {
            if (cost > 0)
                return Math.Ceiling(StripeHelper.GetStripeFee(cost)) - .01M;

            return 0;
        }

        public static decimal GetConvenienceFee(decimal cost)
        {
            if (cost > 0)
                return Math.Ceiling(cost * .05M) - .01M;

            return 0;
        }
    }
}
