using System.Security.Claims;
using System.Text;
using KPCOS.DataAccessLayer.Enums;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace KPCOS.API.Extensions.ServicesAddIn;

public static class AddAuth
{
    public static IServiceCollection AddAuthServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAuthentication(option =>
        {
            option.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            option.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            option.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(opt =>
        {
            opt.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Secret"]!))
            };
            opt.RequireHttpsMetadata = false;
            opt.Events = new JwtBearerEvents
            {
                OnAuthenticationFailed = context =>
                {
                    if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                    {
                        context.Response.Headers.Add("Token-Expired", "true");
                    }
                    return Task.CompletedTask;
                }
            };
        });

        services.AddAuthorization(option =>
        {
            option.AddPolicy(RoleEnum.ADMINISTRATOR.ToString(), policy => policy.RequireClaim(ClaimTypes.Role, RoleEnum.ADMINISTRATOR.ToString(), "true"));
            option.AddPolicy(RoleEnum.MANAGER.ToString(), policy => policy.RequireClaim(ClaimTypes.Role, RoleEnum.MANAGER.ToString(), "true"));
        });

        // CORS
        services.AddCors(options =>
        {
            options.AddPolicy("Cors",
                builder =>
                {
                    builder.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader();
                });
        });

        return services;
    }
}