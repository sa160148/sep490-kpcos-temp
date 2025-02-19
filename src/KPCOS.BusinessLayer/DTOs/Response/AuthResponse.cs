namespace KPCOS.BusinessLayer.DTOs.Response;

public class AuthResponse
{

}

public class SigninResponse
{
    public string Token { get; set; }
    public string Role { get; set; }
    public UserResponse User { get; set; } /*= new UserResponse();*/
}