using Azure.Messaging.ServiceBus;
using EmailProvider.Interfaces;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace EmailProvider.Functions;

public class EmailSender
{
    private readonly ILogger<EmailSender> _logger;
    private readonly IEmailService _emailService;

    public EmailSender(ILogger<EmailSender> logger, IEmailService emailService)
    {
        _logger = logger;
        _emailService = emailService;
    }

    [Function(nameof(EmailSender))]
    public async Task Run(
        [ServiceBusTrigger("email-queue", Connection = "ServiceBusConnection")]
        ServiceBusReceivedMessage message,
        ServiceBusMessageActions messageActions)
    {

        try
        {
            var emailRequest = _emailService.UnpackEmailRequest(message);
            if (emailRequest != null && !string.IsNullOrEmpty(emailRequest.To))
            {

                _logger.LogInformation($"Sending email to {emailRequest.To}");

                if (_emailService.SendEmail(emailRequest))
                {
                    await messageActions.CompleteMessageAsync(message);
                    _logger.LogInformation($"Email sent to {emailRequest.To} and message completed");
                }
                else
                {
                    _logger.LogError($"Failed to send email to {emailRequest.To}");
                }
            }
            else
            {
                _logger.LogError($"Invalid email request or recipient address is empty");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"ERROR : EmailSender.Run :: {ex.Message}");
        }
        
    }
}
