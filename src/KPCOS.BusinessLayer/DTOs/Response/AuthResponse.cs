namespace KPCOS.BusinessLayer.DTOs.Response;

public class AuthResponse
{

}

public class SigninResponse
{
    public String Token { get; set; }
    public UserResponse User { get; set; } /*= new UserResponse();*/
}