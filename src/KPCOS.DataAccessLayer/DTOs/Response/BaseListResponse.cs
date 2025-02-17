namespace KPCOS.DataAccessLayer.DTOs.Response;

public class BaseListResponse<T> where T : class
{
    public int Page { get; set; }
    public int Size { get; set; }
    public int TotalPage { get; set; }
    public int TotalRecord { get; set; }
    public List<T>? Data { get; set; }
}