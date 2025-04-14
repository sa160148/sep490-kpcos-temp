using System.Linq.Expressions;
using AutoMapper;
using KPCOS.BusinessLayer.DTOs.Request;
using KPCOS.BusinessLayer.DTOs.Request.Users;
using KPCOS.BusinessLayer.DTOs.Response;
using KPCOS.BusinessLayer.DTOs.Response.Users;
using KPCOS.Common.Exceptions;
using KPCOS.Common.Pagination;
using KPCOS.Common.Utilities;
using KPCOS.DataAccessLayer.Entities;
using KPCOS.DataAccessLayer.Enums;
using KPCOS.DataAccessLayer.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace KPCOS.BusinessLayer.Services.Implements;

public class UserService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<UserService> logger) : IUserService
{
    public async Task RegiterStaffAsync(UserRequest request)
    {
        if (await UserExitByEmail(request.Email) != null)
        {
            throw new BadRequestException("Email đã tồn tại");
        }

        /*if ()
        {
            throw new Exception("Chức vụ không đúng");
        }*/
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            FullName = request.FullName,
            Email = request.Email,
            Password = request.Password,
            Phone = request.Phone
        };
        user.Staff.Add(new Staff
        {
            Position = request.Position.ToString(),
            User = user
        });

        await unitOfWork.Repository<User>().AddAsync(user);
    }

    public async Task<(IEnumerable<StaffResponse> Data, int TotalRecords)> GetsStaffAsync(GetAllStaffRequest filter)
    {
        var repo = unitOfWork.Repository<Staff>();
        var staffs = repo.GetWithCount(
            filter: filter.GetExpressions(),
            orderBy: filter.GetOrder(),
            includeProperties: "User",
            pageIndex: filter.PageNumber,
            pageSize: filter.PageSize);

        var staffResponses = mapper.Map<IEnumerable<StaffResponse>>(staffs.Data);

        return (staffResponses, staffs.Count);
    }

    public async Task<int> CountStaffAsync()
    {
        return await unitOfWork.Repository<Staff>().Get().CountAsync();
    }

    public async Task<(IEnumerable<StaffResponse> Data, int TotalRecords)> GetsConsultantAsync(
        GetAllStaffRequest filter)
    {
        var staffs = unitOfWork.Repository<Staff>()
        .GetWithCount(
            filter: filter.GetConsultantExpressions(),
            orderBy: filter.GetOrder(),
            includeProperties: "User",
            pageIndex: filter.PageNumber,
            pageSize: filter.PageSize);

        // get all consultant not have in  process project
        // var pageData = await repo.Get()
        //     .Where(staff => staff.Position == RoleEnum.CONSULTANT.ToString() &&
        //                     !staff.ProjectStaffs.
        //                         Any(ps => ps.Project.IsActive == true && 
        //                                   ps.Project.Status == EnumProjectStatus.PROCESSING.ToString())) 
        //     .Include("User")
        //     .Skip((filter.PageNumber - 1) * filter.PageSize)
        //     .Take(filter.PageSize)
        //     .ToListAsync();
        // var totalRecords = await repo.Get()
        //     .Where(staff => staff.Position == RoleEnum.CONSULTANT.ToString() &&
        //                     !staff.ProjectStaffs.
        //                         Any(ps => ps.Project.IsActive == true && 
        //                                   ps.Project.Status == EnumProjectStatus.PROCESSING.ToString())) 
        //     .CountAsync();

        var data = mapper.Map<IEnumerable<StaffResponse>>(staffs.Data);
        
        return (data, staffs.Count);
    }

    public async Task<(IEnumerable<StaffResponse> data, int total)> GetsManagerAsync(GetAllStaffRequest filter)
    {
        var repo = unitOfWork.Repository<Staff>();
        // Expression<Func<Staff, bool>> advanceFilter = staff => 
        //     staff.Position == RoleEnum.MANAGER.ToString() &&
        //     staff.User.IsActive == true &&
        //     !staff.ProjectStaffs.Any(ps => 
        //         ps.Project.IsActive == true && 
        //         ps.Project.Status != EnumProjectStatus.FINISHED.ToString());
        var staffs = repo.GetWithCount(
            filter: filter.GetManagerExpressions(),
            orderBy: filter.GetOrder(),
            includeProperties: "User",
            pageIndex: filter.PageNumber,
            pageSize: filter.PageSize);
        var response = mapper.Map<IEnumerable<StaffResponse>>(staffs.Data);
        return (response, staffs.Count);
    }

    public async Task<(IEnumerable<StaffResponse> data, int total)> GetsDesignerAsync(PaginationFilter filter)
    {
        var repo = unitOfWork.Repository<Staff>();
        Expression<Func<Staff, bool>> advanceFilter = staff => 
            staff.Position == RoleEnum.DESIGNER.ToString() &&
            staff.User.IsActive == true &&
            !staff.ProjectStaffs.Any(ps => 
                ps.Project.IsActive == true && 
                ps.Project.Status == EnumProjectStatus.DESIGNING.ToString());
        var staffs = repo.GetWithCount(
            filter: advanceFilter,
            orderBy: null,
            "User",
            filter.PageNumber,
            filter.PageSize);
        var response = mapper.Map<IEnumerable<StaffResponse>>(staffs.Data);
        return (response, staffs.Count);
    }

    public async Task<(IEnumerable<StaffResponse> data, int total)> GetsConstructorAsync(PaginationFilter filter)
    {
        var repo = unitOfWork.Repository<Staff>();
        Expression<Func<Staff, bool>> advanceFilter = staff => 
            staff.Position == RoleEnum.CONSTRUCTOR.ToString() &&
            staff.User.IsActive == true &&
            // Constructor should not be in any project that is not finished
            !staff.ProjectStaffs.Any(ps => 
                ps.Project.IsActive == true &&
                ps.Project.Status != EnumProjectStatus.FINISHED.ToString()) &&
            // Constructor should not be in any maintenance request task (level 1) that is not done
            !staff.MaintenanceStaffs.Any(ms => 
                ms.MaintenanceRequestTask.ParentId == null && // Level 1 tasks (parent is null)
                ms.MaintenanceRequestTask.Status != EnumMaintenanceRequestTaskStatus.DONE.ToString());
            
        var staffs = repo.GetWithCount(
            filter: advanceFilter,
            orderBy: null,
            "User",
            filter.PageNumber,
            filter.PageSize);
        var response = mapper.Map<IEnumerable<StaffResponse>>(staffs.Data);
        return (response, staffs.Count);
    }

    private async Task<User?> UserExitByEmail(string email)
    {
        var isUser = await unitOfWork.Repository<User>()
            .SingleOrDefaultAsync(user => user.Email == email);
        return isUser;
    }

    /// <summary>
    /// Lấy thông tin chi tiết người dùng theo ID
    /// </summary>
    /// <param name="id">ID người dùng</param>
    /// <returns>Thông tin chi tiết người dùng</returns>
    public async Task<GetDetailUserResponse> GetUserByIdAsync(Guid id)
    {
        var user =  unitOfWork.Repository<User>()
            .Get(   
                filter: u => u.Id == id,
                includeProperties: "Staff,Customers"
            )
            .SingleOrDefault();

        if (user == null)
        {
            throw new NotFoundException("Người dùng không tồn tại");
        }

        var response = mapper.Map<GetDetailUserResponse>(user);
        return response;
    }

    /// <summary>
    /// Lấy danh sách tất cả người dùng
    /// </summary>
    /// <param name="filter">Bộ lọc phân trang</param>
    /// <returns>Danh sách người dùng</returns>
    public async Task<(IEnumerable<GetDetailUserResponse> Data, int TotalRecords)> GetAllUsersAsync(
        GetAllUserFilterRequest filter)
    {
        var repo = unitOfWork.Repository<User>();
        var users = repo.GetWithCount(
            filter: filter.GetExpressions(),
            orderBy: filter.GetOrder(),
            includeProperties: "Staff,Customers",
            pageIndex: filter.PageNumber,
            pageSize: filter.PageSize);

        var response = mapper.Map<IEnumerable<GetDetailUserResponse>>(users.Data);
        return (response, users.Count);
    }

    /// <summary>
    /// Cập nhật thông tin người dùng
    /// </summary>
    /// <param name="id">ID người dùng</param>
    /// <param name="request">Thông tin cập nhật</param>
    public async Task UpdateUserAsync(
        Guid id, 
        CommandUserRequest request)
    {
        var user = unitOfWork.Repository<User>()
            .Get(
                filter: u => u.Id == id,
                includeProperties: "Staff,Customers"
            )
            .SingleOrDefault();

        if (user == null)
        {
            throw new NotFoundException("Người dùng không tồn tại");
        }

        // Use ReflectionUtil to update properties
        ReflectionUtil.UpdateProperties(request, user);

        // If position is being updated and user is staff
        if (!string.IsNullOrEmpty(request.Position) && user.Staff.Any())
        {
            var staff = user.Staff.First();
            staff.Position = request.Position;
        }

        await unitOfWork.Repository<User>().UpdateAsync(user);
    }
}