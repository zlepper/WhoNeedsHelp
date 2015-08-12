﻿// ReSharper disable DoNotCallOverridableMethodsInConstructor
namespace WhoNeedsHelp.Server.Chat
{
    public class Connection
    {
        public string ConnectionId { get; set; }
        public int UserId { get; set; }
        public virtual User User { get; set; }

        public Connection()
        {
            
        }

        public Connection(User user)
        {
            User = user;
        }
    }
}