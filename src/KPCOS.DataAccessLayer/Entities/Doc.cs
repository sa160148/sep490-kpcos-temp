using System;
using System.Collections.Generic;

namespace KPCOS.DataAccessLayer.Entities;

public partial class Doc
{
    public Guid Id { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public bool? IsActive { get; set; }

    public string Name { get; set; } = null!;

    public string Url { get; set; } = null!;

    public string Type { get; set; } = null!;

    public Guid ProjectId { get; set; }

    public virtual Project Project { get; set; } = null!;

    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}
