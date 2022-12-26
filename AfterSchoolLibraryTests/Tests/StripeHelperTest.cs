using System;
using AfterSchoolLibrary.Billing;
using AfterSchoolLibrary.Services;
using AfterSchoolLibraryTests.Factories;
using Stripe;
using Xunit;
using Xunit.Abstractions;

namespace AfterSchoolLibraryTests.Tests
{
    public class StripeHelperTest : BaseTest
    {
        public StripeHelperTest(ITestOutputHelper output) : base(output)
        {
        }

        private Product CreateStripeProduct(string id)
        {
            return StripeHelper.GetStripeHelperConnectAccount().CreateProduct(id, "my product name", "this is the best product ever", true, null, null, msgs);
        }

        private Plan CreateStripePlan(Product product, string id)
        {
            return StripeHelper.GetStripeHelperConnectAccount().CreatePlan(product, id, 35, BillingPeriodType.Month, "my plan nickname", null, null, msgs);
        }

        [Fact]
        public void CreateProductTest()
        {
            var product = CreateStripeProduct(Guid.NewGuid().ToString());
            Assert.NotNull(product);
            msgs.AddRange(StripeHelper.GetStripeHelperConnectAccount().DeleteProduct(product.Id));
        }

        [Fact]
        public void GetProductTest()
        {
            var product = CreateStripeProduct(Guid.NewGuid().ToString());
            Assert.NotNull(StripeHelper.GetStripeHelperConnectAccount().GetProduct(product.Id, null, msgs));
            msgs.AddRange(StripeHelper.GetStripeHelperConnectAccount().DeleteProduct(product.Id));
        }

        [Fact]
        public void DeleteProductTest()
        {
            var productId = Guid.NewGuid().ToString();
            var product = CreateStripeProduct(productId);
            msgs.AddRange(StripeHelper.GetStripeHelperConnectAccount().DeleteProduct(productId));

            Assert.Null(StripeHelper.GetStripeHelperConnectAccount().GetProduct(productId, null, msgs));
        }

        [Fact]
        public void UpdateProduct()
        {
            var product = CreateStripeProduct(Guid.NewGuid().ToString());
            var newName = "my product name 2";
            var product2 = StripeHelper.GetStripeHelperConnectAccount()
                .UpdateProduct(product.Id, newName, "this is the 2nd best product ever", false, null, msgs);

            Assert.Equal(newName, product2.Name);

            msgs.AddRange(StripeHelper.GetStripeHelperConnectAccount().DeleteProduct(product.Id));
        }

        [Fact]
        public void CreatePlanTest()
        {
            var product = CreateStripeProduct(Guid.NewGuid().ToString());
            var plan = CreateStripePlan(product, Guid.NewGuid().ToString());
            Assert.NotNull(plan);
            msgs.AddRange(StripeHelper.GetStripeHelperConnectAccount().DeletePlan(plan.Id));
            msgs.AddRange(StripeHelper.GetStripeHelperConnectAccount().DeleteProduct(product.Id));
        }

        [Fact]
        public void GetPlanTest()
        {
            var product = CreateStripeProduct(Guid.NewGuid().ToString());
            var plan = CreateStripePlan(product, Guid.NewGuid().ToString());
            Assert.NotNull(StripeHelper.GetStripeHelperConnectAccount().GetPlan(plan.Id, null));
            msgs.AddRange(StripeHelper.GetStripeHelperConnectAccount().DeletePlan(plan.Id));
            msgs.AddRange(StripeHelper.GetStripeHelperConnectAccount().DeleteProduct(product.Id));
        }

        [Fact]
        public void UpdatePlanTest()
        {
            var product = CreateStripeProduct(Guid.NewGuid().ToString());
            var plan = CreateStripePlan(product, Guid.NewGuid().ToString());
            var updatedNickname = "my plan nickname 2";

            var plan2 = StripeHelper.GetStripeHelperConnectAccount().UpdatePlan(plan.Id, updatedNickname, false, null, msgs);

            Assert.Equal(updatedNickname, plan2.Nickname);

            msgs.AddRange(StripeHelper.GetStripeHelperConnectAccount().DeletePlan(plan.Id));
            msgs.AddRange(StripeHelper.GetStripeHelperConnectAccount().DeleteProduct(product.Id));
        }

