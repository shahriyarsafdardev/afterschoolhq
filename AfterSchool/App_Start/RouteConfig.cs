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

            routes.IgnoreRoute("");

            //routes.MapRoute(
            //    "HomePage",
            //    "",
            //    new { controller = "home", action = "index", id = UrlParameter.Optional },
            //    new[] { "AfterSchool.Controllers" });

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
                "OAuth",
                "oauth/cleverintegration",
                new { controller = "oauth", action = "cleverintegration" });

            routes.MapRoute(
                "OAuthCleverLogin",
                "oauth/cleverlogin",
                new { controller = "oauth", action = "cleverlogin" });

            routes.MapRoute(
                "Indianapolis",
                "indianapolis/{action}/{id}",
                new { controller = "indianapolis", action = "index", id = UrlParameter.Optional });

            routes.MapRoute(
                "Search",
                "search/{action}/{id}",
                new { controller = "search", action = "index", id = UrlParameter.Optional });            

            routes.MapRoute(
                "Logout",
                "logout/{action}/{id}",
                new { controller = "logout", action = "index", id = UrlParameter.Optional });

            routes.MapRoute(
                "Sitemap",
                "sitemap/{action}/{id}",
                new { controller = "sitemap", action = "index", id = UrlParameter.Optional });

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
                "Msg",
                "msg/{id}",
                new { controller = "msg", action = "index", id = UrlParameter.Optional });

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
                "LeagueHome1",
                "{accountname}/league/{leaguename}",
                new
                {
                    controller = "r",
                    action = "league",
                    accountname = UrlParameter.Optional,
                    districtname = UrlParameter.Optional,
                    leaguename = UrlParameter.Optional,
                });

            routes.MapRoute(
                "AccountPay",
                "{accountname}/pay",
                new
                {
                    controller = "r",
                    action = "pay",
                });

            routes.MapRoute(
                "LeagueRegister1",
                "{accountname}/league/{group}/register",
                new
                {
                    controller = "r",
                    action = "registerleague",
                    accountname = UrlParameter.Optional,
                    activitygrouptype = "league",
                    externalaccount = UrlParameter.Optional,
                    group = UrlParameter.Optional
                });

            routes.MapRoute(
                "LeagueHome2",
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
                "SessionHome",
                "{accountname}/session/{sessionname}",
                new
                {
                    controller = "r",
                    action = "activitysession",
                    accountname = UrlParameter.Optional,
                    sessionname = UrlParameter.Optional,
                });

            routes.MapRoute(
                "SessionRegister",
                "{accountname}/session/{sessionname}/register",
                new
                {
                    controller = "r",
                    action = "registersession",
                    accountname = UrlParameter.Optional,
                    sessionname = UrlParameter.Optional
                });

            routes.MapRoute(
                "SessionBrowse",
                "{accountname}/session/{sessionname}/browse",
                new
                {
                    controller = "r",
                    action = "browsesession",
                    accountname = UrlParameter.Optional,
                    sessionname = UrlParameter.Optional
                });

            routes.MapRoute(
                "ActivityRegister",
                "{accountname}/activity/{activityname}/register",
                new
                {
                    controller = "r",
                    action = "register",
                    accountname = UrlParameter.Optional,
                    activitytype = UrlParameter.Optional,
                    activityname = UrlParameter.Optional
                });

            routes.MapRoute(
                "ActivitySelectDates",
                "{accountname}/activity/{activityname}/selectdates",
                new
                {
                    controller = "r",
                    action = "selectdates",
                    accountname = UrlParameter.Optional,
                    activitytype = UrlParameter.Optional,
                    activityname = UrlParameter.Optional
                });

            routes.MapRoute(
                "FieldTripRegister",
                "{accountname}/fieldtrip/{activityname}/register",
                new
                {
                    controller = "r",
                    action = "register",
                    accountname = UrlParameter.Optional,
                    activitytype = UrlParameter.Optional,
                    activityname = UrlParameter.Optional
                });

            routes.MapRoute(
                "PermissionSlipRegister",
                "{accountname}/permissionslip/{activityname}/register",
                new
                {
                    controller = "r",
                    action = "register",
                    accountname = UrlParameter.Optional,
                    activitytype = UrlParameter.Optional,
                    activityname = UrlParameter.Optional
                });

            routes.MapRoute(
                "LearningPodRegister",
                "{accountname}/learningpod/{activityname}/register",
                new
                {
                    controller = "r",
                    action = "register",
                    accountname = UrlParameter.Optional,
                    activitytype = UrlParameter.Optional,
                    activityname = UrlParameter.Optional
                });

            routes.MapRoute(
                "LeagueRegister2",
                "{accountname}/{activitygrouptype}/{externalaccountname}/{group}/register",
                new
                {
                    controller = "r",
                    action = "registerleague",
                    accountname = UrlParameter.Optional,
                    activitygrouptype = UrlParameter.Optional,
                    externalaccount = UrlParameter.Optional,
                    group = UrlParameter.Optional
                });

            routes.MapRoute(
                "ActivityInterested",
                "{accountname}/activity/{activityname}/interested",
                new
                {
                    controller = "r",
                    action = "interested",
                    accountname = UrlParameter.Optional,
                    activitytype = UrlParameter.Optional,
                    activityname = UrlParameter.Optional
                });

            routes.MapRoute(
                "FieldTripInterested",
                "{accountname}/fieldtrip/{activityname}/interested",
                new
                {
                    controller = "r",
                    action = "interested",
                    accountname = UrlParameter.Optional,
                    activitytype = UrlParameter.Optional,
                    activityname = UrlParameter.Optional
                });

            routes.MapRoute(
                "PermissionSlipInterested",
                "{accountname}/permissionslip/{activityname}/interested",
                new
                {
                    controller = "r",
                    action = "interested",
                    accountname = UrlParameter.Optional,
                    activitytype = UrlParameter.Optional,
                    activityname = UrlParameter.Optional
                });

            routes.MapRoute(
                "LearningPodInterested",
                "{accountname}/learningpod/{activityname}/interested",
                new
                {
                    controller = "r",
                    action = "interested",
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
                "AccountContact",
                "{accountname}/contact",
                new { controller = "r", action = "contact", accountname = UrlParameter.Optional });

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
