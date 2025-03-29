using System.Security.Claims;
using KPCOS.BusinessLayer.DTOs.Request.Blogs;
using KPCOS.BusinessLayer.DTOs.Response.Blogs;
using KPCOS.BusinessLayer.Services;
using KPCOS.WebFramework.Api;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using KPCOS.Common;

namespace KPCOS.API.Controllers
{
    /// <summary>
    /// API endpoints for managing blog posts about koi pond construction, maintenance, and services
    /// </summary>
    [Route("api/[controller]")]
    public class BlogsController : BaseController
    {
        private readonly IBlogService _blogService;

        public BlogsController(IBlogService blogService)
        {
            _blogService = blogService;
        }

        /// <summary>
        /// Gets a paginated list of blogs with optional filtering
        /// </summary>
        /// <remarks>
        /// Sample request:
        /// 
        ///     GET /api/Blogs?PageNumber=1&amp;PageSize=10&amp;Search=koi&amp;Type=PROJECT
        ///     
        /// Sample response:
        /// 
        ///     {
        ///       "data": [
        ///         {
        ///           "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        ///           "name": "Modern Koi Pond Design for Urban Gardens",
        ///           "description": "A showcase of our recent koi pond project with advanced filtration system and elegant design elements",
        ///           "type": "PROJECT",
        ///           "createdAt": "2023-05-10T10:00:00",
        ///           "updatedAt": "2023-05-15T14:30:00",
        ///           "isActive": true,
        ///           "staff": {
        ///             "id": "1fa85f64-5717-4562-b3fc-2c963f66afa1",
        ///             "fullName": "John Doe",
        ///             "email": "john.doe@example.com",
        ///             "position": "Senior Designer",
        ///             "avatar": "http://example.com/avatars/johndoe.jpg"
        ///           },
        ///           "project": {
        ///             "id": "2fa85f64-5717-4562-b3fc-2c963f66afa2",
        ///             "name": "Urban Garden Koi Pond",
        ///             "status": "COMPLETED",
        ///             "packageName": "Premium Pond Package"
        ///           }
        ///         }
        ///       ],
        ///       "pageNumber": 1,
        ///       "pageSize": 10,
        ///       "totalRecords": 1
        ///     }
        /// </remarks>
        /// <param name="filter">Filter parameters for blogs</param>
        /// <returns>A paged list of blog posts</returns>
        [HttpGet]
        [SwaggerOperation(
            Summary = "Lấy danh sách bài blog",
            Description = "Retrieves a paginated list of blog posts with optional filtering"
        )]
        [SwaggerResponse(StatusCodes.Status200OK, "Returns a list of blog posts", typeof(PagedApiResponse<GetAllBlogResponse>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid request parameters")]
        public async Task<PagedApiResponse<GetAllBlogResponse>> GetAllBlogs(
            [FromQuery] [SwaggerParameter("Filter parameters to search and paginate blog posts")] GetAllBlogFilterRequest filter)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            (IEnumerable<GetAllBlogResponse> data, int total) result;
            if (string.IsNullOrEmpty(userIdClaim))
            {
                result = await _blogService.GetAllBlogs(filter);
                return new PagedApiResponse<GetAllBlogResponse>(
                    result.data,
                    filter.PageNumber,
                    filter.PageSize,
                    result.total
                );
            }
            result = await _blogService.GetAllBlogs(filter);
            return new PagedApiResponse<GetAllBlogResponse>(
                result.data, 
                filter.PageNumber, 
                filter.PageSize, 
                result.total);
        }

        /// <summary>
        /// Gets a specific blog post by its ID
        /// </summary>
        /// <remarks>
        /// Sample request:
        /// 
        ///     GET /api/Blogs/3fa85f64-5717-4562-b3fc-2c963f66afa6
        ///     
        /// Sample response:
        /// 
        ///     {
        ///       "data": {
        ///         "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        ///         "name": "Advanced Koi Pond Filtration Systems",
        ///         "description": "An in-depth guide to different filtration systems for koi ponds, including biological, mechanical, and UV filtration",
        ///         "type": "PACKAGE",
        ///         "createdAt": "2023-06-05T09:30:00",
        ///         "updatedAt": "2023-06-10T11:15:00",
        ///         "isActive": true,
        ///         "staff": {
        ///           "id": "1fa85f64-5717-4562-b3fc-2c963f66afa1",
        ///           "fullName": "Jane Smith",
        ///           "email": "jane.smith@example.com",
        ///           "position": "Technical Specialist",
        ///           "avatar": "http://example.com/avatars/janesmith.jpg"
        ///         },
        ///         "package": {
        ///           "id": "5fa85f64-5717-4562-b3fc-2c963f66afa5",
        ///           "name": "Premium Filtration Package",
        ///           "description": "Complete filtration solution for koi ponds",
        ///           "price": [50000000, 47500000, 45125000, 42868750, 40725312]
        ///         }
        ///       },
        ///       "isSuccess": true,
        ///       "statusCode": 200,
        ///       "message": "Blog post retrieved successfully"
        ///     }
        /// </remarks>
        /// <param name="id">ID of the blog post to retrieve</param>
        /// <returns>Details of a specific blog post</returns>
        [HttpGet("{id}")]
        [SwaggerOperation(
            Summary = "Lấy bài blog theo id",
            Description = "Retrieves a specific blog post by its unique identifier"
        )]
        [SwaggerResponse(StatusCodes.Status200OK, "Returns the requested blog post", typeof(ApiResult<GetAllBlogResponse>))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Blog post not found")]
        public async Task<ApiResult<GetAllBlogResponse>> GetDetailBlogById(
            [SwaggerParameter(Description = "ID of the blog post to retrieve")]
            Guid id)
        {
            var blog = await _blogService.GetDetailBlogById(id);
            return Ok(blog);
        }

        /// <summary>
        /// Creates a new blog post
        /// </summary>
        /// <remarks>
        /// Sample request:
        /// 
        ///     POST /api/Blogs
        ///     {
        ///       "name": "Seasonal Koi Pond Maintenance Guide",
        ///       "description": "Comprehensive guidance on how to maintain your koi pond throughout the different seasons for optimal fish health and water quality",
        ///       "type": "MAINTENANCE_PACKAGE",
        ///       "no": "7fa85f64-5717-4562-b3fc-2c963f66afa7"
        ///     }
        ///     
        /// Sample response:
        /// 
        ///     {
        ///       "isSuccess": true,
        ///       "statusCode": 201,
        ///       "message": "Blog post created successfully"
        ///     }
        /// </remarks>
        /// <param name="request">Blog post information</param>
        /// <returns>Result of the operation</returns>
        [HttpPost]
        [SwaggerOperation(
            Summary = "Tạo bài blog",
            Description = "Creates a new blog post with the provided information"
        )]
        [SwaggerResponse(StatusCodes.Status201Created, "Blog post created successfully", typeof(ApiResult))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid blog post data")]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "User not authenticated")]
        public async Task<ApiResult> CreateBlog(
            [SwaggerParameter(Description = "Information for the new blog post")]
            CommandBlogRequest request)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            if (!string.IsNullOrEmpty(userIdClaim))
            {
                var userId = Guid.Parse(userIdClaim);
                await _blogService.CreateBlog(request, userId);
                return Ok();
            }
            
            await _blogService.CreateBlog(request);
            return Ok();
        }
    }
}
