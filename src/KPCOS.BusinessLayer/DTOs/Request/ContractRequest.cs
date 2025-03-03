namespace KPCOS.BusinessLayer.DTOs.Request;

public class ContractRequest
{
  
}

public class ContractCreateRequest
{
    public string Name {get; set;}
    public string CustomerName {get; set;}
    public int ContractValue {get; set;}
    public string Url {get; set;}
    public string? Note {get; set;}
    public Guid QuotationId {get; set;}
    public Guid ProjectId {get; set;}
}