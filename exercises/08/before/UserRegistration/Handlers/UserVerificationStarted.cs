namespace UserRegistration.Handlers
{
    using Messages.Events;
    using NServiceBus;
    using NServiceBus.Logging;
    using System.Threading.Tasks;

    public class UserVerificationStartedHandler : IHandleMessages<UserVerificationStarted>
    {
        private static readonly ILog logger = LogManager.GetLogger<UserVerificationStartedHandler>();

        public Task Handle(UserVerificationStarted message, IMessageHandlerContext context)
        {
            logger.Info($"Received {message.EmailAddress}");

            // We should not store the data in our own database from within the saga.
            // We could also store the data on incoming RegisterNewUser message, but then we'd do that in the same UnitOfWork as the saga
            // Instead we decided to create a new message that is EXACTLY the original message and publish that as an event

            var userid = message.UserId;
            var name = message.Name;
            var email = message.EmailAddress;

            // Store it into the database

            return Task.CompletedTask;
        }
    }
}