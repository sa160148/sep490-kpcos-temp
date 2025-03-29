using System;
using KPCOS.BusinessLayer.DTOs.Request.Blogs;
using KPCOS.BusinessLayer.DTOs.Response.Blogs;

namespace KPCOS.BusinessLayer.Services;

public interface IBlogService
{
    Task<(IEnumerable<GetAllBlogResponse> data, int total)> GetAllBlogs(GetAllBlogFilterRequest filter, Guid? userId = null);
    Task<GetAllBlogResponse> GetDetailBlogById(Guid id);
    Task CreateBlog(CommandBlogRequest request, Guid? userId = null);
}
