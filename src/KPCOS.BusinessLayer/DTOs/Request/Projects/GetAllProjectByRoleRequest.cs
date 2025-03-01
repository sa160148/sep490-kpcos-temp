using System.Linq.Expressions;
using KPCOS.Common.Pagination;
using KPCOS.DataAccessLayer.Entities;
using KPCOS.DataAccessLayer.Enums;
using LinqKit;

namespace KPCOS.BusinessLayer.DTOs.Request.Projects;

public class GetAllProjectByRoleRequest : PaginationRequest<Project>
{
    public string? Search { get; set; }
    public string? Name { get; set; }
    
    public override Expression<Func<Project, bool>> GetExpressions()
    {
        throw new NotImplementedException();
    }
    
    public Expression<Func<Project, bool>> GetExpressionsV2(Guid userId, string role)
    {       
        var customerQueryExpression = PredicateBuilder.New<Project>(true);
        /*if (role == RoleEnum.ADMINISTRATOR.ToString())
        {
            return Expression = Expression.And(customerQueryExpression);
        }*/
        customerQueryExpression.Or(pro => pro.Customer.UserId == userId || 
                                          pro.ProjectStaffs.Any(ps => ps.Staff.UserId == userId) || 
                                          pro.ProjectStaffs.Any(ps => ps.StaffId == userId && ps.Staff.Position == RoleEnum.ADMINISTRATOR.ToString()));
        return Expression = Expression.And(customerQueryExpression);
    }
}