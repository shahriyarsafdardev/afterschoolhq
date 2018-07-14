using System;
using Xunit;
using Xunit.Abstractions;
using AfterSchoolLibrary;
using BasicLibrary;
using AfterSchoolLibraryTests.Factories;

namespace AfterSchoolLibraryTests.Tests
{
    public class RegistrationPeriodTest : BaseTest
    {
        private WebUser AccountOwner { get; set; }
        private Account PeriodOwnerAccount { get; set; }

        public RegistrationPeriodTest(ITestOutputHelper output) : base(output)
        {
            AccountOwner = WebUserFactory.Create(msgs, dc);
            PeriodOwnerAccount = AccountFactory.Create(AccountOwner, msgs, dc);
        }

        [Fact]
        public void LoadTest()
        {
            var period = RegistrationPeriodFactory.Create(PeriodOwnerAccount, msgs, dc);

            Assert.True(RegistrationPeriod.Load(period.ID.Value, msgs, dc).IsNotEmpty());
        }

        [Theory]
        [InlineData(8, 10)]
        [InlineData(15, 10)]
        public void LoadPagingTest(int numberToCreate, int numberPerPage)
        {
            for (var i = 0; i < numberToCreate; i++)
            {
                var period = RegistrationPeriodFactory.Create(PeriodOwnerAccount, msgs, dc);
            }

            Assert.Equal(
                Math.Min(numberPerPage, numberToCreate),
                RegistrationPeriod.Load(null, 0, numberPerPage, msgs, dc).Count
            );
        }

        [Theory]
        [InlineData(8)]
        [InlineData(2)]
        public void GetCountTest(int numberToCreate)
        {
            for (var i = 0; i < numberToCreate; i++)
            {
                var period = RegistrationPeriodFactory.Create(PeriodOwnerAccount, msgs, dc);
            }

            Assert.Equal(numberToCreate, RegistrationPeriod.GetCount(msgs: msgs, dc: dc));
        }

        [Fact]
        public void SaveTest()
        {
            var period = RegistrationPeriodFactory.Create(PeriodOwnerAccount, msgs, dc);

            period.LastModifiedBy = 1;
            msgs.AddRange(period.Save(dc));

            Assert.True(msgs.HasNoErrors());
        }
        [Fact]
        public void DeleteTest()
        {
            var period = RegistrationPeriodFactory.Create(PeriodOwnerAccount, msgs, dc);

            msgs.AddRange(period.Delete(dc));

            Assert.Equal(0, RegistrationPeriod.GetCount(msgs: msgs, dc: dc));
        }
        [Fact]
        public void ReloadTest()
        {
            var period = RegistrationPeriodFactory.Create(PeriodOwnerAccount, msgs, dc);

            Assert.Equal(period.ID, period.Reload(msgs, dc).ID);
        }

        [Fact]
        public void IsOpenTestPast()
        {
            var startDate = DateTime.UtcNow.AddWeeks(-2);
            var endDate = DateTime.UtcNow.AddWeeks(-1);
            var period = RegistrationPeriodFactory.Create(PeriodOwnerAccount, startDate, endDate, msgs, dc);

            Assert.False(period.IsOpen(msgs, dc));
        }

        [Fact]
        public void IsOpenTestPresent()
        {
            var startDate = DateTime.UtcNow.AddWeeks(-2);
            var endDate = DateTime.UtcNow.AddWeeks(1);
            var period = RegistrationPeriodFactory.Create(PeriodOwnerAccount, startDate, endDate, msgs, dc);

            Assert.True(period.IsOpen(msgs, dc));
        }

        [Fact]
        public void IsOpenTestFuture()
        {
            var startDate = DateTime.UtcNow.AddWeeks(1);
            var endDate = DateTime.UtcNow.AddWeeks(2);
            var period = RegistrationPeriodFactory.Create(PeriodOwnerAccount, startDate, endDate, msgs, dc);

            Assert.False(period.IsOpen(msgs, dc));
        }

        [Fact]
        public void IsClosedTestPast()
        {
            var startDate = DateTime.UtcNow.AddWeeks(-2);
            var endDate = DateTime.UtcNow.AddWeeks(-1);
            var period = RegistrationPeriodFactory.Create(PeriodOwnerAccount, startDate, endDate, msgs, dc);

            Assert.True(period.IsClosed(msgs, dc));
        }

        [Fact]
        public void IsClosedTestPresent()
        {
            var startDate = DateTime.UtcNow.AddWeeks(-2);
            var endDate = DateTime.UtcNow.AddWeeks(1);
            var period = RegistrationPeriodFactory.Create(PeriodOwnerAccount, startDate, endDate, msgs, dc);

            Assert.False(period.IsClosed(msgs, dc));
        }

        [Fact]
        public void IsClosedTestFuture()
        {
            var startDate = DateTime.UtcNow.AddWeeks(1);
            var endDate = DateTime.UtcNow.AddWeeks(2);
            var period = RegistrationPeriodFactory.Create(PeriodOwnerAccount, startDate, endDate, msgs, dc);

            Assert.True(period.IsClosed(msgs, dc));
        }

        [Fact]
        public void WillOpenTestPast()
        {
            var startDate = DateTime.UtcNow.AddWeeks(-2);
            var endDate = DateTime.UtcNow.AddWeeks(-1);
            var period = RegistrationPeriodFactory.Create(PeriodOwnerAccount, startDate, endDate, msgs, dc);

            Assert.False(period.WillOpen(msgs, dc));
        }

        [Fact]
        public void WillOpenTestPresent()
        {
            var startDate = DateTime.UtcNow.AddWeeks(-2);
            var endDate = DateTime.UtcNow.AddWeeks(1);
            var period = RegistrationPeriodFactory.Create(PeriodOwnerAccount, startDate, endDate, msgs, dc);

            Assert.False(period.WillOpen(msgs, dc));
        }

        [Fact]
        public void WillOpenTestFuture()
        {
            var startDate = DateTime.UtcNow.AddWeeks(1);
            var endDate = DateTime.UtcNow.AddWeeks(2);
            var period = RegistrationPeriodFactory.Create(PeriodOwnerAccount, startDate, endDate, msgs, dc);

            Assert.True(period.WillOpen(msgs, dc));
        }

    }
}
