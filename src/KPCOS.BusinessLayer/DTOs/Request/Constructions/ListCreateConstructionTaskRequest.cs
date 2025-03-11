using System.ComponentModel.DataAnnotations;

namespace KPCOS.BusinessLayer.DTOs.Request.Constructions;

public class ListCreateConstructionTaskRequest
{
    [Required]
    public Guid? ConstructionItemId { get; set; }
    public List<CreateConstructionTaskRequest> ConstructionTasks { get; set; } = new List<CreateConstructionTaskRequest>();
}