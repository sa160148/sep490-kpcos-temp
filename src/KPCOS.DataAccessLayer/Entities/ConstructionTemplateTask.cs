using System;
using System.Collections.Generic;

namespace KPCOS.DataAccessLayer.Entities;

public partial class ConstructionTemplateTask
{
    public Guid Id { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public bool? IsActive { get; set; }

    public string Name { get; set; } = null!;

    public Guid Idtemplateitem { get; set; }

    public string? Status { get; set; }

    public virtual ConstructionTemplateItem IdtemplateitemNavigation { get; set; } = null!;
}
