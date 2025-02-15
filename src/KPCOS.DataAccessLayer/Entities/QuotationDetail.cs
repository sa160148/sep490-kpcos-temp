namespace KPCOS.DataAccessLayer.Entities;

public partial class QuotationDetail
{
    public Guid Id { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public bool? IsActive { get; set; }

    public int Quantity { get; set; }

    public int Price { get; set; }

    public string? Note { get; set; }

    public Guid QuotationId { get; set; }

    public Guid ServiceId { get; set; }

    public virtual Quotation Quotation { get; set; } = null!;

    public virtual Service Service { get; set; } = null!;
}
