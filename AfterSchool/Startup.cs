using System;
using System.Linq;
using System.Web.Mvc;
using AfterSchoolLibrary.FeatureFlag;
using AfterSchoolLibrary.Models;
using Microsoft.Owin;
using Owin;
using Hangfire;
using AfterSchoolLibrary.Services;
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
                .UseSqlServerStorage(Database.Credentials.ConnectionString);

            var services = new ServiceCollection();
            ConfigureServices(services);
            var resolver = new DefaultDependencyResolver(services.BuildServiceProvider());
            DependencyResolver.SetResolver(resolver);


            app.UseHangfireDashboard();
            app.UseHangfireServer();

            // schedule and run the task daily at 3am EST
            RecurringJob.AddOrUpdate(() => AccountService.PreparePayouts(null, null), Cron.Daily(hour: 3, minute: 00));
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddTransient(provider => Database.Credentials);
            services.AddTransient<IFeatureFlagRepository, FeatureFlagRepository>();

            services.AddControllersAsServices(typeof(Startup).Assembly.GetExportedTypes()
                .Where(t => !t.IsAbstract && !t.IsGenericTypeDefinition)
                .Where(t => typeof(IController).IsAssignableFrom(t)
                            || t.Name.EndsWith("Controller", StringComparison.OrdinalIgnoreCase)));
        }
    }
}