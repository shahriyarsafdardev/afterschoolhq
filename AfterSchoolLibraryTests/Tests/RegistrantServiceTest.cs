using Xunit;
using Xunit.Abstractions;
using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using AfterSchoolLibraryTests.Factories;
using System.Linq;
using System;
using BasicLibrary;
using AfterSchoolLibrary.Billing;
using System.Collections.Generic;
using System.Configuration;

namespace AfterSchoolLibraryTests.Tests
{
    public class RegistrantServiceTest : BaseTest
    {
        private WebUser user { get; set; }
        private Account account { get; set; }

        public RegistrantServiceTest(ITestOutputHelper output) : base(output)
        {
            user = WebUserFactory.Create(msgs, dc);
            account = AccountFactory.Create(user, msgs, dc);
        }

        [Theory(DisplayName = "StartSubscriptionPlan Test")]
        [InlineData(110)]
        [InlineData(12)]
        public void StartSubscriptionPlanTest(decimal amount)
        {
            try
            {
                // Arrange
                var billingAccountOwner = WebUserFactory.Create("Don", "John", "DonJohn@test.com", true, msgs, dc);
                var payingAccountOwner = WebUserFactory.Create("Cristine", "Trump", "CristineTrump@test.com", true, msgs, dc);
                var billingAccount = AccountFactory.CreateAccount(billingAccountOwner, AccountType.School, stripeChargesEnabled: true, stripePayoutsEnabled: true, msgs, dc);
                var payingAccount = AccountFactory.CreateWithFreePackage("Purchasing account", "PurchasingAccount@test.com", payingAccountOwner, AccountType.School, msgs, dc);
                var masterRegistrant = RegistrantFactory.Create(payingAccount, isMaster: true, msgs, dc);

                //UseStripeConnectedAccount is true for this package
                var package = PackageFactory.CreateFreePackageWithUseStripeConnectedAccount(msgs, dc);
                var accountPackage = AccountPackageFactory.Create(billingAccount, package, msgs, dc);

                var activity = ActivityFactory.Create(billingAccount, msgs, dc: dc);
                var registrant = RegistrantFactory.Create(billingAccount, isMaster: false, msgs, dc: dc);
                var registration = RegistrationFactory.Create(billingAccount, registrant, msgs, dc);
                var subscriptionPlan = SubscriptionPlanFactory.Create(billingAccount, activity, SubscriptionPlanType.Subscription, amount, msgs, dc);

                var stripeBillingObject = AccountFactory.CreateStripeAccountAndCustomer(billingAccount, msgs, dc);                
                var stripePayingObject = AccountFactory.CreateStripeAccountAndCustomer(payingAccount, msgs, dc);
                var sourceId = payingAccount.CreateStripeSourceFromToken(StripeHelper.TestSourceToken, stripePayingObject.Item2, msgs, dc);

                CreditCardPurchaseObject creditCardPurchaseObject = new CreditCardPurchaseObject() {
                    StripeToken = StripeHelper.TestSourceToken,
                    StripeSourceId = sourceId
                };

                var participationObject = activity.GetParticipationObject(msgs, dc);
                var checkoutObject = new CheckoutObject()
                {
                    BillingAccount = billingAccount,
                    PayingAccount = payingAccount,
                    Coupon = null,
                    OverrideAmount = amount,
                    MasterRegistrant = masterRegistrant,
                    NumberOfSeats = 1,
                    ParticipationObject = participationObject,
                    SubscriptionPlan = subscriptionPlan,
                    PaymentSourceType = PaymentSourceType.AfterSchoolPlatform,
                    PaymentType = PaymentType.CreditCard,
                    CreditCardPurchaseObject = creditCardPurchaseObject,
                    SavePaymentInfo = true,
                    Registration = registration
                };

                //Act
                masterRegistrant.StartSubscriptionPlan(
                    checkoutObject,
                    payingAccountOwner,
                    msgs,
                    dc);

                //Assert
                //account
                Assert.NotNull(billingAccount);
                Assert.NotNull(payingAccount);

                //activity
                Assert.NotNull(activity);
                Assert.Equal(activity?.AccountId, billingAccount.ID.Value);

                //registrant
                Assert.NotNull(registrant);
                Assert.NotNull(masterRegistrant);
                Assert.Equal(registrant?.AccountId, billingAccount.ID.Value);
                Assert.Equal(masterRegistrant?.AccountId, payingAccount.ID.Value);

                //registration
                Assert.NotNull(registration);
                Assert.Equal(registration?.RegistrantId, registrant.ID.Value);
                Assert.Equal(registration?.AccountId, billingAccount.ID.Value);

                var startDateTime = masterRegistrant.GetPlanStartDate(subscriptionPlan, participationObject, msgs, dc);
                var endDateTime = subscriptionPlan.GetPlanEndDateForRegistrantSubscription(startDateTime, participationObject, msgs);
                var isSubscriptionRequired = true;
                
                if (startDateTime.IsNotEmpty() && endDateTime.IsNotEmpty())
                    isSubscriptionRequired = subscriptionPlan.CheckIfSubscriptionRequired(startDateTime, endDateTime.Value);

                if (isSubscriptionRequired)
                {
                    //registrantSubscriptionPlan
                    var registrantSubscriptionPlan = checkoutObject.RegistrantSubscriptionPlan;
                    Assert.NotNull(registrantSubscriptionPlan);
                    Assert.Equal(registrantSubscriptionPlan?.Amount, amount);
                    Assert.Equal(registrantSubscriptionPlan?.SubscriptionPlanId, subscriptionPlan.ID.Value);

                    //here we will use masterRegistrant as we created plan for him
                    Assert.Equal(registrantSubscriptionPlan?.RegistrantId, masterRegistrant.ID.Value);
                    Assert.Equal(registrantSubscriptionPlan?.ConvenienceFee, checkoutObject.GetSubscriptionCostTodayConvenienceFee(msgs, dc));
                    Assert.Equal(registrantSubscriptionPlan?.PaymentTypeFee, checkoutObject.GetSubscriptionCostTodayCreditCardFee(msgs, dc));

                    //subscription plan
                    Assert.NotNull(subscriptionPlan);
                    Assert.Equal(subscriptionPlan?.ParticipationObjectId, participationObject.ObjectID.Value);
                    Assert.Equal(subscriptionPlan?.ParticipationObjectTypeId, participationObject.ObjectType);
                    Assert.Equal(subscriptionPlan?.CostPerPeriod, amount);
                    Assert.Equal(subscriptionPlan?.PlanOwnerAccountId, billingAccount.ID.Value);
                    Assert.Equal(BillingPeriodType.Month, subscriptionPlan?.BillingPeriodTypeId);
                    Assert.Equal(SubscriptionPlanType.Subscription, subscriptionPlan?.PlanTypeId);

                    //purchase
                    var purchase = checkoutObject.Purchase;
                    Assert.NotNull(purchase);
                    Assert.Equal(purchase?.ID, purchase.ID.Value);
                    Assert.Equal(purchase?.AmountPaid, amount);
                    Assert.Equal(purchase?.RegistrationId, registration.ID.Value);
                    Assert.Equal(purchase?.RegistrantId, masterRegistrant.ID.Value);
                    Assert.Equal(purchase?.PurchaserAccountId, payingAccount.ID.Value);
                }                

                //Stripe
                var product = StripeHelper.GetProduct(activity.Uid.ToString(), stripeAccountId: null, msgs);
                Assert.NotNull(product);

                Assert.True(msgs.HasNoErrors());

                //Notes: this test case does not contains transction and activitypayment assertion as it happens with webhook.
            }
            catch (Exception ex)
            {
                //Assert
                Assert.False(true, ex.Message);
            }            
        }

