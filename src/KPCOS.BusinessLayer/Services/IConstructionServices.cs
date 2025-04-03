using KPCOS.BusinessLayer.DTOs.Request;
using KPCOS.BusinessLayer.DTOs.Request.Constructions;
using KPCOS.BusinessLayer.DTOs.Response.Constructions;

namespace KPCOS.BusinessLayer.Services;

public interface IConstructionServices
{
    Task CreateConstructionAsync(ConstructionRequest request);
    
    /// <summary>
    /// Creates construction items for a project with improved structure and validation
    /// </summary>
    /// <param name="request">The construction creation request containing project ID and construction items</param>
    /// <remarks>
    /// This method:
    /// - Validates that the project exists
    /// - Ensures exactly 3 parent items have payment status
    /// - Supports a 2-level hierarchy (parent and child items)
    /// - All items are created with status OPENING
    /// - Only parent (level 1) items can have isPayment=true and category field
    /// - Child (level 2) items always have isPayment=false and category=null
    /// - Checks if construction items already exist for the project and throws BadRequestException if they do
    /// - Creates new construction items based on templates or custom definitions
    /// - When templateItemId is provided:
    ///   - Name and description are taken from the template
    ///   - For level 1 items, category is taken from the template
    ///   - EstimateAt is still taken from the request
    /// - When templateItemId is null, custom name, description and category are used
    /// </remarks>
    /// <returns>A task representing the asynchronous operation</returns>
    /// <exception cref="NotFoundException">Thrown when the project or template item is not found</exception>
    /// <exception cref="BadRequestException">
    /// Thrown when:
    /// - The project already has construction items
    /// - Exactly 3 parent items must have payment status
    /// </exception>
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
    ///             (Deadline comparison uses Southeast Asia time zone for consistency)
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
    /// - Changes the status of the level 2 (child) construction item from OPENING or DONE to PROCESSING
    /// - If the parent (level 1) construction item has status OPENING or DONE, changes it to PROCESSING as well
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
    
    /// <summary>
    /// Updates a level 1 (parent) construction item and optionally adds new child items
    /// </summary>
    /// <param name="request">The request containing name, description, and optional child items to add</param>
    /// <param name="id">ID of the level 1 (parent) construction item to update</param>
    /// <returns>A task representing the asynchronous operation</returns>
    /// <remarks>
    /// This method:
    /// - Updates only the name and description of a level 1 (parent) construction item if provided
    /// - Validates that the name is unique among level 1 items in the same project
    /// - Adds new child items if the Childs collection is not empty or null
    /// - Validates that each child item has a name and the name is unique among existing child items and within the request
    /// - All new child items are created with status OPENING
    /// - For level 2 (child) items, name, description, and estimateAt are used from the request
    /// - For level 2 (child) items, is_payment is always set to false regardless of request
    /// - For level 2 (child) items, template_item_id and childs properties are ignored
    /// - CreatedAt, UpdatedAt, and IsActive fields are handled automatically by the database
    /// - Does not modify existing child items
    /// </remarks>
    /// <exception cref="BadRequestException">
    /// Thrown when:
    /// - The construction item is not a level 1 (parent) item
    /// - A level 1 item with the same name already exists in the project
    /// - A child item name is missing
    /// - A child item name is duplicated in the request
    /// - A child item name already exists among the existing child items
    /// </exception>
    /// <exception cref="NotFoundException">Thrown when the construction item with the specified ID is not found</exception>
    Task UpdateConstructionItemLv1Async(CreateConstructionItemRequest request, Guid id);

    /// <summary>
    /// Updates a level 2 (child) construction item and optionally creates new construction tasks
    /// </summary>
    /// <param name="request">The request containing name, description, and optional construction tasks to create</param>
    /// <param name="id">ID of the level 2 (child) construction item to update</param>
    /// <returns>A task representing the asynchronous operation</returns>
    /// <remarks>
    /// This method:
    /// - Updates only the name and description of a level 2 (child) construction item if provided
    /// - Validates that the construction item is a level 2 (child) item with a parent ID
    /// - Creates new construction tasks if the ConstructionTasks collection is not empty or null
    /// - Validates that each construction task has a unique name within the construction item
    /// - All new construction tasks are created with status OPENING
    /// - Handles deadline dates with proper time zone conversion for PostgreSQL compatibility
    /// - If the construction item has status OPENING or DONE, it will be changed to PROCESSING when adding new tasks
    /// - If the parent construction item has status OPENING or DONE, it will also be changed to PROCESSING
    /// - Does not modify existing construction tasks
    /// </remarks>
    /// <exception cref="BadRequestException">
    /// Thrown when:
    /// - The construction item is not a level 2 (child) item
    /// - A construction task name is missing
    /// - A construction task name is duplicated in the request
    /// - A construction task name already exists in the construction item
    /// </exception>
    /// <exception cref="NotFoundException">Thrown when the construction item with the specified ID is not found</exception>
    Task UpdateConstructionItemLv2Async(UpdateConstructionItemLv2Request request, Guid id);

