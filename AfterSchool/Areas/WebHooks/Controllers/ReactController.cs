using AfterSchoolLibrary.Billing;
using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using BasicLibrary;
using Newtonsoft.Json;
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using AfterSchool.Models;
using Amazon.DynamoDBv2.Model;

namespace AfterSchool.Areas.WebHooks.Controllers
{
    public class ReactController : ApiController
    {
        [HttpGet]
        public IHttpActionResult GetCurrentUser()
        {
            var msgs = new SystemMessages();
            var getCurrentUserUid = GetCookie("CurrentUser")?.Value;

            if (getCurrentUserUid.IsEmpty())
            {
                msgs.Add("User not found" , MessageType.ApiError);
                return Ok(new ApiResponse(msgs, HttpStatusCode.NotFound, null).ToJSON());
            }

            var CurrentUser = WebUser.LoadByUid(getCurrentUserUid.ToGuid(), msgs);
            

            return Ok(new ApiResponse(msgs, HttpStatusCode.OK, CurrentUser).ToJSON());
        }

        private CookieState GetCookie(string name)
        {
            return Request.Headers.GetCookies().FirstOrDefault().Cookies.Where(x => x.Name == name).FirstOrDefault();
        }

        public class ApiResponse
        {
            public SystemMessages msgs { get; set; }
            public HttpStatusCode code { get; set; }
            public dynamic obj { get; set; }

            public ApiResponse(SystemMessages msgs, HttpStatusCode code, dynamic obj)
            {
                this.msgs = msgs;
                this.code = code;
                this.obj = obj;
            }
            
        }
    }
}