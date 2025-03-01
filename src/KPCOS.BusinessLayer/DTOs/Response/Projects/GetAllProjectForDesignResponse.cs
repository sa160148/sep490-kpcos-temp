namespace KPCOS.BusinessLayer.DTOs.Response.Projects;

public class GetAllProjectForDesignResponse : ProjectForListResponse
{
    public bool StandOut { get; set; }
    public string ImageUrl { get; set; } = "https://upload.wikimedia.org/wikipedia/commons/a/a7/Blank_image.jpg";
}