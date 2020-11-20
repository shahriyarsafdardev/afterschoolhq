using BasicLibrary;
using BasicLibrary.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using AfterSchoolLibrary.Services;

namespace AfterSchoolLibrary.Models
{
    public class CheckoutObject
    {
        public Registrant MasterRegistrant { get; set; }
        public Registration Registration { get; set; }
        public Coupon Coupon { get; set; }
        public SubscriptionPlan SubscriptionPlan { get; set; }
        public SubscriptionPlan PaymentPlan { get; set; }
        public SubscriptionPlan PostPayPlan { get; set; }
        public BankAccountPurchaseObject BankAccountPurchaseObject { get; set; }
        public CreditCardPurchaseObject CreditCardPurchaseObject { get; set; }
        public ParticipationObject ParticipationObject { get; set; }
        public Account PayingAccount { get; set; }
        public Account BillingAccount { get; set; }
        public int NumberOfSeats { get; set; }
        public List<Product> Products { get; set; }
        public bool SavePaymentInfo { get; set; }
        public PaymentType PaymentType { get; set; }
        /// <summary>
        /// override will sometimes be the manual payment or the subscription payment
        /// </summary>
        public decimal? OverrideAmount { get; set; }
        public decimal? ConvenienceFeeOverrideAmount { get; set; }
        public decimal? PaymentTypeFeeOverrideAmount { get; set; }
        public string StripeChargeId { get; set; }
        public Purchase Purchase { get; set; }
        public PaymentSourceType PaymentSourceType { get; set; }
        public RegistrantSubscriptionPlan RegistrantSubscriptionPlan { get; set; }
        public Account ConvenienceFeePayer { get; set; }
        public Account CreditCardFeePayer { get; set; }
        public DateTime Date { get; set; }

        public int? Quantity { get; set; }

        public Transaction Transaction { get; set; }

        public CheckoutObject()
        {
            Products = new List<Product>();
            Date = DateTime.UtcNow;
        }

        private string _description;
        public string Description
        {
            get
            {
                if (ParticipationObject.IsNotEmpty())
                    return $"{this.ParticipationObject.Name} Registration";

                return _description;
            }
            set
            {
                _description = value;
            }
        }

        public decimal GetStripeAmount(SystemMessages msgs, DatabaseConnection dc = null)
        {
            if (OverrideAmount.HasValue)
                return OverrideAmount.Value;

            return this.GetRegistrationCostWithFees(msgs, dc);
        }

        public bool HasStripeSourceId
        {
            get
            {
                return BankAccountPurchaseObject?.StripeSourceId != null || CreditCardPurchaseObject?.StripeSourceId != null;
            }
        }

        public string StripeSourceId
        {
            get
            {
                return BankAccountPurchaseObject?.StripeSourceId ?? CreditCardPurchaseObject?.StripeSourceId;
            }
        }

        public string StripeToken
        {
            get
            {
                return BankAccountPurchaseObject?.PlaidPublicToken ?? CreditCardPurchaseObject?.StripeToken;
            }
        }

        public bool IsPaying
        {
            get
            {
                return CreditCardPurchaseObject.IsNotEmpty() || BankAccountPurchaseObject.IsNotEmpty();
            }
        }

        public bool IsFree
        {
            get
            {
                return ParticipationObject.IsFree;
            }
        }   
        
        public bool IsRegistrationFree
        {
            get
            {
                return ParticipationObject.IsRegistrationFree;
            }
        }

        public decimal GetTotalCostToPurchaseTodayNoFees(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (PaymentPlan.IsNotEmpty())
                return PaymentPlanCostTodayNoFees + GetSubscriptionCostTodayNoFees(msgs, dc);

            return RegistrationCostNoFees + GetSubscriptionCostTodayNoFees(msgs, dc);
        }

        public decimal GetTotalCostWithFees(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (PaymentPlan.IsNotEmpty())
                return PaymentPlan.GetTotalCostWithFees(this.BillingAccount, msgs, dc) * PaymentPlan.TotalPeriods.Value;

            if (OverrideAmount.HasValue)
                return OverrideAmount.Value;

            return GetRegistrationCostWithFees(msgs, dc);
        }

        public decimal GetTotalCost()
        {
            if (PaymentPlan.IsNotEmpty())
                return PaymentPlan.CostPerPeriod.Value * PaymentPlan.TotalPeriods.Value;

            if (OverrideAmount.HasValue)
                return OverrideAmount.Value - PaymentTypeFeeOverrideAmount.Value;

            return RegistrationCostNoFees;
        }

        public decimal GetTotalCostToPurchaseTodayWithFees(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (PaymentPlan.IsNotEmpty())
                return GetPaymentPlanCostTodayWithFees(msgs, dc) + GetSubscriptionCostTodayWithFees(msgs, dc);

            return GetRegistrationCostWithFees(msgs, dc) + GetSubscriptionCostTodayWithFees(msgs, dc);
        }

