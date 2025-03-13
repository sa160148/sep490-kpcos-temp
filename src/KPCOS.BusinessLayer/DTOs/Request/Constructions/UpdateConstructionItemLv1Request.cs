using System;

namespace KPCOS.BusinessLayer.DTOs.Request.Constructions;

public class UpdateConstructionItemLv1Request
{
    public string? Name { get; set; }
    public string? Description { get; set; }

    public List<CreateConstructionItemRequest>? Childs { get; set; }
}