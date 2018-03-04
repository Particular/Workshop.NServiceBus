using System;

namespace Integration.Messages.Commands
{
    public class SendVerificationReminderEmail
    {
        public Guid UserId { get; }
        public string VerificationCode { get; }

        public SendVerificationReminderEmail(Guid UserId, string VerificationCode)
        {
            this.UserId = UserId;
            this.VerificationCode = VerificationCode;
        }
    }
}
