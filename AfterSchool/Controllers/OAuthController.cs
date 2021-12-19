using System;
using System.Web.Mvc;
using System.Net;
using System.IO;
using System.Web.Script.Serialization;
using BasicLibrary;
using AfterSchool.Models;
using System.Linq;
using AfterSchoolLibrary.Helpers;
using AfterSchoolLibrary.Services;

namespace AfterSchool.Controllers
{
    public class OAuthController : BaseController
    {
        public OAuthController()
        {

        }

        // GET: /Stripe/
        public ActionResult Stripe(string id)
        {
            SystemMessages msgs = new SystemMessages();

            if (Request["error"].IsEmpty())
            {
                try
                {
                    string scope = Request["scope"];
                    string code = Request["code"];
                    string state = Request["state"];

                    string postVariables = "client_secret=" + AfterSchoolLibrary.Billing.StripeHelper.SecretKey;
                    postVariables += "&code=" + code;
                    postVariables += "&grant_type=authorization_code";
                    postVariables += "&client_id=" + AfterSchoolLibrary.Billing.StripeHelper.PlatformClientId;

                    WebRequest request = WebRequest.Create("https://connect.stripe.com/oauth/token");
                    request.Method = "POST";
                    byte[] byteArray = System.Text.Encoding.UTF8.GetBytes(postVariables);
                    request.ContentType = "application/x-www-form-urlencoded";
                    request.ContentLength = byteArray.Length;
                    Stream dataStream = request.GetRequestStream();
                    dataStream.Write(byteArray, 0, byteArray.Length);
                    dataStream.Close();
                    WebResponse response = request.GetResponse();
                    dataStream = response.GetResponseStream();
                    StreamReader reader = new StreamReader(dataStream);
                    string responseFromServer = reader.ReadToEnd();
                    reader.Close();
                    dataStream.Close();
                    response.Close();

                    JavaScriptSerializer jsserializer = new JavaScriptSerializer();
                    dynamic results = jsserializer.DeserializeObject(responseFromServer);

                    string token_type = results["token_type"];
                    string stripe_publishable_key = results["stripe_publishable_key"];
                    bool livemode = results["livemode"];
                    string stripe_user_id = results["stripe_user_id"];
                    string refresh_token = results["refresh_token"];
                    string access_token = results["access_token"];
                    scope = results["scope"];

                    ViewData["publishablekey"] = stripe_publishable_key;
                    ViewData["secretkey"] = access_token;
                    ViewData["accesstoken"] = access_token;
                }
                catch (Exception ex)
                {
                    msgs.Add("An error occurred while connecting your Stripe account. Please contact <a href=\"mailto:support@BasicFramework.com\">support@BasicFramework.com</a>", MessageType.Error, ex);
                }
            }
            else
            {
                //Request["error"] + " - " + Request["error_description"]
                msgs.Add("It appears you have canceled your request to connect your Stripe account.", MessageType.UserError);
            }

            ViewData["msgs"] = msgs;

            return View();
        }

        #region Clever Integration

        [HttpGet]
        public ActionResult CleverIntegration(string code, string scope)
        {
            var msgs = new SystemMessages();

            var token = CleverIntegrationService.GetToken(code: code, redirectUrl: CleverHelper.RedirectUrlForIntegration);
            var userIdentity = CleverIntegrationService.GetCleverUserIdentity(token: token.access_token);

            if (userIdentity.IsNotEmpty() && userIdentity.data.IsNotEmpty() && userIdentity.data.id.IsNotEmpty())
                WebUserService.SetCleverIdentityToUser(id: userIdentity.data.id, webUser: SessionVariables.CurrentUser, msgs: msgs);

            LoadTempMsgAndLogErrors(msgs: msgs);
            return RedirectToAction("edit", "user", new { area = "org" });
        }

        [HttpGet]
        public ActionResult CleverLogin(string code, string scope)
        {
            var msgs = new SystemMessages();

            var token = CleverIntegrationService.GetToken(code: code, redirectUrl: CleverHelper.RedirectUrlForLogin);
            var userIdentity = CleverIntegrationService.GetCleverUserIdentity(token: token.access_token);

            if (userIdentity.IsNotEmpty() && userIdentity.data.IsNotEmpty() && userIdentity.data.id.IsNotEmpty())
            {
                // get ahq user for this clever id
                var user = WebUserService.GetUserByCleverId(id: userIdentity.data.id, msgs: msgs);

                if (user.IsNotEmpty())
                    return RedirectToAction("loginUser", "login", new { area = "", id = user.ID.ToString() });
                else
                    msgs.Add("We didn't find any user in our database for this clever user.", MessageType.Error);
            }

            LoadTempMsgAndLogErrors(msgs: msgs);            
            return RedirectToAction("index", "login", new { area = "" });
        }

        #endregion
    }
}
