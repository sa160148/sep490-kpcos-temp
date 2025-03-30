using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using System.Net;
using System.Net.Sockets;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;
using System.Collections.Generic;
using System.Text;

namespace KPCOS.Common.Utilities;

public static class GlobalUtility
{
    /// <summary>
    /// Generates a random code with the specified length consisting of uppercase letters and numbers
    /// </summary>
    /// <param name="length">The length of the code to generate</param>
    /// <returns>A random alphanumeric code</returns>
    public static string GenerateRandomCode(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var random = new Random();
        var code = new StringBuilder(length);
        
        for (int i = 0; i < length; i++)
        {
            code.Append(chars[random.Next(chars.Length)]);
        }
        
        return code.ToString();
    }

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

    /// <summary>
    /// Converts a DateTime to SEA time zone and specifies the kind as Unspecified for PostgreSQL compatibility.
    /// This is useful when working with PostgreSQL timestamp without time zone columns.
    /// </summary>
    /// <param name="dateTime">The DateTime to convert</param>
    /// <returns>A DateTime in SEA time zone with DateTimeKind.Unspecified</returns>
    public static DateTime? ConvertToSEATimeForPostgres(DateTime? dateTime)
    {
        if (!dateTime.HasValue)
        {
            return null;
        }

        // Convert to SEA time zone
        var seaTimeZone = GetSEATimeZone();
        var seaTime = TimeZoneInfo.ConvertTime(dateTime.Value, seaTimeZone);
        
        // Specify kind as Unspecified for PostgreSQL compatibility with timestamp without time zone
        return DateTime.SpecifyKind(seaTime, DateTimeKind.Unspecified);
    }

    /// <summary>
    /// Tính tổng giá tiền theo số mét khối với giá giảm dần.
    /// </summary>
    /// <param name="cubicMeters">Số mét khối (có thể là số lẻ).</param>
    /// <param name="initialPrice">Giá tiền cho mét khối đầu tiên.</param>
    /// <param name="priceDropPerCubic">Số tiền giảm cho mỗi mét khối tiếp theo.</param>
    /// <param name="minPrice">Giá thấp nhất có thể giảm đến (mặc định = 0).</param>
    /// <returns>Tổng giá tiền làm tròn (int, đơn vị VND).</returns>
    public static int CalculatePrice(double cubicMeters, int initialPrice, int priceDropPerCubic, int minPrice = 0)
    {
        double totalCost = 0;

        for (int i = 0; i < Math.Floor(cubicMeters); i++)
        {
            int price = Math.Max(initialPrice - (i * priceDropPerCubic), minPrice);
            totalCost += price;
        }

        // Xử lý phần mét khối lẻ
        double fractionalPart = cubicMeters - Math.Floor(cubicMeters);
        if (fractionalPart > 0)
        {
            int lastPrice = Math.Max(initialPrice - ((int)cubicMeters * priceDropPerCubic), minPrice);
            totalCost += fractionalPart * lastPrice;
        }

        return (int)Math.Round(totalCost);
    }

    /// <summary>
    /// Checks if a given date falls on a weekend (Saturday or Sunday)
    /// </summary>
    /// <param name="date">The date to check</param>
    /// <returns>True if the date is a weekend day</returns>
    public static bool IsWeekend(DateOnly date)
    {
        return date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday;
    }

