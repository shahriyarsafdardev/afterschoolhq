using System.Web.Mvc;

namespace AfterSchool.Areas.ReactApi
{
    public class ReactApiAreaRegistration : AreaRegistration
    {
        public override string AreaName 
        {
            get 
            {
                return "ReactApi";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "ReactApi_default",
                "ReactApi/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}