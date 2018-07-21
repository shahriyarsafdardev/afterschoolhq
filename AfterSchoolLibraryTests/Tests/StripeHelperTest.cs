using BasicLibrary;
using AfterSchoolLibrary;
using AfterSchoolLibrary.Billing;
using Xunit;
using Xunit.Abstractions;
using AfterSchoolLibraryTests.Factories;


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
            var product = StripeHelper.CreateProduct("myproductid", "my product name", "this is the best product ever", true, msgs);
            Assert.NotNull(product);
            msgs.AddRange(StripeHelper.DeleteProduct(product.Id));
        }

        [Fact]
        public void GetProductTest()
        {
            var product = StripeHelper.CreateProduct("myproductid", "my product name", "this is the best product ever", true, msgs);
            Assert.NotNull(StripeHelper.GetProduct(product.Id, msgs));
            msgs.AddRange(StripeHelper.DeleteProduct(product.Id));
        }

        [Fact]
        public void DeleteProductTest()
        {
            var productId = "myproductid";
            var product = StripeHelper.CreateProduct(productId, "my product name", "this is the best product ever", true, msgs);
            msgs.AddRange(StripeHelper.DeleteProduct(productId));

            Assert.Null(StripeHelper.GetProduct(productId, msgs));
        }

        [Fact]
        public void UpdateProduct()
        {
            var product = StripeHelper.CreateProduct("myproductid", "my product name", "this is the best product ever", true, msgs);
            var newName = "my product name 2";
            msgs.AddRange(StripeHelper.UpdateProduct(product.Id, newName, "this is the 2nd best product ever", false));

            Assert.Equal(newName, StripeHelper.GetProduct(product.Id, msgs).Name);

            msgs.AddRange(StripeHelper.DeleteProduct(product.Id));
        }

        [Fact]
        public void CreatePlanTest()
        {
            var product = StripeHelper.CreateProduct("myproductid", "my product name", "this is the best product ever", true, msgs);
            var plan = StripeHelper.CreatePlan(product, "myplanid", 35, BillingPeriodType.Month, "my plan nickname", msgs);
            Assert.NotNull(plan);
            msgs.AddRange(StripeHelper.DeletePlan(plan.Id));
            msgs.AddRange(StripeHelper.DeleteProduct(product.Id));
        }

        [Fact]
        public void GetPlanTest()
        {
            var product = StripeHelper.CreateProduct("myproductid", "my product name", "this is the best product ever", true, msgs);
            var plan = StripeHelper.CreatePlan(product, "myplanid", 35, BillingPeriodType.Month, "my plan nickname", msgs);
            Assert.NotNull(StripeHelper.GetPlan(plan.Id, msgs));
            msgs.AddRange(StripeHelper.DeletePlan(plan.Id));
            msgs.AddRange(StripeHelper.DeleteProduct(product.Id));
        }

        [Fact]
        public void UpdatePlanTest()
        {
            var product = StripeHelper.CreateProduct("myproductid", "my product name", "this is the best product ever", true, msgs);
            var plan = StripeHelper.CreatePlan(product, "myplanid", 35, BillingPeriodType.Month, "my plan nickname", msgs);
            var updatedNickname = "my plan nickname 2";

            msgs.AddRange(StripeHelper.UpdatePlan(plan.Id, updatedNickname, false));

            Assert.Equal(updatedNickname, StripeHelper.GetPlan(plan.Id, msgs).Nickname);

            msgs.AddRange(StripeHelper.DeletePlan(plan.Id));
            msgs.AddRange(StripeHelper.DeleteProduct(product.Id));
        }

        [Fact]
        public void DeletePlanTest()
        {
            var planId = "myplanid";
            var product = StripeHelper.CreateProduct("myproductid", "my product name", "this is the best product ever", true, msgs);
            var plan = StripeHelper.CreatePlan(product, planId, 35, BillingPeriodType.Month, "my plan nickname", msgs);

            msgs.AddRange(StripeHelper.DeletePlan(planId));

            Assert.Null(StripeHelper.GetPlan(planId, msgs));

            msgs.AddRange(StripeHelper.DeleteProduct(product.Id));
        }

        [Fact]
        public void CreateCustomerTest()
        {
            var customer = StripeHelper.CreateCustomer("This is my customer", "test@test.com", msgs);
            Assert.NotNull(customer);
            msgs.AddRange(StripeHelper.DeleteCustomer(customer.Id));
        }

        [Fact]
        public void DeleteCustomerTest()
        {
            var customer = StripeHelper.CreateCustomer("This is my customer", "test@test.com", msgs);
            msgs.AddRange(StripeHelper.DeleteCustomer(customer.Id));

            var customerReload = StripeHelper.GetCustomer(customer.Id, msgs);

            Assert.True((customerReload.Deleted.HasValue && customerReload.Deleted.Value));
        }

        [Fact]
        public void GetCustomer()
        {
            var customer = StripeHelper.CreateCustomer("This is my customer", "test@test.com", msgs);
            Assert.NotNull(StripeHelper.GetCustomer(customer.Id, msgs));
            msgs.AddRange(StripeHelper.DeleteCustomer(customer.Id));
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
            var customer = StripeHelper.CreateCustomer("This is my customer", "test@test.com", msgs);
            var newDescription = "this is my new customer";
            msgs.AddRange(StripeHelper.UpdateCustomer(customer.Id, newDescription, "test@test.com"));

            Assert.Equal(newDescription, StripeHelper.GetCustomer(customer.Id, msgs).Description);

            msgs.AddRange(StripeHelper.DeleteCustomer(customer.Id));
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
