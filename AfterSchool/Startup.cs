using Microsoft.Owin;
using Owin;
using Hangfire;
using AfterSchoolLibrary.Services;

[assembly: OwinStartupAttribute(typeof(AfterSchool.Startup))]
namespace AfterSchool
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.MapSignalR();
            ConfigureAuth(app);

            GlobalConfiguration.Configuration
                .UseSqlServerStorage(AfterSchoolLibrary.Models.Database.Credentials.ConnectionString);

            app.UseHangfireDashboard();
            app.UseHangfireServer();

            // schedule and run the task daily at 3am EST
            RecurringJob.AddOrUpdate(() => AccountService.PreparePayouts(), Cron.Daily(hour: 3, minute: 00));
        }
    }
}
