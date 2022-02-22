using System.Threading.Tasks;

namespace Integration.Handlers
{
    using Integration.Messages.Commands;
    using NServiceBus;
    using NServiceBus.Logging;

    public class AddUserToSystemHandler : IHandleMessages<AddUserToSystem>
    {
        static ILog logger = LogManager.GetLogger<AddUserToSystemHandler>();

        public Task Handle(AddUserToSystem message, IMessageHandlerContext context)
        {
            logger.Info($"Received {message.EmailAddress}");
            // Do some integration with a CRM here...

            return Task.CompletedTask;
        }
    }
}
