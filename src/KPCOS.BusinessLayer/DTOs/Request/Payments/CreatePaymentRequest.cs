using System.ComponentModel.DataAnnotations;

namespace KPCOS.BusinessLayer.DTOs.Request.Payments;

public class CreatePaymentRequest
{
    public Guid? BatchPaymentId { get; set; }
    public Guid? MaintenanceRequestId { get; set; }
    public string ReturnUrl { get; set; }
}