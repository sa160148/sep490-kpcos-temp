using AutoMapper;
using KPCOS.DataAccessLayer.Context;
using KPCOS.DataAccessLayer.Repositories;
using KPCOS.DataAccessLayer.Repositories.Implements;
using Microsoft.EntityFrameworkCore;

namespace KPCOS.API.Extensions.DatabaseServices;

public static class DatabaseAddIn
{
    public static IServiceCollection AddDatabaseServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<KPCOSDBContext>(options =>
        {
            options.UseNpgsql(configuration.GetConnectionString("Default"));
        });
        services.AddStackExchangeRedisCache(options =>
        {
            configuration.GetConnectionString("Redis");
            //options.InstanceName = "cache";
        });

        services.AddScoped<Func<KPCOSDBContext>>((provider) => () => provider.GetService<KPCOSDBContext>()!);
        services.AddScoped<DbFactory>();
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        
        return services;
    }
}