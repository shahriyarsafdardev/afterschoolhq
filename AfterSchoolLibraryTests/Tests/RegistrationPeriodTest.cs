using System;
using Xunit;
using Xunit.Abstractions;
using AfterSchoolLibrary.Models;
using BasicLibrary;
using AfterSchoolLibraryTests.Factories;
using AfterSchoolLibrary.Services;

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

            Assert.Null(RegistrationPeriod.Load(period.ID.Value));
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

            Assert.False(period.IsOpen(PeriodOwnerAccount, msgs, dc));
        }

        [Fact]
        public void IsOpenTestPresent()
        {
            var startDate = DateTime.UtcNow.AddWeeks(-2);
            var endDate = DateTime.UtcNow.AddWeeks(1);
            var period = RegistrationPeriodFactory.Create(PeriodOwnerAccount, startDate, endDate, msgs, dc);

            Assert.True(period.IsOpen(PeriodOwnerAccount, msgs, dc));
        }

        [Fact]
        public void IsOpenTestFuture()
        {
            var startDate = DateTime.UtcNow.AddWeeks(1);
            var endDate = DateTime.UtcNow.AddWeeks(2);
            var period = RegistrationPeriodFactory.Create(PeriodOwnerAccount, startDate, endDate, msgs, dc);

            Assert.False(period.IsOpen(PeriodOwnerAccount, msgs, dc));
        }

        [Fact]
        public void IsClosedTestPast()
        {
            var startDate = DateTime.UtcNow.AddWeeks(-2);
            var endDate = DateTime.UtcNow.AddWeeks(-1);
            var period = RegistrationPeriodFactory.Create(PeriodOwnerAccount, startDate, endDate, msgs, dc);

            Assert.True(period.IsClosed(PeriodOwnerAccount, msgs, dc));
        }

        [Fact]
        public void IsClosedTestPresent()
        {
            var startDate = DateTime.UtcNow.AddWeeks(-2);
            var endDate = DateTime.UtcNow.AddWeeks(1);
            var period = RegistrationPeriodFactory.Create(PeriodOwnerAccount, startDate, endDate, msgs, dc);

            Assert.False(period.IsClosed(PeriodOwnerAccount, msgs, dc));
        }

        [Fact]
        public void IsClosedTestFuture()
        {
            var startDate = DateTime.UtcNow.AddWeeks(1);
            var endDate = DateTime.UtcNow.AddWeeks(2);
            var period = RegistrationPeriodFactory.Create(PeriodOwnerAccount, startDate, endDate, msgs, dc);

            Assert.True(period.IsClosed(PeriodOwnerAccount, msgs, dc));
        }

        [Fact]
        public void WillOpenTestPast()
        {
            var startDate = DateTime.UtcNow.AddWeeks(-2);
            var endDate = DateTime.UtcNow.AddWeeks(-1);
            var period = RegistrationPeriodFactory.Create(PeriodOwnerAccount, startDate, endDate, msgs, dc);

            Assert.False(period.WillOpen(PeriodOwnerAccount, msgs, dc));
        }

        [Fact]
        public void WillOpenTestPresent()
        {
            var startDate = DateTime.UtcNow.AddWeeks(-2);
            var endDate = DateTime.UtcNow.AddWeeks(1);
            var period = RegistrationPeriodFactory.Create(PeriodOwnerAccount, startDate, endDate, msgs, dc);

            Assert.False(period.WillOpen(PeriodOwnerAccount, msgs, dc));
        }

        [Fact]
        public void WillOpenTestFuture()
        {
            var startDate = DateTime.UtcNow.AddWeeks(1);
            var endDate = DateTime.UtcNow.AddWeeks(2);
            var period = RegistrationPeriodFactory.Create(PeriodOwnerAccount, startDate, endDate, msgs, dc);

            Assert.True(period.WillOpen(PeriodOwnerAccount, msgs, dc));
        }

    }
}
