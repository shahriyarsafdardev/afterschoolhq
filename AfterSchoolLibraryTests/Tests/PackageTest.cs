using System;
using Xunit;
using Xunit.Abstractions;
using AfterSchoolLibrary;
using BasicLibrary;

namespace AfterSchoolLibraryTests.Tests
{
    public class PackageTest: BaseTest
    {
        public PackageTest(ITestOutputHelper output) : base(output)
        {

        }

        [Fact]
        public void LoadTest()
        {
            var package = Factories.PackageFactory.Create(msgs, dc);

            Assert.True(Package.Load(package.ID.Value, msgs, dc).IsNotEmpty());
        }

        [Theory]
        [InlineData(8, 10)]
        [InlineData(15, 10)]
        public void LoadPagingTest(int numberToCreate, int numberPerPage)
        {
            for (var i = 0; i < numberToCreate; i++)
            {
                var package = Factories.PackageFactory.Create(msgs, dc);
            }

            Assert.Equal(
                Math.Min(numberPerPage, numberToCreate),
                Package.Load(null, 0, numberPerPage, msgs, dc).Count
            );
        }
        
        [Theory]
        [InlineData(8)]
        [InlineData(2)]
        public void GetCountTest(int numberToCreate)
        {
            for (var i = 0; i < numberToCreate; i++)
            {
                var package = Factories.PackageFactory.Create(msgs, dc);
            }

            Assert.Equal(numberToCreate, Package.GetCount(msgs: msgs, dc: dc));
        }

        [Fact]
        public void SaveTest()
        {
            var package = Factories.PackageFactory.Create(msgs, dc);

            package.LastModifiedBy = 1;
            msgs.AddRange(package.Save(dc));

            Assert.True(msgs.HasNoErrors());
        }
        [Fact]
        public void DeleteTest()
        {
            var package = Factories.PackageFactory.Create(msgs, dc);

            msgs.AddRange(package.Delete(dc));

            Assert.Equal(0, Package.GetCount(msgs: msgs, dc: dc));
        }
        [Fact]
        public void ReloadTest()
        {
            var package = Factories.PackageFactory.Create(msgs, dc);

            Assert.Equal(package.ID, package.Reload(msgs, dc).ID);
        }
        
        [Fact()] // Skip = "Don't want to create stripe packages all the time"
        public void SaveWithStripeTest()
        {
            var package = Factories.PackageFactory.Create(0, 0, msgs, dc);

            msgs.AddRange(package.SaveWithStripe(dc));

            Assert.True(msgs.HasNoErrors());
        }
    }
}
