namespace Integration.Handlers
{
    using Messages.Commands;
    using Messages.Events;
    using NServiceBus;
    using NServiceBus.Logging;
    using System.Threading.Tasks;

    public class SendVerificationEmailHandler : IHandleMessages<SendVerificationEmail>
    {
        private static readonly ILog logger = LogManager.GetLogger<SendVerificationEmailHandler>();

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