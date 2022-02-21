namespace Integration.Messages.Commands
{
    using System;

    public class AddUserToSystem
    {
        public Guid UserId { get; set; }
        public string Name { get; set; }
        public string EmailAddress { get; set; }
    }
}