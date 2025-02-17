namespace KPCOS.DataAccessLayer.DTOs.Response;

public class BaseResponse<T> where T : class
{
    public int ResponseCode { get; set; }
    public string? Message { get; set; }
    public T? Data { get; set; }
}

public class BaseResponse
{
    public int ResponseCode { get; set; }
    public string? Message { get; set; }
}