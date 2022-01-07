using System.Web.Mvc;
using Microsoft.Owin;
using Owin;
using Hangfire;
using Microsoft.Extensions.DependencyInjection;

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

            var services = new ServiceCollection();
            ConfigureServices(services);
            var resolver = new DefaultDependencyResolver(services.BuildServiceProvider());
            DependencyResolver.SetResolver(resolver);

            app.UseHangfireDashboard();
            app.UseHangfireServer();
        }

        public void ConfigureServices(IServiceCollection services)
        {
            //services.AddTransient(typeof());
        }
    }
}
