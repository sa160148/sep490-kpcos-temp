namespace KPCOS.DataAccessLayer.DTOs.Response;

public class ServiceReponse
{
    public Guid? Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public int? Price { get; set; }
    public string? Unit { get; set; }
    public string? Type { get; set; }
}