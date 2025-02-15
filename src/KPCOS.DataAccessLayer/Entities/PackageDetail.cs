namespace KPCOS.DataAccessLayer.Entities;

public partial class PackageDetail : BaseEntity
{
    public Guid Id { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public bool? IsActive { get; set; }

    public int? Quantity { get; set; }

    public string? Description { get; set; }

    public Guid PackageId { get; set; }

    public Guid PackageItemId { get; set; }

    public virtual Package Package { get; set; } = null!;

    public virtual PackageItem PackageItem { get; set; } = null!;
}
