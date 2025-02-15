namespace KPCOS.DataAccessLayer.Entities;

public partial class Design : BaseEntity
{
    public Guid Id { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public bool? IsActive { get; set; }

    public int Version { get; set; }

    public string? Reason { get; set; }

    public string Status { get; set; } = null!;

    public bool? IsPublic { get; set; }

    public Guid ProjectId { get; set; }

    public Guid StaffId { get; set; }

    public string Type { get; set; } = null!;

    public virtual ICollection<DesignImage> DesignImages { get; set; } = new List<DesignImage>();

    public virtual Project Project { get; set; } = null!;

    public virtual Staff Staff { get; set; } = null!;
}
