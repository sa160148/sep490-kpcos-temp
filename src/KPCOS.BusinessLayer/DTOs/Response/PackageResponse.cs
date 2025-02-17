namespace KPCOS.BusinessLayer.DTOs.Response;

public class PackageResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public bool? IsActive { get; set; }
    public List<int> Price { get; set; }
    public List<PackageItem> Items { get; set; }
    
    
    public class PackageItem
    {
        public Guid IdPackageItem { get; set; }
        public int? Quantity { get; set; }
        public string? Description { get; set; }
        public string ? Name { get; set; }
    }
}