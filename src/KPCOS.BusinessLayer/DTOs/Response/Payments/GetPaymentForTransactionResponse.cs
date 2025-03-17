using System;
using KPCOS.BusinessLayer.DTOs.Response.Contracts;

namespace KPCOS.BusinessLayer.DTOs.Response.Payments;

public class GetPaymentForTransactionResponse : GetPaymentResponse
{
    public GetContractForPaymentBatchResponse Contract { get; set; }
}
