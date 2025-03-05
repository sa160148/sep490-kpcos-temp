namespace KPCOS.BusinessLayer.DTOs.Response.Designs;

public class GetAllDesignImageResponse
{
    public Guid Id { get; set; }
    public string ImageUrl { get; set; } = null!;
    public DateTime? CreatedAt { get; set; }
}