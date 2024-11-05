using Azure.Messaging.ServiceBus;
using EmailProvider.Models;

namespace EmailProvider.Interfaces
{
    public interface IEmailService
    {
        bool SendEmail(EmailRequest request);
        EmailRequest UnpackEmailRequest(ServiceBusReceivedMessage request);
    }
}