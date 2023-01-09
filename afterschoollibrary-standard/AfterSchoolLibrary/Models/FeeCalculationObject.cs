using AfterSchoolLibrary.Billing;
using AfterSchoolLibrary.Services;
using BasicLibrary;
using BasicLibrary.DataAccess;
using System;
using System.Collections.Generic;
using System.Text;

namespace AfterSchoolLibrary.Models
{
    public class FeeCalculationObject
    {
        public decimal Cost { get; set; }
        public Account BillingAccount { get; set; }
        public Activity Activity { get; set; }
        public decimal ConveniencePercentFee { get; private set; }
        public decimal ConvenienceFlatFee { get; private set; }

        private bool? _IsConvenienceFeeTakenOutOfCost { get; set; }

        public FeeCalculationObject(decimal cost, decimal convenienceFlatFee, decimal conveniencePercentFee, bool isConvenienceFeeTakenOutOfCost)
        {
            Cost = cost;
            ConvenienceFlatFee = convenienceFlatFee;
            ConveniencePercentFee= conveniencePercentFee;
            _IsConvenienceFeeTakenOutOfCost = isConvenienceFeeTakenOutOfCost;
        }

        public FeeCalculationObject(decimal cost, Account billingAccount, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            Cost = cost;
            BillingAccount = billingAccount;
            LoadConvenienceFlatFee(msgs, dc);
            LoadConveniencePercentFee(msgs, dc);
        }

        public FeeCalculationObject(decimal cost, Account billingAccount, Activity activity, bool? isConvenienceFeeTakenOutOfCost, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            Cost = cost;
            BillingAccount = billingAccount;
            Activity = activity;
            _IsConvenienceFeeTakenOutOfCost = isConvenienceFeeTakenOutOfCost;
            LoadConvenienceFlatFee(msgs, dc);
            LoadConveniencePercentFee(msgs, dc);
        }

        public FeeCalculationObject(decimal cost, Account billingAccount, ParticipationObject participationObject, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            Cost = cost;
            BillingAccount = billingAccount;
            if (participationObject.IsActivity())
                Activity = participationObject.GetActivity();
            LoadConvenienceFlatFee(msgs, dc);
            LoadConveniencePercentFee(msgs, dc);
        }

        public FeeCalculationObject(decimal cost, Account billingAccount, bool isConvenienceFeeTakenOutOfCost, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            Cost = cost;
            BillingAccount = billingAccount;
            _IsConvenienceFeeTakenOutOfCost = isConvenienceFeeTakenOutOfCost;
            LoadConvenienceFlatFee(msgs, dc);
            LoadConveniencePercentFee(msgs, dc);
        }

        public bool IsConvenienceFeeTakenOutOfCost
        {
            get
            {
                if (_IsConvenienceFeeTakenOutOfCost.HasValue)
                    return _IsConvenienceFeeTakenOutOfCost.Value;

                if (Activity.IsNotEmpty())
                    return Activity.IsConvenienceFeeTakenOutOfCost;

                return BillingAccount.IsConvenienceFeeTakenOutOfCost;
            }
        }

        public bool IsPaymentTypeFeeTakenOutOfCost
        {
            get
            {
                return BillingAccount.IsCreditCardFeeTakenOutOfCost;
            }
        }

        public decimal GetConvenienceFee()
        {
            if (Cost <= 0)
                return 0;

            var fee = 0M;

            if (IsConvenienceFeeTakenOutOfCost)
                fee = (Cost * ConveniencePercentFee) + ConvenienceFlatFee;
            else
                fee = ((Cost + ConvenienceFlatFee) / (1 - ConveniencePercentFee)) - Cost;

            var finalFee = Math.Ceiling(fee * 2) / 2;

            if (Cost > 50)
                finalFee = Math.Ceiling(fee);

            return Math.Max(finalFee, 0.50M);
        }

        public decimal GetCreditCardFee()
        {
            if (Cost <= 0)
                return 0;

            // Because stripe will take their charge out of the total charge with the convenience fee
            var costWithConvenienceFee = Cost;

            if (!IsConvenienceFeeTakenOutOfCost)
                costWithConvenienceFee += GetConvenienceFee();

            var stripeFee = StripeHelper.CalculateStripeFee(costWithConvenienceFee, true);

            // no more rounding
            return stripeFee;
            
            // always round up to nearest dollar or .5
            // result should always be 3.50 or 4.00 or 2.50 or 3.00, etc.
            if (Cost <= 50)
                return (Math.Ceiling(stripeFee * 2) / 2); //  - .01M;

            return Math.Ceiling(stripeFee); // - .01M;
        }

        public decimal GetTotalFees(PaymentType paymentType)
        {
            return GetConvenienceFee() + (paymentType == PaymentType.CreditCard ? GetCreditCardFee() : 0);
        }

        public decimal GetCostWithConvenienceFee()
        {
            return Cost + (IsConvenienceFeeTakenOutOfCost ? 0 : GetConvenienceFee());
        }

        public decimal GetFinalTotalCost(PaymentType paymentType)
        {
            return GetCostWithConvenienceFee() + (paymentType == PaymentType.CreditCard ? GetCreditCardFee() : 0);
        }

        public decimal GetFinalTotalCostForPostPay(PaymentType paymentType)
        {
            //assuming that we have a ConvenienceFee applied
            return Math.Ceiling(Cost + (paymentType == PaymentType.CreditCard ? GetCreditCardFee() : 0));
        }

        public decimal GetAmountToProvider()
        {
            if (IsConvenienceFeeTakenOutOfCost)
                return Cost - GetConvenienceFee();

            return Cost;
        }



        private void LoadConveniencePercentFee(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var accountPackageFeature = BillingAccount.GetAccountPackageFeature(FeatureType.PaymentsPercentageFee, msgs, dc);
            if (accountPackageFeature != null)
                ConveniencePercentFee = accountPackageFeature.GetMaxValue();
        }
        private void LoadConvenienceFlatFee(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var accountPackageFeature = BillingAccount.GetAccountPackageFeature(FeatureType.PaymentsPercentageFlatFee, msgs, dc);
            if (accountPackageFeature != null)
                ConvenienceFlatFee = accountPackageFeature.GetMaxValue();

        }
    }
}
