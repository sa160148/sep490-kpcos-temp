using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace KPCOS.BusinessLayer.DTOs.Request.Contracts;

public class ContractRequest
{
    // [Required(ErrorMessage = "Can not let ProjectId null")]
    public Guid ProjectId { get; set; }

    // [Required(ErrorMessage = "Can not let QuotationId null")]
    public Guid QuotationId { get; set; }
    
    [DefaultValue("Contract 1")]
    public string? Name { get; set; }
    
    [DefaultValue("Customer 1")]
    public string? CustomerName { get; set; }

    [DefaultValue(0)]
    public int? ContractValue { get; set; }

    // [Required(ErrorMessage = "Can not let Url null")]
    public string Url { get; set; }

    public string? Note { get; set; }
    
    public string? Code { get; set; }
}