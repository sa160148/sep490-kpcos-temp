namespace KPCOS.BusinessLayer.Exceptions.ExceptionProfiles;

public static class AuthErrors
{
    public static readonly CustomError SignInFailed = new("User.", 
        "Sign in failed");
    public static readonly CustomError UserInActive = new("User.IsActive", 
        "User is inactive");
    public static  CustomError NotFoundByEmail(string email) => new("User.", 
        "User not found by email: " + email);
} 