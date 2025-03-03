using KPCOS.BusinessLayer.DTOs.Request;
using KPCOS.Common.Exceptions;
using KPCOS.DataAccessLayer.Entities;
using KPCOS.DataAccessLayer.Enums;
using KPCOS.DataAccessLayer.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace KPCOS.BusinessLayer.Services.Implements;

public class ConstructionService : IConstructionServices
{
    
    private readonly IUnitOfWork _unitOfWork;
    private readonly IConfiguration _configuration;
    private readonly ILogger<PackageService> _logger;

    public ConstructionService(IUnitOfWork unitOfWork, IConfiguration configuration, ILogger<PackageService> logger)
    {
        _unitOfWork = unitOfWork;
        _configuration = configuration;
        _logger = logger;
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

    var constructionItemRaw =  constructionItemRepo.Get(x => x.Idproject == request.ProjectId).ToList();
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
            Estdate = item.EstDate,
            Idproject = request.ProjectId,
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
                Estdate = child.EstDate,
                Idproject = request.ProjectId,
                Idparent = constructionItem.Id
            };
            
            await constructionItemRepo.AddAsync(constructionItemChild, false);
        }
        
    }

  
   
    
    await _unitOfWork.SaveChangesAsync();
}
}