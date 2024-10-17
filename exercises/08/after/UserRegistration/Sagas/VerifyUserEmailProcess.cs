namespace UserRegistration.Sagas
{
    using AutoMapper;
    using Integration.Messages.Commands;
    using Messages.Commands;
    using Messages.Events;
    using NServiceBus;
    using NServiceBus.Logging;
    using System;
    using System.Threading.Tasks;
    using Timeouts;

    public class VerifyUserEmailProcess : Saga<VerifyUserEmailProcess.VerifyUserEmailData>,
        IAmStartedByMessages<RegisterNewUser>,
        IHandleMessages<UserVerifyingEmail>,
        IHandleTimeouts<EmailReminderTimeout>,
        IHandleTimeouts<UserEmailVerificationExpiredTimeout>
    {
        private static readonly ILog logger = LogManager.GetLogger<VerifyUserEmailProcess>();

        public async Task Handle(RegisterNewUser message, IMessageHandlerContext context)
        {
            logger.Info($"Received {message.UserId} - {message.EmailAddress}");

            Data.Name = message.Name;
            Data.EmailAddress = message.EmailAddress;
            Data.VerificationCode = Guid.NewGuid().ToString("n").Substring(0, 4);

            // Publish fact that we've started, so anyone interested can have the data available.
            var @event = Mapper.Map<UserVerificationStarted>(message);
            await context.Publish(@event);

            // Send command to send an email
            var command = new SendVerificationEmail(message.UserId, message.Name, message.EmailAddress,
                Data.VerificationCode);
            await context.Send(command);

            // Create timeout to make user aware to _really_ click that link.
            await RequestTimeout<EmailReminderTimeout>(context, TimeSpan.FromSeconds(5));
        }

        public async Task Handle(UserVerifyingEmail message, IMessageHandlerContext context)
        {
            logger.Info($"Received verification code {message.VerificationCode} for {Data.EmailAddress}");

            if (message.VerificationCode == Data.VerificationCode)
            {
                logger.Info($"[{message.VerificationCode}] is correct!");
                var cmd = new AddUserToSystem
                {
                    UserId = Data.UserIdentifier,
                    Name = Data.Name,
                    EmailAddress = Data.EmailAddress
                };
                await context.Send(cmd);
                MarkAsComplete();
            }
            // If for some reason a UserVerifyingEmail message came in, but with incorrect VerificationCode, it's just ignored.
        }

        public async Task Timeout(EmailReminderTimeout state, IMessageHandlerContext context)
        {
            var command = new SendVerificationReminderEmail(Data.UserIdentifier, Data.VerificationCode);
            await context.Send(command);

            await RequestTimeout<UserEmailVerificationExpiredTimeout>(context, TimeSpan.FromSeconds(5))
                ;
        }

        public Task Timeout(UserEmailVerificationExpiredTimeout state, IMessageHandlerContext context)
        {
            MarkAsComplete();

            return Task.CompletedTask;
        }

        protected override void ConfigureHowToFindSaga(SagaPropertyMapper<VerifyUserEmailData> mapper)
        {
            mapper.MapSaga(saga => saga.UserIdentifier)
                .ToMessage<RegisterNewUser>(message => message.UserId)
                .ToMessage<UserVerifyingEmail>(message => message.UserId);
        }

        public class VerifyUserEmailData : ContainSagaData
        {
            public Guid UserIdentifier { get; set; }
            public string VerificationCode { get; set; }
            public string Name { get; set; }
            public string EmailAddress { get; set; }
        }
    }
}