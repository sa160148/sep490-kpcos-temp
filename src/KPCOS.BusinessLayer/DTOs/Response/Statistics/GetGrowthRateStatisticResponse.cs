using System;

namespace KPCOS.BusinessLayer.DTOs.Response.Statistics;

public class GetGrowthRateStatisticResponse
{
    /// <summary>
    /// The calculated growth rate. Null if growth rate cannot be calculated (e.g., when previous period has no data)
    /// </summary>
    public double? GrowthRate { get; set; }

    /// <summary>
    /// Current period value (count or amount)
    /// </summary>
    public double CurrentValue { get; set; }

    /// <summary>
    /// Previous period value (count or amount)
    /// </summary>
    public double PreviousValue { get; set; }

    /// <summary>
    /// Indicates if this is new activity (no previous data exists)
    /// </summary>
    public bool IsNewActivity { get; set; }
}
