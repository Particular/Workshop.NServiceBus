using System;
using System.Threading.Tasks;

namespace UserRegistration.Sagas
{
    using AutoMapper;
    using Integration.Messages.Commands;
    using NServiceBus;
    using UserRegistration.Messages.Commands;
    using UserRegistration.Messages.Events;

    public class VerifyUserEmailProcess : Saga<VerifyUserEmailProcess.VerifyUserEmailData>,
        IAmStartedByMessages<RegisterNewUser>
    {
        public class VerifyUserEmailData : ContainSagaData
        {
            public Guid UserIdentifier { get; set; }
            public string VerificationCode { get; set; }
        }

        protected override void ConfigureHowToFindSaga(SagaPropertyMapper<VerifyUserEmailData> mapper)
        {
            mapper.ConfigureMapping<RegisterNewUser>(message => message.UserId).ToSaga(saga => saga.UserIdentifier);
        }

        public async Task Handle(RegisterNewUser message, IMessageHandlerContext context)
        {
            Mapper.Initialize(cfg => cfg.CreateMap<RegisterNewUser, UserVerificationStarted>());
            var @event = Mapper.Map<UserVerificationStarted>(message);
            await context.Publish(@event);

            Data.VerificationCode = Guid.NewGuid().ToString("n").Substring(0, 4);

            var command = new SendVerificationEmail(message.UserId, message.Name, message.Email, Data.VerificationCode);
            await context.Send(command);
        }
    }
}
