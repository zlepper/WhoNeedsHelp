using System;
using WhoNeedsHelp.Server.Chat;

namespace WhoNeedsHelp.Models
{
    public class LoginToken
    {
        /// <summary>
        /// The key the user should have stored in their cookies
        /// Should be hashed
        /// </summary>
        public string Key { get; set; }
        public int UserId { get; set; }
        public virtual User User { get; set; }
        public DateTime CreatedAt { get; set; }

        public LoginToken() { }

        public LoginToken(User user, Guid keyGuid)
        {
            CreatedAt = DateTime.Now;
            User = user;
            Key = PasswordHash.CreateHash(keyGuid.ToString());
        }
    }
}
