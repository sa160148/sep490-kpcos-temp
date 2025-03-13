using System;
using System.ComponentModel.DataAnnotations;

namespace KPCOS.BusinessLayer.DTOs.Request.Constructions;
public class CreateConstructionTaskRequest
{
    [Required]
    public string Name { get; set; } = default!;
    [Required]
    public DateTime? DeadlineAt { get; set; }
}