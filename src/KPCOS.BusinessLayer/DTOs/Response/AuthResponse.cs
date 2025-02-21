namespace KPCOS.BusinessLayer.DTOs.Response;

public class AuthResponse
{

}

public class SigninResponse
{
    public string Token { get; set; }
    public string Role { get; set; }
    public CustomerResponse User { get; set; } /*= new UserResponse();*/
}

public class CustomerResponse
{
    public string FullName { get; set; }
    public string Avatar { get; set; }
}