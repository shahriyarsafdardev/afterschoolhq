using System;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using AfterSchool;
using AfterSchoolLibrary.FeatureFlag;
using AfterSchoolLibrary.Integrations;
using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using BasicLibrary;
using Emmersion.InfluxDB;
using Hangfire;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(Startup))]

namespace AfterSchool
{
    public class Startup
    {
        private readonly IConfigurationRoot configurationRoot;

        public Startup()
        {
            var localConfigOverridesPath =
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                    "afterschoolhq-overrides",
                    "appsettings.json");
            configurationRoot = new ConfigurationBuilder()
                .SetBasePath(Path.Combine(AppContext.BaseDirectory))
                .AddJsonFile("appsettings.json", true)
                .AddJsonFile(localConfigOverridesPath, true)
                .Build();
        }

        public void Configuration(IAppBuilder app)
        {
            app.MapSignalR();

            var services = new ServiceCollection();
            ConfigureServices(services);
            var resolver = new DefaultDependencyResolver(services.BuildServiceProvider());
            DependencyResolver.SetResolver(resolver);

            var connectionString = resolver.GetService<DatabaseCredentials>().ConnectionString;
            GlobalConfiguration.Configuration.UseSqlServerStorage(connectionString);
            Utilities.ConnectionString = connectionString;
            Database.ConnectionString = connectionString;

            AppConfig.Init();

            app.Use<RecordControllerActionsToInfluxMiddleware>(resolver.GetService<IInfluxRecorder>());

            app.UseHangfireDashboard();
            app.UseHangfireServer();

            // schedule and run the task daily at 3am EST
            RecurringJob.AddOrUpdate(() => AccountService.PreparePayouts(null, null), Cron.Daily(3, 00));
        }

        private void ConfigureServices(IServiceCollection services)
        {
            SetConfigurationValues(services, configurationRoot);
            SetDependencyInjection(services);

            services.AddControllersAsServices(typeof(Startup).Assembly.GetExportedTypes()
                .Where(t => !t.IsAbstract && !t.IsGenericTypeDefinition)
                .Where(t => typeof(IController).IsAssignableFrom(t)
                            || t.Name.EndsWith("Controller", StringComparison.OrdinalIgnoreCase)));
        }

        private static void SetConfigurationValues(IServiceCollection services, IConfiguration configuration)
        {
            services.AddTransient(provider => new DatabaseCredentials(
                configuration.GetValue<string>("SqlServer:ServerName"),
                configuration.GetValue<string>("SqlServer:DatabaseName"),
                configuration.GetValue<string>("SqlServer:UserName"),
                configuration.GetValue<string>("SqlServer:Password")));
            services.AddTransient(provider => new SterlingCredentials(
                configuration.GetValue<string>("Sterling:Endpoint"),
                configuration.GetValue<string>("Sterling:ClientId"),
                configuration.GetValue<string>("Sterling:ClientSecret"),
                configuration.GetValue<string>("Sterling:PackageId")));
            services.AddTransient<IInfluxSettings>(provider => new InfluxSettings(
                configuration.GetValue<string>("InfluxDB:Hostname"),
                configuration.GetValue<string>("InfluxDB:OrgName"),
                configuration.GetValue<string>("InfluxDB:BucketName"),
                configuration.GetValue<string>("InfluxDB:ApiToken")));
        }

        private static void SetDependencyInjection(IServiceCollection services)
        {
            services.AddTransient<IFeatureFlagRepository, FeatureFlagRepository>();
            services.AddTransient<IBackgroundCheckWorkflow, BackgroundCheckWorkflow>();
            services.AddTransient<IJsonSerializer, JsonSerializer>();
            services.AddTransient<ISterlingBearerTokenQuery, SterlingBearerTokenQuery>();
            services.AddTransient<ISterlingCreateCandidateQuery, SterlingCreateCandidateQuery>();
            services.AddTransient<ISterlingCreateScreeningQuery, SterlingCreateScreeningQuery>();
            services.AddTransient<IInfluxLogger, InfluxLogger>();

            DependencyInjectionConfig.ConfigureServices(services);
            AfterSchoolLibrary.DependencyInjectionConfig.ConfigureServices(services);
        }
    }
}