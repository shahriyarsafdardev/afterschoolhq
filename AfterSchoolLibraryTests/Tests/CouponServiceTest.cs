using System;
using Xunit;
using Xunit.Abstractions;
using AfterSchoolLibrary.Models;
using BasicLibrary;
using BasicLibrary.DataAccess;
using AfterSchoolLibraryTests.Factories;

namespace AfterSchoolLibraryTests.Tests
{
    public class CouponServiceTest : BaseTest
    {
        private WebUser SendingAccountOwner { get; set; }
        private Account SendingAccount { get; set; }

        public CouponServiceTest(ITestOutputHelper output) : base(output)
        {
            SendingAccountOwner = WebUserFactory.Create(msgs, dc);
            SendingAccount = AccountFactory.Create(SendingAccountOwner, msgs, dc);
        }
    }
}
