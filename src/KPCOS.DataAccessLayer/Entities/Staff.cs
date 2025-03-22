namespace KPCOS.DataAccessLayer.Entities;

public partial class Staff
{
    public Guid Id { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public bool? IsActive { get; set; }

    public string Position { get; set; } = null!;

    public Guid UserId { get; set; }

    public virtual ICollection<ConstructionTask> ConstructionTasks { get; set; } = new List<ConstructionTask>();

    public virtual ICollection<Design> Designs { get; set; } = new List<Design>();

    public virtual ICollection<MaintenanceRequestTask> MaintenanceRequestTasks { get; set; } = new List<MaintenanceRequestTask>();

    public virtual ICollection<ProjectStaff> ProjectStaffs { get; set; } = new List<ProjectStaff>();

    public virtual User User { get; set; } = null!;

    public virtual ICollection<ProjectIssue> ProjectIssues { get; set; } = new List<ProjectIssue>();
}
