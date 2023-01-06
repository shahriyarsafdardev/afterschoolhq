using System;
using System.Linq;
using System.Web.Mvc;
using System.Windows.Interop;
using AfterSchool.Models;
using AfterSchoolLibrary.Billing;
using AfterSchoolLibrary.Helpers;
using AfterSchoolLibrary.Models;
using BasicLibrary;

namespace AfterSchool.Controllers
{
    public class DbController : BaseController
    {
        private readonly DatabaseCredentials databaseCredentials;

        public DbController(DatabaseCredentials databaseCredentials)
        {
            this.databaseCredentials = databaseCredentials;
        }

        [HttpGet]
        public ActionResult Update(string id)
        {
            if (Request.UserHostAddress != "::1" && id !=
                "eibfeiwalncdsja32rq9yfwbfg32yewody7832re7ywofaudisfh7238qoweiafb7328qwoadg78027rweubfad")
                return Json(new {status = "ok"}, JsonRequestBehavior.AllowGet);

            var msgs = new SystemMessages();

            msgs.AddRange(DatabaseInit.InitDatabase(databaseCredentials, "darye@afterschoolhq.com",
                "help students learn", "317-316-0596", false, "AfterSchool HQ"));

            return Json(new {status = "ok", messages = msgs.ToList().Select(x => new {x.MessageType, x.Text})},
                JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult Stripe()
        {
            var msgs = new SystemMessages();
            var stripeHelper = StripeHelper.GetStripeHelperConnectAccount();

            var customerId = "cus_N7GrW50fBNFg33";
            var planId = "price_1MN2jXCEgXQRZLhkSGdAPNdQ"; // https://dashboard.stripe.com/test/prices/price_1MN2jXCEgXQRZLhkSGdAPNdQ
            var cardId = "card_1MN2KcCEgXQRZLhkYOQgtEa9";
            var conveniencePercentFee = .05M;

            // example assumes that the date the subscription should start is Jan 1st
            // therefore the anchor date is one month after today
            // will start on activity start
            var subscriptionStoredCost = 103.20M;
            var activityStartDate = "2022/12/30".ToDateTime();
            var activityEndDate = "2023/05/22".ToDateTime();
            var lengthOfSubscription = 4; // 4 months
            var checkoutTime = DateTime.UtcNow; // stripe uses the exact time to calculate prorating
            var checkoutDate = checkoutTime.Date; // the date the user is checking out to easily calculate the anchor date later
            var dayDiff = (checkoutDate - activityStartDate).Days; // difference between the days of the month, for instance if it is the 5th, and the start date is the 1st, then this should be 4, no matter what month it is.
            var anchorDate = checkoutDate.AddMonths(1).AddDays(dayDiff * -1); // anchor date has to be in the future. It should be on the same day as the period start or the activity start
            var startDate = DateTime.UtcNow.AddSeconds(1); // start the subscription right now
            var endDate = anchorDate.AddMonths(lengthOfSubscription); // end the subscription exactly x months after activity start date (if it needs to end)
            var predictedStripeCharge = StripeHelper.CalculateStripeProratedCharge(anchorDate, checkoutTime, subscriptionStoredCost); // use this instead of the stripe prorate preview
            var ccFeeForProratedCharge = StripeHelper.CalculateStripeCCFeeOutOfTotalCost(predictedStripeCharge); // get our estimated CC fee (it will be wrong for a few reasons, we can discuss)
            var listPrice = predictedStripeCharge - ccFeeForProratedCharge; //how much is the "original" charge before cc fee

            var feeCalcObj = new FeeCalculationObject(listPrice, 0M, conveniencePercentFee, true);
            var convenienceFee = feeCalcObj.GetConvenienceFee(); // let's get the convenience fee
            var providerAmount = predictedStripeCharge - ccFeeForProratedCharge - convenienceFee; // what should the provider get
            var achCouponAmount = ccFeeForProratedCharge; // how much should the ACH coupon be for

            stripeHelper.CreateRegistrantSubscription(
                customerId,
                planId,
                true,
                cardId,
                conveniencePercentFee,
                startDate,
                endDate,
                anchorDate,
                null,
                null,
                null,
                msgs);

            return Json(
            new { 
                status = "ok", 
                predictedStripeCharge = predictedStripeCharge.ToCurrency(),
                ccFeeForProratedCharge = ccFeeForProratedCharge.ToCurrency(),
                listPrice = listPrice.ToCurrency(),
                convenienceFee = convenienceFee.ToCurrency(),
                providerAmount = providerAmount.ToCurrency(),
                achCouponAmount= achCouponAmount.ToCurrency(),
                messages = msgs.ToList().Select(x => new { x.MessageType, x.Text }) },
                JsonRequestBehavior.AllowGet);
        }
    }
}