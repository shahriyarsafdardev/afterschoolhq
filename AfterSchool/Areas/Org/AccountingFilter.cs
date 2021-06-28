using AfterSchool.Models;
using AfterSchoolLibrary.Services;
using BasicLibrary;
using System.Web.Mvc;
using System.Web.Routing;

namespace AfterSchool.Areas.Org
{
    public class AccountingFilterAttribute : System.Web.Mvc.ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var account = SessionVariables.CurrentAccount;
            var msgs = new SystemMessages();

            if (account.CanReceiveCheckPayouts(msgs))
                return;

            var isStripeEnabled = account.UseStripeConnectDuringCheckout(msgs);
            if (!isStripeEnabled)
            {
                filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary {
                    { "controller", "account" },
                    { "action", "paymentsoptions" },
                    { "area", "org" },
                    { "showPaymentNotificationPopup", "1" },
                });
            }
        }

        public override void OnResultExecuting(ResultExecutingContext filterContext)
        {
            var account = SessionVariables.CurrentAccount;
            var msgs = new SystemMessages();

            if (account.CanReceiveCheckPayouts(msgs))
                return;

            var isStripeEnabled = account.UseStripeConnectDuringCheckout(msgs);
            if (!isStripeEnabled)
            {
                filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary {
                    { "controller", "account" },
                    { "action", "paymentsoptions" },
                    { "area", "org" },
                    { "showPaymentNotificationPopup", "1" },
                });
            }
        }

        //public override void OnResultExecuted(ResultExecutedContext filterContext)
        //{
        //    Log("OnResultExecuted", filterContext.RouteData, filterContext.Result);
        //}

        //public override void OnActionExecuted(ActionExecutedContext filterContext)
        //{
        //    Log("OnActionExecuted", filterContext.RouteData, filterContext.Result);
        //}
    }
}