        [Theory(DisplayName = "StartPaymentPlan Test")]
        [InlineData(110)]
        [InlineData(12)]
        public void StartPaymentPlanTest(decimal amount)
        {
            try
            {
                // Arrange
                var billingAccountOwner = WebUserFactory.Create("Don", "John", "DonJohn@test.com", true, msgs, dc);
                var payingAccountOwner = WebUserFactory.Create("Cristine", "Trump", "CristineTrump@test.com", true, msgs, dc);
                var billingAccount = AccountFactory.CreateAccount(billingAccountOwner, AccountType.School, stripeChargesEnabled: true, stripePayoutsEnabled: true, msgs, dc);
                var payingAccount = AccountFactory.CreateWithFreePackage("Purchasing account", "PurchasingAccount@test.com", payingAccountOwner, AccountType.School, msgs, dc);
                var masterRegistrant = RegistrantFactory.Create(payingAccount, isMaster: true, msgs, dc);

                //UseStripeConnectedAccount is true for this package
                var package = PackageFactory.CreateFreePackageWithUseStripeConnectedAccount(msgs, dc);
                var accountPackage = AccountPackageFactory.Create(billingAccount, package, msgs, dc);

                var activity = ActivityFactory.Create(billingAccount, msgs, dc: dc);
                var registrant = RegistrantFactory.Create(billingAccount, isMaster: false, msgs, dc: dc);
                var registration = RegistrationFactory.Create(billingAccount, registrant, msgs, dc);
                var paymentPlan = SubscriptionPlanFactory.Create(billingAccount, activity, SubscriptionPlanType.PaymentPlan, amount, msgs, dc);

                var stripeBillingObject = AccountFactory.CreateStripeAccountAndCustomer(billingAccount, msgs, dc);
                var stripePayingObject = AccountFactory.CreateStripeAccountAndCustomer(payingAccount, msgs, dc);
                var sourceId = payingAccount.CreateStripeSourceFromToken(StripeHelper.TestSourceToken, stripePayingObject.Item2, msgs, dc);

                CreditCardPurchaseObject creditCardPurchaseObject = new CreditCardPurchaseObject()
                {
                    StripeToken = StripeHelper.TestSourceToken,
                    StripeSourceId = sourceId
                };

                var participationObject = activity.GetParticipationObject(msgs, dc);
                var checkoutObject = new CheckoutObject()
                {
                    BillingAccount = billingAccount,
                    PayingAccount = payingAccount,
                    Coupon = null,
                    OverrideAmount = amount,
                    MasterRegistrant = masterRegistrant,
                    NumberOfSeats = 1,
                    ParticipationObject = participationObject,
                    PaymentPlan = paymentPlan,
                    PaymentSourceType = PaymentSourceType.AfterSchoolPlatform,
                    PaymentType = PaymentType.CreditCard,
                    CreditCardPurchaseObject = creditCardPurchaseObject,
                    SavePaymentInfo = true,
                    Registration = registration
                };

                //Act
                masterRegistrant.StartPaymentPlan(
                    checkoutObject,
                    payingAccountOwner,
                    msgs,
                    dc);

                //Assert
                //account
                Assert.NotNull(billingAccount);
                Assert.NotNull(payingAccount);

                //activity
                Assert.NotNull(activity);
                Assert.Equal(activity?.AccountId, billingAccount.ID.Value);

                //registrant
                Assert.NotNull(registrant);
                Assert.NotNull(masterRegistrant);
                Assert.Equal(registrant?.AccountId, billingAccount.ID.Value);
                Assert.Equal(masterRegistrant?.AccountId, payingAccount.ID.Value);

                //registration
                Assert.NotNull(registration);
                Assert.Equal(registration?.RegistrantId, registrant.ID.Value);
                Assert.Equal(registration?.AccountId, billingAccount.ID.Value);

                var startDateTime = masterRegistrant.GetPlanStartDate(paymentPlan, participationObject, msgs, dc);
                var endDateTime = paymentPlan.GetPlanEndDateForRegistrantSubscription(startDateTime, participationObject, msgs);
                var isSubscriptionRequired = true;

                if (startDateTime.IsNotEmpty() && endDateTime.IsNotEmpty())
                    isSubscriptionRequired = paymentPlan.CheckIfSubscriptionRequired(startDateTime, endDateTime.Value);

                if (isSubscriptionRequired)
                {
                    //registrantSubscriptionPlan
                    var registrantSubscriptionPlan = checkoutObject.RegistrantSubscriptionPlan;
                    Assert.NotNull(registrantSubscriptionPlan);
                    Assert.Equal(registrantSubscriptionPlan?.Amount, amount);
                    Assert.Equal(registrantSubscriptionPlan?.SubscriptionPlanId, paymentPlan.ID.Value);

                    //here we will use masterRegistrant as we created plan for him
                    Assert.Equal(registrantSubscriptionPlan?.RegistrantId, masterRegistrant.ID.Value);
                    Assert.Equal(registrantSubscriptionPlan?.ConvenienceFee, checkoutObject.GetPaymentPlanCostTodayConvenienceFee(msgs, dc));
                    Assert.Equal(registrantSubscriptionPlan?.PaymentTypeFee, checkoutObject.GetPaymentPlanCostTodayCreditCardFee(msgs, dc));

                    //subscription plan
                    Assert.NotNull(paymentPlan);
                    Assert.Equal(paymentPlan?.ParticipationObjectId, participationObject.ObjectID.Value);
                    Assert.Equal(paymentPlan?.ParticipationObjectTypeId, participationObject.ObjectType);
                    Assert.Equal(paymentPlan?.CostPerPeriod, amount);
                    Assert.Equal(paymentPlan?.PlanOwnerAccountId, billingAccount.ID.Value);
                    Assert.Equal(BillingPeriodType.Month, paymentPlan?.BillingPeriodTypeId);
                    Assert.Equal(SubscriptionPlanType.PaymentPlan, paymentPlan?.PlanTypeId);

                    //purchase
                    var purchase = checkoutObject.Purchase;
                    Assert.NotNull(purchase);
                    Assert.Equal(purchase?.ID, purchase.ID.Value);
                    Assert.Equal(purchase?.AmountPaid, amount);
                    Assert.Equal(purchase?.RegistrationId, registration.ID.Value);
                    Assert.Equal(purchase?.RegistrantId, masterRegistrant.ID.Value);
                    Assert.Equal(purchase?.PurchaserAccountId, payingAccount.ID.Value);
                }

                //Stripe
                var product = StripeHelper.GetProduct(activity.Uid.ToString(), stripeAccountId: null, msgs);
                Assert.NotNull(product);

                Assert.True(msgs.HasNoErrors());

                //Notes: this test case does not contains transction and activitypayment assertion as it happens with webhook.
            }
            catch (Exception ex)
            {
                //Assert
                Assert.False(true, ex.Message);
            }
        }

