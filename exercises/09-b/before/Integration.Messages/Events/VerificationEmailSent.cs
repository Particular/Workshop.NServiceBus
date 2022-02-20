using System;

namespace Integration.Messages.Events
{
    public class VerificationEmailSent
    {
        public Guid UserId { get; set; }
        public string VerificationCode { get; set; }
    }
}
