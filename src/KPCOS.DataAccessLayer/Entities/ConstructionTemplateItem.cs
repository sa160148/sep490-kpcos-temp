namespace KPCOS.DataAccessLayer.Entities;

public partial class ConstructionTemplateItem : BaseEntity
{
    public Guid Id { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public bool? IsActive { get; set; }

    public string Name { get; set; } = null!;

    public string Description { get; set; } = null!;

    public Guid? Idparent { get; set; }

    public Guid Idtemplate { get; set; }

    public string Status { get; set; } = null!;

    public virtual ICollection<ConstructionTemplateTask> ConstructionTemplateTasks { get; set; } = new List<ConstructionTemplateTask>();

    public virtual ConstructionTemplateItem? IdparentNavigation { get; set; }

    public virtual ConstructionTemplate IdtemplateNavigation { get; set; } = null!;

    public virtual ICollection<ConstructionTemplateItem> InverseIdparentNavigation { get; set; } = new List<ConstructionTemplateItem>();
}