        [Theory(DisplayName = "ProcessActivityPayment without pro-rate Test")]
        [InlineData(110)]
        [InlineData(12)]
        public void ProcessActivityPaymentWithoutProRateTest(decimal amount)
        {
            try
            {
                // Arrange
                var billingAccountOwner = WebUserFactory.Create("Don", "John", "DonJohn@test.com", true, msgs, dc);
                var payingAccountOwner = WebUserFactory.Create("Cristine", "Trump", "CristineTrump@test.com", true, msgs, dc);
                var billingAccount = AccountFactory.CreateAccount(billingAccountOwner, AccountType.School, stripeChargesEnabled: true, stripePayoutsEnabled: true, msgs, dc);
                var payingAccount = AccountFactory.CreateWithFreePackage("Purchasing account", "PurchasingAccount@test.com", payingAccountOwner, AccountType.School, msgs, dc);
                var masterRegistrant = RegistrantFactory.Create(payingAccount, isMaster: true, msgs, dc);

                //UseStripeConnectedAccount is true for this package
                var package = PackageFactory.CreateFreePackageWithUseStripeConnectedAccount(msgs, dc);
                AccountPackageFactory.Create(billingAccount, package, msgs, dc);

                var activity = ActivityFactory.Create(billingAccount, msgs, dc: dc);
                var registrant = RegistrantFactory.Create(billingAccount, isMaster: false, msgs, dc: dc);
                var registration = RegistrationFactory.Create(billingAccount, registrant, msgs, dc);
                var participationObject = activity.GetParticipationObject(msgs, dc);

                var subscriptionPlan = SubscriptionPlanFactory.Create(billingAccount, activity, SubscriptionPlanType.Subscription, amount, msgs, dc);

                var checkoutObject = new CheckoutObject()
                {
                    BillingAccount = billingAccount,
                    PayingAccount = payingAccount,
                    Coupon = null,
                    OverrideAmount = amount,
                    MasterRegistrant = masterRegistrant,
                    NumberOfSeats = 1,
                    ParticipationObject = participationObject,
                    SubscriptionPlan = subscriptionPlan,
                    PaymentSourceType = PaymentSourceType.AfterSchoolPlatform,
                    PaymentType = PaymentType.CreditCard,
                    SavePaymentInfo = true,
                    Registration = registration
                };

                // Act
                masterRegistrant.ProcessActivityPayment(checkoutObject, null, user, msgs, dc);

                // Assert
                var activityPayment = ActivityPayment.LoadByParticipationObjectByRegistrant(participationObject.ObjectID.Value, 
                    participationObject.ObjectType, 
                    masterRegistrant.ID.Value, 
                    IsDeleted: false, 
                    msgs, 
                    dc).FirstOrDefault();

                Assert.NotNull(activityPayment);
                Assert.Equal(activityPayment?.Amount, amount);            
                Assert.Equal(activityPayment?.ParticipationObjectId, participationObject.ObjectID.Value);            
                Assert.Equal(activityPayment?.RegistrantId, masterRegistrant.ID.Value);

                Assert.Null(activityPayment?.AmountPaidByParent);
                Assert.False(activityPayment?.IsProRated);

                Assert.True(msgs.HasNoErrors());
            }
            catch (Exception ex)
            {
                //Assert
                Assert.False(true, ex.Message);
            }
}

