using System;

namespace Integration.Messages.Commands
{
    public class SendVerificationEmail
    {
        public Guid UserId { get; }
        public string Name { get; }
        public string Email { get; }
        public string VerificationCode { get; }

        public SendVerificationEmail(Guid UserId, string Name, string Email, string VerificationCode)
        {
            this.UserId = UserId;
            this.Name = Name;
            this.Email = Email;
            this.VerificationCode = VerificationCode;
        }
    }
}
