namespace KPCOS.DataAccessLayer.Entities;

public partial class QuotationEquipment
{
    public Guid Id { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public bool? IsActive { get; set; }

    public int Quantity { get; set; }

    public int Price { get; set; }

    public string? Note { get; set; }

    public string Category { get; set; } = null!;

    public Guid QuotationId { get; set; }

    public Guid EquipmentId { get; set; }

    public virtual Equipment Equipment { get; set; } = null!;

    public virtual Quotation Quotation { get; set; } = null!;
}
