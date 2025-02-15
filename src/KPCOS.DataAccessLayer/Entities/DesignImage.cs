using System;
using System.Collections.Generic;

namespace KPCOS.DataAccessLayer.Entities;

public partial class DesignImage
{
    public Guid Id { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public bool? IsActive { get; set; }

    public string ImageUrl { get; set; } = null!;

    public Guid DesignId { get; set; }

    public virtual Design Design { get; set; } = null!;
}
