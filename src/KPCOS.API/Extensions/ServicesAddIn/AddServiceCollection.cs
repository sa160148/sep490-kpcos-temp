using AutoMapper;
using Hangfire;
using Hangfire.PostgreSql;
using KPCOS.BusinessLayer;
using KPCOS.BusinessLayer.Helpers;
using KPCOS.BusinessLayer.Services;
using KPCOS.BusinessLayer.Services.Implements;
using KPCOS.Common;
using KPCOS.Common.Utilities;
using KPCOS.DataAccessLayer.Entities;
using MailKit.Net.Smtp;
using BackgroundService = KPCOS.BusinessLayer.Services.Implements.BackgroundService;

namespace KPCOS.API.Extensions.ServicesAddIn;

public static class AddServiceCollection
{
    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddAutoMapper(typeof(MapperProfile).Assembly);

        // Add SmtpClient as Scoped service
        services.AddScoped<SmtpClient>();
        
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
        services.AddScoped<IContractService, ContractService>();
        services.AddScoped<IDesignService, DesignService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IBackgroundService, BackgroundService>();
        services.AddScoped<IConstructionServices, ConstructionService>();

        /*services.AddScoped<IUserService, UserService>();
        services.AddScoped<IRoleService, RoleService>();*/

        return services;
    }
    
    public static IServiceCollection AddFirebase(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IFirebaseService, FirebaseService>(sp => new FirebaseService(configuration, sp.GetRequiredService<IMapper>()));
        return services;
    }
    public static IServiceCollection AddBackgroundServices(this IServiceCollection services)
    {
        services.AddHangfire(config =>
            config.UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UsePostgreSqlStorage(opt => opt.UseNpgsqlConnection(GlobalUtility.GetConnectionString))
        );
        services.AddHangfireServer();
        return services;
    }
}