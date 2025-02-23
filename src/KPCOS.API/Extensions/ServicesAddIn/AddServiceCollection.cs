using KPCOS.BusinessLayer;
using KPCOS.BusinessLayer.Helpers;
using KPCOS.BusinessLayer.Services;
using KPCOS.BusinessLayer.Services.Implements;
using KPCOS.Common;
using KPCOS.DataAccessLayer.Entities;

namespace KPCOS.API.Extensions.ServicesAddIn;

public static class AddServiceCollection
{
    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddAutoMapper(typeof(MapperProfile).Assembly);

        services.AddScoped<IAuthService, AuthService>();
        /*services.AddScoped<IRedisPublisher, RedisPublisher>();*/
        /*services.AddScoped<SocketIoEmitter>();*/
        services.AddScoped<IServiceService, ServiceService>();    
        services.AddScoped<IPackageItemService, PackageItemService>();
        services.AddScoped<IPackageService, PackageService>();
        services.AddScoped<IEquipmentService, EquipmentService>();
        services.AddScoped<ITemplateContructionService, TemplateContructionService>();
        services.AddScoped<IProjectService, ProjectService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IQuotationService, QuotationService>();

        /*services.AddScoped<IUserService, UserService>();
        services.AddScoped<IRoleService, RoleService>();*/

        return services;
    }
}