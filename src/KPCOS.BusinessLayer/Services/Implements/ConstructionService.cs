using KPCOS.BusinessLayer.DTOs.Request.Constructions;
using KPCOS.Common.Exceptions;
using KPCOS.DataAccessLayer.Entities;
using KPCOS.DataAccessLayer.Enums;
using KPCOS.DataAccessLayer.Repositories;

namespace KPCOS.BusinessLayer.Services.Implements;

public class ConstructionService : IConstructionServices
{
    private readonly IUnitOfWork _unitOfWork;

    public ConstructionService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
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
        if (specialItemCount > 3)
        {
            throw new BadRequestException("Số lượng hạng mục thanh toán không được vượt quá 3");
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
                EstimateAt = request.EstimateAt,
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
                EstimateAt = request.EstimateAt,
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
}