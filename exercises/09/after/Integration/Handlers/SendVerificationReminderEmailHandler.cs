namespace Integration.Handlers
{
    using Messages.Commands;
    using NServiceBus;
    using NServiceBus.Logging;
    using System.Threading.Tasks;

    public class SendVerificationReminderEmailHandler : IHandleMessages<SendVerificationReminderEmail>
    {
        private static readonly ILog logger = LogManager.GetLogger<SendVerificationReminderEmailHandler>();

        public Task Handle(SendVerificationReminderEmail message, IMessageHandlerContext context)
        {
            logger.Info($"Should send another email, but ignoring {message.VerificationCode} for {message.UserId}");

            return Task.Delay(1500);
        }
    }
}