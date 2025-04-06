using System.Security.Claims;
using KPCOS.BusinessLayer.DTOs.Request.Maintenances;
using KPCOS.BusinessLayer.DTOs.Request.Payments;
using KPCOS.BusinessLayer.DTOs.Request.Projects;
using KPCOS.BusinessLayer.DTOs.Response.Payments;
using KPCOS.BusinessLayer.DTOs.Response.Projects;
using KPCOS.BusinessLayer.Services;
using KPCOS.WebFramework.Api;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

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

        /// <summary>
        /// Creates a payment transaction and returns the VNPAY payment URL
        /// </summary>
        /// <remarks>
        /// This endpoint integrates with VNPAY payment gateway.
        /// For more information and test accounts, see: https://sandbox.vnpayment.vn/apis/vnpay-demo/
        /// 
        /// Test accounts available:
        /// - Bank: NCB, Card number: 9704198526191432198, Name: NGUYEN VAN A, Issue date: 07/15, OTP: 123456 (Success)
        /// - Bank: NCB, Card number: 9704195798459170488, Name: NGUYEN VAN A, Issue date: 07/15 (Insufficient balance)
        /// - Bank: NCB, Card number: 9704192181368742, Name: NGUYEN VAN A, Issue date: 07/15 (Inactive card)
        /// 
        /// Important notes:
        /// - The returnUrl parameter should be a URL from your frontend application where the user will be redirected after payment
        /// - The payment session expires after 3 minutes (180 seconds)
        /// - If the user leaves the payment page or doesn't complete payment within 3 minutes, VNPAY will call the callback API with a failed status
        /// - After payment processing, the user will be redirected to your returnUrl with additional parameters:
        ///   * Success: https://yourapp.com/payment-result?success=true&amp;transactionId=00000000-0000-0000-0000-000000000000
        ///   * Failure: https://yourapp.com/payment-result?success=failed&amp;code=24
        /// 
        /// Sample request:
        /// 
        ///     POST /api/payments
        ///     {
        ///        "batchPaymentId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        ///        "returnUrl": "https://yourapp.com/payment-result"
        ///     }
        /// </remarks>
        /// <param name="request">Payment request containing batch payment ID and return URL</param>
        /// <returns>VNPAY payment URL for redirection</returns>
        [HttpPost]
        [SwaggerOperation(
            Summary = "Creates a payment transaction with VNPAY",
            Description = "Generates a payment URL for VNPAY gateway and returns it for client redirection",
            OperationId = "CreatePayment",
            Tags = new[] { "Payments" }
        )]
        [SwaggerResponse(200, "Returns the VNPAY payment URL for redirection", typeof(ApiResult<string>))]
        [SwaggerResponse(400, "Invalid request data", typeof(ApiResult))]
        [SwaggerResponse(404, "Payment batch not found", typeof(ApiResult))]
        public async Task<ApiResult<string>> CreateTransactionPaymentAsync(CreatePaymentRequest request)
        {
            var response = await _paymentService.CreateTransactionPaymentAsync(request);
            return Ok(response.VnpayUrl);
        }

        /// <summary>
        /// Handles the callback from VNPAY after payment completion
        /// </summary>
        /// <remarks>
        /// This endpoint is automatically called by the VNPAY payment gateway after payment processing.
        /// It should not be called directly by clients.
        /// 
        /// For more information about VNPAY integration: https://sandbox.vnpayment.vn/apis/vnpay-demo/
        /// 
        /// The callback process:
        /// 1. VNPAY processes the payment and automatically calls this endpoint with the result
        /// 2. This endpoint verifies the payment status
        /// 3. Updates the payment batch status in the database
        /// 4. Creates transaction and document records
        /// 5. Redirects the user's browser to the appropriate result page
        /// 
        /// Return URL format examples:
        /// - Success: https://yourapp.com/payment-result?success=true&amp;transactionId=00000000-0000-0000-0000-000000000000
        /// - Failure: https://yourapp.com/payment-result?success=failed&amp;code=24
        /// - Batch not found: https://yourapp.com/payment-result?success=failed&amp;message=PaymentBatchNotFound
        /// </remarks>
        /// <param name="request">Callback request from VNPAY containing payment result</param>
        /// <returns>Redirects to the client application with success or failure status</returns>
        [HttpGet("vnpay-callback")]
        [SwaggerOperation(
            Summary = "VNPAY payment callback endpoint",
            Description = "Receives payment result from VNPAY and processes the payment completion",
            OperationId = "PaymentCallback",
            Tags = new[] { "Payments" }
        )]
        [SwaggerResponse(302, "Redirects to the client application with success or failure status")]
        public async Task<IActionResult> PaymentVnpayCallback([FromQuery] VnpayCallbackRequest request)
        {
            var redirectUrl = await _paymentService.PaymentVnpayCallback(request);
            return Redirect(redirectUrl);
        }

        [HttpGet("{id}")]
        [SwaggerOperation(
            Summary = "Get detailed information about a payment transaction",
            Description = "Retrieves comprehensive details about a specific payment transaction including associated payment batch, contract, project, maintenance request, or document information depending on the transaction type.",
            OperationId = "GetPaymentDetail",
            Tags = new[] { "Payments" }
        )]
        [SwaggerResponse(200, "Returns the payment transaction with all related entity details", typeof(ApiResult<GetTransactionDetailResponse>))]
        [SwaggerResponse(404, "Transaction not found", typeof(ApiResult))]
        public async Task<ApiResult<GetTransactionDetailResponse>> GetPaymentDetailAsync(Guid id)
        {
            var response = await _paymentService.GetPaymentDetailAsync(id);
            return Ok(response);
        }

        [HttpGet("transaction")]
        [SwaggerOperation(
            Summary = "Lấy danh sách giao dịch thanh toán",
            Description = "Lấy danh sách giao dịch thanh toán với các bộ lọc và phân trang. Hỗ trợ lọc theo loại giao dịch, số tiền, trạng thái và người dùng.\n\n" +
                        "Quy tắc truy cập:\n" +
                        "- ADMINISTRATOR: Có thể xem tất cả giao dịch\n" +
                        "- CUSTOMER: Chỉ xem được các giao dịch của mình\n" +
                        "- STAFF: Có thể xem tất cả giao dịch, nhưng nó là lỗi\n\n" +
                        "Lưu ý:\n" +
                        "- Các trường Role và UserId **được tự động thiết lập** dựa trên người dùng đang đăng nhập\n" +
                        "- **Không cần nhập giá trị cho các trường Role và UserId**",
            OperationId = "GetAllTransactions",
            Tags = new[] { "Payments" }
        )]
        [SwaggerResponse(200, "Danh sách giao dịch thanh toán", typeof(PagedApiResponse<GetTransactionDetailResponse>))]
        [SwaggerResponse(400, "Yêu cầu không hợp lệ", typeof(ApiResult))]
        [SwaggerResponse(401, "Chưa xác thực", typeof(ApiResult))]
        [SwaggerResponse(403, "Không có quyền truy cập", typeof(ApiResult))]
        public async Task<PagedApiResponse<GetTransactionDetailResponse>> GetTransactionsAsync(
           [FromQuery]
           [SwaggerParameter(
                Description = "Bộ lọc giao dịch thanh toán:\n" +
                            "- AmountMin: Số tiền tối thiểu\n" +
                            "- AmountMax: Số tiền tối đa\n" +
                            "- Type: Loại giao dịch (PAYMENT_BATCH, MAINTENANCE_REQUEST, DOC)\n" +
                            "- Status: Trạng thái giao dịch (SUCCESSFUL, FAILED)\n" +
                            "- FromAmount: Số tiền bắt đầu khoảng tìm kiếm\n" +
                            "- ToAmount: Số tiền kết thúc khoảng tìm kiếm\n" +
                            "- PageNumber: Số trang\n" +
                            "- PageSize: Số phần tử trên mỗi trang\n\n" +
                            "Lưu ý:\n" +
                            "- Role và UserId được tự động thiết lập, không cần nhập\n" +
                            "- Quyền truy cập được kiểm soát dựa trên vai trò người dùng",
                Required = false
            )]
            GetAllTransactionFilterRequest request
        )
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var roleClaim = User.FindFirst(ClaimTypes.Role)?.Value;
            (IEnumerable<GetTransactionDetailResponse> data, int total) transactions;
            if (userIdClaim != null && roleClaim != null)
            {
                var userId = Guid.Parse(userIdClaim);
                request.Role = roleClaim;
                request.UserId = userId;
            }
            
            transactions = await _paymentService.GetTransactionsAsync(request);
            return new PagedApiResponse<GetTransactionDetailResponse>(
                transactions.data, 
                request.PageNumber, 
                request.PageSize, 
                transactions.total);
        }
    }
}
