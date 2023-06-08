using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using AfterSchoolLibraryTests.Factories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BasicLibrary;
using Xunit;
using Xunit.Abstractions;
using AfterSchool.Models;

namespace AfterSchoolLibraryTests.Tests
{
    public class SubscriptionEndDateTest : BaseTest
    {
        //private WebUser AccountOwner { get; set; }
        //private Account PlanOwner { get; set; }
        public SubscriptionEndDateTest(ITestOutputHelper output) : base(output) 
        {
            //AccountOwner = WebUserFactory.Create(msgs, dc);
            //PlanOwner = AccountFactory.Create(AccountOwner, msgs, dc);
        }

        [Fact]
        public void VerifySubscriptionEndDate()
        {
            //Arrange
            DateTime startDate = new DateTime(2024, 05, 01);
            var periods = 4;

            //var activity = ActivityFactory.Create(PlanOwner, msgs, dc);

            var plan = new SubscriptionPlan()
            {
                PlanOwnerAccountId = 10,
                BillingPeriodTypeId = BillingPeriodType.Week,
                CostPerPeriod = 25,
                CreateDate = DateTime.UtcNow,
                CreatedBy = 10,
                LastModifiedDate = DateTime.UtcNow,
                LastModifiedBy = 10,
                IsActive = true,
                ParticipationObjectId = 10,
                ParticipationObjectTypeId = ParticipationObjectType.Activity,
                PaymentStartTypeId = PlanPaymentStartType.OnPeriodStart,
                IsOngoingSubscription = false,
                DoesAutoRenew = false,
                SplitCostEvenly = false,
                PlanTypeId = SubscriptionPlanType.Subscription,
                RegFeeAsDownPayment = false,
                IsConvenienceFeeTakenOutOfCost = false,
                StartDate = startDate,
                EndDate = null,
            };

            //Act
            var endDate = plan.CalculateEndDateFromNumberOfPeriods(startDate, periods);
            plan.EndDate = endDate;

            //Assert
            var expectedEndDate = new DateTime(2024, 05, 29);
            Assert.Equal(expectedEndDate, plan.EndDate);
        }
    }
}
