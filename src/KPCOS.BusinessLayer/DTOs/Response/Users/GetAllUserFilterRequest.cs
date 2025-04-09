using System;
using System.Linq.Expressions;
using KPCOS.Common.Pagination;
using KPCOS.DataAccessLayer.Entities;
using KPCOS.DataAccessLayer.Enums;
using LinqKit;

namespace KPCOS.BusinessLayer.DTOs.Response.Users;

public class GetAllUserFilterRequest : PaginationRequest<User>
{
    public Guid? Id { get; set; }
    public string? FullName { get; set; }
    public string? Email { get; set; }
    public string? Position { get; set; }
    public string? Status { get; set; }
    public string? Address { get; set; }
    public bool? IsActive { get; set; }

    public override Expression<Func<User, bool>> GetExpressions()
    {
        var predicate = PredicateBuilder.New<User>(true);
        
        if (Id.HasValue)
        {
            predicate = predicate.And(user => user.Id == Id);
        }

        if (!string.IsNullOrEmpty(FullName))
        {
            predicate = predicate.And(user => user.FullName.Contains(FullName));
        }
        
        if (!string.IsNullOrEmpty(Email))
        {
            predicate = predicate.And(user => user.Email.Contains(Email));
        }
        
        if (!string.IsNullOrEmpty(Position))
        {
            if (Position == RoleEnum.CUSTOMER.ToString())
            {
                predicate = predicate.And(user => user.Customers.Any());
            }
            else
            {
                predicate = predicate.And(user => user.Staff.Any(s => s.Position == Position));
            }
        }
        
        if (!string.IsNullOrEmpty(Status))
        {
            predicate = predicate.And(user => user.Status != null && user.Status == Status);
        }
        
        if (!string.IsNullOrEmpty(Address))
        {
            predicate = predicate.And(user => user.Customers.Any(c => c.Address.Contains(Address)));
        }
        
        if (IsActive.HasValue)
        {
            predicate = predicate.And(user => user.IsActive == IsActive);
        }
        
        return predicate;
    }
}
