using System;
using System.Web;
using AfterSchoolLibrary.Models;
using BasicLibrary;

namespace AfterSchool.Models
{
    public static class CookieVariables
    {
        public static string DemoOrgName
        {
            get
            {
                return GetCookie("DemoOrgName");
            }
            set
            {
                SetCookie("DemoOrgName", value);

            }
        }
        public static string DemoFirstName
        {
            get
            {
                return GetCookie("DemoFirstName");
            }
            set
            {
                SetCookie("DemoFirstName", value);

            }
        }

        public static string DemoUniqueOrgName
        {
            get
            {
                return GetCookie("DemoUniqueOrgName");
            }
            set
            {
                SetCookie("DemoUniqueOrgName", value);
            }
        }

        public static string DemoEmailAddress
        {
            get
            {
                return GetCookie("DemoEmailAddress");
            }
            set
            {
                SetCookie("DemoEmailAddress", value);
            }
        }

        private static string GetCookie(string key)
        {
            if (HttpContext.Current.Request.Cookies[key].IsNotEmpty())
                return HttpContext.Current.Request.Cookies[key].Value;

            return null;
        }

        private static void SetCookie(string key, string value)
        {
            HttpContext.Current.Response.Cookies.Remove(key);

            if (value.IsNotEmpty())
            {
                var cookie = new HttpCookie(key)
                {
                    Expires = DateTime.UtcNow.AddYears(1),
                    Value = value,
                    HttpOnly = true
                };
                HttpContext.Current.Response.Cookies.Add(cookie);
            }
        }

    }
}
