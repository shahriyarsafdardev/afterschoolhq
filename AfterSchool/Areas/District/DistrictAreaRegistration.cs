using System.Web.Mvc;

namespace AfterSchool.Areas.District
{
    public class DistrictAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "District";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "District_default",
                "District/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}