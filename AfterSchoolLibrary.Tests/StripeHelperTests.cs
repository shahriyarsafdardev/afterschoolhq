using System.Collections.Generic;
using AfterSchoolLibrary.Billing;
using ApprovalTests.Combinations;
using ApprovalTests.Reporters;
using NUnit.Framework;

namespace AfterSchoolLibrary.Tests
{
    [UseReporter(typeof(DiffReporter))]
    public class StripeHelperTests
    {
        [Test]
        public void CalculateStripeAchFeeTest()
        {
            CombinationApprovals.VerifyAllCombinations((c, f) => StripeHelper.CalculateStripeAchFee(c, f), new List<decimal>
                {
                    0, 5, 13, 18.5m, 24.95m, 37, 38.5m, 44.5m, 55, 100, 134.28m, 181, 381
                },
                new List<bool>
                {
                    true,
                    false
                });
        }

        [Test]
        public void CalculateStripeCcFeeTest()
        {
            CombinationApprovals.VerifyAllCombinations((c, f) => StripeHelper.CalculateStripeFee(c, f), new List<decimal>
                {
                    0, 5, 13, 18.5m, 24.95m, 37, 38.5m, 44.5m, 55, 100, 134.28m, 181, 381
                },
                new List<bool>
                {
                    true,
                    false
                });
        }
    }
}