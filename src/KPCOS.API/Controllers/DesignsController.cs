using System.Security.Claims;
using KPCOS.BusinessLayer.DTOs.Request.Designs;
using KPCOS.BusinessLayer.DTOs.Response.Designs;
using KPCOS.BusinessLayer.Services;
using KPCOS.Common.Exceptions;
using KPCOS.WebFramework.Api;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace KPCOS.API.Controllers;

/// <summary>
/// Controller for managing design-related operations
/// </summary>
[Route("api/[controller]")]
public class DesignsController(IDesignService service): BaseController
{
    /// <summary>
    /// Creates a new design with associated images
    /// </summary>
    /// <param name="request">The design creation request containing project and image information
    /// <para><see cref="CreateDesignRequest"/> request object: </para>
    ///
    /// projectId: guid,
    /// type: string(3D, 2D),
    /// designImages: (list) [ imageUrl: string, ]
    /// </param>
    /// <returns>Success response if the design is created successfully</returns>
    /// <remarks>
    /// <para>It required to login as Designer to get staff id</para>
    /// <para>The design version is automatically incremented based on existing designs:</para>
    /// <list type="bullet">
    /// <item><description>Finds the highest version number for designs with the same project ID and type</description></item>
    /// <item><description>Increments that version by 1 for the new design</description></item>
    /// <item><description>2D and 3D designs have separate version numbering</description></item>
    /// </list>
    /// </remarks>
    /// <response code="200">Design created successfully</response>
    /// <response code="400">If the user is not logged in as a designer</response>
    /// <response code="401">If the user is not authenticated</response>
    [ProducesResponseType(typeof(ApiResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResult), StatusCodes.Status500InternalServerError)]
    [HttpPost]
    // [CustomAuthorize("DESIGNER")]
    public async Task<ApiResult> CreateDesign(CreateDesignRequest request)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null)
        {
            throw new BadRequestException("Vui lòng đăng nhập với designer");
        }
        var userId = Guid.Parse(userIdClaim.Value);
        await service.CreateDesignAsync(userId, request);
        return Ok();
    }

    [HttpGet("{id}")]
    public async Task<ApiResult<GetDesignDetailResponse>> GetDesignDetail(Guid id)
    {
        var result = await service.GetDesignDetailAsync(id);
        return result;
    }
    
    /// <summary>
    /// Rejects a design with a specified reason
    /// </summary>
    /// <param name="id">The ID of the design to reject</param>
    /// <param name="request">The rejection request containing the reason.
    /// <para><see cref="RejectDesignRequest"/> request object: </para>
    /// 
    /// reason: string
    /// </param>
    /// <returns>Success response if the design is rejected successfully</returns>
    /// <remarks>
    /// Only MANAGER can reject.
    /// </remarks>
    /// <response code="200">Design rejected successfully</response>
    /// <response code="500">If the design is not found</response>
    /// <response code="401">If the user is not authenticated as a manager</response>
    [ProducesResponseType(typeof(ApiResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResult), StatusCodes.Status500InternalServerError)]
    [HttpPut("{id}/reject")]
    // [CustomAuthorize("MANAGER")]
    public async Task<ApiResult> RejectDesign(Guid id, RejectDesignRequest request)
    {
        await service.RejectDesignAsync(id, request);
        return Ok();
    }
    
    /// <summary>
    /// Accepts a design and updates its status based on the user's role
    /// </summary>
    /// <param name="id">The ID of the design to accept</param>
    /// <returns>Success response if the design is accepted successfully</returns>
    /// <remarks>
    /// For MANAGER: Changes design status to PREVIEWING.
    /// <para>For CUSTOMER: Changes design status to CONFIRMED and updates project status to CONSTRUCTING.</para>
    /// </remarks>
    /// <response code="200">Design accepted successfully</response>
    /// <response code="500">Internal Error</response>
    /// <response code="401">If the user is not authenticated</response>
    [ProducesResponseType(typeof(ApiResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResult), StatusCodes.Status500InternalServerError)]
    [HttpGet("{id}/accept")]
    // [CustomAuthorize("MANAGER", "CUSTOMER")]
    public async Task<ApiResult> AcceptDesign(Guid id)
    {
        var roleClaim = User.FindFirst(ClaimTypes.Role);
        if (roleClaim == null)
        {
            throw new BadRequestException("Vui lòng đăng nhập với manager hoặc customer");
        }
        var role = roleClaim.Value;
        await service.AcceptDesignAsync(id, role);
        return Ok();
    }
    
    /// <summary>
    /// Requests an edit for a design with a specified reason
    /// </summary>
    /// <param name="id">The ID of the design to edit</param>
    /// <param name="request">The edit request containing the reason.
    /// <para><see cref="RejectDesignRequest"/> request object: </para>
    /// 
    /// reason: string
    /// </param>
    /// <returns>Success response if the edit request is processed successfully</returns>
    /// <remarks>
    /// <para>Only CUSTOMER do this, change status of desgin to editing also with the reason.</para>
    /// </remarks>
    /// <response code="200">Design edit requested successfully</response>
    /// <response code="500">Internal Error</response>
    /// <response code="401">If the user is not authenticated as a customer</response>
    [ProducesResponseType(typeof(ApiResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResult), StatusCodes.Status500InternalServerError)]
    [HttpPut("{id}/request-edit")]
    // [CustomAuthorize("CUSTOMER")]
    public async Task<ApiResult> RequestEditDesign(Guid id, RejectDesignRequest request)
    {
        await service.EditDesignAsync(id, request);
        return Ok();
    }
    
    /// <summary>
    /// Updates an existing design with new information and creates a new version
    /// </summary>
    /// <param name="id">The ID of the design to update</param>
    /// <param name="request">The update request containing new design information.
    /// <para><see cref="UpdateDesignRequest"/> request object: </para>
    ///
    /// projectId: guid,
    /// type: string(3D, 2D),
    /// designImages: (list) [ imageUrl: string, ]
    /// </param>
    /// <returns>Success response if the design is updated successfully</returns>
    /// <remarks>
    /// <para>Only DESIGNER do this, this will make a clone version that plus of old one that from id, the request for cloning.</para>
    /// </remarks>
    /// <response code="200">Design updated successfully</response>
    /// <response code="500">Internal Error</response>
    /// <response code="401">If the user is not authenticated</response>
    [ProducesResponseType(typeof(ApiResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResult), StatusCodes.Status500InternalServerError)]
    [HttpPut("{id}")]
    // [CustomAuthorize("DESIGNER")]
    public async Task<ApiResult> UpdateDesign(Guid id, UpdateDesignRequest request)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null)
        {
            throw new BadRequestException("Vui lòng đăng nhập với designer");
        }
        var userId = Guid.Parse(userIdClaim.Value);
        await service.UpdateDesignAsync(id, userId, request);
        return Ok();
    }
    
    /// <summary>
    /// Xuất bản một thiết kế làm showroom/mẫu
    /// </summary>
    /// <param name="id">ID của thiết kế cần xuất bản</param>
    /// <returns>Phản hồi thành công nếu thiết kế được xuất bản thành công</returns>
    /// <remarks>
    /// <para>Chỉ ADMINISTRATOR mới có thể thực hiện thao tác này</para>
    /// <para>Thiết kế phải thỏa mãn các điều kiện sau:</para>
    /// <list type="bullet">
    /// <item><description>Phải là thiết kế 3D</description></item>
    /// <item><description>Phải ở trạng thái CONFIRMED (đã xác nhận)</description></item>
    /// </list>
    /// </remarks>
    /// <response code="200">Thiết kế được xuất bản thành công</response>
    /// <response code="400">Nếu thiết kế không phải 3D hoặc chưa được xác nhận</response>
    /// <response code="404">Nếu không tìm thấy thiết kế</response>
    /// <response code="401">Nếu người dùng không được xác thực</response>
    [HttpPut("{id}/publish")]
    [ProducesResponseType(typeof(ApiResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResult), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResult), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResult), StatusCodes.Status401Unauthorized)]
    // [CustomAuthorize("ADMINISTRATOR")]
    public async Task<ApiResult> PublishDesignAsync(
        [SwaggerParameter("ID của thiết kế cần xuất bản")] Guid id)
    {
        await service.PublishDesignAsync(id);
        return Ok();
    }
}