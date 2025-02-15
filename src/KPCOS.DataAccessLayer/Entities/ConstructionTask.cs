namespace KPCOS.DataAccessLayer.Entities;

public partial class ConstructionTask
{
    public Guid Id { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public bool? IsActive { get; set; }

    public string Name { get; set; } = null!;

    public Guid Idconstructionitem { get; set; }

    public string? ImageUrl { get; set; }

    public string? Reason { get; set; }

    public Guid? Idstaff { get; set; }

    public string Status { get; set; } = null!;

    public virtual ConstructionItem IdconstructionitemNavigation { get; set; } = null!;

    public virtual Staff? IdstaffNavigation { get; set; }
}
