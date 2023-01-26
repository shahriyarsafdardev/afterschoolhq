using System;
using System.Collections.Generic;
using System.Linq;
using AfterSchoolLibrary.Billing;
using AfterSchoolLibrary.Helpers;
using AfterSchoolLibrary.Services;
using BasicLibrary;
using BasicLibrary.DataAccess;

namespace AfterSchoolLibrary.Models
{
    public class CheckoutObject
    {
        private string description;

        public CheckoutObject()
        {
            Products = new List<Product>();
            Date = DateTime.UtcNow;
            SelectedDates = new List<DateTime>();
        }

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
        public ProratingCalculationHelper ProratingCalculation { get; set; }


        /// <summary>
        ///     override will sometimes be the manual payment or the subscription payment
        /// </summary>
        public decimal? OverrideAmount { get; set; }

        /// <summary>
        ///     after pro-rating the subscription, AmountPaidByParent will have actual amount parent paid
        /// </summary>
        public decimal? AmountPaidByParent { get; set; }

        /// <summary>
        ///     after pro-rating the subscription, AmountReceivedByProvider will hold the amount received by provider
        /// </summary>
        public decimal? AmountReceivedByProvider { get; set; }

        /// <summary>
        ///     flag used denote whether amount is pro-rated for any plans or not. will be used mostly in ActivityPayment
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

        public string Description
        {
            get
            {
                if (ParticipationObject.IsNotEmpty())
                {
                    if (ParticipationObject.CanRegisterPerClass)
                        return $"{ParticipationObject.Name} Registration - {ClassQuantity} Classes";

                    return $"{ParticipationObject.Name} Registration";
                }

                return description;
            }
            set => description = value;
        }

        public bool HasStripeSourceId => BankAccountPurchaseObject?.StripeSourceId != null || CreditCardPurchaseObject?.StripeSourceId != null;

        public string StripeSourceId => BankAccountPurchaseObject?.StripeSourceId ?? CreditCardPurchaseObject?.StripeSourceId;

        public bool IsFree => ParticipationObject.IsFree;

        private bool IsRegistrationFree => ParticipationObject.IsRegistrationFree;

        public int ClassQuantity
        {
            get
            {
                if (SelectedDates.IsNotEmpty() && SelectedDates.Any())
                    return SelectedDates.Count;

                return 1;
            }
        }

        private bool IsCouponApplied => Coupon.IsNotEmpty() && Coupon.ID.IsNotEmpty() && Coupon.DoesApplyToRegistrationFee();

        private decimal RegistrationCostNoFees => ParticipationObject.IsNotEmpty() && ParticipationObject.Cost.HasValue
            ? ParticipationObject.Cost.Value * NumberOfSeats
            : 0;

        public decimal GetStripeAmount(SystemMessages msgs, DatabaseConnection dc = null)
        {
            return OverrideAmount ?? (PostPayPlan.IsNotEmpty() ? GetPostPayPlanCostTodayWithFees() : GetRegistrationCostWithFees(msgs, dc));
        }

        public decimal GetTotalCostWithoutSubscriptionToPurchaseTodayNoFees(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return PaymentPlan.IsNotEmpty()
                ? PaymentPlanCostTodayNoFees
                : GetTotalCostNoFees(msgs, dc) + GetPostPayPlanCostTodayWithFees();
        }

        private decimal GetTotalCost()
        {
            if (PaymentPlan.IsNotEmpty())
                return PaymentPlan.CostPerPeriod.Value * PaymentPlan.TotalPeriods.Value;

            if (OverrideAmount.HasValue)
                return OverrideAmount.Value - PaymentTypeFeeOverrideAmount.Value;

            return RegistrationCost() * ClassQuantity;
        }

        public decimal GetTotalCostNoFees(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            // NOTE: Coupon value should know whether to include fees or not, but have to research much deeper
            return IsCouponApplied
                ? Math.Max(GetTotalCost() - CouponValue(msgs, dc), 0)
                : GetTotalCost();
        }

        public decimal GetTotalCostToPurchaseTodayWithFees(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var subscriptionCostTodayWithFees = GetSubscriptionCostTodayWithFees(msgs, dc);
            return GetTotalCostWithoutSubscriptionToPurchaseTodayWithFees(msgs, dc) + subscriptionCostTodayWithFees;
        }

        public decimal GetTotalCostWithoutSubscriptionToPurchaseTodayWithFees(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return PaymentPlan.IsNotEmpty()
                ? GetPaymentPlanCostTodayWithFees(msgs, dc)
                : GetRegistrationCostWithFees(msgs, dc) + GetPostPayPlanCostTodayWithFees();
        }

