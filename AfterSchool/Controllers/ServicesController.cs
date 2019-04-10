using System;
using System.Linq;
using System.Web.Mvc;
using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Helpers;
using BasicLibrary;
using AfterSchool.Models;
using AfterSchool.Helpers;
using AfterSchoolLibrary.Services;

namespace AfterSchool.Controllers
{
    public class ServicesController : BaseController
    {
        // GET: Services

        private bool IsRequestFromApp()
        {
            var origin = (Request.Headers["Origin"] ?? Request.Headers["Referer"]).ToLower();
            if (origin.StartsWith("https://afterschoolhq.com") ||
                origin.StartsWith("https://qa.afterschoolhq.com") ||
                origin.StartsWith("https://ci.afterschoolhq.com") ||
                origin.StartsWith("https://stating.afterschoolhq.com") ||
                origin.StartsWith("http://localhost:54849"))
            {
                return true;
            }

            return false;
        }

        public ActionResult SetDemoOrgName(string orgname, string firstname, string email)
        {
            var msgs = new SystemMessages();

            if (orgname.IsBlank())
                msgs.Add("Organization / Program name is required", MessageType.UserError);

            if (firstname.IsBlank())
                msgs.Add("First Name is required", MessageType.UserError);

            if (!email.IsEmail())
                msgs.Add("Email is required", MessageType.UserError);

            if (msgs.HasNoErrors())
            {
                var uniqueName = AccountService.GenerateUniqueName(orgname, null, msgs);
                CookieVariables.DemoOrgName = orgname;
                CookieVariables.DemoUniqueOrgName = uniqueName;
                CookieVariables.DemoFirstName = firstname;
                CookieVariables.DemoEmailAddress = email;

                return Json(new { status = "ok", demoUrl = $"/{uniqueName}" }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { status = "error", messages = msgs }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult HasDemoOrgName()
        {
            return Json(new { status = "ok", hasDemoOrgName = CookieVariables.DemoOrgName.IsNotBlank() }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult LeadForm(string org, string name, string email, string phone, string comments)
        {
            if (!IsRequestFromApp())
                return Json(new { status = "error", messages = new SystemMessages() }, JsonRequestBehavior.AllowGet);

            var msgs = new SystemMessages();

            if (org.IsEmpty())
                msgs.Add("Invalid Request", MessageType.UserError);

            if (name.IsEmpty())
                msgs.Add("Name is required", MessageType.UserError);

            if (!email.IsEmail())
                msgs.Add("Email is required", MessageType.UserError);

            if (!phone.IsPhone())
                msgs.Add("Valid phone is required", MessageType.UserError);
            
            if (msgs.HasNoErrors())
            {
                EmailHelper.SendEmail(
                    "sales@afterschoolhq.com",
                    $"AHQ Lead Form - {org}",
                    $"<p>Organization: {org}<br />Name: {name}<br />Email: {email}<br />Phone: {phone}<br />Best time to contact: {comments}</p>",
                    true);

                return Json(new { status = "ok" }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { status = "error", messages = msgs }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SearchLocations(string search)
        {
            if (!IsRequestFromApp())
                return Json(new { status = "error", messages = new SystemMessages() }, JsonRequestBehavior.AllowGet);

            var msgs = new SystemMessages();
            var locations = Location.Search(search.Trim(), 0, 10, msgs);

            return Json(new { 
                status = "ok",
                locations = locations.Select(location => new
                {
                    location.ID,
                    location.Name,
                    location.Address1,
                    location.Address2,
                    location.City,
                    location.State,
                    location.PostalCode,
                    FullAddress = location.FormatAddress(useLineBreaks: false),
                    location.HostAccountId,
                    LocationType = location.LocationTypeId.ToString()
                })
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SearchProviders(string search)
        {
            if (!IsRequestFromApp())
                return Json(new { status = "error", messages = new SystemMessages() }, JsonRequestBehavior.AllowGet);

            var msgs = new SystemMessages();
            var accounts = Account.FullTextSearch(search.Trim(), 0, 10, msgs);

            return Json(new
            {
                status = "ok",
                providers = accounts.Select(account => new
                {
                    account.ID,
                    account.Name,
                    account.Address1,
                    account.Address2,
                    account.City,
                    account.State,
                    account.PostalCode,
                    FullLabel = account.FormatAddress(useLineBreaks: false)
                })
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ValidateCoupon(string couponcode, string paymentoption)
        {
            if (!IsRequestFromApp())
                return Json(new { status = "error", messages = new SystemMessages() }, JsonRequestBehavior.AllowGet);

            var msgs = new SystemMessages();
            SubscriptionPlan paymentPlan = null;

            var participationObject = SessionVariables.Cart.GetCartItems(msgs)
                .FirstOrDefault()?
                .GetParticipationObject(msgs);

            if (paymentoption.IsNotEmpty() && paymentoption.IsNumber())
                paymentPlan = SubscriptionPlan.Load(paymentoption.ToInteger(), msgs);
            SubscriptionPlan subscriptionPlan = participationObject.SubscriptionPlans.FirstOrDefault();

            var coupon = SessionVariables.CurrentAccount.ValidateCoupon(participationObject, couponcode, msgs);
            decimal amount = participationObject.Cost.Value;
            decimal subscriptionPlanAmount = 0;
            int numberInMonths = 0;

            if (paymentPlan.IsNotEmpty())
            {
                amount = paymentPlan.CostPerPeriod.Value;
                numberInMonths = paymentPlan.TotalPeriods.Value;
            }

            if (subscriptionPlan.IsNotEmpty())
                subscriptionPlanAmount = subscriptionPlan.CostPerPeriod.Value;

            if (msgs.HasNoErrors())
                return Json(new { status = "ok", coupon, amount, numberInMonths, subscriptionPlanAmount }, JsonRequestBehavior.AllowGet);

            return Json(new { status = "error", messages = msgs, amount, subscriptionPlanAmount }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SubmitContactForm(string email, string subject, string message, string g_recaptcha_response)
        {
            if (!IsRequestFromApp())
                return Json(new { status = "error", messages = new SystemMessages() }, JsonRequestBehavior.AllowGet);

            var msgs = new SystemMessages();

            var status = "";

            if (!email.IsMatch(BasicLibrary.Utilities.EmailRegEx))
                msgs.Add("Please enter an email", MessageType.UserError);

            if (subject.IsBlank())
                msgs.Add("Please enter a subject", MessageType.UserError);

            if (message.IsBlank())
                msgs.Add("Please enter a message", MessageType.UserError);

            msgs.AddRange(RecaptchaHelper.Validate(g_recaptcha_response));

            if (msgs.HasNoErrors())
                msgs.AddRange(EmailHelper.SendEmail(
                    toEmail: "sales@afterschoolhq.com",
                    subject: string.Format("WEBSITE FORM: {0}", subject),
                    body: string.Format("<p>from: <a href=\"mailto:{0}\">{0}</a></p><p>{1}</p>", email, message),
                    isAfterSchool: true
                    ));

            status = msgs.HasNoErrors() ? "ok" : "error";

            return Json(new { status, messages = msgs }, JsonRequestBehavior.AllowGet);
        }    
    }
}
