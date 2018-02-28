using System;

namespace Integration.Messages.Commands
{
    public class VerifyEmailCode
    {
        public Guid UserId { get; set; }
        public string VerificationCode { get; set; }
    }
}
