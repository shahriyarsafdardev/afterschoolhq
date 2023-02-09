using AfterSchoolLibrary.Models;
using BasicLibrary;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Web.Http;
using Newtonsoft.Json;
using RestSharp;
using AfterSchoolLibrary.Services;
using Account = Stripe.Account;

namespace AfterSchool.Areas.WebHooks.Controllers
{
    public class ReactController : ApiController
    {
        [HttpGet]
        public HttpResponseMessage GetCurrentUser()
        {
            var msgs = new SystemMessages();
            try
            {
                var currentUser = CurrentUser(msgs);

                var obj = new ApiResponse(msgs , new ApiUser(currentUser.FirstName, currentUser.LastName, currentUser.Email, currentUser.Phone, currentUser.Username));
                
                var response = Request.CreateResponse(HttpStatusCode.OK);
                response.Content = new StringContent((new ApiResponse(msgs , new ApiUser(currentUser.FirstName, currentUser.LastName, currentUser.Email, currentUser.Phone, currentUser.Username))).ToJSON(), System.Text.Encoding.UTF8, "application/json");
                return response;
            }
            catch (Exception ex)
            {
                msgs.Add(ex.Message, MessageType.ApiError);
                
                var response = Request.CreateResponse(HttpStatusCode.NotFound);
                response.Content = new StringContent((new ApiResponse(msgs, null)).ToJSON(), System.Text.Encoding.UTF8, "application/json");
                return response;
            }
        }

        private WebUser CurrentUser(SystemMessages msgs)
        {
            var currentUserUid = Request.Headers.GetCookies().FirstOrDefault().Cookies.Where(x => x.Name == "CurrentUser").FirstOrDefault();

            if (currentUserUid.IsEmpty())
                throw new Exception("Session not found");

            msgs.Add("User Found", MessageType.Success);
            return WebUser.LoadByUid(currentUserUid.Value.ToGuid(), msgs);
        }

        public class ApiResponse
        {
            public SystemMessages msgs { get; set; }
            public dynamic obj { get; set; }

            public ApiResponse(SystemMessages msgs, dynamic obj)
            {
                this.msgs = msgs;
                this.obj = obj;
            }
        }

        public class ApiUser
        {
            public string firstName { get; set; }
            public string lastName { get; set; }
            public string email { get; set; }
            public string phone { get; set; }
            public string userName { get; set; }

            public ApiUser(string firstName, string lastName, string email, string phone, string userName)
            {
                this.firstName = firstName;
                this.lastName = lastName;
                this.email = email;
                this.phone = phone;
                this.userName = userName;
            }
            
        }
        
    }
}