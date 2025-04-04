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
