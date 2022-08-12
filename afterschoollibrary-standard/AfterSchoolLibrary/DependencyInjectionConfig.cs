using Microsoft.Extensions.DependencyInjection;

namespace AfterSchoolLibrary
{
    public static class DependencyInjectionConfig
    {
        public static void ConfigureServices(IServiceCollection services)
        {
            Emmersion.Http.DependencyInjectionConfig.ConfigureServices(services);
        }
    }
}