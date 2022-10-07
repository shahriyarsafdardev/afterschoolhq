using System.Web.Mvc;

namespace AfterSchool.Areas.Volunteer
{
    public class VolunteerAreaRegistration : AreaRegistration
    {
        public override string AreaName => "Volunteer";


        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "Volunteer_default",
                "Volunteer/{controller}/{action}/{id}",
                new {action = "Index", id = UrlParameter.Optional}
            );
        }
    }
}