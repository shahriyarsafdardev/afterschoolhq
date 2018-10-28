using System;
using System.Linq;
using System.Web.Mvc;
using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Helpers;
using BasicLibrary;
using AfterSchool.Models;
using AfterSchool.Helpers;

namespace AfterSchool.Controllers
{
    public class ServicesController : BaseController
    {
        // GET: Services

        public ActionResult SearchLocations(string search)
        {
            Response.Clear();

            if (Request.UserHostAddress == "::1" || true)
            {
                var msgs = new SystemMessages();
                var locations = Location.Search(search.Trim(), 0, 10, msgs);

                WriteOutput(new
                {
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
                },
                ResponseType.JSON);
            }
            else
            {
                WriteOutput(new { status = "ok" }, ResponseType.JSON);
            }
            Response.End();

            return View();
        }

        public ActionResult ValidateCoupon(string couponcode, decimal totalCost)
        {
            var msgs = new SystemMessages();

            if (Request.UserHostAddress == "::1" || true)
            {
                var coupon = Coupon.LoadByAccountIdByCouponCode(SessionVariables.CurrentAccount.ID.Value, couponcode, msgs);
                var status = "";

                if (coupon.IsEmpty())
                    msgs.Add("Invalid coupon", MessageType.UserError);

                status = msgs.HasNoErrors() ? "ok" : "error";

                return new JsonResult { Data = new { status, messages = msgs } };
            }

            return new JsonResult { Data = new { status = "ok" } };
        }

        public ActionResult SubmitContactForm(string email, string subject, string message, string g_recaptcha_response)
        {
            if (Request.UserHostAddress == "::1" || true)
            {
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

                return new JsonResult { Data = new { status, messages = msgs } };
            }

            return new JsonResult { Data = new { status = "ok" } };
        }   
    }
}
