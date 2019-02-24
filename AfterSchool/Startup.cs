using Microsoft.Owin;
using Owin;
using Hangfire;

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
        }
    }
}
