using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WhoNeedsHelp.server
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

        public LoginToken() { }

        public LoginToken(User user, Guid keyGuid)
        {
            User = user;
            Key = PasswordHash.CreateHash(keyGuid.ToString());
        }
    }
}
