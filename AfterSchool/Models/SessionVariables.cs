using System.Web;
using AfterSchoolLibrary.Models;
using BasicLibrary;

namespace AfterSchool.Models
{
    public static class SessionVariables
    {
        public static Registrant CurrentRegistrant
        {
            get
            {
                return SessionHelper.GetCurrentRegistrant();
            }
            set
            {
                if (value.IsNotEmpty())
                    SessionHelper.AddToSession("CurrentRegistrant", value.ID.Value.ToString());
                else
                    SessionHelper.RemoveFromSession("CurrentRegistrant");
            }
        }
        
        public static WebUser CurrentUser
        {
            get
            {                
                return SessionHelper.GetCurrentUser();
            }
            set
            {
                if (value.IsNotEmpty())
                    SessionHelper.AddToSession("LoggedInUser", value.ID.Value.ToString());
                else
                    SessionHelper.RemoveFromSession("LoggedInUser");
            }
        }

        public static WebUser LoggedInUser
        {
            get
            {
                return OriginalLoggedInUser ?? CurrentUser;
            }
        }

        public static WebUser OriginalLoggedInUser
        {
            get
            {
                return SessionHelper.GetOriginalLoggedInUser();
            }
            set
            {
                if (value.IsNotEmpty())
                    SessionHelper.AddToSession("OriginalLoggedInUser", value.ID.Value.ToString());
                else
                    SessionHelper.RemoveFromSession("OriginalLoggedInUser");
            }
        }
        
        public static Account CurrentAccount
        {
            get
            {
                return SessionHelper.GetAccount();
            }
            set
            {
                if (value.IsNotEmpty())
                    SessionHelper.AddToSession("CurrentAccount", value.ID.Value.ToString());
                else
                {
                    SessionHelper.RemoveFromSession("CurrentAccount");
                    CookieVariables.CurrentAccount = null;
                }
            }
        }

        public static Account ParentAccount
        {
            get
            {
                return SessionHelper.GetParentAccount();
            }
            set
            {
                if (value.IsNotEmpty())
                    SessionHelper.AddToSession("ParentAccount", value.ID.Value.ToString());
                else
                {
                    SessionHelper.RemoveFromSession("ParentAccount");
                    CookieVariables.ParentAccount = null;
                }
            }
        }

        public static UserType AccountUserType
        {
            get
            {
                return SessionHelper.GetAccountUserType();
            }
            set
            {
                HttpContext.Current.Session["AccountUserType"] = value;
            }
        }

        public static Account RegistrationStartPageAccount
        {
            get
            {
                return SessionHelper.GetRegistrationStartPageAccount();
            }
            set
            {
                if (value.IsNotEmpty() && value.ID.HasValue)
                    SessionHelper.AddToSession("RegistrationStartPageAccount", value.ID.Value.ToString());
                else
                    SessionHelper.RemoveFromSession("RegistrationStartPageAccount");
            }
        }

        public static AccountPackage AccountPackage
        {
            get
            {
                return SessionHelper.GetAccountPackage();
            }
            set
            {
                if (value.IsNotEmpty())
                    SessionHelper.AddToSession("AccountPackage", value.ID.Value.ToString());
                else
                    SessionHelper.RemoveFromSession("AccountPackage");
            }
        }

        public static string SessionInvite
        {
            get
            {
                return SessionHelper.GetSessionInvite();
            }
            set
            {
                if (value.IsNotEmpty())
                    SessionHelper.AddToSession("SessionInvite", value.ToString());
                else
                    SessionHelper.RemoveFromSession("SessionInvite");
            }
        }

        public static Session VisitorSession
        {
            get
            {
                return SessionHelper.GetVisitorSession(); 
            }
            set
            {
                if (value.IsNotEmpty())
                    SessionHelper.AddToSession("VisitorSession", value.ID.Value.ToString());
                else
                    SessionHelper.RemoveFromSession("VisitorSession");
            }
        }
        
        public static Cart Cart
        {
            get
            {
                return SessionHelper.GetCart();
            }
            set
            {
                if (value.IsNotEmpty())
                    SessionHelper.AddToSession("Cart", value.ID.Value.ToString());
                else
                    SessionHelper.RemoveFromSession("Cart");
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


        public static WebSession WebSession
        {
            get
            {
                return SessionHelper.GetWebSession();
            }
            set
            {
                if (value.IsNotEmpty())
                    SessionHelper.AddToSession("WebSession", value.ID.Value.ToString());
                else
                    SessionHelper.RemoveFromSession("WebSession");
            }
        }

    }
}