        [Theory(DisplayName = "ProcessActivityPayment with pro-rate Test")]
        [InlineData(100, 83.33)]
        [InlineData(10, 9.1)]
        public void ProcessActivityPaymentWithProRateTest(decimal amountPaidByParent, decimal amountReceivedByProvider)
        {
            try
            {
                // Arrange
                var billingAccountOwner = WebUserFactory.Create("Don", "John", "DonJohn@test.com", true, msgs, dc);
                var payingAccountOwner = WebUserFactory.Create("Cristine", "Trump", "CristineTrump@test.com", true, msgs, dc);
                var billingAccount = AccountFactory.CreateAccount(billingAccountOwner, AccountType.School, stripeChargesEnabled: true, stripePayoutsEnabled: true, msgs, dc);
                var payingAccount = AccountFactory.CreateWithFreePackage("Purchasing account", "PurchasingAccount@test.com", payingAccountOwner, AccountType.School, msgs, dc);
                var masterRegistrant = RegistrantFactory.Create(payingAccount, isMaster: true, msgs, dc);

                //UseStripeConnectedAccount is true for this package
                var package = PackageFactory.CreateFreePackageWithUseStripeConnectedAccount(msgs, dc);
                AccountPackageFactory.Create(billingAccount, package, msgs, dc);

                var activity = ActivityFactory.Create(billingAccount, msgs, dc: dc);
                var registrant = RegistrantFactory.Create(billingAccount, isMaster: false, msgs, dc: dc);
                var registration = RegistrationFactory.Create(billingAccount, registrant, msgs, dc);
                var participationObject = activity.GetParticipationObject(msgs, dc);

                var subscriptionPlan = SubscriptionPlanFactory.Create(billingAccount, activity, SubscriptionPlanType.Subscription, amountPaidByParent, msgs, dc);

                var checkoutObject = new CheckoutObject()
                {
                    BillingAccount = billingAccount,
                    PayingAccount = payingAccount,
                    Coupon = null,
                    AmountPaidByParent = amountPaidByParent,
                    AmountReceivedByProvider = amountReceivedByProvider,
                    IsAmountProRated = true,
                    MasterRegistrant = masterRegistrant,
                    NumberOfSeats = 1,
                    ParticipationObject = participationObject,
                    SubscriptionPlan = subscriptionPlan,
                    PaymentSourceType = PaymentSourceType.AfterSchoolPlatform,
                    PaymentType = PaymentType.CreditCard,
                    SavePaymentInfo = true,
                    Registration = registration
                };

                // Act
                masterRegistrant.ProcessActivityPayment(checkoutObject, null, user, msgs, dc);

                // Assert
                var activityPayment = ActivityPayment.LoadByParticipationObjectByRegistrant(participationObject.ObjectID.Value,
                    participationObject.ObjectType,
                    masterRegistrant.ID.Value,
                    IsDeleted: false,
                    msgs,
                    dc).FirstOrDefault();

                Assert.NotNull(activityPayment);
                Assert.Equal(activityPayment?.Amount, amountReceivedByProvider);
                Assert.Equal(activityPayment?.AmountPaidByParent, amountPaidByParent);
                Assert.Equal(activityPayment?.ParticipationObjectId, participationObject.ObjectID.Value);
                Assert.Equal(activityPayment?.RegistrantId, masterRegistrant.ID.Value);                
                Assert.True(activityPayment?.IsProRated);

                Assert.True(msgs.HasNoErrors());
            }
            catch (Exception ex)
            {
                //Assert
                Assert.False(true, ex.Message);
            }
        }

