using System;
using Xunit;
using Xunit.Abstractions;
using AfterSchoolLibrary.Models;
using BasicLibrary;
using BasicLibrary.DataAccess;

namespace AfterSchoolLibraryTests.Tests
{
    public class ProductTest : BaseTest
    {
        private WebUser AccountOwner { get; set; }
        private Account ProductAccount { get; set; }

        public ProductTest(ITestOutputHelper output) : base(output)
        {
            AccountOwner = Factories.WebUserFactory.Create(msgs, dc);
            ProductAccount = Factories.AccountFactory.Create(AccountOwner, msgs, dc);
        }

        [Fact]
        public void LoadTest()
        {
            var product = Factories.ProductFactory.Create(ProductAccount, msgs, dc);

            Assert.True(Product.Load(product.ID.Value, msgs, dc).IsNotEmpty());
        }

        [Theory]
        [InlineData(8, 10)]
        [InlineData(15, 10)]
        public void LoadPagingTest(int numberToCreate, int numberPerPage)
        {
            for (var i = 0; i < numberToCreate; i++)
            {
                var product = Factories.ProductFactory.Create(ProductAccount, msgs, dc);
            }

            Assert.Equal(
                Math.Min(numberPerPage, numberToCreate),
                Product.Load(null, 0, numberPerPage, msgs, dc).Count
            );
        }

        /// <summary>
        /// Gets number of products in the database
        /// </summary>
        /// <param name="numberToCreate"></param>
        [Theory]
        [InlineData(8)]
        [InlineData(2)]
        public void GetCountTest(int numberToCreate)
        {
            for (var i = 0; i < numberToCreate; i++)
            {
                var product = Factories.ProductFactory.Create(ProductAccount, msgs, dc);
            }

            Assert.Equal(numberToCreate, Product.GetCount(msgs: msgs, dc: dc));
        }

        /// <summary>
        /// saves the plan to the DB
        /// </summary>
        [Fact]
        public void SaveTest()
        {
            var product = Factories.ProductFactory.Create(ProductAccount, msgs, dc);

            product.LastModifiedBy = 1;
            msgs.AddRange(product.Save(dc));

            Assert.True(msgs.HasNoErrors());
        }

        /// <summary>
        /// removes the plan from the DB
        /// </summary>
        [Fact]
        public void DeleteTest()
        {
            var product = Factories.ProductFactory.Create(ProductAccount, msgs, dc);

            msgs.AddRange(product.Delete(dc));

            Assert.Equal(0, Product.GetCount(msgs: msgs, dc: dc));
        }

        /// <summary>
        /// loads the plan using its own ID
        /// </summary>
        [Fact]
        public void ReloadTest()
        {
            var product = Factories.ProductFactory.Create(ProductAccount, msgs, dc);

            Assert.Equal(product.ID, product.Reload(msgs, dc).ID);
        }

        [Fact]
        public void LoadByAccountIdTest()
        {
            var product = Factories.ProductFactory.Create(ProductAccount, msgs, dc);

            Assert.Single(Product.LoadByAccountId(ProductAccount.ID.Value, msgs, dc));
        }

        [Fact]
        public void LoadByUidTest()
        {
            var product = Factories.ProductFactory.Create(ProductAccount, msgs, dc);

            Assert.NotNull(Product.LoadByUid(product.Uid, msgs, dc));
        }
    }
}
