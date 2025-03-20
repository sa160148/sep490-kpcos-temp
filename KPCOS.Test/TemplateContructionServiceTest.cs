using System.Linq.Expressions;
using AutoMapper;
using KPCOS.BusinessLayer.DTOs.Request;
using KPCOS.BusinessLayer.Services;
using KPCOS.BusinessLayer.Services.Implements;
using KPCOS.Common.Exceptions;
using KPCOS.DataAccessLayer.Entities;
using KPCOS.DataAccessLayer.Repositories;
using LinqKit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;

namespace KPCOS.Test;


[TestClass]
public class TemplateContructionServiceTest
{
    private Mock<IUnitOfWork> _mockUnitOfWork;
    private Mock<IConfiguration> _mockConfiguration;
    private Mock<IMapper> _mockMapper;
    private Mock<ILogger<ServiceService>> _mockLogger;
    private TemplateContructionService  _templateContructionService;
    
    [TestInitialize]
    public void Setup()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockConfiguration = new Mock<IConfiguration>();
        _mockLogger = new Mock<ILogger<ServiceService>>();
        _templateContructionService = new TemplateContructionService(_mockUnitOfWork.Object, _mockConfiguration.Object, _mockLogger.Object);
    }
    
    
    // create template contruction and init is active is false
    [TestMethod]
    public async Task CreateTemplateContructionAsync_ValidRequest_CreatesTemplate()
    {
        // Arrange
        var request = new TemplateContructionCreateRequest
        {
            Name = "Template 1",
            Description = "Description 1",
        };
        
        _mockUnitOfWork.Setup(x => x.Repository<ConstructionTemplate>().SingleOrDefaultAsync(It.IsAny<Expression<Func<ConstructionTemplate, bool>>>()))
            .ReturnsAsync((ConstructionTemplate)null);
        
        // Act
        await _templateContructionService.CreateTemplateContructionAsync(request);
        
        // Assert
        _mockUnitOfWork.Verify(x => x.Repository<ConstructionTemplate>().AddAsync(It.Is<ConstructionTemplate>(x => x.IsActive == false), false), Times.Once);
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(), Times.Once);
    }
    
    // get template contruction by filter
    [TestMethod]
    public async Task GetsAsyncPaging_ValidFilter_ReturnsPagedData()
    {
        // Arrange
        var filter = new GetAllConstructionTemplateFilterRequest
        {
            PageNumber = 1,
            PageSize = 10,
            
        };

        var fakeData = new List<ConstructionTemplate>
        {
            new ConstructionTemplate { Id = Guid.NewGuid(), Name = "Template 1", Description = "Description 1", IsActive = true },
            new ConstructionTemplate { Id = Guid.NewGuid(), Name = "Template 2", Description = "Description 2", IsActive = true }
        };

        var mockTemplateRepo = new Mock<IRepository<ConstructionTemplate>>();
        mockTemplateRepo
            .Setup(repo => repo.GetWithCount(
                It.IsAny<Expression<Func<ConstructionTemplate, bool>>>(),
                It.IsAny<Func<IQueryable<ConstructionTemplate>, IOrderedQueryable<ConstructionTemplate>>>(), 
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<int>()
            ))
            .Returns((fakeData, fakeData.Count)); 

        _mockUnitOfWork.Setup(uow => uow.Repository<ConstructionTemplate>()).Returns(mockTemplateRepo.Object);

        _mockUnitOfWork.Setup(uow => uow.Repository<ConstructionTemplate>()).Returns(mockTemplateRepo.Object);

        // Act
        var result = await _templateContructionService.GetsAsyncPaging(filter);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(2, result.Data.Count()); 
        Assert.AreEqual(fakeData.Count, result.TotalRecords); 
        Assert.AreEqual("Template 1", result.Data.First().Name);
    }
    
    // change active template contruction
    [TestMethod]
    public async Task ActiveTemplateContructionAsync_ValidId_ActiveTemplate_SetInActive()
    {
        // Arrange
        var id = Guid.NewGuid();
        var template = new ConstructionTemplate
        {
            Id = id,
            IsActive = true
        };
        
        _mockUnitOfWork.Setup(x => x.Repository<ConstructionTemplate>().FindAsync(id))
            .ReturnsAsync(template);
        
        // Act
        await _templateContructionService.ActiveTemplateContructionAsync(id);
        
        // Assert
        Assert.IsFalse(template.IsActive);
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(), Times.Once);
    }
    
    [TestMethod]
    public async Task ActiveTemplateContructionAsync_ValidId_ActiveTemplate_SetActive()
    {
        // Arrange
        var id = Guid.NewGuid();
        var template = new ConstructionTemplate
        {
            Id = id,
            IsActive = true
        };
        var mockTemplateRepo = new Mock<IRepository<ConstructionTemplate>>();
        mockTemplateRepo.Setup(repo => repo.FindAsync(id))
            .ReturnsAsync(template);
        
        var mockTemplateItemRepo = new Mock<IRepository<ConstructionTemplateItem>>();
        mockTemplateItemRepo.Setup(repo => repo.GetWithCount(
                It.IsAny<Expression<Func<ConstructionTemplateItem, bool>>>(), 
                It.IsAny<Func<IQueryable<ConstructionTemplateItem>, IOrderedQueryable<ConstructionTemplateItem>>>(),
                It.IsAny<string>(), 
                It.IsAny<int>(), 
                It.IsAny<int>() 
            ))
            .Returns((new List<ConstructionTemplateItem>
            {
                new ConstructionTemplateItem { Id = Guid.NewGuid(), Idtemplate = id },
                new ConstructionTemplateItem { Id = Guid.NewGuid(), Idtemplate = id },
                new ConstructionTemplateItem { Id = Guid.NewGuid(), Idtemplate = id }
            }, 3));
        
        _mockUnitOfWork.Setup(uow => uow.Repository<ConstructionTemplate>()).Returns(mockTemplateRepo.Object);
        
        _mockUnitOfWork.Setup(uow => uow.Repository<ConstructionTemplateItem>()).Returns(mockTemplateItemRepo.Object);
        
        // Act
        await _templateContructionService.ActiveTemplateContructionAsync(id);
        
        // Assert
        
        Assert.IsFalse(template.IsActive);
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(), Times.Once);
        
    }
    
    [TestMethod]
    public async Task ActiveTemplateContructionAsync_ValidId_ActiveTemplate_SetActive_ThereIsNoItem()
    {
        // Arrange
        var templateId = Guid.NewGuid();
        var template = new ConstructionTemplate { Id = templateId, IsActive = false };

        var mockTemplateRepo = new Mock<IRepository<ConstructionTemplate>>();
        mockTemplateRepo.Setup(repo => repo.FindAsync(templateId))
            .ReturnsAsync(template);

        var mockTemplateItemRepo = new Mock<IRepository<ConstructionTemplateItem>>();
        mockTemplateItemRepo.Setup(repo => repo.GetWithCount(
                It.IsAny<Expression<Func<ConstructionTemplateItem, bool>>>(), // filter
                It.IsAny<Func<IQueryable<ConstructionTemplateItem>, IOrderedQueryable<ConstructionTemplateItem>>>(), // orderBy
                It.IsAny<string>(), // includeProperties
                It.IsAny<int>(), // pageIndex
                It.IsAny<int>()  // pageSize
            ))
            .Returns((new List<ConstructionTemplateItem>
            {
                new ConstructionTemplateItem { Id = Guid.NewGuid(), Idtemplate = templateId },
                new ConstructionTemplateItem { Id = Guid.NewGuid(), Idtemplate = templateId }
            }, 2)); 

        _mockUnitOfWork.Setup(uow => uow.Repository<ConstructionTemplate>()).Returns(mockTemplateRepo.Object);
        _mockUnitOfWork.Setup(uow => uow.Repository<ConstructionTemplateItem>()).Returns(mockTemplateItemRepo.Object);

        // Act & Assert
        await Assert.ThrowsExceptionAsync<BadRequestException>(() => _templateContructionService.ActiveTemplateContructionAsync(templateId));
    }
    
    
    [TestMethod]
    public async Task CreateTemplateContructionAsyncItemSuccess()
    {
        
        var IdTemplateContruction = Guid.NewGuid();
        // Arrange
        var request = new TemplateContructionItemCreateRequest
        {
            Name = "Template 1",
            Description = "Description 1",
            IdTemplateContruction = IdTemplateContruction,
            Duration = 10,
            IdParent = Guid.NewGuid()
        };
        
        var template = new ConstructionTemplate
        {
            Id = IdTemplateContruction,
            IsActive = true
        };
        
        var templateItem = new ConstructionTemplateItem
        {
            Id = Guid.NewGuid(),
            Idtemplate = request.IdTemplateContruction,
            Idparent = request.IdParent
        };
        
        
        // Act
        _mockUnitOfWork.Setup(x => x.Repository<ConstructionTemplate>().
                SingleOrDefaultAsync(s => s.Id == request.IdTemplateContruction))
            .ReturnsAsync(template);

        _mockUnitOfWork.Setup(x =>
            x.Repository<ConstructionTemplateItem>().AddAsync(It.IsAny<ConstructionTemplateItem>(), false));
        
        await _templateContructionService.CreateTemplateContructionItemAsync(request);
        
        // Assert
        
        _mockUnitOfWork.Verify(x => x.Repository<ConstructionTemplateItem>().AddAsync(It.Is<ConstructionTemplateItem>(x => x.Idtemplate == request.IdTemplateContruction), false), Times.Once);
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(), Times.Once);
        
    }
        
    
    
}
    
    
