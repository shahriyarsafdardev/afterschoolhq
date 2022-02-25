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

        public static string CurrentUser
        {
            get
            {
                return GetCookie("CurrentUser");
            }
            set
            {
                SetCookie("CurrentUser", value);
            }
        }

        public static string CurrentAccount
        {
            get
            {
                return GetCookie("CurrentAccount");
            }
            set
            {
                SetCookie("CurrentAccount", value);
            }
        }

        public static string ParentAccount
        {
            get
            {
                return GetCookie("ParentAccount");
            }
            set
            {
                SetCookie("ParentAccount", value);
            }
        }

        public static string ShowISYPModal
        {
            get
            {
                return GetCookie("ShowISYPModal");
            }
            set
            {
                SetCookie("ShowISYPModal", value);
            }
        }

        public static string OnboardingStepsAcknowledged
        {
            get
            {
                return GetCookie("OnboardingStepsAcknowledged");
            }
            set
            {
                SetCookie("OnboardingStepsAcknowledged", value);
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
            if (HttpContext.Current.Request.Cookies[key].IsNotEmpty() && value == null)
            {
                var cookie = new HttpCookie(key, "")
                {
                    Expires = DateTime.UtcNow.AddDays(-1),
                    HttpOnly = true                   
                };
                HttpContext.Current.Response.Cookies.Add(cookie);
            }
            else if (value.IsNotEmpty())
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
