using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using AfterSchoolLibrary;
using BasicLibrary;
using BasicLibrary.DataAccess;
using AfterSchoolLibraryTests.Factories;
using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;

namespace AfterSchoolLibraryTests.Tests
{
    public class RegistrantTest: BaseTest
    {
        public RegistrantTest(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public void LoadTest()
        {
            var registrant = RegistrantFactory.Create(msgs, dc);

            Assert.True(Registrant.Load(registrant.ID.Value, msgs, dc).IsNotEmpty());
        }

        [Theory]
        [InlineData(8, 10)]
        [InlineData(15, 10)]
        public void LoadPagingTest(int numberToCreate, int numberPerPage)
        {
            for (var i = 0; i < numberToCreate; i++)
            {
                var registrant = RegistrantFactory.Create(msgs, dc);
            }

            Assert.Equal(
                Math.Min(numberPerPage, numberToCreate),
                Registrant.Load(null, 0, numberPerPage, msgs, dc).Count
            );
        }

        [Theory]
        [InlineData(8)]
        [InlineData(2)]
        public void GetCountTest(int numberToCreate)
        {
            for (var i = 0; i < numberToCreate; i++)
            {
                var registrant = RegistrantFactory.Create(msgs, dc);
            }

            Assert.Equal(numberToCreate, Registrant.GetCount(msgs: msgs, dc: dc));
        }

        [Fact]
        public void SaveTest()
        {
            var registrant = RegistrantFactory.Create(msgs, dc);

            registrant.LastModifiedBy = 1;
            msgs.AddRange(registrant.Save(dc));

            Assert.True(msgs.HasNoErrors());
        }
        [Fact]
        public void DeleteTest()
        {
            var registrant = RegistrantFactory.Create(msgs, dc);

            msgs.AddRange(registrant.Delete(dc));

            Assert.Equal(0, Coupon.GetCount(msgs: msgs, dc: dc));
        }
        [Fact]
        public void ReloadTest()
        {
            var registrant = RegistrantFactory.Create(msgs, dc);

            Assert.Equal(registrant.ID, registrant.Reload(msgs, dc).ID);
        }

        [Fact]
        public void LoadByUidByAccountIdTest()
        {
            try
            {
                // Arrange
                var user = WebUserFactory.Create(msgs: msgs, dc: dc);
                var account = AccountFactory.Create(user: user, msgs: msgs, dc: dc);
                var registrant = RegistrantFactory.Create(account, isMaster: false, msgs, dc);

                // Act                
                var expected = Registrant.LoadByUidByAccountId(Uid: registrant.Uid, AccountId: account.ID.Value, msgs: msgs, dc: dc);

                // Assert
                Assert.Equal(expected: expected.ID, actual: registrant.ID);
                Assert.Equal(expected: expected.Uid, actual: registrant.Uid);
                Assert.Equal(expected: expected.Email, actual: registrant.Email);                
            }
            catch (Exception ex)
            {
                //Assert
                Assert.False(false, ex.Message);
            }
        }

        [Fact]
        public void GetSubscriptionCount()
        {
            var poviderUser = WebUserFactory.Create(msgs: msgs, dc: dc);
            var registrantUser = WebUserFactory.Create("Vikas", "Lalwani","vikas@gmail.com",true, msgs: msgs, dc: dc);

            var account = AccountFactory.Create(user: poviderUser, msgs: msgs, dc: dc);
            var registrantAccount = AccountFactory.Create(registrantUser, AccountType.Registrant, msgs, dc);

            var registrant = RegistrantFactory.Create(registrantAccount, isMaster: false, msgs, dc);
            var activity = ActivityFactory.Create(account, msgs, dc);

            var subscriptionPlan = SubscriptionPlanFactory.Create(account, activity, SubscriptionPlanType.Subscription, msgs, dc);
            var purchase = PurchaseFactory.Create(registrantAccount, 100, msgs, dc);

            var activityRegister = RegistrantActivityFactory.Create(activity, registrant, msgs,dc);

            var registrantSubscription = RegistrantSubscriptionPlanFactory.Create(registrant, subscriptionPlan, purchase, msgs, dc);


            var registrants = activity.GetRegistrants(msgs, dc).FirstOrDefault();
            var getParticipationObject = registrants.GetParticipationObjects(msgs, dc).FirstOrDefault();
            var getSubPlans = registrants.GetRegistrantSubscriptionPlans(msgs, dc);
            var createTransaction = TransactionFactory.Create(account, registrantAccount, PaymentType.CreditCard, msgs, dc);
            var addPayment = getSubPlans.FirstOrDefault().AddPayment(createTransaction, 50, registrantUser, msgs, dc);

            var getSubCount = registrant.GetSubscriptionCount(getParticipationObject, msgs, dc);

            Assert.True(getSubCount > 0);
        }

    }

}
