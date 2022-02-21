namespace Integration.Messages.Events
{
    using System;

    public class VerificationEmailSent
    {
        public Guid UserId { get; set; }
        public string VerificationCode { get; set; }
    }
}