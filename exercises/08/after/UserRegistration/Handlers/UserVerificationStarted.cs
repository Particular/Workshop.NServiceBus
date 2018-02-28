using System.Threading.Tasks;

namespace UserRegistration.Handlers
{
    using NServiceBus;
    using UserRegistration.Messages.Events;
    public class UserVerificationStartedHandler : IHandleMessages<UserVerificationStarted>
    {
        public Task Handle(UserVerificationStarted message, IMessageHandlerContext context)
        {
            // We should not store the data in our own database from within the saga.
            // We could also store the data on incoming RegisterNewUser message, but then we'd do that in the same UnitOfWork as the saga
            // Instead we decided to create a new message that is EXACTLY the original message and publish that as an event

            var userid = message.UserId;
            var name = message.Name;
            var email = message.Email;

            // Store it into the database

            return Task.CompletedTask;
        }
    }
}
