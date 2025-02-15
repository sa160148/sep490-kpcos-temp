using AutoMapper;
using KPCOS.DataAccessLayer;
using KPCOS.DataAccessLayer.Context;
using KPCOS.DataAccessLayer.Enums;
using KPCOS.DataAccessLayer.Repositories;
using KPCOS.DataAccessLayer.Repositories.Implements;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

namespace KPCOS.API.Extensions.DatabaseServices;

public static class DatabaseAddIn
{
    public static IServiceCollection AddDatabaseServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<KpcosContext>(options =>
        {
            options.UseNpgsql(configuration.GetConnectionString("Default"),
                o => o.MapEnum<EnumService>("enumService"));
        });
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = configuration.GetConnectionString("Redis");
            //options.InstanceName = "cache";
        });
        services.AddSingleton<IConnectionMultiplexer>(sp =>
        {
            var redisConnectionString = configuration.GetConnectionString("Redis");
            return ConnectionMultiplexer.Connect(redisConnectionString);
        });

        services.AddScoped<Func<KpcosContext>>((provider) => () => provider.GetService<KpcosContext>()!);
        services.AddScoped<DbFactory>();
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        
        
        
        return services;
    }
}