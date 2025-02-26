using KPCOS.BusinessLayer.DTOs.Request;
using KPCOS.BusinessLayer.DTOs.Response;
using KPCOS.Common.Exceptions;
using KPCOS.Common.Pagination;
using KPCOS.DataAccessLayer.Entities;
using KPCOS.DataAccessLayer.Enums;
using KPCOS.DataAccessLayer.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Quartz.Logging;

namespace KPCOS.BusinessLayer.Services.Implements;

public class UserService(IUnitOfWork unitOfWork, ILogger<UserService> logger) : IUserService
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

        var yes = RoleEnum.CONSTRUCTOR.ToString();

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

    public async Task<IEnumerable<StaffResponse>> GetsStaffAsync(PaginationFilter filter)
    {
        var repo = unitOfWork.Repository<Staff>();
        var staffs = repo.Get(
            filter: null,
            orderBy: null,
            "User",
            filter.PageNumber,
            filter.PageSize);

        var staffResponses = staffs.Select(staff => new StaffResponse
        {
            Id = staff.Id,
            FullName = staff.User.FullName,
            Email = staff.User.Email,
            Phone = staff.User.Phone,
            Position = staff.Position,
            Avatar = staff.User.Avatar,
            IsActive = staff.User.IsActive
        });

        return staffResponses;
    }

    public async Task<int> CountStaffAsync()
    {
        return await unitOfWork.Repository<Staff>().Get().CountAsync();
    }

    public async Task<(IEnumerable<StaffResponse> Data, int TotalRecords)> GetsConsultantAsync(PaginationFilter filter)
    {
        var repo = unitOfWork.Repository<Staff>();
        // get all consultant not have in  process project
        var pageData = await repo.Get()
            .Where(staff => staff.Position == RoleEnum.CONSULTANT.ToString() &&
                            !staff.ProjectStaffs.
                                Any(ps => ps.Project.IsActive == true && 
                                          ps.Project.Status == EnumProjectStatus.PROCESSING.ToString())) 
            .Include("User")
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();


        var totalRecords = await repo.Get()
            .Where(staff => staff.Position == RoleEnum.CONSULTANT.ToString() &&
                            !staff.ProjectStaffs.
                                Any(ps => ps.Project.IsActive == true && 
                                          ps.Project.Status == EnumProjectStatus.PROCESSING.ToString())) 
            .CountAsync();
       
        
        var data = pageData.Select(staff => new StaffResponse
        {
            Id = staff.User.Id,
            FullName = staff.User.FullName,
            Email = staff.User.Email,
            Phone = staff.User.Phone,
            Position = staff.Position,
            Avatar = staff.User.Avatar
           
        });
        
        
        return (data, totalRecords);
    }

    private async Task<User?> UserExitByEmail(string email)
    {
        var isUser = await unitOfWork.Repository<User>()
            .SingleOrDefaultAsync(user => user.Email == email);
        return isUser;
    }
}