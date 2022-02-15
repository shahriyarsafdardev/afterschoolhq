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
        public void GetStripeAchFeeTest()
        {
            CombinationApprovals.VerifyAllCombinations((c, f) => StripeHelper.GetStripeAchFee(c, f), new List<decimal>
                {
                    0, 5, 13, 18.5m, 24.95m, 37, 38.5m, 44.5m, 100, 134.28m, 181, 381
                },
                new List<bool>
                {
                    true,
                    false
                });
        }

        [Test]
        public void GetStripeCcFeeTest()
        {
            CombinationApprovals.VerifyAllCombinations((c, f) => StripeHelper.GetStripeFee(c, f), new List<decimal>
                {
                    0, 5, 13, 18.5m, 24.95m, 37, 38.5m, 44.5m, 100, 134.28m, 181, 381
                },
                new List<bool>
                {
                    true,
                    false
                });
        }
    }
}