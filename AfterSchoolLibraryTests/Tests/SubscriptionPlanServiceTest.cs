using Xunit;
using Xunit.Abstractions;
using System;
using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using BasicLibrary;
using AfterSchoolLibraryTests.Factories;
using AfterSchoolLibrary.Billing;
using System.Collections.Generic;
using System.Linq;

namespace AfterSchoolLibraryTests.Tests
{
    public class SubscriptionPlanServiceTest : BaseTest
    {
        private WebUser LoggedInUser { get; set; }
        private WebUser AccountOwner { get; set; }
        private Account PlanOwner { get; set; }
        private Package Package { get; set; }

        private DateTime Now { get; set; }
        
        public SubscriptionPlanServiceTest(ITestOutputHelper output) : base(output)
        {
            LoggedInUser = WebUserFactory.Create(msgs, dc);
            AccountOwner = WebUserFactory.Create(msgs, dc);
            PlanOwner = AccountFactory.Create(AccountOwner, msgs, dc);
            Package = PackageFactory.CreateFreePackage(msgs, dc);
            Now = DateTime.UtcNow;
        }

        [Fact]
        public void CreateNewActivityObjectTest()
        {
            var activity = ActivityFactory.Create(PlanOwner, msgs, dc);

            var plan = SubscriptionPlanService.CreateNewSubscriptionPlan(
                        PlanOwner,
                        ParticipationObjectType.Activity,
                        activity.ID.Value,
                        35.ToDecimal(),
                        BillingPeriodType.Month,
                        true,
                        false,
                        false,
                        false,
                        LoggedInUser.ID.Value,
                        msgs,
                        dc);

            Assert.True(plan.ID.HasValue);
        }

        [Fact(DisplayName = "Create new Activity Group")]
        public void CreateNewActivityGroupTest()
        {
            var activityGroup = ActivityGroupFactory.Create(PlanOwner, ActivityGroupType.Session, msgs, dc);

            var plan = SubscriptionPlanService.CreateNewSubscriptionPlan(
                        PlanOwner,
                        ParticipationObjectType.ActivityGroup,
                        activityGroup.ID.Value,
                        35.ToDecimal(),
                        BillingPeriodType.Month,
                        true,
                        false,
                        false,
                        false,
                        LoggedInUser.ID.Value,
                        msgs,
                        dc);

            Assert.True(plan.ID.HasValue);
        }

        [Theory(DisplayName = "CheckAndCancelPaymentPlanTest")]
        [InlineData(2, 2, true)]
        [InlineData(2, 1, false)]
        [InlineData(2, 3, true)]
        public void CheckAndCancelPaymentPlanTest(int totalPeriods, int totalPayments, bool expectedResult)
        {
            var payerUser = WebUserFactory.Create(msgs, dc);
            var payerAccount = AccountFactory.Create(payerUser, msgs, dc);
            var activity = ActivityFactory.Create(PlanOwner, msgs, dc);
            var paymentPlan = SubscriptionPlanFactory.CreatePaymentPlan(
                                PlanOwner, 
                                activity, 
                                100, 
                                totalPeriods, 
                                msgs, 
                                dc);
            var purchase = PurchaseFactory.Create(payerAccount, 100, msgs, dc);
            var registrant = RegistrantFactory.Create(msgs, dc);
            var registrantSubscriptionPlan = RegistrantSubscriptionPlanFactory.Create(registrant, paymentPlan, purchase, msgs, dc);
            var transaction = TransactionFactory.Create(PlanOwner, payerAccount, PaymentType.CreditCard, msgs, dc);

            for (var i = 0; i < totalPayments; i++)
                ActivityPaymentFactory.Create(PlanOwner, activity, registrantSubscriptionPlan, transaction, msgs, dc);

            Assert.Equal(expectedResult, paymentPlan.CheckAndCancelPlan(registrantSubscriptionPlan, PlanOwner, msgs, dc));
        }

        [Fact]
        public void CreateNewSubscriptionPlanTest()
        {
            var activity = ActivityFactory.Create(PlanOwner, msgs, dc);            
            var accountPackage = AccountPackageFactory.Create(PlanOwner, Package, msgs, dc);
         
            var plan = SubscriptionPlanService.CreateNewSubscriptionPlan(
                               PlanOwner, 
                               ParticipationObjectType.Activity,
                               activity.ID.Value,
                               100, 
                               BillingPeriodType.Month,
                               PlanPaymentStartType.OnCheckout, 
                               null, 
                               null, 
                               true, 
                               true, 
                               false,
                               false,
                               true,
                               LoggedInUser.ID.Value,
                               msgs, 
                               dc);

            Assert.True(plan.ID.HasValue);
            Assert.True(msgs.HasNoErrors());
        }


        [Fact(DisplayName = "GetPlanEndDateNotSetPostPayTest")]
        public void GetPlanEndDateNotSetPostPayTest()
        {
            // Arrange
            var activity = ActivityFactory.Create(PlanOwner, msgs, dc);
            var planTypeNotSet = SubscriptionPlanFactory.Create(PlanOwner, activity, SubscriptionPlanType.NotSet, msgs, dc);
            var planTypePostPay = SubscriptionPlanFactory.Create(PlanOwner, activity, SubscriptionPlanType.PostPayPlan, msgs, dc);

            // Act
            var endDatePlanTypeNotSet = planTypeNotSet.GetPlanEndDateForRegistrantSubscription(Now, activity.GetParticipationObject(msgs, dc), msgs);
            var endDatePlanTypePostPay = planTypeNotSet.GetPlanEndDateForRegistrantSubscription(Now, activity.GetParticipationObject(msgs, dc), msgs);

            // Assert
            Assert.False(endDatePlanTypeNotSet.HasValue);
            Assert.False(endDatePlanTypePostPay.HasValue);
        }

        [Fact(DisplayName ="GetPlanEndDateSubscriptionTest")]
        public void GetPlanEndDateSubscriptionTest()
        {
            // Arrange
            var schedule = ActivityScheduleFactory.Create(PlanOwner, msgs, dc);
            var activityWithSchedule = ActivityFactory.Create(PlanOwner, schedule, msgs, dc);
            var onGoingActivity = ActivityFactory.Create(PlanOwner, msgs, dc);
            var planScheduleSubscription = SubscriptionPlanFactory.Create(PlanOwner, activityWithSchedule, SubscriptionPlanType.Subscription, msgs, dc);
            var planOnGoingSubscription = SubscriptionPlanFactory.Create(PlanOwner, onGoingActivity, SubscriptionPlanType.Subscription, msgs, dc);

            // Act
            var endDateSchedule = planScheduleSubscription.GetPlanEndDateForRegistrantSubscription(Now, activityWithSchedule.GetParticipationObject(msgs, dc), msgs);
            var endDateOnGoing = planOnGoingSubscription.GetPlanEndDateForRegistrantSubscription(Now, onGoingActivity.GetParticipationObject(msgs, dc), msgs);


            // Assert
            Assert.True(endDateSchedule.HasValue);
            Assert.Equal(expected: schedule.EndDate?.AddDays(1).Date, actual: endDateSchedule.Value.Date);
            Assert.False(endDateOnGoing.HasValue);

        }


