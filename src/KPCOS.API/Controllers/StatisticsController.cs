using KPCOS.BusinessLayer.DTOs.Request.Maintenances;
using KPCOS.BusinessLayer.DTOs.Request.Payments;
using KPCOS.BusinessLayer.DTOs.Request.Projects;
using KPCOS.BusinessLayer.DTOs.Request.Statistics;
using KPCOS.BusinessLayer.DTOs.Request.Users;
using KPCOS.BusinessLayer.DTOs.Response.Payments;
using KPCOS.BusinessLayer.DTOs.Response.Statistics;
using KPCOS.BusinessLayer.DTOs.Response.Users;
using KPCOS.BusinessLayer.Services;
using KPCOS.WebFramework.Api;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace KPCOS.API.Controllers
{
    [Route("api/[controller]")]
    public class StatisticsController : BaseController
    {
        private readonly IStatisticsService _statisticsService;

        public StatisticsController(IStatisticsService statisticsService)
        {
            _statisticsService = statisticsService;
        }

        [HttpGet]
        [SwaggerOperation(
            Summary = "Lấy thống kê số lượng và tổng tiền của các dự án",
            Description = "Lấy thống kê số lượng và tổng tiền của các dự án dựa trên các thanh toán từ contract-payment batch",
            OperationId = "GetProjectStatistics",
            Tags = new[] { "Statistics" }
        )]
        public async Task<PagedApiResponse<GetStatisticsResponse>> GetStatisticsAsync(
            [FromQuery]
            [SwaggerParameter(
                Description = "Filter criteria for statistics including Year",
                Required = false
            )]
            GetStatisticFilterRequest request
        )
        {
            var statistics = await _statisticsService.GetStatisticsAsync(request);
            return new PagedApiResponse<GetStatisticsResponse>(
                statistics.data,
                request.PageNumber,
                request.PageSize,
                statistics.totalRecords
            );
        }

        [HttpGet("total-transaction")]
        [SwaggerOperation(
            Summary = "Lấy thống kê tổng số tiền của các giao dịch",
            Description = "Lấy thống kê tổng số tiền của các giao dịch",
            OperationId = "GetTotalTransactionStatistics",
            Tags = new[] { "Statistics" }
        )]
        public async Task<PagedApiResponse<GetStatisticsResponse>> GetTotalTransactionStatisticsAsync(
            [FromQuery]
            [SwaggerParameter(
                Description = "Filter criteria for statistics including Year",
                Required = false
            )]
            GetStatisticFilterRequest request
        )
        {
            var statistics = await _statisticsService.GetTotalTransactionStatisticsAsync(request);
            return new PagedApiResponse<GetStatisticsResponse>(
                statistics.data,
                request.PageNumber,
                request.PageSize,
                statistics.totalRecords
            );
        }

        [HttpGet("transaction-count-growth")]
        [SwaggerOperation(
            Summary = "Lấy tỷ lệ tăng trưởng số lượng giao dịch",
            Description = @"Lấy tỷ lệ tăng trưởng số lượng giao dịch so với năm trước.

Phản hồi bao gồm:
- GrowthRate: Tỷ lệ tăng trưởng (%). Null nếu không thể tính (năm trước không có giao dịch)
- CurrentValue: Số lượng giao dịch năm hiện tại
- PreviousValue: Số lượng giao dịch năm trước
- IsNewActivity: True nếu năm nay có giao dịch mới và năm trước không có

Các trường hợp đặc biệt:
1. Không có giao dịch cả 2 năm: GrowthRate = 0%
2. Năm trước không có, năm nay có: GrowthRate = null, IsNewActivity = true
3. Có dữ liệu cả 2 năm: GrowthRate = ((CurrentValue - PreviousValue) / PreviousValue) * 100

Ví dụ phản hồi:
```json
{
    ""isSuccess"": true,
    ""statusCode"": 200,
    ""data"": {
        ""growthRate"": 25.5,        // hoặc null nếu là hoạt động mới
        ""currentValue"": 125,       // số lượng giao dịch năm nay
        ""previousValue"": 100,      // số lượng giao dịch năm trước
        ""isNewActivity"": false     // true nếu là hoạt động mới
    }
}
```",
            OperationId = "GetTransactionCountGrowthRate",
            Tags = new[] { "Statistics" }
        )]
        [ProducesResponseType(typeof(ApiResult<GetGrowthRateStatisticResponse>), StatusCodes.Status200OK)]
        public async Task<ApiResult<GetGrowthRateStatisticResponse>> GetTransactionCountGrowthRateAsync()
        {
            var response = await _statisticsService.GetTransactionCountGrowthRateAsync();
            return Ok(response);
        }

        [HttpGet("transaction-amount-growth")]
        [SwaggerOperation(
            Summary = "Lấy tỷ lệ tăng trưởng tổng tiền giao dịch",
            Description = @"Lấy tỷ lệ tăng trưởng tổng tiền giao dịch so với năm trước.

Phản hồi bao gồm:
- GrowthRate: Tỷ lệ tăng trưởng (%). Null nếu không thể tính (năm trước không có giao dịch)
- CurrentValue: Tổng tiền giao dịch năm hiện tại (VND)
- PreviousValue: Tổng tiền giao dịch năm trước (VND)
- IsNewActivity: True nếu năm nay có giao dịch mới và năm trước không có

Các trường hợp đặc biệt:
1. Không có giao dịch cả 2 năm: GrowthRate = 0%
2. Năm trước không có, năm nay có: GrowthRate = null, IsNewActivity = true
3. Có dữ liệu cả 2 năm: GrowthRate = ((CurrentValue - PreviousValue) / PreviousValue) * 100

Ví dụ phản hồi:
```json
{
    ""isSuccess"": true,
    ""statusCode"": 200,
    ""data"": {
        ""growthRate"": 33.33,      // hoặc null nếu là hoạt động mới
        ""currentValue"": 4000000,   // tổng tiền năm nay (VND)
        ""previousValue"": 3000000,  // tổng tiền năm trước (VND)
        ""isNewActivity"": false     // true nếu là hoạt động mới
    }
}
```",
            OperationId = "GetTransactionAmountGrowthRate",
            Tags = new[] { "Statistics" }
        )]
        [ProducesResponseType(typeof(ApiResult<GetGrowthRateStatisticResponse>), StatusCodes.Status200OK)]
        public async Task<ApiResult<GetGrowthRateStatisticResponse>> GetTransactionAmountGrowthRateAsync()
        {
            var response = await _statisticsService.GetTransactionAmountGrowthRateAsync();
            return Ok(response);
        }
        
        /*
        [HttpGet("contract")]
        [SwaggerOperation(
            Summary = "Lấy thống kê số lượng và tổng tiền của các dự án",
            Description = "Lấy thống kê số lượng và tổng tiền của các dự án dựa trên các thanh toán từ contract-payment batch",
            OperationId = "GetProjectStatistics",
            Tags = new[] { "Payments" }
        )]
        public async Task<PagedApiResponse<GetStatisticsResponse>> GetProjectStatisticsAsync(
            [FromQuery]
            [SwaggerParameter(
                Description = "Filter criteria for statistics including Year",
                Required = false
            )]
            GetProjectStatisticsFilterRequest request
        )
        {
            var statistics = await _statisticsService.GetProjectStatisticsAsync(request);
            return new PagedApiResponse<GetStatisticsResponse>(
                statistics.data,
                request.PageNumber,
                request.PageSize,
                statistics.total
            );
        }
        
        [HttpGet("maintenance")]
        [SwaggerOperation(
            Summary = "Lấy thống kê số lượng và tổng tiền của các maintenance request",
            Description = "Lấy thống kê số lượng và tổng tiền của các maintenance request dựa trên các thanh toán từ maintenance request",
            OperationId = "GetProjectStatistics",
            Tags = new[] { "Payments" }
        )]
        public async Task<PagedApiResponse<GetStatisticsResponse>> GetMaintenanceStatisticsAsync(
            [FromQuery]
            [SwaggerParameter(
                Description = "Filter criteria for statistics including Year",
                Required = false
            )]
            GetMaintenanceStatisticsFilterRequest request
        )
        {
            var statistics = await _statisticsService.GetMaintenanceStatisticsAsync(request);
            return new PagedApiResponse<GetStatisticsResponse>(
                statistics.data,
                request.PageNumber,
                request.PageSize,
                statistics.total
            );
        }

        [HttpGet("transaction")]
        [SwaggerOperation(
            Summary = "Lấy thống kê số lượng và tổng tiền của các giao dịch",
            Description = "Lấy thống kê số lượng và tổng tiền của các giao dịch",
            OperationId = "GetProjectStatistics",
            Tags = new[] { "Payments" }
        )]
        public async Task<PagedApiResponse<GetStatisticsResponse>> GetTransactionStatisticsAsync(
            [FromQuery]
            [SwaggerParameter(
                Description = "Filter criteria for statistics including Year",
                Required = false
            )]
            GetAllTransactionFilterRequest request
        )
        {
            var statistics = await _statisticsService.GetTransactionStatisticsAsync(request);
            return new PagedApiResponse<GetStatisticsResponse>(
                statistics.data,
                request.PageNumber,
                request.PageSize,
                statistics.total
            );
        }
        */

        [HttpGet("user")]
        [SwaggerOperation(
            Summary = "Lấy thống kê số lượng các user",
            Description = "Lấy thống kê số lượng các user",
            OperationId = "GetUserStatistics",
            Tags = new[] { "Statistics" }
        )]
        public async Task<ApiResult<GetUserStatisticResponse>> GetUserStatisticsAsync()
        {
            var statistics = await _statisticsService.GetUserStatisticsAsync();
            return Ok(statistics);
        }
    }
}