        [Theory(DisplayName = "ChargeRegistrantMidPeriod Test")]
        [InlineData(100, 83.33, 6, 4)]
        [InlineData(10, 9.1, 0.5, 0.5)]
        public void ChargeRegistrantMidPeriodTest(decimal amountPaidByParent, decimal amountReceivedByProvider, decimal convenienceFee, decimal paymentTypeFee)
        {
            try
            {
                // Arrange
                var billingAccountOwner = WebUserFactory.Create("Don", "John", "DonJohn@test.com", true, msgs, dc);
                var payingAccountOwner = WebUserFactory.Create("Cristine", "Trump", "CristineTrump@test.com", true, msgs, dc);
                var billingAccount = AccountFactory.CreateAccount(billingAccountOwner, AccountType.School, stripeChargesEnabled: true, stripePayoutsEnabled: true, msgs, dc);
                var payingAccount = AccountFactory.CreateAccount(payingAccountOwner, AccountType.School, stripeChargesEnabled: true, stripePayoutsEnabled: true, msgs, dc);
                var masterRegistrant = RegistrantFactory.Create(payingAccount, isMaster: true, msgs, dc);

                //UseStripeConnectedAccount is true for this package
                var package = PackageFactory.CreateFreePackageWithUseStripeConnectedAccount(msgs, dc);
                AccountPackageFactory.Create(billingAccount, package, msgs, dc);
                AccountPackageFactory.Create(payingAccount, package, msgs, dc);

                var schedule = ActivityScheduleFactory.Create(billingAccount, msgs, dc);
                var activity = ActivityFactory.Create(billingAccount, schedule, msgs, dc: dc);
                var startDates = new List<DateRange>()
                {
                    new DateRange() {StartDate = $"6/{DateTime.Now.Month}/{DateTime.Now.Year} 3:00:00 PM".ToDateTime(), EndDate = $"6/{DateTime.Now.Month}/{DateTime.Now.Year} 5:00:00 PM".ToDateTime()},
                    new DateRange() {StartDate = $"7/{DateTime.Now.Month}/{DateTime.Now.Year} 3:00:00 PM".ToDateTime(), EndDate = $"7/{DateTime.Now.Month}/{DateTime.Now.Year} 5:00:00 PM".ToDateTime()},
                    new DateRange() {StartDate = $"8/{DateTime.Now.Month}/{DateTime.Now.Year} 3:00:00 PM".ToDateTime(), EndDate = $"8/{DateTime.Now.Month}/{DateTime.Now.Year} 5:00:00 PM".ToDateTime()},
                    new DateRange() {StartDate = $"6/{DateTime.Now.Month + 1}/{DateTime.Now.Year} 3:00:00 PM".ToDateTime(), EndDate = $"6/{DateTime.Now.Month + 1}/{DateTime.Now.Year} 5:00:00 PM".ToDateTime()},
                    new DateRange() {StartDate = $"7/{DateTime.Now.Month + 1}/{DateTime.Now.Year} 3:00:00 PM".ToDateTime(), EndDate = $"7/{DateTime.Now.Month + 1}/{DateTime.Now.Year} 5:00:00 PM".ToDateTime()},
                    new DateRange() {StartDate = $"8/{DateTime.Now.Month + 1}/{DateTime.Now.Year} 3:00:00 PM".ToDateTime(), EndDate = $"8/{DateTime.Now.Month + 1}/{DateTime.Now.Year} 5:00:00 PM".ToDateTime()},
                    new DateRange() {StartDate = $"6/{DateTime.Now.Month + 2}/{DateTime.Now.Year} 3:00:00 PM".ToDateTime(), EndDate = $"6/{DateTime.Now.Month + 2}/{DateTime.Now.Year} 5:00:00 PM".ToDateTime()},
                    new DateRange() {StartDate = $"7/{DateTime.Now.Month + 2}/{DateTime.Now.Year} 3:00:00 PM".ToDateTime(), EndDate = $"7/{DateTime.Now.Month + 2}/{DateTime.Now.Year} 5:00:00 PM".ToDateTime()},
                    new DateRange() {StartDate = $"8/{DateTime.Now.Month + 2}/{DateTime.Now.Year} 3:00:00 PM".ToDateTime(), EndDate = $"8/{DateTime.Now.Month + 2}/{DateTime.Now.Year} 5:00:00 PM".ToDateTime()},
                    new DateRange() {StartDate = $"6/{DateTime.Now.Month + 3}/{DateTime.Now.Year} 3:00:00 PM".ToDateTime(), EndDate = $"6/{DateTime.Now.Month + 3}/{DateTime.Now.Year} 5:00:00 PM".ToDateTime()},
                    new DateRange() {StartDate = $"7/{DateTime.Now.Month + 3}/{DateTime.Now.Year} 3:00:00 PM".ToDateTime(), EndDate = $"7/{DateTime.Now.Month + 3}/{DateTime.Now.Year} 5:00:00 PM".ToDateTime()},
                    new DateRange() {StartDate = $"8/{DateTime.Now.Month + 3}/{DateTime.Now.Year} 3:00:00 PM".ToDateTime(), EndDate = $"8/{DateTime.Now.Month + 3}/{DateTime.Now.Year} 5:00:00 PM".ToDateTime()},
                    new DateRange() {StartDate = $"6/{DateTime.Now.Month + 4}/{DateTime.Now.Year} 3:00:00 PM".ToDateTime(), EndDate = $"6/{DateTime.Now.Month + 4}/{DateTime.Now.Year} 5:00:00 PM".ToDateTime()},
                    new DateRange() {StartDate = $"7/{DateTime.Now.Month + 4}/{DateTime.Now.Year} 3:00:00 PM".ToDateTime(), EndDate = $"7/{DateTime.Now.Month + 4}/{DateTime.Now.Year} 5:00:00 PM".ToDateTime()},
                    new DateRange() {StartDate = $"8/{DateTime.Now.Month + 4}/{DateTime.Now.Year} 3:00:00 PM".ToDateTime(), EndDate = $"8/{DateTime.Now.Month + 4}/{DateTime.Now.Year} 5:00:00 PM".ToDateTime()},
                };

                var endDates = new List<DateTime>() { };
                ActivityScheduleFactory.Create(billingAccount, activity, schedule, startDates, endDates, user, msgs, dc);

                var registrant = RegistrantFactory.Create(billingAccount, isMaster: false, msgs, dc: dc);
                var registration = RegistrationFactory.Create(billingAccount, registrant, msgs, dc);
                var registrantActivity = RegistrantActivityFactory.Create(activity, registrant, msgs, dc: dc);
                var subscriptionPlan = SubscriptionPlanFactory.Create(billingAccount, activity, SubscriptionPlanType.Subscription, amountPaidByParent, msgs, dc);
                var purchase = PurchaseFactory.Create(payingAccount, amountReceivedByProvider, registrantId: registrant.ID.Value, registrationId: registration.ID.Value, msgs, dc);
                var registrantSubscriptionPlan = RegistrantSubscriptionPlanFactory.Create(registrant, subscriptionPlan, purchase, amount: amountReceivedByProvider, convenienceFee: convenienceFee, paymentTypeFee: paymentTypeFee, msgs, dc);
                var participationObject = activity.GetParticipationObject(msgs, dc);
                
                var stripeBillingObject = AccountFactory.CreateStripeAccountAndCustomer(billingAccount, msgs, dc);
                var stripePayingObject = AccountFactory.CreateStripeAccountAndCustomer(payingAccount, msgs, dc);
                var sourceId = payingAccount.CreateStripeSourceFromToken(StripeHelper.TestSourceToken, stripePayingObject.Item2, msgs, dc);

                CreditCardPurchaseObject creditCardPurchaseObject = new CreditCardPurchaseObject()
                {
                    StripeToken = StripeHelper.TestSourceToken,
                    StripeSourceId = sourceId
                };

                Account convenienceFeePayer;
                Account transactionFeePayer;

                if (registrantSubscriptionPlan.ConvenienceFeesPaidBy == FeesPaidByType.Parent)
                    convenienceFeePayer = payingAccount;
                else
                    convenienceFeePayer = billingAccount;

                if (registrantSubscriptionPlan.PaymentTypeFeesPaidBy == FeesPaidByType.Parent)
                    transactionFeePayer = payingAccount;
                else
                    transactionFeePayer = billingAccount;

                var checkoutObject = new CheckoutObject()
                {
                    BillingAccount = billingAccount,
                    PayingAccount = payingAccount,
                    Coupon = null,
                    AmountPaidByParent = amountPaidByParent,
                    AmountReceivedByProvider = amountReceivedByProvider,
                    IsAmountProRated = true,
                    MasterRegistrant = registrant,
                    NumberOfSeats = 1,
                    ParticipationObject = participationObject,
                    SubscriptionPlan = subscriptionPlan,
                    PaymentSourceType = PaymentSourceType.AfterSchoolPlatform,
                    PaymentType = PaymentType.CreditCard,
                    CreditCardPurchaseObject = creditCardPurchaseObject,
                    ConvenienceFeePayer = convenienceFeePayer,
                    CreditCardFeePayer = transactionFeePayer,
                    SavePaymentInfo = true,
                    Registration = registration
                };

                // Act
                masterRegistrant.ChargeRegistrantMidPeriod(checkoutObject, SubscriptionPlanType.Subscription, isSubscriptionRequired: true, user, msgs, dc);

                // Assert
                //get the total number of periods for current cycle x
                var classesInCurrentCycle = participationObject.GetNumberOfClassesInCurrentCycle(subscriptionPlan.BillingPeriodTypeId, msgs, dc);

                //get the total remaining periods in current cycle y
                var classesRemainingInCurrentCycle = participationObject.GetNumberOfRemainingClassesInCurrentCycle(subscriptionPlan.BillingPeriodTypeId, msgs, dc);

                //caluclated price of current cycle to charge p = (y * costperperiod)/ x
                var currentCycleCharge = classesRemainingInCurrentCycle * subscriptionPlan.CostPerPeriod.Value / classesInCurrentCycle;

                if (classesInCurrentCycle <= 0 || classesRemainingInCurrentCycle <= 0)
                {
                    Assert.Equal(0, currentCycleCharge);
                    Assert.True(msgs.HasNoErrors());
                }
                else
                {
                    var calcObj = new FeeCalculationObject(currentCycleCharge, billingAccount, msgs, dc);
                    var convenienceFeeCharged = calcObj.GetConvenienceFee();
                    var ccFee = 0M;
                    if (checkoutObject.PaymentType == PaymentType.CreditCard)
                        ccFee = calcObj.GetCreditCardFee();

                    var amount = calcObj.GetFinalTotalCost(paymentType: checkoutObject.PaymentType);

                    var activityPayment = ActivityPayment.LoadByParticipationObjectByRegistrant(participationObject.ObjectID.Value,
                        participationObject.ObjectType,
                        registrant.ID.Value,
                        IsDeleted: false,
                        msgs,
                        dc).FirstOrDefault();

                    Assert.NotNull(activityPayment);
                    Assert.Equal(activityPayment?.Amount, calcObj.GetAmountToProvider());
                    Assert.Equal(activityPayment?.AmountPaidByParent, amount);
                    Assert.Equal(activityPayment?.ParticipationObjectId, participationObject.ObjectID.Value);
                    Assert.Equal(activityPayment?.RegistrantId, registrant.ID.Value);
                    Assert.True(activityPayment?.IsProRated);

                    //purchase
                    var purchaseStored = registration.GetPurchase(msgs, dc);
                    Assert.NotNull(purchaseStored);
                    Assert.Equal(purchaseStored?.AmountPaid, amount);
                    Assert.Equal(purchaseStored?.RegistrationId, registration.ID.Value);
                    Assert.Equal(purchaseStored?.RegistrantId, masterRegistrant.ID.Value);
                    Assert.Equal(purchaseStored?.PurchaserAccountId, payingAccount.ID.Value);

                    //transaction
                    var transaction = Transaction.LoadByBillerAccountIdByIsDeleted(BillerAccountId: billingAccount.ID.Value, IsDeleted: false, msgs, dc).FirstOrDefault();
                    Assert.NotNull(transaction);
                    Assert.Equal(transaction?.PurchaseId, purchaseStored.ID.Value);
                    Assert.Equal(transaction?.Amount, amount);
                    Assert.Equal(transaction?.PaymentTypeFeeCharged, ccFee);
                    Assert.Equal(transaction?.ConvenienceFeeCharged, convenienceFeeCharged);
                    Assert.Equal(transaction?.PaymentTypeFeePayerAccountId, transactionFeePayer.ID.Value);
                    Assert.Equal(transaction?.ConvenienceFeePayerAccountId, convenienceFeePayer.ID.Value);
                    Assert.Equal(transaction?.BillerAccountId, billingAccount.ID.Value);
                    Assert.Equal(transaction?.PayerAccountId, payingAccount.ID.Value);

                    //check seat status
                    var ra = RegistrantActivity.LoadByParticipationObjectByRegistrant(checkoutObject.ParticipationObject.ObjectID.Value,
                        checkoutObject.ParticipationObject.ObjectType,
                        registrant.ID.Value,
                        msgs,
                        dc);
                    Assert.True(ra.IsAssigned(RegistrationAssignmentType.Reserved));

                    //stripe checks
                    var product = StripeHelper.GetProduct(activity.Uid.ToString(), stripeAccountId: null, msgs);
                    Assert.NotNull(product);

                    var customer = payingAccount.GetStripeCustomer(msgs);
                    Assert.NotNull(customer);

                    var charge = StripeHelper.GetCharge(transaction.StripeChargeOrRefundId, msgs);
                    Assert.Equal(charge.Amount, amount * 100); //since stripe returns this way (amount * 100)

                    Assert.True(msgs.HasNoErrors());
                }
            }
            catch (Exception ex)
            {
                //Assert
                Assert.False(true, ex.Message);
            }
        }
    }
}
