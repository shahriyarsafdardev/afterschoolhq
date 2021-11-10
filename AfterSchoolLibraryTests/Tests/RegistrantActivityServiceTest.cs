using System;
using Xunit;
using Xunit.Abstractions;
using AfterSchoolLibrary.Models;
using AfterSchoolLibraryTests.Factories;
using AfterSchoolLibrary.Services;

namespace AfterSchoolLibraryTests.Tests
{
    public class RegistrantActivityServiceTest : BaseTest
    {
        private WebUser user { get; set; }
        private Account account { get; set; }

        public RegistrantActivityServiceTest(ITestOutputHelper output) : base(output)
        {
            user = WebUserFactory.Create(msgs, dc);
            account = AccountFactory.Create(user, msgs, dc);
        }

        [Fact]
        public void GetRegistrantActivityTest()
        {
            // Arrange
            var activity = ActivityFactory.Create(account, msgs, dc);
            var registrant = RegistrantFactory.Create(msgs, dc);
            var registration = RegistrationFactory.Create(account, registrant, msgs, dc);
            var actualRegistrantActivity = RegistrantActivityFactory.Create(activity, registrant, registration, msgs, dc);

            // Act    
            var expectedRegistrantActivity = RegistrantActivityService.GetRegistrantActivity(actualRegistrantActivity.Uid, msgs, dc);

            // Assert
            Assert.Equal(expected: expectedRegistrantActivity.ID, actual: actualRegistrantActivity.ID);
        }

        [Theory(DisplayName = "test cases for GetAmountPaidTest, GetAmountPaidByParent, GetIsProRatedFlagByParent")]
        [InlineData(100, 6, 4, 110, false)] //no pro-rate
        [InlineData(100, 6, 4, 90, true)] //assume user joins mid-period and so as we have to keep pro-rated amount, its 94.3m(otherwise it would 100m)
        public void ActivityPayemntRelatedTest(decimal amount, decimal convenienceFee, decimal paymentTypeFee, decimal amountPaidByParent, bool isAmountProRated = false)
        {
            try
            {
                // Arrange
                var billingAccountOwner = WebUserFactory.Create("Don", "John", "DonJohn@test.com", true, msgs, dc);
                var billingAccount = AccountFactory.CreateWithFreePackage("Billing account", "BillingAccount@test.com", billingAccountOwner, AccountType.School, msgs, dc);
                var purchasingAccountOwner = WebUserFactory.Create("Cristine", "Trump", "CristineTrump@test.com", true, msgs, dc);
                var purchasingAccount = AccountFactory.CreateWithFreePackage("Purchasing account", "PurchasingAccount@test.com", purchasingAccountOwner, AccountType.School, msgs, dc);
                var activity = ActivityFactory.Create(billingAccount, msgs, dc: dc);

                var registrant = RegistrantFactory.Create(billingAccount, isMaster: false, msgs, dc: dc);
                var registration = RegistrationFactory.Create(billingAccount, registrant, msgs, dc);
                var registrantActivity = RegistrantActivityFactory.Create(activity, registrant, msgs, dc: dc);

                var subscriptionPlan = SubscriptionPlanFactory.Create(billingAccount, activity, SubscriptionPlanType.Subscription, amountPaidByParent, msgs, dc);
                var purchase = PurchaseFactory.Create(purchasingAccount, amount, registrantId: registrant.ID.Value, registrationId: registration.ID.Value, msgs, dc); // check: we are inserting row amount here
                var registrantSubscriptionPlan = RegistrantSubscriptionPlanFactory.Create(registrant, subscriptionPlan, purchase, amount: amount, convenienceFee: convenienceFee, paymentTypeFee: paymentTypeFee, msgs, dc);

                // Act    
                var activityPayment = ActivityPaymentFactory.Create(billingAccount, activity, registrant, registrantSubscriptionPlan, amount, amountPaidByParent, isAmountProRated, msgs, dc);

                var activityPaymentAmount = RegistrantActivityService.GetAmountPaid(registrantActivity, msgs, dc);
                var activityPaymentAmountPaidByParent = RegistrantActivityService.GetAmountPaidByParent(registrantActivity, msgs, dc);
                var activityPaymentIsProRated = RegistrantActivityService.GetIsProRatedFlagByParent(registrantActivity, msgs, dc);

                // Assert
                Assert.Equal(expected: activityPaymentAmount, actual: activityPayment.Amount);
                Assert.Equal(expected: activityPaymentAmountPaidByParent, actual: activityPayment.AmountPaidByParent);
                Assert.Equal(expected: activityPaymentIsProRated, actual: activityPayment.IsProRated);

                Assert.True(msgs.HasNoErrors());
            }
            catch (Exception ex)
            {
                Assert.False(false, ex.Message);
            }
            
        }


