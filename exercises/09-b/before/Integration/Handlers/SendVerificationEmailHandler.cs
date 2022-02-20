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
        static Random random = new Random();
        static ILog logger = LogManager.GetLogger<SendVerificationEmailHandler>();

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
