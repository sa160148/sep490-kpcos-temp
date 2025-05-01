using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using MimeKit.Text;
using System;

namespace KPCOS.BusinessLayer.Services.Implements;

public class EmailService : IEmailService
{
    private readonly SmtpClient _smtpClient;
    private readonly string _contractTemplatePath;
    private readonly string _docTemplatePath;
    private readonly string _logoPath;

    public EmailService(SmtpClient smtpClient)
    {
        _smtpClient = smtpClient;
        _contractTemplatePath = "../../KPCOS.Common/Constants/EmailTemplates/ContractTemplate.html";
        _docTemplatePath = "../../KPCOS.Common/Constants/EmailTemplates/DocTemplate.html";
        _logoPath = "../../KPCOS.Common/Constants/EmailTemplates/full_logo.jpg";
    }
    
    public async Task SentMailAsync(string toEmail, string subject, string body)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress("kpcos-noreply", "tongtbsa160148@fpt.edu.vn"));
        message.To.Add(new MailboxAddress("Khách Hàng", toEmail));
        message.Subject = subject;
        
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

    private async Task SendMailWithTemplateAsync(string toEmail, string subject, string templateContent, string logoPath)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress("kpcos-noreply", "tongtbsa160148@fpt.edu.vn"));
        message.To.Add(new MailboxAddress("Khách Hàng", toEmail));
        message.Subject = subject;

        var builder = new BodyBuilder();
        
        // Add the logo as a linked resource
        var image = builder.LinkedResources.Add(logoPath);
        image.ContentId = "logo";

        // Set the HTML body
        builder.HtmlBody = templateContent;

        message.Body = builder.ToMessageBody();

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
        
        // Get current time in Vietnam timezone
        var currentTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"));
        string sentTime = currentTime.ToString("dd/MM/yyyy HH:mm:ss");
        
        // Read the template file
        string templateContent = await File.ReadAllTextAsync(_contractTemplatePath);
        
        // Replace placeholders with actual values
        templateContent = templateContent.Replace("{OTP_CODE}", otpCode.ToString())
                                      .Replace("{EXPIRY_TIME}", formattedTime)
                                      .Replace("{SENT_TIME}", sentTime);
        
        string subject = "Xác nhận hợp đồng";
        await SendMailWithTemplateAsync(userEmail, subject, templateContent, _logoPath);
    }

    public async Task SendVerifyDocOtpAsync(string userEmail, string docName, int otpCode, DateTime expiresAt)
    {
        var vietnamTime = TimeZoneInfo.ConvertTimeFromUtc(expiresAt, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"));
        string formattedTime = vietnamTime.ToString("dd/MM/yyyy HH:mm:ss");
        
        // Get current time in Vietnam timezone
        var currentTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"));
        string sentTime = currentTime.ToString("dd/MM/yyyy HH:mm:ss");

        // Read the template file
        string templateContent = await File.ReadAllTextAsync(_docTemplatePath);
        
        // Replace placeholders with actual values
        templateContent = templateContent.Replace("{DOC_NAME}", docName)
                                      .Replace("{OTP_CODE}", otpCode.ToString())
                                      .Replace("{EXPIRY_TIME}", formattedTime)
                                      .Replace("{SENT_TIME}", sentTime);
        
        string subject = "Xác nhận tài liệu";
        await SendMailWithTemplateAsync(userEmail, subject, templateContent, _logoPath);
    }
}