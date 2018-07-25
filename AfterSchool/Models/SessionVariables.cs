using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AfterSchoolLibrary;
using BasicLibrary;

namespace AfterSchool.Models
{
    public class SessionVariables
    {
        public static Registrant CurrentRegistrant
        {
            get
            {
                return (Registrant)HttpContext.Current.Session["CurrentRegistrant"];
            }
            set
            {
                HttpContext.Current.Session["CurrentRegistrant"] = value;
            }
        }
        public static WebUser LoggedInUser
        {
            get
            {
                return (WebUser)HttpContext.Current.Session["LoggedInUser"];
            }
            set
            {
                HttpContext.Current.Session["LoggedInUser"] = value;
            }
        }
        public static WebUser OriginalLoggedInUser
        {
            get
            {
                return (WebUser)HttpContext.Current.Session["OriginalLoggedInUser"];
            }
            set
            {
                HttpContext.Current.Session["OriginalLoggedInUser"] = value;
            }
        }
        public static Account CurrentAccount
        {
            get
            {
                return (Account)HttpContext.Current.Session["CurrentAccount"];
            }
            set
            {
                HttpContext.Current.Session["CurrentAccount"] = value;
            }
        }
        public static Account RegistrationStartPageAccount
        {
            get
            {
                return (Account)HttpContext.Current.Session["RegistrationStartPageAccount"];
            }
            set
            {
                HttpContext.Current.Session["RegistrationStartPageAccount"] = value;
            }
        }
        public static UserType AccountUserType
        {
            get
            {
                return (UserType)HttpContext.Current.Session["AccountUserType"];
            }
            set
            {
                HttpContext.Current.Session["AccountUserType"] = value;
            }
        }
        public static AccountPackage AccountPackage
        {
            get
            {
                return (AccountPackage)HttpContext.Current.Session["AccountPackage"];
            }
            set
            {
                HttpContext.Current.Session["AccountPackage"] = value;
            }
        }
        public static Session VisitorSession
        {
            get
            {
                return (Session)HttpContext.Current.Session["VisitorSession"];
            }
            set
            {
                HttpContext.Current.Session["VisitorSession"] = value;
            }
        }

        public static Cart Cart
        {
            get
            {
                return (Cart)HttpContext.Current.Session["Cart"];
            }
            set
            {
                HttpContext.Current.Session["Cart"] = value;
            }
        }

        public static bool? IsAfterSchool
        {
            get
            {
                if (HttpContext.Current.Session["IsAfterSchool"].IsEmpty())
                {
                    HttpContext.Current.Session["IsAfterSchool"] = HttpContext.Current.Request.Url.Host.Contains("afterschool");
                }
                return (bool?)HttpContext.Current.Session["IsAfterSchool"];
            }
            set
            {
                HttpContext.Current.Session["IsAfterSchool"] = value;
            }
        }

    }
}
