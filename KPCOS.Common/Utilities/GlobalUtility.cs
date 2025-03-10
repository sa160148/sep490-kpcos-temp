using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using System.Net;
using System.Net.Sockets;

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

    /// <summary>
    /// Generate a server URL with HTTPS scheme.
    /// </summary>
    /// <param name="httpContextAccessor">The IHttpContextAccessor instance.</param>
    /// <param name="httpContextAccessor">The IHttpContextAccessor instance.</param>
    /// <returns>The server URL with HTTPS scheme.</returns>
    public static string GetSecureServerUrl(IHttpContextAccessor httpContextAccessor)
    {
        if (httpContextAccessor?.HttpContext == null)
        {
             throw new Exception("HttpContext is null.");
        }

        var host = httpContextAccessor.HttpContext.Request.Host.ToUriComponent();

        if (string.IsNullOrEmpty(host))
        {
            throw new Exception("Host is not available.");
        }

        // Always force HTTPS scheme
        return $"https://{host}";
    } 

    public static string GetIpAddress()
    {
        var host = Dns.GetHostEntry(Dns.GetHostName());

        foreach (var ip in host.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                Console.WriteLine(ip.ToString());
                return ip.ToString();
            }
        }

        return "127.0.0.1";
    }

    public static DateTime GetCurrentSEATime()
    {
        TimeZoneInfo tz = TimeZoneInfo.FindSystemTimeZoneById("Asia/Ho_Chi_Minh");
        DateTime localTime = DateTime.Now;
        DateTime utcTime = TimeZoneInfo.ConvertTime(localTime, TimeZoneInfo.Local, tz);
        return utcTime;
    }

    public static DateTime ConvertToSEATime(DateTime value)
    {
        TimeZoneInfo tz = TimeZoneInfo.FindSystemTimeZoneById("Asia/Ho_Chi_Minh");
        DateTime convertedTime = TimeZoneInfo.ConvertTime(value, tz);
        return convertedTime;
    }
    
    public static TimeZoneInfo GetSEATimeZone()
    {
        TimeZoneInfo tz;
        try
        {
            // Try using Windows time zone
            tz = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
        }
        catch (TimeZoneNotFoundException)
        {
            // Fallback to IANA time zone for Linux/Docker
            tz = TimeZoneInfo.FindSystemTimeZoneById("Asia/Ho_Chi_Minh");
        }
        return tz;
    }
}