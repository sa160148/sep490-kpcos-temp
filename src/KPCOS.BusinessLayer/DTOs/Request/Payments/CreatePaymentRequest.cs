using System.ComponentModel.DataAnnotations;

namespace KPCOS.BusinessLayer.DTOs.Request.Payments;

public class CreatePaymentRequest
{
    [Required]
    public Guid BatchPaymentId { get; set; }
    [Required]
    public string ReturnUrl { get; set; }
}

