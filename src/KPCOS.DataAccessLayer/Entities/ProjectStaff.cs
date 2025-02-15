using System;
using System.Collections.Generic;

namespace KPCOS.DataAccessLayer.Entities;

public partial class ProjectStaff
{
    public Guid Id { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public Guid ProjectId { get; set; }

    public Guid StaffId { get; set; }

    public virtual Project Project { get; set; } = null!;

    public virtual Staff Staff { get; set; } = null!;
}
