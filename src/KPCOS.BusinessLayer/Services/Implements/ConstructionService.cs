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

        // Remove existing construction items
        var existingItems = constructionItemRepo.Get(x => x.ProjectId == request.ProjectId).ToList();
        foreach (var item in existingItems)
        {
            await constructionItemRepo.RemoveAsync(item, false);
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
        
        // Get parent items with count using the repository's GetWithCount method
        var (items, total) = constructionItemRepo.GetWithCount(
            filter: predicate,
            orderBy: filter.GetOrder(),
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
                // Get child items using the repository's Get method
                var childItems = constructionItemRepo.Get(
                    filter: x => x.ParentId == item.Id.Value
                );
                
                item.Childs = _mapper.Map<List<GetAllConstructionItemChildResponse>>(childItems);
            }
            else
            {
                // For child items, ensure Childs collection is empty
                item.Childs = new List<GetAllConstructionItemChildResponse>();
            }
        }
        
        return (result, total);
    }

    public async Task<(IEnumerable<GetAllConstructionTaskResponse> data, int total)> GetAllConstructionTaskAsync(GetAllConstructionTaskFilterRequest filter)
    {
        // Get the repository for ConstructionTask
        var constructionTaskRepo = _unitOfWork.Repository<ConstructionTask>();
        
        // Apply the filter expression from the request
        var filterExpression = filter.GetExpressions();
        
        // Get the data with count using the repository's GetWithCount method
        var result = constructionTaskRepo.GetWithCount(
            filter: filterExpression,
            orderBy: filter.GetOrder(),
            includeProperties: "Staff,Staff.User",
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
        
        // Update the status of the level 2 (child) construction item to PROCESSING if it's currently OPENING
        if (constructionItem.Status == EnumConstructionItemStatus.OPENING.ToString())
        {
            constructionItem.Status = EnumConstructionItemStatus.PROCESSING.ToString();
            await constructionItemRepo.UpdateAsync(constructionItem, false);
            
            // Update the parent (level 1) construction item status to PROCESSING if it's currently OPENING
            if (constructionItem.ParentId.HasValue)
            {
                var parentItem = await constructionItemRepo.FindAsync(constructionItem.ParentId.Value);
                if (parentItem != null && parentItem.Status == EnumConstructionItemStatus.OPENING.ToString())
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
                ParentId = parentId
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
                ParentId = parentId
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
}