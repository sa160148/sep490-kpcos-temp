using KPCOS.BusinessLayer.DTOs.Request.Payments;
using KPCOS.BusinessLayer.DTOs.Response.Payments;
using KPCOS.BusinessLayer.Services;
using KPCOS.WebFramework.Api;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace KPCOS.API.Controllers
{
    [Route("api/[controller]")]
    public class PaymentsController : BaseController
    {
        private readonly IPaymentService _paymentService;
        public PaymentsController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        [HttpPost]
        public async Task<ApiResult<string>> CreateTransactionPaymentAsync(CreatePaymentRequest request)
        {
            var response = await _paymentService.CreateTransactionPaymentAsync(request);
            return Ok(response.VnpayUrl);
        }

        [HttpGet("vnpay-callback")]
        public async Task<ApiResult<string>> PaymentVnpayCallback([FromQuery] VnpayCallbackRequest request)
        {
            return Ok();
        }
    }
}
