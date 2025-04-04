namespace KPCOS.BusinessLayer.DTOs.Response.Statistics;

public class GetStatisticsResponse
{
    public string? Year { get; set; }
    public IEnumerable<GetStatisticDetailResponse> Data { get; set; } = new List<GetStatisticDetailResponse>();
}
