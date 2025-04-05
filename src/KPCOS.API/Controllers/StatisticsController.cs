using KPCOS.BusinessLayer.DTOs.Request.Maintenances;
using KPCOS.BusinessLayer.DTOs.Request.Payments;
using KPCOS.BusinessLayer.DTOs.Request.Projects;
using KPCOS.BusinessLayer.DTOs.Request.Statistics;
using KPCOS.BusinessLayer.DTOs.Request.Users;
using KPCOS.BusinessLayer.DTOs.Response.Payments;
using KPCOS.BusinessLayer.DTOs.Response.Statistics;
using KPCOS.BusinessLayer.DTOs.Response.Users;
using KPCOS.BusinessLayer.Services;
using KPCOS.Common;
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
            var statistics = await _statisticsService.GetProjectAndMaintenanceStatisticsAsync(request);
            return new PagedApiResponse<GetStatisticsResponse>(
                statistics.data,
                request.PageNumber,
                request.PageSize,
                statistics.totalRecords
            );
        }

        [HttpGet("transactions")]
        [SwaggerOperation(
            Summary = "Lấy thống kê số lượng và tổng tiền của các giao dịch",
            Description = "Lấy thống kê số lượng và tổng tiền của các giao dịch",
            OperationId = "GetTransactionsStatistics",
            Tags = new[] { "Statistics" }
        )]
        public async Task<PagedApiResponse<GetStatisticsResponse>> GetTransactionsAsync(
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

        [HttpGet("transactions-total")]
        [SwaggerOperation(
            Summary = "Lấy thống kê tổng số tiền của các giao dịch",
            Description = "Lấy thống kê tổng số tiền của các giao dịch",
            OperationId = "GetTotalTransactionsStatistics",
            Tags = new[] { "Statistics" }
        )]
        public async Task<PagedApiResponse<GetStatisticsResponse>> GetTotalTransactionsAsync(
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
        
        /// <summary>
        /// Gets monthly statistics for finished projects and completed maintenance requests
        /// </summary>
        /// <param name="request">Filter criteria including years to get statistics for</param>
        /// <returns>Monthly statistics grouped by year</returns>
        [HttpGet("project-and-maintenance")]
        [ProducesResponseType(typeof(ApiResult<GetStatisticsListResponse>), StatusCodes.Status200OK)]
        [SwaggerOperation(
            Summary = "Gets monthly statistics for finished projects and completed maintenance requests",
            Description = "Returns monthly statistics for finished projects and completed maintenance requests grouped by year. " +
                         "For the current year, only shows months up to the current month. " +
                         "For past years, shows all 12 months. " +
                         "Each month shows the count of finished projects and completed maintenance requests.",
            OperationId = "GetProjectAndMaintenanceStatistics",
            Tags = new[] { "Statistics" }
        )]
        public async Task<ApiResult<GetStatisticsListResponse>> GetProjectAndMaintenanceStatisticsAsync(
            [FromQuery] GetStatisticFilterRequest request)
        {
            var (data, totalRecords) = await _statisticsService.GetProjectAndMaintenanceStatisticsAsync(request);
            return new ApiResult<GetStatisticsListResponse>(
                true,
                ApiResultStatusCode.Success,
                new GetStatisticsListResponse
                {
                    Data = data,
                    TotalRecords = totalRecords
                }
            );
        }

        /// <summary>
        /// Gets total monthly statistics combining both finished projects and completed maintenance requests
        /// </summary>
        /// <param name="request">Filter criteria including years to get statistics for</param>
        /// <returns>Total monthly statistics grouped by year</returns>
        [HttpGet("total-project-and-maintenance")]
        [ProducesResponseType(typeof(ApiResult<GetStatisticsListResponse>), StatusCodes.Status200OK)]
        [SwaggerOperation(
            Summary = "Gets total monthly statistics for finished projects and completed maintenance requests",
            Description = "Returns total monthly statistics combining both finished projects and completed maintenance requests grouped by year. " +
                         "For the current year, only shows months up to the current month. " +
                         "For past years, shows all 12 months. " +
                         "Each month shows the total count of finished projects and completed maintenance requests.",
            OperationId = "GetTotalProjectAndMaintenanceStatistics",
            Tags = new[] { "Statistics" }
        )]
        public async Task<ApiResult<GetStatisticsListResponse>> GetTotalProjectAndMaintenanceStatisticsAsync(
            [FromQuery] GetStatisticFilterRequest request)
        {
            var (data, totalRecords) = await _statisticsService.GetTotalProjectAndMaintenanceStatisticsAsync(request);
            return new ApiResult<GetStatisticsListResponse>(
                true,
                ApiResultStatusCode.Success,
                new GetStatisticsListResponse
                {
                    Data = data,
                    TotalRecords = totalRecords
                }
            );
        }

        /// <summary>
        /// [DEPRECATED] Gets monthly transaction statistics for specified years
        /// </summary>
        /// <param name="request">Filter criteria including years to get statistics for</param>
        /// <returns>Monthly transaction statistics grouped by year</returns>
        [Obsolete("This endpoint is deprecated. Use GetProjectAndMaintenanceStatisticsAsync instead.")]
        [HttpGet("transactions-old")]
        [ProducesResponseType(typeof(ApiResult<GetStatisticsListResponse>), StatusCodes.Status200OK)]
        [SwaggerOperation(
            Summary = "[DEPRECATED] Gets monthly transaction statistics",
            Description = "This endpoint is deprecated. Use GetProjectAndMaintenanceStatisticsAsync instead.",
            OperationId = "GetStatisticsOld",
            Tags = new[] { "Statistics" }
        )]
        public async Task<ApiResult<GetStatisticsListResponse>> GetStatisticsOldAsync(
            [FromQuery] GetStatisticFilterRequest request)
        {
            var (data, totalRecords) = await _statisticsService.GetStatisticsAsync(request);
            return new ApiResult<GetStatisticsListResponse>(
                true,
                ApiResultStatusCode.Success,
                new GetStatisticsListResponse
                {
                    Data = data,
                    TotalRecords = totalRecords
                }
            );
        }

        /// <summary>
        /// [DEPRECATED] Gets total monthly transaction statistics combining both construction and maintenance transactions
        /// </summary>
        /// <param name="request">Filter criteria including years to get statistics for</param>
        /// <returns>Total monthly transaction statistics grouped by year</returns>
        [Obsolete("This endpoint is deprecated. Use GetTotalProjectAndMaintenanceStatisticsAsync instead.")]
        [HttpGet("total-transactions-old")]
        [ProducesResponseType(typeof(ApiResult<GetStatisticsListResponse>), StatusCodes.Status200OK)]
        [SwaggerOperation(
            Summary = "[DEPRECATED] Gets total monthly transaction statistics",
            Description = "This endpoint is deprecated. Use GetTotalProjectAndMaintenanceStatisticsAsync instead.",
            OperationId = "GetTotalTransactionStatisticsOld",
            Tags = new[] { "Statistics" }
        )]
        public async Task<ApiResult<GetStatisticsListResponse>> GetTotalTransactionStatisticsOldAsync(
            [FromQuery] GetStatisticFilterRequest request)
        {
            var (data, totalRecords) = await _statisticsService.GetTotalTransactionStatisticsAsync(request);
            return new ApiResult<GetStatisticsListResponse>(
                true,
                ApiResultStatusCode.Success,
                new GetStatisticsListResponse
                {
                    Data = data,
                    TotalRecords = totalRecords
                }
            );
        }

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
