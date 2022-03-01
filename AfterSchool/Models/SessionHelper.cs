using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using BasicLibrary;
using BasicLibrary.DataAccess;
using System.Web;

namespace AfterSchool.Models
{
    public class SessionHelper
    {
        public static Account GetAccount(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var id = (string)HttpContext.Current.Session["CurrentAccount"];
            Account account = null;

            if (id.IsNotEmpty())
                account = Account.Load(id.ToInteger(), msgs, dc);
            else if (CookieVariables.CurrentAccount.IsGuid())
                account = Account.LoadByUid(CookieVariables.CurrentAccount.ToGuid(), msgs, dc);

            SessionVariables.CurrentAccount = account;

            return account;
        }
        public static Account GetParentAccount(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var id = (string)HttpContext.Current.Session["ParentAccount"];
            Account account = null;

            if (id.IsNotEmpty())
                account = Account.Load(id.ToInteger(), msgs, dc);
            else if (CookieVariables.ParentAccount.IsGuid())
                account = Account.LoadByUid(CookieVariables.ParentAccount.ToGuid(), msgs, dc);

            SessionVariables.ParentAccount = account;

            return account;
        }
        public static UserType GetAccountUserType(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            UserType usertype = UserType.NotSet;

            if (HttpContext.Current.Session["AccountUserType"] != null)
                usertype = (UserType)HttpContext.Current.Session["AccountUserType"];
            else if (SessionVariables.CurrentAccount.IsNotEmpty() && SessionVariables.CurrentUser.IsNotEmpty())
                usertype = SessionVariables.CurrentUser.GetUserType(SessionVariables.CurrentAccount, msgs, dc);

            SessionVariables.AccountUserType = usertype;

            return usertype;
        }

        public static Registrant GetCurrentRegistrant(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var id = (string)HttpContext.Current.Session["CurrentRegistrant"];
            if (id.IsNotEmpty())
                return Registrant.Load(id.ToInteger(), msgs, dc);

            return null;
        }

        public static WebUser GetCurrentUser(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var id = (string)HttpContext.Current.Session["LoggedInUser"];
            WebUser user = null;

            if (id.IsNotEmpty())
                user = WebUser.Load(id.ToInteger(), msgs, dc);
            else if (CookieVariables.CurrentUser.IsGuid())
                user = WebUser.LoadByUid(CookieVariables.CurrentUser.ToGuid(), msgs, dc);

            SessionVariables.CurrentUser = user;

            return user;
        }

        public static WebUser GetOriginalLoggedInUser(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var id = (string)HttpContext.Current.Session["OriginalLoggedInUser"];
            if (id.IsNotEmpty())
                return WebUser.Load(id.ToInteger(), msgs, dc);

            return null;
        }

        public static Account GetRegistrationStartPageAccount(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var id = (string)HttpContext.Current.Session["RegistrationStartPageAccount"];
            if (id.IsNotEmpty())
                return Account.Load(id.ToInteger(), msgs, dc);

            return null;
        }

        public static AccountPackage GetAccountPackage(SystemMessages msgs = null, DatabaseConnection dc = null)
        {           
            var id = (string)HttpContext.Current.Session["AccountPackage"];
            AccountPackage ap = null;

            if (id.IsNotEmpty())
                ap = AccountPackage.Load(id.ToInteger(), msgs, dc);
            else if (SessionVariables.CurrentAccount.IsNotEmpty())
                ap = SessionVariables.CurrentAccount.GetActiveAccountPackage();

            SessionVariables.AccountPackage = ap;

            return ap;
        }

        public static string GetSessionInvite(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var uid = (string)HttpContext.Current.Session["SessionInvite"];
            if (uid.IsNotEmpty())
                return uid;

            return null;
        }

        public static Session GetVisitorSession(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var id = (string)HttpContext.Current.Session["VisitorSession"];
            if (id.IsNotEmpty())
                return Session.Load(id.ToInteger(), msgs, dc);

            return null;
        }

        public static WebSession GetWebSession(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var id = (string)HttpContext.Current.Session["WebSession"];
            if (id.IsNotEmpty())
                return WebSession.Load(id.ToInteger(), msgs, dc);

            return null;
        }

        public static Cart GetCart(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var id = (string)HttpContext.Current.Session["Cart"];
            if (id.IsNotEmpty())
                return Cart.Load(id.ToInteger(), msgs, dc);

            return null;
        }

        public static void AddToSession(string key, string value)
        {
            HttpContext.Current.Session[key] = value;
        }

        public static void RemoveFromSession(string key)
        {
            HttpContext.Current.Session[key] = null;
        }
    }
}