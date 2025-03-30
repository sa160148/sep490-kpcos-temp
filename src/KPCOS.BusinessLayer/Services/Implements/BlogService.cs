using System;
using AutoMapper;
using KPCOS.DataAccessLayer.Repositories;
using KPCOS.BusinessLayer.DTOs.Request.Blogs;
using KPCOS.BusinessLayer.DTOs.Response.Blogs;
using KPCOS.DataAccessLayer.Entities;
using KPCOS.Common.Utilities;
using KPCOS.DataAccessLayer.Enums;
using System.Linq.Expressions;
using KPCOS.BusinessLayer.DTOs.Response.Projects;
using KPCOS.BusinessLayer.DTOs.Response;
using KPCOS.BusinessLayer.DTOs.Response.MaintenancePackages;
using System.Linq;
using LinqKit;

namespace KPCOS.BusinessLayer.Services.Implements;

/// <summary>
/// Service implementation for managing blog posts related to koi pond construction and maintenance
/// </summary>
public class BlogService : IBlogService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    /// <summary>
    /// Initializes a new instance of the BlogService class
    /// </summary>
    /// <param name="unitOfWork">Unit of work for database operations</param>
    /// <param name="mapper">AutoMapper instance for object mapping</param>
    public BlogService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    /// <summary>
    /// Retrieves a paginated list of blog posts with optional filtering
    /// </summary>
    /// <param name="filter">Filter criteria for searching blogs</param>
    /// <param name="userId">Optional user ID to filter blogs by creator</param>
    /// <returns>A tuple containing the collection of blog posts and the total count</returns>
    /// <example>
    /// Example usage:
    /// <code>
    /// var filter = new GetAllBlogFilterRequest 
    /// { 
    ///     PageNumber = 1, 
    ///     PageSize = 10,
    ///     Search = "koi pond",
    ///     Type = "PROJECT"
    /// };
    /// var (blogs, totalCount) = await _blogService.GetAllBlogs(filter);
    /// </code>
    /// </example>
    public async Task<(IEnumerable<GetAllBlogResponse> data, int total)> GetAllBlogs(GetAllBlogFilterRequest filter, Guid? userId = null)
    {
        Expression<Func<Blog, bool>> expression = filter.GetExpressions();
        if (userId != null)
        {
            expression = expression.And(x => x.StaffId == userId);
        }
        var repository = _unitOfWork.Repository<Blog>();
        var result = repository.GetWithCount(
            filter: expression,
            orderBy: filter.GetOrder(),
            includeProperties: "Staff,Staff.User",
            pageIndex: filter.PageNumber,
            pageSize: filter.PageSize
        );
        
        var blogs = result.Data;
        var count = result.Count;

        var blogResponses = _mapper.Map<IEnumerable<GetAllBlogResponse>>(blogs);

        // Handle additional references based on blog type
        foreach (var blog in blogResponses)
        {
            var blogEntity = blogs.FirstOrDefault(b => b.Id == blog.Id);
            if (blogEntity?.No != null && blogEntity.Type != null)
            {
                switch (blogEntity.Type)
                {
                    case nameof(EnumBlogType.PROJECT):
                        var project = await _unitOfWork.Repository<Project>().FindAsync(blogEntity.No);
                        blog.Project = _mapper.Map<ProjectResponse>(project);
                        break;
                    case nameof(EnumBlogType.PACKAGE):
                        var package = await _unitOfWork.Repository<Package>().FindAsync(blogEntity.No);
                        blog.Package = _mapper.Map<PackageResponse>(package);
                        break;
                    case nameof(EnumBlogType.MAINTENANCE_PACKAGE):
                        var maintenancePackage = await _unitOfWork.Repository<MaintenancePackage>().FindAsync(blogEntity.No);
                        blog.MaintenancePackages = _mapper.Map<GetAllMaintenancePackageResponse>(maintenancePackage);
                        break;
                }
            }
        }

        return (blogResponses, count);
    }

    /// <summary>
    /// Retrieves a specific blog post by its unique identifier
    /// </summary>
    /// <param name="id">The unique identifier of the blog post</param>
    /// <returns>The blog post details</returns>
    /// <exception cref="Exception">Thrown when the blog post with the specified ID is not found</exception>
    /// <example>
    /// Example usage:
    /// <code>
    /// var blogId = Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6");
    /// var blog = await _blogService.GetDetailBlogById(blogId);
    /// </code>
    /// </example>
    public async Task<GetAllBlogResponse> GetDetailBlogById(Guid id)
    {
        var blog = await _unitOfWork.Repository<Blog>().FindAsync(id);
        
        if (blog == null)
        {
            throw new Exception($"Blog with id {id} not found");
        }

        var blogResponse = _mapper.Map<GetAllBlogResponse>(blog);

        // Handle additional references based on blog type
        if (blog.No != null && blog.Type != null)
        {
            switch (blog.Type)
            {
                case nameof(EnumBlogType.PROJECT):
                    var project = await _unitOfWork.Repository<Project>().FindAsync(blog.No);
                    blogResponse.Project = _mapper.Map<ProjectResponse>(project);
                    break;
                case nameof(EnumBlogType.PACKAGE):
                    var package = await _unitOfWork.Repository<Package>().FindAsync(blog.No);
                    blogResponse.Package = _mapper.Map<PackageResponse>(package);
                    break;
                case nameof(EnumBlogType.MAINTENANCE_PACKAGE):
                    var maintenancePackage = await _unitOfWork.Repository<MaintenancePackage>().FindAsync(blog.No);
                    blogResponse.MaintenancePackages = _mapper.Map<GetAllMaintenancePackageResponse>(maintenancePackage);
                    break;
            }
        }

        return blogResponse;
    }

    /// <summary>
    /// Creates a new blog post
    /// </summary>
    /// <param name="request">The blog post information</param>
    /// <param name="userId">Optional user ID of the blog creator</param>
    /// <returns>A task representing the asynchronous operation</returns>
    /// <remarks>
    /// The method will automatically determine the blog type based on the No value:
    /// - If No is null, Type will be set to OTHER
    /// - If No references a Project, Type will be set to PROJECT
    /// - If No references a Package, Type will be set to PACKAGE
    /// - If No references a MaintenancePackage, Type will be set to MAINTENANCE_PACKAGE
    /// </remarks>
    /// <example>
    /// Example usage:
    /// <code>
    /// var newBlog = new CommandBlogRequest
    /// {
    ///     Name = "Modern Koi Pond Design",
    ///     Description = "A detailed guide to designing modern koi ponds for urban gardens",
    ///     No = projectId // The type will be automatically set based on this ID
    /// };
    /// await _blogService.CreateBlog(newBlog, currentUserId);
    /// </code>
    /// </example>
    public async Task CreateBlog(CommandBlogRequest request, Guid? userId = null)
    {
        Blog blog = _mapper.Map<Blog>(request);
        
        // Auto-determine blog type based on No value
        if (request.No.HasValue)
        {
            // Try to find the entity in different repositories to determine type
            var project = await _unitOfWork.Repository<Project>().FindAsync(request.No.Value);
            if (project != null)
            {
                blog.Type = nameof(EnumBlogType.PROJECT);
                blog.No = project.Id;
            }
            else
            {
                var package = await _unitOfWork.Repository<Package>().FindAsync(request.No.Value);
                if (package != null)
                {
                    blog.Type = nameof(EnumBlogType.PACKAGE);
                    blog.No = package.Id;
                }
                else
                {
                    var maintenancePackage = await _unitOfWork.Repository<MaintenancePackage>().FindAsync(request.No.Value);
                    if (maintenancePackage != null)
                    {
                        blog.Type = nameof(EnumBlogType.MAINTENANCE_PACKAGE);
                        blog.No = maintenancePackage.Id;
                    }
                    else
                    {
                        // If No doesn't match any entity, set type to OTHER
                        blog.Type = nameof(EnumBlogType.OTHER);
                    }
                }
            }
        }
        else
        {
            // If No is null, set type to OTHER
            blog.Type = nameof(EnumBlogType.OTHER);
        }

        // Set the StaffId if a userId is provided
        if (userId.HasValue)
        {
            var staff = await _unitOfWork.Repository<Staff>().SingleOrDefaultAsync(x => x.UserId == userId.Value);
            if (staff != null)
            {
                blog.StaffId = staff.Id;
            }
            else
            {
                blog.StaffId = userId.Value;
            }
        }

        await _unitOfWork.Repository<Blog>().AddAsync(blog);
    }
}
