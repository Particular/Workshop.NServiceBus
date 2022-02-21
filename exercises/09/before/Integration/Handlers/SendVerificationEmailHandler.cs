namespace Integration.Handlers
{
    using Messages.Commands;
    using Messages.Events;
    using NServiceBus;
    using NServiceBus.Logging;
    using System;
    using System.Threading.Tasks;

    public class SendVerificationEmailHandler : IHandleMessages<SendVerificationEmail>
    {
        private static readonly Random random = new Random();
        private static readonly ILog logger = LogManager.GetLogger<SendVerificationEmailHandler>();

        public async Task Handle(SendVerificationEmail message, IMessageHandlerContext context)
        {
            logger.Info($"Received {message.UserId} - {message.VerificationCode}");

            // Integrate with SMTP server
            var @event = new VerificationEmailSent
            {
                UserId = message.UserId,
                VerificationCode = message.VerificationCode
            };

            await context.Publish(@event)
                         .ConfigureAwait(false);

            if (random.Next(0, 4) == 0) throw new InvalidOperationException("Random failure!");
        }
    }
}