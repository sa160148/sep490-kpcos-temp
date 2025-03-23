using KPCOS.BusinessLayer.DTOs.Request.Docs;
using KPCOS.BusinessLayer.DTOs.Response.Docs;
using KPCOS.BusinessLayer.Services;
using KPCOS.WebFramework.Api;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace KPCOS.API.Controllers
{
    [Route("api/[controller]")]
    public class DocsController : BaseController
    {
        private readonly IDocService _docService;

        public DocsController(IDocService docService)
        {
            _docService = docService;
        }
        
        [HttpPost]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status500InternalServerError)]
        public async Task<ApiResult> CreateDocAsync(
            [FromBody]
            CommandDocRequest request)
        {
            await _docService.CreateDocAsync(request);
            return Ok();
        }

        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status500InternalServerError)]
        public async Task<ApiResult> UpdateDocAsync(
            [SwaggerParameter(
                Description = "Id của tài liệu",
                Required = true
            )]
            Guid id,
            [FromBody]
            CommandDocRequest request)
        {
            await _docService.UpdateDocAsync(id, request);
            return Ok();
        }

        [HttpPut("{id}/accept")]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(
            Summary = "Duyệt/Chấp nhận tài liệu",
            Description = "Duyệt/Chấp nhận tài liệu và tạo mã OTP",
            OperationId = "AcceptDocAsync",
            Tags = new[] { "Docs" }
        )]
        public async Task<ApiResult> AcceptDocAsync(
            [SwaggerParameter(
                Description = "Id của tài liệu",
                Required = true
            )]
            Guid id)
        {
            await _docService.AcceptDocAsync(id);
            return Ok();
        }

        [HttpPut("{id}/verify")]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(
            Summary = "Xác thực mã OTP",
            Description = "Xác thực mã OTP của tài liệu",
            OperationId = "VerifyDocAsync",
            Tags = new[] { "Docs" }
        )]
        public async Task<ApiResult> VerifyDocAsync(
            [SwaggerParameter(
                Description = "Id của tài liệu",
                Required = true
            )]
            Guid id, 
            [FromBody]
            string otp)
        {
            await _docService.VerifyDocAsync(id, otp);
            return Ok();
        }
    }
}