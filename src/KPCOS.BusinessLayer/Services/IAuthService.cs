using KPCOS.BusinessLayer.DTOs.Request;
using KPCOS.BusinessLayer.DTOs.Response;
using KPCOS.DataAccessLayer.Enums;

namespace KPCOS.BusinessLayer.Services;

public interface IAuthService
{
    Task<SigninResponse> SignInAsync(SigninRequest request);
    Task SignUpAsync(SignupRequest request);
    Task<bool> IsCustomerAsync(Guid userId);

    /// <summary>
    /// return position of user, no any of customer or staff
    /// <para>will return a exception</para>
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    Task<RoleEnum> GetPositionAsync(Guid userId);
}