using KPCOS.API.Extensions.DatabaseServices;
using KPCOS.API.Extensions.ServicesAddIn;
using KPCOS.BusinessLayer.Helpers;

namespace KPCOS.API.Extensions;

public static class ExtensionProfiles
{
    public static IServiceCollection AddExtensionProfiles(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDatabaseServices(configuration: configuration);
        services.AddAutoMapper(typeof(MapperProfile));
        services.AddAuthServices(configuration: configuration);
        services.AddSwaggerServices();
        services.AddBackgroundServices();
        services.AddServices();
        services.AddFirebase(configuration);

        return services;
    }
}