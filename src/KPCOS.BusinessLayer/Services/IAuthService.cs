using KPCOS.DataAccessLayer.DTOs.Request;
using KPCOS.DataAccessLayer.DTOs.Response;

namespace KPCOS.BusinessLayer.Services;

public interface IAuthService
{
    Task<SigninResponse> SignInAsync(SigninRequest request);
    Task SignUpAsync(SignupRequest request);
}