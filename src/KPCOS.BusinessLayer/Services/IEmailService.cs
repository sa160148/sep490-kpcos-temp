namespace KPCOS.BusinessLayer.Services;

public interface IEmailService
{
    Task SendVerifyEmailContractOtpAsync(string userEmail, int otpCode, DateTime expiresAt);
    Task SendVerifyDocOtpAsync(string userEmail, string docName, int otpCode, DateTime expiresAt);
}