using System;
using System.Collections.Generic;

namespace KPCOS.DataAccessLayer.Entities;

public partial class Quotation
{
    public Guid Id { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public bool? IsActive { get; set; }

    public Guid ProjectId { get; set; }

    public int Version { get; set; }

    public int TotalPrice { get; set; }

    public string Reason { get; set; } = null!;

    public string? Status { get; set; }

    public Guid? PromotionId { get; set; }

    public Guid Idtemplate { get; set; }

    public virtual ConstructionTemplate IdtemplateNavigation { get; set; } = null!;

    public virtual Project Project { get; set; } = null!;

    public virtual Promotion? Promotion { get; set; }

    public virtual ICollection<QuotationDetail> QuotationDetails { get; set; } = new List<QuotationDetail>();

    public virtual ICollection<QuotationEquipment> QuotationEquipments { get; set; } = new List<QuotationEquipment>();
}
