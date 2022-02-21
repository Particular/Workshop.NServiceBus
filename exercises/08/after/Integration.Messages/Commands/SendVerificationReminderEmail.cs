namespace Integration.Messages.Commands
{
    using System;

    public class SendVerificationReminderEmail
    {
        public SendVerificationReminderEmail(Guid UserId, string VerificationCode)
        {
            this.UserId = UserId;
            this.VerificationCode = VerificationCode;
        }

        public Guid UserId { get; }
        public string VerificationCode { get; }
    }
}