using Xunit;
using Xunit.Abstractions;
using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using AfterSchoolLibraryTests.Factories;
using System.Linq;
using System.Collections.Generic;
using System.Collections;
using System;

namespace AfterSchoolLibraryTests.Tests
{
    public class RegistrantServiceTest : BaseTest
    {
        public RegistrantServiceTest(ITestOutputHelper output) : base(output)
        {
        }

        [Theory(DisplayName = "StartSubscriptionPlanTest")]
        [InlineData(110)]
        [InlineData(12)]
        public void StartSubscriptionPlanTest(decimal amount)
        {
            try
            {
                // Arrange
                var billingAccountOwner = WebUserFactory.Create("Don", "John", "DonJohn@test.com", true, msgs, dc);
                var purchasingAccountOwner = WebUserFactory.Create("Cristine", "Trump", "CristineTrump@test.com", true, msgs, dc);
                var billingAccount = AccountFactory.CreateAccount(billingAccountOwner, AccountType.School, stripeChargesEnabled: true, stripePayoutsEnabled: true, msgs, dc);
                var purchasingAccount = AccountFactory.CreateWithFreePackage("Purchasing account", "PurchasingAccount@test.com", purchasingAccountOwner, AccountType.School, msgs, dc);
                var masterRegistrant = RegistrantFactory.Create(purchasingAccount, msgs, dc);

                //UseStripeConnectedAccount is true for this package
                var package = PackageFactory.CreateFreePackageWithUseStripeConnectedAccount(msgs, dc);
                var accountPackage = AccountPackageFactory.Create(billingAccount, package, msgs, dc);

                var activity = ActivityFactory.Create(billingAccount, msgs, dc: dc);
                var registrant = RegistrantFactory.Create(billingAccount, msgs, dc: dc);
                var registration = RegistrationFactory.Create(billingAccount, registrant, msgs, dc);

                var subscriptionPlan = SubscriptionPlanFactory.Create(billingAccount, activity, SubscriptionPlanType.Subscription, amount, msgs, dc);

                billingAccount.CreateStripeCustomer(msgs, dc);
                purchasingAccount.CreateStripeCustomer(msgs, dc);

                var billingCustomer = billingAccount.GetStripeCustomer(msgs);

                // TODO: we will set stripe user id so we can use it further with CreateRegistrantSubscription
                // billingCustomer.Id is not working as its customer id
                billingAccount.StripeUserId = billingCustomer.Id;

                var purchasingCustomer = purchasingAccount.GetStripeCustomer(msgs);
                var sourceId = purchasingAccount.CreateStripeSourceFromToken(AfterSchoolLibrary.Billing.StripeHelper.TestSourceToken, purchasingCustomer, msgs, dc);

                CreditCardPurchaseObject creditCardPurchaseObject = new CreditCardPurchaseObject()
                {
                    StripeToken = AfterSchoolLibrary.Billing.StripeHelper.TestSourceToken,
                    StripeSourceId = sourceId
                };

                var checkoutObject = new CheckoutObject()
                {
                    BillingAccount = billingAccount,
                    PayingAccount = purchasingAccount,
                    Coupon = null,
                    OverrideAmount = amount,
                    MasterRegistrant = masterRegistrant,
                    NumberOfSeats = 1,
                    ParticipationObject = activity.GetParticipationObject(msgs, dc),
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
                    purchasingAccountOwner,
                    msgs,
                    dc);

                //Assert
                //account
                Assert.NotNull(billingAccount);
                Assert.NotNull(purchasingAccount);

                //activity
                Assert.NotNull(activity);
                Assert.Equal(activity?.AccountId, billingAccount.ID.Value);

                //registrant
                Assert.NotNull(registrant);
                Assert.NotNull(masterRegistrant);
                Assert.Equal(registrant?.AccountId, billingAccount.ID.Value);
                Assert.Equal(masterRegistrant?.AccountId, purchasingAccount.ID.Value);

                //registration
                Assert.NotNull(registration);
                Assert.Equal(registration?.RegistrantId, registrant.ID.Value);
                Assert.Equal(registration?.AccountId, billingAccount.ID.Value);

                //subscription plan
                Assert.NotNull(subscriptionPlan);
                //Assert.Equal(subscriptionPlan?.ParticipationObjectId, participationObject.ObjectID.Value);
                //Assert.Equal(subscriptionPlan?.ParticipationObjectTypeId, participationObject.ObjectType);
                //Assert.Equal(subscriptionPlan?.CostPerPeriod, amountPaidByParent); //here subscription plan will have amount with all fees
                Assert.Equal(subscriptionPlan?.PlanOwnerAccountId, billingAccount.ID.Value);
                Assert.Equal(BillingPeriodType.Month, subscriptionPlan?.BillingPeriodTypeId);
                Assert.Equal(SubscriptionPlanType.Subscription, subscriptionPlan?.PlanTypeId);

                Assert.True(msgs.HasNoErrors());
            }
            catch (Exception ex)
            {
                //Assert
                Assert.False(false, ex.Message);
            }            
        }
    }
}
