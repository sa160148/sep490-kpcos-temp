namespace KPCOS.DataAccessLayer.Entities;

public partial class Transaction : BaseEntity
{
    public Guid Id { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public bool? IsActive { get; set; }

    public Guid CustomerId { get; set; }

    public string Type { get; set; } = null!;

    public Guid No { get; set; }

    public int Amount { get; set; }

    public string? Note { get; set; }

    public Guid? IdDocs { get; set; }

    public string Status { get; set; } = null!;

    public virtual Customer Customer { get; set; } = null!;

    public virtual Doc? IdDocsNavigation { get; set; }
}
