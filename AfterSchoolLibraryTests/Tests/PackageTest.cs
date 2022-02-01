using System;
using Xunit;
using Xunit.Abstractions;
using AfterSchoolLibrary;
using BasicLibrary;
using AfterSchoolLibrary.Models;

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

            Assert.Null(Package.Load(package.ID.Value));
        }

        [Fact]
        public void ReloadTest()
        {
            var package = Factories.PackageFactory.Create(msgs, dc);

            Assert.Equal(package.ID, package.Reload(msgs, dc).ID);
        }
        
        [Fact(Skip = "Don't want to create stripe packages all the time")]
        public void SaveWithStripeTest()
        {
            var package = Factories.PackageFactory.Create(0, 0, msgs, dc);

            msgs.AddRange(package.SaveWithStripe(dc));

            Assert.True(msgs.HasNoErrors());
        }
    }
}
