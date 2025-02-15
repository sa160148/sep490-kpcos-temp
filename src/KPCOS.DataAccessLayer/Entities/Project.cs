namespace KPCOS.DataAccessLayer.Entities;

public partial class Project : BaseEntity
{
    public Guid Id { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public bool? IsActive { get; set; }

    public string Name { get; set; } = null!;

    public string CustomerName { get; set; } = null!;

    public string Address { get; set; } = null!;

    public string Phone { get; set; } = null!;

    public string Email { get; set; } = null!;

    public double Area { get; set; }

    public double Depth { get; set; }

    public Guid PackageId { get; set; }

    public string? Note { get; set; }

    public string Status { get; set; } = null!;

    public Guid CustomerId { get; set; }

    public Guid? Templatedesignid { get; set; }

    public virtual ICollection<ConstructionItem> ConstructionItems { get; set; } = new List<ConstructionItem>();

    public virtual ICollection<Contract> Contracts { get; set; } = new List<Contract>();

    public virtual Customer Customer { get; set; } = null!;

    public virtual ICollection<Design> Designs { get; set; } = new List<Design>();

    public virtual ICollection<Doc> Docs { get; set; } = new List<Doc>();

    public virtual Package Package { get; set; } = null!;

    public virtual ICollection<ProjectStaff> ProjectStaffs { get; set; } = new List<ProjectStaff>();

    public virtual ICollection<Quotation> Quotations { get; set; } = new List<Quotation>();
}
