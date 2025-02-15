using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using KPCOS.BusinessLayer.DTOs.Request;
using KPCOS.BusinessLayer.DTOs.Response;
using KPCOS.DataAccessLayer.Enums;
using KPCOS.DataAccessLayer.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using KPCOS.Common.Exceptions;
using KPCOS.DataAccessLayer;
using KPCOS.DataAccessLayer.Entities;

namespace KPCOS.BusinessLayer.Services.Implements;

public class AuthService : IAuthService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IConfiguration _configuration;
    private IAuthService _authServiceImplementation;

    public AuthService(IUnitOfWork unitOfWork, IConfiguration configuration)
    {
        _unitOfWork = unitOfWork;
        _configuration = configuration;
    }

    public async Task<SigninResponse> SignInAsync(SigninRequest request)
    {
        IRepository<User> userRepo = _unitOfWork.Repository<User>();
        var userRaw = await userRepo.SingleOrDefaultAsync(user => user!.Email == request.Email);
        if (userRaw == null)
        {
            throw new NotFoundException("user not found");
        }

        if (userRaw.Password != request.Password)
        {
            throw new BadRequestException("password is incorrect");
        }

        return new SigninResponse
        {
            Token = GenerateToken(userRaw)
        };
    }

    public async Task SignUpAsync(SignupRequest request)
    {
        var userRepo = _unitOfWork.Repository<User>();
        var isUserExit = await userRepo.SingleOrDefaultAsync(user => user.Email == request.Email);
        if (isUserExit != null)
        {
            throw new Exception("user exit");
        }
        Guid userId = Guid.NewGuid();
        var user = new User()
        {
            Id = userId,
            Email = request.Email,
            Password = request.Password,
            CreatedAt = DateTime.UtcNow,
            FullName = request.Fullname,
            IsActive = true,
            // Role = RoleEnum.CUSTOMER.ToString(),
        };

        await userRepo.AddAsync(user, false);
        try
        {
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

    private string GenerateToken(User user)
    {
        var secret = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Secret"]!));
        var credentials = new SigningCredentials(secret, SecurityAlgorithms.HmacSha256);
        
        var tokenDescript = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            expires: DateTime.Now.AddHours(1),
            signingCredentials: credentials,
            claims: [
                new Claim(ClaimTypes.Name, user.Email),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                // new Claim(ClaimTypes.Role, user.Role.ToString())
            ]
        );
        return new JwtSecurityTokenHandler().WriteToken(tokenDescript);
    }
}