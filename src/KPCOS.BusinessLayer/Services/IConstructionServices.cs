using KPCOS.BusinessLayer.DTOs.Request;
using KPCOS.BusinessLayer.DTOs.Request.Constructions;
using KPCOS.BusinessLayer.DTOs.Response.Constructions;

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
    /// - All items are created with status OPENING
    /// - Only parent (level 1) items can have isPayment=true
    /// - Child (level 2) items always have isPayment=false
    /// - Removes any existing construction items for the project
    /// - Creates new construction items based on templates or custom definitions
    /// - When templateItemId is provided, name and description are taken from the template
    /// - When templateItemId is null, custom name and description are used
    /// </remarks>
    /// <returns>A task representing the asynchronous operation</returns>
    /// <exception cref="NotFoundException">Thrown when the project or template item is not found</exception>
    /// <exception cref="BadRequestException">Thrown when more than 3 parent items have payment status</exception>
    Task CreateConstructionV2Async(CreateConstructionRequest request);
    
    /// <summary>
    /// Gets a paginated list of construction items with their children
    /// </summary>
    /// <param name="filter">Filter criteria for construction items</param>
    /// <param name="projectId">Optional project ID to filter items by project</param>
    /// <returns>A tuple containing the list of construction items and the total count</returns>
    /// <remarks>
    /// This method retrieves construction items based on the provided filter criteria.
    /// Parent items are returned with their child items populated in the Childs property.
    /// If projectId is provided, only items for that project will be returned.
    /// 
    /// Available status values for filtering:
    /// - OPENING: Initial status for new construction items
    /// - PROCESSING: Construction items that are currently in progress
    /// - DONE: Completed construction items
    /// 
    /// IsChild filter behavior:
    /// - When IsChild=true: Returns only child items (items with a parent)
    /// - When IsChild=false: Returns only parent items (items without a parent) with their children
    /// - When IsChild is not specified: Returns parent items with their children (default behavior)
    /// </remarks>
    Task<(IEnumerable<GetAllConstructionItemResponse> data, int total)> GetAllConstructionItemsAsync(GetAllConstructionItemFilterRequest filter, Guid? projectId = null);

    /// <summary>
    /// Gets a paginated list of construction tasks based on filter criteria
    /// </summary>
    /// <param name="filter">Filter criteria for construction tasks</param>
    /// <returns>A tuple containing the list of construction tasks and the total count</returns>
    /// <remarks>
    /// This method retrieves construction tasks based on the provided filter criteria.
    /// 
    /// Available filter options:
    /// - Search: Filters tasks by name containing the search term
    /// - IsActive: Filters tasks by their active status
    /// - Status: Filters tasks by their status (e.g., "OPENING", "PROCESSING", "DONE")
    /// - IsOverdue: When true, returns tasks with deadlines in the past that are not marked as DONE
    ///             When false, returns tasks that are not overdue or are marked as DONE
    /// - ConstructionItemId: Filters tasks by their associated construction item
    /// 
    /// Tasks are returned with their associated Staff information.
    /// </remarks>
    Task<(IEnumerable<GetAllConstructionTaskResponse> data, int total)> GetAllConstructionTaskAsync(GetAllConstructionTaskFilterRequest filter);
}