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

namespace AfterSchoolLibraryTests.Tests
{
    public class RegistrantServiceTest : BaseTest
    {
        private WebUser User { get; }

        public RegistrantServiceTest(ITestOutputHelper output) : base(output)
        {
            User = WebUserFactory.Create(msgs, dc);
        }

        [Theory(DisplayName = "StartSubscriptionPlan Test")]
        [InlineData(110)]
        [InlineData(12)]
        public void StartSubscriptionPlanTest(decimal amount)
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
            var subscriptionPlan = SubscriptionPlanFactory.Create(billingAccount, activity, SubscriptionPlanType.Subscription, amount, msgs, dc);

            AccountFactory.CreateStripeAccountAndCustomer(billingAccount, msgs, dc);
            var stripePayingObject = AccountFactory.CreateStripeAccountAndCustomer(payingAccount, msgs, dc);
            var sourceId = payingAccount.CreateStripeSourceFromToken(StripeHelper.TestSourceToken, stripePayingObject.Item2, msgs, dc);

            var creditCardPurchaseObject = new CreditCardPurchaseObject()
            {
                StripeToken = StripeHelper.TestSourceToken,
                StripeSourceId = sourceId
            };

            // Will wait for 10 seconds for stripe to create this account
            DelayExecution(10000);

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
        }

        [Theory(DisplayName = "StartPaymentPlan Test")]
        [InlineData(110)]
        [InlineData(12)]
        public void StartPaymentPlanTest(decimal amount)
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
            var paymentPlan = SubscriptionPlanFactory.CreatePaymentPlan(planOwnerAccount: billingAccount,
                activity: activity,
                costPerPeriod: amount,
                totalPeriods: 1,
                billingPeriodType: BillingPeriodType.Month,
                planPaymentStartType: PlanPaymentStartType.OnCheckout,
                msgs: msgs,
                dc: dc);

            AccountFactory.CreateStripeAccountAndCustomer(billingAccount, msgs, dc);
            var stripePayingObject = AccountFactory.CreateStripeAccountAndCustomer(payingAccount, msgs, dc);
            var sourceId = payingAccount.CreateStripeSourceFromToken(StripeHelper.TestSourceToken, stripePayingObject.Item2, msgs, dc);

            var creditCardPurchaseObject = new CreditCardPurchaseObject()
            {
                StripeToken = StripeHelper.TestSourceToken,
                StripeSourceId = sourceId
            };

            // Will wait for 10 seconds for stripe to create this account
            DelayExecution(10000);

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

            //Stripe
            var product = StripeHelper.GetProduct(activity.Uid.ToString(), stripeAccountId: null, msgs);
            Assert.NotNull(product);

