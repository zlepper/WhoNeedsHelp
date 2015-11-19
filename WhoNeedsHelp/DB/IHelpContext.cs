using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WhoNeedsHelp.Models;

namespace WhoNeedsHelp.DB
{
    public interface IHelpContext
    {
        DbSet<User> Users { get; set; }
        DbSet<Connection> Connections { get; set; }
        DbSet<Channel> Channels { get; set; }
        DbSet<ChatMessage> ChatMessages { get; set; }
        DbSet<Question> Questions { get; set; }
        DbSet<LoginToken> LoginTokens { get; set; }
        DbSet<Locale> Locales { get; set; }
        DbSet<Locale.Translation> Translations { get; set; }
        DbSet<CleanupAlarm> CleanupAlarms { get; set; }

        /// <summary>
        /// Gets a user by his/her id in the database
        /// </summary>
        /// <param name="id">The id of the user</param>
        /// <returns>A user with the specified id, or null</returns>
        User GetUserById(int id);

        /// <summary>
        /// Gets a user by their connection context
        /// </summary>
        /// <param name="connection">Their connection string from signalR</param>
        /// <returns>The user with the specified connection</returns>
        User GetUserByConnection(string connection);

        /// <summary>
        /// Get a channel with the specified id
        /// </summary>
        /// <param name="id">The id of the channel</param>
        /// <returns></returns>
        Channel GetChannelById(int id);


        int SaveChanges();
        void Dispose();
    }
}
