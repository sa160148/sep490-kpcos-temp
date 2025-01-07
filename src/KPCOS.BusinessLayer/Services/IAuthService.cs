using KPCOS.BusinessLayer.DTOs.Request;
using KPCOS.BusinessLayer.DTOs.Response;
using KPCOS.BusinessLayer.DTOs.Response.Objects;

namespace KPCOS.BusinessLayer.Services;

public interface IAuthService
{
    Task<SigninResponse> SignInAsync(SigninRequest request);
    Task SignUpAsync(SignupRequest request);
}