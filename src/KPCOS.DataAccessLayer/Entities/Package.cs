namespace KPCOS.DataAccessLayer.Entities;

public partial class Package
{
    public Guid Id { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public bool? IsActive { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public int Price { get; set; }

    public int Rate { get; set; }

    public string? Status { get; set; }

    public virtual ICollection<PackageDetail> PackageDetails { get; set; } = new List<PackageDetail>();

    public virtual ICollection<Project> Projects { get; set; } = new List<Project>();
}