        [Fact]
        public void IsWithdrawEnabledTest()
        {
            // Arrange
            var activity = ActivityFactory.Create(account, msgs, dc);
            var registrant = RegistrantFactory.Create(msgs, dc);
            var registration = RegistrationFactory.Create(account, registrant, msgs, dc);
            var registrantActivity = RegistrantActivityFactory.Create(activity, registrant, registration, msgs, dc);

            foreach (RegistrationAssignmentType registrationAssignment in Enum.GetValues(typeof(RegistrationAssignmentType)))
            {
                // Act
                registrantActivity.AssignmentTypeId = registrationAssignment;

                // Assert
                switch (registrantActivity.AssignmentTypeId)
                {
                    case RegistrationAssignmentType.NotSet:
                    case RegistrationAssignmentType.Withdrawn:
                    case RegistrationAssignmentType.Declined:
                        Assert.False(registrantActivity.IsWithdrawEnabled());
                        break;

                    case RegistrationAssignmentType.Reserved:
                    case RegistrationAssignmentType.Applied:
                    case RegistrationAssignmentType.WaitingList:
                    case RegistrationAssignmentType.Invited:
                    case RegistrationAssignmentType.ReservedPendingPayment:
                    case RegistrationAssignmentType.ReservedPendingParentalConsent:
                    case RegistrationAssignmentType.Interested:
                        Assert.True(registrantActivity.IsWithdrawEnabled());
                        break;
                    
                    // Test should fail if values are added to the enum without updating tests
                    default:
                        Assert.True(false);
                        break;
                }
            }

        }

        [Fact]
        public void IsSendInvoiceEnabledTest()
        {
            // Arrange
            var activity = ActivityFactory.Create(account, msgs, dc);
            var registrant = RegistrantFactory.Create(msgs, dc);
            var registration = RegistrationFactory.Create(account, registrant, msgs, dc);
            var registrantActivity = RegistrantActivityFactory.Create(activity, registrant, registration, msgs, dc);

            foreach (RegistrationAssignmentType registrationAssignment in Enum.GetValues(typeof(RegistrationAssignmentType)))
            {
                // Act
                registrantActivity.AssignmentTypeId = registrationAssignment;

                // Assert
                switch (registrantActivity.AssignmentTypeId)
                {
                    case RegistrationAssignmentType.NotSet:
                    case RegistrationAssignmentType.Withdrawn:
                    case RegistrationAssignmentType.Declined:                    
                    case RegistrationAssignmentType.WaitingList:
                    case RegistrationAssignmentType.Invited:
                    case RegistrationAssignmentType.Interested:
                        Assert.False(registrantActivity.IsSendInvoiceEnabled());
                        break;

                    case RegistrationAssignmentType.Reserved:
                    case RegistrationAssignmentType.Applied:
                    case RegistrationAssignmentType.ReservedPendingPayment:
                    case RegistrationAssignmentType.ReservedPendingParentalConsent:
                        Assert.True(registrantActivity.IsSendInvoiceEnabled());
                        break;

                    // Test should fail if values are added to the enum without updating tests
                    default:
                        Assert.True(false);
                        break;
                }
            }

        }

        [Fact(DisplayName = "IsReservedRegistrationAssignmentTypeTest")]
        public void IsReservedRegistrationAssignmentTypeTest()
        {
            // Arrange
            var activity = ActivityFactory.Create(account, msgs, dc);
            var registrant = RegistrantFactory.Create(msgs, dc);
            var reservedRegistrantActivity = RegistrantActivityFactory.Create(activity, registrant, msgs, dc);
            var reservedPendingParentalConsentRegistrantActivity = RegistrantActivityFactory.Create(activity, registrant, msgs, dc);
            var reservedPendingPaymentRegistrantActivity = RegistrantActivityFactory.Create(activity, registrant, msgs, dc);
            var notReservedRegistrantActivity = RegistrantActivityFactory.Create(activity, registrant, msgs, dc);


            // Act
            reservedRegistrantActivity.AssignmentTypeId = RegistrationAssignmentType.Reserved;
            reservedPendingParentalConsentRegistrantActivity.AssignmentTypeId = RegistrationAssignmentType.ReservedPendingParentalConsent;
            reservedPendingPaymentRegistrantActivity.AssignmentTypeId = RegistrationAssignmentType.ReservedPendingPayment;
            notReservedRegistrantActivity.AssignmentTypeId = RegistrationAssignmentType.NotSet;

            msgs.AddRange(reservedRegistrantActivity.Save(dc));
            msgs.AddRange(reservedPendingParentalConsentRegistrantActivity.Save(dc));
            msgs.AddRange(reservedPendingPaymentRegistrantActivity.Save(dc));
            msgs.AddRange(notReservedRegistrantActivity.Save(dc));

            // Assert
            Assert.True(msgs.HasNoErrors());
            Assert.True(reservedRegistrantActivity.IsReservedRegistrationAssignmentType());
            Assert.True(reservedPendingParentalConsentRegistrantActivity.IsReservedRegistrationAssignmentType());
            Assert.True(reservedPendingPaymentRegistrantActivity.IsReservedRegistrationAssignmentType());
            Assert.False(notReservedRegistrantActivity.IsReservedRegistrationAssignmentType());


        }
    }
}
