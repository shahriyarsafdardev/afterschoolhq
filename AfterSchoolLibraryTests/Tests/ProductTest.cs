using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using Xunit.Abstractions;
using AfterSchoolLibrary;
using BasicLibrary;
using BasicLibrary.DataAccess;

namespace AfterSchoolLibraryTests
{
    public class ProductTest : BaseTest
    {
        public ProductTest(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public void LoadTest()
        {
            var accountOwner = Factories.WebUserFactory.Create(msgs, dc);
            var account = Factories.AccountFactory.Create(accountOwner, msgs, dc);
            var product = Factories.ProductFactory.Create(account, msgs, dc);

            Assert.True(Product.Load(product.ID.Value, msgs, dc).IsNotEmpty());
        }

        [Theory]
        [InlineData(8, 10)]
        [InlineData(15, 10)]
        public void LoadPagingTest(int numberToCreate, int numberPerPage)
        {
            for (var i = 0; i < numberToCreate; i++)
            {
                var accountOwner = Factories.WebUserFactory.Create(msgs, dc);
                var account = Factories.AccountFactory.Create(accountOwner, msgs, dc);
                var product = Factories.ProductFactory.Create(account, msgs, dc);
            }

            Assert.Equal(
                Math.Min(numberPerPage, numberToCreate),
                Product.Load(null, 0, numberPerPage, msgs, dc).Count
            );
        }

        /// <summary>
        /// Gets number of payment plans in the database
        /// </summary>
        /// <param name="numberOfProductsToCreate"></param>
        [Theory]
        [InlineData(8)]
        [InlineData(2)]
        public void GetCountTest(int numberToCreate)
        {
            for (var i = 0; i < numberToCreate; i++)
            {
                var accountOwner = Factories.WebUserFactory.Create(msgs, dc);
                var account = Factories.AccountFactory.Create(accountOwner, msgs, dc);
                var product = Factories.ProductFactory.Create(account, msgs, dc);
            }

            Assert.Equal(numberToCreate, Product.GetCount(msgs: msgs, dc: dc));
        }

        /// <summary>
        /// saves the plan to the DB
        /// </summary>
        [Fact]
        public void SaveTest()
        {
            var accountOwner = Factories.WebUserFactory.Create(msgs, dc);
            var account = Factories.AccountFactory.Create(accountOwner, msgs, dc);
            var product = Factories.ProductFactory.Create(account, msgs, dc);

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
            var accountOwner = Factories.WebUserFactory.Create(msgs, dc);
            var account = Factories.AccountFactory.Create(accountOwner, msgs, dc);
            var product = Factories.ProductFactory.Create(account, msgs, dc);

            msgs.AddRange(product.Delete(dc));

            Assert.Equal(0, Product.GetCount(msgs: msgs, dc: dc));
        }

        /// <summary>
        /// loads the plan using its own ID
        /// </summary>
        [Fact]
        public void ReloadTest()
        {
            var accountOwner = Factories.WebUserFactory.Create(msgs, dc);
            var account = Factories.AccountFactory.Create(accountOwner, msgs, dc);
            var product = Factories.ProductFactory.Create(account, msgs, dc);

            Assert.Equal(product.ID, product.Reload(msgs, dc).ID);
        }

        [Fact]
        public void LoadByAccountIdTest()
        {
            var accountOwner = Factories.WebUserFactory.Create(msgs, dc);
            var account = Factories.AccountFactory.Create(accountOwner, msgs, dc);
            var product = Factories.ProductFactory.Create(account, msgs, dc);

            Assert.Single(Product.LoadByAccountId(account.ID.Value, msgs, dc));
        }

        [Fact]
        public void LoadByUidTest()
        {
            var accountOwner = Factories.WebUserFactory.Create(msgs, dc);
            var account = Factories.AccountFactory.Create(accountOwner, msgs, dc);
            var product = Factories.ProductFactory.Create(account, msgs, dc);

            Assert.NotNull(Product.LoadByUid(product.Uid, msgs, dc));
        }
    }
}
