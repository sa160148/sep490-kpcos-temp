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
            throw new NotFoundException("user not found");
        }

        if (userRaw.Password != request.Password)
        {
            throw new BadRequestException("password is incorrect");
        }
        var role = await CheckRole(userRaw.Id);
        return new SigninResponse
        {
            Token = await GenerateToken(userRaw, role!),
            Role = role!,
            User = new UserResponse
            {
                Avatar = userRaw.Avatar,
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

            /*var customer = new Customer
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Address = request.Address,
                Dob = request.Dob,
                Gender = request.Gender,
                IsActive = true
            };*/

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
            expires: DateTime.Now.AddHours(1),
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