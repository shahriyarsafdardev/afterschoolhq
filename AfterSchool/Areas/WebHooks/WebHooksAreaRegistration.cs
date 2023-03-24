using System.Web.Mvc;

namespace AfterSchool.Areas.WebHooks
{
    public class WebHooksAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "WebHooks";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            // context.MapRoute(
            //     "WebHooks_default",
            //     "WebHooks/{controller}/{action}/{id}",
            //     new { action = "Index", id = UrlParameter.Optional}
            // );
        }
    }
}