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

    /// <summary>
    /// Gets detailed information about a specific construction task by its ID
    /// </summary>
    /// <param name="id">The unique identifier of the construction task</param>
    /// <returns>Detailed information about the construction task</returns>
    /// <remarks>
    /// This method retrieves a specific construction task by its ID and returns detailed information including:
    /// - Basic task properties (ID, name, reason, status, image URL)
    /// - Associated construction item ID
    /// - Deadline and timestamps (creation and last update)
    /// - Associated staff information
    /// 
    /// If the task with the specified ID does not exist, a NotFoundException is thrown.
    /// </remarks>
    /// <exception cref="NotFoundException">Thrown when the construction task with the specified ID is not found</exception>
    Task<GetConstructionTaskDetailResponse> GetConstructionTaskDetailByIdAsync(Guid id);

    /// <summary>
    /// Gets detailed information about a specific construction item by its ID
    /// </summary>
    /// <param name="id">The unique identifier of the construction item</param>
    /// <returns>Detailed information about the construction item</returns>
    /// <remarks>
    /// This method retrieves a specific construction item by its ID and returns detailed information including:
    /// - Basic item properties (ID, name, description, status, etc.)
    /// - Associated project ID
    /// - Creation and update timestamps
    /// 
    /// The behavior differs based on whether the item is a parent (level 1) or child (level 2):
    /// - For parent items: Includes child items in the Childs property, Parent property is null
    /// - For child items: Includes parent item information in the Parent property
    /// 
    /// Associated construction tasks are included for both parent and child items.
    /// 
    /// If the item with the specified ID does not exist, a NotFoundException is thrown.
    /// </remarks>
    /// <exception cref="NotFoundException">Thrown when the construction item with the specified ID is not found</exception>
    Task<GetConstructionItemDetailResponse> GetConstructionItemDetailByIdAsync(Guid id);

    /// <summary>
    /// Creates new construction tasks for a specific level 2 (child) construction item
    /// </summary>
    /// <param name="request">List of tasks to create</param>
    /// <param name="id">ID of the construction item</param>
    /// <returns>A task representing the asynchronous operation</returns>
    /// <remarks>
    /// This method creates new construction tasks for a specified level 2 (child) construction item:
    /// - Validates that the construction item exists and is a level 2 (child) item
    /// - Ensures task names are unique within the construction item
    /// - All tasks are created with status OPENING
    /// - Handles deadline dates with proper time zone conversion for PostgreSQL compatibility
    /// - Changes the status of the level 2 (child) construction item from OPENING to PROCESSING
    /// - If the parent (level 1) construction item has status OPENING, changes it to PROCESSING as well
    /// 
    /// Validation rules:
    /// - Construction item ID must be valid
    /// - Construction item must be a level 2 (child) item
    /// - Task name is required for each task
    /// - Task names must be unique within the construction item
    /// - Task names must be unique within the current request batch
    /// </remarks>
    /// <exception cref="BadRequestException">
    /// Thrown when:
    /// - Construction item ID is invalid
    /// - Construction item is not a level 2 (child) item
    /// - Task name is missing
    /// - Task name is duplicated in the request
    /// - Task with the same name already exists in the construction item
    /// </exception>
    /// <exception cref="NotFoundException">Thrown when the construction item with the specified ID is not found</exception>
    Task CreateConstructionTaskAsync(List<CreateConstructionTaskRequest> request, Guid id);
}