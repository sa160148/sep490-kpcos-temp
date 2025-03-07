namespace KPCOS.DataAccessLayer.Entities;

public partial class Equipment
{
    public Guid Id { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public bool? IsActive { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public virtual ICollection<QuotationEquipment> QuotationEquipments { get; set; } = new List<QuotationEquipment>();
}
