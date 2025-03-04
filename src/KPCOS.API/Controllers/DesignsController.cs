using System.Security.Claims;
using KPCOS.BusinessLayer.DTOs.Request.Designs;
using KPCOS.BusinessLayer.DTOs.Response.Designs;
using KPCOS.BusinessLayer.Services;
using KPCOS.Common.Exceptions;
using KPCOS.WebFramework.Api;
using Microsoft.AspNetCore.Mvc;

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
    
}