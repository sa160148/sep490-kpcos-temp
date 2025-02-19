using Microsoft.Extensions.Configuration;

namespace KPCOS.Common.Utilities;

public static class GlobalUtility
{
    /// <summary>
    /// Get connection string from appsettings.json or environment variables,
    /// you should have .env file in your KPCOS.API folder or project root folder
    /// </summary>
    /// <returns></returns>
    public static string? GetConnectionString()
    {
        DotNetEnv.Env.Load();
        string? connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__Default");
        string? finalConnectionString = "";
        if (!string.IsNullOrEmpty(connectionString))
        {
            finalConnectionString = connectionString;
        }
        else
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", true, false);
            IConfigurationRoot configurationRoot = builder.Build();
            finalConnectionString = configurationRoot.GetConnectionString("Default");
        }
        return finalConnectionString;
    }
}