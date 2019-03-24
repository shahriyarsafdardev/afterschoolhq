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
        [InlineData(1)]
        [InlineData(0)]
        public void StartSubscriptionPlanTest(int stripeConnectedAccountValue)
        {
            var billingAccountOwner = WebUserFactory.Create(msgs, dc);
            var purchasingAccountOwner = WebUserFactory.Create(msgs, dc);
            var billingAccount = AccountFactory.Create(billingAccountOwner, msgs, dc);
            var purchasingAccount = AccountFactory.Create(purchasingAccountOwner, msgs, dc);
            var masterRegistrant = RegistrantFactory.Create(purchasingAccount, msgs, dc);
            var activity = ActivityFactory.Create(billingAccount, msgs, dc);
            var registration = RegistrationFactory.Create(billingAccount, masterRegistrant, msgs, dc);
            var package = PackageFactory.Create(msgs, dc);
            var accountPackage = AccountPackageFactory.Create(billingAccount, package, msgs, dc);
            var accountPackageFeature1 = AccountPackageFeatureFactory.Create(accountPackage, FeatureType.PaymentsPercentageFee, .05M, msgs, dc);
            var accountPackageFeature2 = AccountPackageFeatureFactory.Create(accountPackage, FeatureType.PaymentsPercentageFlatFee, .99M, msgs, dc);
            var accountPackageFeature3 = AccountPackageFeatureFactory.Create(accountPackage, FeatureType.UseStripeConnectedAccount, stripeConnectedAccountValue, msgs, dc);

            billingAccount.CreateStripeCustomAccount(msgs, dc);

            var paymentPlan = SubscriptionPlanFactory.CreatePaymentPlan(billingAccount, activity, 100, 3, msgs, dc);

            purchasingAccount.CreateStripeCustomer(msgs, dc);

            var checkoutObject = new CheckoutObject()
            {
                BillingAccount = billingAccount,
                Coupon = null,
                MasterRegistrant = masterRegistrant,
                NumberOfSeats = 1,
                ParticipationObject = activity.GetParticipationObject(msgs, dc),
                PaymentPlan = paymentPlan,
                PaymentSourceType = PaymentSourceType.AfterSchoolPlatform,
                PaymentType = PaymentType.CreditCard,
                PurchasingAccount = purchasingAccount,
                SavePaymentInfo = true,
                Registration = registration,
                StripeToken = AfterSchoolLibrary.Billing.StripeHelper.GetTestSourceToken()
            };

            masterRegistrant.StartSubscriptionPlan(
                checkoutObject, 
                SubscriptionPlanType.PaymentPlan, 
                DateTime.UtcNow, 
                purchasingAccountOwner, 
                msgs, 
                dc);

            Assert.True(msgs.HasNoErrors());
        }
    }
}
