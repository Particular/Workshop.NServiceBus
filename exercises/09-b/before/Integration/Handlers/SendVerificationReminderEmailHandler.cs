using System.Threading.Tasks;

namespace Integration.Handlers
{
    using Integration.Messages.Commands;
    using NServiceBus;
    using NServiceBus.Logging;

    public class SendVerificationReminderEmailHandler : IHandleMessages<SendVerificationReminderEmail>
    {
        static ILog logger = LogManager.GetLogger<SendVerificationReminderEmailHandler>();

        public Task Handle(SendVerificationReminderEmail message, IMessageHandlerContext context)
        {
            logger.Info($"Should send another email, but ignoring {message.VerificationCode} for {message.UserId}");

            return Task.Delay(1500);
        }
    }
}
