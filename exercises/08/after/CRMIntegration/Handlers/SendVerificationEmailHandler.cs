using System.Threading.Tasks;

namespace Integration.Handlers
{
    using System;
    using Integration.Messages.Commands;
    using NServiceBus;
    public class SendVerificationEmailHandler : IHandleMessages<SendVerificationEmail>
    {
        public Task Handle(SendVerificationEmail message, IMessageHandlerContext context)
        {
            // Integrate with SMTP server
            // Instead we'll fake delivery of the command from the website
            var options = new SendOptions();
            options.DelayDeliveryWith(TimeSpan.FromSeconds(10));

            var command = new VerifyEmailCode
            {
                UserId = message.UserId,
                VerificationCode = message.VerificationCode
            };

            return context.Send(message, options);
        }
    }
}
