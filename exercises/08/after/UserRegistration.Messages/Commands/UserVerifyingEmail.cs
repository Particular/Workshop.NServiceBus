using System;

namespace UserRegistration.Messages.Commands
{
    public class UserVerifyingEmail
    {
        public Guid UserId { get; }
        public string VerificationCode { get; }

        public UserVerifyingEmail(Guid userId, string verificationCode)
        {
            UserId = userId;
            VerificationCode = verificationCode;
        }
    }
}