        public decimal GetTotalCostToPurchaseTodayWithConvenienceFee(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (OverrideAmount.HasValue && PaymentTypeFeeOverrideAmount.HasValue)
                return OverrideAmount.Value - PaymentTypeFeeOverrideAmount.Value;

            if (PaymentPlan.IsNotEmpty())
                return GetPaymentPlanCostTodayWithFees(msgs, dc) + GetSubscriptionCostTodayWithFees(msgs, dc);

            return GetRegistrationCostWithConvenienceFee(msgs, dc) + GetSubscriptionCostTodayWithConvenienceFee(msgs, dc) +
                   GetPostPayPlanCostTodayWithConvenienceFee();
        }

        public decimal GetTotalConvenienceFeesToday(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (ConvenienceFeeOverrideAmount.HasValue)
                return ConvenienceFeeOverrideAmount.Value;

            var subscriptionCostTodayConvenienceFee = GetSubscriptionCostTodayConvenienceFee(msgs, dc);
            if (PaymentPlan.IsNotEmpty())
                return GetPaymentPlanCostTodayConvenienceFee(msgs, dc) + subscriptionCostTodayConvenienceFee;

            var regConvenienceFee = GetRegistrationConvenienceFee(msgs, dc);

            if (RegistrationCostWithCoupon(msgs, dc) == 0)
                regConvenienceFee = 0;

            return regConvenienceFee + subscriptionCostTodayConvenienceFee + GetPostPayPlanCostTodayConvenienceFee();
        }

        public decimal GetTotalCreditCardFeesToday(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (PaymentTypeFeeOverrideAmount.HasValue)
                return PaymentTypeFeeOverrideAmount.Value;

            var subscriptionCostTodayCreditCardFee = GetSubscriptionCostTodayCreditCardFee(msgs, dc);
            if (PaymentPlan.IsNotEmpty())
                return GetPaymentPlanCostTodayCreditCardFee(msgs, dc) + subscriptionCostTodayCreditCardFee;

            return GetRegistrationCreditCardFee(msgs, dc) + subscriptionCostTodayCreditCardFee + GetPostPayPlanCostTodayCreditCardFee();
        }

        /// <summary>
        ///     Coupon value needs to include convenience fees
        /// </summary>
        public decimal CouponValue(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (!IsCouponApplied)
                return 0;

            if (PostPayPlan.IsNotEmpty() && OverrideAmount > 0 && Registration.CouponValue.HasValue)
                //.. this is specific case, 
                //.. post pay plan has pre payment enabled and provider is taking attendance, 
                //.. so in that case, CouponValue is also multiplying the CostPerPeriod with number of class, which should not happen.
                //.. reason for that is right now we can't differentiate between amount is for pre payment or for attendance so.
                return Registration.CouponValue.Value;

            var value = Coupon.GetValue(GetAmountToApplyToCoupon(msgs, dc));
            return Coupon.AmountTypeId == CouponAmountType.Percentage && Coupon.Amount.HasValue ? Math.Round(value) : value;
        }

        private decimal RegistrationCost()
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

        private decimal GetAmountToApplyToCoupon(SystemMessages msgs, DatabaseConnection dc)
        {
            if (!ParticipationObject.IsActivity())
                return 0;

            var activity = ParticipationObject.GetActivity();

            if (activity.IsConvenienceFeeTakenOutOfCost)
                return GetTotalCost();

            if (PostPayPlan.IsNotEmpty())
                return ParticipationObject.GetPostPayCheckoutCostWithoutConvenienceFee();

            return GetTotalCost() + GetRegistrationConvenienceFee(msgs, dc);
        }

        internal decimal GetRegistrationCostWithConvenienceFee(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return GetRegistrationCostWithFeeAndCoupon(IsCouponApplied, msgs, dc);
        }

        public decimal GetRegistrationCostWithFeeAndCoupon(bool applyCoupon = false, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (!ParticipationObject.IsActivity())
                return 0;

            var activity = ParticipationObject.GetActivity();
            var discount = CouponValue(msgs, dc);
            var cost = RegistrationCost();
            var fee = GetRegistrationConvenienceFee(msgs, dc); //fee with quantity

            if (applyCoupon)
            {
                if (activity.IsConvenienceFeeTakenOutOfCost)
                    return cost * ClassQuantity - discount;

                return cost * ClassQuantity + fee - discount;
            }

            if (activity.IsConvenienceFeeTakenOutOfCost)
                return cost * ClassQuantity;

            return cost * ClassQuantity + fee;
        }

