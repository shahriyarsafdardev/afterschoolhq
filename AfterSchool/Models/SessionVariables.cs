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
        private static Registrant _CurrentRegistrant;
        public static Registrant CurrentRegistrant
        {
            get
            {
                if (_CurrentRegistrant.IsEmpty())
                    _CurrentRegistrant = SessionHelper.GetCurrentRegistrant();

                return _CurrentRegistrant;
            }
            set
            {
                if (value.IsNotEmpty())
                    SessionHelper.AddToSession("CurrentRegistrant", value.ID.Value.ToString());
                _CurrentRegistrant = value;
            }
        }

        private static WebUser _LoggedInUser;
        public static WebUser LoggedInUser
        {
            get
            {
                if (_LoggedInUser.IsEmpty())
                    _LoggedInUser = SessionHelper.GetLoggedInUser();

                return _LoggedInUser;
            }
            set
            {
                if (value.IsNotEmpty())
                    SessionHelper.AddToSession("LoggedInUser", value.ID.Value.ToString());
                _LoggedInUser = value;
            }
        }

        private static WebUser _OriginalLoggedInUser;
        public static WebUser OriginalLoggedInUser
        {
            get
            {
                if (_OriginalLoggedInUser.IsEmpty())
                    _OriginalLoggedInUser = SessionHelper.GetOriginalLoggedInUser();

                return _OriginalLoggedInUser;
            }
            set
            {
                if (value.IsNotEmpty())
                    SessionHelper.AddToSession("OriginalLoggedInUser", value.ID.Value.ToString());
                _OriginalLoggedInUser = value;
            }
        }

        private static Account _CurrentAccount;
        public static Account CurrentAccount
        {
            get
            {
                if (_CurrentAccount.IsEmpty())
                    _CurrentAccount = SessionHelper.GetAccount();

                return _CurrentAccount;
            }
            set
            {
                if (value.IsNotEmpty())
                    SessionHelper.AddToSession("CurrentAccount", value.ID.Value.ToString());
                _CurrentAccount = value;
            }
        }

        private static Account _RegistrationStartPageAccount;
        public static Account RegistrationStartPageAccount
        {
            get
            {
                if (_RegistrationStartPageAccount.IsEmpty())
                    _RegistrationStartPageAccount = SessionHelper.GetRegistrationStartPageAccount();

                return _RegistrationStartPageAccount;
            }
            set
            {
                if (value.IsNotEmpty())
                    SessionHelper.AddToSession("RegistrationStartPageAccount", value.ID.Value.ToString());
                _RegistrationStartPageAccount = value;
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

        private static AccountPackage _AccountPackage;
        public static AccountPackage AccountPackage
        {
            get
            {
                if (_AccountPackage.IsEmpty())
                    _AccountPackage = SessionHelper.GetAccountPackage();

                return _AccountPackage;
            }
            set
            {
                if (value.IsNotEmpty())
                    SessionHelper.AddToSession("AccountPackage", value.ID.Value.ToString());
                _AccountPackage = value;
            }
        }

        private static Session _VisitorSession;
        public static Session VisitorSession
        {
            get
            {
                if (_VisitorSession.IsEmpty())
                    _VisitorSession = SessionHelper.GetVisitorSession();

                return _VisitorSession;
            }
            set
            {
                if (value.IsNotEmpty())
                    SessionHelper.AddToSession("VisitorSession", value.ID.Value.ToString());
                _VisitorSession = value;
            }
        }

        private static Cart _Cart;
        public static Cart Cart
        {
            get
            {
                if (_Cart.IsEmpty())
                    _Cart = SessionHelper.GetCart();

                return _Cart;
            }
            set
            {
                if (value.IsNotEmpty())
                    SessionHelper.AddToSession("Cart", value.ID.Value.ToString());
                _Cart = value;
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
