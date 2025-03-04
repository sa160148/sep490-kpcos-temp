using KPCOS.Common.Constants;

namespace KPCOS.BusinessLayer.DTOs.Response.Designs;

public class GetAllDesignResponse
{
    public Guid Id { get; set; }
    public int? Version { get; set; }
    public string Type { get; set; }
    public string Status { get; set; }
    public string? Reason { get; set; }
    public bool? IsPublic { get; set; }
    public string? ImageUrl { get; set; } = ImageConstant.BlankImageUrl;
}