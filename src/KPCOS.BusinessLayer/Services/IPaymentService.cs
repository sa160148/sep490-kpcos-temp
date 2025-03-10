using System;
using KPCOS.BusinessLayer.DTOs.Request.Payments;
using KPCOS.BusinessLayer.DTOs.Response.Payments;

namespace KPCOS.BusinessLayer.Services;

public interface IPaymentService
{
    Task<CreateTransactionPaymentResponse> CreateTransactionPaymentAsync(CreatePaymentRequest request);
}
