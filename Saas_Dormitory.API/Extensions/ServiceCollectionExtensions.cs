using Saas_Dormitory.DAL.Interface;
using Saas_Dormitory.DAL.Repositories;

namespace Saas_Dormitory.API.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddScoped<ITenantRepository, TenantRepository>();
            services.AddScoped<IUserProfileRepository, UserProfileRepository>();
            //services.AddScoped<TenantService>();

            return services;
        }
    }
}
