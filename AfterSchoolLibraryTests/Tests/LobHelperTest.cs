using BasicLibrary;
using AfterSchoolLibrary;
using AfterSchoolLibrary.Billing;
using Xunit;
using Xunit.Abstractions;
using AfterSchoolLibraryTests.Factories;
using System;
using AfterSchoolLibrary.Services;
using AfterSchoolLibrary.Helpers;

namespace AfterSchoolLibraryTests.Tests
{
    public class LobHelperTest : BaseTest
    {
        public LobHelperTest(ITestOutputHelper output) : base(output)
        {
        }

        private Lob.Models.Check CreateLobCheck()
        {
            return LobHelper.CreateCheck(
                description: "My test check",
                accountType: Lob.Models.AccountType.Company,
                toName: "Test Company",
                toAddress1: "12175 Visionary Way",
                toAddress2: "710",
                toCity: "Fishers",
                toState: "IN",
                toPostalCode: "46038",
                fromAddressId: "adr_7e68ac15e4ff4bf7",
                bankAccountId: "bank_a9ca0ac762c4040",
                amount: 300,
                memo: "Heres your paycheck",
                metaData: new { },
                msgs: msgs);
        }

        [Fact]
        public void CreateCheckTest()
        {
            var check = CreateLobCheck();
            Assert.NotNull(check);
        }
    }
}
