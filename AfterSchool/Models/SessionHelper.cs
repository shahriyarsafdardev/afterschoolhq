using System.Web;
using BasicLibrary;
using BasicLibrary.DataAccess;
using AfterSchoolLibrary.Models;

namespace AfterSchool.Models
{
    public class SessionHelper
    {
        public static Account GetAccount(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var id = (string)HttpContext.Current.Session["CurrentAccount"];
            if (id.IsNotEmpty())
                return Account.Load(id.ToInteger(), msgs, dc);

            return null;
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
            if (id.IsNotEmpty())
                return WebUser.Load(id.ToInteger(), msgs, dc);

            return null;
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
            if (id.IsNotEmpty())
                return AccountPackage.Load(id.ToInteger(), msgs, dc);

            return null;
        }

        public static Session GetVisitorSession(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var id = (string)HttpContext.Current.Session["VisitorSession"];
            if (id.IsNotEmpty())
                return Session.Load(id.ToInteger(), msgs, dc);

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