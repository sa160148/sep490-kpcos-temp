using System;
using System.Collections.Generic;

namespace KPCOS.DataAccessLayer.Entities;

public partial class Promotion
{
    public Guid Id { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public bool? IsActive { get; set; }

    public string Name { get; set; } = null!;

    public string Code { get; set; } = null!;

    public int Discount { get; set; }

    public DateTime Starttime { get; set; }

    public DateTime Exptime { get; set; }

    public string? Status { get; set; }

    public virtual ICollection<Quotation> Quotations { get; set; } = new List<Quotation>();
}
