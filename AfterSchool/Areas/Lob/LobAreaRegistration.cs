using System.Web.Mvc;

namespace AfterSchool.Areas.Lob
{
    public class LobAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "Lob";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "Lob_default",
                "Lob/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}