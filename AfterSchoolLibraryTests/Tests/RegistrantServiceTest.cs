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

        [Fact]
        public void PrepareRegistrantDetailsTest()
        {
            // Arrange
            var webUser = WebUserFactory.Create(true, msgs, dc);
            var account = AccountFactory.Create(webUser, msgs, dc);
            var masterRegistrant = RegistrantFactory.Create(account, isMaster: true, msgs, dc);
            var parentRegistrant = RegistrantFactory.CreateParent(account, msgs, dc: dc);
            var registrant = RegistrantFactory.CreateStudent(account, parentRegistrant.ID.Value, msgs, dc: dc);            

            var tsOfSchedule = DateTime.UtcNow.AddDays(10) - DateTime.UtcNow.AddDays(-1);
            var schedule = ActivityScheduleFactory.Create(account, DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddDays(10), msgs, dc);
            var activity = ActivityFactory.Create(account, schedule, msgs, dc);
            
            RegistrationFactory.Create(account, registrant, msgs, dc);
            RegistrantActivityFactory.Create(activity, registrant, RegistrationAssignmentType.Reserved, msgs, dc: dc);
            RegistrantWebUserFactory.Create(parentRegistrant, webUser, msgs, dc);

            var instances = new List<ActivityInstance>();

            for (var i = -1; i < tsOfSchedule.TotalDays - 1; i++)
            {
                var start = $"{DateTime.UtcNow.AddDays(i).Month}/{DateTime.UtcNow.AddDays(i).Day}/{DateTime.UtcNow.AddDays(i).Year} 3:00:00 PM".ToDateTime();
                var end = $"{DateTime.UtcNow.AddDays(i).Month}/{DateTime.UtcNow.AddDays(i).Day}/{DateTime.UtcNow.AddDays(i).Year} 5:00:00 PM".ToDateTime();

                instances.Add(ActivityInstanceFactory.Create(activity, schedule, InstanceType.InSession, start, end, msgs, dc));
            }

            // provider choose to not meet on 3rd day from now for this activity, so it will be a new OutOfSession instance where class will not meet
            instances.Add(ActivityInstanceFactory.Create(activity, schedule, InstanceType.OutOfSession, DateTime.UtcNow.AddDays(3), DateTime.UtcNow.AddDays(3), msgs, dc));

            // Assert
            Assert.True(msgs.HasNoErrors());
            Assert.True(instances.Count == 12); // 11 InSession, 1 OutOfSession

            // Act
            var inSessionInstances = schedule?.GetInSessionInstances(msgs, dc);
            var outOfSessionInstances = schedule?.GetOutSessionInstances(msgs, dc);

            // Assert
            Assert.True(msgs.HasNoErrors());
            Assert.True(instances.Count == 12);
            Assert.True(inSessionInstances.Count == 11);
            Assert.True(outOfSessionInstances.Count == 1);

            // Final Act
            var registrants = webUser.GetAllRegistrants(msgs, dc);
            var parentModel = registrants.PrepareRegistrantDetails(parentRegistrant, msgs, dc);

            // Assert
            Assert.True(msgs.HasNoErrors());
            Assert.True(parentModel.ActivityInstances.Count == 9);
            Assert.True(parentModel.Providers.Count == 1);
            Assert.True(parentModel.RegistrantActivities.Count == 0);
        }
    }
}
