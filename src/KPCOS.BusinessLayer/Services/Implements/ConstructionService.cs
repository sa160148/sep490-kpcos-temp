using KPCOS.BusinessLayer.DTOs.Request;
using KPCOS.BusinessLayer.DTOs.Request.Constructions;
using KPCOS.BusinessLayer.DTOs.Response.Constructions;
using KPCOS.BusinessLayer.Exceptions;
using KPCOS.Common.Exceptions;
using KPCOS.DataAccessLayer.Entities;
using KPCOS.DataAccessLayer.Enums;
using KPCOS.DataAccessLayer.Repositories;
using AutoMapper;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using LinqKit;
using KPCOS.Common.Utilities;
using System.Linq.Dynamic.Core;

namespace KPCOS.BusinessLayer.Services.Implements;

public class ConstructionService : IConstructionServices
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public ConstructionService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

   public async Task CreateConstructionAsync(ConstructionRequest request)
{
    IRepository<ConstructionItem> constructionItemRepo = _unitOfWork.Repository<ConstructionItem>();
    IRepository<ConstructionTemplateItem> constructionTemplateItemRepo = _unitOfWork.Repository<ConstructionTemplateItem>();
    IRepository<Project> projectRepo = _unitOfWork.Repository<Project>();
    
    var projectRaw = await projectRepo.FindAsync(request.ProjectId);
    if (projectRaw == null)
    {
        throw new NotFoundException("Dự án không tồn tại");
    }
    
    // Validate maximum number of special items
    var specialItemCount = request.Items.Count(x => x.IsPayment);
    if (specialItemCount > 3)
    {
        throw new BadRequestException("Số lượng hạng mục thanh toán không được vượt quá 3");
    }
    
    // Remove all construction items of project
    var constructionItemRaw = constructionItemRepo.Get(x => x.ProjectId == request.ProjectId).ToList();
    foreach (var item in constructionItemRaw)
    {
        await constructionItemRepo.RemoveAsync(item, false);
    }
    
    // Create new construction items
    foreach (var item in request.Items)
    {
        await CreateConstructionItemFromRequestAsync(item, request.ProjectId, null, constructionItemRepo, constructionTemplateItemRepo, true);
    }
    
    await _unitOfWork.SaveChangesAsync();
}

    private async Task CreateConstructionItemFromRequestAsync(
        ConstructionRequest.Item request,
        Guid projectId,
        Guid? parentId,
        IRepository<ConstructionItem> constructionItemRepo,
        IRepository<ConstructionTemplateItem> templateItemRepo,
        bool isParent)
    {
        // All items now have OPENING status
        string status = EnumConstructionItemStatus.OPENING.ToString();
        
        // Only parent items can have isPayment=true, child items always have isPayment=false
        bool isPayment = isParent && request.IsPayment;
        
        ConstructionItem constructionItem;
        
        // Check if we should use template data
        if (request.TemplateItemId != Guid.Empty)
        {
            // Get data from template
            var templateItem = await templateItemRepo.FindAsync(request.TemplateItemId);
            if (templateItem == null)
            {
                throw new NotFoundException($"Mẫu công trình với ID {request.TemplateItemId} không tồn tại");
            }
            
            // Create item using template data but with EstimateAt and IsPayment from request
            constructionItem = new ConstructionItem
            {
                Id = Guid.NewGuid(),
                Name = templateItem.Name,
                Description = templateItem.Description,
                Status = status,
                EstimateAt = request.EstDate,
                IsPayment = isPayment,
                ProjectId = projectId,
                ParentId = parentId,
                IsActive = true,
                CreatedAt = DateTime.Now
            };
        }
        else
        {
            // For custom items without template, we need to provide default values
            // since ConstructionRequest.Item doesn't have Name and Description
            constructionItem = new ConstructionItem
            {
                Id = Guid.NewGuid(),
                Name = $"Construction Item {Guid.NewGuid().ToString().Substring(0, 8)}",
                Description = "Custom construction item",
                Status = status,
                EstimateAt = request.EstDate,
                IsPayment = isPayment,
                ProjectId = projectId,
                ParentId = parentId,
                IsActive = true,
                CreatedAt = DateTime.Now
            };
        }
        
        await constructionItemRepo.AddAsync(constructionItem, false);
        
        // Create child items if this is a parent item
        if (isParent && request.Child?.Any() == true)
        {
            foreach (var child in request.Child)
            {
                await CreateConstructionItemFromRequestAsync(child, projectId, constructionItem.Id, constructionItemRepo, templateItemRepo, false);
            }
        }
    }

    public async Task CreateConstructionV2Async(CreateConstructionRequest request)
    {
        var constructionItemRepo = _unitOfWork.Repository<ConstructionItem>();
        var constructionTemplateItemRepo = _unitOfWork.Repository<ConstructionTemplateItem>();
        var projectRepo = _unitOfWork.Repository<Project>();
        
        // Validate project exists
        var project = await projectRepo.FindAsync(request.ProjectId);
        if (project == null)
        {
            throw new NotFoundException("Dự án không tồn tại");
        }

        // Validate maximum number of special items
        var specialItemCount = request.Items.Count(x => x.IsPayment == true);
        if (specialItemCount != 3)
        {
            throw new BadRequestException("Số lượng hạng mục thanh toán phải bằng 3");
        }

        // Check if construction items already exist for this project
        var existingItems = constructionItemRepo.Get(x => x.ProjectId == request.ProjectId).Any();
        if (existingItems)
        {
            throw new BadRequestException("Dự án này đã có hạng mục xây dựng. Không thể tạo mới.");
        }

        // Create new construction items
        foreach (var item in request.Items)
        {
            await CreateConstructionItemAsync(item, request.ProjectId, null, constructionItemRepo, constructionTemplateItemRepo, isParent: true);
        }

        await _unitOfWork.SaveChangesAsync();
    }

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
    public async Task<(IEnumerable<GetAllConstructionItemResponse> data, int total)> GetAllConstructionItemsAsync(GetAllConstructionItemFilterRequest filter, Guid? projectId = null)
    {
        var constructionItemRepo = _unitOfWork.Repository<ConstructionItem>();
        
        // Create a combined filter expression
        var predicate = PredicateBuilder.New<ConstructionItem>(true);
        
        // Only filter by ParentId if IsChild is not specified
        // If IsChild is specified, the filter will handle it
        if (!filter.IsChild.HasValue)
        {
            predicate = predicate.And(x => x.ParentId == null);
        }
        
        // Add projectId filter if provided
        if (projectId.HasValue)
        {
            predicate = predicate.And(x => x.ProjectId == projectId.Value);
        }
        
        // Combine with filter expressions from the filter object
        predicate = predicate.And(filter.GetExpressions());
        
        // Get the ordering function from the filter
        var orderBy = filter.GetOrder();
        
        // Get parent items with count using the repository's GetWithCount method
        var (items, total) = constructionItemRepo.GetWithCount(
            filter: predicate,
            orderBy: orderBy,
            pageIndex: filter.PageNumber,
            pageSize: filter.PageSize
        );
        
        // Map items to response objects using the injected mapper
        var result = _mapper.Map<List<GetAllConstructionItemResponse>>(items);
        
        // For each item, get and map its children (only if it's a parent item)
        foreach (var item in result)
        {
            if (item.Id.HasValue && (filter.IsChild == null || filter.IsChild == false))
            {
                // Get child items using the repository's Get method with the same ordering as parent items
                var childPredicate = PredicateBuilder.New<ConstructionItem>(x => x.ParentId == item.Id.Value);
                
                // Apply any additional filters from the original filter that should also apply to children
                // For example, we might want to apply Status, IsActive, or Search filters to children as well
                if (!string.IsNullOrWhiteSpace(filter.Status))
                {
                    if (Enum.TryParse<EnumConstructionItemStatus>(filter.Status, out var statusEnum))
                    {
                        childPredicate = childPredicate.And(x => x.Status == statusEnum.ToString());
                    }
                    else
                    {
                        childPredicate = childPredicate.And(x => x.Status == filter.Status);
                    }
                }
                
                if (filter.IsActive.HasValue)
                {
                    childPredicate = childPredicate.And(x => x.IsActive == filter.IsActive.Value);
                }
                
                if (!string.IsNullOrWhiteSpace(filter.Search))
                {
                    childPredicate = childPredicate.And(x => 
                        (x.Name != null && x.Name.Contains(filter.Search)) || 
                        (x.Description != null && x.Description.Contains(filter.Search))
                    );
                }
                
                // Manually get child items from repository to apply proper sorting
                var childItems = constructionItemRepo.Get(filter: childPredicate);
                
                // Get dynamic ordering string from filter
                string sortExpression = $"{filter.SortColumn} {filter.SortDir.ToString().ToLower()}";
                
                // Apply sorting using System.Linq.Dynamic.Core
                var sortedChildItems = System.Linq.Dynamic.Core.DynamicQueryableExtensions
                    .OrderBy(childItems.AsQueryable(), sortExpression)
                    .ToList();
                
                item.Childs = _mapper.Map<List<GetAllConstructionItemChildResponse>>(sortedChildItems);
            }
            else
            {
                // For child items, ensure Childs collection is empty
                item.Childs = new List<GetAllConstructionItemChildResponse>();
            }
        }
        
        return (result, total);
    }

    public async Task<(IEnumerable<GetAllConstructionTaskResponse> data, int total)> GetAllConstructionTaskAsync(GetAllConstructionTaskFilterRequest filter, Guid? userId = null)
    {
        // Get the repository for ConstructionTask
        var constructionTaskRepo = _unitOfWork.Repository<ConstructionTask>();
        
        // Apply the filter expression from the request
        var filterExpression = filter.GetExpressions();
        
        // If userId is provided, check if the user is a constructor and filter by their staff ID
        if (userId.HasValue)
        {
            var (isConstructor, staffId) = await CheckUserIsConstructorAsync(userId.Value);
            
            if (isConstructor && staffId.HasValue)
            {
                // Create a new filter expression that includes the staff ID filter
                Guid staffIdValue = staffId.Value;
                var staffFilter = PredicateBuilder.New<ConstructionTask>(task => task.StaffId == staffIdValue);
                filterExpression = filterExpression.And(staffFilter);
            }
        }
        
        // Get the data with count using the repository's GetWithCount method
        var result = constructionTaskRepo.GetWithCount(
            filter: filterExpression,
            orderBy: filter.GetOrder(),
            includeProperties: "Staff,Staff.User,ConstructionItem",
            pageIndex: filter.PageNumber,
            pageSize: filter.PageSize
        );
        
        // Map the entities to response DTOs
        var mappedResult = _mapper.Map<List<GetAllConstructionTaskResponse>>(result.Data);
        
        return (mappedResult, result.Count);
    }

    public async Task<GetConstructionTaskDetailResponse> GetConstructionTaskDetailByIdAsync(Guid id)
    {
        var constructionTask = _unitOfWork.Repository<ConstructionTask>()
        .Get(filter: x => x.Id == id, includeProperties: "Staff,Staff.User")
        .SingleOrDefault() ?? throw new NotFoundException("Công việc không tồn tại");
        return _mapper.Map<GetConstructionTaskDetailResponse>(constructionTask);
    }

    public async Task<GetConstructionItemDetailResponse> GetConstructionItemDetailByIdAsync(Guid id)
    {
        // Get the construction item by ID
        var constructionItem = _unitOfWork.Repository<ConstructionItem>()
            .Get(
                filter: x => x.Id == id,
                includeProperties: "ConstructionTasks,ConstructionTasks.Staff,ConstructionTasks.Staff.User"
            )
            .SingleOrDefault() ?? throw new NotFoundException("Hạng mục không tồn tại");

        // Map the base construction item to response
        var response = _mapper.Map<GetConstructionItemDetailResponse>(constructionItem);

        // Check if it's a parent (level 1) or child (level 2) item
        if (constructionItem.ParentId.HasValue)
        {
            // It's a child item - include its parent
            var parentItem = await _unitOfWork.Repository<ConstructionItem>()
                .FindAsync(constructionItem.ParentId.Value);
            
            if (parentItem != null)
            {
                response.Parent = _mapper.Map<GetConstructionItemParentDetailResponse>(parentItem);
            }
        }
        else
        {
            // It's a parent item - include its children with their construction tasks
            var childItems = _unitOfWork.Repository<ConstructionItem>()
                .Get(
                    filter: x => x.ParentId == id,
                    includeProperties: "ConstructionTasks,ConstructionTasks.Staff,ConstructionTasks.Staff.User"
                ).ToList();
            
            // Map child items
            var mappedChildItems = new List<GetAllConstructionItemChildResponse>();
            
            foreach (var childItem in childItems)
            {
                // Map the child item
                var mappedChild = _mapper.Map<GetAllConstructionItemChildResponse>(childItem);
                
                // Map construction tasks for the child item
                if (childItem.ConstructionTasks != null && childItem.ConstructionTasks.Any())
                {
                    mappedChild.ConstructionTasks = _mapper.Map<List<GetAllConstructionTaskResponse>>(childItem.ConstructionTasks);
                }
                
                mappedChildItems.Add(mappedChild);
            }
            
            response.Childs = mappedChildItems;
            
            // For parent items, ensure Parent is null (already null by default)
            response.Parent = null;
        }

        // Map construction tasks for the main item
        if (constructionItem.ConstructionTasks != null && constructionItem.ConstructionTasks.Any())
        {
            response.ConstructionTasks = _mapper.Map<List<GetAllConstructionTaskResponse>>(constructionItem.ConstructionTasks);
        }

        return response;
    }

    public async Task CreateConstructionTaskAsync(List<CreateConstructionTaskRequest> request, Guid id)
    {
        // Validate request
        if (id == Guid.Empty)
        {
            throw new BadRequestException("ID của hạng mục xây dựng là bắt buộc");
        }

        // Get repositories
        var constructionItemRepo = _unitOfWork.Repository<ConstructionItem>();
        var constructionTaskRepo = _unitOfWork.Repository<ConstructionTask>();

        // Find the construction item with its parent
        var constructionItems = constructionItemRepo.Get(
            filter: ci => ci.Id == id && ci.IsActive == true,
            includeProperties: "ConstructionTasks"
        );
        
        var constructionItem = constructionItems.FirstOrDefault();
        if (constructionItem == null)
        {
            throw new NotFoundException($"Không tìm thấy hạng mục xây dựng với ID {id}");
        }

        // Validate that the construction item is a level 2 (child) item
        if (!constructionItem.ParentId.HasValue)
        {
            throw new BadRequestException("Chỉ có thể tạo công việc cho hạng mục con (cấp 2)");
        }

        // Get existing task names for this construction item
        var existingTasks = constructionTaskRepo.Where(t => 
            t.ConstructionItemId == id && 
            t.IsActive == true);
            
        // Convert to HashSet for efficient lookups (case-insensitive)
        var existingTaskNames = new HashSet<string>(
            existingTasks.Select(t => t.Name),
            StringComparer.OrdinalIgnoreCase);

        // Keep track of task names we're adding in this batch to prevent duplicates
        var batchTaskNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        
        // Create a list to hold all new tasks
        var newTasks = new List<ConstructionTask>();

        // Process each task request
        foreach (var taskRequest in request)
        {
            // Validate task name
            if (string.IsNullOrWhiteSpace(taskRequest.Name))
            {
                throw new BadRequestException("Tên công việc là bắt buộc");
            }

            // Check for duplicates in the current batch
            if (batchTaskNames.Contains(taskRequest.Name))
            {
                throw new BadRequestException($"Công việc với tên '{taskRequest.Name}' bị trùng lặp trong yêu cầu");
            }

            // Check for existing tasks with the same name
            if (existingTaskNames.Contains(taskRequest.Name))
            {
                throw new BadRequestException($"Công việc với tên '{taskRequest.Name}' đã tồn tại trong hạng mục xây dựng này");
            }

            // Create new task using mapper
            var newTask = _mapper.Map<ConstructionTask>(taskRequest);
            newTask.ConstructionItemId = id;
            
            // Add to our list of new tasks
            newTasks.Add(newTask);
            
            // Add to our batch tracking set
            batchTaskNames.Add(taskRequest.Name);
        }

        // Add all tasks directly to the task repository
        await constructionTaskRepo.AddRangeAsync(newTasks, false);
        
        // Update the status of the level 2 (child) construction item to PROCESSING if it's currently OPENING or DONE
        if (constructionItem.Status == EnumConstructionItemStatus.OPENING.ToString() ||
            constructionItem.Status == EnumConstructionItemStatus.DONE.ToString())
        {
            constructionItem.Status = EnumConstructionItemStatus.PROCESSING.ToString();
            await constructionItemRepo.UpdateAsync(constructionItem, false);
            
            // Update the parent (level 1) construction item status to PROCESSING if it's currently OPENING or DONE
            if (constructionItem.ParentId.HasValue)
            {
                var parentItem = await constructionItemRepo.FindAsync(constructionItem.ParentId.Value);
                if (parentItem != null && 
                    (parentItem.Status == EnumConstructionItemStatus.OPENING.ToString() ||
                     parentItem.Status == EnumConstructionItemStatus.DONE.ToString()))
                {
                    parentItem.Status = EnumConstructionItemStatus.PROCESSING.ToString();
                    await constructionItemRepo.UpdateAsync(parentItem, false);
                }
            }
        }
        
        // Save all changes at once
        await _unitOfWork.SaveChangesAsync();
    }

    private async Task CreateConstructionItemAsync(
        CreateConstructionItemRequest request,
        Guid projectId,
        Guid? parentId,
        IRepository<ConstructionItem> constructionItemRepo,
        IRepository<ConstructionTemplateItem> templateItemRepo,
        bool isParent)
    {
        ConstructionItem constructionItem;

        // All items now have OPENING status
        string status = EnumConstructionItemStatus.OPENING.ToString();
        
        // Only parent items can have isPayment=true, child items always have isPayment=false
        bool isPayment = isParent && request.IsPayment == true;

        if (request.TemplateItemId.HasValue)
        {
            // Create from template
            var templateItem = await templateItemRepo.FindAsync(request.TemplateItemId.Value);
            if (templateItem == null)
            {
                throw new NotFoundException($"Mẫu công trình với ID {request.TemplateItemId} không tồn tại");
            }

            constructionItem = new ConstructionItem
            {
                Id = Guid.NewGuid(),
                Name = templateItem.Name,
                Description = templateItem.Description,
                Status = status,
                EstimateAt = request.EstimateAt!.Value,
                IsPayment = isPayment,
                ProjectId = projectId,
                ParentId = parentId,
                // For template items, use category from template for level 1, null for level 2
                Category = isParent ? templateItem.Category : null
            };
        }
        else
        {
            // Create custom item
            constructionItem = new ConstructionItem
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Description = request.Description,
                Status = status,
                EstimateAt = request.EstimateAt!.Value,
                IsPayment = isPayment,
                ProjectId = projectId,
                ParentId = parentId,
                // Only set category for level 1 items
                Category = isParent ? request.Category : null
            };
        }

        await constructionItemRepo.AddAsync(constructionItem, false);

        // Create child items if this is a parent item
        if (isParent && request.Childs?.Any() == true)
        {
            foreach (var child in request.Childs)
            {
                await CreateConstructionItemAsync(child, projectId, constructionItem.Id, constructionItemRepo, templateItemRepo, isParent: false);
            }
        }
    }

    public async Task UpdateConstructionItemLv1Async(CreateConstructionItemRequest request, Guid id)
    {
        // Get repositories
        var constructionItemRepo = _unitOfWork.Repository<ConstructionItem>();
        
        // Find the construction item
        var constructionItem = await constructionItemRepo.FindAsync(id);
        if (constructionItem == null)
        {
            throw new NotFoundException($"Không tìm thấy hạng mục xây dựng với ID {id}");
        }

        // Validate that the construction item is a level 1 (parent) item
        if (constructionItem.ParentId.HasValue)
        {
            throw new BadRequestException("Chỉ có thể cập nhật hạng mục cha (cấp 1)");
        }

        // Validate unique name for level 1 (parent) item if name is being updated
        if (!string.IsNullOrWhiteSpace(request.Name) && request.Name != constructionItem.Name)
        {
            // Check if there's another level 1 item with the same name in the same project
            var existingItemWithSameName = constructionItemRepo.FirstOrDefault(
                x => x.ProjectId == constructionItem.ProjectId && 
                     x.ParentId == null && 
                     x.Id != constructionItem.Id && 
                     x.Name.ToLower() == request.Name.ToLower() &&
                     x.IsActive == true);
                     
            if (existingItemWithSameName != null)
            {
                throw new BadRequestException($"Đã tồn tại hạng mục cha với tên '{request.Name}' trong dự án này");
            }
        }

        // Update the construction item properties using ReflectionUtil
        // Only include name and description, exclude all other properties
        var excludeProperties = new List<string> 
        { 
            "Id", "Childs", "EstimateAt", "IsPayment", "TemplateItemId", 
            "ProjectId", "ParentId", "Status", "CreatedAt", "UpdatedAt", "IsActive" 
        };
        ReflectionUtil.UpdateProperties(request, constructionItem, excludeProperties);

        // Update the parent item
        await constructionItemRepo.UpdateAsync(constructionItem, false);

        // Add new child items if provided
        if (request.Childs != null && request.Childs.Any())
        {
            // Get existing child items to check for duplicate names
            var existingChildItems = constructionItemRepo.Get(
                x => x.ParentId == constructionItem.Id && x.IsActive == true).ToList();
                
            // Create a HashSet of existing child item names (case-insensitive) for efficient lookups
            var existingChildNames = new HashSet<string>(
                existingChildItems.Select(x => x.Name.ToLower()),
                StringComparer.OrdinalIgnoreCase);
                
            // Create a HashSet to track names in the current batch to prevent duplicates within the request
            var batchChildNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            
            // Create a list to hold all new child items
            var childItems = new List<ConstructionItem>();
            
            foreach (var childRequest in request.Childs)
            {
                // Validate child name is provided
                if (string.IsNullOrWhiteSpace(childRequest.Name))
                {
                    throw new BadRequestException("Tên hạng mục con là bắt buộc");
                }
                
                // Check for duplicate names within the current batch
                if (batchChildNames.Contains(childRequest.Name))
                {
                    throw new BadRequestException($"Hạng mục con với tên '{childRequest.Name}' bị trùng lặp trong yêu cầu");
                }
                
                // Check for duplicate names with existing child items
                if (existingChildNames.Contains(childRequest.Name.ToLower()))
                {
                    throw new BadRequestException($"Đã tồn tại hạng mục con với tên '{childRequest.Name}' trong hạng mục cha này");
                }
                
                // Add to batch tracking set
                batchChildNames.Add(childRequest.Name);
                
                // Create new child item with required properties
                var childItem = new ConstructionItem
                {
                    Name = childRequest.Name, // Name is required and validated above
                    Description = childRequest.Description, // Description from request
                    EstimateAt = childRequest.EstimateAt ?? DateOnly.FromDateTime(DateTime.Now), // EstimateAt from request or default
                    Status = EnumConstructionItemStatus.OPENING.ToString(), // Status is always OPENING for new items
                    IsPayment = false, // Child items always have isPayment=false
                    ProjectId = constructionItem.ProjectId, // Same project as parent
                    ParentId = constructionItem.Id // Parent is the level 1 item
                    // CreatedAt, UpdatedAt, and IsActive are handled automatically by the database
                };
                
                // No need to use ReflectionUtil here since we're explicitly setting only the fields we want
                // and we've already validated the name

                // Add to our list of child items
                childItems.Add(childItem);
            }
            
            // Add all child items at once using AddRangeAsync
            // This only adds new items and doesn't modify existing ones
            await constructionItemRepo.AddRangeAsync(childItems, false);
        }

        // Save all changes
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task UpdateConstructionItemLv2Async(UpdateConstructionItemLv2Request request, Guid id)
    {
        // Get repositories
        var constructionItemRepo = _unitOfWork.Repository<ConstructionItem>();
        var constructionTaskRepo = _unitOfWork.Repository<ConstructionTask>();
        
        // Find the construction item
        var constructionItem = await constructionItemRepo.FindAsync(id);
        if (constructionItem == null)
        {
            throw new NotFoundException($"Không tìm thấy hạng mục xây dựng với ID {id}");
        }

        // Validate that the construction item is a level 2 (child) item
        if (!constructionItem.ParentId.HasValue)
        {
            throw new BadRequestException("Chỉ có thể cập nhật hạng mục con (cấp 2)");
        }

        // Update the construction item properties using ReflectionUtil
        // Only include name and description, exclude all other properties
        var excludeProperties = new List<string> 
        { 
            "Id", "ConstructionTasks", "EstimateAt", "IsPayment", "ProjectId", 
            "ParentId", "Status", "CreatedAt", "UpdatedAt", "IsActive", "ActualAt" 
        };
        ReflectionUtil.UpdateProperties(request, constructionItem, excludeProperties);

        // Update the child item
        await constructionItemRepo.UpdateAsync(constructionItem, false);

        // Add new construction tasks if provided
        if (request.ConstructionTasks != null && request.ConstructionTasks.Any())
        {
            // Get existing task names for this construction item
            var existingTasks = constructionTaskRepo.Where(t => 
                t.ConstructionItemId == id && 
                t.IsActive == true);
                
            // Convert to HashSet for efficient lookups (case-insensitive)
            var existingTaskNames = new HashSet<string>(
                existingTasks.Select(t => t.Name),
                StringComparer.OrdinalIgnoreCase);

            // Keep track of task names we're adding in this batch to prevent duplicates
            var batchTaskNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            
            // Create a list to hold all new tasks
            var newTasks = new List<ConstructionTask>();

            // Process each task request
            foreach (var taskRequest in request.ConstructionTasks)
            {
                // Validate task name
                if (string.IsNullOrWhiteSpace(taskRequest.Name))
                {
                    throw new BadRequestException("Tên công việc là bắt buộc");
                }

                // Check for duplicates in the current batch
                if (batchTaskNames.Contains(taskRequest.Name))
                {
                    throw new BadRequestException($"Công việc với tên '{taskRequest.Name}' bị trùng lặp trong yêu cầu");
                }

                // Check for existing tasks with the same name
                if (existingTaskNames.Contains(taskRequest.Name))
                {
                    throw new BadRequestException($"Công việc với tên '{taskRequest.Name}' đã tồn tại trong hạng mục xây dựng này");
                }

                // Create new task
                var newTask = new ConstructionTask
                {
                    Name = taskRequest.Name,
                    DeadlineAt = GlobalUtility.ConvertToSEATimeForPostgres(taskRequest.DeadlineAt),
                    Status = EnumConstructionTaskStatus.OPENING.ToString(),
                    ConstructionItemId = id,
                    IsActive = true
                };
                
                // Add to our list of new tasks
                newTasks.Add(newTask);
                
                // Add to our batch tracking set
                batchTaskNames.Add(taskRequest.Name);
            }

            // Add all tasks directly to the task repository
            await constructionTaskRepo.AddRangeAsync(newTasks, false);
            
            // Update the status of the level 2 (child) construction item to PROCESSING if it's currently OPENING or DONE
            if (constructionItem.Status == EnumConstructionItemStatus.OPENING.ToString() || 
                constructionItem.Status == EnumConstructionItemStatus.DONE.ToString())
            {
                constructionItem.Status = EnumConstructionItemStatus.PROCESSING.ToString();
                await constructionItemRepo.UpdateAsync(constructionItem, false);
                
                // Update the parent (level 1) construction item status to PROCESSING if it's currently OPENING or DONE
                if (constructionItem.ParentId.HasValue)
                {
                    var parentItem = await constructionItemRepo.FindAsync(constructionItem.ParentId.Value);
                    if (parentItem != null && 
                        (parentItem.Status == EnumConstructionItemStatus.OPENING.ToString() || 
                         parentItem.Status == EnumConstructionItemStatus.DONE.ToString()))
                    {
                        parentItem.Status = EnumConstructionItemStatus.PROCESSING.ToString();
                        await constructionItemRepo.UpdateAsync(parentItem, false);
                    }
                }
            }
        }
        
        // Save all changes at once
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task UpdateConstructionTaskAsync(UpdateConstructionTaskRequest request, Guid id)
    {
        // Get repositories
        var constructionTaskRepo = _unitOfWork.Repository<ConstructionTask>();
        var constructionItemRepo = _unitOfWork.Repository<ConstructionItem>();
        var staffRepo = _unitOfWork.Repository<Staff>();
        var projectStaffRepo = _unitOfWork.Repository<ProjectStaff>();
        
        // Find the construction task with its construction item
        var constructionTask = constructionTaskRepo.Get(
            filter: x => x.Id == id && x.IsActive == true,
            includeProperties: "ConstructionItem"
        ).FirstOrDefault();
        
        if (constructionTask == null)
        {
            throw new NotFoundException($"Không tìm thấy công việc xây dựng với ID {id}");
        }
        
        // Get the construction item
        var constructionItem = constructionTask.ConstructionItem;
        
        // Check for unique name if name is being updated
        if (!string.IsNullOrWhiteSpace(request.Name) && request.Name != constructionTask.Name)
        {
            // Check if there's another task with the same name in the same construction item
            var existingTaskWithSameName = constructionTaskRepo.FirstOrDefault(
                x => x.ConstructionItemId == constructionTask.ConstructionItemId && 
                     x.Id != constructionTask.Id && 
                     x.Name.ToLower() == request.Name.ToLower() &&
                     x.IsActive == true);
                     
            if (existingTaskWithSameName != null)
            {
                throw new BadRequestException($"Đã tồn tại công việc với tên '{request.Name}' trong hạng mục xây dựng này");
            }
            
            // Update the name
            constructionTask.Name = request.Name;
        }
        
        // Update deadline if provided
        if (request.DeadlineAt.HasValue)
        {
            constructionTask.DeadlineAt = GlobalUtility.ConvertToSEATimeForPostgres(request.DeadlineAt.Value);
        }
        
        // Handle staff assignment
        if (request.StaffId.HasValue)
        {
            // Find the staff by UserId (not by Staff.Id)
            var staff = staffRepo.FirstOrDefault(
                x => x.UserId == request.StaffId.Value && x.IsActive == true);
                
            if (staff == null)
            {
                throw new NotFoundException($"Không tìm thấy nhân viên với User ID {request.StaffId.Value}");
            }
            
            // Validate that the staff has the position "constructor"
            if (staff.Position != RoleEnum.CONSTRUCTOR.ToString())
            {
                throw new BadRequestException($"Nhân viên phải có vị trí là 'constructor' để được gán vào công việc xây dựng");
            }
            
            // Get the project ID from the construction item
            var projectId = constructionItem.ProjectId;
            
            // Check if the staff is assigned to the project
            var projectStaff = projectStaffRepo.FirstOrDefault(
                x => x.ProjectId == projectId && x.StaffId == staff.Id);
                
            if (projectStaff == null)
            {
                throw new BadRequestException($"Nhân viên không thuộc dự án này");
            }
            
            // Assign the staff to the task
            constructionTask.StaffId = staff.Id;
            
            // Change status from OPENING to PROCESSING if current status is OPENING
            if (constructionTask.Status == EnumConstructionTaskStatus.OPENING.ToString())
            {
                constructionTask.Status = EnumConstructionTaskStatus.PROCESSING.ToString();
            }
        }
        
        // Handle image URL update
        if (!string.IsNullOrWhiteSpace(request.ImageUrl))
        {
            constructionTask.ImageUrl = request.ImageUrl;
            
            // Change status from PROCESSING to PREVIEWING if current status is PROCESSING
            if (constructionTask.Status == EnumConstructionTaskStatus.PROCESSING.ToString())
            {
                constructionTask.Status = EnumConstructionTaskStatus.PREVIEWING.ToString();
            }
        }
        
        // Handle reason update
        if (!string.IsNullOrWhiteSpace(request.Reason))
        {
            // Cannot update reason while image URL is null
            if (string.IsNullOrWhiteSpace(constructionTask.ImageUrl) && string.IsNullOrWhiteSpace(request.ImageUrl))
            {
                throw new BadRequestException($"Không thể cập nhật lý do khi URL hình ảnh chưa được cung cấp");
            }
            
            constructionTask.Reason = request.Reason;
            
            // Change status from PREVIEWING to PROCESSING if current status is PREVIEWING
            if (constructionTask.Status == EnumConstructionTaskStatus.PREVIEWING.ToString())
            {
                constructionTask.Status = EnumConstructionTaskStatus.PROCESSING.ToString();
            }
        }
        
        // Update the task
        await constructionTaskRepo.UpdateAsync(constructionTask, false);
        
        // Save all changes
        await _unitOfWork.SaveChangesAsync();
    }

    /// <summary>
    /// Checks if a user is a constructor and returns their staff ID if they are
    /// </summary>
    /// <param name="userId">The user ID to check</param>
    /// <returns>A tuple containing whether the user is a constructor and their staff ID if they are</returns>
    private async Task<(bool isConstructor, Guid? staffId)> CheckUserIsConstructorAsync(Guid userId)
    {
        // Get the repository for Staff
        var staffRepo = _unitOfWork.Repository<Staff>();
        
        // Find the staff record for the user
        var staff = await staffRepo.FirstOrDefaultAsync(s => s.UserId == userId && s.IsActive == true);
        
        // If no staff record is found, return false
        if (staff == null)
        {
            return (false, null);
        }
        
        // Check if the staff position is "constructor"
        bool isConstructor = staff.Position == RoleEnum.CONSTRUCTOR.ToString();
        
        return (isConstructor, staff.Id);
    }

    /// <summary>
    /// Permanently deletes a construction task from the system
    /// </summary>
    /// <param name="id">ID of the construction task to delete</param>
    /// <returns>A task representing the asynchronous operation</returns>
    /// <exception cref="NotFoundException">Thrown when the construction task with the specified ID is not found</exception>
    /// <exception cref="BadRequestException">Thrown when the task is not in OPENING status or is assigned to a staff member</exception>
    public async Task DeleteConstructionTaskAsync(Guid id)
    {
        // Get the repository for ConstructionTask
        var constructionTaskRepo = _unitOfWork.Repository<ConstructionTask>();
        
        // Find the construction task by ID
        var constructionTask = await constructionTaskRepo.FindAsync(id);
        
        // If the task doesn't exist, throw NotFoundException
        if (constructionTask == null)
        {
            throw new NotFoundException($"Không tìm thấy công việc xây dựng với ID {id}");
        }
        
        // Check if the task is in OPENING status
        if (constructionTask.Status != EnumConstructionTaskStatus.OPENING.ToString())
        {
            throw new BadRequestException($"Chỉ có thể xóa công việc xây dựng có trạng thái OPENING. Trạng thái hiện tại: {constructionTask.Status}");
        }
        
        // Check if the task is assigned to a staff member
        if (constructionTask.StaffId != null)
        {
            throw new BadRequestException("Không thể xóa công việc xây dựng đang được gán cho nhân viên");
        }
        
        // Delete the construction task
        await constructionTaskRepo.RemoveAsync(constructionTask, false);
        
        // Save changes to the database
        await _unitOfWork.SaveChangesAsync();
    }

    /// <summary>
    /// Permanently deletes a construction item from the system
    /// </summary>
    /// <param name="id">ID of the construction item to delete</param>
    /// <returns>A task representing the asynchronous operation</returns>
    /// <exception cref="NotFoundException">Thrown when the construction item with the specified ID is not found</exception>
    /// <exception cref="BadRequestException">
    /// Thrown when:
    /// - The item is not in OPENING status
    /// - The item has child items
    /// - The item has isPayment set to true
    /// </exception>
    public async Task DeleteConstructionItemAsync(Guid id)
    {
        // Get the repository for ConstructionItem
        var constructionItemRepo = _unitOfWork.Repository<ConstructionItem>();
        
        // Find the construction item by ID
        var constructionItem = await constructionItemRepo.FindAsync( id);
        
        // If the item doesn't exist, throw NotFoundException
        if (constructionItem == null)
        {
            throw new NotFoundException($"Không tìm thấy hạng mục xây dựng với ID {id}");
        }
        
        // Check if the item is in OPENING status
        if (constructionItem.Status != EnumConstructionItemStatus.OPENING.ToString())
        {
            throw new BadRequestException($"Chỉ có thể xóa hạng mục xây dựng có trạng thái OPENING. Trạng thái hiện tại: {constructionItem.Status}");
        }
        
        // Check if the item has isPayment set to true
        if (constructionItem.IsPayment == true)
        {
            throw new BadRequestException("Không thể xóa hạng mục xây dựng có trạng thái thanh toán (isPayment = true)");
        }
        
        // Check if the item has any child items (level 2)
        var hasChildItems = await constructionItemRepo.SingleOrDefaultAsync(i => i.ParentId == id) != null;
        if (hasChildItems)
        {
            throw new BadRequestException("Không thể xóa hạng mục xây dựng có hạng mục con (cấp 2)");
        }
        
        // Check if the item has any associated construction tasks
        var constructionTaskRepo = _unitOfWork.Repository<ConstructionTask>();
        var hasConstructionTasks = await constructionTaskRepo.SingleOrDefaultAsync(t => t.ConstructionItemId == id) != null;
        if (hasConstructionTasks)
        {
            throw new BadRequestException("Không thể xóa hạng mục xây dựng có công việc xây dựng liên quan");
        }
        
        // Delete the construction item
        await constructionItemRepo.RemoveAsync(constructionItem, false);
        
        // Save changes to the database
        await _unitOfWork.SaveChangesAsync();
    }

    /// <summary>
    /// Confirms a construction task and updates related construction items
    /// </summary>
    /// <param name="id">ID of the construction task to confirm</param>
    /// <returns>A task representing the asynchronous operation</returns>
    /// <exception cref="NotFoundException">Thrown when the construction task with the specified ID is not found</exception>
    /// <exception cref="BadRequestException">
    /// Thrown when:
    /// - The task is not in PREVIEWING status
    /// - The task does not have an image URL
    /// </exception>
    public async Task ConfirmConstructionTaskAsync(Guid id)
    {
        // Get the repositories
        var constructionTaskRepo = _unitOfWork.Repository<ConstructionTask>();
        var constructionItemRepo = _unitOfWork.Repository<ConstructionItem>();
        var projectIssueRepo = _unitOfWork.Repository<ProjectIssue>();
        
        // Find the construction task by ID
        var constructionTask = await constructionTaskRepo.FindAsync(id);
        
        // If the task doesn't exist, throw NotFoundException
        if (constructionTask == null)
        {
            throw new NotFoundException($"Không tìm thấy công việc xây dựng với ID {id}");
        }
        
        // Check if the task is in PREVIEWING status
        if (constructionTask.Status != EnumConstructionTaskStatus.PREVIEWING.ToString())
        {
            throw new BadRequestException($"Chỉ có thể xác nhận công việc đang ở trạng thái PREVIEWING. Trạng thái hiện tại: {constructionTask.Status}");
        }
        
        // Check if the task has an image URL
        if (constructionTask.ImageUrl is null)
        {
            throw new BadRequestException("Không thể xác nhận công việc khi chưa có hình ảnh (URL hình ảnh trống)");
        }
        
        // Update the task status to DONE
        constructionTask.Status = EnumConstructionTaskStatus.DONE.ToString();
        await constructionTaskRepo.UpdateAsync(constructionTask, false);
        
        // Get the construction item level 2 (child) that contains this task
        // Since only level 2 items can have tasks, we know this is a level 2 item
        var constructionItemLv2 = await constructionItemRepo.FindAsync(constructionTask.ConstructionItemId);
        if (constructionItemLv2 == null)
        {
            throw new NotFoundException($"Không tìm thấy hạng mục xây dựng với ID {constructionTask.ConstructionItemId}");
        }
        
        // Check if all tasks in the construction item level 2 are DONE
        var allTasksInItemLv2 = constructionTaskRepo.Get(
            filter: t => t.ConstructionItemId == constructionItemLv2.Id && t.IsActive == true,
            includeProperties: ""
        ).ToList();
        
        var allTasksDone = allTasksInItemLv2.All(t => t.Status == EnumConstructionTaskStatus.DONE.ToString());
        
        // If all tasks are DONE, update the construction item level 2 status to DONE
        if (allTasksDone && allTasksInItemLv2.Count > 0)
        {
            constructionItemLv2.Status = EnumConstructionItemStatus.DONE.ToString();
            
            // Set the actual completion date to the current SEA time
            var currentDate = DateOnly.FromDateTime(GlobalUtility.GetCurrentSEATime());
            constructionItemLv2.ActualAt = currentDate;
            
            await constructionItemRepo.UpdateAsync(constructionItemLv2, false);
            
            // Check if this level 2 item has a parent (level 1 item)
            if (constructionItemLv2.ParentId.HasValue)
            {
                var parentId = constructionItemLv2.ParentId.Value;
                var constructionItemLv1 = await constructionItemRepo.FindAsync(parentId);
                
                if (constructionItemLv1 != null)
                {
                    // Get all level 2 items that belong to this level 1 item
                    var allChildItems = constructionItemRepo.Get(
                        filter: i => i.ParentId == parentId && i.IsActive == true,
                        includeProperties: ""
                    ).ToList();
                    
                    // Check if all level 2 items are DONE
                    var allChildItemsDone = allChildItems.All(i => i.Status == EnumConstructionItemStatus.DONE.ToString());
                    
                    // If all level 2 items are DONE, then check if all project issues are also DONE
                    if (allChildItemsDone && allChildItems.Count > 0)
                    {
                        // Get all project issues for the construction item level 1
                        var allProjectIssues = projectIssueRepo.Get(
                            filter: i => i.ConstructionItemId == parentId && i.IsActive == true,
                            includeProperties: ""
                        ).ToList();
                        
                        // Check if there are any project issues that are not in DONE status
                        var allIssuesDone = true;
                        if (allProjectIssues.Count > 0)
                        {
                            allIssuesDone = allProjectIssues.All(i => i.Status == EnumProjectIssueStatus.DONE.ToString());
                        }
                        
                        // Only update the construction item level 1 to DONE if all child items and all project issues are DONE
                        if (allIssuesDone)
                        {
                            constructionItemLv1.Status = EnumConstructionItemStatus.DONE.ToString();
                            
                            // Set the actual completion date to the current SEA time
                            constructionItemLv1.ActualAt = currentDate;
                            
                            await constructionItemRepo.UpdateAsync(constructionItemLv1, false);
                        }
                    }
                }
            }
        }
        
        // Save all changes to the database
        await _unitOfWork.SaveChangesAsync();
    }

    /// <summary>
    /// Creates a new construction item level 2 (child) for a specified parent
    /// </summary>
    /// <param name="request">The request containing information for the new construction item</param>
    /// <param name="id">ID of the parent construction item (level 1)</param>
    /// <returns>A task representing the asynchronous operation</returns>
    /// <exception cref="NotFoundException">Thrown when the parent construction item with the specified ID is not found</exception>
    /// <exception cref="BadRequestException">
    /// Thrown when:
    /// - The parent ID is not a level 1 (parent) construction item
    /// - The name is empty
    /// - The estimate date is not provided
    /// - A child item with the same name already exists under the parent
    /// </exception>
    public async Task CreateConstructionItemLv2Async(CreateConstructionItemRequest request, Guid id)
    {
        // Get required repositories
        var constructionItemRepo = _unitOfWork.Repository<ConstructionItem>();
        
        // Find the parent construction item (level 1)
        var parentItem = await constructionItemRepo.FindAsync(id);
        if (parentItem == null)
        {
            throw new NotFoundException("Hạng mục xây dựng cha không tồn tại");
        }
        
        // Validate that this is a level 1 (parent) item
        if (parentItem.ParentId != null)
        {
            throw new BadRequestException("ID không phải là hạng mục xây dựng cấp 1 (cha)");
        }
        
        // Validate required fields
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            throw new BadRequestException("Tên hạng mục xây dựng không được để trống");
        }
        
        if (!request.EstimateAt.HasValue)
        {
            throw new BadRequestException("Ngày dự kiến hoàn thành không được để trống");
        }
        
        // Check if name is unique among siblings (other children of the same parent)
        var existingItemWithSameName = await constructionItemRepo.FirstOrDefaultAsync(
            item => item.ParentId == id &&
                   item.Name.ToLower() == request.Name.ToLower() &&
                   item.IsActive == true);
        
        if (existingItemWithSameName != null)
        {
            throw new BadRequestException($"Hạng mục xây dựng với tên '{request.Name}' đã tồn tại trong hạng mục cha");
        }
        
        // Create new construction item level 2
        var newConstructionItem = new ConstructionItem
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            EstimateAt = request.EstimateAt.Value,
            ParentId = id,
            ProjectId = parentItem.ProjectId,
            IsPayment = false, // Child items cannot be payment items
            Status = EnumConstructionItemStatus.OPENING.ToString() // New items always start with OPENING status
        };
        
        // Save the new construction item
        await constructionItemRepo.AddAsync(newConstructionItem, true);
    }
}