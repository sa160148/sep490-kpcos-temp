using System.Reflection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.OpenApi.Models;

namespace KPCOS.API.Extensions.ServicesAddIn;

public static class AddSwagger
{
    public static IServiceCollection AddSwaggerServices(this IServiceCollection services)
    {
        services.AddSwaggerGen(opt =>
        {
            opt.SwaggerDoc("v1", new OpenApiInfo { Title = "KPCOS.api", Version = "v1" });

            // Enable annotations for more detailed parameter descriptions
            opt.EnableAnnotations();

            opt.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                In = ParameterLocation.Header,
                Description = "Please enter token",
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                BearerFormat = "JWT",
                Scheme = "Bearer"
            });
            opt.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });

            // Include XML comments from the API assembly
            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            opt.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFile));
            
            // Include XML comments from the BusinessLayer assembly
            var businessLayerXmlFile = "KPCOS.BusinessLayer.xml";
            var businessLayerXmlPath = Path.Combine(AppContext.BaseDirectory, businessLayerXmlFile);
            if (File.Exists(businessLayerXmlPath))
            {
                opt.IncludeXmlComments(businessLayerXmlPath);
            }
            
            // Include XML comments from the Common assembly
            var commonXmlFile = "KPCOS.Common.xml";
            var commonXmlPath = Path.Combine(AppContext.BaseDirectory, commonXmlFile);
            if (File.Exists(commonXmlPath))
            {
                opt.IncludeXmlComments(commonXmlPath);
            }
        });

        return services;
    }
}