using System.Web.Mvc;
using System.Web.Routing;

namespace AfterSchool
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            //routes.RouteExistingFiles = true;
            //routes.MapRoute(
            //    name: "StaticFileRoute",
            //    url: "home/{*file}",
            //    defaults: new { controller = "Home", action = "HandleStatic", area = "" }
            //);

            routes.MapRoute(
                "HomePage",
                "",
                new { controller = "home", action = "index", id = UrlParameter.Optional },
                new[] { "AfterSchool.Controllers" });

            routes.MapRoute(
                "Home",
                "home/{action}/{id}",
                new { controller = "home", action = "index", id = UrlParameter.Optional },
                new[] { "AfterSchool.Controllers" });

            routes.MapRoute(
                "AfterSchool",
                "afterschool/{action}/{id}",
                new { controller = "afterschool", action = "index", id = UrlParameter.Optional },
                new[] { "AfterSchool.Controllers" });

            routes.MapRoute(
                "Services",
                "services/{action}/{id}",
                new { controller = "services", action = "index", id = UrlParameter.Optional },
                new[] { "AfterSchool.Controllers" });

            routes.MapRoute(
                "Login",
                "login/{action}/{id}",
                new { controller = "login", action = "index", id = UrlParameter.Optional });

            routes.MapRoute(
                "Logout",
                "logout/{action}/{id}",
                new { controller = "logout", action = "index", id = UrlParameter.Optional });

            routes.MapRoute(
                "Reg",
                "reg/{action}/{id}",
                new { controller = "reg", action = "index", id = UrlParameter.Optional });

            routes.MapRoute(
                "SignUp",
                "signup/{action}/{id}",
                new { controller = "signup", action = "index", id = UrlParameter.Optional });

            routes.MapRoute(
                "Pricing",
                "pricing/{action}/{id}",
                new { controller = "pricing", action = "index", id = UrlParameter.Optional });

            routes.MapRoute(
                "Legal",
                "legal/{action}/{id}",
                new { controller = "legal", action = "index", id = UrlParameter.Optional });

            routes.MapRoute(
                "Faq",
                "faq/{action}/{id}",
                new { controller = "faq", action = "index", id = UrlParameter.Optional });

            routes.MapRoute(
                "DB",
                "db/{action}/{id}",
                new { controller = "db", action = "index", id = UrlParameter.Optional });

            routes.MapRoute(
                "RegistrantAccount",
                "account/{action}/{id}",
                new { controller = "account", action = "getstarted", id = UrlParameter.Optional },
                new string[] { "AfterSchool.Controllers" });

            routes.MapRoute(
                "LeagueHome",
                "{accountname}/league/{districtname}/{leaguename}",
                new
                {
                    controller = "r",
                    action = "league",
                    accountname = UrlParameter.Optional,
                    districtname = UrlParameter.Optional,
                    leaguename = UrlParameter.Optional,
                });

            routes.MapRoute(
                "ActivityRegister",
                "{accountname}/{activitytype}/{activityname}/register",
                new
                {
                    controller = "r",
                    action = "register",
                    accountname = UrlParameter.Optional,
                    activitytype = UrlParameter.Optional,
                    activityname = UrlParameter.Optional
                });

            routes.MapRoute(
                "AccountHome",
                "{accountname}",
                new { controller = "r", action = "account", accountname = UrlParameter.Optional });

            routes.MapRoute(
                "AccountBrowse",
                "{accountname}/browse",
                new { controller = "r", action = "browse", accountname = UrlParameter.Optional });

            routes.MapRoute(
                "ActivityHome",
                "{accountname}/{activitytype}/{activityname}",
                new
                {
                    controller = "r",
                    action = "activity",
                    accountname = UrlParameter.Optional,
                    activitytype = UrlParameter.Optional,
                    activityname = UrlParameter.Optional
                });

            routes.MapRoute(
                "Standard",
                "{controller}/{action}/{id}",
                new { controller = "pricing", action = "index", id = UrlParameter.Optional }
            );

        }
    }
}