        [Theory(DisplayName = "GetPlanEndDatePaymentPlanBillingPeriodDayTest")]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(10)]
        public void GetPlanEndDatePaymentPlanBillingPeriodDayTest(int totalPeriods)
        {
            // Arrange
            var activity = ActivityFactory.Create(PlanOwner, msgs, dc);
            var planPaymentPlan = SubscriptionPlanFactory.CreatePaymentPlan(PlanOwner, activity, 10, totalPeriods, BillingPeriodType.Day, msgs, dc);
            var expected = Now.Date.AddDays(totalPeriods);

            if (totalPeriods == 0)
                expected = Now.Date.AddDays(1);

            // Act
            var endDate = planPaymentPlan.GetPlanEndDateForRegistrantSubscription(Now, activity.GetParticipationObject(msgs, dc), msgs);            

            // Assert
            Assert.True(endDate.HasValue);
            Assert.True(msgs.HasNoErrors());
            Assert.Equal(expected: expected.Date, actual: endDate.Value.Date);
        }

        [Theory(DisplayName = "GetPlanEndDatePaymentPlanBillingPeriodWeekTest")]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(10)]
        public void GetPlanEndDatePaymentPlanBillingPeriodWeekTest(int totalPeriods)
        {
            // Arrange
            var activity = ActivityFactory.Create(PlanOwner, msgs, dc);
            var planPaymentPlan = SubscriptionPlanFactory.CreatePaymentPlan(PlanOwner, activity, 10, totalPeriods, BillingPeriodType.Week, msgs, dc);
            var expected = Now.Date.AddWeeks(totalPeriods);

            if (totalPeriods == 0)
                expected = Now.Date.AddDays(1);

            // Act
            var endDate = planPaymentPlan.GetPlanEndDateForRegistrantSubscription(Now, activity.GetParticipationObject(msgs, dc), msgs);

            // Assert
            Assert.True(endDate.HasValue);
            Assert.True(msgs.HasNoErrors());
            Assert.Equal(expected: expected.Date, actual: endDate.Value.Date);
        }        

        [Theory(DisplayName = "GetPlanEndDatePaymentPlanBillingPeriodTwoWeekTest")]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(10)]
        public void GetPlanEndDatePaymentPlanBillingPeriodTwoWeekTest(int totalPeriods)
        {
            // Arrange
            var activity = ActivityFactory.Create(PlanOwner, msgs, dc);
            var planPaymentPlan = SubscriptionPlanFactory.CreatePaymentPlan(PlanOwner, activity, 10, totalPeriods, BillingPeriodType.TwoWeeks, msgs, dc);
            var expected = Now.Date.AddWeeks(totalPeriods * 2);

            if (totalPeriods == 0)
                expected = Now.Date.AddDays(1);

            // Act
            var endDate = planPaymentPlan.GetPlanEndDateForRegistrantSubscription(Now, activity.GetParticipationObject(msgs, dc), msgs);

            // Assert
            Assert.True(endDate.HasValue);
            Assert.True(msgs.HasNoErrors());
            Assert.Equal(expected: expected.Date, actual: endDate.Value.Date);
        }

        [Theory(DisplayName = "GetPlanEndDatePaymentPlanBillingPeriodMonthTest")]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(10)]
        public void GetPlanEndDatePaymentPlanBillingPeriodMonthTest(int totalPeriods)
        {
            // Arrange
            var activity = ActivityFactory.Create(PlanOwner, msgs, dc);
            var planPaymentPlan = SubscriptionPlanFactory.CreatePaymentPlan(PlanOwner, activity, 10, totalPeriods, BillingPeriodType.Month, msgs, dc);
            var expected = Now.Date.AddMonths(totalPeriods);

            if (totalPeriods == 0)
                expected = Now.Date.AddDays(1);

            // Act
            var endDate = planPaymentPlan.GetPlanEndDateForRegistrantSubscription(Now, activity.GetParticipationObject(msgs, dc), msgs);

            // Assert
            Assert.True(endDate.HasValue);
            Assert.True(msgs.HasNoErrors());
            Assert.Equal(expected: expected.Date, actual: endDate.Value.Date);
        }

        [Theory(DisplayName = "GetPlanEndDatePaymentPlanBillingPeriodYearTest")]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(10)]
        public void GetPlanEndDatePaymentPlanBillingPeriodYearTest(int totalPeriods)
        {
            // Arrange
            var activity = ActivityFactory.Create(PlanOwner, msgs, dc);
            var planPaymentPlan = SubscriptionPlanFactory.CreatePaymentPlan(PlanOwner, activity, 10, totalPeriods, BillingPeriodType.Year, msgs, dc);
            var expected = Now.Date.AddYears(totalPeriods);

            if (totalPeriods == 0)
                expected = Now.Date.AddDays(1);

            // Act
            var endDate = planPaymentPlan.GetPlanEndDateForRegistrantSubscription(Now, activity.GetParticipationObject(msgs, dc), msgs);

            // Assert
            Assert.True(endDate.HasValue);
            Assert.True(msgs.HasNoErrors());
            Assert.Equal(expected: expected.Date, actual: endDate.Value.Date);
        }

        /// <summary>
        /// Activity schedule period is small for subscription to start, so just charge registrant directly instead for Subscription plan.
        /// </summary>
        /// <param name="costPerPeriod"></param>
        [Theory(DisplayName = "Case 1: SubscriptionPlanType.Subscription, PlanPaymentStartType.OnCheckout, BillingPeriodType.Month, Only charge")]
        [InlineData(100)]        
        public void SubscriptionPlanDueOnRegistrationCheckoutTest1(int costPerPeriod)
        {
            // Arrange
            var billingAccountOwner = WebUserFactory.Create("Don", "John", "DonJohn@test.com", true, msgs, dc);
            var payingAccountOwner = WebUserFactory.Create("Cristine", "Trump", "CristineTrump@test.com", true, msgs, dc);
            var billingAccount = AccountFactory.CreateAccount(billingAccountOwner, AccountType.School, stripeChargesEnabled: true, stripePayoutsEnabled: true, msgs, dc);
            var payingAccount = AccountFactory.CreateWithFreePackage("Purchasing account", "PurchasingAccount@test.com", payingAccountOwner, AccountType.School, msgs, dc);
            var masterRegistrant = RegistrantFactory.Create(payingAccount, isMaster: true, msgs, dc);
            var registrant = RegistrantFactory.Create(billingAccount, isMaster: false, msgs, dc: dc);
            var registration = RegistrationFactory.Create(billingAccount, registrant, msgs, dc);

            // create a schedule of just a month
            var schedule = ActivityScheduleFactory.Create(billingAccount, DateTime.UtcNow.AddDays(-1), DateTime.Now.AddDays(31), msgs, dc);
            var activity = ActivityFactory.Create(billingAccount, schedule, msgs, dc: dc);            
            ActivityScheduleFactory.Create(billingAccount, activity, schedule, billingAccountOwner, msgs, dc);

            var participantObject = activity.GetParticipationObject(msgs: msgs, dc: dc);
            var subscriptionPlan = SubscriptionPlanFactory.Create(planOwnerAccount: billingAccount, 
                activity: activity,
                costPerPeriod: costPerPeriod,
                planType: SubscriptionPlanType.Subscription,
                billingPeriodType: BillingPeriodType.Month,
                planPaymentStartType: PlanPaymentStartType.OnCheckout,
                msgs: msgs, 
                dc: dc);

            /// Update stripe account and customer of payer/parent
            var stripePurchasingAccount = AccountFactory.CreateStripeAccountAndCustomer(account: payingAccount, msgs: msgs, dc: dc);
            var sourceId = payingAccount.CreateStripeSourceFromToken(StripeHelper.TestSourceToken, stripePurchasingAccount.Item2, msgs, dc);
            var creditCardPurchaseObject = new CreditCardPurchaseObject() { StripeToken = StripeHelper.TestSourceToken, StripeSourceId = sourceId };

            var checkoutObject = new CheckoutObject()
            {
                PayingAccount = payingAccount,
                BillingAccount = billingAccount,
                ParticipationObject = participantObject,
                PaymentSourceType = PaymentSourceType.AfterSchoolPlatform,
                PaymentType = PaymentType.CreditCard,
                CreditCardPurchaseObject = creditCardPurchaseObject,
                SubscriptionPlan = subscriptionPlan,
                MasterRegistrant = masterRegistrant,
                NumberOfSeats = 1,                
                Registration = registration,
                ConvenienceFeePayer = payingAccount,
                CreditCardFeePayer = payingAccount,
                SavePaymentInfo = true                
            };

            //Act
            masterRegistrant.StartSubscriptionPlan(
                checkoutObject: checkoutObject,
                loggedInUser: LoggedInUser,
                msgs: msgs,
                dc: dc);

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

            var startDateTime = masterRegistrant.GetPlanStartDate(subscriptionPlan, participantObject, msgs, dc);
            var endDateTime = subscriptionPlan.GetPlanEndDateForRegistrantSubscription(startDateTime, participantObject, msgs);
            var isSubscriptionRequired = true;

            if (startDateTime.IsNotEmpty() && endDateTime.IsNotEmpty())
                isSubscriptionRequired = subscriptionPlan.CheckIfSubscriptionRequired(startDateTime, endDateTime.Value);

            Assert.False(isSubscriptionRequired);

            //subscription plan
            Assert.NotNull(subscriptionPlan);
            Assert.Equal(subscriptionPlan?.ParticipationObjectId, participantObject.ObjectID.Value);
            Assert.Equal(subscriptionPlan?.ParticipationObjectTypeId, participantObject.ObjectType);
            Assert.Equal(subscriptionPlan?.CostPerPeriod, costPerPeriod);
            Assert.Equal(subscriptionPlan?.PlanOwnerAccountId, billingAccount.ID.Value);
            Assert.Equal(BillingPeriodType.Month, subscriptionPlan?.BillingPeriodTypeId);
            Assert.Equal(SubscriptionPlanType.Subscription, subscriptionPlan?.PlanTypeId);

            var classesToCharge = RegistrantService.GetClassesForCharge(checkoutObject: checkoutObject, plan: subscriptionPlan, msgs, dc: dc);

            var caclObj = RegistrantService.GetAmountsRequiredToCharge(checkoutObject: checkoutObject, plan: subscriptionPlan, isSubscriptionRequired: isSubscriptionRequired,
                classesRemainingInCurrentCycle: classesToCharge.Item2, classesInCurrentCycle: classesToCharge.Item1, msgs: msgs, dc: dc);

            // purchase
            var purchase = registration.GetPurchase(msgs, dc);
            Assert.NotNull(purchase);
            Assert.Equal(purchase.AmountPaid, caclObj.Item1);
            Assert.Equal(purchase.StripeSourceId, sourceId);
            Assert.Equal(purchase.RegistrationId, registration.ID.Value);
            Assert.Equal(purchase.RegistrantId, masterRegistrant.ID.Value);
            Assert.Equal(purchase.PurchaserAccountId, payingAccount.ID.Value);

            // activityPayment
            var activityPayment = ActivityPayment.LoadByParticipationObject(ParticipationObjectId: activity.ID.Value,
                        ParticipationObjectTypeId: ParticipationObjectType.Activity,
                        IsDeleted: false,
                        msgs: msgs,
                        dc: dc).FirstOrDefault();
            Assert.NotNull(activityPayment);
            Assert.Equal(activityPayment.Amount, caclObj.Item2);
            Assert.Equal(activityPayment.ParticipationObjectId, activity.ID.Value);
            Assert.Equal(activityPayment.RegistrantId, registrant.ID.Value);
            Assert.True(activityPayment.IsProRated);
            Assert.Equal(activityPayment.AmountPaidByParent, caclObj.Item1);

            // transaction
            var transaction = activityPayment.GetTransaction(msgs, dc);
            Assert.NotNull(transaction);
            Assert.Equal(transaction.Amount, caclObj.Item1);
            Assert.Equal(transaction.PurchaseId, purchase.ID.Value);                      
            Assert.Equal(transaction.PaymentTypeFeeCharged, caclObj.Item4);
            Assert.Equal(transaction.ConvenienceFeeCharged, caclObj.Item3);
            Assert.Equal(transaction.PaymentTypeFeePayerAccountId, payingAccount.ID.Value);
            Assert.Equal(transaction.ConvenienceFeePayerAccountId, payingAccount.ID.Value);
            Assert.Equal(transaction.BillerAccountId, billingAccount.ID.Value);
            Assert.Equal(transaction.PayerAccountId, payingAccount.ID.Value);

            //Stripe
            var product = StripeHelper.GetProduct(activity.Uid.ToString(), stripeAccountId: null, msgs);
            Assert.NotNull(product);

            var charge = StripeHelper.GetCharge(stripeChargeId: transaction.StripeChargeOrRefundId, msgs);
            Assert.NotNull(charge);

            var customer = payingAccount.GetStripeCustomer(msgs);
            Assert.NotNull(customer);

            // confirm that stripe charge matches transaction
            Assert.Equal(charge.Amount, caclObj.Item1 * 100);
            Assert.Equal(charge.CustomerId, customer.Id);

            Assert.True(msgs.HasNoErrors());
        }

        /// <summary>
        /// Activity schedule period is big and is on going, so Subscription plan will be initiated with a pro-rated charge.
        /// </summary>
        /// <param name="costPerPeriod"></param>
        [Theory(DisplayName = "Case 2: SubscriptionPlanType.Subscription, PlanPaymentStartType.OnCheckout, BillingPeriodType.Month, Both charge + subscription")]
        [InlineData(100)]
        public void SubscriptionPlanDueOnRegistrationCheckoutTest2(int costPerPeriod)
        {
            // Arrange
            var billingAccountOwner = WebUserFactory.Create("Don", "John", "DonJohn@test.com", true, msgs, dc);
            var payingAccountOwner = WebUserFactory.Create("Cristine", "Trump", "CristineTrump@test.com", true, msgs, dc);
            var billingAccount = AccountFactory.CreateAccount(billingAccountOwner, AccountType.School, stripeChargesEnabled: true, stripePayoutsEnabled: true, msgs, dc);
            var payingAccount = AccountFactory.CreateWithFreePackage("Purchasing account", "PurchasingAccount@test.com", payingAccountOwner, AccountType.School, msgs, dc);
            var masterRegistrant = RegistrantFactory.Create(payingAccount, isMaster: true, msgs, dc);
            var registrant = RegistrantFactory.Create(billingAccount, isMaster: false, msgs, dc: dc);
            var registration = RegistrationFactory.Create(billingAccount, registrant, msgs, dc);

            // create a schedule of 3 months
            var schedule = ActivityScheduleFactory.Create(billingAccount, DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddDays(90), msgs, dc);
            var activity = ActivityFactory.Create(billingAccount, schedule, msgs, dc: dc);
            var tsOfSchedule = DateTime.UtcNow.AddDays(90) - DateTime.UtcNow.AddDays(-1);
            var startDates = new List<DateRange>();

            for (int i = 0; i < tsOfSchedule.TotalDays; i++)
            {
                startDates.Add(new DateRange()
                {
                    StartDate = $"{DateTime.UtcNow.AddDays(i).Month}/{DateTime.UtcNow.AddDays(i).Day}/{DateTime.UtcNow.AddDays(i).Year} 3:00:00 PM".ToDateTime(),
                    EndDate = $"{DateTime.UtcNow.AddDays(i).Month}/{DateTime.UtcNow.AddDays(i).Day}/{DateTime.UtcNow.AddDays(i).Year} 5:00:00 PM".ToDateTime()
                });
            }

            var endDates = new List<DateTime>() { };
            ActivityScheduleFactory.Create(billingAccount, activity, schedule, startDates, endDates, billingAccountOwner, msgs, dc);

            var participantObject = activity.GetParticipationObject(msgs: msgs, dc: dc);
            var subscriptionPlan = SubscriptionPlanFactory.Create(planOwnerAccount: billingAccount,
                activity: activity,
                costPerPeriod: costPerPeriod,
                planType: SubscriptionPlanType.Subscription,
                billingPeriodType: BillingPeriodType.Month,
                planPaymentStartType: PlanPaymentStartType.OnCheckout,
                msgs: msgs,
                dc: dc);

            /// Update stripe account and customer of payer/parent
            var stripePurchasingAccount = AccountFactory.CreateStripeAccountAndCustomer(account: payingAccount, msgs: msgs, dc: dc);
            var sourceId = payingAccount.CreateStripeSourceFromToken(StripeHelper.TestSourceToken, stripePurchasingAccount.Item2, msgs, dc);
            var creditCardPurchaseObject = new CreditCardPurchaseObject() { StripeToken = StripeHelper.TestSourceToken, StripeSourceId = sourceId };

            var checkoutObject = new CheckoutObject()
            {
                PayingAccount = payingAccount,
                BillingAccount = billingAccount,
                ParticipationObject = participantObject,
                PaymentSourceType = PaymentSourceType.AfterSchoolPlatform,
                PaymentType = PaymentType.CreditCard,
                CreditCardPurchaseObject = creditCardPurchaseObject,
                SubscriptionPlan = subscriptionPlan,
                MasterRegistrant = masterRegistrant,
                NumberOfSeats = 1,
                Registration = registration,
                ConvenienceFeePayer = payingAccount,
                CreditCardFeePayer = payingAccount,
                SavePaymentInfo = true
            };

            //Act
            masterRegistrant.StartSubscriptionPlan(
                checkoutObject: checkoutObject,
                loggedInUser: LoggedInUser,
                msgs: msgs,
                dc: dc);

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

            var startDateTime = masterRegistrant.GetPlanStartDate(subscriptionPlan, participantObject, msgs, dc);
            var endDateTime = subscriptionPlan.GetPlanEndDateForRegistrantSubscription(startDateTime, participantObject, msgs);
            var isSubscriptionRequired = true;

            if (startDateTime.IsNotEmpty() && endDateTime.IsNotEmpty())
                isSubscriptionRequired = subscriptionPlan.CheckIfSubscriptionRequired(startDateTime, endDateTime.Value);

            Assert.True(isSubscriptionRequired);

            //subscription plan
            Assert.NotNull(subscriptionPlan);
            Assert.Equal(subscriptionPlan?.ParticipationObjectId, participantObject.ObjectID.Value);
            Assert.Equal(subscriptionPlan?.ParticipationObjectTypeId, participantObject.ObjectType);
            Assert.Equal(subscriptionPlan?.CostPerPeriod, costPerPeriod);
            Assert.Equal(subscriptionPlan?.PlanOwnerAccountId, billingAccount.ID.Value);
            Assert.Equal(BillingPeriodType.Month, subscriptionPlan?.BillingPeriodTypeId);
            Assert.Equal(SubscriptionPlanType.Subscription, subscriptionPlan?.PlanTypeId);

            var classesToCharge = RegistrantService.GetClassesForCharge(checkoutObject: checkoutObject, plan: subscriptionPlan, msgs, dc: dc);

            var caclObj = RegistrantService.GetAmountsRequiredToCharge(checkoutObject: checkoutObject, plan: subscriptionPlan, isSubscriptionRequired: isSubscriptionRequired,
                classesRemainingInCurrentCycle: classesToCharge.Item2, classesInCurrentCycle: classesToCharge.Item1, msgs: msgs, dc: dc);

            // purchase
            var purchase = registration.GetPurchase(msgs, dc);
            Assert.NotNull(purchase);
            Assert.Equal(purchase.AmountPaid, caclObj.Item1);
            Assert.Equal(purchase.StripeSourceId, sourceId);
            Assert.Equal(purchase.RegistrationId, registration.ID.Value);
            Assert.Equal(purchase.RegistrantId, masterRegistrant.ID.Value);
            Assert.Equal(purchase.PurchaserAccountId, payingAccount.ID.Value);

            // activityPayment
            var activityPayment = ActivityPayment.LoadByParticipationObject(ParticipationObjectId: activity.ID.Value,
                        ParticipationObjectTypeId: ParticipationObjectType.Activity,
                        IsDeleted: false,
                        msgs: msgs,
                        dc: dc).FirstOrDefault();
            Assert.NotNull(activityPayment);
            Assert.Equal(activityPayment.Amount, caclObj.Item2);
            Assert.Equal(activityPayment.ParticipationObjectId, activity.ID.Value);
            Assert.Equal(activityPayment.RegistrantId, registrant.ID.Value);
            Assert.True(activityPayment.IsProRated);
            Assert.Equal(activityPayment.AmountPaidByParent, caclObj.Item1);

            // transaction
            var transaction = activityPayment.GetTransaction(msgs, dc);
            Assert.NotNull(transaction);
            Assert.Equal(transaction.Amount, caclObj.Item1);
            Assert.Equal(transaction.PurchaseId, purchase.ID.Value);
            Assert.Equal(transaction.PaymentTypeFeeCharged, caclObj.Item4);
            Assert.Equal(transaction.ConvenienceFeeCharged, caclObj.Item3);
            Assert.Equal(transaction.PaymentTypeFeePayerAccountId, payingAccount.ID.Value);
            Assert.Equal(transaction.ConvenienceFeePayerAccountId, payingAccount.ID.Value);
            Assert.Equal(transaction.BillerAccountId, billingAccount.ID.Value);
            Assert.Equal(transaction.PayerAccountId, payingAccount.ID.Value);

            //Stripe
            var product = StripeHelper.GetProduct(activity.Uid.ToString(), stripeAccountId: null, msgs);
            Assert.NotNull(product);

            var charge = StripeHelper.GetCharge(stripeChargeId: transaction.StripeChargeOrRefundId, msgs);
            Assert.NotNull(charge);

            var customer = payingAccount.GetStripeCustomer(msgs);
            Assert.NotNull(customer);

            // confirm that stripe charge matches transaction
            Assert.Equal(charge.Amount, caclObj.Item1 * 100);
            Assert.Equal(charge.CustomerId, customer.Id);

            //TODO: test newly created subscription for registrant with trial days, cancellation date, and upcoming invoice
            Assert.True(msgs.HasNoErrors());
        }

        /// <summary>
        /// Activity schedule period is big and is off future, so there will be no charge, just subscription plan.
        /// </summary>
        /// <param name="costPerPeriod"></param>
        [Theory(DisplayName = "Case 3: SubscriptionPlanType.Subscription, PlanPaymentStartType.OnCheckout, BillingPeriodType.Month, Only subscription")]
        [InlineData(100)]
        public void SubscriptionPlanDueOnRegistrationCheckoutTest3(int costPerPeriod)
        {
            // Arrange
            var billingAccountOwner = WebUserFactory.Create("Don", "John", "DonJohn@test.com", true, msgs, dc);
            var payingAccountOwner = WebUserFactory.Create("Cristine", "Trump", "CristineTrump@test.com", true, msgs, dc);
            var billingAccount = AccountFactory.CreateAccount(billingAccountOwner, AccountType.School, stripeChargesEnabled: true, stripePayoutsEnabled: true, msgs, dc);
            var payingAccount = AccountFactory.CreateWithFreePackage("Purchasing account", "PurchasingAccount@test.com", payingAccountOwner, AccountType.School, msgs, dc);
            var masterRegistrant = RegistrantFactory.Create(payingAccount, isMaster: true, msgs, dc);
            var registrant = RegistrantFactory.Create(billingAccount, isMaster: false, msgs, dc: dc);
            var registration = RegistrationFactory.Create(billingAccount, registrant, msgs, dc);

            // create a schedule of 3 months and that's also of future
            var nextMonth = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1).AddMonths(1);
            var schedule = ActivityScheduleFactory.Create(billingAccount, nextMonth, nextMonth.AddDays(90), msgs, dc);
            var activity = ActivityFactory.Create(billingAccount, schedule, msgs, dc: dc);
            var tsOfSchedule = nextMonth.AddDays(90) - nextMonth;
            var startDates = new List<DateRange>();

            for (int i = 0; i < tsOfSchedule.TotalDays; i++)
            {
                startDates.Add(new DateRange()
                {
                    StartDate = $"{nextMonth.AddDays(i).Month}/{nextMonth.AddDays(i).Day}/{nextMonth.AddDays(i).Year} 3:00:00 PM".ToDateTime(),
                    EndDate = $"{nextMonth.AddDays(i).Month}/{nextMonth.AddDays(i).Day}/{nextMonth.AddDays(i).Year} 5:00:00 PM".ToDateTime()
                });
            }

            var endDates = new List<DateTime>() { };
            ActivityScheduleFactory.Create(billingAccount, activity, schedule, startDates, endDates, billingAccountOwner, msgs, dc);

            var participantObject = activity.GetParticipationObject(msgs: msgs, dc: dc);
            var subscriptionPlan = SubscriptionPlanFactory.Create(planOwnerAccount: billingAccount,
                activity: activity,
                costPerPeriod: costPerPeriod,
                planType: SubscriptionPlanType.Subscription,
                billingPeriodType: BillingPeriodType.Month,
                planPaymentStartType: PlanPaymentStartType.OnCheckout,
                msgs: msgs,
                dc: dc);

            /// Update stripe account and customer of payer/parent
            var stripePurchasingAccount = AccountFactory.CreateStripeAccountAndCustomer(account: payingAccount, msgs: msgs, dc: dc);
            var sourceId = payingAccount.CreateStripeSourceFromToken(StripeHelper.TestSourceToken, stripePurchasingAccount.Item2, msgs, dc);
            var creditCardPurchaseObject = new CreditCardPurchaseObject() { StripeToken = StripeHelper.TestSourceToken, StripeSourceId = sourceId };

            var checkoutObject = new CheckoutObject()
            {
                PayingAccount = payingAccount,
                BillingAccount = billingAccount,
                ParticipationObject = participantObject,
                PaymentSourceType = PaymentSourceType.AfterSchoolPlatform,
                PaymentType = PaymentType.CreditCard,
                CreditCardPurchaseObject = creditCardPurchaseObject,
                SubscriptionPlan = subscriptionPlan,
                MasterRegistrant = masterRegistrant,
                NumberOfSeats = 1,
                Registration = registration,
                ConvenienceFeePayer = payingAccount,
                CreditCardFeePayer = payingAccount,
                SavePaymentInfo = true
            };

            //Act
            masterRegistrant.StartSubscriptionPlan(
                checkoutObject: checkoutObject,
                loggedInUser: LoggedInUser,
                msgs: msgs,
                dc: dc);

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

            var startDateTime = masterRegistrant.GetPlanStartDate(subscriptionPlan, participantObject, msgs, dc);
            var endDateTime = subscriptionPlan.GetPlanEndDateForRegistrantSubscription(startDateTime, participantObject, msgs);
            var isSubscriptionRequired = true;

            if (startDateTime.IsNotEmpty() && endDateTime.IsNotEmpty())
                isSubscriptionRequired = subscriptionPlan.CheckIfSubscriptionRequired(startDateTime, endDateTime.Value);

            Assert.True(isSubscriptionRequired);

            //subscription plan
            Assert.NotNull(subscriptionPlan);
            Assert.Equal(subscriptionPlan?.ParticipationObjectId, participantObject.ObjectID.Value);
            Assert.Equal(subscriptionPlan?.ParticipationObjectTypeId, participantObject.ObjectType);
            Assert.Equal(subscriptionPlan?.CostPerPeriod, costPerPeriod);
            Assert.Equal(subscriptionPlan?.PlanOwnerAccountId, billingAccount.ID.Value);
            Assert.Equal(BillingPeriodType.Month, subscriptionPlan?.BillingPeriodTypeId);
            Assert.Equal(SubscriptionPlanType.Subscription, subscriptionPlan?.PlanTypeId);

            var classesToCharge = RegistrantService.GetClassesForCharge(checkoutObject: checkoutObject, plan: subscriptionPlan, msgs, dc: dc);
            Assert.Equal(0, classesToCharge.Item2);
            Assert.Equal(0, classesToCharge.Item1);

            // purchase
            var purchase = registration.GetPurchase(msgs, dc);
            Assert.NotNull(purchase);
            Assert.Equal(purchase.AmountPaid, costPerPeriod);
            Assert.Equal(purchase.StripeSourceId, sourceId);
            Assert.Equal(purchase.RegistrationId, registration.ID.Value);
            Assert.Equal(purchase.RegistrantId, masterRegistrant.ID.Value);
            Assert.Equal(purchase.PurchaserAccountId, payingAccount.ID.Value);            

            //Stripe
            var product = StripeHelper.GetProduct(activity.Uid.ToString(), stripeAccountId: null, msgs);
            Assert.NotNull(product);

            //TODO: test newly created subscription for registrant with trial days, cancellation date and upcoming invoice
            Assert.True(msgs.HasNoErrors());
        }

        /// <summary>
        /// Activity schedule period is already started, so there will be pro-rated charge + subscription with future invoice (probably from next month)
        /// </summary>
        /// <param name="costPerPeriod"></param>
        [Theory(DisplayName = "Case 1: SubscriptionPlanType.Subscription, PlanPaymentStartType.WhenActivityStarts, BillingPeriodType.Month")]
        [InlineData(100)]
        public void SubscriptionPlanDueOnRegistrationWhenActivityStartsTest1(int costPerPeriod)
        {
            // Arrange
            var billingAccountOwner = WebUserFactory.Create("Don", "John", "DonJohn@test.com", true, msgs, dc);
            var payingAccountOwner = WebUserFactory.Create("Cristine", "Trump", "CristineTrump@test.com", true, msgs, dc);
            var billingAccount = AccountFactory.CreateAccount(billingAccountOwner, AccountType.School, stripeChargesEnabled: true, stripePayoutsEnabled: true, msgs, dc);
            var payingAccount = AccountFactory.CreateWithFreePackage("Purchasing account", "PurchasingAccount@test.com", payingAccountOwner, AccountType.School, msgs, dc);
            var masterRegistrant = RegistrantFactory.Create(payingAccount, isMaster: true, msgs, dc);
            var registrant = RegistrantFactory.Create(billingAccount, isMaster: false, msgs, dc: dc);
            var registration = RegistrationFactory.Create(billingAccount, registrant, msgs, dc);

            // create a schedule of 3 months and that's also of future
            var schedule = ActivityScheduleFactory.Create(billingAccount, DateTime.UtcNow.AddDays(-10), DateTime.UtcNow.AddDays(70), msgs, dc);
            var activity = ActivityFactory.Create(billingAccount, schedule, msgs, dc: dc);
            var tsOfSchedule = DateTime.UtcNow.AddDays(60) - DateTime.UtcNow.AddDays(-10);
            var startDates = new List<DateRange>();

            for (int i = -10; i < tsOfSchedule.TotalDays - 10; i++)
            {
                startDates.Add(new DateRange()
                {
                    StartDate = $"{DateTime.UtcNow.AddDays(i).Month}/{DateTime.UtcNow.AddDays(i).Day}/{DateTime.UtcNow.AddDays(i).Year} 3:00:00 PM".ToDateTime(),
                    EndDate = $"{DateTime.UtcNow.AddDays(i).Month}/{DateTime.UtcNow.AddDays(i).Day}/{DateTime.UtcNow.AddDays(i).Year} 5:00:00 PM".ToDateTime()
                });
            }

            var endDates = new List<DateTime>() { };
            ActivityScheduleFactory.Create(billingAccount, activity, schedule, startDates, endDates, billingAccountOwner, msgs, dc);

            var participantObject = activity.GetParticipationObject(msgs: msgs, dc: dc);
            var subscriptionPlan = SubscriptionPlanFactory.Create(planOwnerAccount: billingAccount,
                activity: activity,
                costPerPeriod: costPerPeriod,
                planType: SubscriptionPlanType.Subscription,
                billingPeriodType: BillingPeriodType.Month,
                planPaymentStartType: PlanPaymentStartType.WhenActivityStarts,
                msgs: msgs,
                dc: dc);

            /// Update stripe account and customer of payer/parent
            var stripePurchasingAccount = AccountFactory.CreateStripeAccountAndCustomer(account: payingAccount, msgs: msgs, dc: dc);
            var sourceId = payingAccount.CreateStripeSourceFromToken(StripeHelper.TestSourceToken, stripePurchasingAccount.Item2, msgs, dc);
            var creditCardPurchaseObject = new CreditCardPurchaseObject() { StripeToken = StripeHelper.TestSourceToken, StripeSourceId = sourceId };

            var checkoutObject = new CheckoutObject()
            {
                PayingAccount = payingAccount,
                BillingAccount = billingAccount,
                ParticipationObject = participantObject,
                PaymentSourceType = PaymentSourceType.AfterSchoolPlatform,
                PaymentType = PaymentType.CreditCard,
                CreditCardPurchaseObject = creditCardPurchaseObject,
                SubscriptionPlan = subscriptionPlan,
                MasterRegistrant = masterRegistrant,
                NumberOfSeats = 1,
                Registration = registration,
                ConvenienceFeePayer = payingAccount,
                CreditCardFeePayer = payingAccount,
                SavePaymentInfo = true
            };

            //Act
            masterRegistrant.StartSubscriptionPlan(
                checkoutObject: checkoutObject,
                loggedInUser: LoggedInUser,
                msgs: msgs,
                dc: dc);

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

            var startDateTime = masterRegistrant.GetPlanStartDate(subscriptionPlan, participantObject, msgs, dc);
            var endDateTime = subscriptionPlan.GetPlanEndDateForRegistrantSubscription(startDateTime, participantObject, msgs);
            var isSubscriptionRequired = true;

            if (startDateTime.IsNotEmpty() && endDateTime.IsNotEmpty())
                isSubscriptionRequired = subscriptionPlan.CheckIfSubscriptionRequired(startDateTime, endDateTime.Value);

            Assert.True(isSubscriptionRequired);

            //subscription plan
            Assert.NotNull(subscriptionPlan);
            Assert.Equal(subscriptionPlan?.ParticipationObjectId, participantObject.ObjectID.Value);
            Assert.Equal(subscriptionPlan?.ParticipationObjectTypeId, participantObject.ObjectType);
            Assert.Equal(subscriptionPlan?.CostPerPeriod, costPerPeriod);
            Assert.Equal(subscriptionPlan?.PlanOwnerAccountId, billingAccount.ID.Value);
            Assert.Equal(BillingPeriodType.Month, subscriptionPlan?.BillingPeriodTypeId);
            Assert.Equal(SubscriptionPlanType.Subscription, subscriptionPlan?.PlanTypeId);

            var classesToCharge = RegistrantService.GetClassesForCharge(checkoutObject: checkoutObject, plan: subscriptionPlan, msgs, dc: dc);            

            var caclObj = RegistrantService.GetAmountsRequiredToCharge(checkoutObject: checkoutObject, plan: subscriptionPlan, isSubscriptionRequired: isSubscriptionRequired,
                classesRemainingInCurrentCycle: classesToCharge.Item2, classesInCurrentCycle: classesToCharge.Item1, msgs: msgs, dc: dc);

            // purchase
            var purchase = registration.GetPurchase(msgs, dc);
            Assert.NotNull(purchase);
            Assert.Equal(purchase.AmountPaid, caclObj.Item1);
            Assert.Equal(purchase.StripeSourceId, sourceId);
            Assert.Equal(purchase.RegistrationId, registration.ID.Value);
            Assert.Equal(purchase.RegistrantId, masterRegistrant.ID.Value);
            Assert.Equal(purchase.PurchaserAccountId, payingAccount.ID.Value);

            // activityPayment
            var activityPayment = ActivityPayment.LoadByParticipationObject(ParticipationObjectId: activity.ID.Value,
                        ParticipationObjectTypeId: ParticipationObjectType.Activity,
                        IsDeleted: false,
                        msgs: msgs,
                        dc: dc).FirstOrDefault();
            Assert.NotNull(activityPayment);
            Assert.Equal(activityPayment.Amount, caclObj.Item2);
            Assert.Equal(activityPayment.ParticipationObjectId, activity.ID.Value);
            Assert.Equal(activityPayment.RegistrantId, registrant.ID.Value);
            Assert.True(activityPayment.IsProRated);
            Assert.Equal(activityPayment.AmountPaidByParent, caclObj.Item1);

            // transaction
            var transaction = activityPayment.GetTransaction(msgs, dc);
            Assert.NotNull(transaction);
            Assert.Equal(transaction.Amount, caclObj.Item1);
            Assert.Equal(transaction.PurchaseId, purchase.ID.Value);
            Assert.Equal(transaction.PaymentTypeFeeCharged, caclObj.Item4);
            Assert.Equal(transaction.ConvenienceFeeCharged, caclObj.Item3);
            Assert.Equal(transaction.PaymentTypeFeePayerAccountId, payingAccount.ID.Value);
            Assert.Equal(transaction.ConvenienceFeePayerAccountId, payingAccount.ID.Value);
            Assert.Equal(transaction.BillerAccountId, billingAccount.ID.Value);
            Assert.Equal(transaction.PayerAccountId, payingAccount.ID.Value);

            //Stripe
            var product = StripeHelper.GetProduct(activity.Uid.ToString(), stripeAccountId: null, msgs);
            Assert.NotNull(product);

            var charge = StripeHelper.GetCharge(stripeChargeId: transaction.StripeChargeOrRefundId, msgs);
            Assert.NotNull(charge);

            var customer = payingAccount.GetStripeCustomer(msgs);
            Assert.NotNull(customer);

            // confirm that stripe charge matches transaction
            Assert.Equal(charge.Amount, caclObj.Item1 * 100);
            Assert.Equal(charge.CustomerId, customer.Id);

            //TODO: charge already checked but need to review upcoming invoice
            Assert.True(msgs.HasNoErrors());
        }

        /// <summary>
        /// Activity schedule period is off future, so there will be no charge, just subscription plan when registrant joins.
        /// </summary>
        /// <param name="costPerPeriod"></param>
        [Theory(DisplayName = "Case 2: SubscriptionPlanType.Subscription, PlanPaymentStartType.WhenActivityStarts, BillingPeriodType.Month, Only subscription")]
        [InlineData(100)]
        public void SubscriptionPlanDueOnRegistrationWhenActivityStartsTest2(int costPerPeriod)
        {
            // Arrange
            var billingAccountOwner = WebUserFactory.Create("Don", "John", "DonJohn@test.com", true, msgs, dc);
            var payingAccountOwner = WebUserFactory.Create("Cristine", "Trump", "CristineTrump@test.com", true, msgs, dc);
            var billingAccount = AccountFactory.CreateAccount(billingAccountOwner, AccountType.School, stripeChargesEnabled: true, stripePayoutsEnabled: true, msgs, dc);
            var payingAccount = AccountFactory.CreateWithFreePackage("Purchasing account", "PurchasingAccount@test.com", payingAccountOwner, AccountType.School, msgs, dc);
            var masterRegistrant = RegistrantFactory.Create(payingAccount, isMaster: true, msgs, dc);
            var registrant = RegistrantFactory.Create(billingAccount, isMaster: false, msgs, dc: dc);
            var registration = RegistrationFactory.Create(billingAccount, registrant, msgs, dc);

            // create a schedule of 3 months and that's also of future
            var nextMonth = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1).AddMonths(1);
            var schedule = ActivityScheduleFactory.Create(billingAccount, nextMonth, nextMonth.AddDays(90), msgs, dc);
            var activity = ActivityFactory.Create(billingAccount, schedule, msgs, dc: dc);
            var tsOfSchedule = nextMonth.AddDays(90) - nextMonth;
            var startDates = new List<DateRange>();

            for (int i = 0; i < tsOfSchedule.TotalDays; i++)
            {
                startDates.Add(new DateRange()
                {
                    StartDate = $"{nextMonth.AddDays(i).Month}/{nextMonth.AddDays(i).Day}/{nextMonth.AddDays(i).Year} 3:00:00 PM".ToDateTime(),
                    EndDate = $"{nextMonth.AddDays(i).Month}/{nextMonth.AddDays(i).Day}/{nextMonth.AddDays(i).Year} 5:00:00 PM".ToDateTime()
                });
            }

            var endDates = new List<DateTime>() { };
            ActivityScheduleFactory.Create(billingAccount, activity, schedule, startDates, endDates, billingAccountOwner, msgs, dc);

            var participantObject = activity.GetParticipationObject(msgs: msgs, dc: dc);
            var subscriptionPlan = SubscriptionPlanFactory.Create(planOwnerAccount: billingAccount,
                activity: activity,
                costPerPeriod: costPerPeriod,
                planType: SubscriptionPlanType.Subscription,
                billingPeriodType: BillingPeriodType.Month,
                planPaymentStartType: PlanPaymentStartType.WhenActivityStarts,
                msgs: msgs,
                dc: dc);

            /// Update stripe account and customer of payer/parent
            var stripePurchasingAccount = AccountFactory.CreateStripeAccountAndCustomer(account: payingAccount, msgs: msgs, dc: dc);
            var sourceId = payingAccount.CreateStripeSourceFromToken(StripeHelper.TestSourceToken, stripePurchasingAccount.Item2, msgs, dc);
            var creditCardPurchaseObject = new CreditCardPurchaseObject() { StripeToken = StripeHelper.TestSourceToken, StripeSourceId = sourceId };

            var checkoutObject = new CheckoutObject()
            {
                PayingAccount = payingAccount,
                BillingAccount = billingAccount,
                ParticipationObject = participantObject,
                PaymentSourceType = PaymentSourceType.AfterSchoolPlatform,
                PaymentType = PaymentType.CreditCard,
                CreditCardPurchaseObject = creditCardPurchaseObject,
                SubscriptionPlan = subscriptionPlan,
                MasterRegistrant = masterRegistrant,
                NumberOfSeats = 1,
                Registration = registration,
                ConvenienceFeePayer = payingAccount,
                CreditCardFeePayer = payingAccount,
                SavePaymentInfo = true
            };

            //Act
            masterRegistrant.StartSubscriptionPlan(
                checkoutObject: checkoutObject,
                loggedInUser: LoggedInUser,
                msgs: msgs,
                dc: dc);

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

            var startDateTime = masterRegistrant.GetPlanStartDate(subscriptionPlan, participantObject, msgs, dc);
            var endDateTime = subscriptionPlan.GetPlanEndDateForRegistrantSubscription(startDateTime, participantObject, msgs);
            var isSubscriptionRequired = true;

            if (startDateTime.IsNotEmpty() && endDateTime.IsNotEmpty())
                isSubscriptionRequired = subscriptionPlan.CheckIfSubscriptionRequired(startDateTime, endDateTime.Value);

            Assert.True(isSubscriptionRequired);

            //subscription plan
            Assert.NotNull(subscriptionPlan);
            Assert.Equal(subscriptionPlan?.ParticipationObjectId, participantObject.ObjectID.Value);
            Assert.Equal(subscriptionPlan?.ParticipationObjectTypeId, participantObject.ObjectType);
            Assert.Equal(subscriptionPlan?.CostPerPeriod, costPerPeriod);
            Assert.Equal(subscriptionPlan?.PlanOwnerAccountId, billingAccount.ID.Value);
            Assert.Equal(BillingPeriodType.Month, subscriptionPlan?.BillingPeriodTypeId);
            Assert.Equal(SubscriptionPlanType.Subscription, subscriptionPlan?.PlanTypeId);

            var classesToCharge = RegistrantService.GetClassesForCharge(checkoutObject: checkoutObject, plan: subscriptionPlan, msgs, dc: dc);
            Assert.Equal(0, classesToCharge.Item2);
            Assert.Equal(0, classesToCharge.Item1);

            //Stripe
            var product = StripeHelper.GetProduct(activity.Uid.ToString(), stripeAccountId: null, msgs);
            Assert.NotNull(product);

            var customer = payingAccount.GetStripeCustomer(msgs);
            Assert.NotNull(customer);

            //TODO: test newly created subscription for registrant with trial days, cancellation date and upcoming invoice
            var upcomingInvoice = StripeHelper.GetUpcomingInvoice(customerId: customer.Id, msgs: msgs);
            Assert.NotNull(upcomingInvoice);
            
            Assert.True(msgs.HasNoErrors());
        }
    }
}
