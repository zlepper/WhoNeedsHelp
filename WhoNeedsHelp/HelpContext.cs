using System;
using System.Data.Common;
using System.Data.Entity;
using System.Linq;
using MySql.Data.Entity;
using WhoNeedsHelp.server;
using MySql.Data;

namespace WhoNeedsHelp
{
    [DbConfigurationType(typeof(MySqlEFConfiguration))]
    public class HelpContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Connection> Connections { get; set; }
        public DbSet<Channel> Channels { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<LoginToken> LoginTokens { get; set; }
        //public DbSet<QuestionComment> QuestionComments { get; set; }

        public HelpContext() : base()
        {
            Database.SetInitializer(new MySqlInitializer());
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            //Configure domain classes using modelBuilder here

            // Configure the user model
            modelBuilder.Entity<User>().HasKey(u => u.Id);
            modelBuilder.Entity<User>().Property(u => u.Pw).IsOptional();
            // Map relation between "User.ChannelsIn" and "Channel.Users"
            modelBuilder.Entity<User>()
                .HasMany<Channel>(u => u.ChannelsIn)
                .WithMany(c => c.Users)
                .Map(cu =>
                {
                    cu.MapLeftKey("UserRefId");
                    cu.MapRightKey("ChannelRefId");
                    cu.ToTable("UsersInChannels");
                });
            // Map relation between "User.ChannelsRequestingHelpIn" and "Channel.UsersRequestingHelp"
            modelBuilder.Entity<User>()
                .HasMany<Channel>(u => u.ChannelsRequestingHelpIn)
                .WithMany(c => c.UsersRequestingHelp)
                .Map(cu =>
                {
                    cu.MapLeftKey("UserRefId");
                    cu.MapRightKey("ChannelRefId");
                    cu.ToTable("UsersRequestingHelpInChannels");
                });
            // Map relation between "User.AreAdministratorIn" and "Channel.Administrators"
            modelBuilder.Entity<User>()
                .HasMany<Channel>(u => u.AreAdministratorIn)
                .WithMany(c => c.Administrators)
                .Map(cu =>
                {
                    cu.MapLeftKey("UserRefId");
                    cu.MapRightKey("ChannelRefId");
                    cu.ToTable("AdministratorsInChannels");
                });
            // Map relation between "User.Questions" and "Question.User"
            modelBuilder.Entity<User>()
                .HasMany<Question>(u => u.Questions)
                .WithRequired(q => q.User)
                .HasForeignKey(q => q.UserId)
                .WillCascadeOnDelete(true);
            // Map relation between "User.ChatMessages" and "ChatMessage.User"
            modelBuilder.Entity<User>()
                .HasMany<ChatMessage>(u => u.ChatMessages)
                .WithRequired(cm => cm.User)
                .HasForeignKey(cm => cm.UserId)
                .WillCascadeOnDelete(true);
            // Map relation between "User.Connections" and "Connection.User"
            modelBuilder.Entity<User>()
                .HasMany<Connection>(u => u.Connections)
                .WithRequired(c => c.User)
                .HasForeignKey(c => c.UserId);
            modelBuilder.Entity<User>()
                .HasMany<LoginToken>(u => u.LoginTokens)
                .WithRequired(lk => lk.User)
                .HasForeignKey(lk => lk.UserId);


            // Configure the channel model
            // No need to configure relations with the "User" class. 
            // this has already been done above.
            modelBuilder.Entity<Channel>().HasKey(c => c.Id);
            // Map relation between "Channel.Questions" and "Question.Channel"
            modelBuilder.Entity<Channel>()
                .HasMany<Question>(c => c.Questions)
                .WithRequired(q => q.Channel)
                .HasForeignKey(q => q.ChannelId)
                .WillCascadeOnDelete(true);
            // Map relation between "Channel.ChatMessages" and "ChatMessage.Channel"
            modelBuilder.Entity<Channel>()
                .HasMany<ChatMessage>(c => c.ChatMessages)
                .WithRequired(cm => cm.Channel)
                .HasForeignKey(cm => cm.ChannelId)
                .WillCascadeOnDelete(true);

            // Configure the ChatMessage model
            modelBuilder.Entity<ChatMessage>().HasKey(cm => cm.Id);

            modelBuilder.Entity<Connection>().HasKey(c => c.ConnectionId);

            modelBuilder.Entity<LoginToken>().HasKey(lk => lk.Key);
            base.OnModelCreating(modelBuilder);
        }
    }
}