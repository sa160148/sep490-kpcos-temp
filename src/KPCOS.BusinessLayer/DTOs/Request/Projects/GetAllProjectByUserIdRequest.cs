using System.Linq.Expressions;
using KPCOS.Common.Pagination;
using KPCOS.DataAccessLayer.Entities;
using KPCOS.DataAccessLayer.Enums;
using LinqKit;
using Microsoft.IdentityModel.Tokens;

namespace KPCOS.BusinessLayer.DTOs.Request.Projects;

/// <summary>
/// Request model for getting projects with pagination and filtering by user role and status
/// </summary>
public class GetAllProjectByUserIdRequest : PaginationRequest<Project>
{
    /// <summary>
    /// List of project statuses to filter by
    /// </summary>
    public List<string>? Status { get; set; }

    public override Expression<Func<Project, bool>> GetExpressions()
    {
        throw new NotImplementedException();
    }
    
    /// <summary>
    /// Builds the filter expression for projects based on user role and status
    /// </summary>
    /// <param name="userId">The ID of the user requesting projects</param>
    /// <param name="role">The role of the user (ADMINISTRATOR, CONSULTANT, etc.)</param>
    /// <returns>Expression to filter projects based on user permissions and status</returns>
    /// <remarks>
    /// <para>Filtering rules:</para>
    /// <list type="bullet">
    ///     <item><description>Administrators can see all projects</description></item>
    ///     <item><description>Other users can only see their own projects or projects they're assigned to</description></item>
    ///     <item><description>Optional status filtering is applied to all queries</description></item>
    /// </list>
    /// </remarks>
    public Expression<Func<Project, bool>> GetExpressionsV2(Guid userId, string? role)
    {
        var predicate = PredicateBuilder.New<Project>(true);

        // Add status filter
        if (!Status.IsNullOrEmpty())
        {
            predicate = predicate.And(p => Status.Contains(p.Status));
        }

        // Admin can see all projects
        if (!role.IsNullOrEmpty() && role == RoleEnum.ADMINISTRATOR.ToString())
        {
            return predicate;
        }

        // User can see their own projects or assigned projects
        predicate = predicate.And(p => 
            p.Customer.UserId == userId || 
            p.ProjectStaffs.Any(ps => ps.Staff.UserId == userId));

        return predicate;
    }
}