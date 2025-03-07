namespace KPCOS.DataAccessLayer.Entities;

public partial class PaymentBatch
{
    public Guid Id { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public bool? IsActive { get; set; }

    public string Name { get; set; } = null!;

    public decimal TotalValue { get; set; }

    public bool? IsPaid { get; set; }

    public Guid ContractId { get; set; }

    public Guid? ConstructionItemId { get; set; }

    public string? PaymentPhase { get; set; }

    public int? Percents { get; set; }

    public string? Status { get; set; }

    public virtual Contract Contract { get; set; } = null!;
}
