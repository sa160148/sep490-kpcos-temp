using KPCOS.API.Extensions.DatabaseServices;
using KPCOS.API.Extensions.ServicesAddIn;

namespace KPCOS.API.Extensions;

public static class ExtensionProfiles
{
    public static IServiceCollection AddExtensionProfiles(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDatabaseServices(configuration: configuration);
        services.AddAuthServices(configuration: configuration);
        services.AddSwaggerServices();
        services.AddServices();
        return services;
    }
}