        internal decimal GetRegistrationCreditCardFee(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (IsRegistrationFree || CreditCardPurchaseObject.IsEmpty())
                return 0;

            var feeCalculationObject = new FeeCalculationObject(RegistrationCostNoFees, BillingAccount, ParticipationObject, msgs, dc);
            return feeCalculationObject.GetCreditCardFee() * ClassQuantity;
        }

        internal decimal GetRegistrationConvenienceFee(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (IsRegistrationFree)
                return 0;

            var feeCalculationObject = new FeeCalculationObject(RegistrationCostNoFees, BillingAccount, ParticipationObject, msgs, dc);
            return feeCalculationObject.GetConvenienceFee() * ClassQuantity;
        }

        public decimal GetSubscriptionCostTodayNoFees(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (SubscriptionPlan.IsEmpty() || (SubscriptionPlan.SplitCostEvenly && !ParticipationObject.RegistrationFinalized))
                return 0;

            var planStartDateInAccountTimeZone = MasterRegistrant.GetPlanStartDate(SubscriptionPlan, ParticipationObject);
            var planStartDateInUtc = ParticipationObject.OwnerAccount.ConvertFromAccountTimeZone(planStartDateInAccountTimeZone);
            var startsToday = planStartDateInUtc.Date == DateTime.UtcNow.Date;
            if (!startsToday)
                return 0;

            var costPerPeriodPerPerson = SubscriptionPlan.GetCostPerPeriodPerPerson(msgs, dc);
            return costPerPeriodPerPerson * NumberOfSeats; 
        }

        public decimal GetSubscriptionCostNoFees(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (SubscriptionPlan.IsEmpty() || (SubscriptionPlan.SplitCostEvenly && !ParticipationObject.RegistrationFinalized))
                return 0;

            var costPerPeriodPerPerson = SubscriptionPlan.GetCostPerPeriodPerPerson(msgs, dc);
            return costPerPeriodPerPerson * NumberOfSeats;
        }

        public decimal GetSubscriptionCostTodayWithFees(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var subscriptionCostTodayWithConvenienceFee = GetSubscriptionCostTodayWithConvenienceFee(msgs, dc);
            var subscriptionCostTodayCreditCardFee = GetSubscriptionCostTodayCreditCardFee(msgs, dc);
            return subscriptionCostTodayWithConvenienceFee + subscriptionCostTodayCreditCardFee;
        }

        public decimal GetSubscriptionCostTodayWithConvenienceFee(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (SubscriptionPlan.IsNotEmpty() && SubscriptionPlan.IsConvenienceFeeTakenOutOfCost)
                return GetSubscriptionCostTodayNoFees(msgs, dc);

            return GetSubscriptionCostTodayNoFees(msgs, dc) + GetSubscriptionCostTodayConvenienceFee(msgs, dc);
        }
        public decimal GetSubscriptionCostWithConvenienceFee(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (SubscriptionPlan.IsNotEmpty() && SubscriptionPlan.IsConvenienceFeeTakenOutOfCost)
                return GetSubscriptionCostNoFees(msgs, dc);

            return GetSubscriptionCostNoFees(msgs, dc) + GetSubscriptionCostConvenienceFee(msgs, dc);
        }

        public decimal GetSubscriptionCostTodayCreditCardFee(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return CreditCardPurchaseObject.IsNotEmpty()
                ? new FeeCalculationObject(
                    GetSubscriptionCostTodayNoFees(msgs, dc), 
                    BillingAccount, 
                    SubscriptionPlan.IsConvenienceFeeTakenOutOfCost, 
                    msgs, 
                    dc).GetCreditCardFee()
                : 0;
        }

        public decimal GetSubscriptionCostCreditCardFee(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return CreditCardPurchaseObject.IsNotEmpty()
                ? new FeeCalculationObject(
                    GetSubscriptionCostNoFees(msgs, dc), 
                    BillingAccount, 
                    SubscriptionPlan.IsConvenienceFeeTakenOutOfCost, 
                    msgs, 
                    dc).GetCreditCardFee()
                : 0;
        }

        public decimal GetSubscriptionCostTodayConvenienceFee(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return new FeeCalculationObject(
                    GetSubscriptionCostTodayNoFees(msgs, dc), 
                    BillingAccount, 
                    SubscriptionPlan.IsConvenienceFeeTakenOutOfCost, 
                    msgs, 
                    dc).GetConvenienceFee();
        }
        
        public decimal GetSubscriptionCostConvenienceFee(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return new FeeCalculationObject(
                GetSubscriptionCostNoFees(msgs, dc), 
                BillingAccount, 
                SubscriptionPlan.IsConvenienceFeeTakenOutOfCost, 
                msgs, 
                dc).GetConvenienceFee();
        }

