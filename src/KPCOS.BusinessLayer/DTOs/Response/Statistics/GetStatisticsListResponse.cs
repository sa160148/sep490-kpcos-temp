using System.Collections.Generic;

namespace KPCOS.BusinessLayer.DTOs.Response.Statistics;

public class GetStatisticsListResponse
{
    /// <summary>
    /// List of statistics data grouped by year
    /// </summary>
    public IEnumerable<GetStatisticsResponse> Data { get; set; }

    /// <summary>
    /// Total number of records
    /// </summary>
    public int TotalRecords { get; set; }
} 