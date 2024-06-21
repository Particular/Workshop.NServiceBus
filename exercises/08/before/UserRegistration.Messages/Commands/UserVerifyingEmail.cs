namespace UserRegistration.Messages.Commands
{
    using System;

    public class UserVerifyingEmail
    {
        public UserVerifyingEmail(Guid userId, string verificationCode)
        {
            UserId = userId;
            VerificationCode = verificationCode;
        }

        public Guid UserId { get; }
        public string VerificationCode { get; }
    }
}