using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using MimeKit.Text;
using System;

namespace KPCOS.BusinessLayer.Services.Implements;

public class EmailService : IEmailService
{
    private readonly SmtpClient _smtpClient;

    public EmailService(SmtpClient smtpClient)
    {
        _smtpClient = smtpClient;
    }
    
    public async Task SentMailAsync(string toEmail, string subject, string body)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress("kpcos-noreply", "tongtbsa160148@fpt.edu.vn"));
        message.To.Add(new MailboxAddress("Khách Hàng", toEmail));

        message.Body = new TextPart(TextFormat.Html)
        {
            Text = body
        };
        
        await _smtpClient.ConnectAsync("smtp.gmail.com", 587, SecureSocketOptions.StartTls);
        _smtpClient.AuthenticationMechanisms.Remove("XOAUTH2");
        _smtpClient.AuthenticationMechanisms.Remove("XOAUTH2");
        await _smtpClient.AuthenticateAsync("tongtbsa160148@fpt.edu.vn", "nuvw ortf ruve klmr");
        await _smtpClient.SendAsync(message);
        await _smtpClient.DisconnectAsync(true);
    }

    /// <summary>
    /// Send email with OTP to verify contract.
    /// <para>Send email to user that containing OTP and ExpiresAt</para>
    /// </summary>
    /// <param name="userEmail"></param>
    /// <param name="otpCode"></param>
    /// <param name="expiresAt"></param>
    public async Task SendVerifyEmailContractOtpAsync(string userEmail, int otpCode, DateTime expiresAt)
    {
        var vietnamTime = TimeZoneInfo.ConvertTimeFromUtc(expiresAt, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"));
        string formattedTime = vietnamTime.ToString("dd/MM/yyyy HH:mm:ss");
        
        string body = $"<h1>Mã OTP xác nhận hợp đồng của bạn là: {otpCode}</h1>" +
                     $"<p>Mã OTP này sẽ hết hạn vào lúc {formattedTime} (GMT+7)</p>";
        string subject = "Xác nhận hợp đồng";
        await SentMailAsync(userEmail, subject, body);
    }
}