        [Fact]
        public void DeletePlanTest()
        {
            var planId = "myplanid";
            var product = CreateStripeProduct(Guid.NewGuid().ToString());
            var plan = CreateStripePlan(product, planId);

            msgs.AddRange(StripeHelper.GetStripeHelperConnectAccount().DeletePlan(planId));

            Assert.Null(StripeHelper.GetStripeHelperConnectAccount().GetPlan(planId, null));

            msgs.AddRange(StripeHelper.GetStripeHelperConnectAccount().DeleteProduct(product.Id));
        }

        [Fact]
        public void CreateCustomerTest()
        {
            var customer = StripeHelper.GetStripeHelperConnectAccount().CreateCustomer("This is my customer", "test@test.com", null, "", null, msgs);
            Assert.NotNull(customer);
            msgs.AddRange(StripeHelper.GetStripeHelperConnectAccount().DeleteCustomer(customer.Id));
        }

        [Fact]
        public void UpdateCustomerTest()
        {
            var customer = StripeHelper.GetStripeHelperConnectAccount().CreateCustomer("This is my customer", "test@test.com", null, "", null, msgs);
            var newDescription = "this is my new customer";
            var customer2 = StripeHelper.GetStripeHelperConnectAccount().UpdateCustomer(customer.Id, newDescription, "test@test.com", null, msgs);

            Assert.Equal(newDescription, customer2.Description);

            msgs.AddRange(StripeHelper.GetStripeHelperConnectAccount().DeleteCustomer(customer.Id));
        }

        [Fact(Skip = "Need to fix")]
        public void CreateCardTest()
        {
            //var customer = StripeHelper.CreateCustomer("This is my customer", "test@test.com", msgs);
            //var card = StripeHelper.CreateCard(customer.Id, StripeHelper.GetTestCard(StripeTestCardType.Visa), msgs);

            //Assert.NotNull(card);

            //msgs.AddRange(StripeHelper.DeleteCard(customer.Id, card.Id));
            //msgs.AddRange(StripeHelper.DeleteCustomer(customer.Id));
        }

        [Fact(Skip = "Need to fix")]
        public void GetCardsTest()
        {
            //var customer = StripeHelper.CreateCustomer("This is my customer", "test@test.com", msgs);
            //var card = StripeHelper.CreateCard(customer.Id, StripeHelper.GetTestCard(StripeTestCardType.Visa), msgs);

            //Assert.Single(StripeHelper.GetCards(customer.Id, msgs));

            //msgs.AddRange(StripeHelper.DeleteCard(customer.Id, card.Id));
            //msgs.AddRange(StripeHelper.DeleteCustomer(customer.Id));
        }

        [Fact(Skip = "Need to fix")]
        public void DeleteCardTest()
        {
            //var customer = StripeHelper.CreateCustomer("This is my customer", "test@test.com", msgs);
            //var card = StripeHelper.CreateCard(customer.Id, StripeHelper.GetTestCard(StripeTestCardType.Visa), msgs);
            //msgs.AddRange(StripeHelper.DeleteCard(customer.Id, card.Id));

            //Assert.Empty(StripeHelper.GetCards(customer.Id, msgs));

            //msgs.AddRange(StripeHelper.DeleteCustomer(customer.Id));
        }

        [Fact(Skip = "Can't test yet")]
        public void CreateChargeTest()
        {
            var accountOwner = WebUserFactory.Create(msgs, dc);
            var account = AccountFactory.Create(accountOwner, msgs, dc);

            msgs.AddRange(account.SaveWithStripe(StripeAccountType.ConnectAccount, dc));
        }
    }
}