using System.Web.Mvc;

namespace AfterSchool.Areas.StripeWebHooks
{
    public class StripeWebHooksAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "StripeWebHooks";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "StripeWebHooks_default",
                "StripeWebHooks/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}