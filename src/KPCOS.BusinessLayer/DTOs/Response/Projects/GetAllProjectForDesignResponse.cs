using System.Text.Json.Serialization;
using KPCOS.BusinessLayer.DTOs.Response.Users;
using KPCOS.Common.Constants;

namespace KPCOS.BusinessLayer.DTOs.Response.Projects;

public class GetAllProjectForDesignResponse : ProjectForListResponse
{
    public bool StandOut { get; set; }
    public string ImageUrl { get; set; } = ImageConstant.BlankImageUrl;
}