    /// <summary>
    /// Updates a construction task with the provided information
    /// </summary>
    /// <param name="request">The request containing updated task information</param>
    /// <param name="id">ID of the construction task to update</param>
    /// <returns>A task representing the asynchronous operation</returns>
    /// <remarks>
    /// This method:
    /// - Updates the construction task with the provided information
    /// - Validates that the task name is unique within the construction item if changed
    /// - Changes task status based on specific field updates:
    ///   - When assigning a staff: Changes status from OPENING to PROCESSING
    ///   - When updating image URL: Changes status from PROCESSING to PREVIEWING
    ///   - When updating reason: Changes status from PREVIEWING to PROCESSING
    /// - Validates that the assigned staff is part of the project staff
    /// - Validates that the assigned staff has the position "constructor"
    /// - Prevents updating reason when image URL is null
    /// - Ignores null fields in the request (keeps existing values)
    /// </remarks>
    /// <exception cref="BadRequestException">
    /// Thrown when:
    /// - The task name is already used by another task in the same construction item
    /// - The assigned staff is not part of the project staff
    /// - The assigned staff does not have the position "constructor"
    /// - Attempting to update reason when image URL is null
    /// </exception>
    /// <exception cref="NotFoundException">Thrown when the construction task with the specified ID is not found</exception>
    Task UpdateConstructionTaskAsync(UpdateConstructionTaskRequest request, Guid id);

    /// <summary>
    /// Permanently deletes a construction task from the system
    /// </summary>
    /// <param name="id">ID of the construction task to delete</param>
    /// <returns>A task representing the asynchronous operation</returns>
    /// <remarks>
    /// This method:
    /// - Validates that the construction task exists
    /// - Checks if the task is in OPENING status (only OPENING tasks can be deleted)
    /// - Permanently removes the task from the database
    /// </remarks>
    /// <exception cref="NotFoundException">Thrown when the construction task with the specified ID is not found</exception>
    /// <exception cref="BadRequestException">Thrown when the task is not in OPENING status or is assigned to a staff member</exception>
    Task DeleteConstructionTaskAsync(Guid id);

    /// <summary>
    /// Permanently deletes a construction item from the system
    /// </summary>
    /// <param name="id">ID of the construction item to delete</param>
    /// <returns>A task representing the asynchronous operation</returns>
    /// <remarks>
    /// This method:
    /// - Validates that the construction item exists
    /// - Checks if the item is in OPENING status (only OPENING items can be deleted)
    /// - Ensures the item does not have any child items (construction item lv2)
    /// - Ensures the item does not have isPayment set to true
    /// - Permanently removes the item from the database
    /// </remarks>
    /// <exception cref="NotFoundException">Thrown when the construction item with the specified ID is not found</exception>
    /// <exception cref="BadRequestException">
    /// Thrown when:
    /// - The item is not in OPENING status
    /// - The item has child items
    /// - The item has isPayment set to true
    /// </exception>
    Task DeleteConstructionItemAsync(Guid id);

    /// <summary>
    /// Confirms a construction task and updates related construction items
    /// </summary>
    /// <param name="id">ID of the construction task to confirm</param>
    /// <returns>A task representing the asynchronous operation</returns>
    /// <remarks>
    /// This method:
    /// - Validates that the construction task exists
    /// - Checks if the task is in PREVIEWING status (only PREVIEWING tasks can be confirmed)
    /// - Ensures the task has an image URL (not null)
    /// - Changes the task status to DONE
    /// - If all tasks in a construction item level 2 (child) are DONE, updates the construction item level 2 status to DONE and sets actualAt to current time
    /// - If all construction items level 2 (children) of a construction item level 1 (parent) are DONE, updates the construction item level 1 status to DONE and sets actualAt to current time
    /// </remarks>
    /// <exception cref="NotFoundException">Thrown when the construction task with the specified ID is not found</exception>
    /// <exception cref="BadRequestException">
    /// Thrown when:
    /// - The task is not in PREVIEWING status
    /// - The task does not have an image URL
    /// </exception>
    Task ConfirmConstructionTaskAsync(Guid id);

    /// <summary>
    /// Creates a new construction item level 2 (child) for a specified parent
    /// </summary>
    /// <param name="request">The request containing information for the new construction item</param>
    /// <param name="id">ID of the parent construction item (level 1)</param>
    /// <returns>A task representing the asynchronous operation</returns>
    /// <remarks>
    /// This method:
    /// - Validates that the parent construction item exists and is a level 1 (parent) item
    /// - Ensures the name is not empty and is unique among siblings (other children of the same parent)
    /// - Ensures the estimate date is provided
    /// - Creates a new construction item level 2 (child) with status OPENING
    /// - IsPayment is always set to false for child items
    /// </remarks>
    /// <exception cref="NotFoundException">Thrown when the parent construction item with the specified ID is not found</exception>
    /// <exception cref="BadRequestException">
    /// Thrown when:
    /// - The parent ID is not a level 1 (parent) construction item
    /// - The name is empty
    /// - The estimate date is not provided
    /// - A child item with the same name already exists under the parent
    /// </exception>
    Task CreateConstructionItemLv2Async(CreateConstructionItemRequest request, Guid id);
}