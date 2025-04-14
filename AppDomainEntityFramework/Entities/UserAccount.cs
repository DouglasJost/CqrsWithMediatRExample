using System;

namespace AppDomainEntityFramework.Entities
{
    public class UserAccount
    {
        public Guid UserAccountId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Login {  get; set; } = string.Empty;
        public string Password {  get; set; } = string.Empty;

        public string? RefreshToken { get; set; }
    }
}
