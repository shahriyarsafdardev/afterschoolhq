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
            this.StripeHelper = new StripeHelper();
        }

        [Fact]
        public void CreateProductTest()
        {
            var product = this.StripeHelper.CreateProduct("myproductid", "my product name", "this is the best product ever", true, msgs);
            Assert.NotNull(product);
            msgs.AddRange(this.StripeHelper.DeleteProduct(product.Id));
        }

        [Fact]
        public void GetProductTest()
        {
            var product = this.StripeHelper.CreateProduct("myproductid", "my product name", "this is the best product ever", true, msgs);
            Assert.NotNull(this.StripeHelper.GetProduct(product.Id, msgs));
            msgs.AddRange(this.StripeHelper.DeleteProduct(product.Id));
        }

        [Fact]
        public void DeleteProductTest()
        {
            var productId = "myproductid";
            var product = this.StripeHelper.CreateProduct(productId, "my product name", "this is the best product ever", true, msgs);
            msgs.AddRange(this.StripeHelper.DeleteProduct(productId));

            Assert.Null(this.StripeHelper.GetProduct(productId, msgs));
        }

        [Fact]
        public void UpdateProduct()
        {
            var product = this.StripeHelper.CreateProduct("myproductid", "my product name", "this is the best product ever", true, msgs);
            var newName = "my product name 2";
            msgs.AddRange(this.StripeHelper.UpdateProduct(product.Id, newName, "this is the 2nd best product ever", false));

            Assert.Equal(newName, this.StripeHelper.GetProduct(product.Id, msgs).Name);

            msgs.AddRange(this.StripeHelper.DeleteProduct(product.Id));
        }

        [Fact]
        public void CreatePlanTest()
        {
            var product = this.StripeHelper.CreateProduct("myproductid", "my product name", "this is the best product ever", true, msgs);
            var plan = this.StripeHelper.CreatePlan(product, "myplanid", 35, BillingPeriodType.Month, "my plan nickname", msgs);
            Assert.NotNull(plan);
            msgs.AddRange(this.StripeHelper.DeletePlan(plan.Id));
            msgs.AddRange(this.StripeHelper.DeleteProduct(product.Id));
        }

        [Fact]
        public void GetPlanTest()
        {
            var product = this.StripeHelper.CreateProduct("myproductid", "my product name", "this is the best product ever", true, msgs);
            var plan = this.StripeHelper.CreatePlan(product, "myplanid", 35, BillingPeriodType.Month, "my plan nickname", msgs);
            Assert.NotNull(this.StripeHelper.GetPlan(plan.Id, msgs));
            msgs.AddRange(this.StripeHelper.DeletePlan(plan.Id));
            msgs.AddRange(this.StripeHelper.DeleteProduct(product.Id));
        }

        [Fact]
        public void UpdatePlanTest()
        {
            var product = this.StripeHelper.CreateProduct("myproductid", "my product name", "this is the best product ever", true, msgs);
            var plan = this.StripeHelper.CreatePlan(product, "myplanid", 35, BillingPeriodType.Month, "my plan nickname", msgs);
            var updatedNickname = "my plan nickname 2";

            msgs.AddRange(this.StripeHelper.UpdatePlan(plan.Id, updatedNickname, false));

            Assert.Equal(updatedNickname, this.StripeHelper.GetPlan(plan.Id, msgs).Nickname);

            msgs.AddRange(this.StripeHelper.DeletePlan(plan.Id));
            msgs.AddRange(this.StripeHelper.DeleteProduct(product.Id));
        }

        [Fact]
        public void DeletePlanTest()
        {
            var planId = "myplanid";
            var product = this.StripeHelper.CreateProduct("myproductid", "my product name", "this is the best product ever", true, msgs);
            var plan = this.StripeHelper.CreatePlan(product, planId, 35, BillingPeriodType.Month, "my plan nickname", msgs);

            msgs.AddRange(this.StripeHelper.DeletePlan(planId));

            Assert.Null(this.StripeHelper.GetPlan(planId, msgs));

            msgs.AddRange(this.StripeHelper.DeleteProduct(product.Id));
        }

        [Fact]
        public void CreateCustomerTest()
        {
            var customer = this.StripeHelper.CreateCustomer("This is my customer", "test@test.com", msgs);
            Assert.NotNull(customer);
            msgs.AddRange(this.StripeHelper.DeleteCustomer(customer.Id));
        }

        [Fact]
        public void DeleteCustomerTest()
        {
            var customer = this.StripeHelper.CreateCustomer("This is my customer", "test@test.com", msgs);
            msgs.AddRange(this.StripeHelper.DeleteCustomer(customer.Id));

            var customerReload = this.StripeHelper.GetCustomer(customer.Id, msgs);

            Assert.True((customerReload.Deleted.HasValue && customerReload.Deleted.Value));
        }

        [Fact]
        public void GetCustomer()
        {
            var customer = this.StripeHelper.CreateCustomer("This is my customer", "test@test.com", msgs);
            Assert.NotNull(this.StripeHelper.GetCustomer(customer.Id, msgs));
            msgs.AddRange(this.StripeHelper.DeleteCustomer(customer.Id));
        }

        [Fact]
        public void UpdateCustomerDefaultCardTest()
        {
            var customer = this.StripeHelper.CreateCustomer("This is my customer", "test@test.com", msgs);
            var card = this.StripeHelper.CreateCard(customer.Id, this.StripeHelper.GetTestCard(StripeTestCardType.Visa), msgs);
            
            msgs.AddRange(this.StripeHelper.UpdateCustomerDefaultCard(customer.Id, card.Id));

            Assert.Equal(card.Id, this.StripeHelper.GetCustomer(customer.Id, msgs).DefaultSourceId);

            msgs.AddRange(this.StripeHelper.DeleteCard(customer.Id, card.Id));
            msgs.AddRange(this.StripeHelper.DeleteCustomer(customer.Id));

        }

        [Fact]
        public void UpdateCustomerTest()
        {
            var customer = this.StripeHelper.CreateCustomer("This is my customer", "test@test.com", msgs);
            var newDescription = "this is my new customer";
            msgs.AddRange(this.StripeHelper.UpdateCustomer(customer.Id, newDescription, "test@test.com"));

            Assert.Equal(newDescription, this.StripeHelper.GetCustomer(customer.Id, msgs).Description);

            msgs.AddRange(this.StripeHelper.DeleteCustomer(customer.Id));
        }

        [Fact]
        public void CreateCardTest()
        {
            var customer = this.StripeHelper.CreateCustomer("This is my customer", "test@test.com", msgs);
            var card = this.StripeHelper.CreateCard(customer.Id, this.StripeHelper.GetTestCard(StripeTestCardType.Visa), msgs);

            Assert.NotNull(card);

            msgs.AddRange(this.StripeHelper.DeleteCard(customer.Id, card.Id));
            msgs.AddRange(this.StripeHelper.DeleteCustomer(customer.Id));
        }

        [Fact]
        public void GetCardsTest()
        {
            var customer = this.StripeHelper.CreateCustomer("This is my customer", "test@test.com", msgs);
            var card = this.StripeHelper.CreateCard(customer.Id, this.StripeHelper.GetTestCard(StripeTestCardType.Visa), msgs);

            Assert.Single(this.StripeHelper.GetCards(customer.Id, msgs));

            msgs.AddRange(this.StripeHelper.DeleteCard(customer.Id, card.Id));
            msgs.AddRange(this.StripeHelper.DeleteCustomer(customer.Id));
        }

        [Fact]
        public void DeleteCardTest()
        {
            var customer = this.StripeHelper.CreateCustomer("This is my customer", "test@test.com", msgs);
            var card = this.StripeHelper.CreateCard(customer.Id, this.StripeHelper.GetTestCard(StripeTestCardType.Visa), msgs);
            msgs.AddRange(this.StripeHelper.DeleteCard(customer.Id, card.Id));

            Assert.Empty(this.StripeHelper.GetCards(customer.Id, msgs));

            msgs.AddRange(this.StripeHelper.DeleteCustomer(customer.Id));
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
