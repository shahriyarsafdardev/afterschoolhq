using System;
using System.Web.Mvc;
using System.Net;
using System.IO;
using System.Web.Script.Serialization;
using BasicLibrary;
using AfterSchool.Models;
using System.Linq;
using RestSharp;
using Newtonsoft.Json;
using AfterSchoolLibrary.Helpers;
using Clever.Models;

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

        [HttpGet]
        public ActionResult Clever(string code, string scope)
        {
            var msgs = new SystemMessages();

            var client = new RestSharp.RestClient(CleverHelper.TokenRetrieveUrl);
            client.Timeout = -1;

            var request = new RestRequest(Method.POST);
            request.AddHeader("Authorization", $"Basic " + CleverHelper.BasicAuthorizationToken);
            request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
            request.AddParameter("code", code);
            request.AddParameter("redirect_uri", CleverHelper.RedirectUrl);
            request.AddParameter("grant_type", "authorization_code");

            var response = client.Execute(request);
            var tokenDto = JsonConvert.DeserializeObject<CleverTokenDto>(response.Content);

            GetCleverUserDetails(token: tokenDto.access_token, msgs: msgs);

            // store temp msgs
            SetTempMessage(msgs: msgs);

            if (msgs.HasErrors())
                AfterSchoolLibrary.Helpers.Utilities.SystemErrorLog(msgs); // log all error msgs 

            return RedirectToAction("edit", "user", new { area = "org" });
        }

        public void GetCleverUserDetails(string token, SystemMessages msgs = null)
        {
            var client = new RestSharp.RestClient(CleverHelper.UserIdentityUrl);
            client.Timeout = -1;
            var request = new RestRequest(Method.GET);
            request.AddHeader("Authorization", "Bearer " + token);
            var response = client.Execute(request);
            Console.WriteLine(response.Content);

            var cleverUser = JsonConvert.DeserializeObject<CleverUserDto>(response.Content);

            SetCleverIdentityForAHQUser(id: cleverUser.data.id, token: token, msgs: msgs);
        }

        public void SetCleverIdentityForAHQUser(string id, string token, SystemMessages msgs = null)
        {
            var client = new RestSharp.RestClient(CleverHelper.GetUsersDetailUrl(Id: id));
            client.Timeout = -1;

            var request = new RestRequest(Method.GET);
            request.AddHeader("Authorization", "Bearer " + token);

            var response = client.Execute(request);
            var cleverUserDistrictDto = JsonConvert.DeserializeObject<CleverUserDistrictDto>(response.Content);

            var webUser = SessionVariables.CurrentUser;
            if (webUser.IsNotEmpty() &&
                cleverUserDistrictDto.IsNotEmpty() &&
                cleverUserDistrictDto.data.IsNotEmpty() &&
                cleverUserDistrictDto.data.id.IsNotEmpty())
            {
                if (webUser.CleverUserId == cleverUserDistrictDto.data.id)
                    msgs.Add("This user is already associated with clever.", MessageType.Success);
                else
                {
                    webUser.CleverUserId = cleverUserDistrictDto.data.id;
                    msgs.AddRange(webUser.Save());

                    if (msgs.HasNoErrors())
                        msgs.Add("Clever integration for this user has been successfully completed.", MessageType.Success);
                }                
            }
        }
    }
}