    /// <summary>
    /// Get Vietnamese holidays for the current year as a fallback when API is unavailable
    /// </summary>
    /// <returns>A set of common Vietnamese holidays</returns>
    public static HashSet<DateOnly> GetVietnameseHolidaysFallback()
    {
        int currentYear = DateTime.Now.Year;
        var holidays = new HashSet<DateOnly>();
        
        // New Year's Day
        holidays.Add(new DateOnly(currentYear, 1, 1));
        
        // Tet Holiday (approximate dates for fallback)
        // Actual dates vary based on lunar calendar
        holidays.Add(new DateOnly(currentYear, 2, 1));
        holidays.Add(new DateOnly(currentYear, 2, 2));
        holidays.Add(new DateOnly(currentYear, 2, 3));
        holidays.Add(new DateOnly(currentYear, 2, 4));
        holidays.Add(new DateOnly(currentYear, 2, 5));
        
        // Hung Kings Commemoration
        holidays.Add(new DateOnly(currentYear, 4, 10));
        
        // Reunification Day
        holidays.Add(new DateOnly(currentYear, 4, 30));
        
        // Labor Day
        holidays.Add(new DateOnly(currentYear, 5, 1));
        
        // Independence Day
        holidays.Add(new DateOnly(currentYear, 9, 2));
        
        return holidays;
    }

    /// <summary>
    /// Fetches Vietnamese holidays from the holiday API
    /// </summary>
    /// <param name="httpClient">The HttpClient to use for API calls</param>
    /// <returns>A collection of holiday dates</returns>
    public static async Task<HashSet<DateOnly>> GetVietnameseHolidaysAsync(HttpClient httpClient)
    {
        try
        {
            string apiUrl = "https://api.11holidays.com/v1/holidays?country=VN";
            var response = await httpClient.GetAsync(apiUrl);
            
            if (!response.IsSuccessStatusCode)
            {
                // Return fallback holidays if the API call fails
                return GetVietnameseHolidaysFallback();
            }
            
            var content = await response.Content.ReadAsStringAsync();
            var holidays = new HashSet<DateOnly>();
            
            try
            {
                // Use JsonDocument to parse the JSON without a predefined class
                using JsonDocument doc = JsonDocument.Parse(content);
                var root = doc.RootElement;
                
                // Check if the response is an array
                if (root.ValueKind == JsonValueKind.Array)
                {
                    foreach (var element in root.EnumerateArray())
                    {
                        // Try to get the date property
                        if (element.TryGetProperty("date", out JsonElement dateElement) && 
                            DateOnly.TryParse(dateElement.GetString(), out DateOnly holidayDate))
                        {
                            holidays.Add(holidayDate);
                        }
                    }
                }
                
                return holidays.Count > 0 ? holidays : GetVietnameseHolidaysFallback();
            }
            catch (JsonException)
            {
                // Return fallback holidays if the JSON parsing fails
                return GetVietnameseHolidaysFallback();
            }
        }
        catch (HttpRequestException)
        {
            // Return fallback holidays if there's a network issue
            return GetVietnameseHolidaysFallback();
        }
    }

    /// <summary>
    /// Gets the next available working date, skipping weekends and holidays
    /// </summary>
    /// <param name="startDate">The initial date</param>
    /// <param name="holidays">Optional collection of holidays to avoid</param>
    /// <returns>The next available working date</returns>
    public static DateOnly GetNextWorkingDay(DateOnly startDate, HashSet<DateOnly> holidays = null)
    {
        var nextDay = startDate.AddDays(1);
        
        // Keep incrementing until we find a day that's not a weekend or holiday
        while (IsWeekend(nextDay) || (holidays != null && holidays.Contains(nextDay)))
        {
            nextDay = nextDay.AddDays(1);
        }
        
        return nextDay;
    }
    
    /// <summary>
    /// Gets the next month's corresponding date ensuring it's a working day (not a weekend or holiday)
    /// </summary>
    /// <param name="currentDate">The current date</param>
    /// <param name="httpClient">HttpClient for fetching holidays</param>
    /// <returns>A date in the next month that is a working day</returns>
    public static async Task<DateOnly> GetNextMonthWorkingDayAsync(DateOnly currentDate, HttpClient httpClient)
    {
        // Get holidays
        var holidays = await GetVietnameseHolidaysAsync(httpClient);
        
        // Calculate next month's date (keeping the same day if possible)
        var nextMonthDate = currentDate.AddMonths(1);
        
        // Ensure it's not a weekend or holiday
        while (IsWeekend(nextMonthDate) || holidays.Contains(nextMonthDate))
        {
            nextMonthDate = nextMonthDate.AddDays(1);
        }
        
        return nextMonthDate;
    }