            Assert.True(msgs.HasNoErrors());
        }

        [Theory(DisplayName = "ProcessActivityPayment without pro-rate Test")]
        [InlineData(110)]
        [InlineData(12)]
        public void ProcessActivityPaymentWithoutProRateTest(decimal amount)
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
            masterRegistrant.ProcessActivityPayment(checkoutObject, null, User, msgs, dc);

            // Assert
            var activityPayment = ActivityPayment.LoadByParticipationObjectByRegistrant(participationObject.ObjectID.Value,
                participationObject.ObjectType,
                masterRegistrant.ID.Value,
                IsDeleted: false,
                msgs,
                dc).FirstOrDefault();

            Assert.NotNull(activityPayment);
            Assert.Equal(activityPayment.Amount, amount);
            Assert.Equal(activityPayment.ParticipationObjectId, participationObject.ObjectID.Value);
            Assert.Equal(activityPayment.RegistrantId, masterRegistrant.ID.Value);

            Assert.Null(activityPayment.AmountPaidByParent);
            Assert.False(activityPayment.IsProRated);

            Assert.True(msgs.HasNoErrors());
        }

        [Theory(DisplayName = "ProcessActivityPayment with pro-rate Test")]
        [InlineData(100, 83.33)]
        [InlineData(10, 9.1)]
        public void ProcessActivityPaymentWithProRateTest(decimal amountPaidByParent, decimal amountReceivedByProvider)
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
            masterRegistrant.ProcessActivityPayment(checkoutObject, null, User, msgs, dc);

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

        [Theory]
        [InlineData(100, 83.33, 6, 4)]
        [InlineData(10, 9.1, 0.5, 0.5)]
        public void ChargeRegistrantMidPeriodTest(decimal amountPaidByParent, decimal amountReceivedByProvider, decimal convenienceFee, decimal paymentTypeFee)
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

            var schedule = ActivityScheduleFactory.Create(billingAccount, DateTime.UtcNow.AddDays(-10), DateTime.UtcNow.AddDays(80), msgs, dc);
            var activity = ActivityFactory.Create(billingAccount, schedule, msgs, dc: dc);
            var tsOfSchedule = DateTime.UtcNow.AddDays(60) - DateTime.UtcNow.AddDays(-10);
            var startDates = new List<DateRange>();

            for (var i = -10; i < tsOfSchedule.TotalDays - 10; i++)
            {
                startDates.Add(new DateRange()
                {
                    StartDate = $"{DateTime.UtcNow.AddDays(i).Month}/{DateTime.UtcNow.AddDays(i).Day}/{DateTime.UtcNow.AddDays(i).Year} 3:00:00 PM".ToDateTime(),
                    EndDate = $"{DateTime.UtcNow.AddDays(i).Month}/{DateTime.UtcNow.AddDays(i).Day}/{DateTime.UtcNow.AddDays(i).Year} 5:00:00 PM".ToDateTime()
                });
            }

            var endDates = new List<DateTime>() { };
            ActivityScheduleFactory.Create(billingAccount, activity, schedule, startDates, endDates, User, msgs, dc);

            var registrant = RegistrantFactory.Create(billingAccount, isMaster: false, msgs, dc: dc);
            var registration = RegistrationFactory.Create(billingAccount, registrant, msgs, dc);
            RegistrantActivityFactory.Create(activity, registrant, msgs, dc: dc);
            var subscriptionPlan = SubscriptionPlanFactory.Create(billingAccount, activity, SubscriptionPlanType.Subscription, amountPaidByParent, msgs, dc);
            var purchase = PurchaseFactory.Create(payingAccount, amountReceivedByProvider, registrantId: registrant.ID.Value, registrationId: registration.ID.Value, msgs, dc);
            var registrantSubscriptionPlan = RegistrantSubscriptionPlanFactory.Create(registrant, subscriptionPlan, purchase, amount: amountReceivedByProvider, convenienceFee: convenienceFee, paymentTypeFee: paymentTypeFee, msgs, dc);
            var participationObject = activity.GetParticipationObject(msgs, dc);

            AccountFactory.CreateStripeAccountAndCustomer(billingAccount, msgs, dc);
            var stripePayingObject = AccountFactory.CreateStripeAccountAndCustomer(payingAccount, msgs, dc);
            var sourceId = payingAccount.CreateStripeSourceFromToken(StripeHelper.TestSourceToken, stripePayingObject.Item2, msgs, dc);

            var creditCardPurchaseObject = new CreditCardPurchaseObject()
            {
                StripeToken = StripeHelper.TestSourceToken,
                StripeSourceId = sourceId
            };

            // Will wait for 10 seconds for stripe to create this account
            DelayExecution(10000);

            Account convenienceFeePayer;
            Account transactionFeePayer;

            convenienceFeePayer = registrantSubscriptionPlan.ConvenienceFeesPaidBy == FeesPaidByType.Parent
                ? payingAccount
                : billingAccount;

            transactionFeePayer = registrantSubscriptionPlan.PaymentTypeFeesPaidBy == FeesPaidByType.Parent
                ? payingAccount
                : billingAccount;

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
            masterRegistrant.ChargeRegistrantMidPeriod(checkoutObject, SubscriptionPlanType.Subscription, isSubscriptionRequired: true, User, msgs, dc);

            // Assert
            //get the total number of periods for current cycle x
            var classesInCurrentCycle = participationObject.GetNumberOfClassesInCurrentCycle(subscriptionPlan.BillingPeriodTypeId, msgs, dc);

            //get the total remaining periods in current cycle y
            var classesRemainingInCurrentCycle = participationObject.GetNumberOfRemainingClassesInCurrentCycle(subscriptionPlan.BillingPeriodTypeId, msgs, dc);

            //calculated price of current cycle to charge p = (y * costperperiod)/ x
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
                Assert.Equal(activityPayment.Amount, Math.Round(calcObj.GetAmountToProvider(), 4));
                Assert.Equal(activityPayment.AmountPaidByParent, Math.Round(amount, 4));
                Assert.Equal(activityPayment.ParticipationObjectId, participationObject.ObjectID.Value);
                Assert.Equal(activityPayment.RegistrantId, registrant.ID.Value);
                Assert.True(activityPayment.IsProRated);

                //purchase
                var purchaseStored = registration.GetPurchase(msgs, dc);
                Assert.NotNull(purchaseStored);
                Assert.Equal(purchaseStored.AmountPaid, Math.Round(amount, 4));
                Assert.Equal(purchaseStored.RegistrationId, registration.ID.Value);
                Assert.Equal(purchaseStored.RegistrantId, masterRegistrant.ID.Value);
                Assert.Equal(purchaseStored.PurchaserAccountId, payingAccount.ID.Value);

                //transaction
                var transaction = Transaction.LoadByBillerAccountIdByIsDeleted(BillerAccountId: billingAccount.ID.Value, IsDeleted: false, msgs, dc).FirstOrDefault();
                Assert.NotNull(transaction);
                Assert.Equal(transaction.PurchaseId, purchaseStored.ID.Value);
                Assert.Equal(transaction.Amount, Math.Round(amount, 4));
                Assert.Equal(transaction.PaymentTypeFeeCharged, Math.Round(ccFee, 2));
                Assert.Equal(transaction.ConvenienceFeeCharged, Math.Round(convenienceFeeCharged, 2));
                Assert.Equal(transaction.PaymentTypeFeePayerAccountId, transactionFeePayer.ID.Value);
                Assert.Equal(transaction.ConvenienceFeePayerAccountId, convenienceFeePayer.ID.Value);
                Assert.Equal(transaction.BillerAccountId, billingAccount.ID.Value);
                Assert.Equal(transaction.PayerAccountId, payingAccount.ID.Value);

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
                Assert.Equal(charge.Amount, Math.Round(amount * 100, 4)); //since stripe returns this way (amount * 100)

                Assert.True(msgs.HasNoErrors());
            }
        }

        [Fact(DisplayName = "StartPaymentPlanTest")]
        public void StartPlanTest()
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

            var schedule = ActivityScheduleFactory.Create(billingAccount, DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddDays(1), msgs, dc);
            var activity = ActivityFactory.Create(billingAccount, schedule, msgs, dc: dc);
            var registrant = RegistrantFactory.Create(billingAccount, isMaster: false, msgs, dc: dc);
            var registration = RegistrationFactory.Create(billingAccount, registrant, msgs, dc);
            var participationObject = activity.GetParticipationObject(msgs, dc);

            var stripePurchasingAccount = AccountFactory.CreateStripeAccountAndCustomer(account: payingAccount, msgs: msgs, dc: dc);
            var sourceId = payingAccount.CreateStripeSourceFromToken(StripeHelper.TestSourceToken, stripePurchasingAccount.Item2, msgs, dc);
            var creditCardPurchaseObject = new CreditCardPurchaseObject() { StripeToken = StripeHelper.TestSourceToken, StripeSourceId = sourceId };
            var costPerPeriod = 100;
            var paymentPlan = SubscriptionPlanFactory.CreatePaymentPlan(billingAccount, activity, costPerPeriod, 5, msgs, dc);

            var checkoutObject = new CheckoutObject()
            {
                BillingAccount = billingAccount,
                PayingAccount = payingAccount,
                Coupon = null,
                OverrideAmount = 100,
                MasterRegistrant = masterRegistrant,
                NumberOfSeats = 1,
                ParticipationObject = participationObject,
                PaymentPlan = paymentPlan,
                PaymentSourceType = PaymentSourceType.AfterSchoolPlatform,
                PaymentType = PaymentType.CreditCard,
                SavePaymentInfo = true,
                Registration = registration,
                CreditCardPurchaseObject = creditCardPurchaseObject
            };

            // Act
            registrant.StartPaymentPlan(checkoutObject, payingAccountOwner, msgs, dc);
            var registrantSubscriptionPlan = RegistrantSubscriptionPlan.LoadByRegistrantId(registrant.ID.Value, msgs, dc).First();
            var stripeSubscription = StripeHelper.GetSubscription(registrantSubscriptionPlan.StripeSubscriptionId, null, msgs);
            var stripeInvoice = StripeHelper.GetInvoice(stripeSubscription.LatestInvoiceId, msgs);

            // Assert
            // payment plan has stripeId
            Assert.True(paymentPlan.StripePlanId.IsNotEmpty());
            // was payment made
            Assert.True(stripeInvoice.IsNotEmpty() && stripeInvoice.Paid);
            // is payment amount correct (this is how we assume the payment plan was not prorated)
            Assert.Equal(expected: checkoutObject.GetPaymentPlanCostTodayWithFees(msgs, dc) * 100, actual: stripeInvoice.AmountPaid);
            // payment plan billing type should match stripe subscription interval 
            Assert.Equal(expected: paymentPlan.BillingPeriodTypeId.ToString().ToLower(), actual: stripeSubscription.Plan.Interval);
            // payment plan should have an end date
            Assert.True(paymentPlan.EndDate.IsNotEmpty());
            // payment plan end date should match the stripe subscription end date
            Assert.Equal(expected: paymentPlan.EndDate.Value.Date, actual: stripeSubscription.CancelAt.Value.Date);
        }
    }
}
