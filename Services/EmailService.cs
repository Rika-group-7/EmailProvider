using Azure;
using Azure.Communication.Email;
using Azure.Messaging.ServiceBus;
using EmailProvider.Interfaces;
using EmailProvider.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace EmailProvider.Services;

public class EmailService : IEmailService
{
    private readonly EmailClient _emailClient;
    private readonly ILogger<EmailService> _logger;

    public EmailService(ILogger<EmailService> logger, EmailClient emailClient)
    {
        _logger = logger;
        _emailClient = emailClient;
    }

    public EmailRequest UnpackEmailRequest(ServiceBusReceivedMessage request)
    {
        try
        {
            var emailRequest = JsonConvert.DeserializeObject<EmailRequest>(request.Body.ToString());
            if (emailRequest != null)
            {
                _logger.LogInformation("Successfully unpacked email request");
                return emailRequest;
            }
            else
            {
                _logger.LogError("Email request is null after deserialization");
            }

        }
        catch (Exception ex)
        {
            _logger.LogError($"ERROR : EmailService.UnpackEmailRequest :: {ex.Message}");
        }

        return null!;
    }

    public bool SendEmail(EmailRequest request)
    {
        try
        {
            var result = _emailClient.Send(WaitUntil.Completed,
            senderAddress: Environment.GetEnvironmentVariable("SenderAddress"),
            recipientAddress: request.To,
            subject: request.Subject,
            htmlContent: request.Body,
            plainTextContent: request.PlainTextContent);
            if (result.HasCompleted)
            {
                _logger.LogError("Email sent successfully");
                return true;
            }
            else
                _logger.LogError("Email sending did not complete successfully");
        }

        catch (Exception ex)
        {
            _logger.LogError($"ERROR : EmailService.SendEmail :: {ex.Message}");
        }

        return false;
    }

}
