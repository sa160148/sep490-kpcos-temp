namespace KPCOS.DataAccessLayer.Entities;

public partial class ConstructionTemplate : BaseEntity
{
    public Guid Id { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public bool? IsActive { get; set; }

    public string Name { get; set; } = null!;

    public string Description { get; set; } = null!;

    public string? Status { get; set; } = "";

    public virtual ICollection<ConstructionTemplateItem> ConstructionTemplateItems { get; set; } = new List<ConstructionTemplateItem>();

    public virtual ICollection<Quotation> Quotations { get; set; } = new List<Quotation>();
}
