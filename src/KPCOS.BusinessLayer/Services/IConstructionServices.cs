using KPCOS.BusinessLayer.DTOs.Request;
using KPCOS.BusinessLayer.DTOs.Request.Constructions;

namespace KPCOS.BusinessLayer.Services;

public interface IConstructionServices
{
    Task CreateConstructionAsync(ConstructionRequest request);
    
    /// <summary>
    /// Creates or updates construction items for a project with improved structure and validation
    /// </summary>
    /// <param name="request">The construction creation request containing project ID and construction items</param>
    /// <remarks>
    /// This method:
    /// - Validates that the project exists
    /// - Ensures no more than 3 parent items have payment status
    /// - Supports a 2-level hierarchy (parent and child items)
    /// - Removes any existing construction items for the project
    /// - Creates new construction items based on templates or custom definitions
    /// </remarks>
    /// <returns>A task representing the asynchronous operation</returns>
    /// <exception cref="NotFoundException">Thrown when the project or template item is not found</exception>
    /// <exception cref="BadRequestException">Thrown when more than 3 parent items have payment status</exception>
    Task CreateConstructionV2Async(CreateConstructionRequest request);
    
}