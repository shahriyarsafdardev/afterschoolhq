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

        [Fact]
        public void SaveTest()
        {
            var product = Factories.ProductFactory.Create(ProductAccount, msgs, dc);

            product.LastModifiedBy = 1;
            msgs.AddRange(product.Save(dc));

            Assert.True(msgs.HasNoErrors());
        }

        [Fact]
        public void DeleteTest()
        {
            var product = Factories.ProductFactory.Create(ProductAccount, msgs, dc);

            msgs.AddRange(product.Delete(dc));

            Assert.Null(Product.Load(product.ID.Value));
        }

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
