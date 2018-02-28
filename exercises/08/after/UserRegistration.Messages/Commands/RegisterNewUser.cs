namespace UserRegistration.Messages.Commands
{
    using System;

    public class RegisterNewUser
    {
        public Guid UserId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
    }
}
