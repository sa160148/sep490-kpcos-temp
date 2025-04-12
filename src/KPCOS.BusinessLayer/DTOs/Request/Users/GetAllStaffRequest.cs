using System;
using System.Linq.Expressions;
using KPCOS.Common.Pagination;
using KPCOS.DataAccessLayer.Entities;
using KPCOS.DataAccessLayer.Enums;
using LinqKit;

namespace KPCOS.BusinessLayer.DTOs.Request.Users;

public class GetAllStaffRequest : PaginationRequest<Staff>
{
    public string? Search { get; set; }
    public string? Position { get; set; }
    public string? Status { get; set; }
    public bool? IsActive { get; set; }
    public bool? IsIdle { get; set; }

    public override Expression<Func<Staff, bool>> GetExpressions()
    {
        var predicate = PredicateBuilder.New<Staff>(true);

        if (!string.IsNullOrEmpty(Search))
        {
            predicate = predicate.And(s => s.User.FullName.Contains(Search) || 
                                          s.User.Email.Contains(Search) ||
                                          s.User.Phone.Contains(Search));
        }

        if (!string.IsNullOrEmpty(Position))
        {
            var positions = Position.Split(',').ToList();
            predicate = predicate.And(s => positions.Contains(s.Position));
        }

        if (IsActive.HasValue)
        {
            predicate = predicate.And(s => s.User.IsActive == IsActive.Value);
        }

        if (IsIdle.HasValue && Position != null)
        {
            var positions = Position.Split(',').ToList();
            foreach (var position in positions)
            {
                if (position == RoleEnum.MANAGER.ToString())
                {
                    predicate = predicate.And(GetManagerExpressions());
                }
                if (position == RoleEnum.DESIGNER.ToString())
                {
                    predicate = predicate.And(GetDesignerExpressions());
                }
                if (position == RoleEnum.CONSTRUCTOR.ToString())
                {
                    predicate = predicate.And(GetConstructorExpressions());
                }
            }
        }

        return predicate;
    }

    public Expression<Func<Staff, bool>> GetManagerExpressions()
    {
        var predicate = PredicateBuilder.New<Staff>(true);
        
        predicate = predicate.And(staff => 
            staff.Position == RoleEnum.MANAGER.ToString() &&
            staff.User.IsActive == true &&
            !staff.ProjectStaffs.Any(ps => 
                ps.Project.IsActive == true && 
                ps.Project.Status != EnumProjectStatus.FINISHED.ToString()));
        return predicate;
    }

    public Expression<Func<Staff, bool>> GetDesignerExpressions()
    {
        var predicate = PredicateBuilder.New<Staff>(true);

        predicate = predicate.And(staff => 
            staff.Position == RoleEnum.DESIGNER.ToString() &&
            staff.User.IsActive == true &&
            !staff.ProjectStaffs.Any(ps => 
                ps.Project.IsActive == true && 
                ps.Project.Status == EnumProjectStatus.DESIGNING.ToString()));
        return predicate;
    }

    public Expression<Func<Staff, bool>> GetConstructorExpressions()
    {
        var predicate = PredicateBuilder.New<Staff>(true);
        predicate = predicate.And(staff => 
            staff.Position == RoleEnum.CONSTRUCTOR.ToString() &&
            staff.User.IsActive == true &&
            // Constructor should not be in any project that is not finished
            !staff.ProjectStaffs.Any(ps => 
                ps.Project.IsActive == true &&
                ps.Project.Status != EnumProjectStatus.FINISHED.ToString()) &&
            // Constructor should not be in any maintenance request task (level 1) that is not done
            !staff.MaintenanceStaffs.Any(ms => 
                ms.MaintenanceRequestTask.ParentId == null && // Level 1 tasks (parent is null)
                ms.MaintenanceRequestTask.Status != EnumMaintenanceRequestTaskStatus.DONE.ToString())
                );
        return predicate;
    }
}
