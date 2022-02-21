namespace Integration.Handlers
{
    using Messages.Commands;
    using NServiceBus;
    using NServiceBus.Logging;
    using System.Threading.Tasks;

    public class AddUserToSystemHandler : IHandleMessages<AddUserToSystem>
    {
        private static readonly ILog logger = LogManager.GetLogger<AddUserToSystemHandler>();

        public Task Handle(AddUserToSystem message, IMessageHandlerContext context)
        {
            logger.Info($"Received {message.EmailAddress}");
            // Do some integration with a CRM here...

            return Task.CompletedTask;
        }
    }
}