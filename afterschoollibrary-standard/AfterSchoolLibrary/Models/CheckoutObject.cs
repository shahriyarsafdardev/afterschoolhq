using BasicLibrary;
using BasicLibrary.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using AfterSchoolLibrary.Billing;
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
        public List<DateTime> SelectedDates { get; set; }


        /// <summary>
        /// override will sometimes be the manual payment or the subscription payment
        /// </summary>
        public decimal? OverrideAmount { get; set; }
        
        /// <summary>
        /// after pro-rating the subscription, AmountPaidByParent will have actual amount parent paid
        /// </summary>
        public decimal? AmountPaidByParent { get; set; }
        
        /// <summary>
        /// after pro-rating the subscription, AmountReceivedByProvider will hold the amount received by provider
        /// </summary>
        public decimal? AmountReceivedByProvider { get; set; }

        /// <summary>
        /// flag used denote whether amount is pro-rated for any plans or not. will be used mostly in ActivityPayment
        /// </summary>
        public bool IsAmountProRated { get; set; }

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
            SelectedDates = new List<DateTime>();
        }

        private string _description;
        public string Description
        {
            get
            {
                if (ParticipationObject.IsNotEmpty())
                {
                    if (ParticipationObject.CanRegisterPerClass)
                        return $"{this.ParticipationObject.Name} Registration - {ClassQuantity} Classes";

                    return $"{this.ParticipationObject.Name} Registration";
                }

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

            if (PostPayPlan.IsNotEmpty())
                return GetPostPayPlanCostTodayWithFees();

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

        public int ClassQuantity
        {
            get
            {
                if (SelectedDates.IsNotEmpty() && SelectedDates.Any())
                    return SelectedDates.Count;

                return 1;
            }
        }

        public bool IsCouponApplied
        {
            get
            {
                return Coupon.IsNotEmpty() && Coupon.ID.IsNotEmpty() && Coupon.DoesApplyToRegistrationFee();
            }
        }

        public decimal GetTotalCostToPurchaseTodayNoFees(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (PaymentPlan.IsNotEmpty())
                return PaymentPlanCostTodayNoFees + GetSubscriptionCostTodayNoFees(msgs, dc);

            return GetTotalCostNoFees(msgs,dc) + GetSubscriptionCostTodayNoFees(msgs, dc) + GetPostPayPlanCostTodayWithFees();
        }

        public decimal GetTotalCostWithFees(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (PaymentPlan.IsNotEmpty())
                return PaymentPlan.GetTotalCostWithFees(this.BillingAccount, msgs, dc) * PaymentPlan.TotalPeriods.Value;

            if (OverrideAmount.HasValue)
                return OverrideAmount.Value;

            return GetRegistrationCostWithFees(msgs, dc);
        }

        public decimal GetTotalCost(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (PaymentPlan.IsNotEmpty())
                return PaymentPlan.CostPerPeriod.Value * PaymentPlan.TotalPeriods.Value;

            if (OverrideAmount.HasValue)
                return OverrideAmount.Value - PaymentTypeFeeOverrideAmount.Value;

            return RegistrationCost(msgs, dc) * ClassQuantity;
        }

        public decimal GetTotalCostNoFees(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (IsCouponApplied)
                return Math.Max(GetTotalCost(msgs, dc) - CouponValue(msgs, dc), 0); // NOTE: Coupon value should know whether to include fees or not, but have to research much deeper
            else
                return GetTotalCost(msgs, dc);
        }

        public decimal GetTotalCostToPurchaseTodayWithFees(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (PaymentPlan.IsNotEmpty())
                return GetPaymentPlanCostTodayWithFees(msgs, dc) + GetSubscriptionCostTodayWithFees(msgs, dc);

            return GetRegistrationCostWithFees(msgs, dc) + GetSubscriptionCostTodayWithFees(msgs, dc) + GetPostPayPlanCostTodayWithFees();
        }

        public decimal GetTotalCostToPurchaseTodayWithConvenienceFee(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (OverrideAmount.HasValue && PaymentTypeFeeOverrideAmount.HasValue)
                return OverrideAmount.Value - PaymentTypeFeeOverrideAmount.Value;

            if (PaymentPlan.IsNotEmpty())
                return GetPaymentPlanCostTodayWithFees(msgs, dc) + GetSubscriptionCostTodayWithFees(msgs, dc);

            return GetRegistrationCostWithConvenienceFee(msgs, dc) + GetSubscriptionCostTodayWithConvenienceFee(msgs, dc) + GetPostPayPlanCostTodayWithConvenienceFee();
        }

        public decimal GetTotalFeesToday(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (PaymentPlan.IsNotEmpty())
                return GetPaymentPlanTotalFeesToday(msgs, dc) + GetSubscriptionTotalFeesToday(msgs, dc);

            return GetRegistrationTotalFees(msgs, dc) + GetSubscriptionTotalFeesToday(msgs, dc) + GetPostPayPlanTotalFeesToday();
        }

        public decimal GetTotalConvenienceFeesToday(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (ConvenienceFeeOverrideAmount.HasValue)
                return ConvenienceFeeOverrideAmount.Value;

            if (PaymentPlan.IsNotEmpty())
                return GetPaymentPlanCostTodayConvenienceFee(msgs, dc) + GetSubscriptionCostTodayConvenienceFee(msgs, dc);

            var regConvenienceFee = GetRegistrationConvenienceFee(msgs, dc);

            if (RegistrationCostWithCoupon(msgs, dc) == 0)
                regConvenienceFee = 0;
            
            return regConvenienceFee + GetSubscriptionCostTodayConvenienceFee(msgs, dc) + GetPostPayPlanCostTodayConvenienceFee();
        }

        public decimal GetTotalCreditCardFeesToday(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (PaymentTypeFeeOverrideAmount.HasValue)
                return PaymentTypeFeeOverrideAmount.Value;

            if (PaymentPlan.IsNotEmpty())
                return GetPaymentPlanCostTodayCreditCardFee(msgs, dc) + GetSubscriptionCostTodayCreditCardFee(msgs, dc);

            return GetRegistrationCreditCardFee(msgs, dc) + GetSubscriptionCostTodayCreditCardFee(msgs, dc) + GetPostPayPlanCostTodayCreditCardFee();
        }

        public decimal RegistrationCostNoFees
        {
            get
            {
                if (ParticipationObject.IsNotEmpty() && ParticipationObject.Cost.HasValue)
                    return ParticipationObject.Cost.Value * NumberOfSeats;

                return 0;
            }
        }
        /// <summary>
        /// Coupon value needs to include convenience fees
        /// </summary>
        public decimal CouponValue(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (IsCouponApplied)
            {
                if (PostPayPlan.IsNotEmpty() && OverrideAmount > 0 && Registration.CouponValue.HasValue)
                {
                    //.. this is specific case, 
                    //.. post pay plan has pre payement enabled and provider is taking attendace, 
                    //.. so in that case, CouponValue is also multiplying the CostPerPeriod with number of class, which should not happen.
                    //.. reason for that is right now we can't differentiate between amount is for pre payment or for attendace so.
                    return Registration.CouponValue.Value;
                }
                else
                {
                    var value = Coupon.GetValue(GetAmountToApplyToCoupon(msgs, dc));
                    return Coupon.AmountTypeId == CouponAmountType.Percentage && Coupon.Amount.HasValue ? Math.Round(value) : value;
                }
            }

            return 0;
        }

        public decimal RegistrationCost(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return Math.Max(RegistrationCostNoFees, 0);
        }

        public decimal RegistrationCostWithCoupon(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return Math.Max(RegistrationCostNoFees - CouponValue(msgs, dc), 0); 
        }

        public decimal GetRegistrationCostWithFees(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return GetRegistrationCostWithConvenienceFee(msgs, dc) + GetRegistrationCreditCardFee(msgs, dc);
        }

        public decimal GetAmountToApplyToCoupon(SystemMessages msgs, DatabaseConnection dc)
        {
            if (!ParticipationObject.IsActivity())
                return 0;

            var activity = ParticipationObject.GetActivity();

            if (activity.IsConvenienceFeeTakenOutOfCost)
                return GetTotalCost(msgs, dc);

            if (PostPayPlan.IsNotEmpty())
                return ParticipationObject.GetPostPayCheckoutCostWithoutConvenienceFee();

            return GetTotalCost(msgs, dc) + GetRegistrationConvenienceFee(msgs, dc);
        }

        public decimal GetRegistrationCostWithConvenienceFee(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return GetRegistrationCostWithFeeAndCoupon(applyCoupon: IsCouponApplied, msgs: msgs, dc: dc);
        }

        public decimal GetRegistrationCostWithFeeAndCoupon(bool applyCoupon = false, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (!ParticipationObject.IsActivity())
                return 0;

            var activity = ParticipationObject.GetActivity();
            var discount = CouponValue(msgs, dc);
            var cost = RegistrationCost(msgs, dc);
            var fee = GetRegistrationConvenienceFee(msgs, dc); //fee with quantity

            if (applyCoupon)
            {
                if (activity.IsConvenienceFeeTakenOutOfCost)
                    return (cost * ClassQuantity) - discount;

                return ((cost * ClassQuantity) + fee) - discount;
            }
            else
            {
                if (activity.IsConvenienceFeeTakenOutOfCost)
                    return (cost * ClassQuantity);

                return (cost * ClassQuantity) + fee;
            }            
        }

        public decimal GetRegistrationCreditCardFee(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (!IsRegistrationFree && CreditCardPurchaseObject.IsNotEmpty())
            {
                var feeCalculationObject = new FeeCalculationObject(RegistrationCostNoFees, BillingAccount, ParticipationObject, msgs, dc);
                return feeCalculationObject.GetCreditCardFee() * ClassQuantity;
            }

            return 0;
        }

        public decimal GetRegistrationConvenienceFee(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (!IsRegistrationFree)
            {
                var feeCalculationObject = new FeeCalculationObject(RegistrationCostNoFees, BillingAccount, ParticipationObject, msgs, dc);
                return feeCalculationObject.GetConvenienceFee() * ClassQuantity;
            }

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
            return GetSubscriptionCostTodayWithConvenienceFee(msgs, dc) +
                    GetSubscriptionCostTodayCreditCardFee(msgs, dc);
        }

        public decimal GetSubscriptionCostTodayWithConvenienceFee(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (SubscriptionPlan.IsNotEmpty() && SubscriptionPlan.IsConvenienceFeeTakenOutOfCost)
                return GetSubscriptionCostTodayNoFees(msgs, dc);
            
            return GetSubscriptionCostTodayNoFees(msgs, dc) + GetSubscriptionCostTodayConvenienceFee(msgs, dc);
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

        #region "PaymentPlan"

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
            if (PaymentPlan.IsNotEmpty() && PaymentPlan.IsConvenienceFeeTakenOutOfCost)
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

        #endregion

        #region "PostPay"

        public decimal GetPostPayPlanCostTodayWithFees()
        {
            return GetPostPayPlanCostTodayWithConvenienceFee() +
                    GetPostPayPlanCostTodayCreditCardFee();
        }

        public decimal GetPostPayPlanCostTodayWithConvenienceFee()
        {
            var couponDiscount = CouponValue();
            var costWithDiscount = ParticipationObject.GetPostPayCheckoutCostWithoutConvenienceFee() - couponDiscount;
            var convenienceFee = ParticipationObject.GetCheckoutPostPayConvenienceFee();
            return Math.Max(costWithDiscount + convenienceFee, 0);
        }

        public decimal GetPostPayPlanCostTodayCreditCardFee()
        {
            if (CreditCardPurchaseObject.IsNotEmpty())
                return ParticipationObject.GetCheckoutPostPayCreditCardFee();

            return 0;
        }

        public decimal GetPostPayPlanCostTodayConvenienceFee()
        {
            return ParticipationObject.GetCheckoutPostPayConvenienceFee();
        }

        public decimal GetPostPayPlanTotalFeesToday()
        {
            return GetPostPayPlanCostTodayConvenienceFee() + GetPostPayPlanCostTodayCreditCardFee();
        }

        #endregion

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

        public decimal GetPlatformPercentFeeWithCoupon(SystemMessages msgs, DatabaseConnection dc)
        {
            if (CouponValue(msgs, dc) >= GetTotalCostToPurchaseTodayWithConvenienceFee())
                return 0;

            return GetPlatformPercentFee(msgs, dc);
        }

        public decimal GetPlatformFlatFeeWithCoupon(SystemMessages msgs, DatabaseConnection dc)
        {
            if (CouponValue(msgs, dc) >= GetTotalCostToPurchaseTodayWithConvenienceFee())
                return 0;

            return GetPlatformFlatFee(msgs, dc);
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

        public void LoadStripeSource(StripeAccountType stripeAccountType, SystemMessages msgs)
        {
            if (StripeSourceId.IsNotEmpty())
                return;

            if (BankAccountPurchaseObject?.PlaidPublicToken != null)
                BankAccountPurchaseObject.StripeSourceId = PayingAccount.GetStripeSourceFromPlaidToken(
                                                            BankAccountPurchaseObject, 
                                                            stripeAccountType,
                                                            msgs);
            else if (SavePaymentInfo || SubscriptionPlan.IsNotEmpty() || PostPayPlan.IsNotEmpty() || CreditCardPurchaseObject.StripeToken.IsNotEmpty())
                CreditCardPurchaseObject.StripeSourceId = PayingAccount.GetStripeSourceFromToken(
                                                            CreditCardPurchaseObject.StripeToken, 
                                                            stripeAccountType,
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

        public void EnsureSeat(RegistrationAssignmentType rat, WebUser loggedInUser, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var ra = this.Registration.GetRegistrantActivity(this.ParticipationObject, msgs, dc);

            // seems like if they're already reserved we shouldn't downgrade them, that decision should be left to provider
            if (ra.IsNotEmpty() && !ra.IsAssigned(rat) && ra.AssignmentTypeId != RegistrationAssignmentType.Reserved)
                ra.ChangeSeatAssignment(rat, loggedInUser, msgs, dc);
        }

    }
}
