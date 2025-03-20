using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AutoMapper;
using KPCOS.BusinessLayer.DTOs.Request;
using KPCOS.BusinessLayer.DTOs.Response;
using KPCOS.DataAccessLayer.Enums;
using KPCOS.DataAccessLayer.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using KPCOS.Common.Exceptions;
using KPCOS.DataAccessLayer.Entities;
using Microsoft.EntityFrameworkCore;

namespace KPCOS.BusinessLayer.Services.Implements;

public class AuthService(IUnitOfWork unitOfWork, IConfiguration configuration, IMapper mapper) : IAuthService
{

    public async Task<SigninResponse> SignInAsync(SigninRequest request)
    {
        IRepository<User> userRepo = unitOfWork.Repository<User>();
        var userRaw = await userRepo.SingleOrDefaultAsync(user => user.Email == request.Email);
        
        if (userRaw == null)
        {
            throw new NotFoundException("Không tìm thấy tài khoản");
        }

        if (userRaw.Password != request.Password)
        {
            throw new BadRequestException("Sai mật khẩu");
        }
        var role = await CheckRole(userRaw.Id);

        if ( role == null)
        {
            throw new NotFoundException("Không tìm thấy role");
            
        }
        return new SigninResponse
        {
            Token = await GenerateToken(userRaw, role!),
            Role = role!,
            User = new CustomerResponse()
            {
                Avatar = userRaw.Avatar ?? "",
                FullName = userRaw.FullName
            }
        };

    }

    public async Task SignUpAsync(SignupRequest request)
    {
        try
        {
            var userRepo = unitOfWork.Repository<User>();
            if (await userRepo.SingleOrDefaultAsync(user => user.Email == request.Email) != null)
            {
                throw new BadRequestException("user đã tồn tại");
            }
            var userId = Guid.NewGuid();
            var user = mapper.Map<User>(request);
            user.Status = "";
            user.Id = userId;

            user.Customers.Add(new Customer
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Address = request.Address,
                Dob = request.Dob,
                Gender = request.Gender,
                IsActive = true
            });

            await userRepo.AddAsync(user, false);

            var isSaved = await userRepo.SaveAsync();
            if (isSaved == 0)
            {
                throw new Exception("save user failed");
            }
        }
        catch (Exception e)
        {
            Console.Write("unknow ex: " + e);
            throw;
        }
    }

    public async Task<bool> IsCustomerAsync(Guid userId)
    {
        var isStaff = await unitOfWork.Repository<Staff>()
            .SingleOrDefaultAsync(staff => staff.UserId == userId) != null;
        if (isStaff)
        {
            return false;
        }
        return true;
    }

    public async Task<RoleEnum> GetPositionAsync(Guid userId)
    {
        var staff = await unitOfWork.Repository<Staff>()
            .SingleOrDefaultAsync(staff => staff.UserId == userId);
        if (staff != null)
        {
            return Enum.Parse<RoleEnum>(staff.Position);
        }
        var customer = await unitOfWork.Repository<Customer>()
            .SingleOrDefaultAsync(customer => customer.UserId == userId);
        if (customer != null)
        {
            return RoleEnum.CUSTOMER;
        }
        throw new Exception("User không có customer hay staff");
    }

    private async Task<string> GenerateToken(User user, string role)
    {
        var secret = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Secret"]!));
        var credentials = new SigningCredentials(secret, SecurityAlgorithms.HmacSha256);
        if (role == null)
        {
            throw new NotFoundException();
        }
        var tokenDescript = new JwtSecurityToken(
            issuer: configuration["Jwt:Issuer"],
            audience: configuration["Jwt:Audience"],
            /*expires: DateTime.Now.AddHours(1),*/
            signingCredentials: credentials,
            claims: [
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Role, role)
            ]
        );
        return new JwtSecurityTokenHandler().WriteToken(tokenDescript);
    }

    private async Task<string?> CheckRole(Guid userId)
    {
        var isCustomer = await unitOfWork.Repository<Customer>()
            .SingleOrDefaultAsync(customer => customer.UserId == userId);

        if (isCustomer != null)
        {
            return RoleEnum.CUSTOMER.ToString();
        }

        var isStaff = await unitOfWork.Repository<Staff>()
            .SingleOrDefaultAsync(staff => staff.UserId == userId);
        if (isStaff != null)
        {
            return isStaff.Position.ToUpperInvariant();
        }
        return null;
    }
}