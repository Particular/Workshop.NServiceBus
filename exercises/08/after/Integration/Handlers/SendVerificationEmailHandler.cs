using System.Threading.Tasks;

namespace Integration.Handlers
{
    using System;
    using Integration.Messages.Commands;
    using Integration.Messages.Events;
    using NServiceBus;
    using NServiceBus.Logging;

    public class SendVerificationEmailHandler : IHandleMessages<SendVerificationEmail>
    {
        static ILog logger = LogManager.GetLogger<SendVerificationEmailHandler>();

        public Task Handle(SendVerificationEmail message, IMessageHandlerContext context)
        {
            logger.Info($"Received {message.UserId} - {message.VerificationCode}");

            // Integrate with SMTP server
            var @event = new VerificationEmailSent
            {
                UserId = message.UserId,
                VerificationCode = message.VerificationCode
            };

            return context.Publish(@event);
        }
    }
}
