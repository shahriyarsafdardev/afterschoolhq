using Xunit;
using Xunit.Abstractions;
using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using BasicLibrary;
using AfterSchoolLibraryTests.Factories;

namespace AfterSchoolLibraryTests.Tests
{
    public class SubscriptionPlanServiceTest : BaseTest
    {
        private WebUser LoggedInUser { get; set; }
        private WebUser AccountOwner { get; set; }
        private Account PlanOwner { get; set; }
        private Package Package { get; }
        
        public SubscriptionPlanServiceTest(ITestOutputHelper output) : base(output)
        {
            LoggedInUser = WebUserFactory.Create(msgs, dc);
            AccountOwner = WebUserFactory.Create(msgs, dc);
            PlanOwner = AccountFactory.Create(AccountOwner, msgs, dc);
            Package = PackageFactory.CreateFreePackage(msgs, dc);                         
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

    }
}