        public decimal GetTotalFeesToday(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (PaymentPlan.IsNotEmpty())
                return GetPaymentPlanTotalFeesToday(msgs, dc) + GetSubscriptionTotalFeesToday(msgs, dc);

            return GetRegistrationTotalFees(msgs, dc) + GetSubscriptionTotalFeesToday(msgs, dc);
        }

        public decimal GetTotalConvenienceFeesToday(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (PaymentPlan.IsNotEmpty())
                return GetPaymentPlanCostTodayConvenienceFee(msgs, dc) + GetSubscriptionCostTodayConvenienceFee(msgs, dc);

            return GetRegistrationConvenienceFee(msgs, dc) + GetSubscriptionCostTodayConvenienceFee(msgs, dc);
        }

        public decimal GetTotalCreditCardFeesToday(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (PaymentPlan.IsNotEmpty())
                return GetPaymentPlanCostTodayCreditCardFee(msgs, dc) + GetSubscriptionCostTodayCreditCardFee(msgs, dc);

            return GetRegistrationCreditCardFee(msgs, dc) + GetSubscriptionCostTodayCreditCardFee(msgs, dc);
        }

        public decimal RegistrationCostNoFees
        {
            get
            {
                if (ParticipationObject.IsNotEmpty() && ParticipationObject.Cost.HasValue)
                {
                    var total = ParticipationObject.Cost.Value * NumberOfSeats;

                    // if coupon applyTo is `RegistrationFee`
                    // then apply coupon discount
                    if (Coupon.IsNotEmpty() && Coupon.ID.IsNotEmpty() && Coupon.DoesApplyToRegistrationFee())
                    {
                        var couponAmount = Coupon.Amount.Value;
                        if (Coupon.HasPercentageAmountType())
                            couponAmount *=  total;
                        return total - couponAmount;
                    }
                    return total;
                }

                return 0;
            }
        }

        public decimal GetRegistrationCostWithFees(SystemMessages msgs = null, DatabaseConnection dc = null)
        {           
            return GetRegistrationCostWithConvenienceFee(msgs, dc) +  GetRegistrationCreditCardFee(msgs, dc);
        }

        public decimal GetRegistrationCostWithConvenienceFee(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var activity = ParticipationObject.GetActivity();

            if (activity.IsConvenienceFeeTakenOutOfCost)
                return RegistrationCostNoFees;
            
            return GetRegistrationConvenienceFee(msgs, dc);
        }

        public decimal GetRegistrationCreditCardFee(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (!IsRegistrationFree && CreditCardPurchaseObject.IsNotEmpty())
                return new FeeCalculationObject(
                                RegistrationCostNoFees,
                                BillingAccount,
                                ParticipationObject,
                                msgs,
                                dc)
                            .GetCreditCardFee();

            return 0;
        }

        public decimal GetRegistrationConvenienceFee(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (!IsRegistrationFree)
                return new FeeCalculationObject(
                                RegistrationCostNoFees,
                                BillingAccount,
                                ParticipationObject,
                                msgs,
                                dc)
                            .GetConvenienceFee();

            return 0;
        }

        public decimal GetRegistrationTotalFees(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return GetRegistrationConvenienceFee(msgs, dc) + GetRegistrationCreditCardFee(msgs, dc);
        }

        public decimal GetSubscriptionCostTodayNoFees(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (SubscriptionPlan.IsNotEmpty() && (!SubscriptionPlan.SplitCostEvenly || ParticipationObject.RegistrationFinalized))
                return SubscriptionPlan.GetCostPerPeriodPerPerson(msgs, dc) * NumberOfSeats;

            return 0;
        }

        public decimal GetSubscriptionCostTodayWithFees(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return GetSubscriptionCostTodayNoFees(msgs, dc) +
                    GetSubscriptionCostTodayCreditCardFee(msgs, dc) +
                    GetSubscriptionCostTodayConvenienceFee(msgs, dc);
        }

        public decimal GetSubscriptionCostTodayWithConvenienceFee(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return GetSubscriptionCostTodayNoFees(msgs, dc) +
                    GetSubscriptionCostTodayConvenienceFee(msgs, dc);
        }

        public decimal GetSubscriptionCostTodayCreditCardFee(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (CreditCardPurchaseObject.IsNotEmpty())
                return new FeeCalculationObject(
                                GetSubscriptionCostTodayNoFees(msgs, dc),
                                BillingAccount,
                                msgs,
                                dc)
                            .GetCreditCardFee();

            return 0;
        }

        public decimal GetSubscriptionCostTodayConvenienceFee(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return new FeeCalculationObject(
                                GetSubscriptionCostTodayNoFees(msgs, dc),
                                BillingAccount,
                                msgs,
                                dc)
                            .GetConvenienceFee();
        }

        public decimal GetSubscriptionTotalFeesToday(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return GetSubscriptionCostTodayConvenienceFee(msgs, dc) + GetSubscriptionCostTodayCreditCardFee(msgs, dc);
        }

