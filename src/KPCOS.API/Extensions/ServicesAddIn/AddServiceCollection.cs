using KPCOS.BusinessLayer.Helpers;
using KPCOS.BusinessLayer.Services;
using KPCOS.BusinessLayer.Services.Implements;
using KPCOS.Common;

namespace KPCOS.API.Extensions.ServicesAddIn;

public static class AddServiceCollection
{
    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddAutoMapper(typeof(MapperProfile).Assembly);

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IRedisPublisher, RedisPublisher>();
        services.AddScoped<SocketIoEmitter>();
        
        
        /*services.AddScoped<IUserService, UserService>();
        services.AddScoped<IRoleService, RoleService>();*/

        return services;
    }
}