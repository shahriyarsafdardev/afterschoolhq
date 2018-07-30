using System.Web.Mvc;

namespace AfterSchool.Areas.StripeHooks
{
    public class StripeHooksAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "StripeHooks";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            //context.MapRoute(
            //    "StripeHooks_default",
            //    "StripeHooks/{controller}/{action}/{id}",
            //    new { action = "Index", id = UrlParameter.Optional }
            //);
        }
    }
}