namespace KPCOS.DataAccessLayer.Entities;

public partial class PackageItem
{
    public Guid Id { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public bool? IsActive { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<PackageDetail> PackageDetails { get; set; } = new List<PackageDetail>();
}
