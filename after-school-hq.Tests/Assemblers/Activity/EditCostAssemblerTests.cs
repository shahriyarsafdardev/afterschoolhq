using AfterSchool.Areas.Org.Assemblers.Activity;
using AfterSchoolLibrary.Models;
using BasicLibrary;
using NUnit.Framework;

namespace after_school_hq.Tests.Assemblers.Activity
{
    public class EditCostAssemblerTests
    {
        [Test]
        public void EditCostAssemblerTest()
        {
            var assembler = new EditCostAssembler();
            var activity = new AfterSchoolLibrary.Models.Activity {AccountId = 3};
            var msgs = new SystemMessages();
            var accountPackage = new AccountPackage();
            const string fullScreenExitUrl = "";
            var subscriptionPlan = new SubscriptionPlan();
            var paymentPlan = new SubscriptionPlan();
            var postPayPlan = new SubscriptionPlan();
            var result = assembler.AssembleEditCostResult(activity, 3, UserType.NotSet, true, "1", fullScreenExitUrl, msgs, subscriptionPlan, paymentPlan,
                postPayPlan, accountPackage);

            Assert.That(result.Activity, Is.SameAs(activity));
            Assert.That(result.Messages, Is.SameAs(msgs));
            Assert.That(result.AccountPackage, Is.SameAs(accountPackage));
            Assert.That(result.IsActivityLeader, Is.True);
            Assert.That(result.IsFullScreen, Is.True);
            Assert.That(result.FullScreenExitUrl, Is.EqualTo(fullScreenExitUrl));
        }
    }
}