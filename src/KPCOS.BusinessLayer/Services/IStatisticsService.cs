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
    /// Gets monthly statistics for finished projects and completed maintenance requests
    /// </summary>
    /// <param name="request">Filter criteria including years to get statistics for</param>
    /// <returns>Monthly statistics grouped by year</returns>
    Task<(IEnumerable<GetStatisticsResponse> data, int totalRecords)> GetProjectAndMaintenanceStatisticsAsync(
        GetStatisticFilterRequest request);

    /// <summary>
    /// Gets total monthly statistics combining both finished projects and completed maintenance requests
    /// </summary>
    /// <param name="request">Filter criteria including years to get statistics for</param>
    /// <returns>Total monthly statistics grouped by year</returns>
    Task<(IEnumerable<GetStatisticsResponse> data, int totalRecords)> GetTotalProjectAndMaintenanceStatisticsAsync(
        GetStatisticFilterRequest request);

    /// <summary>
    /// [DEPRECATED] Gets monthly transaction statistics for specified years
    /// </summary>
    /// <param name="request">Filter criteria including years to get statistics for</param>
    /// <returns>Monthly transaction statistics grouped by year</returns>
    [Obsolete("This method is deprecated. Use GetProjectAndMaintenanceStatisticsAsync instead.")]
    Task<(IEnumerable<GetStatisticsResponse> data, int totalRecords)> GetStatisticsAsync(
        GetStatisticFilterRequest request);

    /// <summary>
    /// [DEPRECATED] Gets total monthly transaction statistics combining both construction and maintenance transactions
    /// </summary>
    /// <param name="request">Filter criteria including years to get statistics for</param>
    /// <returns>Total monthly transaction statistics grouped by year</returns>
    [Obsolete("This method is deprecated. Use GetTotalProjectAndMaintenanceStatisticsAsync instead.")]
    Task<(IEnumerable<GetStatisticsResponse> data, int totalRecords)> GetTotalTransactionStatisticsAsync(
        GetStatisticFilterRequest request);

    /// <summary>
    /// Gets the year-over-year growth rate for total number of transactions
    /// </summary>
    /// <returns>Growth rate compared to previous year</returns>
    Task<GetGrowthRateStatisticResponse> GetTransactionCountGrowthRateAsync();

    /// <summary>
    /// Gets the year-over-year growth rate for total transaction amount
    /// </summary>
    /// <returns>Growth rate compared to previous year</returns>
    Task<GetGrowthRateStatisticResponse> GetTransactionAmountGrowthRateAsync();
}
