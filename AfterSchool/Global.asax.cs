using AfterSchool.Models;
using AfterSchoolLibrary.Models;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Newtonsoft.Json;
using System.Web;
using System;
using System.Linq;

namespace AfterSchool
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            ViewEngines.Engines.Clear();
            ViewEngines.Engines.Add(new RazorViewEngine());

            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);

            if (!AfterSchoolLibrary.Helpers.Utilities.IsLocalHost)
                GlobalFilters.Filters.Add(new RollbarExceptionFilter());

            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };
            
        }

        protected void Application_BeginRequest()
        {
            var maintenanceSchedule = Maintenance.Load().FirstOrDefault();

            if (maintenanceSchedule != null && DateTime.UtcNow >= maintenanceSchedule.StartTime && DateTime.UtcNow <= maintenanceSchedule.EndTime)
                HttpContext.Current.Response.Redirect("~/scheduled-maintenance.html");
        }

        protected void Application_PostAcquireRequestState()
        {
            var maintenanceSchedules = Maintenance.Load().Where(schedule => DateTime.UtcNow <= schedule.EndTime).ToList();

            if (maintenanceSchedules.Count > 0)
            {
                var upcomingMaintenance = maintenanceSchedules.OrderBy(schedule => schedule.StartTime).First();
                var startTime = upcomingMaintenance.StartTime;
                var formattedTime = startTime.ToString("MMMM") + " " + startTime.Day + ", " + startTime.Year + " at " + startTime.ToString("h:mm tt") + " EST";
                TimeSpan duration = upcomingMaintenance.EndTime - startTime;
                string hours;
                if (duration.TotalHours >= 1)
                {
                    if (duration.Minutes > 0)
                        hours = $"{duration.Hours} hour {duration.Minutes} minutes";
                    else
                        hours = $"{duration.Hours} hour";
                }
                else
                    hours = $"{duration.TotalMinutes} minutes";

                if (DateTime.UtcNow <= upcomingMaintenance.StartTime)
                {
                    var maintenanceBarMessage = $"{UILabel.ProductName()} will be undergoing scheduled maintenance on {formattedTime} for \napproximately {hours}. We apologize for any inconvenience. If you need assistance during this time, please <a href=\"https://afterschoolhq.zendesk.com/hc/en-us/requests/new\">contact support</a>.";
                    SessionVariables.MaintenanceMessage = maintenanceBarMessage;
                }
            }
        }
    }
}
