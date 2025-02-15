namespace KPCOS.DataAccessLayer.Entities;

public partial class ConstructionItem : BaseEntity
{
    public Guid Id { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public bool? IsActive { get; set; }

    public string Name { get; set; } = null!;

    public string Description { get; set; } = null!;

    public DateOnly Estdate { get; set; }

    public DateOnly? Actdate { get; set; }

    public Guid? Idparent { get; set; }

    public Guid Idproject { get; set; }

    public string Status { get; set; } = null!;

    public virtual ICollection<ConstructionTask> ConstructionTasks { get; set; } = new List<ConstructionTask>();

    public virtual Project IdprojectNavigation { get; set; } = null!;
}
