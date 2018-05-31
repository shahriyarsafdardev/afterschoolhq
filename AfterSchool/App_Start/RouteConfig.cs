using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace AfterSchool
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

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
                "OrgPage",
                "{id}/{activity}",
                new { controller = "reg", action = "org", id = UrlParameter.Optional, activity = UrlParameter.Optional });

            routes.MapRoute(
                "Standard",
                "{controller}/{action}/{id}",
                new { controller = "pricing", action = "index", id = UrlParameter.Optional }
            );

        }
    }
}
