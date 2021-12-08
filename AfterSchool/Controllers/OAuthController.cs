using System;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Net;
using System.IO;
using System.Web.Script.Serialization;
using BasicLibrary;
using AfterSchool.Models;
using System.Linq;
using RestSharp;
using System.Text;
using Newtonsoft.Json;
using AfterSchoolLibrary.Services;

namespace AfterSchool.Controllers
{
    public class OAuthController : BaseLoggedInController
    {
        public OAuthController() : base(AccountService.CustomerAccountTypes.ToList())
        {

        }

        //
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
            var client = new RestSharp.RestClient("https://clever.com/oauth/tokens");
            client.Timeout = -1;

            var request = new RestRequest(Method.POST);
            var auth = EncodeBase64("6909bfddb2f7cc535ed2:2e6c28be090b924a893a00763d6d38ffab77de19"); //client_id + ":" client_secret
            request.AddHeader("Authorization", $"Basic " + auth);
            request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
            request.AddParameter("code", code);
            request.AddParameter("redirect_uri", "http://localhost:54849/oauth/clever");
            request.AddParameter("grant_type", "authorization_code");

            var response = client.Execute(request);
            var tokenDto = JsonConvert.DeserializeObject<CleverTokenDto>(response.Content);
            GetCleverUserDetails(token: tokenDto.access_token);

            return View();
        }

        public void GetCleverUserDetails(string token)
        {
            var client = new RestSharp.RestClient("https://api.clever.com/me");
            client.Timeout = -1;
            var request = new RestRequest(Method.GET);
            request.AddHeader("Authorization", "Bearer " + token);
            var response = client.Execute(request);
            Console.WriteLine(response.Content);

            var cleverUser = JsonConvert.DeserializeObject<CleverUserDto>(response.Content);
        }

        public static string EncodeBase64(string value)
        {
            var valueBytes = Encoding.UTF8.GetBytes(value);
            return Convert.ToBase64String(valueBytes);
        }

        public static string DecodeBase64(string value)
        {
            var valueBytes = System.Convert.FromBase64String(value);
            return Encoding.UTF8.GetString(valueBytes);
        }

        public class CleverTokenDto
        {
            public string access_token { get; set; }
            public string token_type { get; set; }
        }

        public class CleverUser
        {
            public string first { get; set; }
            public string last { get; set; }
        }

        public class CleverUserData
        {
            public string id { get; set; }
            public string type { get; set; }
            public string email { get; set; }
            public CleverUser name { get; set; }
            public List<string> sections { get; set; }
        }

        public class CleverUserDto
        {
            public string type { get; set; }
            public CleverUserData data { get; set; }
        }
    }
}
