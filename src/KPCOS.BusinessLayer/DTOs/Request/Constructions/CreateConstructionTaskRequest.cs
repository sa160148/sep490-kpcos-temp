using System;
using System.ComponentModel.DataAnnotations;

namespace KPCOS.BusinessLayer.DTOs.Request.Constructions;
public class CreateConstructionTaskRequest
{
    [Required]
    public string Name { get; set; } = default!;
    public DateTime? DeadlineAt { get; set; }
}