    /// <summary>
    /// Gets a sequence of maintenance dates based on duration, ensuring they are all working days
    /// </summary>
    /// <param name="startDate">Initial date for maintenance</param>
    /// <param name="duration">Number of maintenance dates needed</param>
    /// <param name="httpClient">HttpClient for fetching holidays</param>
    /// <returns>A list of maintenance dates</returns>
    public static async Task<List<DateOnly>> GetMaintenanceDatesAsync(DateOnly startDate, int duration, HttpClient httpClient)
    {
        // Get holidays with fallback mechanism
        HashSet<DateOnly> holidays;
        try
        {
            holidays = await GetVietnameseHolidaysAsync(httpClient);
        }
        catch (Exception)
        {
            // Use fallback if any unexpected error occurs
            holidays = GetVietnameseHolidaysFallback(); 
        }
        
        // Initialize date collection
        var dates = new List<DateOnly>();
        
        // Get original milestone day from start date (e.g., 30 from 2025-01-30)
        int milestoneDayOfMonth = startDate.Day;
        
        // Make sure the start date itself is a working day
        var currentDate = startDate;
        while (IsWeekend(currentDate) || holidays.Contains(currentDate))
        {
            currentDate = currentDate.AddDays(1);
        }
        
        // Add the start date
        dates.Add(currentDate);
        
        // Calculate subsequent dates based on the milestone day when possible
        for (int i = 1; i < duration; i++)
        {
            // Get the next month
            var nextMonth = currentDate.AddMonths(1).Month;
            var nextYear = currentDate.AddMonths(1).Year;
            
            // Try to maintain the original milestone day (e.g., 30th of the month)
            // We need to check if the day exists in the next month (e.g., Feb doesn't have 30/31)
            int daysInNextMonth = DateTime.DaysInMonth(nextYear, nextMonth);
            int targetDay = Math.Min(milestoneDayOfMonth, daysInNextMonth);
            
            var nextDate = new DateOnly(nextYear, nextMonth, targetDay);
            
            // Adjust if the date falls on a weekend or holiday
            while (IsWeekend(nextDate) || holidays.Contains(nextDate))
            {
                nextDate = nextDate.AddDays(1);
            }
            
            dates.Add(nextDate);
            currentDate = nextDate;
        }
        
        // For the final task, add an additional task at the end of the last month
        if (duration > 0)
        {
            var lastDate = dates.Last();
            var lastMonth = lastDate.Month;
            var lastYear = lastDate.Year;
            
            // Get the last day of the month
            int daysInLastMonth = DateTime.DaysInMonth(lastYear, lastMonth);
            var endOfMonthDate = new DateOnly(lastYear, lastMonth, daysInLastMonth);
            
            // Adjust if the date falls on a weekend or holiday
            while (IsWeekend(endOfMonthDate) || holidays.Contains(endOfMonthDate))
            {
                endOfMonthDate = endOfMonthDate.AddDays(-1); // Go backward to stay in the same month
                
                // If we've adjusted too much, break to avoid issues
                if (endOfMonthDate <= lastDate)
                {
                    break;
                }
            }
            
            // Only add the end-of-month date if it's different from the regular date
            // and it's still after the regular date
            if (endOfMonthDate > lastDate)
            {
                dates.Add(endOfMonthDate);
            }
        }
        
        return dates;
    }

