using KPCOS.BusinessLayer.DTOs.Request.Designs;

namespace KPCOS.BusinessLayer.Services;

public interface IDesignService
{
    /// <summary>
    /// Creates a new design with associated design images
    /// </summary>
    /// <param name="designerId">The ID of the user (designer) creating the design</param>
    /// <param name="request">The design creation request containing project and image information</param>
    /// <returns>A task representing the asynchronous operation</returns>
    Task CreateDesignAsync(Guid designerId,CreateDesignRequest request);

    /// <summary>
    /// Rejects a design and updates its status to REJECTED
    /// </summary>
    /// <param name="id">The ID of the design to reject</param>
    /// <param name="request">The rejection request containing the reason for rejection</param>
    /// <returns>A task representing the asynchronous operation</returns>
    /// <exception cref="NotFoundException">Thrown when the design is not found</exception>
    Task RejectDesignAsync(Guid id, RejectDesignRequest request);

    /// <summary>
    /// Accepts a design and updates its status based on the user's role
    /// </summary>
    /// <param name="id">The ID of the design to accept</param>
    /// <param name="role">The role of the user accepting the design (MANAGER or CUSTOMER)</param>
    /// <returns>A task representing the asynchronous operation</returns>
    /// <exception cref="NotFoundException">Thrown when the design is not found</exception>
    /// <remarks>
    /// For managers: Changes design status to PREVIEWING
    /// For customers: Changes design status to CONFIRMED and updates project status to CONSTRUCTING
    /// </remarks>
    Task AcceptDesignAsync(Guid id, string role);

    /// <summary>
    /// Requests an edit for a design and updates its status to EDITING
    /// </summary>
    /// <param name="id">The ID of the design to edit</param>
    /// <param name="request">The request containing the reason for the edit</param>
    /// <returns>A task representing the asynchronous operation</returns>
    /// <exception cref="NotFoundException">Thrown when the design is not found</exception>
    Task EditDesignAsync(Guid id, RejectDesignRequest request);

    /// <summary>
    /// Updates an existing design by creating a new version with the updated information
    /// </summary>
    /// <param name="id">The ID of the design to update</param>
    /// <param name="userId">The ID of the user (designer) updating the design</param>
    /// <param name="request">The update request containing the new design information</param>
    /// <returns>A task representing the asynchronous operation</returns>
    /// <exception cref="NotFoundException">Thrown when the design is not found</exception>
    Task UpdateDesignAsync(Guid id, Guid userId, UpdateDesignRequest request);
}