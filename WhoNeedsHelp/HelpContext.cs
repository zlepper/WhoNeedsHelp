﻿using System;
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
        //public DbSet<Connection> Connections { get; set; }
        public DbSet<Channel> Channels { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }
        public DbSet<Question> Questions { get; set; }
        //public DbSet<QuestionComment> QuestionComments { get; set; }

        public HelpContext() : base()
        {
            Database.SetInitializer(new MySqlInitializer());
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            //Configure domain classes using modelBuilder here
            // Configure default schema
            //modelBuilder.HasDefaultSchema("whoneedshelp");

            // Configure the user model
            modelBuilder.Entity<User>().HasKey(u => u.Id);
            //modelBuilder.Entity<User>().Property(u => u.ChannelId).IsOptional();
            modelBuilder.Entity<User>().HasOptional<Channel>(u => u.Channel).WithMany(c => c.ActiveUsers).HasForeignKey(u => u.ChannelId).WillCascadeOnDelete(false);
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
            modelBuilder.Entity<User>()
                .HasMany<Question>(u => u.Questions)
                .WithRequired(q => q.User)
                .HasForeignKey(q => q.UserId)
                .WillCascadeOnDelete(false);
            //modelBuilder.Entity<User>().HasOptional()


            // Configure the channel model
            // No need to configure relations with the "User" class. 
            // this has already been done in the lines above.
            modelBuilder.Entity<Channel>().HasKey(c => c.Id);
            // Map relation between "Channel.Questions" and "Question.Channel"
            modelBuilder.Entity<Channel>()
                .HasMany<Question>(c => c.Questions)
                .WithRequired(q => q.Channel)
                .HasForeignKey(q => q.ChannelId)
                .WillCascadeOnDelete(false);

            // Configure the ChatMessage model
            modelBuilder.Entity<ChatMessage>().HasKey(cm => cm.Id);

            // Configure the Question model
            
            base.OnModelCreating(modelBuilder);
        }
    }
}