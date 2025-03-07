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
    
    // remove all construction item of project

    var constructionItemRaw =  constructionItemRepo.Get(x => x.ProjectId == request.ProjectId).ToList();
    foreach (var item in constructionItemRaw)
    {
        await constructionItemRepo.RemoveAsync(item, false);
    }
    
    foreach (var item in request.Items)
    {
        var constructionTemplateItemRaw =
            await constructionTemplateItemRepo.FirstOrDefaultAsync(x => x.Id == item.TemplateItemId);
        if (constructionTemplateItemRaw == null)
        {
            throw new NotFoundException("Mẫu công trình không tồn tại");
        }

     
        var constructionItem = new ConstructionItem
        {
            Id = Guid.NewGuid(),
            Name = constructionTemplateItemRaw.Name,
            Description = constructionTemplateItemRaw.Description,
            Status = item.IsPayment ? EnumConstructionItem.SPECIAL.ToString() : EnumConstructionItem.NORMAL.ToString(),
            EstimateAt = item.EstDate,
            ProjectId = request.ProjectId,
        };
        
        await constructionItemRepo.AddAsync(constructionItem, false);

        foreach (var child in item.Child)
        {
           
            var constructionTemplateItemChildRaw = await constructionTemplateItemRepo.FindAsync(child.TemplateItemId);
            
            if (constructionTemplateItemChildRaw == null)
            {
                throw new NotFoundException("Mẫu công trình không tồn tại");
            }
            
            var constructionItemChild = new ConstructionItem
            {
                Id = Guid.NewGuid(),
                Name = constructionTemplateItemChildRaw.Name,
                Description = constructionTemplateItemChildRaw.Description,
                Status = child.IsPayment ? EnumConstructionItem.SPECIAL.ToString() : EnumConstructionItem.NORMAL.ToString(),
                EstimateAt = child.EstDate,
                ProjectId = request.ProjectId,
                ParentId = constructionItem.Id
            };
            
            await constructionItemRepo.AddAsync(constructionItemChild, false);
        }
        
    }

  
   
   
    
    await _unitOfWork.SaveChangesAsync();
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

        // Determine status - child items are always NORMAL
        string status = isParent && request.IsPayment == true 
            ? EnumConstructionItem.SPECIAL.ToString() 
            : EnumConstructionItem.NORMAL.ToString();

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
                EstimateAt = request.EstDate,
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
                EstimateAt = request.EstDate,
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