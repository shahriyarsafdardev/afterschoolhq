using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xunit;
using Xunit.Abstractions;
using AfterSchoolLibrary;
using BasicLibrary;
using BasicLibrary.DataAccess;
using AfterSchoolLibraryTests.Factories;
using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;

namespace AfterSchoolLibraryTests.Tests
{
   public class RegistrationTest : BaseTest
    {
        public RegistrationTest(ITestOutputHelper output) : base(output)
        {
        }
        [Fact]
        public void LoadByCouponIdByIsDeleted()
        {
            // Arrange
            var user = WebUserFactory.Create(msgs: msgs, dc: dc);
            var account = AccountFactory.Create(user: user, msgs: msgs, dc: dc);
            var registrant = RegistrantFactory.Create(account, isMaster: false, msgs, dc);
            var coupon = CouponFactory.Create(account, msgs, dc);
            var registration = RegistrationFactory.Create(account, registrant, coupon.ID.Value, false, msgs, dc);
            // Act                
            var registrants = Registration.LoadByCouponIdByIsDeleted(coupon.ID.Value, false, msgs: msgs, dc: dc);

            // Assert
            Assert.NotNull(registrants);
            Assert.True(registrants.Count > 0);

        }
    }
}
