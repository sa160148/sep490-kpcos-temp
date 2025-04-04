using System;
using System.Threading.Tasks;
using KPCOS.BusinessLayer.DTOs.Request.Statistics;
using KPCOS.BusinessLayer.DTOs.Response.Statistics;
using KPCOS.BusinessLayer.DTOs.Response.Users;

namespace KPCOS.BusinessLayer.Services;

public interface IStatisticsService
{
    /// <summary>
    /// Gets user statistics including total users, staff, customers and transactions
    /// </summary>
    Task<GetUserStatisticResponse> GetUserStatisticsAsync();

    /// <summary>
    /// Gets monthly transaction statistics for specified years
    /// </summary>
    /// <param name="request">Filter criteria including years to get statistics for</param>
    /// <returns>Monthly transaction statistics grouped by year</returns>
    Task<(IEnumerable<GetStatisticsResponse> data, int totalRecords)> GetStatisticsAsync(
        GetStatisticFilterRequest request);
}
