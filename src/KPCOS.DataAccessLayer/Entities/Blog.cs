using System;

namespace KPCOS.DataAccessLayer.Entities;

public class Blog
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? Type { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool? IsActive { get; set; }
    public Guid? StaffId { get; set; }
    public Staff? Staff { get; set; }
    public Guid? No { get; set; }
}
