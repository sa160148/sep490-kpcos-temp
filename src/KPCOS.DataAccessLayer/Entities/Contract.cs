namespace KPCOS.DataAccessLayer.Entities;

public partial class Contract
{
    public Guid Id { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public bool? IsActive { get; set; }

    public string Name { get; set; } = null!;

    public string CustomerName { get; set; } = null!;

    public int ContractValue { get; set; }

    public string Url { get; set; } = null!;

    public string? Note { get; set; }

    public Guid QuotationId { get; set; }

    public Guid ProjectId { get; set; }

    public string? Status { get; set; }

    public virtual ICollection<PaymentBatch> PaymentBatches { get; set; } = new List<PaymentBatch>();

    public virtual Project Project { get; set; } = null!;
}
