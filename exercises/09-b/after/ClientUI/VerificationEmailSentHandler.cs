using System;
using System.Threading.Tasks;

namespace ClientUI.Handlers
{
    using Integration.Messages.Events;
    using NServiceBus;
    using NServiceBus.Logging;
    using UserRegistration.Messages.Commands;

    public class VerificationEmailSentHandler : IHandleMessages<VerificationEmailSent>
    {
        private const int SecondsToCompletelyTimeoutSaga = 12;
        static ILog logger = LogManager.GetLogger<VerificationEmailSentHandler>();
        static Random rnd = new Random();

        /// <summary>
        /// This handler mimics the fact that a user actually clicked the email verification link! :-)
        /// </summary>
        public Task Handle(VerificationEmailSent message, IMessageHandlerContext context)
        {
            var secondsToWait = rnd.Next(SecondsToCompletelyTimeoutSaga);
            logger.Info($"Received [{message.VerificationCode}] - Waiting {secondsToWait} seconds to respond.");

            var sendOptions = new SendOptions();
            sendOptions.DelayDeliveryWith(TimeSpan.FromSeconds(secondsToWait));

            var command = new UserVerifyingEmail(message.UserId, message.VerificationCode);
            return context.Send(command, sendOptions);
        }
    }
}
