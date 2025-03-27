using System;

namespace KPCOS.DataAccessLayer.Entities;

public class Feedback
{
    public Guid Id { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public bool? IsActive { get; set; }

    public string? Name { get; set; }

    public string? Description { get; set; }

    public string? ImageUrl { get; set; }

    public int? Rating { get; set; }

    public string? Type { get; set; }

    public Guid? CustomerId { get; set; }

    /// <summary>
    /// Loose foreign key relationship with, it can be project id or maintenance request id
    /// </summary>
    public Guid? No { get; set; }

    public virtual Customer? Customer { get; set; }
}
