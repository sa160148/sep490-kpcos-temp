using KPCOS.BusinessLayer.DTOs.Request;
using KPCOS.BusinessLayer.DTOs.Response;
using KPCOS.Common.Exceptions;
using KPCOS.Common.Pagination;
using KPCOS.DataAccessLayer.Entities;
using KPCOS.DataAccessLayer.Enums;
using KPCOS.DataAccessLayer.Repositories;
using Microsoft.EntityFrameworkCore;

namespace KPCOS.BusinessLayer.Services.Implements;

public class UserService(IUnitOfWork unitOfWork) : IUserService
{
    public async Task<bool> RegiterStaffAsync(UserRequest request)
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

        await unitOfWork.Repository<User>().AddAsync(user, false);

        return await unitOfWork.SaveManualChangesAsync() > 0;
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
            Position = staff.Position
        });

        return staffResponses;
    }

    public async Task<int> CountStaffAsync()
    {
        return await unitOfWork.Repository<Staff>().Get().CountAsync();
    }

    private async Task<User?> UserExitByEmail(string email)
    {
        var isUser = await unitOfWork.Repository<User>()
            .SingleOrDefaultAsync(user => user.Email == email);
        return isUser;
    }
}