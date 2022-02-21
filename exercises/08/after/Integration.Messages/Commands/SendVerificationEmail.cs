namespace Integration.Messages.Commands
{
    using System;

    public class SendVerificationEmail
    {
        public SendVerificationEmail(Guid UserId, string Name, string Email, string VerificationCode)
        {
            this.UserId = UserId;
            this.Name = Name;
            this.Email = Email;
            this.VerificationCode = VerificationCode;
        }

        public Guid UserId { get; }
        public string Name { get; }
        public string Email { get; }
        public string VerificationCode { get; }
    }
}