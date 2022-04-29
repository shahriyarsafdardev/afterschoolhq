using System;
using System.Linq;
using System.Web.Mvc;
using AfterSchool;
using AfterSchoolLibrary.FeatureFlag;
using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using Emmersion.InfluxDB;
using Hangfire;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(Startup))]

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

            app.Use<RecordControllerActionsToInfluxMiddleware>(resolver.GetService(typeof(IInfluxRecorder)));

            app.UseHangfireDashboard();
            app.UseHangfireServer();

            // schedule and run the task daily at 3am EST
            RecurringJob.AddOrUpdate(() => AccountService.PreparePayouts(null, null), Cron.Daily(3, 00));
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddTransient(provider => Database.Credentials);
            services.AddTransient<IFeatureFlagRepository, FeatureFlagRepository>();

            services.AddControllersAsServices(typeof(Startup).Assembly.GetExportedTypes()
                .Where(t => !t.IsAbstract && !t.IsGenericTypeDefinition)
                .Where(t => typeof(IController).IsAssignableFrom(t)
                            || t.Name.EndsWith("Controller", StringComparison.OrdinalIgnoreCase)));

            services.AddTransient<IInfluxSettings>(provider => new InfluxSettings("http://localhost", 8086, "local"));
            services.AddTransient<IInfluxLogger, InfluxLogger>();
            DependencyInjectionConfig.ConfigureServices(services);
        }
    }
}