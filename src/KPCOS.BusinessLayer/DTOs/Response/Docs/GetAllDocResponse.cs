using System;

namespace KPCOS.BusinessLayer.DTOs.Response.Docs;

public class GetAllDocResponse
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public string? Url { get; set; }
    public GetDocTypeResponse? DocType { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
