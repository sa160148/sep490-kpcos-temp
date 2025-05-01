using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using MimeKit.Text;
using System;

namespace KPCOS.BusinessLayer.Services.Implements;

public class EmailService : IEmailService
{
    private readonly SmtpClient _smtpClient;
    private const string LOGO_URL = "https://res.cloudinary.com/dxztbchud/image/upload/v1746014739/iilrxl5f0e66tb3bcun7.jpg";

    public EmailService(SmtpClient smtpClient)
    {
        _smtpClient = smtpClient;
    }
    
    public async Task SentMailAsync(string toEmail, string subject, string body)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress("KPCOS System", "tongtbsa160148@fpt.edu.vn"));
        message.To.Add(new MailboxAddress("Khách Hàng", toEmail));
        message.Subject = subject;
        
        var emailBody = $@"
            <!DOCTYPE html>
            <html>
            <head>
                <meta charset='utf-8'>
                <style>
                    body {{
                        font-family: Arial, sans-serif;
                        line-height: 1.6;
                        color: #333333;
                        margin: 0;
                        padding: 0;
                    }}
                    .container {{
                        max-width: 600px;
                        margin: 0 auto;
                        padding: 20px;
                    }}
                    .header {{
                        text-align: center;
                        padding: 20px 0;
                        background-color: #f8f9fa;
                        border-radius: 5px;
                        margin-bottom: 20px;
                    }}
                    .logo {{
                        max-width: 200px;
                        height: auto;
                    }}
                    .content {{
                        background-color: #ffffff;
                        padding: 20px;
                        border-radius: 5px;
                        box-shadow: 0 2px 4px rgba(0,0,0,0.1);
                    }}
                    .footer {{
                        text-align: center;
                        margin-top: 20px;
                        padding: 20px;
                        color: #666666;
                        font-size: 12px;
                    }}
                    h1 {{
                        color: #2c3e50;
                        margin-bottom: 20px;
                    }}
                    p {{
                        margin-bottom: 15px;
                    }}
                </style>
            </head>
            <body>
                <div class='container'>
                    <div class='header'>
                        <img src='{LOGO_URL}' alt='KPCOS Logo' class='logo'>
                    </div>
                    <div class='content'>
                        {body}
                    </div>
                    <div class='footer'>
                        <p>© {DateTime.Now.Year} KPCOS. All rights reserved.</p>
                        <p>This is an automated message, please do not reply to this email.</p>
                    </div>
                </div>
            </body>
            </html>";
        
        message.Body = new TextPart(TextFormat.Html)
        {
            Text = emailBody
        };
        
        await _smtpClient.ConnectAsync("smtp.gmail.com", 587, SecureSocketOptions.StartTls);
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
        
        string body = $@"
            <h1>Xác nhận hợp đồng</h1>
            <p>Kính gửi Quý khách hàng,</p>
            <p>Mã OTP xác nhận hợp đồng của bạn là:</p>
            <h2 style='color: #2c3e50; font-size: 32px; text-align: center; padding: 20px; background-color: #f8f9fa; border-radius: 5px;'>{otpCode}</h2>
            <p>Mã OTP này sẽ hết hạn vào lúc <strong>{formattedTime}</strong> (GMT+7)</p>
            <p>Vui lòng không chia sẻ mã OTP này với bất kỳ ai để đảm bảo an toàn cho hợp đồng của bạn.</p>
            <p>Trân trọng,<br>KPCOS Team</p>";
            
        string subject = "Xác nhận hợp đồng - KPCOS";
        await SentMailAsync(userEmail, subject, body);
    }

    public async Task SendVerifyDocOtpAsync(string userEmail, string docName, int otpCode, DateTime expiresAt)
    {
        var vietnamTime = TimeZoneInfo.ConvertTimeFromUtc(expiresAt, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"));
        string formattedTime = vietnamTime.ToString("dd/MM/yyyy HH:mm:ss");

        string body = $@"
            <h1>Xác nhận tài liệu</h1>
            <p>Kính gửi Quý khách hàng,</p>
            <p>Mã OTP xác nhận tài liệu <strong>{docName}</strong> của bạn là:</p>
            <h2 style='color: #2c3e50; font-size: 32px; text-align: center; padding: 20px; background-color: #f8f9fa; border-radius: 5px;'>{otpCode}</h2>
            <p>Mã OTP này sẽ hết hạn vào lúc <strong>{formattedTime}</strong> (GMT+7)</p>
            <p>Vui lòng không chia sẻ mã OTP này với bất kỳ ai để đảm bảo an toàn cho tài liệu của bạn.</p>
            <p>Trân trọng,<br>KPCOS Team</p>";
            
        string subject = "Xác nhận tài liệu - KPCOS";
        await SentMailAsync(userEmail, subject, body);
    }
}