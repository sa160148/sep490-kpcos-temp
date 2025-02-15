namespace KPCOS.BusinessLayer.DTOs.Request;



public class PackageRequest
{
}

public class PackageCreateRequest
{
    public string Name { get; set; }
    public string Description { get; set; }
    public int Price { get; set; }
    public List<PackageItem> Items { get; set; }
    
    public class PackageItem
    {
        public Guid IdPackageItem { get; set; }
        public int? Quantity { get; set; }
        public string? Description { get; set; }
    }
}