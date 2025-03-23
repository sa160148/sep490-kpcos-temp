using System;

namespace KPCOS.BusinessLayer.DTOs.Request.Docs;

public class CommandDocRequest
{
    public Guid? ProjectId { get; set; }
    public string? Name { get; set; }
    public string? Url { get; set; }
    public Guid? DocTypeId { get; set; }
}
