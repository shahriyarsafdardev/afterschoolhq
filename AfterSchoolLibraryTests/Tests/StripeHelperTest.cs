using BasicLibrary;
using AfterSchoolLibrary;
using AfterSchoolLibrary.Billing;
using Xunit;
using Xunit.Abstractions;
using AfterSchoolLibraryTests.Factories;
using System;
using AfterSchoolLibrary.Services;

namespace AfterSchoolLibraryTests.Tests
{
    public class StripeHelperTest : BaseTest
    {
        private StripeHelper StripeHelper;

        public StripeHelperTest(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public void CreateProductTest()
        {
            var product = StripeHelper.CreateProduct(Guid.NewGuid().ToString(), "my product name", "this is the best product ever", true, null, msgs);
            Assert.NotNull(product);
            msgs.AddRange(StripeHelper.DeleteProduct(product.Id, null));
        }

        [Fact]
        public void GetProductTest()
        {
            var product = StripeHelper.CreateProduct(Guid.NewGuid().ToString(), "my product name", "this is the best product ever", true, null, msgs);
            Assert.NotNull(StripeHelper.GetProduct(product.Id, null, msgs));
            msgs.AddRange(StripeHelper.DeleteProduct(product.Id, null));
        }

        [Fact]
        public void DeleteProductTest()
        {
            var productId = Guid.NewGuid().ToString();
            var product = StripeHelper.CreateProduct(productId, "my product name", "this is the best product ever", true, null, msgs);
            msgs.AddRange(StripeHelper.DeleteProduct(productId, null));

            Assert.Null(StripeHelper.GetProduct(productId, null, msgs));
        }

        [Fact]
        public void UpdateProduct()
        {
            var product = StripeHelper.CreateProduct(Guid.NewGuid().ToString(), "my product name", "this is the best product ever", true, null, msgs);
            var newName = "my product name 2";
            var product2 = StripeHelper.UpdateProduct(product.Id, newName, "this is the 2nd best product ever", false, null, msgs);

            Assert.Equal(newName, product2.Name);

            msgs.AddRange(StripeHelper.DeleteProduct(product.Id, null));
        }

        [Fact]
        public void CreatePlanTest()
        {
            var product = StripeHelper.CreateProduct(Guid.NewGuid().ToString(), "my product name", "this is the best product ever", true, null, msgs);
            var plan = StripeHelper.CreatePlan(product, Guid.NewGuid().ToString(), 35, BillingPeriodType.Month, "my plan nickname", null, msgs);
            Assert.NotNull(plan);
            msgs.AddRange(StripeHelper.DeletePlan(plan.Id, null));
            msgs.AddRange(StripeHelper.DeleteProduct(product.Id, null));
        }

        [Fact]
        public void GetPlanTest()
        {
            var product = StripeHelper.CreateProduct(Guid.NewGuid().ToString(), "my product name", "this is the best product ever", true, null, msgs);
            var plan = StripeHelper.CreatePlan(product, Guid.NewGuid().ToString(), 35, BillingPeriodType.Month, "my plan nickname", null, msgs);
            Assert.NotNull(StripeHelper.GetPlan(plan.Id, null, msgs));
            msgs.AddRange(StripeHelper.DeletePlan(plan.Id, null));
            msgs.AddRange(StripeHelper.DeleteProduct(product.Id, null));
        }

        [Fact]
        public void UpdatePlanTest()
        {
            var product = StripeHelper.CreateProduct(Guid.NewGuid().ToString(), "my product name", "this is the best product ever", true, null, msgs);
            var plan = StripeHelper.CreatePlan(product, Guid.NewGuid().ToString(), 35, BillingPeriodType.Month, "my plan nickname", null, msgs);
            var updatedNickname = "my plan nickname 2";

            var plan2 = StripeHelper.UpdatePlan(plan.Id, updatedNickname, false, null, msgs);

            Assert.Equal(updatedNickname, plan2.Nickname);

            msgs.AddRange(StripeHelper.DeletePlan(plan.Id, null));
            msgs.AddRange(StripeHelper.DeleteProduct(product.Id, null));
        }

        [Fact]
        public void DeletePlanTest()
        {
            var planId = "myplanid";
            var product = StripeHelper.CreateProduct(Guid.NewGuid().ToString(), "my product name", "this is the best product ever", true, null, msgs);
            var plan = StripeHelper.CreatePlan(product, planId, 35, BillingPeriodType.Month, "my plan nickname", null, msgs);

            msgs.AddRange(StripeHelper.DeletePlan(planId, null));

            Assert.Null(StripeHelper.GetPlan(planId, null, msgs));

            msgs.AddRange(StripeHelper.DeleteProduct(product.Id, null));
        }

        [Fact]
        public void CreateCustomerTest()
        {
            var customer = StripeHelper.CreateCustomer("This is my customer", "test@test.com", null, msgs);
            Assert.NotNull(customer);
            msgs.AddRange(StripeHelper.DeleteCustomer(customer.Id, null));
        }

        [Fact]
        public void DeleteCustomerTest()
        {
            var customer = StripeHelper.CreateCustomer("This is my customer", "test@test.com", null, msgs);
            msgs.AddRange(StripeHelper.DeleteCustomer(customer.Id, null));

            var customerReload = StripeHelper.GetCustomer(customer.Id, null, msgs);

            Assert.True((customerReload.Deleted.HasValue && customerReload.Deleted.Value));
        }

        [Fact]
        public void GetCustomer()
        {
            var customer = StripeHelper.CreateCustomer("This is my customer", "test@test.com", null, msgs);
            Assert.NotNull(StripeHelper.GetCustomer(customer.Id, null, msgs));
            msgs.AddRange(StripeHelper.DeleteCustomer(customer.Id, null));
        }

        [Fact(Skip = "Need to fix")]
        public void UpdateCustomerDefaultCardTest()
        {
            //var customer = StripeHelper.CreateCustomer("This is my customer", "test@test.com", msgs);
            //var card = StripeHelper.CreateCard(customer.Id, StripeHelper.GetTestCard(StripeTestCardType.Visa), msgs);
            
            //msgs.AddRange(StripeHelper.UpdateCustomerDefaultCard(customer.Id, card.Id));

            //Assert.Equal(card.Id, StripeHelper.GetCustomer(customer.Id, msgs).DefaultSourceId);

            //msgs.AddRange(StripeHelper.DeleteCard(customer.Id, card.Id));
            //msgs.AddRange(StripeHelper.DeleteCustomer(customer.Id));

        }

        [Fact]
        public void UpdateCustomerTest()
        {
            var customer = StripeHelper.CreateCustomer("This is my customer", "test@test.com", null, msgs);
            var newDescription = "this is my new customer";
            var customer2 = StripeHelper.UpdateCustomer(customer.Id, newDescription, "test@test.com", null, msgs);

            Assert.Equal(newDescription, customer2.Description);

            msgs.AddRange(StripeHelper.DeleteCustomer(customer.Id, null));
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

            msgs.AddRange(account.SaveWithStripe(dc));            
            
        }

    }
}
