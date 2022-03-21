using AfterSchoolLibrary.Services;
using NUnit.Framework;

namespace AfterSchoolLibrary.Tests
{
    public class AccountServiceTests
    {
        [TestCase(1.2, 3.4, "$1.20 – $3.40")]
        [TestCase(0, 3.4, "Free – $3.40")]
        [TestCase(0, 0, "Free")]
        [TestCase(10, 22.5, "$10 – $22.50")]
        [TestCase(10, 25, "$10 – $25")]
        [TestCase(9.5, 11, "$9.50 – $11")]
        [TestCase(null, null, "")]
        public void GetCostRangeStringTest(decimal? costLow, decimal? costHigh, string expected)
        {
            Assert.That(AccountService.GetCostRangeString(costLow, costHigh), Is.EqualTo(expected));
        }
    }
}