        private decimal GetPlatformPercentFee(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return PaymentSourceType == PaymentSourceType.AfterSchoolPlatform
                ? BillingAccount.GetPlatformConveniencePercentFee(msgs, dc)
                : 0;
        }

        private decimal GetPlatformFlatFee(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (!IsRegistrationFree && PaymentSourceType == PaymentSourceType.AfterSchoolPlatform)
                return BillingAccount.GetPlatformConvenienceFlatFee(msgs, dc);

            return 0;
        }

        public decimal GetPlatformPercentFeeWithCoupon(SystemMessages msgs, DatabaseConnection dc)
        {
            return CouponValue(msgs, dc) >= GetTotalCostToPurchaseTodayWithConvenienceFee() ? 0 : GetPlatformPercentFee(msgs, dc);
        }

        public decimal GetPlatformFlatFeeWithCoupon(SystemMessages msgs, DatabaseConnection dc)
        {
            return CouponValue(msgs, dc) >= GetTotalCostToPurchaseTodayWithConvenienceFee() ? 0 : GetPlatformFlatFee(msgs, dc);
        }

        public Purchase CreateOrUpdatePurchase(WebUser loggedInUser, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            msgs ??= new SystemMessages();

            if (Purchase.IsNotEmpty())
                return Purchase;

            Purchase = Registration.GetPurchase(msgs, dc);
            if (Purchase.IsNotEmpty())
                return Purchase;

            Purchase = AccountService.AddPurchase(this, loggedInUser, msgs, dc);
            Registration.PurchaseId = Purchase.ID;
            msgs.AddRange(Registration.Save(dc));

            return Purchase;
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

        /// <summary>
        /// Only used for Registration Fee and for Attendance based billing Prepayment
        /// </summary>
        /// <param name="msgs"></param>
        /// <param name="dc"></param>
        /// <returns></returns>
        public decimal CalculatePlatformFeeAmountDuringCheckout(SystemMessages msgs, DatabaseConnection dc = null)
        {
            if (PostPayPlan.IsNotEmpty())
            {
                return GetPostPayPlanCostTodayConvenienceFee() + GetPostPayPlanCostTodayCreditCardFee();
            }
            
            return GetRegistrationConvenienceFee(msgs, dc) + GetRegistrationCreditCardFee(msgs, dc);
        }

        public void EnsureSeat(RegistrationAssignmentType rat, WebUser loggedInUser, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var ra = Registration.GetRegistrantActivity(ParticipationObject, msgs, dc);

            // seems like if they're already reserved we shouldn't downgrade them, that decision should be left to provider
            if (ra.IsNotEmpty() && !ra.IsAssigned(rat) && ra.AssignmentTypeId != RegistrationAssignmentType.Reserved)
                ra.ChangeSeatAssignment(rat, loggedInUser, msgs, dc);
        }

        #region "PaymentPlan"

        public decimal PaymentPlanCostTodayNoFees => PaymentPlan.IsNotEmpty() ? PaymentPlan.CostPerPeriod.Value * NumberOfSeats : 0;

        private decimal GetPaymentPlanCostTodayWithFees(SystemMessages msgs = null, DatabaseConnection dc = null)
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
                        PaymentPlan.IsConvenienceFeeTakenOutOfCost,
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
                    PaymentPlan.IsConvenienceFeeTakenOutOfCost,
                    msgs,
                    dc)
                .GetConvenienceFee();
        }

        #endregion

        #region "PostPay"

        public decimal GetPostPayPlanCostTodayWithFees()
        {
            return GetPostPayPlanCostTodayWithConvenienceFee() +
                   GetPostPayPlanCostTodayCreditCardFee();
        }

        private decimal GetPostPayPlanCostTodayWithConvenienceFee()
        {
            var couponDiscount = CouponValue();
            var costWithDiscount = ParticipationObject.GetPostPayCheckoutCostWithoutConvenienceFee() - couponDiscount;
            var convenienceFee = ParticipationObject.GetCheckoutPostPayConvenienceFee();
            return Math.Max(costWithDiscount + convenienceFee, 0);
        }

        internal decimal GetPostPayPlanCostTodayCreditCardFee()
        {
            return CreditCardPurchaseObject.IsNotEmpty() ? ParticipationObject.GetCheckoutPostPayCreditCardFee() : 0;
        }

        internal decimal GetPostPayPlanCostTodayConvenienceFee()
        {
            return ParticipationObject.GetCheckoutPostPayConvenienceFee();
        }

        #endregion
    }
}