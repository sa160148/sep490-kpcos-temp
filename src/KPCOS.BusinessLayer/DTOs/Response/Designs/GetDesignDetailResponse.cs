namespace KPCOS.BusinessLayer.DTOs.Response.Designs;

public class GetDesignDetailResponse
{
    public Guid Id { get; set; }
    public DateTime? CreatedAt { get; set; } = default;
    public DateTime? UpdatedAt { get; set; } = default;
    public bool? IsActive { get; set; } = default;
    public int Version { get; set; }
    public string? Reason { get; set; }
    public string? Status { get; set; }
    public bool? IsPublic { get; set; } = default;
    public string Type { get; set; } = null!;
    public string? CustomerName { get; set; } = default;
    public Guid? ProjectId { get; set; }
    public Guid? StaffId { get; set; }
    public IEnumerable<GetAllDesignImageResponse>? DesignImages { get; set; } = new List<GetAllDesignImageResponse>();
}