    /// <summary>
    /// Calculates total price for maintenance requests with duration-based discounts
    /// </summary>
    /// <param name="basePrice">Base price calculated from area and depth</param>
    /// <param name="duration">The number of maintenance tasks</param>
    /// <param name="packageRate">The rate from maintenance package (as a percentage)</param>
    /// <returns>The discounted total price</returns>
    public static int CalculatePriceWithDurationDiscount(int basePrice, int duration, int packageRate)
    {
        // No discount for single maintenance tasks
        if (duration <= 1)
        {
            return basePrice;
        }
        
        // Calculate discount rate based on duration and package rate
        // The package rate is used as a base multiplier for the discount
        double baseDiscountRate = packageRate / 100.0;
        double discountRate;
        
        if (duration >= 24) // 2 years of monthly maintenance
        {
            discountRate = baseDiscountRate * 3.0; // 3x the package rate
            // Ensure discount doesn't exceed 40%
            discountRate = Math.Min(discountRate, 0.40);
        }
        else if (duration >= 12) // 1 year of monthly maintenance
        {
            discountRate = baseDiscountRate * 2.0; // 2x the package rate
            // Ensure discount doesn't exceed 30%
            discountRate = Math.Min(discountRate, 0.30);
        }
        else if (duration >= 6) // 6 months of maintenance
        {
            discountRate = baseDiscountRate * 1.5; // 1.5x the package rate
            // Ensure discount doesn't exceed 20%
            discountRate = Math.Min(discountRate, 0.20);
        }
        else // Less than 6 months
        {
            discountRate = baseDiscountRate; // 1x the package rate
            // Ensure discount doesn't exceed 10%
            discountRate = Math.Min(discountRate, 0.10);
        }
        
        // Calculate final price
        return (int)(basePrice * (1 - discountRate) * duration);
    }

    /// <summary>
    /// Tính tổng giá tiền dựa trên số tháng theo các cụm giảm giá.
    /// </summary>
    /// <param name="monthlyCost">Tổng giá tiền của một tháng (đã tính từ mét khối).</param>
    /// <param name="months">Số tháng sử dụng.</param>
    /// <param name="discounts">Dictionary chứa các mức giảm giá theo từng cụm tháng.</param>
    /// <returns>Tổng giá tiền làm tròn (int, đơn vị VND).</returns>
    public static int CalculatePriceWithMonthGroups(int monthlyCost, int months, Dictionary<int, int> discounts)
    {
        double totalCost = 0;

        foreach (var kvp in discounts.OrderByDescending(d => d.Key))
        {
            int groupMonths = kvp.Key;
            int discountPerMonth = kvp.Value;

            while (months >= groupMonths)
            {
                int adjustedMonthlyCost = Math.Max(monthlyCost - discountPerMonth, 0);
                totalCost += adjustedMonthlyCost * groupMonths;
                months -= groupMonths;
            }
        }

        return (int)Math.Round(totalCost);
    }

    /// <summary>
    /// Normalizes a DateTime for PostgreSQL timestamp without timezone storage.
    /// This function handles all date conversions consistently by ensuring DateTimeKind.Unspecified
    /// and proper timezone conversion if needed.
    /// </summary>
    /// <param name="dateTime">The DateTime to normalize</param>
    /// <param name="ensureSEATimeZone">If true, converts the time to SEA timezone first (default: false)</param>
    /// <returns>A DateTime with DateTimeKind.Unspecified suitable for PostgreSQL timestamp</returns>
    public static DateTime? NormalizeDateTime(DateTime? dateTime, bool ensureSEATimeZone = false)
    {
        if (!dateTime.HasValue)
        {
            return null;
        }

        DateTime result = dateTime.Value;
        
        // Convert to SEA timezone if requested
        if (ensureSEATimeZone && result.Kind != DateTimeKind.Unspecified)
        {
            var seaTimeZone = GetSEATimeZone();
            result = TimeZoneInfo.ConvertTime(result, seaTimeZone);
        }
        
        // Always ensure DateTimeKind.Unspecified for PostgreSQL compatibility
        return DateTime.SpecifyKind(result, DateTimeKind.Unspecified);
    }
}