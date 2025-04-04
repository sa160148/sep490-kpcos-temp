namespace KPCOS.BusinessLayer.DTOs.Response.Statistics;

public class GetStatisticDetailResponse
{
    public string? Name { get; set; }
    public IEnumerable<int> Data { get; set; } = new List<int>();
}