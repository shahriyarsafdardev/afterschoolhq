using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using BasicLibrary;

namespace AfterSchool.Helpers
{
    public class RecaptchaHelper
    {
        public static SystemMessages Validate(string recaptchaResponse)
        {
            var msgs = new SystemMessages();
            var privateKey = "6Lfg2BEUAAAAACdoRJ8STeOzXfn7NyYIvGU1DQsP";
            var url = "https://www.google.com/recaptcha/api/siteverify";
            var rc = new RestClient();
            rc.Url = url;
            rc.Parameters.Add("secret", privateKey);
            rc.Parameters.Add("remoteip", HttpContext.Current.Request.UserHostAddress);
            rc.Parameters.Add("response", recaptchaResponse);
            rc.RequestMethod = RestClientRequestMethod.POST;
            rc.ContentType = RestClientContentTypeEnum.URLEncoded;
            string results = rc.DoRequest();
            dynamic json = results.FromJSON();

            if (json["success"] == null || !json["success"])
            {
                msgs.Add("You just may be a robot :-/", MessageType.UserError);
            }

            return msgs;
        }
    }
}