        public decimal PaymentPlanCostTodayNoFees
        {
            get
            {
                return PaymentPlan.IsNotEmpty() ? PaymentPlan.CostPerPeriod.Value * NumberOfSeats : 0;
            }
        }

        public decimal GetPaymentPlanCostTodayWithFees(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return GetPaymentPlanCostTodayWithConvenienceFee(msgs, dc) +
                    GetPaymentPlanCostTodayCreditCardFee(msgs, dc);
        }

        public decimal GetPaymentPlanCostTodayWithConvenienceFee(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var activity = ParticipationObject.GetActivity();

            if (activity.IsConvenienceFeeTakenOutOfCost)
                return PaymentPlanCostTodayNoFees;
            
            return PaymentPlanCostTodayNoFees + GetPaymentPlanCostTodayConvenienceFee(msgs, dc);
        }

        public decimal GetPaymentPlanCostTodayCreditCardFee(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (CreditCardPurchaseObject.IsNotEmpty())
                return new FeeCalculationObject(
                                PaymentPlanCostTodayNoFees,
                                BillingAccount,
                                ParticipationObject,
                                msgs,
                                dc)
                            .GetCreditCardFee();

            return 0;
        }

        public decimal GetPaymentPlanCostTodayConvenienceFee(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return new FeeCalculationObject(
                                PaymentPlanCostTodayNoFees,
                                BillingAccount,
                                ParticipationObject,
                                msgs,
                                dc)
                            .GetConvenienceFee();
        }

        public decimal GetPaymentPlanTotalFeesToday(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return GetPaymentPlanCostTodayConvenienceFee(msgs, dc) + GetPaymentPlanCostTodayCreditCardFee(msgs, dc);
        }        

        public decimal ApplyCoupon(decimal total)
        {
            if (Coupon.IsNotEmpty() && Coupon.Amount.HasValue)
            {
                if (Coupon.AmountTypeId == CouponAmountType.FlatAmount)
                    return Math.Max(0, total - Coupon.Amount.Value);
                else if (Coupon.AmountTypeId == CouponAmountType.Percentage)
                    return Math.Max(0, total - (total * Coupon.Amount.Value));
            }

            return total;
        }

        public decimal GetPlatformPercentFee(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (PaymentSourceType == PaymentSourceType.AfterSchoolPlatform)
                return BillingAccount.GetPlatformConveniencePercentFee(msgs, dc);

            return 0;
        }

        public decimal GetPlatformFlatFee(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (!IsRegistrationFree && PaymentSourceType == PaymentSourceType.AfterSchoolPlatform)
                return BillingAccount.GetPlatformConvenienceFlatFee(msgs, dc);

            return 0;
        }

        public Purchase CreateOrUpdatePurchase(WebUser loggedInUser, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (this.Purchase.IsEmpty())
                this.Purchase = this.Registration.GetPurchase(msgs, dc);

            if (this.Purchase.IsEmpty())
            {
                this.Purchase = AccountService.AddPurchase(this, loggedInUser, msgs, dc);
                this.Registration.PurchaseId = this.Purchase.ID;
                msgs.AddRange(this.Registration.Save(dc));
            }

            return this.Purchase;
        }

        private bool? _UseStripeConnectedAccount = null;
        private bool GetUseStripeConnectedAccount(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (!_UseStripeConnectedAccount.HasValue)
                _UseStripeConnectedAccount = BillingAccount.HasFeature(FeatureType.UseStripeConnectedAccount, msgs, dc);

            return _UseStripeConnectedAccount.Value;
        }

        public void LoadStripeSource(SystemMessages msgs)
        {
            if (StripeSourceId.IsNotEmpty())
                return;

            if (BankAccountPurchaseObject?.PlaidPublicToken != null)
                BankAccountPurchaseObject.StripeSourceId = PayingAccount.GetStripeSourceFromPlaidToken(
                                                            BankAccountPurchaseObject, 
                                                            msgs);
            else if (SavePaymentInfo || SubscriptionPlan.IsNotEmpty() || PostPayPlan.IsNotEmpty())
                CreditCardPurchaseObject.StripeSourceId = PayingAccount.GetStripeSourceFromToken(
                                                            BillingAccount,
                                                            CreditCardPurchaseObject.StripeToken, 
                                                            msgs);
        }

        public decimal CalculatePlatformFeeAmount(SystemMessages msgs, DatabaseConnection dc = null)
        {
            var totalAHQFees = 0M;

            if (ConvenienceFeeOverrideAmount.HasValue)
                totalAHQFees += ConvenienceFeeOverrideAmount.Value;
            else
                totalAHQFees += GetTotalConvenienceFeesToday(msgs, dc);

            if (PaymentTypeFeeOverrideAmount.HasValue)
                totalAHQFees += PaymentTypeFeeOverrideAmount.Value;
            else
                totalAHQFees += GetTotalCreditCardFeesToday(msgs, dc);

            return totalAHQFees;
            
        }

    }
}
