namespace UserRegistration.Messages.Entities
{
    using System;

    // This class exists so that we don't have to duplicate (get/set) properties
    // over both RegisterNewUser and UserVerificationStarted. We now have one base-class
    // and if we happen to add 1 new property, both will have them.
    // If we create RegisterNewUser and have UserVerificationStarted inherit from it, then
    // when publishing that event, the saga would start looping as RegisterNewUser IS THAT SAME
    // event and NServiceBus polymorphism would execute both the saga and the handler, starting
    // an endless loop!!!
    public class UserEntity
    {
        public Guid UserId { get; set; }
        public string Name { get; set; }
        public string EmailAddress { get; set; }
    }
}